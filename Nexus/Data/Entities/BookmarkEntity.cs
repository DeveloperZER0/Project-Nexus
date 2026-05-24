namespace Nexus.Data.Entities;

public class BookmarkEntity
{
    public int UserId { get; set; }
    public int PostId { get; set; }

    public UserEntity? User { get; set; }
    public PostEntity? Post { get; set; }
}
