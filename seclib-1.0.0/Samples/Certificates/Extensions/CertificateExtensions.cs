using System;
using System.Text;
using Org.Mentalis.Security.Certificates;

/// <summary>
/// This example shows how to extract certificate extensions from certificates.
/// 
/// The Certificate.GetExtensions() and the Certificate.FindExtension() method
/// return Extension instances that contain an Object Identifier and an encoded
/// extension blob.
/// In order to use an extension, it must be decoded first. This can be done
/// with the Certificate.DecodeExtension method.
/// Every extension decodes to something different, so the user of the Security
/// Library has to implement the methods to convert from a decoded byte array
/// to a struct/class himself.
/// In this example, we show how to decode and parse the 'Key Usage',
/// 'Authority Information Access' and 'Subject Key Identifier' extensions.
/// 
/// If you wish to implement your own certificate extension parsers, you should
/// take a look at the following URL:
/// http://msdn.microsoft.com/library/en-us/security/security/constants_for_cryptencodeobject_and_cryptdecodeobject.asp
/// This page lists the types of structures that will be returned by
/// DecodeExtension function.
/// </summary>
public class CertificateExtensions {
	public static void Main() {
		CertificateExtensions t = new CertificateExtensions();
		t.Start();
	}
	public void Start() {
		Console.WriteLine("This example shows how to extract certificate extensions from certificates.\r\n");
		// load the certificate
		Certificate cert = Certificate.CreateFromCerFile(@"client.cer");
		Console.WriteLine("Successfully loaded the certificate\r\n" + cert.ToString(true) + "\r\n");
		// extract the extensions and print their Object Identifiers in the console
		Extension[] exts = cert.GetExtensions();
		Console.WriteLine("Found " + exts.Length + " certificate extensions:");
		for(int i = 0;  i < exts.Length; i++) {
			Console.WriteLine("  - " + exts[i].ObjectID + (exts[i].Critical ? " [critical extension]" : ""));
		}
		Console.WriteLine();
		// find the Key Usage extension
		Extension ext = cert.FindExtension("2.5.29.15"); // "2.5.29.15" is the object ID of the Key Usage extension
		if (ext != null) { // FindExtension returns a null pointer if the extension could not be found
			// We call the DecodeExtension and pass it our extension, the object identifier of the
			// structure we want it to return [in this case, the OID of the extension and of the
			// structure are the same] and the type of structure we want the method to return
			// [in this case, the type of the KEY_USAGE structure].
			// DecodeExtension will decode the specified extension and will create an instance
			// of the specified type. It will construct the type by using a constructor with two
			// variables: an IntPtr and an int. The first variable is a pointer to a buffer that
			// contains the decoded data. The second variable holds the length of the decoded buffer.
			KEY_USAGE usage = (KEY_USAGE)Certificate.DecodeExtension(ext, ext.ObjectID, typeof(KEY_USAGE));
			Console.WriteLine(usage.ToString());
		}
		// find the Authority Information Access extension
		ext = cert.FindExtension("1.3.6.1.5.5.7.1.1"); // "1.3.6.1.5.5.7.1.1" is the object ID of the Authority Information Access extension
		if (ext != null) { // FindExtension returns a null pointer if the extension could not be found
			CERT_AUTHORITY_INFO_ACCESS infoAccess = (CERT_AUTHORITY_INFO_ACCESS)Certificate.DecodeExtension(ext, ext.ObjectID, typeof(CERT_AUTHORITY_INFO_ACCESS));
			Console.WriteLine(infoAccess.ToString());
		}
		// find the Subject Key Identifier
		ext = cert.FindExtension("2.5.29.14"); // "2.5.29.14" is the object ID of the Subject Key extension
		if (ext != null) { // FindExtension returns a null pointer if the extension could not be found
			KEY_ID id = (KEY_ID)Certificate.DecodeExtension(ext, ext.ObjectID, typeof(KEY_ID));
			Console.WriteLine(id.ToString());
		}
		Console.WriteLine("Press ENTER to continue...");
		Console.ReadLine();
	}
}
