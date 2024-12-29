using DataManager.Constants.Enums;
using System.Runtime.InteropServices;

namespace DataManager.Helpers.Extensions;
public static partial class ConsoleExtension
{
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern nint GetConsoleWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ShowWindow(nint hWnd, int nCmdShow);

    private const int SW_MAXIMIZE = 3;

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

    public static void MaximizeConsoleWindow()
    {
        nint consoleWindow = GetConsoleWindow();
        if (consoleWindow != nint.Zero)
        {
            ShowWindow(consoleWindow, SW_MAXIMIZE);
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
}