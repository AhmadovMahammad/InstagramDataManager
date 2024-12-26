using DataManager.Constants;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataManager.Helpers.Extensions;
public static class WebDriverExtension
{
    public static void JavaScriptClick(this IWebElement webElement)
    {
        IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)webElement.GetWebDriver();
        jsExecutor.ExecuteScript("arguments[0].click();", webElement);
    }

    public static IWebDriver GetWebDriver(this IWebElement webElement) => ((IWrapsDriver)webElement).WrappedDriver;

    public static void EnsureDomLoaded(IWebDriver webDriver)
    {
        WebDriverWait wait = new(webDriver, AppConstant.ExplicitWait);
        wait.Until(d =>
        {
            string? documentReadyState = ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString();
            return documentReadyState != null && documentReadyState == "complete";
        });
    }
}