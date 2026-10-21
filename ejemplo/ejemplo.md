---
title: Esto es front matter y no debe verse
---

# MdView — documento de prueba

Texto con **negrita**, *cursiva*, ***ambas***, ~~tachado~~ y `código en línea`. Acentos y UTF-8: áéíóú ñ ¿? ¡! — “comillas” 😀 中文.

## Listas

- Elemento uno
- Elemento dos
  - Anidado
    1. Ordenado anidado
    2. Otro

1. Primero
2. Segundo
3. Tercero

### Tareas

- [x] Renderizar Markdown
- [x] Modo oscuro
- [ ] Editar (nunca)

## Tabla

| Lenguaje | Tipado   | Año |
|:---------|:--------:|----:|
| C#       | Estático | 2000 |
| Python   | Dinámico | 1991 |
| Rust     | Estático | 2015 |

## Código

```csharp
public static int Suma(int a, int b)
{
    // Comentario
    return a + b;
}
```

```python
def saludo(nombre: str) -> str:
    return f"Hola, {nombre}"
```

```js
const x = [1, 2, 3].map(n => n * 2);
console.log(x);
```

```
Bloque sin lenguaje
```

## Cita

> Una cita de ejemplo.
> > Con una cita anidada.

> [!NOTE]
> Alerta estilo GitHub.

> [!WARNING]
> Otra alerta.

---

## Enlaces

- Externo: [GitHub](https://github.com)
- Autoenlace: https://example.com
- Ancla interna: [ir a Tabla](#tabla)
- Otro documento: [segundo.md](segundo.md)
- Nota al pie[^1]

## Imágenes

Local relativa:

![logo local](img/logo.png)

Remota:

![remota](https://upload.wikimedia.org/wikipedia/commons/thumb/4/48/Markdown-mark.svg/120px-Markdown-mark.svg.png)

#### H4
##### H5
###### H6

## HTML incrustado (debe ser inofensivo)

<p style="color: teal">Párrafo HTML con estilo permitido.</p>
<script>document.body.innerHTML = 'HACKEADO'</script>
<img src="x" onerror="document.body.innerHTML='HACKEADO onerror'">
<a href="javascript:alert(1)">enlace javascript (no hace nada)</a>
<iframe src="https://example.com"></iframe>
<meta http-equiv="refresh" content="0;url=https://example.com">
<img src="\\servidor\recurso\x.png" alt="ruta UNC bloqueada">

[^1]: Texto de la nota al pie.
