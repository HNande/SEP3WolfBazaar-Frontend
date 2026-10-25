namespace BlazorApp.DTO;

public class PhotoDto
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string Filetype { get; set; }
    public byte[] Data { get; set; }
}