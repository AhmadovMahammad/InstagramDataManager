using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.Core.Services.Contracts;
using DataManager.DesignPattern.ChainOfResponsibility;
using DataManager.Exceptions;
using DataManager.Factory;
using DataManager.Helper.Extension;
using DataManager.Helper.Utility;
using DataManager.Model;
using DataManager.Model.JsonModel;
using OpenQA.Selenium;
using System.Text.Json;

namespace DataManager.Core.Services.Implementations;
public class LoginService : ILoginService
{
    private readonly IChainHandler _validationChain;
    private readonly string _credentialsPath;
    private string _savedFilepath = string.Empty;
    public IWebDriver WebDriver { get; }

    public LoginService()
    {
        _validationChain = new ArgumentNotEmptyHandler()
                .SetNext(new FileExistsHandler())
                .SetNext(new FileExtensionHandler([".exe"]));
        WebDriver = FirefoxDriverFactory.CreateDriver(_validationChain);

        _credentialsPath = Path.Combine(AppConstant.ApplicationDataFolderPath, "Credentials");
        if (!Directory.Exists(_credentialsPath))
        {
            Directory.CreateDirectory(_credentialsPath);
        }
    }


    public void ExecuteLogin()
    {
        try
        {
            NavigateToLoginPage();
            PerformLoginSteps();
            HandleLoginOutcome();
            SaveInfo();
        }
        catch (Exception)
        {
            if (File.Exists(_savedFilepath))
            {
                "Your credentials have been removed from an existing file; please register as a new user in our program the next time."
                    .WriteMessage(MessageType.Info);

                File.Delete(_savedFilepath);
            }
            throw;
        }
    }

    private void NavigateToLoginPage()
    {
        try
        {
            WebDriver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
            "Instagram login page opened successfully!\n".WriteMessage(MessageType.Success);
        }
        catch
        {
            throw new Exception($"An error occurred while navigating to Login page.");
        }
    }

    private void PerformLoginSteps()
    {
        bool hasNewCredentials = false;
        LoginDetail loginDetail = null!;

        try
        {
            loginDetail = GetUserCredentials(out hasNewCredentials);
            FillLoginForm(loginDetail);
            WebDriver.FindElement(By.XPath(XPathConstants.SubmitButton)).Submit();
        }
        catch
        {
            throw new Exception($"An error occurred while filling User credentials");
        }
        finally
        {
            if (hasNewCredentials)
            {
                SaveCredentials(loginDetail);
            }
        }
    }

    private LoginDetail GetUserCredentials(out bool hasNewCredentials)
    {
        hasNewCredentials = false;
        LoginDetail loginDetail = RetrieveCredentials();

        if (loginDetail != LoginDetail.Empty)
        {
            "Previous credentials found. Press Enter to use them, or enter new credentials to overwrite.".WriteMessage(MessageType.Info);
            string password = "Password".GetPasswordInput(loginDetail.Password);

            if (password != loginDetail.Password)
            {
                hasNewCredentials = true;
                loginDetail.Password = password;
            }
        }
        else
        {
            hasNewCredentials = true;
            loginDetail.UserName = "Enter username".GetInput();
            loginDetail.Password = "Enter password".GetPasswordInput();
        }

        return loginDetail;
    }

    private LoginDetail RetrieveCredentials()
    {
        string selectedFile = GetSpecificFile(Directory.GetFiles(_credentialsPath, "backup_*.json"));
        if (string.IsNullOrEmpty(selectedFile))
        {
            return LoginDetail.Empty;
        }

        try
        {
            using FileStream fileStream = new FileStream(selectedFile, FileMode.Open, FileAccess.Read);
            Credential? credential = JsonSerializer.Deserialize<Credential>(fileStream);

            if (credential != null)
            {
                string decryptedPassword = DataProtectionHelper.Decrypt(credential.EncryptedPassword, credential.Salt);
                return new LoginDetail() { UserName = credential.Username, Password = decryptedPassword };
            }
        }
        catch
        {
            // todo: handle logs
        }

        return LoginDetail.Empty;
    }

    private string GetSpecificFile(string[] fileNames)
    {
        if (fileNames.Length == 0) return string.Empty;
        var fileOptions = fileNames
            .Select((file, index) =>
            {
                return new
                {
                    Index = index + 1,
                    Username = Path.GetFileNameWithoutExtension(file).Replace("backup_", ""),
                    FileName = Path.GetFileName(file),
                };
            });

        string selectedFile = string.Empty;

        int count = fileOptions.Count();
        string message = fileOptions.Count() == 1
            ? "One credential found."
            : $"Multiple credentials found.";

        message.WriteMessage(MessageType.Info);
        foreach (var item in fileOptions)
        {
            Console.WriteLine($"#{item.Index} {item.Username} ({item.FileName})");
        }

        while (true)
        {
            string input = "Enter the number of the credential you want to use, or type 'exit' to enter new credentials.".GetInput();
            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (int.TryParse(input, out int choice) && choice > 0 && choice <= fileNames.Length)
            {
                selectedFile = fileNames[choice - 1];
                break;
            }

            "Invalid selection. Please try again.".WriteMessage(MessageType.Warning);
        }

        Console.WriteLine();
        return selectedFile;
    }

    private void FillLoginForm(LoginDetail loginDetail)
    {
        IWebElement usernameField = WebDriver.FindElement(By.XPath(XPathConstants.UsernameField));
        SendKeys(usernameField, loginDetail.UserName);

        IWebElement passwordField = WebDriver.FindElement(By.XPath(XPathConstants.PasswordField));
        SendKeys(passwordField, loginDetail.Password);
    }

    private void SaveCredentials(LoginDetail loginDetail)
    {
        _savedFilepath = Path.Combine(_credentialsPath, $"backup_{loginDetail.UserName}.json");
        if (File.Exists(_savedFilepath))
        {
            File.Delete(_savedFilepath);
        }

        try
        {
            string encryptedPassword = DataProtectionHelper.Encrypt(loginDetail.Password, out string salt);

            var credential = new Credential
            {
                Username = loginDetail.UserName,
                Salt = salt,
                EncryptedPassword = encryptedPassword
            };

            string json = JsonSerializer.Serialize(credential);
            File.WriteAllText(_savedFilepath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving credentials: {ex.Message}");
        }
    }

    private void HandleLoginOutcome()
    {
        try
        {
            Task waitForConditionTask = WaitForAnyConditionAsync(WebDriver);
            waitForConditionTask.Wait();

            Task.WhenAll(
                Task.Run(HandleLoginError),
                Task.Run(HandleTwoFactorAuthentication)
            ).Wait();

            "You successfully logged in.\n".WriteMessage(MessageType.Success);
        }
        catch (TimeoutException ex)
        {
            throw new LoginException($"Timeout during login process: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new LoginException(ex.Message);
        }
    }

    private Task WaitForAnyConditionAsync(IWebDriver webWebDriver)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        Task onetapTask = Task.Run(() => WaitForCondition(() => webWebDriver.Url.Contains("accounts/onetap/?next=%2F"), token), token);
        Task twoFactorTask = Task.Run(() => WaitForCondition(() => webWebDriver.Url.Contains("accounts/login/two_factor?next=%2F"), token), token);
        Task incorrectPasswordTask = Task.Run(() =>
        {
            return WaitForCondition(() =>
            {
                var webElement = webWebDriver.FindWebElement(
                    By.XPath(XPathConstants.PasswordIncorrectLabel),
                    WebElementPriorityType.Medium
                );
                return webElement != null;
            }, token);
        }, token);

        Task completedTask = Task.WhenAny(onetapTask, twoFactorTask, incorrectPasswordTask);
        cts.Cancel();

        return Task.CompletedTask;
    }

    private Task WaitForCondition(Func<bool> condition, CancellationToken cancellationToken)
    {
        if (condition == null)
        {
            throw new ArgumentNullException(nameof(condition), "Condition cannot be null");
        }

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (condition.Invoke())
            {
                return Task.CompletedTask;
            }

            Thread.Sleep(1000);
        }

        return Task.FromCanceled(cancellationToken);
    }

    private Task HandleLoginError()
    {
        return Task.Run(() =>
        {
            if (ErrorMessageConstants.ErrorBanner(WebDriver) is LoginOutcome errorOutcome && errorOutcome != LoginOutcome.Empty)
            {
                throw new LoginException(ErrorMessageConstants.IncorrectPasswordMessage);
            }
        });
    }

    private Task HandleTwoFactorAuthentication()
    {
        return Task.Run(() =>
        {
            if (TwoFactorAuthConstants.VerificationCodeField(WebDriver) is LoginOutcome twoFactorOutcome && twoFactorOutcome != LoginOutcome.Empty)
            {
                if (twoFactorOutcome.Data is IWebElement webElement)
                {
                    Console.Write(TwoFactorAuthConstants.TwoFactorPromptMessage);
                    string code = Console.ReadLine() ?? string.Empty;

                    webElement.SendKeys(code);
                }
            }
        });
    }

    private void SaveInfo()
    {
        IWebElement? webElement = WebDriver.FindWebElement(
               By.XPath(XPathConstants.SaveInfoButton),
               WebElementPriorityType.Medium
               );

        if (webElement != null)
        {
            webElement.Click();
        }
        else
        {
            throw new LoginException($"An error occurred while saving information.");
        }
    }

    protected void SendKeys(IWebElement webElement, string data)
    {
        if (string.IsNullOrEmpty(webElement.Text) && webElement.Enabled)
        {
            webElement.Clear();
        }

        webElement.SendKeys(data);
    }
}
