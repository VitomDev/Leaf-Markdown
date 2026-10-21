using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace LeafMarkdown;

internal static class Theme
{
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    public static bool IsDark()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        return key?.GetValue("AppsUseLightTheme") is int light && light == 0;
    }

    public static void Apply(Form form)
    {
        bool dark = IsDark();
        int value = dark ? 1 : 0;
        DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
        // Same backgrounds as github-markdown-css, so there's no white flash before the page loads.
        form.BackColor = dark ? Color.FromArgb(13, 17, 23) : Color.White;
    }
}
