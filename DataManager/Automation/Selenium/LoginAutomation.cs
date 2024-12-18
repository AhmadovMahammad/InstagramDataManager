using DataManager.DesignPatterns.ChainOfResponsibility;
using DataManager.Exceptions;
using OpenQA.Selenium;

namespace DataManager.Automation.Selenium;
public abstract class LoginAutomation
{
    protected IWebDriver _driver;
    protected readonly IChainHandler _validationChain;
    protected bool _errorOccurred;

    public LoginAutomation()
    {
        _validationChain = new ArgumentNotEmptyHandler()
            .SetNext(new FileExistsHandler())
            .SetNext(new FileExtensionHandler([".exe"]));

        _driver = InitializeDriver();
    }

    public void ExecuteLogin()
    {
        try
        {
            NavigateToLoginPage();
            PerformLoginSteps();
            HandleLoginOutcome();
            SaveInfo();
        }
        catch (Exception ex)
        {
            throw new LoginException($"An error occurred. {ex.Message}");
        }
    }

    protected abstract IWebDriver InitializeDriver();
    protected abstract void NavigateToLoginPage();
    protected abstract void PerformLoginSteps();
    protected abstract void HandleLoginOutcome();
    protected abstract void SaveInfo();

    protected void SendKeys(IWebElement webElement, string data)
    {
        if (string.IsNullOrEmpty(webElement.Text) && webElement.Enabled)
        {
            webElement.Clear();
        }

        webElement.SendKeys(data);
    }
}
