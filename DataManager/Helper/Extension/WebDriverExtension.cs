using DataManager.Constant;
using DataManager.Constant.Enums;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataManager.Helper.Extension;
public static class WebDriverExtension
{
    public static IWebDriver GetWebDriver(this IWebElement webElement) => ((IWrapsDriver)webElement).WrappedDriver;

    public static void JavaScriptClick(this IWebElement webElement)
    {
        IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)webElement.GetWebDriver();
        jsExecutor.ExecuteScript("arguments[0].click();", webElement);
    }

    public static IWebElement? FindElementWithRetries(this IWebDriver webDriver, string elementName, By by, int retries = 3, int initialDelay = 1500)
    {
        for (int attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                return webDriver.FindElement(by);
            }
            catch (NoSuchElementException) when (attempt <= retries)
            {
                $"Retrying to find webElement: '{elementName}'. [{attempt}/{retries}]".WriteMessage(MessageType.Warning);

                int currentDelay = initialDelay / attempt;
                Task.Delay(currentDelay).Wait();
            }
            catch (Exception)
            {
                $"Error finding webElement: '{elementName}'".WriteMessage(MessageType.Error);
                break;
            }
        }

        return null;
    }

    public static void EnsureDomLoaded(this IWebDriver webDriver)
    {
        WebDriverWait wait = new(webDriver, AppConstant.ExplicitWait);
        wait.Until(d =>
        {
            string? documentReadyState = ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString();
            return documentReadyState is not null and "complete";
        });
    }

    public static void WaitForElementVisible(this IWebDriver webDriver, IWebElement webElement, int timeoutSeconds = 10)
    {
        new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeoutSeconds))
            .Until(d => webElement.Displayed);
    }
}