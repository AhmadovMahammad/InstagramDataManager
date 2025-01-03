using DataManager.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using static DataManager.Automation.SeleniumAutomation;

namespace DataManager.Constant;
public static class ErrorMessageConstants
{
    public static readonly ConditionDelegate ErrorBanner = (webDriver) =>
    {
        try
        {
            WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
            var webElement = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[contains(text(),'your password was incorrect.')]")));

            return new LoginOutcome(nameof(ErrorMessageConstants), webElement);
        }
        catch
        {
            return LoginOutcome.Empty;
        }
    };

    public const string IncorrectPasswordMessage = "The password entered is incorrect.";
    public const string LoginTimeoutMessage = "Operation could not be completed in the given time. Please check your internet connection or retry.";
}
