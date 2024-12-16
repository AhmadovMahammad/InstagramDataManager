using DataManager.DesignPatterns.ChainOfResponsibilityDP.Contracts;
using DataManager.DesignPatterns.ChainOfResponsibilityDP.Implementations;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace DataManager.Automation;
public class SeleniumAutomation
{
    private readonly IWebDriver _driver;
    private readonly IChainHandler _validationChain;

    public SeleniumAutomation()
    {
        // set up validation chain
        _validationChain = new ArgumentNotEmptyHandler()
            .SetNext(new FileExistsHandler())
            .SetNext(new FileExtensionHandler([".exe"])); // Expecting .exe file

        // prompt user for Firefox executable path
        string firefoxDeveloperEditionPath = GetFirefoxExecutablePath();

        // set Firefox options
        FirefoxOptions options = new FirefoxOptions { BinaryLocation = firefoxDeveloperEditionPath };
        options.AddArgument("--start-maximized");

        // initialize the WebDriver with options
        _driver = new FirefoxDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    public void Execute()
    {
        try
        {

        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            _driver.Quit();
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