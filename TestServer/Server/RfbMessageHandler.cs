using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;	
using System.Text;	
using System.IO;

namespace TestServer
{
	   public delegate int Command();

	#region ServerInitMessage
	struct ServerInitMessage
	{
		public ushort width;
		public ushort height;
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
			
			// name length
			Buffer.BlockCopy(BitConverter.GetBytes(name.Length),0,msg,20,4);
			
			// name
			Buffer.BlockCopy(name,0,msg,24,name.Length);
			
			return msg;
		}
	};
#endregion

	   public class RfbMessageHandler: RfbObject
	   {

		bool done = false;

		private Command rfbVersionInfo;
		private Command rfbSecurityType;
		private Command rfbLogin;
		private Command rfbServerInit;
		private Command rfbIdle;

		private Command rfbCheckFile;
		private Command rfbWriteFile;
		private Command rfbCopyFile;
		private Command rfbGetSyncFolders;
		private Command rfbSetSyncFolders;
		private Command rfbDeleteFile;

/*
		//S2C message handler
		public abstract bool RfbFBUpdate();
		public abstract bool RfbSetColorMapEntries();
		public abstract bool RfbBell();
		public abstract bool RfbServerCutText();
*/

		public Command RfbCheckFile {
			get{ return rfbCheckFile;}
			set{ rfbCheckFile = value;}
		}
		public Command RfbWriteFile {
			get{ return rfbWriteFile;}
			set{ rfbWriteFile = value;}
		}
		public Command RfbCopyFile {
			get{ return rfbCopyFile;}
			set{ rfbCopyFile = value;}
		}
		public Command RfbDeleteFile {
			get{ return rfbDeleteFile;}
			set{ rfbDeleteFile = value;}
		}
		public Command RfbGetSyncFolders {
			get{ return rfbGetSyncFolders;}
			set{ rfbGetSyncFolders = value;}
		}
		public Command RfbSetSyncFolders {
			get{ return rfbSetSyncFolders;}
			set{ rfbSetSyncFolders = value;}
		}
		public Command RfbVersionInfo {
			get {return rfbVersionInfo;}
			set{rfbVersionInfo = value;} 
		}
		public Command RfbSecurityType {
			get{return rfbSecurityType;}
			set{rfbSecurityType = value;}
		}
		public Command RfbLogin {
			get{ return rfbLogin;} 
			set{rfbLogin = value;} 
		} 
		public Command RfbServerInit { 
			get{ return rfbServerInit;}
			set{rfbServerInit = value;} 
		}
		public Command RfbIdle { 
			get{ return rfbIdle;}
			set{rfbIdle = value;} 
		}

		 public RfbMessageHandler ()
		 {
		 }


		public void StartProcessMessage()
		{
			State nextState = State.VersionInfo;

			done = false;

			while(!done)
			{
				switch(nextState)
				{
				case State.Idle:
					switch((CmdList)rfbIdle())
					{
					case CmdList.Idle:
						Thread.Sleep(1000);
						nextState = State.VersionInfo;
						break;
					case CmdList.GetSyncFolders:
						nextState = State.GetSyncFolders;
						break;
					case CmdList.SetSyncFolders:
						nextState = State.SetSyncFolders;
						break;
					case CmdList.CheckFile:
						nextState = State.CheckFile;
						break;
					case CmdList.CopyFile:
						nextState = State.CopyFile;
						break;
					case CmdList.DeleteFile:
						nextState = State.DeleteFile;
						break;
					case CmdList.WriteFile:
						nextState = State.WriteFile;
						break;
					default:
						break;
					}
					break;
				case State.VersionInfo:
					if(Convert.ToBoolean(rfbVersionInfo()))
						nextState = State.Security;
					else
						nextState = State.Idle;
				break;
				case State.Security:
					if(Convert.ToBoolean(rfbSecurityType()))
					nextState = State.Login;
					else
						nextState = State.Idle;
					break;
				case State.Login:
					if(Convert.ToBoolean(rfbLogin()))  // from Client
						nextState = State.ServerInit;
					else
						nextState = State.Idle;
					break;

				case State.ServerInit:
					if(Convert.ToBoolean(rfbServerInit()))
					nextState = State.Idle;
					else
						nextState = State.Idle;
					break;
				case State.GetSyncFolders:
					
					break;
				case State.SetSyncFolders:

					nextState = State.Idle;
					break;
				case State.CheckFile:

					nextState = State.Idle;					
					break;
				case State.CopyFile:

					nextState = State.Idle;
					break;
				case State.DeleteFile:

					nextState = State.Idle;
					break;
				case State.WriteFile:
				default:
					nextState = State.Idle;
					break;
				}
			}
		}

		private Version checkVersion (ref byte[] cv)
		{
			Version ret = Version.Invalid;
			
			if (cv [6] != '3')
				return Version.Invalid;
			
			if (cv [10] == '3')
				ret = Version.v33;
			else if (cv [10] == '7')
				ret = Version.v37;
			else if (cv [10] == '8')
				ret = Version.v38;
			
			if (serverVersion <= ret) {
				return ret;
			}
			
			return Version.Invalid;
			
		}
	   }
}

