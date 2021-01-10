using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
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
    public string kamerabild = "http://192.168.178.88/snapshot.jpg";
    public bool laufendAktualisieren;

    public int fps;

[Tooltip("Abstand Kamerabefehle von Joystick in ms")]
    public int PTZinterval;
     float PTZtimer;
     
     public float deadZone=0.1f;

[Tooltip("Maximalgeschwindigkeit für Pan-Tilt (Joystick)")]
[Range (1,20)]
int maxSpeed=20;//limit für tilt 0x14=20
public int pan;
public int tilt;

    //[Tooltip("Snapshot-Auflösung von Kamera (1920x1080/960x600/480x300)")]
    aufloesung currentRes;
    public enum aufloesung
    {

        hoch, mittel, niedrig
    }


    string[] resolution = { "1920x1080", "960x600", "480x300" };

    const int standardgeschwindigkeit = 2;

    public RawImage background;

    // Start is called before the first frame update
    void Start()
    {
        ChangeResolution(aufloesung.hoch);
        currentRes = aufloesung.hoch;
        TakeSnapshot();

        connection = new UdpConnection();
        connection.StartConnection(sendIp, sendPort, receivePort);
        sendtime = 0f;
        PTZtimer=Time.time;
        //antwort.text="System little endian?: "+BitConverter.IsLittleEndian.ToString();

        //if(laufendAktualisieren)InvokeRepeating("TakeSnapshot",2f,1f/fps);//TODO: evtl. direktt aus Coroutine neu aufrufen


    }

    public void BeginMove()
    {
        if (currentRes != aufloesung.niedrig) StartCoroutine(ChangeResolution(aufloesung.niedrig));
        CancelInvoke();
        InvokeRepeating("TakeSnapshot", 2f, 1f / fps);
    }
    public void MoveRelative(Vector2 joystick){
        if(Time.time-PTZtimer<PTZinterval/1000f)return;
        //leanGUI-Joystick scheint au

#region alt
      /* Methode LR angefangen. Effektiver doch in ViscaCommands!
        pan=Mathf.Clamp(joystick.x,-1,1);
        tilt=Mathf.Clamp(joystick.y,-1,1);
        if ((Mathf.Abs(pan)<deadZone)&&(+Mathf.Abs(tilt)<deadZone)) sendtime = connection.Send(VISCACommands.moveStop());//STOP
        if(Mathf.Abs(tilt)<deadZone){
            if (pan>deadZone)  sendtime = connection.Send(VISCACommands.panRight((int)(20*pan)));//max speed tilt 0x14=20
            else if (pan<-deadZone) sendtime = connection.Send(VISCACommands.panLeft((int)(20*Mathf.Abs(pan))));

        }
        else if (Mathf.Abs(pan)<deadZone){
            //if (tilt>deadZone)print ("hoch");
            //else print ("runter");
        }
         */

#endregion

        //pan=(Mathf.Abs(joystick.x)<deadZone) ? 0 : ((int)((joystick.x-deadZone)/(1-deadZone))*maxSpeed);
        //tilt=(Mathf.Abs(joystick.y)<deadZone) ? 0 : ((int)((joystick.y-deadZone)/(1-deadZone))*maxSpeed);        
        pan=(Mathf.Abs(joystick.x)<deadZone) ? 0 : ((int)(joystick.x*maxSpeed));
        tilt=(Mathf.Abs(joystick.y)<deadZone) ? 0 : ((int)(joystick.y*maxSpeed));        
        
sendtime=connection.Send(VISCACommands.PanTilt(pan,tilt));

        PTZtimer=Time.time;
    }
    public void EndMove()
    {

        StartCoroutine(ChangeResolution(aufloesung.hoch));

    }
    // Update is called once per frame
    void Update()
    {
        #region  Kameraantwort
        msgARR = connection.getMessages();
        if (msgARR.Length > 0)
        {
            //antwort.text = "";
            if (sendtime == 0f) antwort.text += ("Antwort ohne (neue) Anfrage: \n  ");
            else
            {
                //print("Antwort nach " + (Time.time - sendtime).ToString() + ": " + BitConverter.ToString(bytes));

                antwort.text += ("\n Antwort nach " + (Time.time - sendtime).ToString() + "\n");
                sendtime = 0f;
            }
            foreach (string message in msgARR)
            {
                foreach (char c in message.ToCharArray())
                {
                    byte[] bytes = BitConverter.GetBytes(c);
                    if (BitConverter.ToString(bytes) == "FD-FF") antwort.text += "FD-FF";
                    else
                    {
                        antwort.text += "<color=#f00>";
                        antwort.text += BitConverter.ToString(bytes);
                        antwort.text += "</color>";
                    }
                    antwort.text += " ";
                    //Console.WriteLine("{0,10}{1,16}", value,
                    //if(antwort.text!="FD-FF")Debug.Log (antwort.text);

                }
                antwort.text += "\n  ";
                Debug.Log(antwort.text);
                //connection.Send("Hi!");
            }
        }
        #endregion

        #region Kamerabefehl laufend
//im ersten SChritt: "Fire and forget"//TODO: Anständig auf Antwort warten und Socketbelegung berücksichtigen


        #endregion
    }



    void OnDestroy()
    {
        connection.Stop();

    }

    public void TakeSnapshot()
    {
        //background.overrideSprite=kamerabild;
        StartCoroutine(GetTexture());

    }

    IEnumerator GetTexture()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(kamerabild);
        www.timeout = 1;
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            //Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            background.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }
    IEnumerator ChangeResolution(aufloesung newRes)
    {
        if (newRes == aufloesung.hoch)
        {
            yield return new WaitForSeconds(1f);//aktuelle Fahrt beenden lassen
            CancelInvoke();//keine weiteren Bilder anfordern
        }

        UnityWebRequest www = UnityWebRequest.Get("http://" + sendIp + "/cgi-bin/snapshot.cgi?post_snapshot_conf&resolution=" + resolution[(int)newRes]);
        //http://192.168.178.88/cgi-bin/snapshot.cgi?post_snapshot_conf&resolution=1920x1080
        www.timeout = 1;
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Auflösung geändert: " + resolution[(int)newRes]);
        }
    }


    /* 
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
     */
    public void FocusPos(int i)
    {
        print("Visca Focus Pos " + i);
        sendtime = connection.Send(VISCACommands.focusPos(i));
    }

    public void ZoomTele(int speed = standardgeschwindigkeit)
    {
        print("Visca Tele");
        sendtime = connection.Send(VISCACommands.zoomTele(speed));

    }

    public void InqZoom()
    {
        print("inquire Zoom");
        antwort.text = "Abfrage aktueller Zoom";
        sendtime = connection.Send(VISCACommands.INQ_Zoom());
    }
public void InqPosition()
    {
        print("inquire Position");
        antwort.text = "Abfrage Pan/Tilt-Position";
        sendtime = connection.Send(VISCACommands.INQ_Position());
    }


    public void ClearText()
    {
        antwort.text = "";
    }
}
