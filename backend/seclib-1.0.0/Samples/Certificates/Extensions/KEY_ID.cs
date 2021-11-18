using System;
using System.Text;
using System.Runtime.InteropServices;

public struct KEY_ID { // see MSDN docs for more info
	public KEY_ID(IntPtr buffer, int size) { // mandatory constructor
		this.cbData = Marshal.ReadInt32(buffer);
		this.pbData = new byte[this.cbData];
		Marshal.Copy(Marshal.ReadIntPtr(buffer, 4), this.pbData, 0, this.cbData);
	}
	public override string ToString() {
		StringBuilder sb = new StringBuilder();
		sb.Append("Subject Key Identifier:\r\n  ");
		for(int i = 0; i < cbData; i++) {
			sb.Append(this.pbData[i].ToString("x2") + " ");
		}
		sb.Append("\r\n");
		return sb.ToString();
	}
	public int cbData;
	public byte[] pbData;
}
