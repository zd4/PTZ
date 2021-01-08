//https://forum.unity.com/threads/simple-udp-implementation-send-read-via-mono-c.15900/
// Änderungen für direkten Versand des Byte-Arrays

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
 
public class UdpConnection
{
    private UdpClient udpClient;
 
    private readonly Queue<string> incomingQueue = new Queue<string>();
    Thread receiveThread;
    private bool threadRunning = false;
    private string senderIp;
    private int senderPort;
 
    public void StartConnection(string sendIp, int sendPort, int receivePort)
    {
        try { udpClient = new UdpClient(receivePort); }
        catch (Exception e)
        {
            Debug.Log("Failed to listen for UDP at port " + receivePort + ": " + e.Message);
            return;
        }
        Debug.Log("Created receiving client at ip  and port " + receivePort);
        this.senderIp = sendIp;
        this.senderPort = sendPort;
 
        Debug.Log("Set sendee at ip " + sendIp + " and port " + sendPort);
 
        StartReceiveThread();
    }
 
    private void StartReceiveThread()
    {
        receiveThread = new Thread(() => ListenForMessages(udpClient));
        receiveThread.IsBackground = true;
        threadRunning = true;
        receiveThread.Start();
    }
 
    private void ListenForMessages(UdpClient client)
    {
        IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
 
        while (threadRunning)
        {
            try
            {
                Byte[] receiveBytes = client.Receive(ref remoteIpEndPoint); // Blocks until a message returns on this socket from a remote host.
                //string returnData = Encoding.UTF8.GetString(receiveBytes);
                //test
                //Array.Reverse (receiveBytes);
                //string returnData = Encoding.UTF32.GetString(receiveBytes);//"FD-FF"
                string returnData = Encoding.UTF8.GetString(receiveBytes);//((FD-FF 42-00) mit reverse)  Antwort nach 0,09942436 FD-FF 42-00 FD-FF   Antwort ohne (neue) Anfrage:  FD-FF 52-00 FD-FF 
 //ohne rev:  Antwort nach 0,08919263 FD-FF 41-00 FD-FF   Antwort ohne (neue) Anfrage:   FD-FF 51-00 FD-FF 
                lock (incomingQueue)
                {
                    incomingQueue.Enqueue(returnData);
                }
            }
            catch (SocketException e)
            {
                // 10004 thrown when socket is closed
                if (e.ErrorCode != 10004) Debug.Log("Socket exception while receiving data from udp client: " + e.Message);
            }
            catch (Exception e)
            {
                Debug.Log("Error receiving data from udp client: " + e.Message);
            }
            Thread.Sleep(1);
        }
    }
 
    public string[] getMessages()
    {
        string[] pendingMessages = new string[0];
        lock (incomingQueue)
        {
            pendingMessages = new string[incomingQueue.Count];
            int i = 0;
            while (incomingQueue.Count != 0)
            {
                pendingMessages[i] = incomingQueue.Dequeue();
                i++;
            }
        }
 
        return pendingMessages;
    }
 
    public void SendString(string message)
    {
        Debug.Log(String.Format("Send msg to ip:{0} port:{1} msg:{2}",senderIp,senderPort,message));
        IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse(senderIp), senderPort);
        //Byte[] sendBytes = Encoding.UTF8.GetBytes(message);
        
        Byte[] sendBytes = Encoding.UTF32.GetBytes(message);
        //Byte[] sendBytes = Encoding.UTF7.GetBytes(message);
        udpClient.Send(sendBytes, sendBytes.Length, serverEndpoint);
    }

    public float Send(Byte[] sendBytes)
    {
    //    Debug.Log(String.Format("Send msg to ip:{0} port:{1} msg:{2}",senderIp,senderPort,"ByteArray"));
        IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse(senderIp), senderPort);
        udpClient.Send(sendBytes, sendBytes.Length, serverEndpoint);
        return Time.time;
    }
 
    public void Stop()
    {
        threadRunning = false;
        receiveThread.Abort();
        udpClient.Close();
    }

    /*Änderungsvorschlag aus Thread:
    void OnApplicationQuit () {
 
        if (receiveThread.IsAlive) {
            receiveThread.Abort();
        }
        receiver.Close();
 }*/
}