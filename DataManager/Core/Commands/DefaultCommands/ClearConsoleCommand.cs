using DataManager.Core.Commands.Contracts;

namespace DataManager.Core.Commands.DefaultCommands;
public class ClearConsoleCommand : ICommand
{
    public void Execute()
    {
        Console.Clear();
        Console.WriteLine("\x1b[3J");
    }
}
