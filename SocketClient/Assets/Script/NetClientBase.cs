using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class NetClientBase
{

    public enum ConnectEvent
    {
        Success = 0,
        ConnectOverTime,
        ReceiveOverTime,
        ReceiveFailed,
        UnknowError,
        SocketNull,
        ConnectError,
        SendError
    }
    public delegate void CallBackNormal(bool IsSuccess, ConnectEvent connectEvent, string exception);

    public delegate void CallBackReceive(bool IsSuccess, ConnectEvent connectEvent, byte[] Msg, string StringMsg);

    private CallBackNormal CallBackSend;
    private CallBackNormal CallbackConnect;
    private CallBackNormal CallbackDisconnect;
    private CallBackNormal DisConnect;
    private CallBackReceive CallbackRev;
    private ConnectEvent connectEvent;
    private Socket ClientSocket;
    private string AddressIP;
    private ushort port;

    Socketbuffer socketbuffer;
    byte[] Bufferdate;
    public NetClientBase()
    {
        socketbuffer = new Socketbuffer(6, CallBackRecvOver);
        Bufferdate = new byte[1024];

    }

    #region Connect

    public bool IsConnecd()
    {
        if (ClientSocket != null && ClientSocket.Connected)
            return true;
        return false;

    }

    public void AsyncConnect(string IP, ushort port, CallBackNormal callBackConnect, CallBackReceive callBackReceive)
    {
        connectEvent = ConnectEvent.Success;
        this.CallbackConnect = callBackConnect;
        this.CallbackRev = callBackReceive;
        if (ClientSocket != null && ClientSocket.Connected)
        {
            this.CallbackConnect(false, ConnectEvent.ConnectError, "Connect Repect");
        }
        else if (ClientSocket == null || !ClientSocket.Connected)
        {
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress iPAddress = IPAddress.Parse(IP);

            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);
            IAsyncResult asyncConnect = ClientSocket.BeginConnect(iPEndPoint, ConnectCallBack, ClientSocket);
            if (!OverTime(asyncConnect))
            {
                this.CallbackConnect(false, ConnectEvent.ConnectOverTime, "Connect OverTime");
            }
        }

    }

    private void ConnectCallBack(IAsyncResult result)
    {
        try
        {
            ClientSocket.EndConnect(result);
            if (ClientSocket.Connected == false)
            {
                connectEvent = ConnectEvent.UnknowError;
                this.CallbackConnect(false, connectEvent, "  Connect is Failed");
                return;
            }
            else
            {
                this.CallbackConnect(true, ConnectEvent.Success, "Connect Success");
            }
        }
        catch (Exception e)
        {
            this.CallbackConnect(false, ConnectEvent.UnknowError, e.ToString());

        }

    }

    #endregion
    #region Receive

    public void ClientReceive()
    {
        if (ClientSocket != null && ClientSocket.Connected)
        {
            IAsyncResult async = ClientSocket.BeginReceive(Bufferdate, 0, Bufferdate.Length, SocketFlags.None, ReceiveCallback, ClientSocket);
            if (!OverTime(async))
            {
                CallbackRev(false,ConnectEvent.ReceiveFailed,null,"Receive OverTime");
            }
        }

    }
    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {

            if (!ClientSocket.Connected)
            {
                CallbackRev(false, ConnectEvent.ReceiveFailed, null, "Receive Failed");
                return;
            }

            int Length = ClientSocket.EndReceive(result);

            if (Length == 0)
                return;
            socketbuffer.RecvByte(Bufferdate, Length);

        }
        catch (Exception e)
        {
            CallbackRev(false, ConnectEvent.ReceiveFailed, null, e.ToString());

        }

        ClientReceive();
    }
    private void CallBackRecvOver(byte[] date)
    {
        CallbackRev(true, ConnectEvent.Success, null, "Receive Success");


    }


    #endregion

    #region SocketSend

    public void AsyncSend(byte[] SendDate, CallBackNormal AsyncSendcallBack)
    {
        this.CallBackSend = AsyncSendcallBack;
        if (ClientSocket == null)
        {
            CallBackSend(false, ConnectEvent.SendError, "Send failed");
        }
        else if (!ClientSocket.Connected)
        {

            CallBackSend(false, ConnectEvent.SendError, "Send Failed");
        }
        else
        {

            IAsyncResult async = ClientSocket.BeginSend(SendDate, 0, SendDate.Length, SocketFlags.None, SendcallBack, ClientSocket);
            if (!OverTime(async))
            {
                CallBackSend(false, ConnectEvent.SendError, "Send OverTime");
            }
        }
    }
    private void SendcallBack(IAsyncResult result)
    {


        try
        {
            int Length = ClientSocket.EndSend(result);
            if (Length > 0)
            {
                CallBackSend(true, ConnectEvent.Success, "Send Success");
            }


        }
        catch (Exception e)
        {

            CallBackSend(false, ConnectEvent.SendError, e.ToString());
        }
    }

    #endregion

    #region OverTime check
    private bool OverTime(IAsyncResult result)
    {
        int Time = 0;
        while (result.IsCompleted == false)
        {
            Time++;
            if (Time > 20)
            {
                connectEvent = ConnectEvent.ConnectOverTime;
                return false;
            }
            Thread.Sleep(100);
        }
        return true;
    }


    #endregion

    #region DisConnect
    public void AsyncDisConnect(CallBackNormal CallBackDisConnect)
    {
        try
        {
            this.DisConnect = CallBackDisConnect;
            if (ClientSocket == null)
            {
                DisConnect(false, ConnectEvent.UnknowError, "Connect is Null");
            }
            else if (!ClientSocket.Connected)
            {
                DisConnect(false, ConnectEvent.UnknowError, "Client  is UnConnect");
            }
            else
            {
                IAsyncResult async = ClientSocket.BeginDisconnect(false, disConnectcallBack, ClientSocket);
                if (!OverTime(async))
                {
                    DisConnect(false, ConnectEvent.UnknowError, "DisConnect OverTime");
                }
            }
        }
        catch (Exception e)
        {

            DisConnect(false, ConnectEvent.UnknowError, e.ToString());
        }


    }

    private void disConnectcallBack(IAsyncResult result)
    {
        try
        {
            ClientSocket.EndDisconnect(result);
            ClientSocket.Close();
            ClientSocket = null;
            DisConnect(true, ConnectEvent.Success, "DisConnect Success");
        }
        catch (Exception e)
        {

            DisConnect(false, ConnectEvent.UnknowError, e.ToString());
        }

    }
    #endregion


}
