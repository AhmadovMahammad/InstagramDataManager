using DataManager.Constants.Enums;
using DataManager.DesignPatterns.ChainOfResponsibilityDP.Contracts;
using DataManager.DesignPatterns.ChainOfResponsibilityDP.Implementations;

namespace DataManager.Core;
public interface ICommandHandler
{
    void Handle(OperationType operationType);
}

public class CommandHandler : ICommandHandler // Routes command-line inputs to tasks
{
    public void Handle(OperationType operationType)
    {
        IHandler handler = new DefaultHandler()
            .SetNext(new ArgumentNotEmptyHandler())
            .SetNext(new FileExistsHandler())
            .SetNext(new FileExtensionHandler(["json", "html"]));

        if (!handler.Handle("filePath"))
        {
            Console.WriteLine("Invalid file.");
            return;
        }
    }
}