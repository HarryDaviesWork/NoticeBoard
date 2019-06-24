/********************************************************\\

           Created by Harry Davies ~ 21/06/2019

    Example of Removing messages from LocalNoticeBoard

\\********************************************************/

using TravellerPack.NoticeBoard;
using UnityEngine;

public class RemoveSenderExample : MonoBehaviour
{
    [SerializeField] private LocalNoticeBoard m_noticeBoard = null;

    private void Awake()
    {
        //Creates a subscriber list on LocalNoticeBoard to handle MessageHeader
        m_noticeBoard.CreateSubscriberList("RemoveTest");
    }

    private void Update()
    {
        Debug.Log("********** ********** Update Begin ********** **********");

        //Adds pre-remove message
        MPac_Test removePackage = new MPac_Test("RemoveTest Subscription will be Removed!", "Debug_Remove");
        m_noticeBoard.AddMessage(MessageFormat.Rapid, "RemoveTest", removePackage);
    }

    private void LateUpdate()
    {
        Debug.Log("********** ********** Late Update Begin ********** **********");

        //Removes endless RemoveTest message from LocalNoticeBoard, triggering ActiveSubscription removal in Secretaries
        MPac_SubscriberListRemoved removePackage = new MPac_SubscriberListRemoved("RemoveTest Subscription List", "RemoveTest");
        m_noticeBoard.RemoveSubscriberList("RemoveTest", removePackage);
    }
}
