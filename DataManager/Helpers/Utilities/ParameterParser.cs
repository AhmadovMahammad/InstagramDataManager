namespace DataManager.Helpers.Utilities;
public static class ParameterParser
{
    public static T Parse<T>(this IDictionary<string, object> parameters, string key)
    {
        if (!parameters.TryGetValue(key, out var value))
        {
            throw new ArgumentException($"Missing parameter: {key}");
        }

        return value is T parsedValue
            ? parsedValue
            : throw new ArgumentException($"Invalid type for parameter: {key}. Expected {typeof(T).Name}, but got {value.GetType().Name}.");
    }
}
