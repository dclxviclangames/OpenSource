mergeInto(LibraryManager.library, {
    
    // Получение ID пользователя VK
    GetVkUserId: function (unityObjectNamePtr) {
        const unityObjectName = UTF8ToString(unityObjectNamePtr);
        
        // Отправляем запрос через VK Bridge
        vkBridge.send('VKWebAppGetUserInfo')
            .then(data => {
                if (data.id) {
                    // Передаем ID пользователя обратно в Unity
                    window.unityInstance.SendMessage(
                        "VkBridge", 
                        'OnVkIdReceived', 
                        data.id.toString()
                    );
                }
            })
            .catch(error => {
                console.log('VK ID Error', error);
                window.unityInstance.SendMessage(
                    "VkBridge", 
                    'OnVkIdError', 
                    error.error_data.error_reason
                );
            });
    },

    // Сохранение данных в хранилище VK
    SetVkStorage: function (keyPtr, valuePtr) {
        const key = UTF8ToString(keyPtr);
        const value = UTF8ToString(valuePtr);
        
        vkBridge.send('VKWebAppStorageSet', {
            key: key, 
            value: value
        })
        .then(data => console.log(`[VK] Сохранено: ${data.result}`))
        .catch(error => console.error(`[VK] Ошибка сохранения: ${error}`));
    },

    ShowVkOrderBox: function (itemNamePtr, unityObjectNamePtr) {
        const itemName = UTF8ToString(itemNamePtr);
        const unityObjectName = UTF8ToString(unityObjectNamePtr);
    
    // Вызов диалога покупки товара по его названию (item_id)
        vkBridge.send('VKWebAppShowOrderBox', {
            type: 'item', // Тип платежа: 'item' (товар) или 'votes' (голоса)
            item: itemName // ID вашего товара (например, 'premium_player')
        })
        .then(data => {
        // УСПЕХ: Покупка совершена.
            if (data.status === 'success') {
                window.unityInstance.SendMessage(
                    "VkBridge", 
                    'OnOrderSuccess', 
                    itemName
                );
            }
        // Ошибка: Платеж отменен пользователем
            else if (data.status === 'cancel') {
                window.unityInstance.SendMessage(
                    "VkBridge", 
                    'OnOrderCanceled', 
                    itemName
                );
            }
        })
        .catch(error => {
            console.error('[VK] Ошибка при вызове OrderBox:', error);
            window.unityInstance.SendMessage(
                "VkBridge", 
                'OnOrderError', 
                error.error_data.error_reason
            );
        });
    },

    ShowVkNativeAd: function (adFormatPtr, unityObjectNamePtr) {
        const adFormat = UTF8ToString(adFormatPtr); // 'interstitial' или 'reward'
        const unityObjectName = UTF8ToString(unityObjectNamePtr);
    
    // Вызов показа рекламы
        vkBridge.send('VKWebAppShowNativeAds', {
            ad_format: adFormat 
        })
        .then(data => {
            if (data.result) { 
            // Успех: Реклама была показана
            
                if (adFormat === 'reward') {
                // Если это реклама с наградой, вызываем OnRewardedSuccess
                    window.unityInstance.SendMessage(
                        "VkBridge", 
                        'OnRewardedSuccess', // НОВЫЙ МЕТОД
                        'RewardGranted' // Можно передать тип награды
                    );
                } else {
                // Если это обычная реклама, вызываем OnAdSuccess
                    window.unityInstance.SendMessage(
                        "VkBridge", 
                        'OnAdSuccess', 
                        adFormat
                    );
                }

            } else {
            // Провал: Реклама не была доступна
                window.unityInstance.SendMessage(
                    "VkBridge", 
                    'OnAdError', 
                    'Ad not available'
                );
            }
        })
        .catch(error => {
            console.error('[VK] Ошибка при показе рекламы:', error);
            window.unityInstance.SendMessage(
                "VkBridge", 
                'OnAdError', 
                error.error_data.error_reason
            );
        });
    },

    SetVkLeaderBoardScore: function (scorePtr, unityObjectNamePtr) {
        const score = parseInt(UTF8ToString(scorePtr)); // Конвертируем строку в число
        const unityObjectName = UTF8ToString(unityObjectNamePtr);
    
    // Результат, который будет отображен в таблице лидеров
        vkBridge.send('VKWebAppSetScore', {
            score: score, // Ваш итоговый TotalScore
        // ВАЖНО: При первом запуске Mini App часто требует
        // разрешения на отправку сообщений от имени сообщества.
        })
        .then(data => {
            console.log('[VK] счет отправлен:', data);
            if (data.success) {
                window.unityInstance.SendMessage(
                    "VkBridge", 
                    'OnScoreSubmitted', 
                    'Success'
                );
            }
        })
        .catch(error => {
            console.error('[VK] Ошибка при отправке счета:', error);
            window.unityInstance.SendMessage(
                "VkBridge", 
                'OnScoreSubmissionError', 
                error.error_data.error_reason
            );
        });

    },

    ShowVkLeaderBoard: function (unityObjectNamePtr) {
        const unityObjectName = UTF8ToString(unityObjectNamePtr);
    
    // Вызов диалога таблицы лидеров
        vkBridge.send('VKWebAppShowLeaderBoardBox', {
        // Мы используем тип 'level', так как это самый универсальный
        // В более сложных случаях можно использовать 'points' для очков
           // user_result: 1 // Обязательный параметр: передача текущего результата (можно 1 для теста)
        })
        .then(data => {
        // Успех: Таблица лидеров была показана и закрыта пользователем
            window.unityInstance.SendMessage(
                "VkBridge", 
                'OnLeaderBoardClosed', 
                'Success'
            );
        })
        .catch(error => {
            console.error('[VK] Ошибка при показе таблицы лидеров:', error);
            window.unityInstance.SendMessage(
                "VkBridge", 
                'OnLeaderBoardError', 
                error.error_data.error_reason
            );
        });
    },



    // ... (можно добавить заглушки для других функций) ...
});