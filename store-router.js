function handler(event) {
    var request = event.request;
    var uri = request.uri;

    // Only handle Store Manager routes.
    if (uri === '/store' || uri === '/store/') {
        request.uri = '/store/index.html';
        return request;
    }

    // If the request is under /store/ and does not look like a file,
    // treat it as a Blazor client-side route.
    if (uri.startsWith('/store/')) {
        var lastSegment = uri.substring(uri.lastIndexOf('/') + 1);

        if (lastSegment.indexOf('.') === -1) {
            request.uri = '/store/index.html';
        }
    }

    return request;
}