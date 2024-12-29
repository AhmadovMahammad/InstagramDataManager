using DataManager.DesignPatterns.ChainOfResponsibility;
using DataManager.DesignPatterns.Strategy;

namespace DataManager.Automation;
public class FileAutomation
{
    private readonly IChainHandler _validationChain;

    public FileAutomation()
    {
        _validationChain = new ArgumentNotEmptyHandler()
            .SetNext(new FileExistsHandler())
            .SetNext(new FileExtensionHandler(["json", "html"])); // Instagram data can only be exported as HTML or JSON.
    }

    public (string filePath, IFileFormatStrategy? strategy) GetParams()
    {
        string filePath = GetFilePath();

        if (_validationChain.Handle(filePath))
        {
            var fileFormatStrategy = GetFileFormatStrategy(filePath);
            return (filePath, fileFormatStrategy);
        }

        return (string.Empty, null);
    }

    // Prompt for file path
    private string GetFilePath()
    {
        Console.WriteLine("Enter the file path to proceed");
        Console.Write("> ");

        return Console.ReadLine() ?? string.Empty;
    }

    // Get file format strategy based on file extension
    private IFileFormatStrategy GetFileFormatStrategy(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        return extension switch
        {
            ".json" => new JsonFileFormatStrategy(),
            ".html" => new HtmlFileFormatStrategy(),
            _ => throw new InvalidOperationException("Unsupported file format")
        };
    }
}
