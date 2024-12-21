using OpenQA.Selenium;

namespace DataManager.Extensions;
public static class WebDriverExtension
{
    public static void JavaScriptClick(this IWebElement element)
    {
        IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)element.GetWebDriver();
        jsExecutor.ExecuteScript("arguments[0].click();", element);
    }

    public static IWebDriver GetWebDriver(this IWebElement element) => ((IWrapsDriver)element).WrappedDriver;
}