/*
 * Service worker: lets the app be installed to the home screen and keeps
 * recently viewed specifications readable when the network drops.
 * Data is always fetched fresh when online; the cache is only a fallback.
 */
const VERSION = 'v1';
const SHELL = `shell-${VERSION}`;
const ASSETS = `assets-${VERSION}`;
const DATA = `data-${VERSION}`;

self.addEventListener('install', (event) => {
  event.waitUntil(caches.open(SHELL).then((c) => c.addAll(['/', '/manifest.webmanifest', '/icon-192.png'])));
  self.skipWaiting();
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys()
      .then((keys) => Promise.all(keys.filter((k) => !k.endsWith(VERSION)).map((k) => caches.delete(k))))
      .then(() => self.clients.claim()),
  );
});

async function networkFirst(request, cacheName) {
  const cache = await caches.open(cacheName);
  try {
    const response = await fetch(request);
    if (response.ok) {
      cache.put(request, response.clone()).then(() => trim(cache, 300));
    }
    return response;
  } catch (err) {
    const cached = await cache.match(request);
    if (cached) return cached;
    throw err;
  }
}

async function trim(cache, max) {
  const keys = await cache.keys();
  for (let i = 0; i < keys.length - max; i++) await cache.delete(keys[i]);
}

async function cacheFirst(request, cacheName) {
  const cache = await caches.open(cacheName);
  const cached = await cache.match(request);
  if (cached) return cached;
  const response = await fetch(request);
  if (response.ok) cache.put(request, response.clone());
  return response;
}

// Public, read-only data that is safe to keep for offline reading.
const CACHEABLE_API = /^\/api\/(search|products(\/[^/]+(\/revisions)?)?|files\/[^/]+)$/;

self.addEventListener('fetch', (event) => {
  const { request } = event;
  if (request.method !== 'GET') return;
  const url = new URL(request.url);
  if (url.origin !== self.location.origin) return;

  if (request.mode === 'navigate') {
    event.respondWith(
      fetch(request).then((r) => {
        if (r.ok) {
          const copy = r.clone();
          caches.open(SHELL).then((c) => c.put('/', copy));
        }
        return r;
      }).catch(() => caches.match('/')),
    );
    return;
  }
  if (url.pathname.startsWith('/assets/') || url.pathname.startsWith('/api/files/')) {
    event.respondWith(cacheFirst(request, url.pathname.startsWith('/api/') ? DATA : ASSETS));
    return;
  }
  if (CACHEABLE_API.test(url.pathname) && !url.searchParams.has('status')) {
    event.respondWith(networkFirst(request, DATA));
  }
});
