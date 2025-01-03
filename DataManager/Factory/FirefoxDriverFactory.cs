using DataManager.Constant;
using DataManager.DesignPattern.ChainOfResponsibility;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Xml;

namespace DataManager.Factory;
public class FirefoxDriverFactory
{
    private static IChainHandler _validationChain = null!;
    private static readonly string _settingsPath = Path.Combine(Environment.CurrentDirectory, "Config", "settings.xml");

    public static IWebDriver CreateDriver(IChainHandler validationChain)
    {
        Console.WriteLine("\n"); // Firefox options are separated from the application menu using the Line Feed (LF). 
        _validationChain = validationChain;

        var webDriver = new FirefoxDriver(GetFirefoxOptions());
        webDriver.Manage().Timeouts().ImplicitWait = AppConstant.ImplicitWait;

        return webDriver;
    }

    private static FirefoxOptions? GetFirefoxOptions()
    {
        FirefoxOptions defaultOptions = new FirefoxOptions()
        {
            BinaryLocation = GetFirefoxExecutablePath(),
            LogLevel = FirefoxDriverLogLevel.Fatal,
        };

        // load settings dynamically.
        if (!File.Exists(_settingsPath))
        {
            return defaultOptions;
        }

        XmlReaderSettings xmlReaderSettings = new() { IgnoreWhitespace = true, IgnoreComments = true };
        using FileStream fileStream = new FileStream(_settingsPath, FileMode.Open, FileAccess.Read);
        using XmlReader xmlReader = XmlReader.Create(fileStream, xmlReaderSettings);

        while (xmlReader.Read())
        {
            if (xmlReader.NodeType == XmlNodeType.Element)
            {
                string name = xmlReader.Name;

                switch (name)
                {
                    case "headless":
                        if (xmlReader.Read() && xmlReader.NodeType == XmlNodeType.Text)
                        {
                            string value = xmlReader.Value;
                            if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out bool isHeadless) && isHeadless)
                            {
                                defaultOptions.AddArgument("--headless");
                            }
                        }
                        break;

                    case "disableBotDetection":
                        if (xmlReader.Read() && xmlReader.NodeType == XmlNodeType.Text)
                        {
                            string value = xmlReader.Value;
                            if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out bool disableBotDetection) && disableBotDetection)
                            {
                                defaultOptions.AddArgument("--disable-blink-features=AutomationControlled");
                                defaultOptions.SetPreference("dom.webdriver.enabled", false);
                                defaultOptions.SetPreference("useAutomationExtension", false);
                            }
                        }
                        break;
                }
            }
        }

        return defaultOptions;
    }

    private static string GetFirefoxExecutablePath()
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
