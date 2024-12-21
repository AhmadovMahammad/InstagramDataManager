using DataManager.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using static DataManager.Automation.Selenium.SeleniumAutomation;

namespace DataManager.Constants;
public static class ErrorMessageConstants
{
    public static readonly ConditionDelegate ErrorBanner = (driver) =>
    {
        try
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            var element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[contains(text(),'your password was incorrect.')]")));

            return new LoginOutcome(nameof(ErrorMessageConstants), element);
        }
        catch
        {
            return LoginOutcome.Empty;
        }
    };

    public const string IncorrectPasswordMessage = "The password entered is incorrect.";
    public const string LoginTimeoutMessage = "Operation could not be completed in the given time. Please check your internet connection or retry.";
}
