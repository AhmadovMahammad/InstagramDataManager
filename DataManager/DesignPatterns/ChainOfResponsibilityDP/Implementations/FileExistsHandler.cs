using DataManager.Constants.Enums;
using DataManager.Helpers.Extensions;

namespace DataManager.DesignPatterns.ChainOfResponsibilityDP.Implementations;
public class FileExistsHandler : AbstractHandler
{
    public override bool Handle(string input)
    {
        if (!File.Exists(input))
        {
            $"Error: The file '{input}' does not exist.".WriteMessage(MessageType.Error);
            "Hint: Verify that the file path is correct and the file is accessible.".WriteMessage(MessageType.Info);
            return false;
        }

        return base.Handle(input);
    }
}