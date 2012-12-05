using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;	
using System.IO;

namespace TestServer
{

#region Pixel_Format
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

#endregion

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
				RfbMessageHandler mh = new RfbTcpMessageHandler(client);
				mh.StartProcessMessage();
               }
		  }
		}



}

