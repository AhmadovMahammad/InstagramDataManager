using DataManager.DesignPattern.ChainOfResponsibility;
using OpenQA.Selenium;

namespace DataManager.Automation;
public abstract class LoginAutomation
{
    public IWebDriver Driver { get; }
    protected readonly IChainHandler _validationChain;
    protected bool _errorOccurred;

    public LoginAutomation()
    {
        _validationChain = new ArgumentNotEmptyHandler()
            .SetNext(new FileExistsHandler())
            .SetNext(new FileExtensionHandler([".exe"]));

        Driver = InitializeDriver();
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
        catch (Exception)
        {
            throw;
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
