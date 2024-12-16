using DataManager.Automation;
using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.DesignPatterns.StrategyDP.Implementations;

namespace DataManager.Handlers;
public abstract class BaseOperationHandler : IOperationHandler
{
    public abstract bool RequiresFile { get; }

    public void HandleOperation()
    {
        var parameters = RequiresFile
            ? GetFileParameters()
            : GetWebDriverParameters();

        Execute(parameters);
    }

    protected abstract void Execute(Dictionary<string, object> parameters);


    private Dictionary<string, object> GetFileParameters()
    {
        (string filePath, IFileFormatStrategy? strategy) = new FileAutomation().GetParams();

        return new Dictionary<string, object>
        {
            { "FilePath", filePath },
            { "FileFormatStrategy", strategy ?? new JsonFileFormatStrategy() }
        };
    }

    private Dictionary<string, object> GetWebDriverParameters()
    {
        SeleniumAutomation automation = new SeleniumAutomation();
        automation.ExecuteLogin();

        return new Dictionary<string, object>
        {
            { "WebDriver", automation.Driver }
        };
    }
}
