using System;

namespace TestServer
{
	  public class RfbObject
	   {
		public enum State{Idle=0,VersionInfo=1,Security=2,Login=3,ServerInit=4,
			GetSyncFolders,SetSyncFolders,CheckFile, WriteFile, DeleteFile, CopyFile, }
		public enum Version{Invalid=0,v33=1,v37=2, v38=3,};
		public enum SecurityType{Invalid=0,None=1,VNC,RA2=5,RA2ne=6,Tight=16,Ultra=17,TLS=18,VeNCryptGTK,MD5,Colin};
		public enum ExclusiveAccess{True=0, False = 1,};
		public enum C2SMsg{SetPixelFormat=0,SetEncodings=2,FBUpdateRequest=3,KeyEvent=4,PointerEvent=5,ClientCutText=6, };

		public enum CmdList 	{ Idle=0,GetSyncFolders=1, SetSyncFolders,
															CheckFile, WriteFile, DeleteFile, CopyFile, 	}

		protected ExclusiveAccess exAccess;
		protected Version 		 serverVersion;
		protected SecurityType 	 securityType;

		 public RfbObject ()
		 {
		 }

	   }
}

