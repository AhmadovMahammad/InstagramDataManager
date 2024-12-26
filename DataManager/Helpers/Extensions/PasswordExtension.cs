namespace DataManager.Helpers.Extensions;
public static partial class PasswordExtension
{
    public static string ReadPassword()
    {
        string enteredPassword = string.Empty;
        ConsoleKeyInfo info = Console.ReadKey(true);

        while (info.Key != ConsoleKey.Enter)
        {
            if (info.Key != ConsoleKey.Backspace)
            {
                Console.Write("*");
                enteredPassword += info.KeyChar;
            }
            else if (info.Key == ConsoleKey.Backspace)
            {
                if (!string.IsNullOrEmpty(enteredPassword))
                {
                    enteredPassword = enteredPassword[..^1];
                    int pos = Console.CursorLeft;
                    Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    Console.Write(" ");
                    Console.SetCursorPosition(pos - 1, Console.CursorTop);
                }
            }

            info = Console.ReadKey(true);
        }

        Console.WriteLine('\n');
        return enteredPassword;
    }
}
