﻿using ConsoleTables;
using DataManager.Automation;
using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.Helper.Extension;
using DataManager.Model;
using OpenQA.Selenium;

namespace DataManager.Core;
public class ApplicationRunner(ICommandHandler handler)
{
    private readonly ICommandHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    private readonly IEnumerable<MenuModel> _availableOperations = AppConstant.GetAvailableOperations();

    public void Run()
    {
        ConsoleExtension.PrintBanner();

        bool loginSuccessful;
        IWebDriver? webDriver = null;

        if (!"To perform any operation, you must sign in to your Instagram account.\nDo you want to keep logging into your account? (y/n)".AskToProceed())
        {
            return;
        }

        do
        {
            try
            {
                (loginSuccessful, webDriver) = TryStartDriver();
                if (loginSuccessful && webDriver != null)
                {
                    HandleCommands(webDriver);
                }
                else
                {
                    webDriver?.Quit();
                    if (!"Login failed. Would you like to try again? (y/n)".AskToProceed())
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.LogException("An unexpected error occurred in the main Run loop.");
                break;
            }
            finally
            {
                webDriver?.Quit();
            }
        } while (!loginSuccessful && webDriver != null);
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
            ex.LogException("Error during login execution.");
            return (false, automation?.Driver);
        }
    }

    private void HandleCommands(IWebDriver webDriver)
    {
        string? input = string.Empty;
        while (!string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
        {
            DisplayMenu();

            Console.WriteLine("\nCommand (Enter a number for command or type 'exit' to quit)");
            Console.Write("> ");
            input = Console.ReadLine()?.ToLower();

            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (TryGetCommandId(input, out int commandId))
            {
                try
                {
                    _handler.Handle((CommandType)commandId, webDriver);
                }
                catch (Exception ex)
                {
                    ex.LogException("A critical error occurred. Contact support if the issue persists.");
                }
            }
            else
            {
                "Invalid command number. Please try again.".WriteMessage(MessageType.Error);
            }

            Console.WriteLine();
        }
    }

    private void DisplayMenu()
    {
        _availableOperations.DisplayAsTable((ConsoleTable table) =>
        {
            table.Options.EnableCount = false;
        }, "Key", "Action", "Description");
    }

    private bool TryGetCommandId(string? input, out int commandId)
    {
        commandId = 0;

        if (int.TryParse(input, out int parsedId))
        {
            var command = _availableOperations.FirstOrDefault(m => m.Key == parsedId);
            if (command != null)
            {
                commandId = parsedId;
                return true;
            }
        }

        return false;
    }
}
