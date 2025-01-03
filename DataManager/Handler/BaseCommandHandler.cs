using DataManager.Automation;
using DataManager.Constant.Enums;
using DataManager.DesignPattern.Strategy;
using DataManager.Factory;
using DataManager.Helper.Extension;
using OpenQA.Selenium;

namespace DataManager.Handler;
public abstract class BaseCommandHandler : ICommandHandler
{
    public abstract OperationType OperationType { get; }
    protected abstract void Execute(Dictionary<string, object> parameters);

    public void HandleCommand(IWebDriver webDriver)
    {
        if (webDriver is null)
            throw new ArgumentNullException(nameof(webDriver), "WebDriver cannot be null.");

        try
        {
            var defaultParameters = new Dictionary<string, object>
            {
                { "WebDriver", webDriver }
            };

            if ((OperationType & OperationType.FileBased) != 0)
            {
                var fileParameters = TryGetFileParameters;
                if (fileParameters is null)
                {
                    "Operation Aborted".WriteMessage(MessageType.Error);
                    return;
                }

                MergeDictionaries(defaultParameters, fileParameters);
            }

            Execute(defaultParameters);
        }
        catch (InvalidOperationException ex)
        {
            ex.LogException("Invalid operation during command execution.");
            throw;
        }
        catch (Exception ex)
        {
            ex.LogException("Unexpected error during command execution.");
            throw;
        }
    }

    private static Dictionary<string, object>? TryGetFileParameters
    {
        get
        {
            try
            {
                var automation = new FileAutomation();
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
    }

    private static void MergeDictionaries(Dictionary<string, object> defaultParams, Dictionary<string, object> additionalParams)
    {
        foreach (KeyValuePair<string, object> item in additionalParams)
        {
            defaultParams.Add(item.Key, item.Value);
        }
    }
}