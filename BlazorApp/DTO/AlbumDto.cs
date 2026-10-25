namespace BlazorApp.DTO;

public class AlbumDto
{
    public string Title { get; set; }
    public List<PhotoDto> Photos { get; set; } = new();
}