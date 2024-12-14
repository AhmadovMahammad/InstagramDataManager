using DataManager.Constants.Enums;
using DataManager.DesignPatterns.ChainOfResponsibilityDP.Contracts;
using DataManager.DesignPatterns.ChainOfResponsibilityDP.Implementations;
using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.DesignPatterns.StrategyDP.Implementations;
using DataManager.Helpers.Extensions;

namespace DataManager.Core;
public interface ICommandHandler
{
    void Handle(OperationType operationType);
}

public class CommandHandler : ICommandHandler // Routes command-line inputs to tasks
{
    private readonly IChainHandler _validationChain;

    public CommandHandler()
    {
        _validationChain = new ArgumentNotEmptyHandler()
            .SetNext(new FileExistsHandler())
            .SetNext(new FileExtensionHandler(["json", "html"])); // Instagram data can only be exported as HTML or JSON.
    }

    public void Handle(OperationType operationType)
    {
        // First, check if the operation handler exists for the given operationType.
        // If the handler is null, exit early without needing user input.
        var operationHandler = OperationFactory.CreateHandler(operationType, string.Empty);
        if (operationHandler is null)
        {
            $"There is currently no support for operation: {operationType}".WriteMessage(MessageType.Warning);
            return;
        }

        // Now, prompt the user for the file path to proceed
        Console.Write("Enter the file path to proceed: ");
        string filePath = Console.ReadLine() ?? string.Empty;

        // Validate the file using the chain of responsibility
        if (!_validationChain.Handle(filePath))
        {
            "File validation failed. Operation aborted.".WriteMessage(MessageType.Error);
            return;
        }

        IFileFormatStrategy fileFormatStrategy = Path.GetExtension(filePath).ToLower() switch
        {
            ".json" => new JsonFileFormatStrategy(),
            ".html" => new HtmlFileFormatStrategy(),
            _ => throw new InvalidOperationException("Unsupported file format") // Probably we will never reach here...
        };

        try
        {
            // Execute the operation handler with the appropriate file format strategy
            operationHandler.Execute(fileFormatStrategy);
        }
        catch (Exception ex)
        {
            $"An error occurred during operation execution: {ex.Message}".WriteMessage(MessageType.Error);
        }
    }
}