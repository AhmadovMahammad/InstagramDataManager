using DataManager.Constants.Enums;

namespace DataManager.Helpers.Extensions;
public static class ConsoleExtension
{
    private static readonly Dictionary<MessageType, ConsoleColor> MessageTypeColors = new()
    {
        { MessageType.Success, ConsoleColor.Green },
        { MessageType.Error, ConsoleColor.DarkRed },
        { MessageType.Info, ConsoleColor.Blue },
        { MessageType.Warning, ConsoleColor.Yellow }
    };

    public static void WriteMessage(this string message, MessageType messageType)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        if (MessageTypeColors.TryGetValue(messageType, out var chosenColor))
        {
            Console.ForegroundColor = chosenColor;
        }

        Console.WriteLine(message);
        Console.ForegroundColor = originalColor; // Reset to original color
    }
}
