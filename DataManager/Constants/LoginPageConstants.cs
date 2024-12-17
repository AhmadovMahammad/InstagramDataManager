using OpenQA.Selenium;

namespace DataManager.Constants;
public static class LoginPageConstants
{
    // Input Fields
    public static readonly Func<IWebDriver, IWebElement> UsernameField = (driver)
        => driver.FindElement(By.XPath("//input[@name='username']"));

    public static readonly Func<IWebDriver, IWebElement> PasswordField = (driver)
        => driver.FindElement(By.XPath("//input[@name='password']"));

    // Buttons
    public static readonly Func<IWebDriver, IWebElement> SubmitButton = (driver)
        => driver.FindElement(By.XPath("//button[@type='submit']"));
}
