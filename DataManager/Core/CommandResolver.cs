using DataManager.Constant.Enums;
using DataManager.Core.Commands.Contracts;
using DataManager.Core.Commands.DefaultCommands;
using DataManager.Helper.Extension;
using OpenQA.Selenium;

namespace DataManager.Core;
public interface ICommandResolver
{
    void Handle(CommandType commandType, IWebDriver webDriver);
}

public class CommandResolver : ICommandResolver // Routes command-line inputs to tasks
{
    private readonly Dictionary<CommandType, ICommand> _commands = new Dictionary<CommandType, ICommand>()
    {
        { CommandType.Get_Info, new GetInfoCommand() },
        { CommandType.Clear_Console, new ClearConsoleCommand() },
    };

    public void Handle(CommandType commandType, IWebDriver webDriver)
    {
        if (_commands.TryGetValue(commandType, out ICommand? command) && command != null)
        {
            command.Execute();
            return;
        }

        // First, check if the task handler exists for the given commandType.
        // If the task is null, exit early without needing user input.
        var taskHandler = Factory.TaskFactory.CreateTask(commandType);
        if (taskHandler is null)
        {
            $"There is currently no support for command: {commandType}".WriteMessage(MessageType.Warning);
            return;
        }

        try
        {
            taskHandler.HandleTask(webDriver); // Polymorphic call to handle both types
        }
        catch (Exception ex)
        {
            $"An error occurred during command execution: {ex.Message}".WriteMessage(MessageType.Error);
        }
    }
}