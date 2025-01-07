using DataManager.Constant.Enums;
using DataManager.Model;
using System.ComponentModel;
using System.Reflection;

namespace DataManager.Constant;
public static class AppConstant
{
    public static readonly string ApplicationDataFolderPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "InstagramDataManager");
    public static readonly TimeSpan ImplicitWait = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan ExplicitWait = TimeSpan.FromSeconds(15);
    public static readonly int MaxRetryPerPost = 3;

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
