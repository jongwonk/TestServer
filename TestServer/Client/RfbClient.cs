using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;


namespace TestServer
{
	enum returnVal{Error = 0,Success = 1,InitFail=2,};

	public class RfbClient
	{
		private TcpClient tclient;
		private NetworkStream ns;
		private bool done;

		private readonly byte[] versionMsg = Encoding.ASCII.GetBytes("RFB 003.008\n");

		private byte[] ctBuffer;
		private int rCnt;

		public RfbClient ()
		{
			ctBuffer = new byte[13];
			done = false;

			runClient();
		}

		public int runClient ()
		{
			try {
				tclient = new TcpClient ("localhost",4999);
				ns = tclient.GetStream ();
				rCnt = ns.Read (ctBuffer, 0, versionMsg.Length);
				Console.WriteLine (Encoding.ASCII.GetString(ctBuffer, 0, rCnt));
				ns.Write(versionMsg,0,versionMsg.Length);

				rCnt = ns.Read (ctBuffer, 0, 1);
				rCnt = ns.Read (ctBuffer, 0, (int)ctBuffer[0]);
				ctBuffer[0] = 0x1;
				ns.Write(ctBuffer,0,1); // security type


				// secirity result
				rCnt = ns.Read(ctBuffer,0,4);
				int SecurityResult = BitConverter.ToInt32(ctBuffer,0);

				// clientInit
				ctBuffer[0] = 0x0;
				ns.Write(ctBuffer,0,1);

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

