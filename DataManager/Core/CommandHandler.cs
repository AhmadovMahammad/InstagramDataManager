using DataManager.Constants.Enums;
using DataManager.Extensions;

namespace DataManager.Core;
public interface ICommandHandler
{
    void Handle(OperationType operationType);
}

public class CommandHandler : ICommandHandler // Routes command-line inputs to tasks
{
    public void Handle(OperationType operationType)
    {
        // First, check if the operation handler exists for the given operationType.
        // If the handler is null, exit early without needing user input.
        var operationHandler = OperationFactory.CreateHandler(operationType);
        if (operationHandler is null)
        {
            $"There is currently no support for operation: {operationType}".WriteMessage(MessageType.Warning);
            return;
        }

        try
        {
            operationHandler.HandleOperation(); // Polymorphic call to handle both types
        }
        catch (Exception ex)
        {
            $"An error occurred during operation execution: {ex.Message}".WriteMessage(MessageType.Error);
        }
    }
}