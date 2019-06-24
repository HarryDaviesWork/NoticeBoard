/********************************************************\\

           Created by Harry Davies ~ 21/06/2019

        Passes UnsubscriberGlobalMessage message data
                 to subscribed functions

\\********************************************************/

namespace TravellerPack.NoticeBoard
{
    public class MPac_UnsubscribedGlobalMessage : MPac_Base
    {
        public MPac_UnsubscribedGlobalMessage(MessageHeader _unsubscribedHeader, string _unsubscribedMessage,
            string _messageHeader) : base(_messageHeader)
        {
            UnsubscribedHeader = _unsubscribedHeader;
            UnsubscribedMessage = _unsubscribedMessage;
        }

        public MessageHeader UnsubscribedHeader;
        public string UnsubscribedMessage;
    }
}