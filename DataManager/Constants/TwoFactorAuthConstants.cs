using DataManager.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using static DataManager.Automation.Selenium.SeleniumAutomation;

namespace DataManager.Constants;
public static class TwoFactorAuthConstants
{
    public static readonly ConditionDelegate VerificationCodeField = (driver) =>
    {
        try
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            var element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//input[@name='verificationCode']")));

            return new LoginOutcome(nameof(TwoFactorAuthConstants), element);
        }
        catch
        {
            return LoginOutcome.Empty;
        }
    };

    public const string TwoFactorPromptMessage = "Two-factor authentication required.\nEnter the 2FA code: ";
}