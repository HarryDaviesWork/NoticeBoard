/********************************************************\\

           Created by Harry Davies ~ 09/06/2019

         Passes SubscriberListRemoved message data
                 to subscribed functions

\\********************************************************/

namespace TravellerPack.NoticeBoard
{
    public class MPac_SubscriberListRemoved : MPac_Base
    {
        public MPac_SubscriberListRemoved(string _removeMessage, 
            string _messageHeader) : base(_messageHeader)
        {
            RemoveMessage = _removeMessage;
        }
        
        public string RemoveMessage;
    }
}