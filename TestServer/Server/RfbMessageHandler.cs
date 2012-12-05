using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;	
using System.Text;	
using System.IO;

namespace TestServer
{
	   public delegate bool Command();

	#region ServerInitMessage
	struct ServerInitMessage
	{
		public ushort width;
		public ushort height;
		public Pixel_Format pf;
		public int name_len;
		public byte[] name;
		
		public void setResolution(ushort wi,ushort he)
		{
			width = wi;
			height = he;
		}
		public void setPixelFormat()
		{
		}
		
		public void setName(byte[] nm)
		{
			name = nm;
			name_len = name.Length;
		}
		
		public byte[] getBytes()
		{
			if(name_len == 0)
				return null;
			
			byte[] msg = new byte[24+name_len];
			
			// width
			Buffer.BlockCopy(BitConverter.GetBytes(width) ,0,msg,0,2);
			
			// height
			Buffer.BlockCopy(BitConverter.GetBytes(height),0,msg,2,2);
			
			//pixel format
			msg[4] = pf.bpp;        // bpp
			msg[5] = pf.depth;      // depth
			msg[6] = pf.bigEndian;  // big endian
			msg[7] = pf.trueColour; // true colour
			
			// redMax
			Buffer.BlockCopy(BitConverter.GetBytes(pf.redMax),0,msg,8,2);
			
			// greenMax
			Buffer.BlockCopy(BitConverter.GetBytes(pf.greenMax),0,msg,10,2);
			
			// blueMax
			Buffer.BlockCopy(BitConverter.GetBytes(pf.blueMax),0,msg,12,2);
			
			msg[14] = pf.redShift;   // redShift
			msg[15] = pf.greenShift; // greenShift
			msg[16] = pf.blueShift;  // blueShift
			
			// padding
			Buffer.BlockCopy(BitConverter.GetBytes(pf.padding),0,msg,17,3);
			
			// name length
			Buffer.BlockCopy(BitConverter.GetBytes(name.Length),0,msg,20,4);
			
			// name
			Buffer.BlockCopy(name,0,msg,24,name.Length);
			
			return msg;
		}
	};
#endregion

	   public class RfbMessageHandler: RfbObject
	   {

		bool done = false;

		private Command rfbVersionInfo;
		private Command rfbSecurityType;
		private Command rfbClientInit;
		private Command rfbServerInit;

/*
		//S2C message handler
		public abstract bool RfbFBUpdate();
		public abstract bool RfbSetColorMapEntries();
		public abstract bool RfbBell();
		public abstract bool RfbServerCutText();
*/

		public Command RfbVersionInfo {
			get {return rfbVersionInfo;}
			set{rfbVersionInfo = value;} 
		}
		public Command RfbSecurityType {
			get{return rfbSecurityType;}
			set{rfbSecurityType = value;}
		}
		public Command RfbClientInit {
			get{ return rfbClientInit;} 
			set{rfbClientInit = value;} 
		} 
		public Command RfbServerInit { 
			get{ return rfbServerInit;}
			set{rfbServerInit = value;} }

		 public RfbMessageHandler ()
		 {
		 }


		public void StartProcessMessage()
		{
			State nextState = State.VersionInfo;

			done = false;

			while(!done)
			{
				switch(nextState)
				{
				case State.Idle:
					Thread.Sleep(2000);
					nextState = State.VersionInfo;
					break;
				case State.VersionInfo:
					if(rfbVersionInfo())
						nextState = State.Security;
					else
						nextState = State.Idle;
				break;
				case State.Security:
					if(rfbSecurityType())
					nextState = State.ServerInit;
					else
						nextState = State.Idle;
					break;
				case State.ServerInit:
					if(rfbServerInit())
					nextState = State.ClientInit;
					else
						nextState = State.Idle;
					break;
				case State.ClientInit:
					if(rfbClientInit())
						nextState = State.Idle;
					else
						nextState = State.Idle;
					break;
				default:
					nextState = State.Idle;
					break;
				}
			}
		}

		private Version checkVersion (ref byte[] cv)
		{
			Version ret = Version.Invalid;
			
			if (cv [6] != '3')
				return Version.Invalid;
			
			if (cv [10] == '3')
				ret = Version.v33;
			else if (cv [10] == '7')
				ret = Version.v37;
			else if (cv [10] == '8')
				ret = Version.v38;
			
			if (serverVersion <= ret) {
				return ret;
			}
			
			return Version.Invalid;
			
		}
	   }
}

