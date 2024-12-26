namespace DataManager.Constants.Enums;

[Flags]
public enum OperationType
{
    FileBased = 1 << 0,
    SeleniumBased = 1 << 1,
    Hybrid = FileBased | SeleniumBased // Combines file and Selenium operations.
}
