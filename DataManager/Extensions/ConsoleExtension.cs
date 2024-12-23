using DataManager.Constants.Enums;
using System.Runtime.InteropServices;

namespace DataManager.Extensions;
public static partial class ConsoleExtension
{
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

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
        IntPtr consoleWindow = GetConsoleWindow();
        if (consoleWindow != IntPtr.Zero)
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
}