using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace LeafMarkdown;

// Per-user registration (no admin needed). Windows doesn't let an app make itself the default
// handler, so this only lists Leaf Markdown under "Open with" and Settings > Default apps for the user to pick.
internal static class FileAssociation
{
    private const string ProgId = "LeafMarkdown.Markdown";
    private const string CapabilitiesKey = @"Software\LeafMarkdown\Capabilities";
    private static readonly string[] Extensions = [".md", ".markdown", ".mdown", ".mkd", ".mkdn", ".mdwn"];

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

    public static void Register()
    {
        string exe = Environment.ProcessPath!;
        string command = $"\"{exe}\" \"%1\"";
        using var classes = Registry.CurrentUser.CreateSubKey(@"Software\Classes");

        using (var prog = classes.CreateSubKey(ProgId))
        {
            prog.SetValue("", "Markdown Document");
            using (var icon = prog.CreateSubKey("DefaultIcon")) icon.SetValue("", $"\"{exe}\",0");
            using (var cmd = prog.CreateSubKey(@"shell\open\command")) cmd.SetValue("", command);
        }

        using (var app = classes.CreateSubKey(@"Applications\LeafMarkdown.exe"))
        {
            app.SetValue("FriendlyAppName", "Leaf Markdown");
            using (var cmd = app.CreateSubKey(@"shell\open\command")) cmd.SetValue("", command);
            using var types = app.CreateSubKey("SupportedTypes");
            foreach (var ext in Extensions) types.SetValue(ext, "");
        }

        foreach (var ext in Extensions)
            using (var owp = classes.CreateSubKey(ext + @"\OpenWithProgids")) owp.SetValue(ProgId, "");

        using (var caps = Registry.CurrentUser.CreateSubKey(CapabilitiesKey))
        {
            caps.SetValue("ApplicationName", "Leaf Markdown");
            caps.SetValue("ApplicationDescription", "Minimal Markdown viewer");
            using var assoc = caps.CreateSubKey("FileAssociations");
            foreach (var ext in Extensions) assoc.SetValue(ext, ProgId);
        }
        using (var reg = Registry.CurrentUser.CreateSubKey(@"Software\RegisteredApplications"))
            reg.SetValue("LeafMarkdown", CapabilitiesKey);

        CreateStartMenuShortcut(exe);
        Notify();
        try { Process.Start(new ProcessStartInfo("ms-settings:defaultapps?registeredAppUser=LeafMarkdown") { UseShellExecute = true }); }
        catch { }

        MessageBox.Show(
            "Leaf Markdown is now registered for Markdown files and shows up in the Start menu and Windows Search.\n\n" +
            "In the Default apps window that just opened, choose Leaf Markdown for .md.\n" +
            "You can also right-click a .md file > Open with > Choose another app > Leaf Markdown > Always.\n\n" +
            "If you move LeafMarkdown.exe to another folder, run --register again.",
            "Leaf Markdown", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static void Unregister()
    {
        using var classes = Registry.CurrentUser.CreateSubKey(@"Software\Classes");
        classes.DeleteSubKeyTree(ProgId, false);
        classes.DeleteSubKeyTree(@"Applications\LeafMarkdown.exe", false);
        foreach (var ext in Extensions)
            using (var owp = classes.OpenSubKey(ext + @"\OpenWithProgids", true)) owp?.DeleteValue(ProgId, false);
        Registry.CurrentUser.DeleteSubKeyTree(@"Software\LeafMarkdown", false);
        using (var reg = Registry.CurrentUser.OpenSubKey(@"Software\RegisteredApplications", true)) reg?.DeleteValue("LeafMarkdown", false);
        File.Delete(ShortcutPath);

        Notify();
        MessageBox.Show("Leaf Markdown has been unregistered.", "Leaf Markdown", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static string ShortcutPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Leaf Markdown.lnk");

    // Windows Search only lists apps that have a Start menu shortcut.
    private static void CreateStartMenuShortcut(string exe)
    {
        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;
            dynamic shell = Activator.CreateInstance(shellType)!;
            dynamic link = shell.CreateShortcut(ShortcutPath);
            link.TargetPath = exe;
            link.WorkingDirectory = Path.GetDirectoryName(exe);
            link.IconLocation = exe + ",0";
            link.Description = "Minimal Markdown viewer";
            link.Save();
        }
        catch { }
    }

    private static void Notify()
    {
        const int SHCNE_ASSOCCHANGED = 0x08000000;
        SHChangeNotify(SHCNE_ASSOCCHANGED, 0, IntPtr.Zero, IntPtr.Zero);
    }
}
