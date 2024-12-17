using DataManager.Models;
using OpenQA.Selenium;
using static DataManager.Automation.Selenium.SeleniumAutomation;

namespace DataManager.Constants;
public class TwoFactorAuthConstants
{
    public static readonly ConditionDelegate VerificationCodeField = (driver) =>
    {
        try
        {
            var elements = driver.FindElements(By.XPath("//input[@name='verificationCode']"));
            if (elements.Count > 0 && elements[0].Displayed)
            {
                return new LoginOutcome(nameof(TwoFactorAuthConstants), elements[0]);
            }

            return LoginOutcome.Empty;
        }
        catch
        {
            return LoginOutcome.Empty;
        }
    };

    public const string TwoFactorPromptMessage = "Two-factor authentication required.\nEnter the 2FA code: ";
}