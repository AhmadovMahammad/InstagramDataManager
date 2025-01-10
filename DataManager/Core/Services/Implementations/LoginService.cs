using DataManager.Constant.Enums;
using DataManager.Constant;
using DataManager.Core.Services.Contracts;
using DataManager.DesignPattern.ChainOfResponsibility;
using DataManager.Exceptions;
using DataManager.Factory;
using DataManager.Helper.Extension;
using DataManager.Model;
using OpenQA.Selenium;

namespace DataManager.Core.Services.Implementations;
public class LoginService : ILoginService
{
    private readonly IChainHandler _validationChain;

    public LoginService()
    {
        _validationChain = new ArgumentNotEmptyHandler()
                .SetNext(new FileExistsHandler())
                .SetNext(new FileExtensionHandler([".exe"]));

        WebDriver = FirefoxDriverFactory.CreateDriver(_validationChain);
    }
    
    public IWebDriver WebDriver { get; }

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
            throw;
        }
    }

    protected virtual void NavigateToLoginPage()
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

    protected virtual void PerformLoginSteps()
    {
        try
        {
            Console.Write("Enter username > ");
            IWebElement usernameField = WebDriver.FindElement(By.XPath(XPathConstants.UsernameField));
            SendKeys(usernameField, Console.ReadLine() ?? string.Empty);

            Console.Write("Enter password > ");
            IWebElement passwordField = WebDriver.FindElement(By.XPath(XPathConstants.PasswordField));
            SendKeys(passwordField, PasswordExtension.ReadPassword() ?? string.Empty);

            WebDriver.FindElement(By.XPath(XPathConstants.SubmitButton)).Submit();
        }
        catch (Exception ex)
        {
            throw new LoginException($"Error during login steps: {ex.Message}");
        }
    }

    protected virtual void HandleLoginOutcome()
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

    protected virtual void SaveInfo()
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
