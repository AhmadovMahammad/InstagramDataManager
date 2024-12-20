﻿using DataManager.Constants;
using DataManager.Constants.Enums;
using DataManager.Exceptions;
using DataManager.Extensions;
using DataManager.Factories;
using DataManager.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DataManager.Automation.Selenium;
public class SeleniumAutomation : LoginAutomation
{
    public delegate LoginOutcome ConditionDelegate(IWebDriver driver);

    public SeleniumAutomation() : base() { }

    // Overridden methods
    protected override IWebDriver InitializeDriver() => FirefoxDriverFactory.CreateDriver(_validationChain);

    protected override void NavigateToLoginPage()
    {
        Driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
        "Instagram login page opened successfully!".WriteMessage(MessageType.Success);
    }

    protected override void PerformLoginSteps()
    {
        try
        {
            Console.Write("Enter username: ");
            SendKeys(LoginPageConstants.UsernameField.Invoke(Driver), Console.ReadLine() ?? string.Empty);

            Console.Write("Enter password: ");
            SendKeys(LoginPageConstants.PasswordField.Invoke(Driver), PasswordExtension.ReadPassword() ?? string.Empty);

            LoginPageConstants.SubmitButton.Invoke(Driver).Submit();
        }
        catch (Exception ex)
        {
            throw new LoginException($"Error during login steps: {ex.Message}");
        }
    }

    protected override void HandleLoginOutcome()
    {
        WebDriverWait wait = new WebDriverWait(Driver, AppTimeoutConstants.ExplicitWait);

        try
        {
            // Wait until any of the conditions are met: error banner, 2FA prompt, or 'onetap' URL
            wait.Until(driver => IsLoginSuccessfulOrError(driver));

            HandleLoginError();
            HandleTwoFactorAuthenticationIfNeeded();
            "You successfully logged in.".WriteMessage(MessageType.Success);
        }
        catch (WebDriverTimeoutException)
        {
            throw new LoginException(ErrorMessageConstants.LoginTimeoutMessage);
        }
        catch (Exception ex)
        {
            throw new LoginException(ex.Message);
        }
    }

    protected override void SaveInfo()
    {
        try
        {
            WebDriverWait wait = new WebDriverWait(Driver, AppTimeoutConstants.ExplicitWait);
            var saveInfoButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[text()='Save info']")));

            saveInfoButton.JavaScriptClick();
        }
        catch (Exception ex)
        {
            throw new LoginException($"An error occurred while saving information: {ex.Message}");
        }
    }

    // Helper Methods
    private bool IsLoginSuccessfulOrError(IWebDriver driver)
    {
        return driver.Url.Contains("accounts/onetap") ||
               driver.Url.Contains("accounts/login/two_factor?next=%2F") ||
               driver.FindElements(By.XPath("//div[contains(text(),'your password was incorrect.')]")).Count > 0;
    }

    private void HandleLoginError()
    {
        if (ErrorMessageConstants.ErrorBanner(Driver) is LoginOutcome errorOutcome && errorOutcome != LoginOutcome.Empty)
        {
            throw new LoginException(ErrorMessageConstants.IncorrectPasswordMessage);
        }
    }

    private void HandleTwoFactorAuthenticationIfNeeded()
    {
        if (TwoFactorAuthConstants.VerificationCodeField(Driver) is LoginOutcome twoFactorOutcome && twoFactorOutcome != LoginOutcome.Empty)
        {
            if (twoFactorOutcome.Data is IWebElement webElement)
            {
                HandleTwoFactorAuthentication(webElement);
            }
        }
    }

    private void HandleTwoFactorAuthentication(IWebElement webElement)
    {
        Console.Write(TwoFactorAuthConstants.TwoFactorPromptMessage);
        string code = Console.ReadLine() ?? string.Empty;

        webElement.SendKeys(code);
        webElement.Submit();
    }
}