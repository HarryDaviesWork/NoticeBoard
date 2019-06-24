/********************************************************\\

           Created by Harry Davies ~ 21/06/2019

         Example of LocalNoticeBoard Prefix usages

\\********************************************************/

using TravellerPack.NoticeBoard;
using UnityEngine;

public class PrefixSenderExample : MonoBehaviour
{
    [SerializeField] private LocalNoticeBoard m_noticeBoard = null;

    private void Awake()
    {
        //Creates a subscriber list on LocalNoticeBoard to handle MessageHeader
        m_noticeBoard.CreateSubscriberList("Test_MessageA");
        m_noticeBoard.CreateSubscriberList("Test_MessageB");
    }

    private void Update()
    {
        Debug.Log("********** ********** Update Begin ********** **********");

        //Adds example of same prefix messages
        MPac_Test singlePackage1 = new MPac_Test("Update Test Message A", "Test_MessageA");
        m_noticeBoard.AddMessage(MessageFormat.Rapid, "Test_MessageA", singlePackage1);

        MPac_Test singlePackage2 = new MPac_Test("Update Test Message B", "Test_MessageB");
        m_noticeBoard.AddMessage(MessageFormat.Rapid, "Test_MessageB", singlePackage2);
    }
}
