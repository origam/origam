using System;
using System.Runtime.InteropServices;

public struct CERT_AUTHORITY_INFO_ACCESS { // see MSDN docs for more info
	public CERT_AUTHORITY_INFO_ACCESS(IntPtr buffer, int size) { // mandatory constructor
		this.cAccDescr = Marshal.ReadInt32(buffer);
		this.rgAccDescr = new CERT_ACCESS_DESCRIPTION[cAccDescr];
		IntPtr start = Marshal.ReadIntPtr(buffer, 4);
		int offset = 0;
		for(int i = 0; i < rgAccDescr.Length; i++) {
			this.rgAccDescr[i] = new CERT_ACCESS_DESCRIPTION(start, offset);
			offset += 2 * IntPtr.Size + 8;
		}
	}
	public override string ToString() {
		string ret = "Authority Information Access:\r\n";
		for(int i = 0; i < this.cAccDescr; i++) {
			ret += "  Access Method=" + this.rgAccDescr[i].pszAccessMethod + "\r\n";
			ret += "  Alternative Name:\r\n";
			ret += "    " + this.rgAccDescr[i].AccessLocation.ToString() + "\r\n";
		}
		if (this.cAccDescr == 0)
			ret += " NO DATA\r\n";
		return ret;
	}
	public int cAccDescr;
	public CERT_ACCESS_DESCRIPTION[] rgAccDescr;
}

public struct CERT_ACCESS_DESCRIPTION { // see MSDN docs for more info
	public CERT_ACCESS_DESCRIPTION(IntPtr buffer, int offset) {
		IntPtr str = Marshal.ReadIntPtr(buffer, offset);
		this.pszAccessMethod = Marshal.PtrToStringAnsi(str);
		this.AccessLocation = new CERT_ALT_NAME_ENTRY(buffer, offset + IntPtr.Size);
	}
	public string pszAccessMethod;
	public CERT_ALT_NAME_ENTRY AccessLocation;
}

public struct CERT_ALT_NAME_ENTRY { // see MSDN docs for more info
	public CERT_ALT_NAME_ENTRY(IntPtr buffer, int offset) {
		this.dwAltNameChoice = Marshal.ReadInt32(buffer, offset);
		switch(this.dwAltNameChoice) {
			case CERT_ALT_NAME_RFC822_NAME:
			case CERT_ALT_NAME_DNS_NAME:
			case CERT_ALT_NAME_URL:
				this.unionObject = Marshal.PtrToStringUni(Marshal.ReadIntPtr(buffer, offset + 4));
				break;
			case CERT_ALT_NAME_REGISTERED_ID:
				this.unionObject = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(buffer, offset + 4));
				break;
			default: //TODO
				this.unionObject = null;
				break;
		}
	}
	public override string ToString() {
		if (unionObject != null)
			return (string)unionObject;
		return "[OCTET STRING]";
	}
	public int dwAltNameChoice;
	public object unionObject;
	private const int CERT_ALT_NAME_OTHER_NAME = 1;
	private const int CERT_ALT_NAME_RFC822_NAME = 2;
	private const int CERT_ALT_NAME_DNS_NAME = 3;
	private const int CERT_ALT_NAME_X400_ADDRESS = 4;
	private const int CERT_ALT_NAME_DIRECTORY_NAME = 5;
	private const int CERT_ALT_NAME_EDI_PARTY_NAME = 6;
	private const int CERT_ALT_NAME_URL = 7;
	private const int CERT_ALT_NAME_IP_ADDRESS = 8;
	private const int CERT_ALT_NAME_REGISTERED_ID = 9;
}