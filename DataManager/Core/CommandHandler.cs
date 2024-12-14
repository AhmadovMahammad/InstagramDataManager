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
        // We receive this at the beginning because each operation requires a single file to function.
        Console.Write("Enter the file path to proceed: ");
        string? filePath = Console.ReadLine();

        // Validate the file using the chain of responsibility
        if (!_validationChain.Handle(filePath ?? string.Empty))
        {
            "File validation failed. Operation aborted.".WriteMessage(MessageType.Error);
            return;
        }

        IFileFormatStrategy fileFormatStrategy = Path.GetExtension(filePath)?.ToLower() switch
        {
            "json" => new JsonFileFormatStrategy(),
            "html" => new HtmlFileFormatStrategy(),
            _ => throw new InvalidOperationException("Unsupported file format") // Probably we will never reach here...
        };

        try
        {
            // Use the factory to get the appropriate handler for the operation
            var operationHandler = OperationFactory.CreateHandler(operationType, filePath!);
            if (operationHandler is null)
            {
                $"Operation '{operationType}' is not supported.".WriteMessage(MessageType.Warning);
                return;
            }

            operationHandler.Execute(fileFormatStrategy);
        }
        catch (Exception ex)
        {
            $"An error occurred during operation execution: {ex.Message}".WriteMessage(MessageType.Error);
        }
    }
}