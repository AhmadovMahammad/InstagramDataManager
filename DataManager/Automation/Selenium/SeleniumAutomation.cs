using DataManager.Constants;
using DataManager.Constants.Enums;
using DataManager.Exceptions;
using DataManager.Extensions;
using DataManager.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace DataManager.Automation.Selenium;
public class SeleniumAutomation : LoginAutomation
{
    public IWebDriver Driver => _driver;
    public delegate LoginOutcome ConditionDelegate(IWebDriver driver);

    protected override IWebDriver InitializeDriver()
    {
        string firefoxPath = GetFirefoxExecutablePath();

        FirefoxOptions options = new() { BinaryLocation = firefoxPath };
        //options.AddArguments("--headless");

        _driver = new FirefoxDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = AppTimeoutConstants.ImplicitWait;

        return _driver;
    }

    // overriden methods
    protected override void NavigateToLoginPage()
    {
        _driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
        Console.WriteLine("Instagram login page opened successfully!");
    }

    protected override void PerformLoginSteps()
    {
        try
        {
            Console.Write("Enter username: ");
            SendKeys(LoginPageConstants.UsernameField.Invoke(_driver), Console.ReadLine() ?? string.Empty);

            Console.Write("Enter password: ");
            SendKeys(LoginPageConstants.PasswordField.Invoke(_driver), PasswordExtension.ReadPassword() ?? string.Empty);

            LoginPageConstants.SubmitButton.Invoke(_driver).Submit();
        }
        catch (Exception ex)
        {
            throw new LoginException($"Error during login steps: {ex.Message}");
        }
    }

    protected override void HandleLoginOutcome()
    {
        WebDriverWait wait = new WebDriverWait(_driver, AppTimeoutConstants.ExplicitWait);

        try
        {
            // Wait until any of the conditions are met: error banner, 2FA prompt, or 'onetap' URL
            wait.Until(IsLoginSuccessfulOrError);

            // Handle error banner if present
            if (ErrorMessageConstants.ErrorBanner(_driver) is LoginOutcome errorOutcome &&
                errorOutcome != LoginOutcome.Empty)
            {
                throw new LoginException(ErrorMessageConstants.IncorrectPasswordMessage);
            }

            // Handle 2FA if present
            if (TwoFactorAuthConstants.VerificationCodeField(_driver) is LoginOutcome twoFactorOutcome &&
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
        var saveInfo = _driver.FindElement(By.XPath("//button[text()='Save info']"));
        saveInfo.Click();
    }

    // helpers
    private string GetFirefoxExecutablePath()
    {
        string firefoxPath = @"C:\Program Files\Firefox Developer Edition\firefox.exe";

        while (!File.Exists(firefoxPath) || !_validationChain.Handle(firefoxPath))
        {
            Console.Write("Please enter the path to your Firefox Developer Edition executable \n(e.g., C:\\Program Files\\Firefox Developer Edition\\firefox.exe): ");
            firefoxPath = Console.ReadLine() ?? string.Empty;
        }

        return firefoxPath;
    }

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