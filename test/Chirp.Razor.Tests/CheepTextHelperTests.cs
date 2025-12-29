using Chirp.Web.Helpers;
using Microsoft.AspNetCore.Html;

namespace Chirp.Razor.Tests;

public class CheepTextHelperTests
{
    private static string HtmlContentToString(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
        return writer.ToString();
    }

    [Fact]
    public void RenderWithHashtags_ConvertsHashtagsToLinks()
    {
        // Arrange
        var text = "This is a #test message";

        // Act
        var result = CheepTextHelper.RenderWithHashtags(text);
        var html = HtmlContentToString(result);

        // Assert
        Assert.Contains("<a href=\"/hashtag/test\">#test</a>", html);
        Assert.Contains("This is a ", html);
        Assert.Contains(" message", html);
    }

    [Fact]
    public void RenderWithHashtags_HandlesMultipleHashtags()
    {
        // Arrange
        var text = "This is #hashtag1 and #hashtag2";

        // Act
        var result = CheepTextHelper.RenderWithHashtags(text);
        var html = HtmlContentToString(result);

        // Assert
        Assert.Contains("<a href=\"/hashtag/hashtag1\">#hashtag1</a>", html);
        Assert.Contains("<a href=\"/hashtag/hashtag2\">#hashtag2</a>", html);
    }

    [Fact]
    public void RenderWithHashtags_HandlesTextWithoutHashtags()
    {
        // Arrange
        var text = "This is a message without hashtags";

        // Act
        var result = CheepTextHelper.RenderWithHashtags(text);
        var html = HtmlContentToString(result);

        // Assert
        Assert.Equal("This is a message without hashtags", html);
        Assert.DoesNotContain("<a", html);
    }

    [Fact]
    public void RenderWithHashtags_ReturnsEmpty_WhenTextIsEmpty()
    {
        // Arrange
        var text = "";

        // Act
        var result = CheepTextHelper.RenderWithHashtags(text);
        var html = HtmlContentToString(result);

        // Assert
        Assert.Equal("", html);
    }

    [Fact]
    public void RenderWithHashtags_ReturnsEmpty_WhenTextIsNull()
    {
        // Arrange
        string? text = null;

        // Act
        var result = CheepTextHelper.RenderWithHashtags(text!);
        var html = HtmlContentToString(result);

        // Assert
        Assert.Equal("", html);
    }

    [Fact]
    public void RenderWithHashtags_ReturnsEmpty_WhenTextIsWhitespace()
    {
        // Arrange
        var text = "   ";

        // Act
        var result = CheepTextHelper.RenderWithHashtags(text);
        var html = HtmlContentToString(result);

        // Assert
        Assert.Equal("", html);
    }

    [Fact]
    public void RenderWithHashtags_HandlesHashtagAtStart()
    {
        // Arrange
        var text = "#hashtag at the start";

        // Act
        var result = CheepTextHelper.RenderWithHashtags(text);
        var html = HtmlContentToString(result);

        // Assert
        Assert.Contains("<a href=\"/hashtag/hashtag\">#hashtag</a>", html);
        Assert.Contains(" at the start", html);
    }

    [Fact]
    public void RenderWithHashtags_HandlesHashtagAtEnd()
    {
        // Arrange
        var text = "Message ending with #hashtag";

        // Act
        var result = CheepTextHelper.RenderWithHashtags(text);
        var html = HtmlContentToString(result);

        // Assert
        Assert.Contains("Message ending with ", html);
        Assert.Contains("<a href=\"/hashtag/hashtag\">#hashtag</a>", html);
    }

    [Fact]
    public void RenderWithHashtags_HandlesConsecutiveHashtags()
    {
        // Arrange
        var text = "#hashtag1#hashtag2";

        // Act
        var result = CheepTextHelper.RenderWithHashtags(text);
        var html = HtmlContentToString(result);

        // Assert
        Assert.Contains("<a href=\"/hashtag/hashtag1\">#hashtag1</a>", html);
        Assert.Contains("<a href=\"/hashtag/hashtag2\">#hashtag2</a>", html);
    }

    [Fact]
    public void RenderWithHashtags_HandlesAlphanumericHashtags()
    {
        // Arrange
        var text = "Test #test123 and #abc456";

        // Act
        var result = CheepTextHelper.RenderWithHashtags(text);
        var html = HtmlContentToString(result);

        // Assert
        Assert.Contains("<a href=\"/hashtag/test123\">#test123</a>", html);
        Assert.Contains("<a href=\"/hashtag/abc456\">#abc456</a>", html);
    }

    [Fact]
    public void RenderWithHashtags_IgnoresHashtagWithSpecialCharacters()
    {
        // Arrange
        var text = "Test #test-tag and #valid";

        // Act
        var result = CheepTextHelper.RenderWithHashtags(text);
        var html = HtmlContentToString(result);

        // Assert
        // Should only match alphanumeric portion before the hyphen
        Assert.Contains("<a href=\"/hashtag/test\">#test</a>", html);
        Assert.Contains("-tag", html);
        Assert.Contains("<a href=\"/hashtag/valid\">#valid</a>", html);
    }

    [Fact]
    public void RenderWithHashtags_PreservesTextFormatting()
    {
        // Arrange
        var text = "Start #hashtag middle text #another end";

        // Act
        var result = CheepTextHelper.RenderWithHashtags(text);
        var html = HtmlContentToString(result);

        // Assert
        Assert.Contains("Start ", html);
        Assert.Contains(" middle text ", html);
        Assert.Contains(" end", html);
        Assert.Contains("<a href=\"/hashtag/hashtag\">#hashtag</a>", html);
        Assert.Contains("<a href=\"/hashtag/another\">#another</a>", html);
    }
}
