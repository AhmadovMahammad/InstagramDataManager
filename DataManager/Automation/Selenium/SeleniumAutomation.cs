using DataManager.Constants;
using DataManager.Constants.Enums;
using DataManager.Exceptions;
using DataManager.Extensions;
using DataManager.Factories;
using DataManager.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataManager.Automation.Selenium;
public class SeleniumAutomation : LoginAutomation
{
    public delegate LoginOutcome ConditionDelegate(IWebDriver driver);
    protected override IWebDriver InitializeDriver() => FirefoxDriverFactory.CreateDriver(_validationChain);

    // overriden methods
    protected override void NavigateToLoginPage()
    {
        Driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
        Console.WriteLine("Instagram login page opened successfully!");
    }

    protected override void PerformLoginSteps()
    {
        try
        {
            Console.Write("Enter username: ");
            SendKeys(LoginPageConstants.UsernameField.Invoke(Driver), Console.ReadLine() ?? string.Empty);

            Console.Write("Enter password: ");
            SendKeys(LoginPageConstants.PasswordField.Invoke(Driver), PasswordExtension.ReadPassword() ?? string.Empty);

            LoginPageConstants.SubmitButton.Invoke(Driver).Submit();
        }
        catch (Exception ex)
        {
            throw new LoginException($"Error during login steps: {ex.Message}");
        }
    }

    protected override void HandleLoginOutcome()
    {
        WebDriverWait wait = new WebDriverWait(Driver, AppTimeoutConstants.ExplicitWait);

        try
        {
            // Wait until any of the conditions are met: error banner, 2FA prompt, or 'onetap' URL
            wait.Until(IsLoginSuccessfulOrError);

            // Handle error banner if present
            if (ErrorMessageConstants.ErrorBanner(Driver) is LoginOutcome errorOutcome &&
                errorOutcome != LoginOutcome.Empty)
            {
                throw new LoginException(ErrorMessageConstants.IncorrectPasswordMessage);
            }

            // Handle 2FA if present
            if (TwoFactorAuthConstants.VerificationCodeField(Driver) is LoginOutcome twoFactorOutcome &&
                twoFactorOutcome != LoginOutcome.Empty)
            {
                if (twoFactorOutcome.Data is IWebElement webElement)
                {
                    HandleTwoFactorAuthentication(webElement);
                }
            }

            "You successfully logged in.".WriteMessage(MessageType.Success);
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
            var saveInfo = Driver.FindElement(By.XPath("//button[text()='Save info']"));
            saveInfo.Click();
        }
        catch (Exception)
        {
            throw new LoginException("An error occurred while saving information");
        }
    }

    // helpers
    private bool IsLoginSuccessfulOrError(IWebDriver driver)
    {
        return
            driver.Url.Contains("accounts/onetap") ||
            driver.Url.Contains("accounts/login/two_factor?next=%2F") ||
            driver.FindElements(By.XPath("//div[contains(text(),'your password was incorrect.')]")).Count > 0;
    }

    private void HandleTwoFactorAuthentication(IWebElement webElement)
    {
        Console.Write(TwoFactorAuthConstants.TwoFactorPromptMessage);
        string code = Console.ReadLine() ?? string.Empty;

        webElement.SendKeys(code);
        webElement.Submit();
    }
}