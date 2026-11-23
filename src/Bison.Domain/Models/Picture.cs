namespace Bison.Domain.Models;

public class Picture
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47];

    public int pictureId { get; set; }
    public byte[] Data { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;

    public static string? DetectContentType(byte[] data)
    {
        if (data.Take(Jpeg.Length).SequenceEqual(Jpeg))
        {
            return ("image/jpeg");
        }
        return data.Take(Png.Length).SequenceEqual(Png) ? "image/png" : null;
    }
}