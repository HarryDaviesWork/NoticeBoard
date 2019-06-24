/********************************************************\\

           Created by Harry Davies ~ 21/06/2019

      Test message package for debugging and examples

\\********************************************************/

namespace TravellerPack.NoticeBoard
{
    public class MPac_Test : MPac_Base
    {
        public MPac_Test(string _message,
                string _messageHeader) : base(_messageHeader)
        {
            Message = _message;
        }

        public string Message;
    }
}