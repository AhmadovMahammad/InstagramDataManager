using ConsoleTables;
using DataManager.Constants;
using DataManager.Constants.Enums;
using DataManager.Extensions;
using DataManager.Models;

namespace DataManager.Core;
public class ApplicationRunner(ICommandHandler handler)
{
    private readonly ICommandHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    public void Run()
    {
        ConsoleExtension.PrintBanner();

        string? input = string.Empty;
        while (!IsExitCommand(input))
        {
            DisplayMenu();

            Console.Write("\nOperation (Enter a number for an operation or type 'exit' to quit): ");
            input = Console.ReadLine()?.ToLower();

            if (IsExitCommand(input)) break;

            if (IsValidOperationInput(input, out int operationId))
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
            else
            {
                "Invalid operation number. Please try again.".WriteMessage(MessageType.Error);
            }
        }
    }

    private void DisplayMenu()
    {
        Console.WriteLine("Available Operations\n");

        var menuItems = AppConstant.AvailableOperations
            .Select(op => new MenuModel
            {
                Key = op.Key,
                Action = op.Value.action,
                Description = op.Value.description
            }).ToList();

        menuItems.DisplayAsTable((ConsoleTable table) =>
        {
            table.Options.EnableCount = false;
        }, "Key", "Action", "Description");
    }

    private bool IsExitCommand(string? input)
     => string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase);

    private bool IsValidOperationInput(string? input, out int operationId)
        => int.TryParse(input, out operationId) && AppConstant.AvailableOperations.ContainsKey(operationId);
}
