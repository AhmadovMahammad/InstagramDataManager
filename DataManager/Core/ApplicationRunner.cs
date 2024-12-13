using DataManager.Constants;
using DataManager.Constants.Enums;

namespace DataManager.Core;
public class ApplicationRunner(ICommandHandler handler)
{
    private readonly ICommandHandler _handler = handler;

    public void Run()
    {
        Console.WriteLine("Welcome to Instagram Data Manager!\n");

        string? input;
        bool displayMenu = true; // Determines if the menu should be displayed

        do
        {
            if (displayMenu)
            {
                Console.WriteLine("============================================");
                Console.WriteLine("            Available Operations            ");
                Console.WriteLine("============================================\n");

                // Display available operations with index and description
                for (int i = 1; i <= AppConstant.AvailableOperations.Count; i++)
                {
                    (string action, string description) = AppConstant.AvailableOperations[i];
                    Console.WriteLine($"{i}. {action}: {description}");
                }
            }

            Console.Write("\nOperation Number: ");
            input = Console.ReadLine()?.ToLower();

            if (TryGetOperation(input, out var operationId))
            {
                displayMenu = true; // Display menu again for next iteration.
                _handler.Handle((OperationType)operationId);
            }
            else if (!string.IsNullOrWhiteSpace(input) && input != "exit")
            {
                displayMenu = false; // Invalid input, do not display menu.
                Console.WriteLine("Invalid operation number. Please try again.");
            }
        }
        while (!string.IsNullOrWhiteSpace(input) && !string.Equals(input, "exit"));
    }

    private static bool TryGetOperation(string? input, out int operationId)
    {
        operationId = 0;

        return int.TryParse(input, out operationId) &&
            AppConstant.AvailableOperations.ContainsKey(operationId);
    }
}
