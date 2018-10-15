#region NOME DO PROGRAMA
// Copyright (C) 2006  Nando Florestan
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License VERSION 2
// as published by the Free Software Foundation.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, read it on the web:
// http://www.gnu.org/copyleft/gpl.html

/*
 * Created in SharpDevelop: http://www.icsharpcode.net/OpenSource/SD/
 * Author: Nando Florestan: http://oui.com.br/n/
 * Date: 16/1/2006
*/
#endregion

using System.Collections;
using ApplicationException   = System.ApplicationException;
using Environment            = System.Environment;
using StringBuilder          = System.Text.StringBuilder;

namespace NandoF.Data
{
	/// <summary>A reusable class that represents an address.</summary>
	public class Address
	{
		protected string street   = string.Empty; // street and number
		public    string Street  {
			get  { return street;  }
			set  { street = Text.CapsLikeName(value); }
		}
		
		protected string district = string.Empty; // bairro
		public    string District  {
			get  { return district;  }
			set  { district = Text.CapsLikeName(value); }
		}
		
		protected string city     = string.Empty; // cidade
		public    string City  {
			get  { return city;  }
			set  { city = Text.CapsLikeName(value); }
		}
		
		protected string state    = string.Empty; // estado
		public    string State  {
			get  { return state;  }
			set  { state = value.ToUpper(); }
		}
		
		protected string zipCode  = string.Empty; // CEP
		public    string ZipCode  {
			get  { return zipCode;  }
			set  { zipCode = value; }
		}
		
		protected string country  = string.Empty; // país
		public    string Country  {
			get  { return country;  }
			set  { country = Text.CapsLikeName(value); }
		}
		
		override public string    ToString()  {
			StringBuilder sb = new StringBuilder(420);
			if (Street   != string.Empty)  sb.Append(Street   + Environment.NewLine);
			if (District != string.Empty)  sb.Append(District + Environment.NewLine);
			if (City     != string.Empty)  {
				sb.Append(City);
				if (State  != string.Empty)  sb.Append(" - " + State);
				sb.Append(Environment.NewLine);
			}
			else if (State != string.Empty) sb.Append(State + Environment.NewLine);
			if (ZipCode != string.Empty)  sb.Append(ZipCode + Environment.NewLine);
			if (Country != string.Empty)  sb.Append(Country + Environment.NewLine);
			// Remove the last NewLine
			return sb.ToString(0, sb.Length - Environment.NewLine.Length);
		}
		
		virtual  public ArrayList GetWarnings(string language)  {
			if (!language.StartsWith("pt"))  throw new ApplicationException
				("Language not supported: " + language);
			ArrayList a = new ArrayList(6);
			if (Text.IsNullOrEmpty(Street))   a.Add("Rua está em branco.");
			if (Text.IsNullOrEmpty(District)) a.Add("Bairro está em branco.");
			if (Text.IsNullOrEmpty(City))     a.Add("Cidade está em branco.");
			if (Text.IsNullOrEmpty(State))    a.Add("Estado está em branco.");
			if (Text.IsNullOrEmpty(ZipCode))  a.Add("Código postal está em branco.");
			if (Text.IsNullOrEmpty(Country))  a.Add("País está em branco.");
			if (a.Count < 1)  return null;
			else              return a;
		}
		
		public Address()  {}
		public Address(string street, string district, string city, string state,
		               string zipCode, string country)  {
			Street   = street;
			District = district;
			City     = city;
			State    = state;
			ZipCode  = zipCode;
			Country  = country;
		}
		
	}
}
