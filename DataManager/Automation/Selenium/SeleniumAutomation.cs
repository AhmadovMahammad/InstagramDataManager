using DataManager.Constants;
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
        options.AddArguments(/*"--headless",*/ "--start-maximized");

        _driver = new FirefoxDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = AppTimeoutConstants.ImplicitWait;

        return _driver;
    }

    protected override void PerformLoginSteps()
    {
        Console.Write("Enter username: ");
        SendKeys(LoginPageConstants.UsernameField.Invoke(_driver), Console.ReadLine() ?? string.Empty);

        Console.Write("Enter password: ");
        SendKeys(LoginPageConstants.PasswordField.Invoke(_driver), PasswordExtension.ReadPassword() ?? string.Empty);

        LoginPageConstants.SubmitButton.Invoke(_driver).Submit();
    }

    protected override void HandleLoginOutcome()
    {
        WebDriverWait wait = new WebDriverWait(_driver, AppTimeoutConstants.ExplicitWait);

        try
        {
            // Wait until any of the conditions are met: error banner, 2FA prompt, or 'onetap' URL
            wait.Until(driver =>
            {
                return
                    driver.Url.Contains("accounts/onetap") ||
                    driver.FindElements(By.XPath("//div[contains(text(),'your password was incorrect.')]")).Count > 0 ||
                    driver.Url.Contains("https://www.instagram.com/accounts/login/two_factor?next=%2F");
            });

            // Handle error banner if present
            if (ErrorMessageConstants.ErrorBanner(_driver) is LoginOutcome errorOutcome && errorOutcome != LoginOutcome.Empty)
            {
                Console.WriteLine(ErrorMessageConstants.IncorrectPasswordMessage);
                return;
            }

            // Handle 2FA if present
            if (TwoFactorAuthConstants.VerificationCodeField(_driver) is LoginOutcome twoFactorOutcome && twoFactorOutcome != LoginOutcome.Empty)
            {
                if (twoFactorOutcome.Data is IWebElement webElement)
                {
                    Console.Write(TwoFactorAuthConstants.TwoFactorPromptMessage);
                    string code = Console.ReadLine() ?? string.Empty;

                    webElement.SendKeys(code);
                    webElement.Submit();
                    return;
                }
            }

            Console.WriteLine("Logined Successfully.");
        }
        catch (WebDriverTimeoutException)
        {
            throw new TimeoutException(ErrorMessageConstants.LoginTimeoutMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    protected override void SaveInfo()
    {

    }

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
}