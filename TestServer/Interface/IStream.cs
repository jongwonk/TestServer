using System;
using System.Net.Sockets;

namespace TestServer
{
	   public interface IStream
	   {
		NetworkStream netStrm{
			get;
			set;
		}

		void writeByte(byte ch);
		void writeBytes(byte[] byts);
		void writeBytes(byte[] byts, int len);
		int  readByte(ref byte ch);
		void writeU16(UInt16 u16);
		void writeU32(UInt32 u32);
	   }
}

