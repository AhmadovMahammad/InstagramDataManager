using DataManager.Constants.Enums;

namespace DataManager.Extensions;
public static partial class ConsoleExtension
{
    private static readonly Dictionary<MessageType, ConsoleColor> MessageTypeColors = new()
        {
            { MessageType.Success, ConsoleColor.Green },
            { MessageType.Error, ConsoleColor.DarkRed },
            { MessageType.Info, ConsoleColor.Blue },
            { MessageType.Warning, ConsoleColor.Yellow },
        };

    public static void WriteMessage(this string message, MessageType messageType)
    {
        if (MessageTypeColors.TryGetValue(messageType, out var chosenColor))
        {
            Console.ForegroundColor = chosenColor;
        }

        Console.WriteLine(message);
        Console.ResetColor();
    }
}