# MdView

Visor de Markdown minimalista para Windows 10/11. Solo abre y muestra archivos `.md`; no edita nada.

Se abre al instante, muestra el documento con el estilo de GitHub (tablas, código con resaltado, tareas,
imágenes, alertas, notas al pie) y sigue el modo claro u oscuro de Windows.

## Descarga

En [Releases](https://github.com/VitomDev/MdView/releases/latest):

- **`MdView-Setup-1.0.0.exe`**: instalador. No necesita permisos de administrador. Añade MdView al menú
  Inicio y al buscador de Windows, lo asocia a los archivos `.md` y se desinstala desde *Aplicaciones*.
- **`MdView.exe`**: versión portable, un único archivo sin instalación.

## Uso

- **Abrir:** doble clic en un `.md`, `Ctrl+O`, arrastrar el archivo sobre la ventana, o `MdView.exe "archivo.md"`.
- **Zoom:** `Ctrl` + rueda del ratón (`Ctrl+0` lo restablece).
- **Buscar en el documento:** `Ctrl+F`.
- **Recargar:** automático al guardar el archivo desde otro programa, conservando la posición. También `F5`.
- **Enlaces:** los externos se abren en tu navegador; los enlaces a otros `.md` se abren en la misma ventana.

## Aplicación predeterminada para `.md`

Con el instalador ya queda asociado. Si Windows sigue abriendo los `.md` con otro programa: clic derecho
sobre un `.md`, *Abrir con*, *Elegir otra aplicación*, **MdView**, *Siempre*.

Con la versión portable, deja `MdView.exe` en su carpeta definitiva y ejecuta una vez:

```bash
MdView.exe --register
```

Esto lo añade a *Abrir con*, al menú Inicio y a *Configuración > Aplicaciones predeterminadas* (solo para tu
usuario, sin permisos de administrador). Para deshacerlo: `MdView.exe --unregister`.

## Requisitos

- **Microsoft Edge WebView2 Runtime**: viene incluido en Windows 10/11 actualizados.
- **[.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)** (x64). El instalador
  avisa si falta.

## Seguridad

El Markdown se trata como contenido no confiable:

- El HTML incrustado se sanea (`script`, `iframe`, `object`, `form`, `meta`, atributos `on*`, enlaces
  `javascript:`) y una Content-Security-Policy impide ejecutar cualquier script que no sea el del visor.
- El visor nunca navega: los enlaces `http`, `https` y `mailto` se abren en el navegador predeterminado y
  nunca se ejecutan archivos locales.
- Solo se sirven archivos locales de imagen referenciados por el documento, y se bloquean las rutas de red
  (`\\servidor\...`).
- Sin DevTools, descargas, ventanas emergentes, permisos ni autocompletado.

## Compilar

Requiere el SDK de .NET 10. `build.cmd` deja el ejecutable en `dist\MdView.exe` y, si
[Inno Setup 6](https://jrsoftware.org/isinfo.php) está instalado, el instalador en `installer\Output\`.

```
src\MdView\
  Program.cs           entrada y argumentos (--register / --unregister / archivo)
  MainForm.cs          ventana, WebView2, apertura, recarga en vivo, enlaces
  MarkdownRenderer.cs  lectura del archivo y Markdig
  LocalFiles.cs        resolución segura de rutas
  Theme.cs             modo claro/oscuro de la barra de título
  FileAssociation.cs   --register / --unregister
  WebAssets.cs         recursos web incrustados
  Web\                 página del visor, CSS de GitHub y highlight.js
installer\MdView.iss   script del instalador
```

## Licencia

[MIT](LICENSE). Usa [Markdig](https://github.com/xoofx/markdig), [highlight.js](https://highlightjs.org) y
[github-markdown-css](https://github.com/sindresorhus/github-markdown-css); sus licencias están en
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
