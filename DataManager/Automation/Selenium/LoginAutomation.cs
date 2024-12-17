using DataManager.DesignPatterns.ChainOfResponsibility;
using OpenQA.Selenium;

namespace DataManager.Automation.Selenium;
public abstract class LoginAutomation
{
    protected IWebDriver _driver;
    protected readonly IChainHandler _validationChain;

    public LoginAutomation()
    {
        _validationChain = new ArgumentNotEmptyHandler()
            .SetNext(new FileExistsHandler())
            .SetNext(new FileExtensionHandler([".exe"]));

        _driver = InitializeDriver();
    }

    public void ExecuteLogin()
    {
        NavigateToLoginPage();
        PerformLoginSteps();
        HandleLoginOutcome();
    }

    protected abstract IWebDriver InitializeDriver();
    protected abstract void PerformLoginSteps();
    protected abstract void HandleLoginOutcome();
    protected abstract void SaveInfo();

    protected void NavigateToLoginPage()
    {
        _driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
        Console.WriteLine("Instagram login page opened successfully!");
    }

    protected void SendKeys(IWebElement webElement, string data)
    {
        if (string.IsNullOrEmpty(webElement.Text) && webElement.Enabled)
        {
            webElement.Clear();
        }

        webElement.SendKeys(data);
    }
}
