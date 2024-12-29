using DataManager.Constants.Enums;

namespace DataManager.Helpers.Extensions;
public static partial class ConsoleExtension
{
    private static readonly Dictionary<MessageType, ConsoleColor> MessageTypeColors = new()
    {
        { MessageType.Success, ConsoleColor.Green },
        { MessageType.Error, ConsoleColor.DarkRed },
        { MessageType.Info, ConsoleColor.Blue },
        { MessageType.Warning, ConsoleColor.Yellow },
    };

    public static void WriteMessage(this string message, MessageType messageType, bool addNewLine = true)
    {
        if (MessageTypeColors.TryGetValue(messageType, out var chosenColor))
        {
            Console.ForegroundColor = chosenColor;
        }

        Console.Write(message);
        if (addNewLine) Console.WriteLine();

        Console.ResetColor();
    }

    public static bool AskToProceed(this string message)
    {
        Console.WriteLine(message);
        Console.Write("> ");

        string? userInput = Console.ReadLine()?.ToLower();
        return userInput == "y";
    }

    public static void LogException(this Exception exception, string message)
    {
        $"{message} Details: {exception.Message}".WriteMessage(MessageType.Error);
    }

    public static void ClearLine()
    {
        int pos = Console.CursorLeft;
        if (pos < 0) return;

        while (pos > 0)
        {
            Console.SetCursorPosition(pos - 1, Console.CursorTop);
            Console.Write(" ");
            Console.SetCursorPosition(pos - 1, Console.CursorTop);
            pos--;
        }
    }

    public static void PrintBanner()
    {
        Console.WriteLine(@"
 _____            _               __  __                                               
|  __ \          | |             |  \/  |                                              
| |  | |   __ _  | |_    __ _    | \  / |   __ _   _ __     __ _    __ _    ___   _ __ 
| |  | |  / _` | | __|  / _` |   | |\/| |  / _` | | '_ \   / _` |  / _` |  / _ \ | '__|
| |__| | | (_| | | |_  | (_| |   | |  | | | (_| | | | | | | (_| | | (_| | |  __/ | |   
|_____/   \__,_|  \__|  \__,_|   |_|  |_|  \__,_| |_| |_|  \__,_|  \__, |  \___| |_|   
                                                                    __/ |              
                                                                   |___/               
                ");
    }
}