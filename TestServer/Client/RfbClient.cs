using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace TestServer
{
	enum returnVal{Error = 0,Success = 1,InitFail=2,};

	public class RfbClient
	{
		public delegate int ProcessMessage();

		protected NetworkStream ns;
		protected bool done;
		private ProcessMessage pmHandler;

		public ProcessMessage PMHandler
		{
			set{ pmHandler = value;}
			get{ return pmHandler;}
		}

		public readonly byte[] versionMsg = Encoding.ASCII.GetBytes("RFB 003.008\n");

		public RfbClient ()
		{
			done = false;
		}

		public void runProcessMessage()
		{
			pmHandler();
		}

	}
}

