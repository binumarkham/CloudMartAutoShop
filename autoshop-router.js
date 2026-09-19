function handler(event) {
    var request = event.request;
    var uri = request.uri;

    // Auto Shop root.
    if (uri === '/autoshop' || uri === '/autoshop/') {
        request.uri = '/autoshop/index.html';
        return request;
    }

    // Blazor client-side routes under /autoshop/.
    if (uri.startsWith('/autoshop/')) {
        var lastSegment = uri.substring(uri.lastIndexOf('/') + 1);

        if (lastSegment.indexOf('.') === -1) {
            request.uri = '/autoshop/index.html';
        }
    }

    return request;
}
