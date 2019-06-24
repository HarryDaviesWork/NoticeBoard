/********************************************************\\

           Created by Harry Davies ~ 09/06/2019

         Passes DynamicMessageCreated message data
                 to subscribed functions

\\********************************************************/

namespace TravellerPack.NoticeBoard
{
    public class MPac_DynamicMessageCreated : MPac_Base
    {
        public MPac_DynamicMessageCreated(LocalNoticeBoard _noticeBoard, string _messagePrefix,
            string _messageHeader) : base(_messageHeader)
        {
            NoticeBoard = _noticeBoard;
            MessagePrefix = _messagePrefix;
        }

        public LocalNoticeBoard NoticeBoard;
        public string MessagePrefix;
    }
}
