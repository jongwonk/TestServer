using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
	
	
namespace TestServer
{
	public class TestServer
	{
		
		private TcpListener tcpListener;
		private Thread				 listenerThread;
		
		public TestServer()
		{
				tcpListener = new TcpListener(IPAddress.Any,2999);
			
			  listenerThread = new Thread(new ThreadStart(listenForClients));
				  
				 listenerThread.Start();
		}
	
    private void listenForClients()
            {
       tcpListener.Start();
				
    		while(true)
	    			{
	    			TcpClient client = tcpListener.AcceptTcpClient();

						clientHandler ch = new clientHandler(client);
		
	               }
		  }
	}
	
	
	public class clientHandler
	{
		
		private List<byte>rBuffer = new List<byte>();
		private readonly Object rBufLock = new Object();
		private TcpClient tcpClient = null;
		private bool done = false;	
		private NetworkStream clientStream;
		private int bytesRead;
				
		private clientHandler()
		{

		}

		public clientHandler(TcpClient tc)
		{
			setClient(tc);
			
			Thread thread = new Thread(new ThreadStart(clientHandlerHelper));
			thread.Start ();
			
			thread.Join ();
		}

		public void setClient(TcpClient tc)
		{
			tcpClient = tc;	
			clientStream = tcpClient.GetStream();
		}
		
		public void clientHandlerHelper()
		{

			rBuffer.Clear();
			byte[] msg = new byte[4096];
			
			while(!done)
			{
				bytesRead = 0;
				
				try{
					bytesRead += clientStream.Read(msg, 0, 4096);
				}
				catch{
					break;	
				}
				
				if(bytesRead > 0)
				{
					lock(rBufLock)
					{
						rBuffer.AddRange(msg);

						try
						{
							 lock(rBufLock)
							  {
							   Monitor.Pulse(tcpClient);
							  }
						}
						catch(SynchronizationLockException ex)
						{
							Console.WriteLine("{0}",ex.Message);
						}
					}
				}
			}
			
			tcpClient.Close();
		}
		
		private byte[] readMsg(int len)
		{

			while(bytesRead < len)
			{
				try
				{
					lock(rBufLock)
					{
						Monitor.Wait(tcpClient,5000);
					}
				}
				catch(SynchronizationLockException ex)
				{
					Console.WriteLine("{0}",ex.Message);
				}
				catch(ThreadInterruptedException ex)
				{
					Console.WriteLine("{0}",ex.Message);
				}
			}
			
			return rBuffer.ToArray();
			
		}		
		
	}
	
}

