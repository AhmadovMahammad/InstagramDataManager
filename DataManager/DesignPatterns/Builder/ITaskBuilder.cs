using OpenQA.Selenium;

namespace DataManager.DesignPatterns.Builder;
public interface ITaskBuilder
{
    ITaskBuilder NavigateTo(string url);
    ITaskBuilder PerformAction(Action<IWebDriver> action);
    void ExecuteTasks();
}
