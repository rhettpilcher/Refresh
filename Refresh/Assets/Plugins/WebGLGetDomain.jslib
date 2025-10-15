mergeInto(LibraryManager.library, {
GetDomain : function () {
    var url = (window.location != window.parent.location)
    ? document.referrer
    : document.location;
    return url;
}
});