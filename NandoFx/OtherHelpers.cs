#region  NandoF library -- Copyright 2005-2006 Nando Florestan
/*
This library is free software; you can redistribute it and/or modify
it under the terms of the Lesser GNU General Public License as published by
the Free Software Foundation; either version 2.1 of the License, or
(at your option) any later version.

This software is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with this program; if not, see http://www.gnu.org/copyleft/lesser.html
*/
#endregion

namespace NandoF {
	
	#region Imported classes and namespaces
	using ApplicationException  = System.ApplicationException;
	using ArgumentNullException = System.ArgumentNullException;
	using ArrayList             = System.Collections.ArrayList;
	using ConfigurationSettings = System.Configuration.ConfigurationSettings;
	using DateTime              = System.DateTime;
	using Encoding              = System.Text.Encoding;
	using Environment           = System.Environment;
	using Exception             = System.Exception;
	using StringBuilder         = System.Text.StringBuilder;
	using StreamReader          = System.IO.StreamReader;
	using StreamWriter          = System.IO.StreamWriter;
	using TextWriter            = System.IO.TextWriter;
	using StringCollection      = System.Collections.Specialized.StringCollection;
	using Console               = System.Console;
	using Trace                 = System.Diagnostics.Trace;
	#endregion
	
	public class Tracing
	{
		static public void   Setup (bool toScreen, string toFile)
		{
			Trace.Listeners.Clear();
			if (toScreen) System.Diagnostics.Trace.Listeners.Add
				(new System.Diagnostics.TextWriterTraceListener(Console.Out));
			if (toFile!=null && toFile!=string.Empty)
				System.Diagnostics.Trace.Listeners.Add
					(new System.Diagnostics.TextWriterTraceListener
					 (new StreamWriter(toFile)));
			System.Diagnostics.Trace.AutoFlush = true;
		}
	} // end class
	
	
	#region Haven't used serialization for a long time now
	/*
  public class Serialization {
    /// <summary>
    /// Serializes any class into a string</summary>
    /// <remarks>
    /// To use serialization, your class must be marked with the
    /// [Serializable()] attribute.
    /// <p>Remember that due to a .NET bug, a TimeSpan value does not
    /// get serialized.</p></remarks>
    /// <param name="obj">The instance of the class to be serialized</param>
    /// <returns>A string containing an XML representation</returns>
    static public string XmlSerialize(object obj) {
      XmlSerializer ser = new XmlSerializer( obj.GetType() );
      StringWriter  sw  = new StringWriter ();
      ser.Serialize(sw, obj);
      return sw.ToString();
    }
    
    static public string XmlSerializeWorse(object obj) {
      // A worse version of XmlSerialize (because code is longer)
      XmlSerializer ser = new XmlSerializer(obj.GetType());
      MemoryStream ms = new MemoryStream();
      XmlTextWriter xtw = new XmlTextWriter(ms, System.Text.Encoding.UTF8);
      ser.Serialize(xtw, obj);
      // read stream to string
      ms.Position = 0;
      StreamReader sr = new StreamReader(ms);
      string Xml = sr.ReadToEnd();
      ms.Close();
      sr.Close();
      return(Xml);
    }
    
    /// <summary>
    /// Deserializes a string into a class</summary>
    /// <param name="sXML">
    /// The string containing the XML representation of the class</param>
    /// <param name="type">typeof(YourClass)</param>
    /// <returns>An object which you should cast into your class</returns>
    static public object XmlDeserialize(string sXML, System.Type type) {
      XmlSerializer ser = new XmlSerializer(type);
      // XmlTextReader rd  = new XmlTextReader();
      StringReader  sr  = new StringReader(sXML);
      return ser.Deserialize(sr);
    }
  } // end class
	 */
	#endregion
	
	
	public class Burocrasil
	{
		static public bool   IsValidCpf      (string cpf) {
			string c = Text.LeaveOnlyDigits(cpf);  // Examinar s os algarismos
			if (c.Length != 11) return false; // Tem de haver 11 algarismos
			byte[] n = new byte[11];
			for (byte index=1; index<12; index++)
				n[index] = checked(System.Convert.ToByte(c[index]));
			// Os 2 ultimos dgitos sao de verificacao.
			// O primeiro eh dado por...
			int d1 = n[9]*2 + n[8]*3 + n[7]*4 + n[6]*5 + n[5]*6 + n[4]*7 + n[3]*8 +
				n[2]*9 + n[1]*10;
			d1 = 11 - (d1 % 11);           // o porcento eh modulus (resto)
			if (d1 >= 10)    d1 = 0;
			if (d1 != n[10]) return false;
			// Segundo digito de verificacao:
			int d2 = d1*2 + n[9]*3 + n[8]*4 + n[7]*5 + n[6]*6 + n[5]*7 + n[4]*8 +
				n[3]*9 + n[2]*10 + n[1]*11;
			d2 = 11 - (d2 % 11);
			if (d2 >= 10)    d2 = 0;
			return (d2 == n[11]);
		}
		
		static public bool   IsValidCnpj     (string cnpj) {
			string tmp = Text.LeaveOnlyDigits(cnpj);  // discard non-digits
			if (tmp.Length != 14) return false;  // there must be 14 digits
			byte[] n = new byte[14];
			for(byte index=1; index<15; index++)
				n[index] = checked (System.Convert.ToByte(tmp[index]));
			int total1 = n[1] * 5 + n[2] * 4 + n[3] * 3 + n[4] * 2 + n[5] * 9 +
				n[6] * 8 + n[7] * 7 + n[8] * 6 + n[9] * 5 + n[10] * 4 +
				n[11] * 3 + n[12] * 2;
			byte resto = checked (System.Convert.ToByte(total1 % 11));  // modulus
			byte dv1, dv2;
			if (resto > 1) dv1 = System.Convert.ToByte(11 - resto);
			else           dv1 = 0;
			int total2 = n[1] * 6 + n[2] * 5 + n[3] * 4 + n[4] * 3 + n[5] * 2 +
				n[6] * 9 + n[7] * 8 + n[8] * 7 + n[9] * 6 + n[10] * 5 +
				n[11] * 4 + n[12] * 3 + dv1 * 2;
			resto = checked(System.Convert.ToByte(total2 % 11));
			if (resto > 1) dv2 = System.Convert.ToByte(11 - resto);
			else           dv2 = 0;
			return ((dv1 == n[13]) && (dv2 == n[14]));
		}
		
		static public bool   IsValidCpfOrCnpj(string s) {
			return (IsValidCpf(s) || IsValidCnpj(s));
		}
	}
	
	
	
	
}   // end namespace
