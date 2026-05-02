self.addEventListener('install', event => {
    event.waitUntil(
        caches.open('static-v2').then(cache => {
            return cache.addAll([
                '/css/site.css',
                '/js/site.js',
                '/manifest.webmanifest',
                '/icons/icon-192.png',
                '/icons/icon-512.png'
            ]);
        })
    );
});

self.addEventListener('fetch', event => {
    if (event.request.mode === 'navigate') {
        event.respondWith(
            fetch(event.request).catch(() => caches.match('/Home/Offline'))
        );
    } else {
        event.respondWith(
            caches.match(event.request).then(r => r || fetch(event.request))
        );
    }
});
