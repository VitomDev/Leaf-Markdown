namespace LeafMarkdown;

internal static class WebAssets
{
    private static readonly Dictionary<string, (byte[] Data, string Mime)> Cache = new();

    public static (byte[] Data, string Mime)? Get(string path)
    {
        if (Cache.TryGetValue(path, out var hit)) return hit;

        using var stream = typeof(WebAssets).Assembly.GetManifestResourceStream("web" + path);
        if (stream == null) return null;

        var data = new byte[stream.Length];
        stream.ReadExactly(data);
        string mime = Path.GetExtension(path) switch
        {
            ".html" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "text/javascript; charset=utf-8",
            _ => "application/octet-stream",
        };
        return Cache[path] = (data, mime);
    }
}
