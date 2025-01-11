using DataManager.DesignPattern.ChainOfResponsibility;
using DataManager.DesignPattern.Strategy;
using DataManager.Helper.Extension;

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
        // Prompt for file path
        string filePath = "Enter the file path to proceed".GetInput();

        if (_validationChain.Handle(filePath))
        {
            var fileFormatStrategy = GetFileFormatStrategy(filePath);
            return (filePath, fileFormatStrategy);
        }

        return (string.Empty, null);
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
