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

using  ApplicationException   = System.ApplicationException;
using ArgumentNullException   = System.ArgumentNullException;
using Console                 = System.Console;
using StringCollection        = System.Collections.Specialized.StringCollection;
using System.Collections;
using System.IO;

namespace NandoF
{
	public class Conzole
	{
		static public string LetUserChooseFile(string prompt, string extension,
		                                       out bool fileExists)  {
			string currentDir = Directory.GetCurrentDirectory();
			return LetUserChooseFile(prompt, extension, out fileExists, currentDir);
		}
		
		static public string LetUserChooseFile(string prompt, string extension,
		                                       out bool fileExists,
		                                       string directory)  {
			if (extension==null) throw new ArgumentNullException("extension");
			if (directory==null) throw new ArgumentNullException("directory");
			string[] dirFiles = Directory.GetFiles(directory, "*" + extension);
			ArrayList files = new ArrayList(dirFiles.GetLength(0));
			foreach(string filePath in dirFiles)
				files.Add(Path.GetFileName(filePath));
			string choice = LetUserChoose(prompt,
			                              (string[])files.ToArray(typeof(string)));
			if (choice==string.Empty)  fileExists = false;
			else  {
				choice     = Path.Combine(directory, choice);
				fileExists = File.Exists(choice);
			}
			return choice;
		}
		
		/// <param name="prompt">A string with the question to be shown.
		/// Can be null or empty.</param>
		/// <param name="options">String array with the options</param>
		/// <param name="chosen">Out integer that returns the chosen option index,
		/// or int.MinValue if no option was chosen.</param>
		/// <returns>A string containing the user's choice.</returns>
		static public string LetUserChoose(string prompt, string[] options)  {
			if (options==null)  throw new ArgumentNullException("options");
			if (prompt==null || prompt==string.Empty)  prompt = "Your choice: ";
			for (int i=0; i < options.GetLength(0); ++i) {
				Console.WriteLine(i.ToString() + ". " + options[i]);
			}
			string choice = ReadLine(prompt);
			if (choice==string.Empty)  return string.Empty;
			// See if choice was a number
			try  {
				ushort n = ushort.Parse(choice);
				return options[n];
			}
			// This executes if it is not a number
			catch  { return choice; }
		}
		
		/// <param name="prompt">A string with the question to be shown.
		/// Can be null or empty.</param>
		/// <param name="options">IList with the options. They are shown using their
		/// ToString() override.</param>
		/// <param name="allowNew">Boolean specifying whether user can choose a non
		/// existing option.</param>
		/// <returns>The selected object, or a new string.</returns>
		static public object LetUserChoose(string prompt, IList options,
		                                   bool allowNew)  {
			if (options==null)  throw new ArgumentNullException("options");
			if (prompt==null || prompt==string.Empty)  prompt = "Your choice: ";
			ushort count = 0;
			foreach (object option in options)  {
				Console.WriteLine(count.ToString() + ". " + option.ToString());
				++count;
			}
			string choice = ReadLine(prompt);
			if (choice==string.Empty)  return null;
			try  {
				// If choice is a positive integer, return the corresponding option
				ushort n = ushort.Parse(choice.Trim());
				return options[n];
			}
			catch  {
				// If something else was chosen, either allow a new object or complain
				if (allowNew)  return choice;
				else throw new ApplicationException("Not a listed number: " + choice);
			}
		}
		
		static public string LetUserChooseDir(string prompt, out bool dirExists,
		                                      string parentDir)  {
			if (parentDir==null || parentDir==string.Empty)
				throw new ArgumentNullException("parentDir");
			string[] subdirs = Directory.GetDirectories(parentDir);
			ArrayList names = new ArrayList(subdirs.GetLength(0));
			foreach(string subdir in subdirs)
				names.Add(Path.GetFileName(subdir));
			string choice = LetUserChoose(prompt,
			                              (string[])names.ToArray(typeof(string)));
			if (choice==string.Empty)  dirExists = false;
			else  {
				choice    = Path.Combine(parentDir, choice);
				dirExists = Directory.Exists(choice);
			}
			return choice;
		}
		
		static public bool   GetBoolResponse  (string prompt, bool defaux)  {
			string p;
			if (defaux) p = prompt + " (Y/n) ";
			else        p = prompt + " (y/N) ";
			Console.Write(p);
			string choice = Console.ReadLine().ToLower();
			if (choice=="y")           return true;
			if (choice=="n")           return false;
			if (choice==string.Empty)  return defaux;
			Console.WriteLine("Please answer  y  or  n");
			return GetBoolResponse(prompt, defaux);
		}
		
		static public string ReadLine         (string prompt)  {
			Console.Write(prompt + " ");
			return Console.ReadLine();
		}
		
		static public bool   ConfirmOverwriteIfExists(string path)  {
			if (File.Exists(path))  {
				Console.WriteLine("File already exists: " + path);
				return GetBoolResponse("Overwrite?", false);
			}
			else return true;
		}
	}
}
