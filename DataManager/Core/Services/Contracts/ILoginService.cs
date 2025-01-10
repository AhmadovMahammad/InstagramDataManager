using OpenQA.Selenium;

namespace DataManager.Core.Services.Contracts;
public interface ILoginService
{
    IWebDriver WebDriver { get; }
    void ExecuteLogin();
}
