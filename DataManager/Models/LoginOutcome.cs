﻿namespace DataManager.Models;
public record LoginOutcome
{
    public LoginOutcome(string condition, object? data = null)
    {
        Condition = condition;
        Data = data;
    }

    public string Condition { get; init; } = string.Empty;
    public object? Data { get; init; }

    public static LoginOutcome Empty = new LoginOutcome(string.Empty);
}
