// localStoragePlugin.js
mergeInto(LibraryManager.library, {
    SetLocalStorage: function (key, value) {
        localStorage.setItem(Pointer_stringify(key), Pointer_stringify(value));
    },

    CloseWindow: function () {
            window.close();
    },

    GetLocalStorage: function (key) {
        var value = localStorage.getItem(Pointer_stringify(key));
        if (value === null) {
            return allocate(intArrayFromString("0"), 'i8', ALLOC_NORMAL);
        }
        return allocate(intArrayFromString(value), 'i8', ALLOC_NORMAL);
    },

    InitializeLocalStorageListener: function () {
        if (typeof localStorageListenerInitialized === 'undefined') {
            localStorageListenerInitialized = true;
            window.addEventListener('storage', function (event) {
                if (event.key && event.newValue) {
                    var unityInstance = window.UnityInstance;
                    if (unityInstance && unityInstance.SendMessage) {
                        unityInstance.SendMessage('LocalStorageManager', 'OnLocalStorageChanged', event.key + "|" + event.newValue);
                    }
                }
            });
        }
    }
});