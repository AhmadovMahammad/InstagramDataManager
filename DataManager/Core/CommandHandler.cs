using DataManager.Constants.Enums;
using DataManager.Factories;
using DataManager.Helpers.Extensions;
using OpenQA.Selenium;

namespace DataManager.Core;
public interface ICommandHandler
{
    void Handle(CommandType commandType, IWebDriver webDriver);
}

public class CommandHandler : ICommandHandler // Routes command-line inputs to tasks
{
    public void Handle(CommandType commandType, IWebDriver webDriver)
    {
        if (commandType == CommandType.ClearConsole)
        {
            Console.Clear();
            Console.WriteLine("\x1b[3J");
            return;
        }

        // First, check if the command handler exists for the given commandType.
        // If the command is null, exit early without needing user input.
        var commandHandler = CommandFactory.CreateHandler(commandType);
        if (commandHandler is null)
        {
            $"There is currently no support for command: {commandType}".WriteMessage(MessageType.Warning);
            return;
        }

        try
        {
            commandHandler.HandleCommand(webDriver); // Polymorphic call to handle both types
        }
        catch (Exception ex)
        {
            $"An error occurred during command execution: {ex.Message}".WriteMessage(MessageType.Error);
        }
    }
}