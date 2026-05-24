namespace Nexus.Data.Entities;

public class UserSessionEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Device { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Expiry { get; set; } = string.Empty;
    public bool Current { get; set; }

    public UserEntity? User { get; set; }
}
