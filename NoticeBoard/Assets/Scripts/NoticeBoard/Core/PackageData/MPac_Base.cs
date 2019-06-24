/********************************************************\\

           Created by Harry Davies ~ 21/06/2019

              Base for all Message Packages,
                 containing common data.

\\********************************************************/

namespace TravellerPack.NoticeBoard
{
    public class MPac_Base
    {
        public MPac_Base(string _messageHeader)
        {
            MessageHeader = _messageHeader;
        }

        public string MessageHeader;
    }
}
