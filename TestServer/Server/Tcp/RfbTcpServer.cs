using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
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
				Thread thread = new Thread(new ParameterizedThreadStart(clientHandler));
                                    thread.Start(client);
               }
		  }

		private void clientHandler(object obj)
		{
			RfbMessageHandler mh = new RfbTcpServerMessageHandler((TcpClient)obj);
			mh.StartProcessMessage();
		}


		}

	}

