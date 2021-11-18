using System;
using System.Text;
using System.Security.Cryptography;
using Org.Mentalis.Security.Cryptography;

/// <summary>
/// This example shows how to use the symmetric encryption classes from the Security Library.
/// </summary>
class SymmetricCryptography {
	static void Main(string[] args) {
		Console.WriteLine("This example shows how to use the symmetric encryption classes from the Security Library.\r\n");
		SymmetricAlgorithm algorithm;
		ICryptoTransform encryptor, decryptor;
		Console.WriteLine("Select the symmetric algorithm you want to use:");
		Console.WriteLine("  [1] ARCFour [managed RC4]");
		Console.WriteLine("  [2] RC4 [unmanaged]");
		Console.WriteLine("  [3] Rijndael");
		Console.Write("Your choice: ");
		string input = Console.ReadLine().Trim();
		// initialize the selected symmetric algorithm
		switch(input) {
			case "1":
				algorithm = new ARCFourManaged();
				break;
			case "2":
				algorithm = new RC4CryptoServiceProvider();
				break;
			case "3":
				algorithm = new RijndaelCryptoServiceProvider();
				break;
			default:
				Console.WriteLine("Invalid input.");
				return;
		}
		Console.WriteLine("Enter some text that will be encrypted:");
		input = Console.ReadLine();
		byte[] plaintext = Encoding.ASCII.GetBytes(input);
		// generate an IV that consists of bytes with the value zero
		// in real life applications, the IV should not be set to
		// an array of bytes with the value zero!
		algorithm.IV = new byte[algorithm.BlockSize / 8];
		// generate a new key
		algorithm.GenerateKey();
		// create the encryption and decryption objects
		encryptor = algorithm.CreateEncryptor();
		decryptor = algorithm.CreateDecryptor();
		// encrypt the bytes
		byte[] encrypted = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
		// decrypt the encrypted bytes
		byte[] decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
		// write the byte arrays to the console
		Console.WriteLine("\r\nResults:");
		Console.WriteLine("  Input data: " + Encoding.ASCII.GetString(plaintext));
		Console.WriteLine("  Key: " + BytesToHex(algorithm.Key));
		Console.WriteLine("  Encrypted data: " + BytesToHex(encrypted));
		Console.WriteLine("  Decrypted data: " + Encoding.ASCII.GetString(decrypted));
		Console.WriteLine("\r\nPress ENTER to continue...");
		Console.ReadLine();
		// dispose of the resources
		algorithm.Clear();
		encryptor.Dispose();
		decryptor.Dispose();
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