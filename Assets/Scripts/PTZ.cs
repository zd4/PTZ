using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using myVISCACommands;

public class PTZ : MonoBehaviour
{
    //private
    UdpConnection connection;
    public string sendIp = "192.168.178.88";
    public int sendPort = 1259;
    int receivePort = 11000;
    float sendtime;
    public Text antwort;
    string[] msgARR;

    const int standardgeschwindigkeit=2;
    // Start is called before the first frame update
    void Start()
    {
        connection = new UdpConnection();
        connection.StartConnection(sendIp, sendPort, receivePort);
        sendtime = 0f;
        antwort.text="System little endian?: "+BitConverter.IsLittleEndian.ToString();
    }

    // Update is called once per frame
    void Update()
    {msgARR=connection.getMessages();
if(msgARR.Length>0){   
         //antwort.text = "";
        if (sendtime == 0f)     antwort.text += ("Antwort ohne (neue) Anfrage: \n  ");
        else
        {
            //print("Antwort nach " + (Time.time - sendtime).ToString() + ": " + BitConverter.ToString(bytes));
            antwort.text += ("\n Antwort nach " + (Time.time - sendtime).ToString() +"\n");
            sendtime = 0f;
        }
        foreach (string message in msgARR)
        {
            foreach (char c in message.ToCharArray())
            {
                byte[] bytes = BitConverter.GetBytes(c);
                antwort.text+= BitConverter.ToString(bytes);
                antwort.text+=" ";
                //Console.WriteLine("{0,10}{1,16}", value,
                
                
            }
            antwort.text+="\n  ";
            //connection.Send("Hi!");
        }
    }
    }
    void OnDestroy()
    {
        connection.Stop();

    }

    public void SwitchPosition(int i)
    {
        print("Schwenk zu Position " + i);
        antwort.text = ("Schwenk zu Position " + i);
        switch (i)
        {
            case 1:
                connection.SendString("8101043F0201FF");
                break;
            case 2:
                connection.SendString("8101043F0202FF");
                break;
            default:
                print("Fehler");
                break;
        }
        sendtime = Time.time;

    }
    public void FocusPos(int i)
    {
        print("Visca Focus Pos " + i);
        connection.Send(VISCACommands.focusPos(i));
    }

    public void ZoomTele(int speed=standardgeschwindigkeit)
    {
        print("Visca Tele");
        sendtime=connection.Send(VISCACommands.zoomTele(speed));
       
    }


    public void ClearText(){
        antwort.text="";
    }
}
