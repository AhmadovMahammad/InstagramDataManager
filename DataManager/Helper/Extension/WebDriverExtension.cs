using DataManager.Constant;
using DataManager.Constant.Enums;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataManager.Helper.Extension;
public static class WebDriverExtension
{
    public static IWebElement? FindWebElement(this IWebDriver webDriver, By by, WebElementPriorityType priorityType)
    {
        (int retries, int initialDelay) = priorityType switch
        {
            WebElementPriorityType.High => (5, 500),
            WebElementPriorityType.Medium => (3, 1500),
            WebElementPriorityType.Low => (1, 3000),
            _ => (3, 1500)
        };

        for (int attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                return webDriver.FindElement(by);
            }
            catch (NoSuchElementException) when (attempt < retries)
            {
                int currentDelay = initialDelay / attempt;
                Task.Delay(currentDelay).Wait();
            }
            catch (Exception)
            {
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

    public static void ScrollToElement(this IWebDriver webDriver, IWebElement webElement)
    {
        ((IJavaScriptExecutor)webDriver).ExecuteScript(
            "arguments[0].scrollIntoView(true);", webElement);

        WaitForElementVisible(webDriver, webElement);
    }

    public static void ScrollToBottom(this IWebDriver webDriver, IWebElement webElement, int pixels = 200)
    {
        var jsExecutor = (IJavaScriptExecutor)webDriver;
        jsExecutor.ExecuteScript(
            "arguments[0].scrollTop += arguments[1];", webElement, pixels);
    }

    public static void WaitForElementVisible(this IWebDriver webDriver, IWebElement webElement, int timeoutSeconds = 10)
    {
        new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeoutSeconds))
            .Until(d => webElement.Displayed);
    }

    public static IWebDriver GetWebDriver(this IWebElement webElement) => ((IWrapsDriver)webElement).WrappedDriver;
}