mergeInto(LibraryManager.library, {
    
    // ��������� ID ������������ VK
    GetVkUserId: function (unityObjectNamePtr) {
        const unityObjectName = UTF8ToString(unityObjectNamePtr);
        
        // ���������� ������ ����� VK Bridge
        vkBridge.send('VKWebAppGetUserInfo')
            .then(data => {
                if (data.id) {
                    // �������� ID ������������ ������� � Unity
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

    // ���������� ������ � ��������� VK
    SetVkStorage: function (keyPtr, valuePtr) {
        const key = UTF8ToString(keyPtr);
        const value = UTF8ToString(valuePtr);
        
        vkBridge.send('VKWebAppStorageSet', {
            key: key, 
            value: value
        })
        .then(data => console.log(`[VK] ���������: ${data.result}`))
        .catch(error => console.error(`[VK] ������ ����������: ${error}`));
    },

    ShowVkOrderBox: function (itemNamePtr, unityObjectNamePtr) {
        const itemName = UTF8ToString(itemNamePtr);
        const unityObjectName = UTF8ToString(unityObjectNamePtr);
    
    // ����� ������� ������� ������ �� ��� �������� (item_id)
        vkBridge.send('VKWebAppShowOrderBox', {
            type: 'item', // ��� �������: 'item' (�����) ��� 'votes' (������)
            item: itemName // ID ������ ������ (��������, 'premium_player')
        })
        .then(data => {
        // �����: ������� ���������.
            if (data.status === 'success') {
                window.unityInstance.SendMessage(
                    "VkBridge", 
                    'OnOrderSuccess', 
                    itemName
                );
            }
        // ������: ������ ������� �������������
            else if (data.status === 'cancel') {
                window.unityInstance.SendMessage(
                    "VkBridge", 
                    'OnOrderCanceled', 
                    itemName
                );
            }
        })
        .catch(error => {
            console.error('[VK] ������ ��� ������ OrderBox:', error);
            window.unityInstance.SendMessage(
                "VkBridge", 
                'OnOrderError', 
                error.error_data.error_reason
            );
        });
    },

    ShowVkNativeAd: function (adFormatPtr, unityObjectNamePtr) {
        const adFormat = UTF8ToString(adFormatPtr); // 'interstitial' ��� 'reward'
        const unityObjectName = UTF8ToString(unityObjectNamePtr);
    
    // ����� ������ �������
        vkBridge.send('VKWebAppShowNativeAds', {
            ad_format: adFormat 
        })
        .then(data => {
            if (data.result) { 
            // �����: ������� ���� ��������
            
                if (adFormat === 'reward') {
                // ���� ��� ������� � ��������, �������� OnRewardedSuccess
                    window.unityInstance.SendMessage(
                        "VkBridge", 
                        'OnRewardedSuccess', // ����� �����
                        'RewardGranted' // ����� �������� ��� �������
                    );
                } else {
                // ���� ��� ������� �������, �������� OnAdSuccess
                    window.unityInstance.SendMessage(
                        "VkBridge", 
                        'OnAdSuccess', 
                        adFormat
                    );
                }

            } else {
            // ������: ������� �� ���� ��������
                window.unityInstance.SendMessage(
                    "VkBridge", 
                    'OnAdError', 
                    'Ad not available'
                );
            }
        })
        .catch(error => {
            console.error('[VK] ������ ��� ������ �������:', error);
            window.unityInstance.SendMessage(
                "VkBridge", 
                'OnAdError', 
                error.error_data.error_reason
            );
        });
    },

    SetVkLeaderBoardScore: function (scorePtr, unityObjectNamePtr) {
        const score = parseInt(UTF8ToString(scorePtr)); // ������������ ������ � �����
        const unityObjectName = UTF8ToString(unityObjectNamePtr);
    
    // ���������, ������� ����� ��������� � ������� �������
        vkBridge.send('VKWebAppSetScore', {
            score: score, // ��� �������� TotalScore
        // �����: ��� ������ ������� Mini App ����� �������
        // ���������� �� �������� ��������� �� ����� ����������.
        })
        .then(data => {
            console.log('[VK] ���� ���������:', data);
            if (data.success) {
                window.unityInstance.SendMessage(
                    "VkBridge", 
                    'OnScoreSubmitted', 
                    'Success'
                );
            }
        })
        .catch(error => {
            console.error('[VK] ������ ��� �������� �����:', error);
            window.unityInstance.SendMessage(
                "VkBridge", 
                'OnScoreSubmissionError', 
                error.error_data.error_reason
            );
        });

    },

    ShowVkLeaderBoard: function (unityObjectNamePtr) {
        const unityObjectName = UTF8ToString(unityObjectNamePtr);
    
    // ����� ������� ������� �������
        vkBridge.send('VKWebAppShowLeaderBoardBox', {
        // �� ���������� ��� 'level', ��� ��� ��� ����� �������������
        // � ����� ������� ������� ����� ������������ 'points' ��� �����
           // user_result: 1 // ������������ ��������: �������� �������� ���������� (����� 1 ��� �����)
        })
        .then(data => {
        // �����: ������� ������� ���� �������� � ������� �������������
            window.unityInstance.SendMessage(
                "VkBridge", 
                'OnLeaderBoardClosed', 
                'Success'
            );
        })
        .catch(error => {
            console.error('[VK] ������ ��� ������ ������� �������:', error);
            window.unityInstance.SendMessage(
                "VkBridge", 
                'OnLeaderBoardError', 
                error.error_data.error_reason
            );
        });
    },



    // ... (����� �������� �������� ��� ������ �������) ...
});