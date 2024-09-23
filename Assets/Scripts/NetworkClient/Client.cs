using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Client : MonoBehaviour
{
    string SERVER_HOST = "127.0.0.1";
    int SERVER_PORT = 65432;

    void Start()
    {
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);

        //----test----
        NetManager.AddMsgListener("MsgTest", OnMsgTest);

    }

    //---------------------------连接服务端----------------------------------

    //连接成功回调
    public void OnConnectSucc(string err)
    {
        Debug.Log("OnConnectSucc");
    }

    //连接失败回调
    public void OnConnectFail(string err)
    {
        Debug.Log("OnConnectFail" + err);

    }

    //关闭连接
    public void OnConnectClose(string err)
    {
        Debug.Log("OnConnectClose");

    }

    //点击连接按钮
    public void OnConnectClick()
    {
        NetManager.Connect(SERVER_HOST, SERVER_PORT);
        //TODO:开始转圈，提示“连接中”
    }

    //主动关闭
    public void OnCloseCLick()
    {
        NetManager.Close();
    }


    public void Update()
    {
        NetManager.Update();


        if (Input.GetKeyDown(KeyCode.T))
        {

            TestMain();

        }
    }
    //---------------------------------------Test------------------------------------ 
    public void TestMain()
    {

        MsgTest msg = new MsgTest();
        msg.str = "Hello";
        msg.num = 100;
        NetManager.Send(msg);

    }

    public void OnMsgTest(MsgBase msgBase)
    {
        MsgTest msg = (MsgTest)msgBase;
        Debug.Log("OnMsgTest: str=" + msg.str + " num=" + msg.num);
    }

}
