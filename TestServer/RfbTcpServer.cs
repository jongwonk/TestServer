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
	public class RfbTcpServer : RfbServer
	{
		private TcpListener tcpListener;
		private Thread		listenerThread;
		
		public RfbTcpServer()
		{
			tcpListener = new TcpListener(IPAddress.Any,4999);
			listenerThread = new Thread(new ThreadStart(listenForClients));
			listenerThread.Start();
		}
	
	    private void listenForClients()
            {
       		tcpListener.Start();
				
	    		while(true)
    			{
	    			TcpClient client = tcpListener.AcceptTcpClient();
				TcpServerClientHandler ch = new TcpServerClientHandler(client);
               }
		  }
		}

	struct Pixel_Format
	{
		public byte   bpp;
		public byte   depth;
		public byte   bigEndian;
		public byte   trueColour;
		public ushort redMax;
		public ushort greenMax;
		public ushort blueMax;
		public byte   redShift;
		public byte   greenShift;
		public byte   blueShift;
		public int    padding;
	}



	public class TcpServerClientHandler : RfbServerObject,IStream
	{
		private ServerInitMessage ServerInitMsg;

		private readonly byte[] versionMsg = Encoding.ASCII.GetBytes("RFB 003.008\n");

		private bool done = false;	
		private NetworkStream netStream;



		private TcpServerClientHandler()
		{
			exAccess = ExclusiveAccess.False;
			serverVersion = checkVersion(ref versionMsg);
			securityType = SecurityType.Invalid;
		}

		public TcpServerClientHandler(TcpClient tc)
		{

			netStream = tc.GetStream ();

//			netStream.WriteTimeout = 5000;
//			netStream.ReadTimeout  = 5000;

			exAccess = ExclusiveAccess.False;
			serverVersion = checkVersion(ref versionMsg);
			securityType = SecurityType.Invalid;

			Thread thread = new Thread(new ParameterizedThreadStart(clientHandlerHelper));
			thread.Start ((object)tc);
			
			thread.Join ();
		}

		public NetworkStream netStrm {
			get{
				return netStream;
			}
			set{
				netStream = value;
			}
		}

		public void clientHandlerHelper (object tc)
		 {
		    State nextState = State.VersionInfo;

			while(!done)
			{
				switch(nextState)
				{
				case State.VersionInfo:
					if (RfbVersionInfo ()) {
						nextState = State.Security;
					} 
					else {
						nextState = State.VersionInfo;
					}
					break;
				case State.Security:
					if(RfbSecirityType()){
						nextState = State.ClientInit;
					}
					else
					{
						nextState = State.VersionInfo;
					}
					break;
				case State.ClientInit:
					if(RfbClientInit()){
						nextState = State.ServerInit;
					}
					else{
						nextState = State.VersionInfo;
					}
					break;
				case State.ServerInit:
					if(RfbServerInit()){
						nextState = State.WaitMsg;
					}
					else{
						nextState = State.VersionInfo;
					}
					break;
				case State.WaitMsg:
					// RfbSetPixelFormatHandler(new Action());
					break;
				default:
					nextState = State.VersionInfo;
					break;
				}
			}
			
			((TcpClient)tc).Close();
		}

		private byte[] supportedSecurityType()
		{
			byte[] ret = new byte[2];

			ret[0] = 0x1; // # of security typres
			ret[1] = 0x1; // security types
			return ret;
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

		public override bool RfbVersionInfo ()
		{
			int cnt;
			byte[] msg = new byte[13];

			try{
				netStream.Write(versionMsg ,0 ,versionMsg.Length ); // sending version info to Client
				cnt = netStream.Read(msg, 0, versionMsg.Length); // received version info from Client
			}
			catch(Exception ex){
				Console.WriteLine ( ex.Message );
				return false;
			}

			if(checkVersion(ref msg) == Version.Invalid){
				return false;
			}

			return true;
		}

		public override bool RfbSecirityType ()
		 {
			int cnt;
			int reasonLen;
		     byte[] sst = supportedSecurityType ();
			byte[] msg = new byte[16];

		    try {
				 netStream.Write (sst, 0, sst.Length);
				 cnt = netStream.Read (msg, 0, 1);

				 if(cnt>0){
					
					if(msg[0] == 0x0) // connection failed
					{
						cnt = netStream.Read(msg, 0, 4);
						reasonLen =  BitConverter.ToInt32(msg,0);
						cnt = netStream.Read(msg, 0, reasonLen);
						netStream.Write(BitConverter.GetBytes((long)0),0,4 );
						securityType = SecurityType.Invalid;
						return false;

						// nextState = State.VersionInfo;
					}
					else{
						securityType = (SecurityType)msg[0];
						netStream.Write(BitConverter.GetBytes((long)1),0,4);
						//nextState = State.ClientInit;
					}
				 }
		    } 
			catch (Exception ex) {
				Console.WriteLine (ex.Message);
				return false;
		    }

			return true;

		}

		public override bool RfbClientInit ()
		{
			int cnt;
			byte[] msg = new byte[1];

			try{
				cnt = netStream.Read(msg,0,1);
				if(cnt == 1)
				{
					if(msg[0] == 0x0)
						exAccess = ExclusiveAccess.False;
					else
						exAccess = ExclusiveAccess.True;
				}
				else return false;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
			return true;
		}

		public override bool RfbServerInit ()
		{
			byte[] msg;

			ServerInitMsg.setResolution(100,100);
			ServerInitMsg.setPixelFormat();
			ServerInitMsg.setName(System.Text.Encoding.ASCII.GetBytes("fb0") );

			msg = ServerInitMsg.getBytes ();
			try{
				netStream.Write(msg,0,msg.Length);
			}
			catch(Exception ex)
			{
				Console.WriteLine ( ex.Message );
			}

			return true;
		}

		public override void RfbSetPixelFormatHandler (Func<Object,bool> handler)
		{

			throw new System.NotImplementedException ();
		}

		public override  bool RfbFBUpdate ()
		{
			throw new System.NotImplementedException ();
		}

		public override bool RfbSetColorMapEntries ()
		{
			throw new System.NotImplementedException ();
		}

		public override bool RfbBell ()
		{
			throw new System.NotImplementedException ();
		}

		public override bool RfbServerCutText ()
		{
			throw new System.NotImplementedException ();
		}

		public void writeU16(UInt16 u16)
		{
			byte[] byts = BitConverter.GetBytes(u16);

			try{
				netStream.Write (byts,0,2);
			}
			catch(Exception ex){
				Console.WriteLine(ex.Message);
			}
		}

		public void writeU32 (UInt32 u32)
		 {
			byte[] byts = BitConverter.GetBytes(u32);

			try{
				netStream.Write (byts,0,4);
			}
			catch(Exception ex){
				Console.WriteLine(ex.Message);
			}
		 }

		public void writeByte(byte ch)
		{
			try{
				netStream.WriteByte(ch);
			}
			catch(Exception ex){
				Console.WriteLine(ex.Message);
			}
		}

		public void writeBytes(byte[] byts)
		{
			try{
				netStream.Write (byts,0,byts.Length);
			}
			catch(Exception ex){
				Console.WriteLine(ex.Message);
			}
		}

		public void writeBytes(byte[] byts, int len)
		{
			try{
				netStream.Write (byts,0,len);
			}
			catch(Exception ex){
				Console.WriteLine(ex.Message);
			}			
		}

		public int readByte(ref byte ch)
		{
			int ret = 0;

			return ret;
		}

		public int readBytes(ref byte[] byts,int len)
		{
			int ret = 0;
			
			return ret;
		}


	}
	
}

