using System;
using System.Threading;
using System.Text;

namespace TestServer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			RfbTcpServer myTestServer = new RfbTcpServer();

			Thread.Sleep(1000);

			string passwd = "pass1";

			RfbClient myTest = new RfbTcpClient(passwd);
			myTest.runProcessMessage();


		}
	}
}
