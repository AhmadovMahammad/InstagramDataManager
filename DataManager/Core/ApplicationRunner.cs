using ConsoleTables;
using DataManager.Automation.Selenium;
using DataManager.Constants;
using DataManager.Constants.Enums;
using DataManager.Extensions;
using DataManager.Models;
using OpenQA.Selenium;

namespace DataManager.Core;
public class ApplicationRunner(ICommandHandler handler)
{
    private readonly ICommandHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    public void Run()
    {
        ConsoleExtension.PrintBanner();

        bool loginSuccessful;
        IWebDriver? webDriver;

        do
        {
            Console.WriteLine("To perform any operation, you must sign in to your Instagram account. Please note that your credentials will be deleted when the program ends.\n\n");
            (loginSuccessful, webDriver) = TryStartDriver();

            if (loginSuccessful)
            {
                //todo
            }
            else
            {
                webDriver?.Quit();

                Console.WriteLine("Login failed. Would you like to try again? (y/n)");
                string? retry = Console.ReadLine()?.ToLower();

                if (retry != "y")
                {
                    break;
                }
            }
        } while (!loginSuccessful && webDriver is not null);
    }

    private (bool, IWebDriver?) TryStartDriver()
    {
        SeleniumAutomation automation = null!;

        try
        {
            automation = new SeleniumAutomation();
            automation.ExecuteLogin();

            return (true, automation.Driver);
        }
        catch (Exception ex)
        {
            ex.Message.WriteMessage(MessageType.Error);
            return (false, automation.Driver);
        }
    }

    private void DisplayMenu()
    {
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

    //private bool IsExitCommand(string? input)
    //{
    //    return string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase);
    //}

    //private bool IsValidOperationInput(string? input, out int operationId)
    //{
    //    return int.TryParse(input, out operationId) && AppConstant.AvailableOperations.ContainsKey(operationId);
    //}
}
