namespace DataManager.DesignPatterns.ChainOfResponsibilityDP.Implementations;
public class FileExistsHandler : AbstractHandler
{
    public override bool Handle(string input)
    {
        if (!File.Exists(input))
        {
            Console.WriteLine($"Error: The file '{input}' does not exist.");
            Console.WriteLine("Hint: Verify that the file path is correct and the file is accessible.");
            return false;
        }

        return base.Handle(input);
    }
}