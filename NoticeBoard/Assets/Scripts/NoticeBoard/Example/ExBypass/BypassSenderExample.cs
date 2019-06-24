/********************************************************\\

           Created by Harry Davies ~ 21/06/2019

             Example of Message Bypass usages

\\********************************************************/

using TravellerPack.NoticeBoard;
using UnityEngine;

public class BypassSenderExample : MonoBehaviour
{
    [SerializeField] private LocalNoticeBoard m_noticeBoard = null;

    private void Awake()
    {
        //Creates a subscriber list on LocalNoticeBoard to handle MessageHeader
        m_noticeBoard.CreateSubscriberList("LocalRapidBypass");
        m_noticeBoard.CreateSubscriberList("LocalDelayBypass");
    }

    private void Update()
    {
        Debug.Log("********** ********** Update Begin ********** **********");
        
        //Local bypass examples
        MPac_Test localPackageRapid = new MPac_Test("Delay Bypass Local Message", "LocalRapidBypass");
        m_noticeBoard.AddMessage(MessageFormat.RapidBypass, "LocalRapidBypass", localPackageRapid);

        MPac_Test localPackageDelay = new MPac_Test("Delay Bypass Local Message", "LocalDelayBypass");
        m_noticeBoard.AddMessage(MessageFormat.DelayedBypass, "LocalDelayBypass", localPackageDelay);

        //Global bypass examples
        MPac_Test gloablPackageRapid = new MPac_Test("Rapid Bypass Global Message", MessageHeader.TS_PackageTestA.ToString());
        GlobalNoticeBoard.s_instance.AddMessage(MessageFormat.RapidBypass, MessageHeader.TS_PackageTestA, gloablPackageRapid);

        MPac_Test gloablPackageDelay = new MPac_Test("Delay Bypass Global Message", MessageHeader.TS_PackageTestB.ToString());
        GlobalNoticeBoard.s_instance.AddMessage(MessageFormat.DelayedBypass, MessageHeader.TS_PackageTestB, gloablPackageDelay);
    }
}
