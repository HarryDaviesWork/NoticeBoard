/********************************************************\\

           Created by Harry Davies ~ 21/06/2019

        Passes UnsubscriberLocalMessage message data
                 to subscribed functions

\\********************************************************/

namespace TravellerPack.NoticeBoard
{
    public class MPac_UnsubscribedLocalMessage : MPac_Base
    {
        public MPac_UnsubscribedLocalMessage(string _unsubscribedHeader, string _unsubscribedMessage,
            string _messageHeader) : base(_messageHeader)
        {
            UnsubscribedHeader = _unsubscribedHeader;
            UnsubscribedMessage = _unsubscribedMessage;
        }

        public string UnsubscribedHeader;
        public string UnsubscribedMessage;
    }
}