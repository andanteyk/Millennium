mergeInto(LibraryManager.library, {
    IsMobile: function() {
        return Module.SystemInfo.mobile;
    }
});
