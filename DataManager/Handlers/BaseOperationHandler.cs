using DataManager.Automation;
using DataManager.Automation.Selenium;
using DataManager.Constants.Enums;
using DataManager.DesignPatterns.Strategy;
using DataManager.Extensions;
using DataManager.Factories;
using OpenQA.Selenium;

namespace DataManager.Handlers;
public abstract class BaseOperationHandler : IOperationHandler
{
    public abstract bool RequiresFile { get; }
    public event Action? OnDriverQuit;

    public void HandleOperation()
    {
        try
        {
            Dictionary<string, object>? parameters = RequiresFile
                ? GetFileParameters()
                : GetWebDriverParameters();

            if (parameters is null)
            {
                Console.WriteLine("Operation aborted.");
                return;
            }

            Execute(parameters);
        }
        catch (Exception)
        {
            Console.WriteLine("Action cannot be carried out due to an error.");
        }
        finally
        {
            if (!RequiresFile)
            {
                OnDriverQuit?.Invoke();
            }
        }
    }

    protected abstract void Execute(Dictionary<string, object> parameters);


    private Dictionary<string, object>? GetFileParameters()
    {
        try
        {
            FileAutomation automation = new FileAutomation();
            (string filePath, IFileFormatStrategy? strategy) = automation.GetParams();

            return new Dictionary<string, object>
            {
                { "FilePath", filePath },
                { "FileFormatStrategy", strategy ?? new JsonFileFormatStrategy() }
            };
        }
        catch (Exception ex)
        {
            ex.Message.WriteMessage(MessageType.Error);
            return null;
        }
    }

    private Dictionary<string, object>? GetWebDriverParameters()
    {
        SeleniumAutomation automation = null!;

        try
        {
            automation = new SeleniumAutomation();
            OnDriverQuit += () => QuitDriver(automation.Driver);

            automation.ExecuteLogin();
            return new Dictionary<string, object>
            {
                { "WebDriver", automation.Driver }
            };
        }
        catch (Exception ex)
        {
            ex.Message.WriteMessage(MessageType.Error);
            throw;
        }
    }

    private static void QuitDriver(IWebDriver driver)
    {
        driver?.Quit();
    }
}
