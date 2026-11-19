const CACHE_NAME = "gmk360-pwa-v4";
const PRECACHE_ASSETS = [
  "/manifest.json",
  "/icons/icon-192.png",
  "/icons/icon-512.png",
  "/icons/apple-touch-icon.png"
];

self.addEventListener("install", event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(PRECACHE_ASSETS))
      .catch(() => Promise.resolve())
  );
  self.skipWaiting();
});

self.addEventListener("activate", event => {
  event.waitUntil(
    caches.keys()
      .then(keys => Promise.all(keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k))))
      .then(() => self.clients.claim())
  );
});

// Network-First: Canlı ERP verisi asla bayat önbellekten dönmez; yalnızca çevrimdışı static varlıklar için yedeklenir.
self.addEventListener("fetch", event => {
  if (event.request.method !== "GET") return;
  const reqUrl = new URL(event.request.url);
  if (reqUrl.origin !== self.location.origin) return;

  if (event.request.mode === "navigate") {
    event.respondWith(
      fetch(event.request).catch(() =>
        new Response(
          "<!DOCTYPE html><html lang='tr'><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'><title>GMK360 Çevrimdışı</title><style>body{font-family:system-ui,sans-serif;background:#0f172a;color:#fff;display:flex;align-items:center;justify-content:center;min-height:100vh;margin:0;text-align:center;padding:24px}.box{max-width:420px;background:#1e293b;padding:28px;border-radius:16px;border:1px solid #334155}button{margin-top:16px;background:#f97316;color:#fff;border:none;padding:10px 20px;border-radius:99px;font-weight:700;cursor:pointer}</style></head><body><div class='box'><h2>İnternet Bağlantısı Yok</h2><p>GMK360 canlı verileri için aktif internet bağlantısı gereklidir.</p><button onclick='location.reload()'>Tekrar Dene</button></div></body></html>",
          { status: 503, headers: { "Content-Type": "text/html; charset=utf-8" } }
        )
      )
    );
    return;
  }

  if (reqUrl.pathname.startsWith("/icons/") || reqUrl.pathname.startsWith("/css/") || reqUrl.pathname.startsWith("/js/") || reqUrl.pathname === "/manifest.json") {
    event.respondWith(
      fetch(event.request)
        .then(res => {
          if (res && res.ok) {
            const copy = res.clone();
            caches.open(CACHE_NAME).then(c => c.put(event.request, copy));
          }
          return res;
        })
        .catch(() => caches.match(event.request))
    );
  }
});
