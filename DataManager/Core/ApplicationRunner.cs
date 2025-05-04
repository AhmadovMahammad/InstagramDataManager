using DataManager.Automation;
using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.Core.Services.Contracts;
using DataManager.Helper.Extension;
using DataManager.Model;

using OpenQA.Selenium;
using TableTower.Core.Builder;

namespace DataManager.Core;
public class ApplicationRunner
{
    private readonly ICommandResolver _commandHandler;
    private readonly ILoginService _loginService;
    private readonly IEnumerable<MenuModel> _availableOperations = AppConstant.GetAvailableOperations();

    public ApplicationRunner(ICommandResolver commandHandler, ILoginService loginService)
    {
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
    }

    public void Run()
    {
        bool loginSuccessful, stopProcess = false;
        IWebDriver? webDriver = null;

        if (!"To perform any operation, you must sign in to your Instagram account.\nDo you want to keep logging into your account? (y/n)".AskToProceed())
            return;

        DisplayMenu();

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
                    if (!"Login failed. Would you like to try again? (y/n)".AskToProceed())
                    {
                        stopProcess = true;
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
                if (stopProcess)
                {
                    webDriver?.Quit();
                }
            }
        } while (!loginSuccessful && webDriver != null);
    }

    private (bool, IWebDriver?) TryStartDriver()
    {
        (bool startedSuccessfully, IWebDriver? webDriver) output = (false, null);

        try
        {
            output = _loginService.StartWebDriver();
            if (output.startedSuccessfully)
            {
                _loginService.ExecuteLogin();
            }

            return (true, output.webDriver);
        }
        catch (Exception ex)
        {
            ex.LogException("Error during login execution.");
            return (false, output.webDriver ?? null);
        }
    }

    private void HandleCommands(IWebDriver webDriver)
    {
        string? input = string.Empty;
        while (!string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
        {
            input = "Command (Enter a number for command or type 'exit' to quit)".GetInput();
            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (TryGetCommandId(input, out int commandId))
            {
                try
                {
                    _commandHandler.Handle((CommandType)commandId, webDriver);
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
        _availableOperations.DisplayAsTable((TableOptions options) =>
        {
            options.Title = string.Empty;
            options.WrapData = false;
            options.EnableDataCount = false;
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
