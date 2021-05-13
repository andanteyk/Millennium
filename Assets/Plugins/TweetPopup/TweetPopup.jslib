mergeInto(LibraryManager.library, {
    ShowPopup: function(url) {
        var open = function() { 
            window.open(Pointer_stringify(url));
            document.onmousedown = null;
            document.onkeydown = null;
        };
        document.onmousedown = open;
        document.onkeydown = open;
    }
});
