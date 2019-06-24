/********************************************************\\

           Created by Harry Davies ~ 21/06/2019

         Example of MessageSecretary Prefix usages

\\********************************************************/

using UnityEngine;
using TravellerPack.NoticeBoard;

public class PrefixListenerExample : MonoBehaviour
{
    [SerializeField] private MessageSecretary m_secretary = null;

    #region General Functions
    /// <summary> Secertary remove test function </summary>
    public void MessageRemoveTest(object _package = null)
    {
        //Check if _package is not null before casting
        if (_package != null)
        {
            MPac_SubscriberListRemoved package = (MPac_SubscriberListRemoved)_package;
            Debug.Log("Local Susbcriber List: " + package.RemoveMessage + " ~Removed");
        }
    }
    #endregion  

    #region Local Examples
    /// <summary> Secertary LocalNoticeBoard endless subscription test function </summary>
    public void LocalMessageTest(object _package = null)
    {
        //Check if _package is not null before casting
        if (_package != null)
        {
            MPac_Test package = (MPac_Test)_package;
            Debug.Log("Local Message: " + package.Message + ", ~Received");
            m_secretary.SubscriptionTriggered(package.MessageHeader, LocalMessageTest);
        }
    }

    /// <summary> Secertary LocalNoticeBoard unsubscription test function </summary>
    public void LocalMessageUnsubscribeTest(object _package = null)
    {
        //Check if _package is not null before casting
        if (_package != null)
        {
            MPac_UnsubscribedLocalMessage package = (MPac_UnsubscribedLocalMessage)_package;
            Debug.Log(package.UnsubscribedMessage);
        }
    }
    #endregion
}
