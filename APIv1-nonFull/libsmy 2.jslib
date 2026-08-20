mergeInto(LibraryManager.library, {

    // 1. Инициализация моста ВК
    InitVkBridge: function () {
        if (typeof vkBridge !== 'undefined') {
            vkBridge.send("VKWebAppInit")
                .then(data => {
                    console.log("[VK JS] Мост успешно запущен.");
                    if (window.unityInstance) {
                        window.unityInstance.SendMessage("VkBridge", "OnVkInitialized");
                    }
                })
                .catch(error => {
                    console.error("[VK JS] Ошибка инициализации:", error);
                });
        }
    },

    // 2. Получение ID пользователя
    GetVkUserId: function () {
        if (typeof vkBridge !== 'undefined') {
            vkBridge.send('VKWebAppGetUserInfo')
                .then(data => {
                    if (data && data.id) {
                        const userId = data.id.toString();
                        if (window.unityInstance) {
                            window.unityInstance.SendMessage("VkBridge", "OnVkIdReceived", userId);
                        }
                    }
                })
                .catch(error => {
                    console.error("[VK JS] Ошибка получения ID юзера:", error);
                    if (window.unityInstance) {
                        window.unityInstance.SendMessage("VkBridge", "OnVkIdError", "Error");
                    }
                });
        }
    },

    // 3. Сохранение данных в Облако (Storage)
    SetVkStorage: function (keyPtr, valuePtr) {
        if (typeof vkBridge !== 'undefined') {
            const key = UTF8ToString(keyPtr);
            const value = UTF8ToString(valuePtr);
            
            vkBridge.send('VKWebAppStorageSet', { key: key, value: value })
                .then(data => console.log(`[VK JS] Успешно сохранено: ${key} = ${value}`))
                .catch(error => console.error("[VK JS] Ошибка сохранения в Storage:", error));
        }
    },

    // 4. Получение данных из Облака (Storage)
    GetVkStorage: function (keyPtr) {
        if (typeof vkBridge !== 'undefined') {
            const key = UTF8ToString(keyPtr);
            
            vkBridge.send('VKWebAppStorageGet', { keys: [key] })
                .then(data => {
                    let loadedValue = "0";
                    if (data && data.keys && data.keys[0] && data.keys[0].value) {
                        loadedValue = data.keys[0].value;
                    }
                    if (window.unityInstance) {
                        window.unityInstance.SendMessage("VkBridge", "OnVkStorageLoaded", loadedValue);
                    }
                })
                .catch(error => {
                    console.error("[VK JS] Ошибка загрузки из Storage:", error);
                    if (window.unityInstance) {
                        window.unityInstance.SendMessage("VkBridge", "OnVkStorageLoaded", "0");
                    }
                });
        }
    },

    // 5. Показать нативное окно Лидерборда (ВК Корс фикс)
    ShowVkLeaderBoard: function (score) {
        if (typeof vkBridge !== 'undefined') {
            vkBridge.send('VKWebAppShowLeaderBoardBox', {
                user_result: parseInt(score),   
                type: 'points',       
                global: 1             
            })
            .then(data => {
                if (window.unityInstance) {
                    window.unityInstance.SendMessage("VkBridge", "OnLeaderBoardClosed", "Success");
                }
            })
            .catch(error => {
                console.warn("[VK JS] Лидерборд закрыт или недоступен (на ПК это норма):", error);
                if (window.unityInstance) {
                    window.unityInstance.SendMessage("VkBridge", "OnLeaderBoardClosed", "ErrorOrCancel");
                }
            });
        }
    },

    // 6. Показ рекламы (Интерстициал и Ревардед)
    ShowVkNativeAd: function (adFormatPtr) {
        if (typeof vkBridge !== 'undefined') {
            let adFormat = UTF8ToString(adFormatPtr);
            
            // Фикс формата: ВК требует именно "rewarded", исправляем "reward" налету
            if (adFormat === 'reward') adFormat = 'rewarded';
        
            vkBridge.send('VKWebAppShowNativeAds', { ad_format: adFormat })
            .then(data => {
                if (data && data.result) { 
                    if (adFormat === 'rewarded') {
                        if (window.unityInstance) window.unityInstance.SendMessage("VkBridge", "OnRewardedSuccess", "Success");
                    } else {
                        if (window.unityInstance) window.unityInstance.SendMessage("VkBridge", "OnAdSuccess", adFormat);
                    }
                } else {
                    if (window.unityInstance) window.unityInstance.SendMessage("VkBridge", "OnAdError", "NotAvailable");
                }
            })
            .catch(error => {
                console.error("[VK JS] Ошибка при показе рекламы:", error);
                if (window.unityInstance) {
                    window.unityInstance.SendMessage("VkBridge", "OnAdError", "ClosedOrError");
                }
            });
        }
    }
});
