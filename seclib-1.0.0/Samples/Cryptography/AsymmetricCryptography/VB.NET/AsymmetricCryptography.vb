imports System
imports System.Text
imports System.Security.Cryptography
imports Org.Mentalis.Security.Certificates
imports Microsoft.VisualBasic

' <summary>
' This example shows how to use the public and private key from a certificate to encrypt and decrypt data.
' </summary>
class AsymmetricCryptography
	public shared sub Main(args() as string)
		Console.WriteLine("This example shows how to use the public and private key from a certificate to encrypt and decrypt data." + ControlChars.CrLf)
		dim data() as byte = Encoding.ASCII.GetBytes("Hello World!")
		' load the certificate from a file
		dim cert as Certificate = Certificate.CreateFromCerFile("client.cer")

		' get an RSA instance that represents the public key of the certificate
		dim public_key as RSA = cert.PublicKey
		' create a PKCS#1 key exchange formatter instance with the public key
		dim kef as RSAPKCS1KeyExchangeFormatter = new RSAPKCS1KeyExchangeFormatter(public_key)
		' encrypt the data, using the public key from the certificate
		dim encrypted() as byte = kef.CreateKeyExchange(data)

		' associate the certificate with its private key
		' we set exportable to true because decryption will fail on Windows 98
		' if this flag is not set. If you do not use Windows 98, you should set
		' the exportable flag to false for increased security.
		cert.AssociateWithPrivateKey("client.pvk", "test", true)
		' get an RSA instance that represents the private key
		dim private_key as RSA = cert.PrivateKey
		' create a PKCS#1 key exchange deformatter instance with the private key
		dim ked as RSAPKCS1KeyExchangeDeformatter = new RSAPKCS1KeyExchangeDeformatter(private_key)
		' decrypt the data, using the private key from the certificate
		dim decrypted() as byte = ked.DecryptKeyExchange(encrypted)
		
		' print the results in the console
		Console.WriteLine("Input data: " + Encoding.ASCII.GetString(data) + ControlChars.CrLf)
		Console.WriteLine("Encrypted data:" + ControlChars.CrLf + BytesToHex(encrypted) + ControlChars.CrLf)
		Console.WriteLine("Decrypted data: " + Encoding.ASCII.GetString(decrypted))
		Console.WriteLine(ControlChars.CrLf + "Press ENTER to continue...")
		Console.ReadLine()

		' clean up
		public_key.Clear()
		private_key.Clear()
	end sub
	' converts a byte array to its hexadecimal representation
	public shared function BytesToHex(array() as byte) as string
		dim sb as new StringBuilder(array.Length * 2)
    	dim i as integer
        for i = 0 to array.Length - 1
			sb.Append(array(i).ToString("X2"))
		next
		return sb.ToString()
	end function
end class