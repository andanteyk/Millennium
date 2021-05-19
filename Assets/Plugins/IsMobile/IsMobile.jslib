mergeInto(LibraryManager.library, {
    IsMobile: function() {
        return UnityLoader.SystemInfo.mobile;
    }
});
