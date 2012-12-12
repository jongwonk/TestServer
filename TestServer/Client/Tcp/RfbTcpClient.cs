using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Security.Cryptography;


namespace TestServer
{
	   public class RfbTcpClient : RfbClient
	   {

			private TcpClient tclient;
			private byte[] ctBuffer;
			private int rCnt;
			private byte[] _passwd;		

			byte[] Password {	
				set{ _passwd = SHA256Managed.Create().ComputeHash (value);}
				get{ return _passwd;}
			}

			 public RfbTcpClient ()
			 {
				 PMHandler = new ProcessMessage ( ProcessMessageHandler );
			 }
			
			public RfbTcpClient (string passwd)
			 {
				Password = ASCIIEncoding.ASCII.GetBytes(passwd);				
				PMHandler = new ProcessMessage ( ProcessMessageHandler );
			}

		private int ProcessMessageHandler()
			{
				try {

					tclient = new TcpClient ("localhost",4999);
					ns = tclient.GetStream ();

				ctBuffer = new byte[versionMsg.Length];

					rCnt = ns.Read (ctBuffer, 0, versionMsg.Length);
					Console.WriteLine (Encoding.ASCII.GetString(ctBuffer, 0, rCnt));
					ns.Write(versionMsg,0,versionMsg.Length);


					rCnt = ns.Read (ctBuffer, 0, 1);
					rCnt = ns.Read (ctBuffer, 0, (int)ctBuffer[0]);
					ctBuffer[0] = 0x1;
					ns.Write(ctBuffer,0,1); // security type
					
					// security result
					rCnt = ns.Read(ctBuffer,0,4);
					int SecurityResult = BitConverter.ToInt32(ctBuffer,0);
					
					// login			
					ns.Write(Password,0,Password.Length/*32- SHA256*/);
					
					if(ns.ReadByte() == 0x0)
					{
#if DEBUG
						Console.WriteLine("Login success");
#endif
					}
				else{
#if DEBUG
					Console.WriteLine("Logi failed");
#endif
				}
					
					// serverinit
					
				} catch (SocketException ex) {
					Console.WriteLine (ex.Message);
					return (int)returnVal.Error;
				} catch (Exception ex) {
					Console.WriteLine (ex.Message);
					return (int)returnVal.Error;
				}
				
				if (rCnt > 0) {
					
				}
				while(!done)
				{
					
					Thread.Sleep (1000);
				}
				
				return (int)returnVal.Success;
			}

	   }
}

