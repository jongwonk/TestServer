using System;

namespace TestServer
{
	   public class RfbClientObject
	   {
			 public RfbClientObject ()
			 {
			 }

			//C2S message handler
			public virtual bool RfbSetPixelFormat()
			{
				return false;
			}
			public virtual bool RfbSetEncoding()
			{
				return false;
			}
			public virtual bool RfbFBUpdateRequest()
			{
				return false;
			}
			public virtual bool RfbKeyEvent()
			{
				return false;
			}
			public virtual bool RfbPointerEvent()
			{
				return false;
			}
			public virtual bool RfbClientCutText()
			{
				return false;
			}

			public delegate void RfbFBUpdateHandler();
			public delegate void RfbSetColorMapEntriesHandler();
			public delegate void RfbBellHandler();
			public delegate void RfbServerCutTextHandler();

	   }
}

