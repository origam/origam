using System;
using System.Text;
using System.Security.Cryptography;
using Org.Mentalis.Security.Cryptography;

/// <summary>
/// This example shows how to use the hashing classes from the Security Library.
/// </summary>
class Hashing {
	static void Main(string[] args) {
		Console.WriteLine("This example shows how to use the hashing classes from the Security Library.\r\n");
		Console.WriteLine("Please enter a string that will be hashed:");
		string input = Console.ReadLine();
		byte[] data = Encoding.ASCII.GetBytes(input);
		Console.WriteLine("Please enter a string that will be used as the HMAC key:");
		input = Console.ReadLine();
		byte[] key = Encoding.ASCII.GetBytes(input);
		Console.WriteLine();
		// print out the hash of the input using four different hashes
		PrintHash("MD2 hash:       ", new MD2CryptoServiceProvider(), data);
		PrintHash("MD5 hash:       ", new MD5CryptoServiceProvider(), data);
		PrintHash("SHA1 hash:      ", new SHA1CryptoServiceProvider(), data);
		PrintHash("RIPEMD160 hash: ", new RIPEMD160Managed(), data);
		PrintHash("HMAC-MD5 hash:  ", new HMAC(new MD5CryptoServiceProvider(), key), data);
		PrintHash("HMAC-SHA1 hash: ", new HMAC(new SHA1CryptoServiceProvider(), key), data);
		
		Console.WriteLine("\r\nPress ENTER to continue...");
		Console.ReadLine();
	}
	// prints the hash of a specified input to the console
	public static void PrintHash(string name, HashAlgorithm algo, byte[] data) {
		// compute the hash of the input data..
		byte[] hash = algo.ComputeHash(data);
		// ..and write the hash to the console
		Console.WriteLine(name + BytesToHex(hash));
		// dispose of the hash algorithm; we do not need to hash more data with it
		algo.Clear();
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
