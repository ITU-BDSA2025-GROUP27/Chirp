using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace Chirp.Web.Helpers;

// Written with assistance from Claude.
public static class CheepTextHelper
{
    /// <summary>
    /// Converts hashtags in cheep text to clickable links.
    /// </summary>
    public static IHtmlContent RenderWithHashtags(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return HtmlString.Empty;
        }

        var builder = new HtmlContentBuilder();
        var pattern = @"#([a-zA-Z0-9]+)";
        var lastIndex = 0;

        foreach (Match match in Regex.Matches(text, pattern))
        {
            // Add text before the hashtag
            if (match.Index > lastIndex)
            {
                var textBefore = text.Substring(lastIndex, match.Index - lastIndex);
                builder.Append(textBefore);
            }

            // Add hashtag as a clickable link
            var tag = match.Groups[1].Value;
            builder.AppendHtml($"<a href=\"/hashtag/{tag}\">#{tag}</a>");

            lastIndex = match.Index + match.Length;
        }

        // Add remaining text after the last hashtag
        if (lastIndex < text.Length)
        {
            var textAfter = text.Substring(lastIndex);
            builder.Append(textAfter);
        }

        return builder;
    }
}
