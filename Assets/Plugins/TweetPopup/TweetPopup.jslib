// note: keyup イベント上での発行は IE, FireFox では popup block に引っかかる
// が、タイミングを合わせる意味でもここに書いておく
// (クリックでもイベントが消費されるので、意図通りに操作すれば出ない…はず)
mergeInto(LibraryManager.library, {
    RegisterPopupEvent: function(url) {
        var resolvedUrl = Pointer_stringify(url);
        var open = function(event) {
            if (!(event instanceof MouseEvent) && event.key != "z")
                return;
            window.open(resolvedUrl);
            document.getElementById('unity-canvas').removeEventListener('click', open);
            document.removeEventListener('keyup', open);
        };
        document.getElementById('unity-canvas').addEventListener('click', open, false);
        document.addEventListener('keyup', open, false);
    }
});
