/********************************************************\\

           Created by Harry Davies ~ 21/06/2019

         Example of Global MessageSecretary usages

\\********************************************************/

using UnityEngine;
using TravellerPack.NoticeBoard;

public class GlobalListenerExample : MonoBehaviour
{
    [SerializeField] private MessageSecretary m_secretary = null;

    #region General Functions
    private void Awake()
    {
        SetupStaticMessages();
    }

    private void OnDestroy()
    {
        CleatStaticMessages();
    }
    #endregion

    #region Global Examples
    //Manual subscription to GlobalNoticeBoard subscriber list.
    private void SetupStaticMessages()
    {
        GlobalNoticeBoard.s_instance.SubscribeToMessage(MessageHeader.TS_PackageTestA, GlobalMessageTest);
    }

    //Manual unsubscription from GlobalNoticeBoard subscriber list.
    private void CleatStaticMessages()
    {
        //Required to prevents dangling references during object deletion.
        GlobalNoticeBoard.s_instance.UnsubscribeToMessage(MessageHeader.TS_PackageTestA, GlobalMessageTest);
    }

    /// <summary> Manual GlobalNoticeBoard subscription test function </summary>
    public void GlobalMessageTest(object _package = null)
    {
        //Check if _package is not null before casting
        if (_package != null)
        {
            MPac_Test package = (MPac_Test)_package;
            Debug.Log("Global Message: " + package.Message + ", ~Received");
            m_secretary.SubscriptionTriggered(package.MessageHeader, GlobalMessageTest);
        }
    }

    /// <summary> Secertary LocalNoticeBoard unsubscription test function </summary>
    public void GlobalMessageUnsubscribeTest(object _package = null)
    {
        //Check if _package is not null before casting
        if (_package != null)
        {
            MPac_UnsubscribedGlobalMessage package = (MPac_UnsubscribedGlobalMessage)_package;
            Debug.Log(package.UnsubscribedMessage);
        }
    }
    #endregion
}