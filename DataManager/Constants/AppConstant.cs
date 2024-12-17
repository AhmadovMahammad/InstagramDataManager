using DataManager.Constants.Enums;
using System.ComponentModel;
using System.Reflection;

namespace DataManager.Constants;
public static class AppConstant
{
    public readonly static Dictionary<int, (string action, string description)> AvailableOperations =
        Enum.GetValues<OperationType>()
        .ToDictionary(
            op => (int)op,
            op => (op.ToString(), GetEnumDescription(op))
        );

    private static string GetEnumDescription<TEnum>(TEnum enumValue) where TEnum : Enum
    {
        FieldInfo? fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
        if (fieldInfo is not null)
        {
            DescriptionAttribute? descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttribute?.Description ?? string.Empty;
        }

        return string.Empty;
    }
}
