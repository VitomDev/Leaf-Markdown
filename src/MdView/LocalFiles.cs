namespace MdView;

internal static class LocalFiles
{
    private static readonly string[] MarkdownExtensions =
        [".md", ".markdown", ".mdown", ".mkd", ".mkdn", ".mdwn", ".mdtxt", ".mdtext"];

    private static readonly Dictionary<string, string> ImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
        [".bmp"] = "image/bmp",
        [".ico"] = "image/x-icon",
        [".avif"] = "image/avif",
        [".svg"] = "image/svg+xml",
    };

    public static bool IsMarkdown(string path) =>
        MarkdownExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);

    public static string? ImageMimeType(string path) =>
        ImageTypes.GetValueOrDefault(Path.GetExtension(path));

    // Network paths (\\server\share) are rejected unless they're inside the document's own folder:
    // otherwise a document could make Windows connect to an arbitrary SMB server and leak NTLM credentials.
    public static string? Resolve(string reference, string documentPath)
    {
        string baseDir = Path.GetDirectoryName(documentPath)!;
        string s = reference.Trim();

        try
        {
            if (s.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                if (!Uri.TryCreate(s, UriKind.Absolute, out var uri) || !uri.IsFile) return null;
                s = uri.LocalPath;
            }
            else
            {
                int cut = s.IndexOfAny(['?', '#']);
                if (cut >= 0) s = s[..cut];
                s = Uri.UnescapeDataString(s);
            }

            if (s.Length == 0) return null;

            string full = Path.IsPathFullyQualified(s)
                ? Path.GetFullPath(s)
                : Path.GetFullPath(Path.Combine(baseDir, s.TrimStart('/', '\\')));

            if (IsNetworkPath(full) && !IsInside(full, baseDir)) return null;
            return full;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or UriFormatException)
        {
            return null;
        }
    }

    private static bool IsNetworkPath(string path) =>
        path.StartsWith(@"\\", StringComparison.Ordinal) || path.StartsWith("//", StringComparison.Ordinal);

    private static bool IsInside(string path, string dir) =>
        path.StartsWith(Path.TrimEndingDirectorySeparator(dir) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
}
