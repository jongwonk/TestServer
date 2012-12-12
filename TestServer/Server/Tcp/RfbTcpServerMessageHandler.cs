using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;	
using System.IO;
using System.Security.Cryptography;

namespace TestServer
{
	   public class RfbTcpServerMessageHandler : RfbMessageHandler 
	   {
		private readonly byte[] versionMsg = Encoding.ASCII.GetBytes("RFB 003.008\n");
		private readonly int sha256length = 32;
		private ServerInitMessage ServerInitMsg;

		private TcpClient tcpClient;
		private NetworkStream netStream;
		private Dictionary<string,string>Passwords = new Dictionary<string,string>();
		private byte[] sss;

		private void setPasswords ()
		  {
			SHA256 sha256 = SHA256Managed.Create ();

			string tmp = "pass1";
			byte[] tmp1 = ASCIIEncoding.ASCII.GetBytes(tmp);
			Passwords.Add(ASCIIEncoding.ASCII.GetString(sha256.ComputeHash(tmp1)),tmp);

			tmp = "pass2";
			tmp1 = ASCIIEncoding.ASCII.GetBytes(tmp);
			Passwords.Add(ASCIIEncoding.ASCII.GetString(sha256.ComputeHash(tmp1)),tmp);

			tmp = "pass3";
			tmp1 = ASCIIEncoding.ASCII.GetBytes(tmp);
			Passwords.Add(ASCIIEncoding.ASCII.GetString(sha256.ComputeHash(tmp1)),tmp);

		  }

		public RfbTcpServerMessageHandler(TcpClient tcpclient)
		 {
			tcpClient = tcpclient;
			netStream = tcpclient.GetStream();

			setPasswords();
			setTcpMessageHandler();
		 }

		void setTcpMessageHandler()
		{
			RfbVersionInfo = new Command(TcpVersionInfo);
			RfbSecurityType = new Command(TcpSecurityType);
			RfbServerInit = new Command(TcpServerInit);
			RfbLogin = new Command(TcpLogin);
			RfbIdle = new Command(TcpIdle);


		}

		private int TcpIdle()
		{
			int ret = 0;

			try{
				ret = (int)netStream.ReadByte();
			}
			catch(Exception ex)
			{
#if DEBUG
				Console.WriteLine(ex.Message);
#endif
			}

			return ret;
		}

		private int TcpVersionInfo()
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
				return 0;
			}
			
			if(checkVersion(ref msg) == Version.Invalid){
				return 0;
			}

			return 1;
		}

		private int TcpSecurityType()
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
						return 0;
						
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
				return 0;
			}

			return 1;
		}

		private int TcpServerInit()
		{
#if DEBUG
			Console.WriteLine("entered TcpServerInit");
#endif
			return 1;
		}

		private int TcpLogin()
		{
			int ret = 0;

#if DEBUG
			Console.WriteLine("entered TcpLogin");
#endif
			int cnt;
			byte[] msg = new byte[32];
			string msgOut;
			
			try{
				cnt = netStream.Read(msg,0,sha256length);
				if(cnt == sha256length)
				{
					Console.WriteLine ( ASCIIEncoding.ASCII.GetString(msg));

					if(Passwords.TryGetValue(ASCIIEncoding.ASCII.GetString(msg),out msgOut))
					{
						ret = 1;
						Console.WriteLine("Got it");
					}
					else
						ret = 0;
				}
				else 
					ret = 0;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				ret = 0;
			}


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

		private byte[] supportedSecurityType()
		{
			byte[] ret = new byte[2];
			
			ret[0] = 0x1; // # of security typres
			ret[1] = 0x1; // security types
			return ret;
		}

	   }
}

