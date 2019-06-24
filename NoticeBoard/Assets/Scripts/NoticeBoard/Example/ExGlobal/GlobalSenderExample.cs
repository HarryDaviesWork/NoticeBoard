/********************************************************\\

           Created by Harry Davies ~ 21/06/2019

        Example of Basic GlobalNoticeBoard usages

\\********************************************************/

using TravellerPack.NoticeBoard;
using UnityEngine;

public class GlobalSenderExample : MonoBehaviour
{
    private void Update()
    {
        Debug.Log("********** ********** Update Begin ********** **********");

        //Adds message to trigger immediately
        MPac_Test globalAPackage = new MPac_Test("Update Global Message", MessageHeader.TS_PackageTestA.ToString());
        GlobalNoticeBoard.s_instance.AddMessage(MessageFormat.Rapid, MessageHeader.TS_PackageTestA, globalAPackage);

        //Adds message to delay buffer, to trigger in next LateUpdate
        MPac_Test globalBPackage = new MPac_Test("Update Delay Global Message", MessageHeader.TS_PackageTestB.ToString());
        GlobalNoticeBoard.s_instance.AddMessage(MessageFormat.Delayed, MessageHeader.TS_PackageTestB, globalBPackage);
    }

    private void LateUpdate()
    {
        Debug.Log("********** ********** Late Update Begin ********** **********");

        //Adds message prior to delay messages
        MPac_Test globalBPackage = new MPac_Test("LateUpdate Delay Global Message", MessageHeader.TS_PackageTestB.ToString());
        GlobalNoticeBoard.s_instance.AddMessage(MessageFormat.Rapid, MessageHeader.TS_PackageTestB, globalBPackage);
    }
}
