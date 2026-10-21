using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;

namespace MdView;

internal sealed class MainForm : Form
{
    // Not .local: Windows resolves that TLD over mDNS, which delays every page load by ~2 s.
    private const string Origin = "https://mdview.example";
    private const string AppName = "MdView";

    private const string Csp =
        "default-src 'none'; " +
        "script-src " + Origin + "/highlight.min.js " + Origin + "/app.js; " +
        "style-src " + Origin + " 'unsafe-inline'; " +
        "img-src " + Origin + " https: http: data:; " +
        "base-uri 'none'; form-action 'none'; frame-ancestors 'none'";

    private CoreWebView2Controller? _controller;
    private CoreWebView2? _web;
    private bool _pageReady;

    private string? _path;
    private FileSystemWatcher? _watcher;
    private readonly System.Windows.Forms.Timer _reloadTimer = new() { Interval = 150 };
    private int _reloadRetries;

    public MainForm(string? path)
    {
        Text = AppName;
        Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath!);
        StartPosition = FormStartPosition.CenterScreen;
        AllowDrop = true;

        float scale = DeviceDpi / 96f;
        var work = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 900);
        Size = new Size(Math.Min((int)(980 * scale), work.Width), Math.Min((int)(900 * scale), work.Height - 40));

        _reloadTimer.Tick += (_, _) => { _reloadTimer.Stop(); SendDocument(reset: false); };
        Microsoft.Win32.SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

        if (!string.IsNullOrWhiteSpace(path)) SetDocument(Path.GetFullPath(path));
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        Theme.Apply(this);
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await InitWebViewAsync();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (_controller == null) return;
        bool minimized = WindowState == FormWindowState.Minimized;
        _controller.IsVisible = !minimized;
        if (_web != null)
            _web.MemoryUsageTargetLevel = minimized ? CoreWebView2MemoryUsageTargetLevel.Low : CoreWebView2MemoryUsageTargetLevel.Normal;
        if (!minimized) _controller.Bounds = ClientRectangle;
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        _controller?.MoveFocus(CoreWebView2MoveFocusReason.Programmatic);
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_MOVE = 0x0003;
        base.WndProc(ref m);
        if (m.Msg == WM_MOVE) _controller?.NotifyParentWindowPositionChanged();
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
        base.OnDragEnter(e);
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true) e.Effect = DragDropEffects.Copy;
    }

    protected override void OnDragDrop(DragEventArgs e)
    {
        base.OnDragDrop(e);
        if (e.Data?.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files) OpenFile(files[0]);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Microsoft.Win32.SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            _watcher?.Dispose();
            _reloadTimer.Dispose();
            _controller?.Close();
        }
        base.Dispose(disposing);
    }

    private void OnUserPreferenceChanged(object? sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
    {
        if (e.Category != Microsoft.Win32.UserPreferenceCategory.General || !IsHandleCreated) return;
        BeginInvoke(() =>
        {
            Theme.Apply(this);
            if (_controller != null) _controller.DefaultBackgroundColor = BackColor;
        });
    }

    private async Task InitWebViewAsync()
    {
        try
        {
            string dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);
            // Software rendering is plenty for text and saves ~70 MB (no GPU process memory).
            var options = new CoreWebView2EnvironmentOptions("--disable-gpu");
            var env = await CoreWebView2Environment.CreateAsync(null, dataDir, options);
            _controller = await env.CreateCoreWebView2ControllerAsync(Handle);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show(this,
                "No se encontró Microsoft Edge WebView2 Runtime.\n\nDescárgalo desde:\nhttps://developer.microsoft.com/microsoft-edge/webview2/",
                AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "No se pudo iniciar el visor:\n" + ex.Message, AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }

        _controller.DefaultBackgroundColor = BackColor;
        _controller.Bounds = ClientRectangle;
        _web = _controller.CoreWebView2;

        var s = _web.Settings;
        s.AreDevToolsEnabled = false;
        s.AreHostObjectsAllowed = false;
        s.AreDefaultScriptDialogsEnabled = false;
        s.IsStatusBarEnabled = false;
        s.IsZoomControlEnabled = true;
        s.IsGeneralAutofillEnabled = false;
        s.IsPasswordAutosaveEnabled = false;
        s.IsSwipeNavigationEnabled = false;

        _web.AddWebResourceRequestedFilter(Origin + "/*", CoreWebView2WebResourceContext.All);
        _web.WebResourceRequested += OnWebResourceRequested;
        _web.NavigationStarting += OnNavigationStarting;
        _web.NewWindowRequested += (_, e) => e.Handled = true;
        _web.DownloadStarting += (_, e) => e.Cancel = true;
        _web.PermissionRequested += (_, e) => e.State = CoreWebView2PermissionState.Deny;
        _web.WebMessageReceived += OnWebMessageReceived;
        _web.ContextMenuRequested += OnContextMenuRequested;
        _web.ProcessFailed += (_, _) => { _pageReady = false; _web?.Navigate(Origin + "/"); };
        _controller.AcceleratorKeyPressed += OnAcceleratorKeyPressed;

        _web.Navigate(Origin + "/");
        _controller.MoveFocus(CoreWebView2MoveFocusReason.Programmatic);
    }

    // Everything under Origin is served from here: the embedded viewer page, and /local?p=...
    // for images referenced by the document (image files only).
    private void OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
        var uri = new Uri(e.Request.Uri);
        string path = uri.AbsolutePath;
        string headers = "X-Content-Type-Options: nosniff\r\nCache-Control: no-cache\r\n";

        if (path == "/local")
        {
            string? file = _path == null ? null : LocalFiles.Resolve(QueryValue(uri.Query, "p") ?? "", _path);
            string? mime = file == null ? null : LocalFiles.ImageMimeType(file);
            byte[]? data = null;
            if (mime != null)
            {
                try { data = File.ReadAllBytes(file!); }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
            }
            e.Response = data == null
                ? _web!.Environment.CreateWebResourceResponse(null, 404, "Not Found", headers)
                : _web!.Environment.CreateWebResourceResponse(new MemoryStream(data), 200, "OK", headers + "Content-Type: " + mime);
            return;
        }

        var asset = WebAssets.Get(path == "/" ? "/index.html" : path);
        if (asset == null)
        {
            e.Response = _web!.Environment.CreateWebResourceResponse(null, 404, "Not Found", headers);
            return;
        }
        string extra = "Content-Type: " + asset.Value.Mime;
        if (asset.Value.Mime.StartsWith("text/html", StringComparison.Ordinal)) extra += "\r\nContent-Security-Policy: " + Csp;
        e.Response = _web!.Environment.CreateWebResourceResponse(new MemoryStream(asset.Value.Data), 200, "OK", headers + extra);
    }

    private static string? QueryValue(string query, string key)
    {
        foreach (var part in query.TrimStart('?').Split('&'))
        {
            int eq = part.IndexOf('=');
            if (eq > 0 && part[..eq] == key) return WebUtility.UrlDecode(part[(eq + 1)..]);
        }
        return null;
    }

    // Only the viewer page itself may load. A file dropped onto the WebView shows up here as a file: navigation.
    private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (e.Uri == Origin + "/") return;
        e.Cancel = true;
        if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri) && uri.IsFile)
            BeginInvoke(() => OpenFile(uri.LocalPath));
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        if (!e.Source.StartsWith(Origin + "/", StringComparison.Ordinal)) return;
        try
        {
            using var json = JsonDocument.Parse(e.WebMessageAsJson);
            var root = json.RootElement;
            switch (root.GetProperty("type").GetString())
            {
                case "ready":
                    _pageReady = true;
                    SendDocument(reset: true);
                    break;
                case "link":
                    HandleLink(root.GetProperty("href").GetString() ?? "");
                    break;
            }
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException) { }
    }

    private void OnContextMenuRequested(object? sender, CoreWebView2ContextMenuRequestedEventArgs e)
    {
        string[] keep = ["copy", "selectAll", "copyLinkLocation", "copyImage", "copyImageLocation"];
        var items = e.MenuItems;
        for (int i = items.Count - 1; i >= 0; i--)
            if (!keep.Contains(items[i].Name)) items.RemoveAt(i);
        if (items.Count == 0) e.Handled = true;
    }

    private void OnAcceleratorKeyPressed(object? sender, CoreWebView2AcceleratorKeyPressedEventArgs e)
    {
        if (e.KeyEventKind != CoreWebView2KeyEventKind.KeyDown) return;
        var key = (Keys)e.VirtualKey;
        bool ctrl = (ModifierKeys & Keys.Control) != 0;

        if (ctrl && key == Keys.O) { e.Handled = true; BeginInvoke(ShowOpenDialog); }
        else if (key == Keys.F5 || (ctrl && key == Keys.R)) { e.Handled = true; BeginInvoke(() => SendDocument(reset: false)); }
        else if (ctrl && key is Keys.S or Keys.N or Keys.T or Keys.U or Keys.J or Keys.H or Keys.D) e.Handled = true;
        else if (key == Keys.BrowserBack || key == Keys.BrowserForward) e.Handled = true;
    }

    private void ShowOpenDialog()
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Abrir Markdown",
            Filter = "Markdown (*.md;*.markdown;*.mdown;*.mkd)|*.md;*.markdown;*.mdown;*.mkd;*.mkdn;*.mdwn;*.mdtxt;*.mdtext|Todos los archivos (*.*)|*.*",
            InitialDirectory = _path != null ? Path.GetDirectoryName(_path) : null,
        };
        if (dlg.ShowDialog(this) == DialogResult.OK) OpenFile(dlg.FileName);
    }

    private void OpenFile(string path)
    {
        SetDocument(Path.GetFullPath(path));
        SendDocument(reset: true);
    }

    private void SetDocument(string path)
    {
        _path = path;
        Text = Path.GetFileName(path) + " - " + AppName;
        Watch(path);
    }

    private void HandleLink(string href)
    {
        if (href == "mdview:open") { ShowOpenDialog(); return; }

        if (Uri.TryCreate(href, UriKind.Absolute, out var uri) && !uri.IsFile)
        {
            if (uri.Scheme is "http" or "https" or "mailto")
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
            return;
        }

        if (_path == null) return;
        string? target = LocalFiles.Resolve(href, _path);
        if (target != null && LocalFiles.IsMarkdown(target) && File.Exists(target)) OpenFile(target);
    }

    private void SendDocument(bool reset)
    {
        if (!_pageReady || _web == null) return;

        string html;
        if (_path == null)
        {
            html = "<div class=\"empty\"><p>Arrastra un archivo Markdown aquí<br>o <a href=\"mdview:open\">ábrelo</a> con <kbd>Ctrl</kbd>+<kbd>O</kbd></p></div>";
        }
        else
        {
            try
            {
                html = MarkdownRenderer.ToHtml(MarkdownRenderer.ReadText(_path));
                _reloadRetries = 0;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                if (!reset)
                {
                    // An external editor may still be writing or replacing the file; keep the old content and retry.
                    if (++_reloadRetries <= 10) _reloadTimer.Start();
                    return;
                }
                string reason = ex is FileNotFoundException or DirectoryNotFoundException
                    ? "El archivo no existe."
                    : ex is UnauthorizedAccessException ? "No hay permiso para leer el archivo." : ex.Message;
                html = "<div class=\"empty\"><p class=\"error\">No se pudo abrir <b>" + WebUtility.HtmlEncode(Path.GetFileName(_path)) +
                       "</b><br>" + WebUtility.HtmlEncode(reason) + "</p></div>";
            }
        }

        string message = "{\"reset\":" + (reset ? "true" : "false") + ",\"html\":" + JsonSerializer.Serialize(html) + "}";
        _web.PostWebMessageAsString(message);
    }

    private void Watch(string path)
    {
        _watcher?.Dispose();
        _watcher = null;
        try
        {
            var w = new FileSystemWatcher(Path.GetDirectoryName(path)!, Path.GetFileName(path))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.CreationTime,
                SynchronizingObject = this,
            };
            w.Changed += (_, _) => ScheduleReload();
            w.Created += (_, _) => ScheduleReload();
            w.Renamed += (_, e) => { if (string.Equals(e.FullPath, _path, StringComparison.OrdinalIgnoreCase)) ScheduleReload(); };
            w.EnableRaisingEvents = true;
            _watcher = w;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException) { }
    }

    private void ScheduleReload()
    {
        _reloadRetries = 0;
        _reloadTimer.Stop();
        _reloadTimer.Start();
    }
}
