using DataManager.Constants;
using DataManager.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace DataManager.Automation;
public class SeleniumAutomation : LoginAutomation
{
    public IWebDriver Driver => _driver;

    protected override IWebDriver InitializeDriver()
    {
        string firefoxPath = GetFirefoxExecutablePath();

        FirefoxOptions options = new() { BinaryLocation = firefoxPath };
        options.AddArgument("--headless");

        _driver = new FirefoxDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = AppConstant.ImplicitWait;

        return _driver;
    }

    protected override void PerformLoginSteps()
    {
        Console.Write("Enter username: ");
        SendKeys(LoginPageConstants.usernameField.Invoke(_driver), Console.ReadLine() ?? string.Empty);

        Console.Write("Enter password: ");
        SendKeys(LoginPageConstants.passwordField.Invoke(_driver), Extension.ReadPassword() ?? string.Empty);

        LoginPageConstants.submitButton.Invoke(_driver).Submit();
    }

    protected override void HandleLoginOutcome()
    {
        WebDriverWait wait = new WebDriverWait(_driver, AppConstant.ExplicitWait);

        try
        {
            wait.Until(driver =>
                driver.FindElements(By.XPath("//div[contains(text(),'your password was incorrect')]")).Count > 0 ||
                driver.FindElements(By.XPath("//input[@name='verificationCode']")).Count > 0 ||
                driver.Url.Contains("accounts/onetap"));

            if (_driver.FindElements(By.XPath("//div[contains(text(),'your password was incorrect')]")).Count > 0)
            {
                Console.WriteLine("The password entered is incorrect.");
            }
            else if (_driver.FindElements(By.XPath("//input[@name='verificationCode']")).Count > 0)
            {
                Console.WriteLine("Two-factor authentication required.");
                Console.Write("Enter the 2FA code: ");
                string code = Console.ReadLine() ?? string.Empty;
                IWebElement verificationField = _driver.FindElement(By.XPath("//input[@name='verificationCode']"));
                verificationField.SendKeys(code);
                verificationField.Submit();
            }
        }
        catch (WebDriverTimeoutException)
        {
            throw new TimeoutException($"Operation could not be completed in {AppConstant.ImplicitWait.Seconds} seconds or Verify the internet connection.");
        }
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