using System;
using System.Threading;

namespace TestServer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			RfbTcpServer myTestServer = new RfbTcpServer();

			Thread.Sleep(1000);

			RfbClient myTest = new RfbClient();


		}
	}
}
