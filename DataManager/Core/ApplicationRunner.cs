using DataManager.Constants;
using DataManager.Constants.Enums;
using DataManager.Extensions;
using DataManager.Models.View;

namespace DataManager.Core;
public class ApplicationRunner(ICommandHandler handler)
{
    private readonly ICommandHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    public void Run()
    {
        Console.WriteLine("Welcome to Instagram Data Manager!");

        string? input;

        do
        {
            DisplayMenu();

            Console.Write("\nOperation Number: ");
            input = Console.ReadLine()?.ToLower();

            if (TryGetOperation(input, out int operationId) && operationId != default)
            {
                try
                {
                    _handler.Handle((OperationType)operationId);
                }
                catch (Exception ex)
                {
                    ex.Message.WriteMessage(MessageType.Error);
                }
            }
            else if (!string.IsNullOrWhiteSpace(input) && input != "exit")
            {
                "Invalid operation number. Please try again.".WriteMessage(MessageType.Error);
            }
        }
        while (!string.IsNullOrWhiteSpace(input) && !string.Equals(input, "exit"));
    }

    private void DisplayMenu()
    {
        Console.WriteLine("\nAvailable Operations\n");
        AppConstant.AvailableOperations
            .Select(op => new MenuModel { Key = op.Key, Action = op.Value.action, Description = op.Value.description })
            .DisplayAsTable(ConsoleColor.Blue, "Key", "Action", "Description");
    }

    private bool TryGetOperation(string? input, out int operationId)
    {
        return int.TryParse(input, out operationId) && AppConstant.AvailableOperations.ContainsKey(operationId);
    }
}
