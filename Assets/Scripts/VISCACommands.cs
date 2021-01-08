//aus https://github.com/jitenshap/SONY_VISCA_dotnet/tree/master/VISCALib
//angepasst für Cam1


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myVISCACommands
{
    

    public class VISCACommands
    {
        byte[] setAddress = { 0x88, 0x30, 0x01, 0xff };
        byte[] ifClear = { 0x88, 0x01, 0x00, 0x01, 0xff };
        static byte CamId=(byte) (0x81);
        

/* 
        public static byte[] cmdCancel( int socketId)
        {
            byte[] buf = { 0x80, 0x20, 0xff };
            buf[0] = CamId;
            buf[1] = (byte) (socketId + 0x20);
            buf[2] = 0xff;
            return buf;
        }
 */        public static byte[] swOn(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x00, 0x02, 0xff };
            buf[0] = CamId;
            return buf;
        }
        public static byte[] swOff(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x00, 0x03, 0xff };
            buf[0] = CamId;
            return buf;
        }
        public static byte[] zoomStop(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x07, 0x00, 0xff };
            buf[0] = CamId;
            return buf;
        }
        public static byte[] zoomTele( int speed)
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x07, 0x20, 0xff };
            buf[0] = CamId;
            buf[4] += (byte)speed;
            return buf;
        }
        public static byte[] zoomWide(  int  speed)
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x07, 0x30, 0xff };
            buf[0] = CamId;
            buf[4] += (byte)speed;
            return buf;
        }
        public static byte[] zoomPos(  int  pos)
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x47, 0x00, 0x00, 0x00, 0x00, 0xff };
            buf[0] = CamId;
            if(pos >> 0x0c > 0x0)
            {
                buf[4] = (byte)(pos >> 0x0c);
                pos -= pos >> 0x0c;
            }
            if (pos >> 0x08 > 0x0)
            {
                buf[5] = (byte)(pos >> 0x08);
                pos -= pos >> 0x08;
            }
            if (pos >> 0x04 > 0x0)
            {
                buf[6] = (byte)(pos >> 0x04);
                pos -= pos >> 0x04;
            }
            buf[7] = (byte)pos;
            return buf;
        }
        public static byte[] focusStop(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x08, 0x00, 0xff };
            buf[0] = CamId;
            return buf;
        }
        public static byte[] focusFar(  int  speed)
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x08, 0x20, 0xff };
            buf[0] = CamId;
            buf[4] += (byte)speed;
            return buf;
        }
        public static byte[] focusNear(  int  speed)
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x08, 0x30, 0xff };
            buf[0] = CamId;
            buf[4] += (byte)speed;
            return buf;
        }
        public static byte[] focusPos(  int  pos)
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x47, 0x00, 0x00, 0x00, 0x00, 0xff };
            buf[0] = CamId;
            if (pos >> 0x0c > 0x0)
            {
                buf[4] = (byte)(pos >> 0x0c);
                pos -= pos >> 0x0c;
            }
            if (pos >> 0x08 > 0x0)
            {
                buf[5] = (byte)(pos >> 0x08);
                pos -= pos >> 0x08;
            }
            if (pos >> 0x04 > 0x0)
            {
                buf[6] = (byte)(pos >> 0x04);
                pos -= pos >> 0x04;
            }
            buf[7] = (byte)pos;
            return buf;
        }
       /*  public static byte[] focusInf(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x18, 0x02, 0xff };
            buf[0] = CamId;
            return buf;
        } */
        public static byte[] focusAF(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x38, 0x02, 0xff };
            buf[0] = CamId;
            return buf;
        }
        public static byte[] focusMF(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x38, 0x03, 0xff };
            buf[0] = CamId;
            return buf;
        }
        public static byte[] focusToggle(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x38, 0x10, 0xff };
            buf[0] = CamId;
            return buf;
        }
        /* public static byte[] focusOnePush(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x18, 0x01, 0xff };
            buf[0] = CamId;
            return buf;
        } */
        public static byte[] whiteAuto(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x35, 0x00, 0xff };
            buf[0] = CamId;
            return buf;
        }

        public static byte[] whiteIndoor(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x35, 0x01, 0xff };
            buf[0] = CamId;
            return buf;
        }

        public static byte[] whiteOutdoor(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x35, 0x02, 0xff };
            buf[0] = CamId;
            return buf;
        }

        public static byte[] whiteOnePush(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x35, 0x03, 0xff };
            buf[0] = CamId;
            return buf;
        }

        public static byte[] whiteManual(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x35, 0x05, 0xff };
            buf[0] = CamId;
            return buf;
        }

        public static byte[] whiteTrigOnePush(  )
        {
            byte[] buf = { 0x80, 0x01, 0x04, 0x10, 0x05, 0xff };
            buf[0] = CamId;
            return buf;
        }

        #region panTilt
        //Interpretationslogik außerhalb dieser Library
        public static byte[] moveStop(  )
        {                       
            byte[] buf = { 0x80, 0x01, 0x06, 0x01,0x01,0x01,0x03,0x03, 0xff };
            buf[0] = CamId;
            buf[4]=0x01;//pan speed low
            buf[5]=0x01;//tilt speed low
            return buf;
        }
                public static byte[] panRight( int speed=1 )
        {                 
            byte[] buf = { 0x80, 0x01, 0x06, 0x01,0x01,0x01,0x02,0x03, 0xff };
            buf[0] = CamId;
            buf[4]=(byte)speed;//pan speed 
            buf[5]=0x01;//tilt speed low
            return buf;
        }
                  public static byte[] panLeft( int speed=1 )
        {                 
            byte[] buf = { 0x80, 0x01, 0x06, 0x01,0x01,0x01,0x01,0x03, 0xff };
            buf[0] = CamId;
            buf[4]=(byte)speed;//pan speed 
            buf[5]=0x01;//tilt speed low
            return buf;
        }
    #endregion
    #region inquire
    
    public static byte[] INQ_zoom(  )
        {
            byte[] buf = { 0x80, 0x09, 0x04, 0x47, 0xff };
            buf[0] = CamId;
            return buf;
        }




        #endregion
    }
}