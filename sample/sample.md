---
title: This is front matter and should not be shown
---

# MdView test document

Text with **bold**, *italic*, ***both***, ~~strikethrough~~ and `inline code`. Unicode: áéíóú ñ ü ß ç, “quotes”, 😀 中文.

## Lists

- Item one
- Item two
  - Nested
    1. Nested ordered
    2. Another

1. First
2. Second
3. Third

### Tasks

- [x] Render Markdown
- [x] Dark mode
- [ ] Edit files (never)

## Table

| Language | Typing   | Year |
|:---------|:--------:|-----:|
| C#       | Static   | 2000 |
| Python   | Dynamic  | 1991 |
| Rust     | Static   | 2015 |

## Code

```csharp
public static int Add(int a, int b)
{
    // Comment
    return a + b;
}
```

```python
def greet(name: str) -> str:
    return f"Hello, {name}"
```

```js
const x = [1, 2, 3].map(n => n * 2);
console.log(x);
```

```
Block without a language
```

## Quotes

> A sample quote.
> > With a nested quote.

> [!NOTE]
> GitHub-style alert.

> [!WARNING]
> Another alert.

---

## Links

- External: [GitHub](https://github.com)
- Autolink: https://example.com
- In-page anchor: [go to Table](#table)
- Another document: [second.md](second.md)
- Footnote[^1]

## Images

Local, relative path:

![local logo](img/logo.png)

Remote:

![remote](https://upload.wikimedia.org/wikipedia/commons/thumb/4/48/Markdown-mark.svg/120px-Markdown-mark.svg.png)

#### H4
##### H5
###### H6

## Embedded HTML (must be harmless)

<p style="color: teal">HTML paragraph with an allowed style.</p>
<script>document.body.innerHTML = 'HACKED'</script>
<img src="x" onerror="document.body.innerHTML='HACKED via onerror'">
<a href="javascript:alert(1)">javascript link (does nothing)</a>
<iframe src="https://example.com"></iframe>
<meta http-equiv="refresh" content="0;url=https://example.com">
<img src="\\server\share\x.png" alt="blocked UNC path">

[^1]: Footnote text.
