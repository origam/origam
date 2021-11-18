using System;
using System.Text;
using System.Security.Cryptography;
using Org.Mentalis.Security.Certificates;

/// <summary>
/// This example shows how to use the public and private key from a certificate to encrypt and decrypt data.
/// </summary>
class AsymmetricCryptography {
	static void Main(string[] args) {
		Console.WriteLine("This example shows how to use the public and private key from a certificate to encrypt and decrypt data.\r\n");
		byte[] data = Encoding.ASCII.GetBytes("Hello World!");
		// load the certificate from a file
		Certificate cert = Certificate.CreateFromCerFile(@"client.cer");

		// get an RSA instance that represents the public key of the certificate
		RSA public_key = cert.PublicKey;
		// create a PKCS#1 key exchange formatter instance with the public key
		RSAPKCS1KeyExchangeFormatter kef = new RSAPKCS1KeyExchangeFormatter(public_key);
		// encrypt the data, using the public key from the certificate
		byte[] encrypted = kef.CreateKeyExchange(data);

		// associate the certificate with its private key
		// we set exportable to true because decryption will fail on Windows 98
		// if this flag is not set. If you do not use Windows 98, you should set
		// the exportable flag to false for increased security.
		cert.AssociateWithPrivateKey(@"client.pvk", "test", true);
		// get an RSA instance that represents the private key
		RSA private_key = cert.PrivateKey;
		// create a PKCS#1 key exchange deformatter instance with the private key
		RSAPKCS1KeyExchangeDeformatter ked = new RSAPKCS1KeyExchangeDeformatter(private_key);
		// decrypt the data, using the private key from the certificate
		byte[] decrypted = ked.DecryptKeyExchange(encrypted);
		
		// print the results in the console
		Console.WriteLine("Input data: " + Encoding.ASCII.GetString(data) + "\r\n");
		Console.WriteLine("Encrypted data:\r\n" + BytesToHex(encrypted) + "\r\n");
		Console.WriteLine("Decrypted data: " + Encoding.ASCII.GetString(decrypted));
		Console.WriteLine("\r\nPress ENTER to continue...");
		Console.ReadLine();

		// clean up
		public_key.Clear();
		private_key.Clear();
	}
	// converts a byte array to its hexadecimal representation
	public static string BytesToHex(byte[] array) {
		StringBuilder sb = new StringBuilder(array.Length * 2);
		for(int i = 0; i < array.Length; i++) {
			sb.Append(array[i].ToString("X2"));
		}
		return sb.ToString();
	}
}