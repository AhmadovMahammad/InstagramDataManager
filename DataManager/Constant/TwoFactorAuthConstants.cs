using DataManager.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using static DataManager.Delegates;

namespace DataManager.Constant;
public static class TwoFactorAuthConstants
{
    public static readonly ConditionDelegate VerificationCodeField = (webDriver) =>
    {
        try
        {
            WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
            var webElement = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//input[@name='verificationCode']")));

            return new LoginOutcome(nameof(TwoFactorAuthConstants), webElement);
        }
        catch
        {
            return LoginOutcome.Empty;
        }
    };

    public const string TwoFactorPromptMessage = "Two-factor authentication required.\nEnter the 2FA code: ";
}