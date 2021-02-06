using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using myVISCACommands;



public class PTZ : MonoBehaviour
{
    //private
    UdpConnection connection;
    string sendIp = "192.168.178.88";
    int sendPort = 1259;
    int receivePort = 11000;
    float sendtime;
    public Text antwort;
    string[] msgARR;
    string kamerabild = "http://192.168.178.88/snapshot.jpg";
    //public bool laufendAktualisieren;
    public int fps;

    [Tooltip("Abstand Kamerabefehle von Joystick in ms")]
    public int PTZinterval;
    float PTZtimer;
    public float deadZone = 0.1f;//TODO:grafisch darstellen

    [Tooltip("Maximalgeschwindigkeit für Pan-Tilt (Joystick)")]
    [Range(1, 20)]
    int maxSpeed = 20;//limit für tilt 0x14=20
    public int pan;
    public int tilt;
    public int zoom;

    //[Tooltip("Snapshot-Auflösung von Kamera (1920x1080/960x600/480x300)")]
    aufloesung currentRes;
    enum aufloesung
    {
        hoch, mittel, niedrig
    }
    string[] resolution = { "1920x1080", "960x600", "480x300" };
    const int standardgeschwindigkeitZoom = 2;
    public RawImage background;
    // Start is called before the first frame update
    void Start()
    {
        //ChangeResolution(aufloesung.hoch);
        ChangeResolution(aufloesung.hoch);
        TakeSnapshot();

        connection = new UdpConnection();
        connection.StartConnection(sendIp, sendPort, receivePort);
        sendtime = 0f;
        PTZtimer = Time.time;
        //antwort.text="System little endian?: "+BitConverter.IsLittleEndian.ToString();

        //if(laufendAktualisieren)
        InvokeRepeating("TakeSnapshot", 2f, 1f / fps);//TODO: evtl. direkt aus Coroutine neu aufrufen
    }
    public void Reset()
    {
        connection.Stop();
        CancelInvoke();
        ClearText();

        ChangeResolution(aufloesung.hoch);
        TakeSnapshot();

        connection = new UdpConnection();
        connection.StartConnection(sendIp, sendPort, receivePort);
        sendtime = 0f;
        PTZtimer = Time.time;
        //antwort.text="System little endian?: "+BitConverter.IsLittleEndian.ToString();

        //if(laufendAktualisieren)
        InvokeRepeating("TakeSnapshot", 2f, 1f / fps);//TODO: evtl. direkt aus Coroutine neu aufrufen
    }
    public void BeginMove()
    {
        /*//TODO: ggf. später Auflösung bei Bewegung verringern
        if (currentRes != aufloesung.niedrig) StartCoroutine(ChangeResolution(aufloesung.niedrig));
        CancelInvoke();
        InvokeRepeating("TakeSnapshot", 2f, 1f / fps);
        */
    }
    public void MoveRelative(Vector2 joystick)
    {
        if (Time.time - PTZtimer < PTZinterval / 1000f) return;
        //leanGUI-Joystick scheint trotzdem zu aktualisieren    //TODO:Performance testen

        //pan=(Mathf.Abs(joystick.x)<deadZone) ? 0 : ((int)((joystick.x-deadZone)/(1-deadZone))*maxSpeed);
        //tilt=(Mathf.Abs(joystick.y)<deadZone) ? 0 : ((int)((joystick.y-deadZone)/(1-deadZone))*maxSpeed);        
        pan = (Mathf.Abs(joystick.x) < deadZone) ? 0 : ((int)(joystick.x * maxSpeed));
        tilt = (Mathf.Abs(joystick.y) < deadZone) ? 0 : ((int)(joystick.y * maxSpeed));

        sendtime = connection.Send(VISCACommands.PanTilt(pan, tilt));

        PTZtimer = Time.time;
    }
    public void EndMove()
    {
        //StartCoroutine(ChangeResolution(aufloesung.hoch));
        TakeSnapshot();//TODO: Invoke mit Verzögerung, oder Bestätigung Position abwarten
    }
    public void ZoomRelative(Vector2 joystick)
    {
       // if (Time.time - PTZtimer < PTZinterval / 1000f) return;
        zoom = (Mathf.Abs(joystick.y) < deadZone) ? 0 : ((int)(joystick.y * 8));//Achtung: später 1 abziehen!


        if (zoom == 0) sendtime = connection.Send(VISCACommands.zoomStop());
        else if (zoom > 0) sendtime = connection.Send(VISCACommands.zoomTele(zoom - 1));
        else sendtime = connection.Send(VISCACommands.zoomWide(-zoom - 1));
        PTZtimer = Time.time;
    }

    void Update()
    {
        if (connection == null)
        {
            Debug.Log("no udp connection");
            return;
        }
        #region  Kameraantwort
        msgARR = connection.getMessages();
        if (msgARR.Length > 0)
        {
            antwort.text = "";
            //TODO:"Durchscrollen"
            if (sendtime == 0f) antwort.text += ("Antwort ohne (neue) Anfrage: \n  ");
            else
            {
                antwort.text += ("\n Antwort nach " + (Time.time - sendtime).ToString() + "\n");
                sendtime = 0f;
            }
            foreach (string message in msgARR)
            {
                foreach (char c in message.ToCharArray())
                {
                    byte[] bytes = BitConverter.GetBytes(c);
                    if (BitConverter.ToString(bytes) == "FD-FF")
                        antwort.text += "";
                    //antwort.text += "FD-FF";
                    else
                    {
                        antwort.text += "<color=#00ffff>";
                        antwort.text += BitConverter.ToString(bytes).Substring(0, 2);
                        antwort.text += "</color>";
                    }
                    antwort.text += " ";
                    //Console.WriteLine("{0,10}{1,16}", value,
                    //if(antwort.text!="FD-FF")Debug.Log (antwort.text);
                }
                antwort.text += "\n  ";
                //Debug.Log(antwort.text);

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
        CancelInvoke();
    }

    public void TakeSnapshot() { StartCoroutine(GetTexture()); }

    IEnumerator GetTexture()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(kamerabild);
        www.timeout = 1;
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            antwort.text = "<color=#ff>";
            antwort.text += "Verbindungsfehler!";
            antwort.text += "</color>";
          //  CancelInvoke();
        }
        else background.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
    }
    IEnumerator ChangeResolution(aufloesung newRes)
    {
        /* //TODO: ggf. nach Start und Ende Auflösung anpassen, hängt aber noch irgendwo
        if (newRes == aufloesung.hoch)
        {
            yield return new WaitForSeconds(1f);//aktuelle Fahrt beenden lassen
            CancelInvoke();//keine weiteren Bilder anfordern
        }
 */
        UnityWebRequest www = UnityWebRequest.Get("http://" + sendIp + "/cgi-bin/snapshot.cgi?post_snapshot_conf&resolution=" + resolution[(int)newRes]);
        //http://192.168.178.88/cgi-bin/snapshot.cgi?post_snapshot_conf&resolution=1920x1080
        www.timeout = 1;
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            antwort.text = "<color=#ff>";
            antwort.text += "Verbindungsfehler!";
            antwort.text += "</color>";
           // CancelInvoke();
        }
        else
        {
            Debug.Log("Auflösung geändert: " + resolution[(int)newRes]);
            currentRes = newRes;
        }
    }


    public void FocusPos(int i)
    {
        print("Visca Focus Pos " + i);
        sendtime = connection.Send(VISCACommands.focusPos(i));
    }

    public void ZoomTele(int speed = standardgeschwindigkeitZoom)
    {
        //print("Visca Zoom Tele");

        sendtime = connection.Send(VISCACommands.zoomTele(speed));
    }
    public void ZoomWide(int speed = standardgeschwindigkeitZoom)
    {
        //print("Visca Zoom Wide");
        sendtime = connection.Send(VISCACommands.zoomWide(speed));
    }
    public void ZoomStop()
    {
        print("Visca Zoom Stop");
        sendtime = connection.Send(VISCACommands.zoomStop());
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
    public void ClearText() { antwort.text = ""; }
}
