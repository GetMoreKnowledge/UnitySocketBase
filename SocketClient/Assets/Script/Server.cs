using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;
public class Server: MonoBehaviour
{
    private Socket ServerSocket;
    List<SocketState> SocketList;


    void Start ()
    {

        IntialSocket(); 
    }
    public void IntialSocket()
    {
        IPEndPoint EndPoint = new IPEndPoint(IPAddress.Any, 18810);
        ServerSocket = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        ServerSocket.Bind(EndPoint);
        ServerSocket.Listen(100);
        SocketList = new List<SocketState>();
        Thread TmpThread = new Thread(ListenReceive);
        TmpThread.Start();


    }

    void Update()
    {
        if (SocketList.Count>0)
        {
            for (int i = 0; i <SocketList.Count; i++)
            {
                SocketList[i].ServerReceive();
            }
        }

    }


    #region 接受请求
    bool IsRunning = true;
    public void ListenReceive()
    {
        while (IsRunning)
        {
            try
            {
                ServerSocket.BeginAccept(new AsyncCallback(AsyncCallback),ServerSocket);
            }
            catch (Exception e)
            {

                Debug.Log(e);
            }
            Debug.Log("线程");
            Thread.Sleep(1000);
        }

    }
    #endregion

    public void AsyncCallback(IAsyncResult asyncResult)
    {
        Socket Listener = (Socket)asyncResult.AsyncState;
        Socket clientSocket = Listener.EndAccept(asyncResult);
        SocketState socketState = new SocketState(clientSocket);
        SocketList.Add(socketState);
    }

    void OnApplicationQuit()
    {
        ServerSocket.Shutdown(SocketShutdown.Both);
        ServerSocket.Close();

    }
  
}

public class SocketState
{
    public byte[] Buffer;
    public Socket socket;
    public SocketState(Socket socket)
    {
        this.socket = socket;
        Buffer = new byte[1024];

    }
    
    #region  服务器接收消息
    public void ServerReceive()
    {

        //
        socket.BeginReceive(Buffer,0,1024,SocketFlags.None, RecCallBack,this);
         
    }

    public void RecCallBack(IAsyncResult asyncResult)
    {

        //接受数据的位数
        int length = socket.EndReceive(asyncResult);
        string tmpStr = Encoding.Default.GetString(Buffer,0,length);
        Debug.Log("服务器接收的数据"+tmpStr);
        Send("哈哈");
    }


    #endregion
    #region 服务器发送消息

    public void Send(string temStr)
    {
        byte[] Data = Encoding.Default.GetBytes(temStr);

        socket.BeginSend(Data,0,Data.Length,SocketFlags.None,SendCallBack,this);
    }
    public void SendCallBack(IAsyncResult asyncResult)
    {
        int length = socket.EndSend(asyncResult);
        Debug.Log("服务器发送的消息");

    }



    #endregion
}
