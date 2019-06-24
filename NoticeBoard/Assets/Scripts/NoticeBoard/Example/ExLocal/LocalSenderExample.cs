/********************************************************\\

           Created by Harry Davies ~ 21/06/2019

         Example of Basic LocalNoticeBoard usages

\\********************************************************/

using TravellerPack.NoticeBoard;
using UnityEngine;

public class LocalSenderExample : MonoBehaviour
{
    [SerializeField] private LocalNoticeBoard m_noticeBoard = null;

    private void Awake()
    {
        Debug.Log("********** ********** Awake Begin ********** **********");

        //Creates a subscriber list on LocalNoticeBoard to handle MessageHeader
        m_noticeBoard.CreateSubscriberList("EndlessTest");
        m_noticeBoard.CreateSubscriberList("SingleTest");

        //Creates a subscriber list and sends a message for the same header
        MPac_Test soloPackage = new MPac_Test("Awake Solo Message", "SoloTest");
        m_noticeBoard.CreateListAndAddMessage(MessageFormat.Rapid, "SoloTest", soloPackage);
    }

    private void Update()
    {
        Debug.Log("********** ********** Update Begin ********** **********");

        //Adds message to delay buffer, to trigger in next LateUpdate
        MPac_Test soloDelayPackage = new MPac_Test("Update Delay Solo Message", "SoloTest");
        m_noticeBoard.AddMessage(MessageFormat.Delayed, "SoloTest", soloDelayPackage);

        //Adds message to trigger immediately
        MPac_Test endlessPackage = new MPac_Test("Update Endless Message", "EndlessTest");
        m_noticeBoard.AddMessage(MessageFormat.Rapid, "EndlessTest", endlessPackage);

        MPac_Test singlePackage = new MPac_Test("Update Single Message", "SingleTest");
        m_noticeBoard.AddMessage(MessageFormat.Rapid, "SingleTest", singlePackage);

        MPac_Test soloPackage = new MPac_Test("Update Solo Message", "SoloTest");
        m_noticeBoard.AddMessage(MessageFormat.Rapid, "SoloTest", soloPackage);
    }

    private void LateUpdate()
    {
        Debug.Log("********** ********** Late Update Begin ********** **********");

        //Adds message to trigger immediately
        MPac_Test soloPackage = new MPac_Test("LateUpdate Solo Message", "SoloTest");
        m_noticeBoard.AddMessage(MessageFormat.Rapid, "SoloTest", soloPackage);

        /* Normally if the delayed message is sent in LateUpdate it will trigger in the next frame,
         due to script execution order it will trigger this frame */
        MPac_Test soloDelayPackage = new MPac_Test("LateUpdate Delay Solo Message", "SoloTest");
        m_noticeBoard.AddMessage(MessageFormat.Delayed, "SoloTest", soloDelayPackage);
    }
}
