using DataManager.Constant.Enums;
using DataManager.Constant;
using DataManager.Core.Services.Contracts;
using DataManager.DesignPattern.ChainOfResponsibility;
using DataManager.Exceptions;
using DataManager.Factory;
using DataManager.Helper.Extension;
using DataManager.Model;
using OpenQA.Selenium;
using System.Text.Json;
using DataManager.Helper.Utility;
using System.Reflection.PortableExecutable;

namespace DataManager.Core.Services.Implementations;
public class LoginService : ILoginService
{
    private readonly IChainHandler _validationChain;
    private readonly string _credentialsPath = Path.Combine(AppConstant.ApplicationDataFolderPath, "Credentials");

    public LoginService()
    {
        _validationChain = new ArgumentNotEmptyHandler()
                .SetNext(new FileExistsHandler())
                .SetNext(new FileExtensionHandler([".exe"]));

        WebDriver = FirefoxDriverFactory.CreateDriver(_validationChain);

        if (!Directory.Exists(_credentialsPath))
        {
            Directory.CreateDirectory(_credentialsPath);
        }
    }

    public IWebDriver WebDriver { get; }

    public void ExecuteLogin()
    {
        try
        {
            NavigateToLoginPage();
            PerformLoginSteps(); // get credentials from file if they exists, otherwise ask from user.
            HandleLoginOutcome(); // handle [ Success, 2FA, IncorrectPassword ] outcomes
            SaveInfo();
        }
        catch (Exception)
        {
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
        catch (Exception ex)
        {
            throw new LoginException($"Error navigating to login page: {ex.Message}");
        }
    }

    private void PerformLoginSteps()
    {
        try
        {
            var credentials = GetUserCredentials(out bool hasNewCredentials);
            FillLoginForm(credentials);
            WebDriver.FindElement(By.XPath(XPathConstants.SubmitButton)).Submit();

            if (hasNewCredentials)
            {
                SaveCredentials(credentials);
            }
        }
        catch (Exception ex)
        {
            throw new LoginException($"Error during login steps: {ex.Message}");
        }
    }

    private (string username, string password) GetUserCredentials(out bool hasNewCredentials)
    {
        hasNewCredentials = false;
        var (username, password) = RetrieveCredentials();

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            Console.WriteLine("Previous credentials found. Press Enter to use them, or type new credentials to overwrite.");

            username = $"Current username: {username}. Enter new username (or press Enter to keep): ".GetInput(username);
            password = "Enter password (or press Enter to keep previous): ".GetPasswordInput(password);
        }
        else
        {
            hasNewCredentials = true;

            username = "Enter username > ".GetInput();
            password = "Enter password > ".GetPasswordInput();
        }

        return (username, password);
    }

    private (string username, string password) RetrieveCredentials()
    {
        string selectedFile = GetSpecificCredentialFile(Directory.GetFiles(_credentialsPath, "backup_*.json"));
        if (string.IsNullOrEmpty(selectedFile))
        {
            return (string.Empty, string.Empty);
        }

        try
        {
            using FileStream fileStream = new FileStream(selectedFile, FileMode.Open, FileAccess.Read);
            Credential? credential = JsonSerializer.Deserialize<Credential>(fileStream);

            if (credential != null)
            {
                string decryptedPassword = DataProtectionHelper.Decrypt(credential.EncryptedPassword, credential.Salt);
                return (credential.Username, decryptedPassword);
            }
        }
        catch
        {
            // todo: handle logs
        }

        return (string.Empty, string.Empty);
    }

    private string GetSpecificCredentialFile(string[] fileNames)
    {
        if (fileNames.Length == 0)
        {
            "No saved credentials found.".WriteMessage(MessageType.Info);
            return string.Empty;
        }

        string selectedFile = string.Empty;
        if (fileNames.Length == 1)
        {
            selectedFile = fileNames[0];
        }
        else
        {
            Console.WriteLine("Multiple credentials found. Please select one:");
            for (int i = 0; i < fileNames.Length; i++)
            {
                string username = Path.GetFileNameWithoutExtension(fileNames[i]).Replace("backup_", "");
                Console.WriteLine($"{i + 1}: {username}");
            }

            while (true)
            {
                Console.Write("Enter the number of the credential to use or type 'exit' to quit");
                string? input = Console.ReadLine();

                if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (int.TryParse(input, out int choice) && choice > 0 && choice <= fileNames.Length)
                {
                    selectedFile = fileNames[choice - 1];
                    break;
                }

                Console.WriteLine("Invalid selection. Please try again.");
            }
        }

        return selectedFile;
    }

    private void FillLoginForm((string username, string password) credentials)
    {
        IWebElement usernameField = WebDriver.FindElement(By.XPath(XPathConstants.UsernameField));
        SendKeys(usernameField, credentials.username);

        IWebElement passwordField = WebDriver.FindElement(By.XPath(XPathConstants.PasswordField));
        SendKeys(passwordField, credentials.password);
    }

    private void SaveCredentials((string username, string password) credentials)
    {
        string fileName = Path.Combine(_credentialsPath, $"backup_{credentials.username}.json");

        try
        {
            string encryptedPassword = DataProtectionHelper.Encrypt(credentials.password, out string salt);

            var credential = new Credential
            {
                Username = credentials.username,
                Salt = salt,
                EncryptedPassword = encryptedPassword
            };

            string json = JsonSerializer.Serialize(credential);
            File.WriteAllText(fileName, json);
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
