using OpenQA.Selenium;

namespace DataManager.Core.Services.Contracts;
public interface ILoginService
{
    (bool startedSuccessfully, IWebDriver WebDriver) StartWebDriver();
    void ExecuteLogin();
}
