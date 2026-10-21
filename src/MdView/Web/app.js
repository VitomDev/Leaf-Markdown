// Document HTML is untrusted. It's sanitized here, and the page's CSP blocks any script anyway.
(() => {
  'use strict';
  const root = document.getElementById('content');
  const host = window.chrome.webview;

  const BLOCKED = new Set(['SCRIPT', 'IFRAME', 'FRAME', 'FRAMESET', 'OBJECT', 'EMBED', 'APPLET',
    'META', 'BASE', 'LINK', 'FORM', 'NOSCRIPT', 'PORTAL', 'AUDIO', 'VIDEO', 'SOURCE', 'TRACK']);
  const URL_ATTRS = new Set(['href', 'src', 'xlink:href', 'action', 'formaction', 'poster', 'background', 'data', 'srcset']);
  const SAFE_URL = /^(https?:|mailto:|mdview:open$|data:image\/(png|jpe?g|gif|webp|bmp)[;,]|#|[^:]*$)/i;

  function sanitize(fragment) {
    const walker = document.createTreeWalker(fragment, NodeFilter.SHOW_ELEMENT);
    const remove = [];
    for (let el = walker.nextNode(); el; el = walker.nextNode()) {
      if (BLOCKED.has(el.tagName.toUpperCase())) { remove.push(el); continue; }
      for (const attr of [...el.attributes]) {
        const name = attr.name.toLowerCase();
        if (name.startsWith('on') || name === 'srcset' || name === 'srcdoc' || name === 'formaction') {
          el.removeAttribute(attr.name);
        } else if (URL_ATTRS.has(name) && !SAFE_URL.test(attr.value.trim())) {
          if (/^([a-z]:[\\/]|file:)/i.test(attr.value.trim())) continue;
          el.removeAttribute(attr.name);
        }
      }
      if (el.tagName === 'INPUT') el.setAttribute('disabled', '');
      if (el.tagName === 'IMG') fixImage(el);
    }
    remove.forEach(el => el.remove());
  }

  // The browser can't resolve paths like ../img.png or C:\x.png against the document's folder,
  // so the raw path is handed to the host, which resolves it and serves image files only.
  function fixImage(img) {
    const src = (img.getAttribute('src') || '').trim();
    if (!src || /^(https?:|data:)/i.test(src)) return;
    img.setAttribute('src', '/local?p=' + encodeURIComponent(src));
  }

  function render(msg) {
    const y = window.scrollY;
    const tpl = document.createElement('template');
    tpl.innerHTML = msg.html;
    sanitize(tpl.content);
    root.replaceChildren(tpl.content);
    root.querySelectorAll('pre code[class*="language-"]').forEach(el => {
      try { hljs.highlightElement(el); } catch { }
    });
    window.scrollTo(0, msg.reset ? 0 : y);
  }

  // The page never navigates: in-page anchors scroll here, every other link goes to the host.
  function onLinkClick(e) {
    const a = e.target.closest && e.target.closest('a[href]');
    if (!a) return;
    const href = a.getAttribute('href');
    if (href.startsWith('#')) {
      e.preventDefault();
      const id = decodeURIComponent(href.slice(1));
      const target = document.getElementById(id) || document.getElementsByName(id)[0];
      if (target) target.scrollIntoView();
      return;
    }
    e.preventDefault();
    if (e.type === 'click' || e.button === 1) host.postMessage({ type: 'link', href });
  }

  if (window.hljs) hljs.configure({ ignoreUnescapedHTML: true });
  document.addEventListener('click', onLinkClick);
  document.addEventListener('auxclick', onLinkClick);
  host.addEventListener('message', e => render(JSON.parse(e.data)));
  host.postMessage({ type: 'ready' });
})();
