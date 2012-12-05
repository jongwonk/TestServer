using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;	
using System.IO;

namespace TestServer
{
	   public class RfbTcpMessageHandler : RfbMessageHandler 
	   {
		private readonly byte[] versionMsg = Encoding.ASCII.GetBytes("RFB 003.008\n");

		private ServerInitMessage ServerInitMsg;

		private TcpClient tcpClinet;
		private NetworkStream netStream;


		 public RfbTcpMessageHandler(TcpClient tcpclient)
		 {
			tcpClinet = tcpclient;
			netStream = tcpclient.GetStream();

			setTcpMessageHandler();
		 }

		void setTcpMessageHandler()
		{
			RfbVersionInfo = new Command(TcpVersionInfo);
			RfbSecurityType = new Command(TcpSecurityType);
			RfbServerInit = new Command(TcpServerInit);
			RfbClientInit = new Command(TcpClientInit);
		}

		private bool TcpVersionInfo()
		{
#if DEBUG
			Console.WriteLine("entered TcpVersionInfo");
#endif
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

		private bool TcpSecurityType()
		{
#if DEBUG
			Console.WriteLine("entered TcpSecurityType");
#endif
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

		private bool TcpServerInit()
		{
#if DEBUG
			Console.WriteLine("entered TcpServerInit");
#endif
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

		private bool TcpClientInit()
		{
#if DEBUG
			Console.WriteLine("entered TcpClientInit");
#endif
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

		private byte[] supportedSecurityType()
		{
			byte[] ret = new byte[2];
			
			ret[0] = 0x1; // # of security typres
			ret[1] = 0x1; // security types
			return ret;
		}

	   }
}

