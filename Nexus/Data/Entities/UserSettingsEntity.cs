namespace Nexus.Data.Entities;

public class UserSettingsEntity
{
    public int UserId { get; set; }
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public string PrivacyLevel { get; set; } = "Public";
    public string Theme { get; set; } = "Dark";
    public string Language { get; set; } = "pl-PL";

    public UserEntity? User { get; set; }
}
