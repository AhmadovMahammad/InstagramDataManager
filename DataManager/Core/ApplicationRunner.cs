using DataManager.Constants;
using DataManager.Constants.Enums;
using DataManager.Helpers.Extensions;
using DataManager.Models.View;

namespace DataManager.Core;
public class ApplicationRunner(ICommandHandler handler)
{
    private readonly ICommandHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    private bool _displayMenu = true; // Determines if the menu should be displayed

    public void Run()
    {
        Console.WriteLine("Welcome to Instagram Data Manager!");

        string? input;

        do
        {
            DisplayMenu();

            Console.Write("\nOperation Number: ");
            input = Console.ReadLine()?.ToLower();

            if (TryGetOperation(input, out var operationId))
            {
                try
                {
                    _handler.Handle((OperationType)operationId);
                    _displayMenu = true;
                    // Display menu again for next iteration.
                    // Because a successful process will take a long time and the menu will remain at very high position.
                }
                catch (Exception ex)
                {
                    _displayMenu = false;
                    ex.Message.WriteMessage(MessageType.Error);
                }
            }
            else if (!string.IsNullOrWhiteSpace(input) && input != "exit")
            {
                "Invalid operation number. Please try again.".WriteMessage(MessageType.Error);
                _displayMenu = false; // Invalid input, do not display menu.
            }
        }
        while (!string.IsNullOrWhiteSpace(input) && !string.Equals(input, "exit"));
    }

    private void DisplayMenu()
    {
        if (_displayMenu)
        {
            Console.WriteLine("\nAvailable Operations\n");
            AppConstant.AvailableOperations
                .Select(op => new MenuModel { Key = op.Key, Action = op.Value.action, Description = op.Value.description })
                .DisplayAsTable(ConsoleColor.Blue, "Key", "Action", "Description");
        }
    }

    private bool TryGetOperation(string? input, out int operationId)
    {
        return int.TryParse(input, out operationId) && AppConstant.AvailableOperations.ContainsKey(operationId);
    }
}
