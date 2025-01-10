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
        _validationChain = validationChain;
        return new FirefoxDriver(GetFirefoxOptions());
    }

    private static FirefoxOptions? GetFirefoxOptions()
    {
        FirefoxOptions firefoxOptions = new FirefoxOptions()
        {
            LogLevel = FirefoxDriverLogLevel.Fatal,
            EnableDevToolsProtocol = true,
        };

        if (!File.Exists(_settingsPath))
        {
            return firefoxOptions;
        }

        ReadSettings(ref firefoxOptions);
        return firefoxOptions;
    }

    private static void ReadSettings(ref FirefoxOptions firefoxOptions)
    {
        XmlReaderSettings xmlReaderSettings = new()
        {
            IgnoreWhitespace = true,
            IgnoreComments = true
        };

        using FileStream fileStream = new FileStream(_settingsPath, FileMode.Open, FileAccess.Read);
        using XmlReader xmlReader = XmlReader.Create(fileStream, xmlReaderSettings);

        while (xmlReader.Read())
        {
            if (xmlReader.NodeType == XmlNodeType.Element)
            {
                switch (xmlReader.Name.ToLowerInvariant())
                {
                    // Browser Settings
                    case "headless":
                        if (xmlReader.Read() && xmlReader.NodeType == XmlNodeType.Text)
                        {
                            string value = xmlReader.Value;
                            if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out bool isHeadless) && isHeadless)
                            {
                                firefoxOptions.AddArgument("--headless");
                            }
                        }
                        break;

                    case "firefoxpath":
                        if (xmlReader.Read() && xmlReader.NodeType == XmlNodeType.Text)
                        {
                            string value = xmlReader.Value;
                            while (!File.Exists(value) || !_validationChain.Handle(value))
                            {
                                Console.Write("Please enter the path to your Firefox Developer Edition executable \n(e.g., C:\\Program Files\\Firefox Developer Edition\\firefox.exe): ");
                                value = Console.ReadLine() ?? string.Empty;
                            }
                        }
                        break;

                    case "disablebotdetection":
                        if (xmlReader.Read() && xmlReader.NodeType == XmlNodeType.Text)
                        {
                            string value = xmlReader.Value;
                            if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out bool disableBotDetection) && disableBotDetection)
                            {
                                firefoxOptions.AddArgument("--disable-blink-features=AutomationControlled");
                                firefoxOptions.SetPreference("dom.webdriver.enabled", false);
                                firefoxOptions.SetPreference("useAutomationExtension", false);
                            }
                        }
                        break;

                    // Timeouts
                    case "implicitwait":
                        if (xmlReader.Read() && xmlReader.NodeType == XmlNodeType.Text)
                        {
                            if (int.TryParse(xmlReader.Value, out int implicitWait))
                            {
                                firefoxOptions.ImplicitWaitTimeout = TimeSpan.FromSeconds(implicitWait);
                            }
                        }
                        break;

                    case "pageload":
                        if (xmlReader.Read() && xmlReader.NodeType == XmlNodeType.Text)
                        {
                            if (int.TryParse(xmlReader.Value, out int pageLoadTimeout))
                            {
                                firefoxOptions.PageLoadTimeout = TimeSpan.FromSeconds(pageLoadTimeout);
                            }
                        }
                        break;

                    case "scripttimeout":
                        if (xmlReader.Read() && xmlReader.NodeType == XmlNodeType.Text)
                        {
                            if (int.TryParse(xmlReader.Value, out int scriptTimeout))
                            {
                                firefoxOptions.ScriptTimeout = TimeSpan.FromSeconds(scriptTimeout);
                            }
                        }
                        break;
                }
            }
        }
    }
}
