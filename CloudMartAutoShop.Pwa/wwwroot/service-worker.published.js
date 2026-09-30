// CloudMart Auto Shop release 1.0.16
self.importScripts('./service-worker-assets.js');

self.addEventListener('message', event => {
    if (event.data?.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});

const cacheNamePrefix = 'cloudmart-autoshop-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;

const offlineAssetsInclude = [
    /\.dll$/,
    /\.pdb$/,
    /\.wasm$/,
    /\.html$/,
    /\.js$/,
    /\.json$/,
    /\.css$/,
    /\.png$/,
    /\.jpg$/,
    /\.jpeg$/,
    /\.svg$/,
    /\.woff2?$/
];

const offlineAssetsExclude = [
    /^service-worker\.js$/,
    /^version\.json$/
];

self.addEventListener('install', event =>
    event.waitUntil(onInstall()));

self.addEventListener('activate', event =>
    event.waitUntil(onActivate()));

self.addEventListener('fetch', event =>
    event.respondWith(onFetch(event)));

async function onInstall() {
    const assetsRequests = self.assetsManifest.assets
        .filter(asset =>
            offlineAssetsInclude.some(pattern =>
                pattern.test(asset.url)))
        .filter(asset =>
            !offlineAssetsExclude.some(pattern =>
                pattern.test(asset.url)))
        .map(asset =>
            new Request(asset.url, {
                integrity: asset.hash,
                cache: 'no-cache'
            }));

    const cache = await caches.open(cacheName);

    await cache.addAll(assetsRequests);
}

async function onActivate() {
    const cacheKeys = await caches.keys();

    await Promise.all(
        cacheKeys
            .filter(key =>
                key.startsWith(cacheNamePrefix) &&
                key !== cacheName)
            .map(key => caches.delete(key))
    );

    await self.clients.claim();
}

async function onFetch(event) {
    const request = event.request;
    const url = new URL(request.url);

    // Only handle requests belonging to Auto Shop.
    if (
        url.origin !== self.location.origin ||
        !url.pathname.startsWith('/autoshop/')
    ) {
        return fetch(request);
    }

    // Always retrieve version information from the network.
    if (url.pathname === '/autoshop/version.json') {
        return fetch(request, {
            cache: 'no-store'
        });
    }

    // Never cache authenticated requests.
    if (request.headers.has('Authorization')) {
        return fetch(request);
    }

    // Auto Shop SPA navigation.
    // Prefer the current application shell from the network.
    // Use the cached shell only when the network is unavailable.
    if (request.mode === 'navigate') {
        try {
            return await fetch(request, {
                cache: 'no-store'
            });
        }
        catch {
            const cache = await caches.open(cacheName);

            return await cache.match('index.html');
        }
    }

    const cachedResponse =
        await caches.match(request);

    return cachedResponse || fetch(request);
}

















