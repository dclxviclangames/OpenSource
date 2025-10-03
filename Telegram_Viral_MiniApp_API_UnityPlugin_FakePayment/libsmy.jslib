
mergeInto(LibraryManager.library, {
    
    RequestTelegramInvoice: function (unityObjectNamePtr, itemIdentifierPtr, pricePtr) {
        
        const unityObjectName = UTF8ToString(unityObjectNamePtr);
        const itemIdentifier = UTF8ToString(itemIdentifierPtr);
        const price = UTF8ToString(pricePtr);
        
        console.log(`[JSLIB] ������ ��������� �������: ${itemIdentifier} �� ${price} ����.`);

        // --- ������ �������� ������ ---
        setTimeout(() => {
            
            console.log(`[JSLIB] ����� ���������. ����� OnPaymentSuccess � C#.`);
            
            // �������� ����� OnPaymentSuccess � C#
            window.unityInstance.SendMessage(unityObjectName, 'OnPaymentSuccess', itemIdentifier);
            
        }, 2000);
    },
    
    // �������� ��� �������������� ������ ������������
    OnPaymentSuccess: function (itemIdentifierPtr) {},
    OnPaymentFailure: function (messagePtr) {}
});
