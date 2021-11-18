using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Org.Mentalis.Security.Cryptography;

public class Hash {
	static void Main(string[] args) {
		if (args.Length == 0 || args.Length > 1) {
			Console.WriteLine("\r\nUsage: hash [filename]");
			return;
		}
		HashAlgorithm[] hashes = new HashAlgorithm[] {
				new MD2CryptoServiceProvider(), new MD5CryptoServiceProvider(),
				new SHA1CryptoServiceProvider(), new RIPEMD160Managed()
			};
		string[] names = new string[] {"MD2:       ", "MD5:       ", "SHA1:      ", "RIPEMD160: "};
		byte[] buffer = new byte[4096];
		FileStream fs = File.Open(args[0], FileMode.Open, FileAccess.Read, FileShare.Read);
		int size = fs.Read(buffer, 0, buffer.Length);
		while(size > 0) {
			for(int i = 0;  i < hashes.Length; i++) {
				hashes[i].TransformBlock(buffer, 0, size, buffer, 0);
			}
			size = fs.Read(buffer, 0, buffer.Length);
		}
		for(int i = 0;  i < hashes.Length; i++) {
			hashes[i].TransformFinalBlock(buffer, 0, 0);
			Console.WriteLine(names[i] + BytesToHex(hashes[i].Hash));
			hashes[i].Clear();
		}
		fs.Close();
	}
	public static string BytesToHex(byte[] bytes) {
		StringBuilder sb = new StringBuilder(bytes.Length * 2);
		for(int i = 0; i < bytes.Length; i++) {
			sb.Append(bytes[i].ToString("X2"));
		}
		return sb.ToString();
	}
}
