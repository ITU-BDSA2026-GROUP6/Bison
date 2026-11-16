namespace Bison.Razor.Tests;

using Bison.Razor.Models;

public class PictureTests
{
    [Theory]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, "image/jpeg")]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D }, "image/png")]
    [InlineData(new byte[] { 0x3C, 0x73, 0x76, 0x67 }, null)]
    [InlineData(new byte[] { }, null)]
    public void DetectContentType_OnlyAcceptsJpegAndPng(byte[] data, string? expected)
    {
        Assert.Equal(expected, Picture.DetectContentType(data));
    }
}
