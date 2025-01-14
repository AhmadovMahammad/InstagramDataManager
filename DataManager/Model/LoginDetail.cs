namespace DataManager.Model;
public class LoginDetail
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public static LoginDetail Empty => new() { UserName = string.Empty, Password = string.Empty };

    public override bool Equals(object? obj)
    {
        if (obj is not LoginDetail other) return false;
        return UserName == other.UserName && Password == other.Password;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UserName, Password);
    }

    public static bool operator ==(LoginDetail left, LoginDetail right) => Equals(left, right);
    public static bool operator !=(LoginDetail left, LoginDetail right) => !Equals(left, right);
}
