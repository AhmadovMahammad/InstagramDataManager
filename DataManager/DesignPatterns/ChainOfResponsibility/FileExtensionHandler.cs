﻿using DataManager.Constants.Enums;
using DataManager.Extensions;

namespace DataManager.DesignPatterns.ChainOfResponsibility;
public class FileExtensionHandler(string[] requiredExtensions) : AbstractHandler
{
    public override bool Handle(string input)
    {
        if (!requiredExtensions.Any(requiredExtension => input.EndsWith(requiredExtension, StringComparison.OrdinalIgnoreCase)))
        {
            $"Error: The file must have one of '{string.Join('-', requiredExtensions)}'extensions.".WriteMessage(MessageType.Error);
            $"Provided file: {input}".WriteMessage(MessageType.Info);
            return false;
        }

        return base.Handle(input);
    }
}