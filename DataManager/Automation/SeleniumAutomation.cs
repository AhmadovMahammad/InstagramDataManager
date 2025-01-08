using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.Exceptions;
using DataManager.Factory;
using DataManager.Helper.Extension;
using DataManager.Model;
using OpenQA.Selenium;

namespace DataManager.Automation;
public class SeleniumAutomation : LoginAutomation
{
    public delegate LoginOutcome ConditionDelegate(IWebDriver webDriver);

    public SeleniumAutomation() : base() { }

    // Overridden methods
    protected override IWebDriver InitializeDriver() => FirefoxDriverFactory.CreateDriver(_validationChain);

    // STEP #1
    protected override void NavigateToLoginPage()
    {
        try
        {
            Driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
            "Instagram login page opened successfully!\n".WriteMessage(MessageType.Success);
        }
        catch (Exception ex)
        {
            throw new LoginException($"Error navigating to login page: {ex.Message}");
        }
    }

    // STEP #2
    protected override void PerformLoginSteps()
    {
        try
        {
            Console.Write("Enter username > ");
            IWebElement usernameField = Driver.FindElement(By.XPath("//input[@name='username']"));
            SendKeys(usernameField, Console.ReadLine() ?? string.Empty);

            Console.Write("Enter password > ");
            IWebElement passwordField = Driver.FindElement(By.XPath("//input[@name='password']"));
            SendKeys(passwordField, PasswordExtension.ReadPassword() ?? string.Empty);

            Driver.FindElement(By.XPath("//button[@type='submit']")).Submit();
        }
        catch (Exception ex)
        {
            throw new LoginException($"Error during login steps: {ex.Message}");
        }
    }

    // STEP #3
    protected override void HandleLoginOutcome()
    {
        try
        {
            // Wait until any of the conditions are met: error banner, 2FA prompt, or 'onetap' URL
            Task waitForConditionTask = WaitForAnyConditionAsync(Driver);
            waitForConditionTask.Wait();

            var postLoginTasks = new[]
            {
                Task.Run(HandleLoginError),                // Check for login errors
                Task.Run(HandleTwoFactorAuthentication)    // Handle two-factor authentication
            };

            Task.WhenAll(postLoginTasks).Wait();
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

    // STEP #3 - sub method
    private Task WaitForAnyConditionAsync(IWebDriver webDriver)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        Task onetapTask = Task.Run(() => WaitForCondition(() => webDriver.Url.Contains("accounts/onetap/?next=%2F"), token), token);
        Task twoFactorTask = Task.Run(() => WaitForCondition(() => webDriver.Url.Contains("accounts/login/two_factor?next=%2F"), token), token);
        Task incorrectPasswordTask = Task.Run(() =>
        {
            return WaitForCondition(() =>
            {
                var webElement = webDriver.FindWebElement(
                    By.XPath("//div[normalize-space(text())='Sorry, your password was incorrect. Please double-check your password.']"),
                    WebElementPriorityType.Medium
                    );

                return webElement != null;
            }, token);
        }, token);

        Task completedTask = Task.WhenAny(onetapTask, twoFactorTask, incorrectPasswordTask);
        cts.Cancel();

        return Task.CompletedTask;
    }

    // STEP #3 - sub method
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

            bool result = condition.Invoke();
            if (result) return Task.CompletedTask;

            Thread.Sleep(1000);
        }

        return Task.FromCanceled(cancellationToken);
    }

    // STEP #3 - sub method
    private Task HandleLoginError()
    {
        return Task.Run(() =>
        {
            if (ErrorMessageConstants.ErrorBanner(Driver) is LoginOutcome errorOutcome && errorOutcome != LoginOutcome.Empty)
            {
                throw new LoginException(ErrorMessageConstants.IncorrectPasswordMessage);
            }
        });
    }

    // STEP #3 - sub method
    private Task HandleTwoFactorAuthentication()
    {
        return Task.Run(() =>
        {
            if (TwoFactorAuthConstants.VerificationCodeField(Driver) is LoginOutcome twoFactorOutcome && twoFactorOutcome != LoginOutcome.Empty)
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

    // STEP #4
    protected override void SaveInfo()
    {
        IWebElement? webElement = Driver.FindWebElement(
               By.XPath("//button[text()='Save info' and contains(@class,'_acan _acap _acas _aj1- _ap30')]"),
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
}