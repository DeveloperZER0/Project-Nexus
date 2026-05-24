namespace Nexus.Data.Entities;

public class PostHashtagEntity
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string Tag { get; set; } = string.Empty;

    public PostEntity? Post { get; set; }
}
