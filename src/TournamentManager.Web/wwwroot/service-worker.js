const cacheName = 'tournament-manager-v2';
const staticAssets = ['/css/site.css', '/manifest.webmanifest'];

self.addEventListener('install', event => {
    self.skipWaiting();
    event.waitUntil(caches.open(cacheName).then(cache => cache.addAll(staticAssets)));
});
self.addEventListener('fetch', event => {

    self.addEventListener('activate', event => {
        event.waitUntil(
            caches.keys()
                .then(keys => Promise.all(keys.filter(key => key !== cacheName).map(key => caches.delete(key))))
                .then(() => self.clients.claim())
        );
    });

    if (event.request.method !== 'GET') return;

    if (event.request.mode === 'navigate') {
        event.respondWith(fetch(event.request));
        return;
    }

    event.respondWith(caches.match(event.request).then(response => response || fetch(event.request)));
});
