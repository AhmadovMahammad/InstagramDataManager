using OpenQA.Selenium;

namespace DataManager.DesignPatterns.Builder;
public class SeleniumTaskBuilder : ITaskBuilder
{
    private readonly IWebDriver _driver;
    private readonly HashSet<Action<IWebDriver>> _actions;

    public SeleniumTaskBuilder(IWebDriver webDriver)
    {
        _driver = webDriver;
        _actions = new HashSet<Action<IWebDriver>>();
    }

    public ITaskBuilder NavigateTo(string url)
    {
        _actions.Add((IWebDriver webDriver) =>
        {
            webDriver.Navigate().GoToUrl(url);
            Console.WriteLine($"Navigated to {url}");
        });

        return this;
    }

    public ITaskBuilder PerformAction(Action<IWebDriver> action)
    {
        _actions.Add(action);
        return this;
    }

    // crucial operation
    public void ExecuteTasks()
    {
        foreach (var action in _actions)
        {
            action.Invoke(_driver);
        }
    }
}
