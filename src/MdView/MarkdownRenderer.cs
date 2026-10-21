using System.Text;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;

namespace MdView;

internal static class MarkdownRenderer
{
    private const long MaxFileSize = 64L * 1024 * 1024;

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseYamlFrontMatter()
        .UseEmphasisExtras()
        .UsePipeTables()
        .UseGridTables()
        .UseTaskLists()
        .UseAutoLinks()
        .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
        .UseFootnotes()
        .UseListExtras()
        .UseAlertBlocks()
        .Build();

    public static string ToHtml(string markdown) => Markdown.ToHtml(markdown, Pipeline);

    // Shared read so editors can keep saving while the file is open. Encoding: BOM if present,
    // else UTF-8, falling back to Windows-1252 for legacy files that aren't valid UTF-8.
    public static string ReadText(string path)
    {
        byte[] bytes;
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        {
            if (fs.Length > MaxFileSize)
                throw new IOException("El archivo es demasiado grande para mostrarlo.");
            using var ms = new MemoryStream((int)fs.Length);
            fs.CopyTo(ms);
            bytes = ms.ToArray();
        }

        ReadOnlySpan<byte> span = bytes;
        if (span.StartsWith(new byte[] { 0xEF, 0xBB, 0xBF })) return Encoding.UTF8.GetString(span[3..]);
        if (span.StartsWith(new byte[] { 0xFF, 0xFE })) return Encoding.Unicode.GetString(span[2..]);
        if (span.StartsWith(new byte[] { 0xFE, 0xFF })) return Encoding.BigEndianUnicode.GetString(span[2..]);

        try
        {
            return new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(span);
        }
        catch (DecoderFallbackException)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding(1252).GetString(span);
        }
    }
}
