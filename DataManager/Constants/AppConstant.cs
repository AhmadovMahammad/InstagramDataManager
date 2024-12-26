using DataManager.Constants.Enums;
using DataManager.Models;
using System.ComponentModel;
using System.Reflection;

namespace DataManager.Constants;
public static class AppConstant
{
    public static readonly TimeSpan ImplicitWait = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan ExplicitWait = TimeSpan.FromSeconds(15);
    public static readonly int MaxRetryPerPost = 3;
    public static readonly int MaxPostStack = 5;

    public static IEnumerable<MenuModel> GetAvailableOperations()
    {
        return Enum.GetValues<CommandType>()
            .Select(commandType => new MenuModel
            {
                Key = (int)commandType,
                Action = commandType.ToString(),
                Description = GetEnumDescription(commandType)
            });
    }

    private static string GetEnumDescription<TEnum>(TEnum enumValue) where TEnum : Enum
    {
        FieldInfo? fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
        if (fieldInfo != null)
        {
            DescriptionAttribute? descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttribute?.Description ?? string.Empty;
        }

        return string.Empty;
    }
}
