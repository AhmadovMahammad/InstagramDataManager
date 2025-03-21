﻿using DataManager.Constant.Enums;
using DataManager.Helper.Extension;

namespace DataManager.DesignPattern.ChainOfResponsibility;
public class ArgumentNotEmptyHandler : AbstractHandler
{
    public override bool Handle(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            "Error: The provided file path is empty or contains only whitespace.".WriteMessage(MessageType.Error);
            "Hint: Make sure to provide a valid file path to a JSON or HTML file.".WriteMessage(MessageType.Info);
            return false;
        }

        return base.Handle(input);
    }
}