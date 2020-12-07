using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start()
    {
     connection = new UdpConnection();   
     connection.StartConnection(sendIp, sendPort, receivePort);
     sendtime=0f;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var message in connection.getMessages())
        {if (sendtime==0f) {print ("Antwort ohne (neue) Anfrage: "+ message);
        antwort.text= ("Antwort ohne (neue) Anfrage: "+ message);}
        else{print ("Antwort nach "+ (Time.time - sendtime).ToString() + ": "+ message);
        antwort.text=("Antwort nach "+ (Time.time - sendtime).ToString() + ": "+ message);
        sendtime=0f;
        }
        //connection.Send("Hi!");
    }}
            void OnDestroy()
    {
        connection.Stop();
    
    }

    public void SwitchPosition(int i){
        print("Schwenk zu Position "+i);
        antwort.text=("Schwenk zu Position "+i);
        switch( i){
        case 1:
        connection.SendString("8101043F0201FF");
        break;
        case 2:
        connection.SendString("8101043F0202FF");
        break;
        default:
        print("Fehler");
        break;}
        sendtime=Time.time;
       
    }
    public void FocusPos(int i){
        print("Visca Focus Pos "+i);
        connection.Send(VISCACommands.focusPos(i));
    }

    public void ZoomTele(){
        print("Visca Tele");
        connection.Send(VISCACommands.zoomTele(2));
    }
}
