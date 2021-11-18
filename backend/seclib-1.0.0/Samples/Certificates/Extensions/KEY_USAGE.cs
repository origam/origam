using System;
using System.Runtime.InteropServices;

public struct KEY_USAGE { // see MSDN docs for more info
	public KEY_USAGE(IntPtr buffer, int size) { // mandatory constructor
		this.cbData = Marshal.ReadInt32(buffer);
		this.pbData = new byte[this.cbData];
		Marshal.Copy(Marshal.ReadIntPtr(buffer, 4), this.pbData, 0, this.cbData);
	}
	public override string ToString() {
		string ret = "Key Usage:\r\n";
		if (this.cbData == 0) {
			ret += "  No Restrictions\r\n";
		} else {
			for(int i = 0; i < m_Usage.Length; i++) {
				if ((m_Usage[i] & this.pbData[0]) != 0) {
					ret += "  " + m_UsageName[i] + "\r\n";
				}
			}
		}
		return ret;
	}
	public int cbData;
	public byte[] pbData;
	private static int[] m_Usage = new int[]{CERT_DIGITAL_SIGNATURE_KEY_USAGE, CERT_NON_REPUDIATION_KEY_USAGE, CERT_KEY_ENCIPHERMENT_KEY_USAGE, CERT_DATA_ENCIPHERMENT_KEY_USAGE, CERT_KEY_AGREEMENT_KEY_USAGE, CERT_KEY_CERT_SIGN_KEY_USAGE, CERT_OFFLINE_CRL_SIGN_KEY_USAGE};
	private static string[] m_UsageName = new string[]{"Digital Signature", "Non-Repudiation", "Key Encipherment", "Data Encipherment", "Key Agreement", "Digital Certificate Signature", "Digital CRL Signature"};
	private const int CERT_DIGITAL_SIGNATURE_KEY_USAGE = 0x80;
	private const int CERT_NON_REPUDIATION_KEY_USAGE = 0x40;
	private const int CERT_KEY_ENCIPHERMENT_KEY_USAGE = 0x20;
	private const int CERT_DATA_ENCIPHERMENT_KEY_USAGE = 0x10;
	private const int CERT_KEY_AGREEMENT_KEY_USAGE = 0x08;
	private const int CERT_KEY_CERT_SIGN_KEY_USAGE = 0x04;
	private const int CERT_OFFLINE_CRL_SIGN_KEY_USAGE = 0x02;
}
