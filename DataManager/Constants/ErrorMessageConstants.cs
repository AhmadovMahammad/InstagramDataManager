using DataManager.Models;
using OpenQA.Selenium;
using static DataManager.Automation.Selenium.SeleniumAutomation;

namespace DataManager.Constants;
public static class ErrorMessageConstants
{
    public static readonly ConditionDelegate ErrorBanner = (driver) =>
    {
        try
        {
            var elements = driver.FindElements(By.XPath("//div[contains(text(),'your password was incorrect.')]"));
            if (elements.Count > 0 && elements[0].Displayed)
            {
                return new LoginOutcome(nameof(ErrorMessageConstants), elements[0]);
            }

            return LoginOutcome.Empty;
        }
        catch
        {
            return LoginOutcome.Empty;
        }
    };

    public const string IncorrectPasswordMessage = "The password entered is incorrect.";
    public const string LoginTimeoutMessage = "Operation could not be completed in the given time or Verify the internet connection.";
}
