namespace DataManager.Model.JsonModel;
public class Credential
{
    public string Username { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string EncryptedPassword { get; set; } = string.Empty;
}
