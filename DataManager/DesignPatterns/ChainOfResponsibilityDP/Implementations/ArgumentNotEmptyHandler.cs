namespace DataManager.DesignPatterns.ChainOfResponsibilityDP.Implementations;
public class ArgumentNotEmptyHandler : AbstractHandler
{
    public override bool Handle(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Error: The provided file path is empty or contains only whitespace.");
            Console.WriteLine("Hint: Make sure to provide a valid file path to a JSON file.");
            return false;
        }

        return base.Handle(input);
    }
}