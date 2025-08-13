// MyBrowserPlugin.jslib
mergeInto(LibraryManager.library, {
    // --- Функции, которые Unity будет вызывать в JavaScript (браузере) ---
    // Эти функции вызываются из C# через DllImport

    // Функция JS, чтобы имитировать клик по кнопке "Слушать" в HTML
    JsTriggerListenClick: function () {
        // Проверяем, существует ли глобальная JS-функция, определенная в index.html
        if (window.JsTriggerListenClickInternal) {
            window.JsTriggerListenClickInternal();
        } else {
            console.warn("JsTriggerListenClickInternal не определен в глобальной области window.");
        }
    },

    // Функция JS, чтобы запросить AI с заданным текстом
    JsAskAIWithPrompt: function (promptPtr) {
        // Преобразуем указатель строки из Unity в JavaScript строку
        // eslint-disable-next-line no-undef
        var prompt = UTF8ToString(promptPtr); 
        if (window.JsAskAIWithPromptInternal) {
            window.JsAskAIWithPromptInternal(prompt);
        } else {
            console.warn("JsAskAIWithPromptInternal не определен в глобальной области window.");
        }
    },

    // Функция JS, чтобы отобразить сообщение в браузере (для отладки)
    ShowBrowserMessage: function (messagePtr) {
        // eslint-disable-next-line no-undef
        var message = UTF8ToString(messagePtr);
        if (window.ShowBrowserMessageInternal) {
            window.ShowBrowserMessageInternal(message);
        } else {
            console.warn("ShowBrowserMessageInternal не определен в глобальной области window.");
            // Если внутренней функции нет, просто выведем в консоль
            console.log("Сообщение из Unity: " + message);
        }
    }
});

