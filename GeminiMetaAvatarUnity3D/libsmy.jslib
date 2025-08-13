// MyBrowserPlugin.jslib
mergeInto(LibraryManager.library, {
    // --- �������, ������� Unity ����� �������� � JavaScript (��������) ---
    // ��� ������� ���������� �� C# ����� DllImport

    // ������� JS, ����� ����������� ���� �� ������ "�������" � HTML
    JsTriggerListenClick: function () {
        // ���������, ���������� �� ���������� JS-�������, ������������ � index.html
        if (window.JsTriggerListenClickInternal) {
            window.JsTriggerListenClickInternal();
        } else {
            console.warn("JsTriggerListenClickInternal �� ��������� � ���������� ������� window.");
        }
    },

    // ������� JS, ����� ��������� AI � �������� �������
    JsAskAIWithPrompt: function (promptPtr) {
        // ����������� ��������� ������ �� Unity � JavaScript ������
        // eslint-disable-next-line no-undef
        var prompt = UTF8ToString(promptPtr); 
        if (window.JsAskAIWithPromptInternal) {
            window.JsAskAIWithPromptInternal(prompt);
        } else {
            console.warn("JsAskAIWithPromptInternal �� ��������� � ���������� ������� window.");
        }
    },

    // ������� JS, ����� ���������� ��������� � �������� (��� �������)
    ShowBrowserMessage: function (messagePtr) {
        // eslint-disable-next-line no-undef
        var message = UTF8ToString(messagePtr);
        if (window.ShowBrowserMessageInternal) {
            window.ShowBrowserMessageInternal(message);
        } else {
            console.warn("ShowBrowserMessageInternal �� ��������� � ���������� ������� window.");
            // ���� ���������� ������� ���, ������ ������� � �������
            console.log("��������� �� Unity: " + message);
        }
    }
});

