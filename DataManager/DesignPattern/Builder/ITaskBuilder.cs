using OpenQA.Selenium;

namespace DataManager.DesignPattern.Builder;
public interface ITaskBuilder
{
    ITaskBuilder NavigateTo(string url);
    ITaskBuilder PerformAction(Action<IWebDriver> action);
    void ExecuteTasks();
}
