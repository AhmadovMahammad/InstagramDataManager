using OpenQA.Selenium;

namespace DataManager.Constants;
public class LoginPageConstants
{
    // inputs
    public static Func<IWebDriver, IWebElement> usernameField = (IWebDriver driver)
        => driver.FindElement(By.XPath("//input[@name='username']"));

    public static Func<IWebDriver, IWebElement> passwordField = (IWebDriver driver)
        => driver.FindElement(By.XPath("//input[@name='password']"));

    // buttons
    public static Func<IWebDriver, IWebElement> submitButton = (IWebDriver driver)
        => driver.FindElement(By.XPath("//button[@type='submit']"));
}
