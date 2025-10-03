
mergeInto(LibraryManager.library, {
    
    RequestTelegramInvoice: function (unityObjectNamePtr, itemIdentifierPtr, pricePtr) {
        
        const unityObjectName = UTF8ToString(unityObjectNamePtr);
        const itemIdentifier = UTF8ToString(itemIdentifierPtr);
        const price = UTF8ToString(pricePtr);
        
        console.log(`[JSLIB] Начало симуляции покупки: ${itemIdentifier} за ${price} Звёзд.`);

        // --- ЛОГИКА ФЕЙКОВОЙ ОПЛАТЫ ---
        setTimeout(() => {
            
            console.log(`[JSLIB] Успех симуляции. Вызов OnPaymentSuccess в C#.`);
            
            // Вызываем метод OnPaymentSuccess в C#
            window.unityInstance.SendMessage(unityObjectName, 'OnPaymentSuccess', itemIdentifier);
            
        }, 2000);
    },
    
    // Заглушки для предотвращения ошибок компоновщика
    OnPaymentSuccess: function (itemIdentifierPtr) {},
    OnPaymentFailure: function (messagePtr) {}
});
