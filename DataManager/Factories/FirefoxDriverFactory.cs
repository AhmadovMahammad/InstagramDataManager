using DataManager.Constants;
using DataManager.DesignPatterns.ChainOfResponsibility;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Xml;

namespace DataManager.Factories;
public class FirefoxDriverFactory
{
    private static IChainHandler _validationChain = null!;

    public static IWebDriver CreateDriver(IChainHandler validationChain)
    {
        _validationChain = validationChain;

        var driver = new FirefoxDriver(GetFirefoxOptions());
        driver.Manage().Timeouts().ImplicitWait = AppTimeoutConstants.ImplicitWait;

        return driver;
    }

    private static FirefoxOptions? GetFirefoxOptions()
    {
        FirefoxOptions defaultOptions = new FirefoxOptions()
        {
            BinaryLocation = GetFirefoxExecutablePath()
        };

        // load settings dynamically.
        string settingsPath = Path.Combine(Environment.CurrentDirectory, "Settings.xml");
        if (!File.Exists(settingsPath))
        {
            return defaultOptions;
        }

        XmlReaderSettings xmlReaderSettings = new() { IgnoreWhitespace = true, IgnoreComments = true };
        using FileStream fileStream = new FileStream(settingsPath, FileMode.Open);
        using XmlReader xmlReader = XmlReader.Create(fileStream, xmlReaderSettings);

        while (xmlReader.Read())
        {
            if (xmlReader.NodeType == XmlNodeType.Element)
            {
                string name = xmlReader.Name;
                string value = xmlReader.Value;

                switch (name)
                {
                    case "profilePath":
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            defaultOptions.Profile = new FirefoxProfile(value);
                        }
                        break;

                    case "headless":
                        if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out bool isHeadless) && isHeadless)
                        {
                            defaultOptions.AddArgument("--headless");
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
