using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

public class Socketbuffer
{

    private byte[] headbyte;
    private byte headlength;
    private byte[] allRecdate;
    private int curRecvlength;
    private int allDatelength;
    public Socketbuffer(byte headlength,CallBackRecvOver callBackRecvOver)
    {
        this.headlength = headlength;
        headbyte = new byte[headlength];
        over = callBackRecvOver;
    }
    public void RecvByte(byte[]recvByte,int realLength )
    {
        if (realLength == 0)
            return;
        if (curRecvlength<headbyte.Length)
        {
            Recvhead(recvByte,realLength);
        }
        else
        {
            int temLength = curRecvlength + realLength;
            if (temLength==allDatelength)
            {
                RecvOneAll(recvByte,realLength);
            }
            else if(temLength>allDatelength)
            {
                Recvlarger(recvByte,realLength);
            }
            else
            {
                RecvSamll(recvByte,realLength);
            }
                
        }

    }

     private void Recvhead(byte[] recvByte, int realLength )
    {
        int temReal = headbyte.Length - curRecvlength;
        int temLength = curRecvlength + realLength;
        if (temLength<headbyte.Length)
        {
            Buffer.BlockCopy(recvByte,0,headbyte,curRecvlength,realLength);
            curRecvlength += realLength;
        }
        else
        {
            Buffer.BlockCopy(recvByte, 0, headbyte, curRecvlength, temReal);
            curRecvlength += temReal;

            allDatelength = BitConverter.ToInt32(headbyte,0)+headlength;
          
            allRecdate = new byte[allDatelength];
         
            Buffer.BlockCopy(headbyte, 0, allRecdate, 0, headlength);
            int temRemin = realLength - temReal;
            if (temRemin>0)
            {
                byte[] tembyte = new byte[temRemin];
                Buffer.BlockCopy(recvByte, temReal, tembyte, 0, temRemin);
                RecvByte(tembyte,temRemin);
            }
            else
            {
                RecvMsgOver();  
            }
        }
    }

    private void RecvOneAll(byte[]revbyte, int realLength)
    {

        Buffer.BlockCopy(revbyte,0, allRecdate, curRecvlength,  realLength);
        curRecvlength += realLength;
        RecvMsgOver();
    }

    private void Recvlarger(byte[] revbyte, int realLength)
    {
        int temLength = allDatelength - curRecvlength;
        Buffer.BlockCopy(revbyte, 0, allRecdate, curRecvlength, temLength);
        curRecvlength += temLength;
        RecvMsgOver();

        int reaminLength = realLength - temLength;
        byte[] reaminbyte = new byte[reaminLength];
        Buffer.BlockCopy(revbyte,temLength,reaminbyte,0,reaminLength);
        RecvByte(reaminbyte,reaminLength);
    }

    private void RecvSamll(byte[] revbyte, int realLength)
    {
        Buffer.BlockCopy(revbyte, 0, allRecdate, curRecvlength, realLength);
        curRecvlength += realLength;
    }


    public delegate void CallBackRecvOver(byte[]allDate);
    public CallBackRecvOver over;
    private void RecvMsgOver()
    {
        over?.Invoke(allRecdate);
        curRecvlength = 0;
        allDatelength = 0;
        allRecdate = null;


    }

}
