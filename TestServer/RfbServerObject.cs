using System;
using System.Net;

namespace TestServer
{

	struct ServerInitMessage
	{
		public ushort width;
		public ushort height;
		public Pixel_Format pf;
		public int name_len;
		public byte[] name;
		
		public void setResolution(ushort wi,ushort he)
		{
			width = wi;
			height = he;
		}
		public void setPixelFormat()
		{
		}
		
		public void setName(byte[] nm)
		{
			name = nm;
			name_len = name.Length;
		}
		
		public byte[] getBytes()
		{
			if(name_len == 0)
				return null;
			
			byte[] msg = new byte[24+name_len];
			
			// width
			Buffer.BlockCopy(BitConverter.GetBytes(width) ,0,msg,0,2);
			
			// height
			Buffer.BlockCopy(BitConverter.GetBytes(height),0,msg,2,2);
			
			//pixel format
			msg[4] = pf.bpp;        // bpp
			msg[5] = pf.depth;      // depth
			msg[6] = pf.bigEndian;  // big endian
			msg[7] = pf.trueColour; // true colour
			
			// redMax
			Buffer.BlockCopy(BitConverter.GetBytes(pf.redMax),0,msg,8,2);
			
			// greenMax
			Buffer.BlockCopy(BitConverter.GetBytes(pf.greenMax),0,msg,10,2);
			
			// blueMax
			Buffer.BlockCopy(BitConverter.GetBytes(pf.blueMax),0,msg,12,2);
			
			msg[14] = pf.redShift;   // redShift
			msg[15] = pf.greenShift; // greenShift
			msg[16] = pf.blueShift;  // blueShift
			
			// padding
			Buffer.BlockCopy(BitConverter.GetBytes(pf.padding),0,msg,17,3);
			
			// name length
			Buffer.BlockCopy(BitConverter.GetBytes(name.Length),0,msg,20,4);
			
			// name
			Buffer.BlockCopy(name,0,msg,24,name.Length);
			
			return msg;
		}
	};

	public abstract class RfbServerObject : RfbObject
	   {
		public RfbServerObject ()
		{

		}

		public abstract void RfbSetPixelFormatHandler(Func<Object,bool> handler);
		public virtual void RfbSetEncodingHandler (Func<object,bool> handler)
		{

		}
		public virtual void RfbFBUpdateRequestHandler(Func<object,bool> handler)
		{

		}
		public virtual void RfbKeyEventHandler(Func<object,bool> handler)
		{

		}
		public virtual void RfbPointerEventHandler(Func<object,bool> handler)
		{

		}
		public virtual void RfbClientCutTextHandler(Func<object,bool> handler)
		{

		}

		//S2C message handler
		public abstract bool RfbFBUpdate();
		public abstract bool RfbSetColorMapEntries();
		public abstract bool RfbBell();
		public abstract bool RfbServerCutText();

	   }
}

