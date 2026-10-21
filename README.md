# MdView

A minimal Markdown viewer for Windows 10/11. It opens and displays `.md` files, and nothing else: no editing,
no sidebars, no tabs.

It starts almost instantly, renders documents with GitHub's style (tables, syntax-highlighted code, task lists,
images, alerts, footnotes) and follows the Windows light or dark theme.

## Download

From [Releases](https://github.com/VitomDev/MdView/releases/latest):

- **`MdView-Setup-1.0.0.exe`**: installer. No admin rights needed. It adds MdView to the Start menu and
  Windows Search, associates it with `.md` files, and can be uninstalled from *Settings > Apps*.
- **`MdView.exe`**: portable version, a single file with no installation.

## Usage

- **Open:** double-click a `.md` file, press `Ctrl+O`, drop the file onto the window, or run `MdView.exe "file.md"`.
- **Zoom:** `Ctrl` + mouse wheel (`Ctrl+0` resets it).
- **Find in document:** `Ctrl+F`.
- **Reload:** automatic when the file is saved by another program, keeping your scroll position. Also `F5`.
- **Links:** external links open in your default browser; links to other `.md` files open in the same window.

## Default app for `.md`

The installer sets this up. If Windows still opens `.md` files with another program, right-click a `.md` file,
choose *Open with*, *Choose another app*, **MdView**, *Always*.

With the portable version, put `MdView.exe` in its final folder and run once:

```bash
MdView.exe --register
```

This adds MdView to *Open with*, the Start menu and *Settings > Default apps* (current user only, no admin
rights). To undo it: `MdView.exe --unregister`.

## Requirements

- **Microsoft Edge WebView2 Runtime**: included with up-to-date Windows 10/11.
- **[.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)** (x64). The installer warns
  you if it's missing.

## Security

Markdown files are treated as untrusted content:

- Embedded HTML is sanitized (`script`, `iframe`, `object`, `form`, `meta`, `on*` attributes, `javascript:`
  links), and a Content-Security-Policy blocks every script except the viewer's own.
- The viewer never navigates: `http`, `https` and `mailto` links open in the default browser, and local files
  are never executed.
- Only local image files referenced by the document are served, and network paths (`\\server\...`) are blocked.
- No DevTools, downloads, pop-ups, permissions or autofill.

## Building

Requires the .NET 10 SDK. `build.cmd` puts the executable in `dist\MdView.exe` and, if
[Inno Setup 6](https://jrsoftware.org/isinfo.php) is installed, the installer in `installer\Output\`.

```
src\MdView\
  Program.cs           entry point and arguments (--register / --unregister / file)
  MainForm.cs          window, WebView2, opening files, live reload, links
  MarkdownRenderer.cs  file reading and Markdig
  LocalFiles.cs        safe path resolution
  Theme.cs             light/dark title bar
  FileAssociation.cs   --register / --unregister
  WebAssets.cs         embedded web resources
  Web\                 viewer page, GitHub CSS and highlight.js
installer\MdView.iss   installer script
sample\                test documents covering every supported feature
```

## License

[MIT](LICENSE). MdView uses [Markdig](https://github.com/xoofx/markdig), [highlight.js](https://highlightjs.org)
and [github-markdown-css](https://github.com/sindresorhus/github-markdown-css); their licenses are in
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
