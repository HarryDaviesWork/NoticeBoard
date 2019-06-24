/********************************************************\\

           Created by Harry Davies ~ 08/06/2019

          Contains message subscription delegate,
              Static Message Identifiers and
               Common Message functionality

\\********************************************************/
/*#######################################################\\

                   GLO = Global Messages
                   TS  = Test Messages

\\#######################################################*/

namespace TravellerPack.NoticeBoard
{
    public delegate void SubscriberFunction(object data = null);

    public enum MessageFormat
    {
        Rapid,
        Delayed,
        RapidBypass,
        DelayedBypass
    }

    public enum MessageHeader
    {
        GLO_DynamicMessageCreated,
        
        TS_PackageTestA,
        TS_PackageTestB,


        Count //Must be last element
    }

    public class MessageHelper
    {
        /// <summary>Returns MessageHeader prefix, if none found, returns "N/A"</summary>
        public static string GetPrefix(string _messageHeader)
        {
            int underscoreIndex = _messageHeader.IndexOf("_");
            if (underscoreIndex != -1)
            {
                return _messageHeader.Substring(0, underscoreIndex);
            }
            return "N/A";
        }
    }
}
