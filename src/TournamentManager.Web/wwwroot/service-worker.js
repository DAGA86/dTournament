const cacheName = 'tournament-manager-v1';
const assets = ['/', '/css/site.css', '/manifest.webmanifest'];
self.addEventListener('install', event => {
    event.waitUntil(caches.open(cacheName).then(cache => cache.addAll(assets)));
});
self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') return;
    event.respondWith(caches.match(event.request).then(response => response || fetch(event.request)));
});
