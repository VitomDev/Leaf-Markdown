namespace MdView;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        string? arg = args.Length > 0 ? args[0] : null;
        switch (arg?.ToLowerInvariant())
        {
            case "--register":
                FileAssociation.Register();
                return;
            case "--unregister":
                FileAssociation.Unregister();
                return;
        }

        Application.Run(new MainForm(arg));
    }
}
