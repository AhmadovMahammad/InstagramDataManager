using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.Exceptions;
using DataManager.Factory;
using DataManager.Helper.Extension;
using DataManager.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DataManager.Automation;
public class SeleniumAutomation : LoginAutomation
{
    public delegate LoginOutcome ConditionDelegate(IWebDriver webDriver);

    public SeleniumAutomation() : base() { }

    // Overridden methods
    protected override IWebDriver InitializeDriver() => FirefoxDriverFactory.CreateDriver(_validationChain);

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

    protected override void HandleLoginOutcome()
    {
        WebDriverWait wait = new WebDriverWait(Driver, AppConstant.ExplicitWait);

        try
        {
            // Wait until any of the conditions are met: error banner, 2FA prompt, or 'onetap' URL
            wait.Until(webDriver => IsLoginSuccessfulOrError(webDriver));

            // Check for errors or handle 2FA if needed
            HandleLoginError();
            HandleTwoFactorAuthenticationIfNeeded();

            "You successfully logged in.\n".WriteMessage(MessageType.Success);
        }
        catch (WebDriverTimeoutException)
        {
            throw new LoginException(ErrorMessageConstants.LoginTimeoutMessage);
        }
        catch (Exception ex)
        {
            throw new LoginException(ex.Message);
        }
    }

    protected override void SaveInfo()
    {
        try
        {
            WebDriverWait wait = new WebDriverWait(Driver, AppConstant.ExplicitWait);
            var saveInfoButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[text()='Save info']")));

            saveInfoButton.JavaScriptClick();
        }
        catch (Exception ex)
        {
            throw new LoginException($"An error occurred while saving information: {ex.Message}");
        }
    }

    // Helper Methods
    private bool IsLoginSuccessfulOrError(IWebDriver webDriver)
    {
        return webDriver.Url.Contains("accounts/onetap") ||
               webDriver.Url.Contains("accounts/login/two_factor?next=%2F") ||
               webDriver.FindElements(By.XPath("//div[contains(text(),'your password was incorrect.')]")).Count > 0;
    }

    private void HandleLoginError()
    {
        if (ErrorMessageConstants.ErrorBanner(Driver) is LoginOutcome errorOutcome && errorOutcome != LoginOutcome.Empty)
        {
            throw new LoginException(ErrorMessageConstants.IncorrectPasswordMessage);
        }
    }

    private void HandleTwoFactorAuthenticationIfNeeded()
    {
        if (TwoFactorAuthConstants.VerificationCodeField(Driver) is LoginOutcome twoFactorOutcome && twoFactorOutcome != LoginOutcome.Empty)
        {
            if (twoFactorOutcome.Data is IWebElement webElement)
            {
                HandleTwoFactorAuthentication(webElement);
            }
        }
    }

    private void HandleTwoFactorAuthentication(IWebElement webElement)
    {
        Console.Write(TwoFactorAuthConstants.TwoFactorPromptMessage);
        string code = Console.ReadLine() ?? string.Empty;

        webElement.SendKeys(code);
        webElement.Submit();
    }
}