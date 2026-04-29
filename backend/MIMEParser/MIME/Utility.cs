/******************************************************************************
    Copyright 2003-2004 Hamid Qureshi and Unruled Boy
    OpenPOP.Net is free software; you can redistribute it and/or modify
    it under the terms of the Lesser GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    OpenPOP.Net is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    Lesser GNU General Public License for more details.

    You should have received a copy of the Lesser GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
/*******************************************************************************/

/*
*Name:			OpenPOP.Utility
*Function:		Utility
*Author:		Hamid Qureshi
*Created:		2003/8
*Modified:		2004/5/31 14:22 GMT+8 by Unruled Boy
*Description:
*Changes:
*				2004/5/31 14:22 GMT+8 by Unruled Boy
*					1.Fixed a bug in decoding Base64 text when using non-standard encoding
*				2004/5/30 15:04 GMT+8 by Unruled Boy
*					1.Added all description to all functions
*				2004/5/25 13:55 GMT+8 by Unruled Boy
*					1.Rewrote the DecodeText function using Regular Expression
*				2004/5/17 14:20 GMT+8 by Unruled Boy
*					1.Added ParseFileName
*				2004/4/29 19:05 GMT+8 by Unruled Boy
*					1.Adding ReadPlainTextFromFile function
*				2004/4/28 19:06 GMT+8 by Unruled Boy
*					1.Rewriting the Decode method
*				2004/3/29 12:25 GMT+8 by Unruled Boy
*					1.GetMimeType support for MONO
*					2.cleaning up the names of variants
*/
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenPOP.MIMEParser;

/// <summary>
/// Summary description for Utility.
/// </summary>
public class Utility
{
    private static bool m_blnLog = false;
    private static string m_strLogFile = "OpenPOP.log";

    public Utility() { }

    //		public static string[] SplitText(string strText, string strSplitter)
    //		{
    //			string []segments=new string[0];
    //			int indexOfstrSplitter=strText.IndexOf(strSplitter);
    //			if(indexOfstrSplitter!=-1)
    //			{
    //
    //			}
    //			return segments;
    //		}
    //
    /// <summary>
    /// Verifies whether the file is of picture type or not
    /// </summary>
    /// <param name="strFile">File to be verified</param>
    /// <returns>True if picture file, false if not</returns>
    public static bool IsPictureFile(string strFile)
    {
        try
        {
            if (strFile != null && strFile != "")
            {
                strFile = strFile.ToLower();
                if (
                    strFile.EndsWith(value: ".jpg")
                    || strFile.EndsWith(value: ".bmp")
                    || strFile.EndsWith(value: ".ico")
                    || strFile.EndsWith(value: ".gif")
                    || strFile.EndsWith(value: ".png")
                )
                {
                    return true;
                }

                return false;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Parse date time info from MIME header
    /// </summary>
    /// <param name="strDate">Encoded MIME date time</param>
    /// <returns>Decoded date time info</returns>
    public static string ParseEmailDate(string strDate)
    {
        string strRet = strDate.Trim();
        int indexOfTag = strRet.IndexOf(value: ",");
        if (indexOfTag != -1)
        {
            strRet = strRet.Substring(startIndex: indexOfTag + 1);
        }
        strRet = QuoteText(strText: strRet, strTag: "+");
        strRet = QuoteText(strText: strRet, strTag: "-");
        strRet = QuoteText(strText: strRet, strTag: "GMT");
        strRet = QuoteText(strText: strRet, strTag: "CST");
        return strRet.Trim();
    }

    /// <summary>
    /// Quote the text according to a tag
    /// </summary>
    /// <param name="strText">Text to be quoted</param>
    /// <param name="strTag">Quote tag</param>
    /// <returns>Quoted Text</returns>
    public static string QuoteText(string strText, string strTag)
    {
        int indexOfTag = strText.IndexOf(value: strTag);
        if (indexOfTag != -1)
        {
            return strText.Substring(startIndex: 0, length: indexOfTag - 1);
        }

        return strText;
    }

    /// <summary>
    /// Parse file name from MIME header
    /// </summary>
    /// <param name="strHeader">MIME header</param>
    /// <returns>Decoded file name</returns>
    public static string ParseFileName(string strHeader)
    {
        string strTag;
        strTag = "filename=";
        int intPos = strHeader.ToLower().IndexOf(value: strTag);
        if (intPos == -1)
        {
            strTag = "name=";
            intPos = strHeader.ToLower().IndexOf(value: strTag);
        }
        string strRet;
        if (intPos != -1)
        {
            strRet = strHeader.Substring(startIndex: intPos + strTag.Length);
            intPos = strRet.ToLower().IndexOf(value: ";");
            if (intPos != -1)
            {
                strRet = strRet.Substring(startIndex: 1, length: intPos - 1);
            }

            strRet = RemoveQuote(strText: strRet);
        }
        else
        {
            strRet = "";
        }

        return strRet;
    }

    /// <summary>
    /// Parse email address from MIME header
    /// </summary>
    /// <param name="strEmailAddress">MIME header</param>
    /// <param name="strUser">Decoded user name</param>
    /// <param name="strAddress">Decoded email address</param>
    /// <returns>True if decoding succeeded, false if failed</returns>
    public static bool ParseEmailAddress(
        string strEmailAddress,
        ref string strUser,
        ref string strAddress
    )
    {
        int indexOfAB = strEmailAddress.Trim().LastIndexOf(value: "<");
        int indexOfEndAB = strEmailAddress.Trim().LastIndexOf(value: ">");
        strUser = strEmailAddress;
        strAddress = strEmailAddress;
        if (indexOfAB >= 0 && indexOfEndAB >= 0)
        {
            if (indexOfAB > 0)
            {
                strUser = strUser.Substring(startIndex: 0, length: indexOfAB - 1);
                //					strUser=strUser.Substring(0,indexOfAB-1).Trim('\"');
                //					if(strUser.IndexOf("\"")>=0)
                //					{
                //						strUser=strUser.Substring(1,strUser.Length-1);
                //					}
            }
            strUser = strUser.Trim();
            strUser = strUser.Trim(trimChars: '\"');
            strAddress = strAddress.Substring(
                startIndex: indexOfAB + 1,
                length: indexOfEndAB - (indexOfAB + 1)
            );
        }
        strUser = strUser.Trim();
        strUser = DecodeText(strText: strUser);
        strAddress = strAddress.Trim();
        return true;
    }

    /// <summary>
    /// Save byte content to a file
    /// </summary>
    /// <param name="strFile">File to be saved to</param>
    /// <param name="bytContent">Byte array content</param>
    /// <returns>True if saving succeeded, false if failed</returns>
    public static bool SaveByteContentToFile(string strFile, byte[] bytContent)
    {
        try
        {
            if (File.Exists(path: strFile))
            {
                File.Delete(path: strFile);
            }

            FileStream fs = File.Create(path: strFile);
            fs.Write(array: bytContent, offset: 0, count: bytContent.Length);
            fs.Close();
            return true;
        }
        catch (Exception e)
        {
            Utility.LogError(strText: "SaveByteContentToFile():" + e.Message);
            return false;
        }
    }

    /// <summary>
    /// Save text content to a file
    /// </summary>
    /// <param name="strFile">File to be saved to</param>
    /// <param name="strText">Text content</param>
    /// <param name="blnReplaceExists">Replace file if exists</param>
    /// <returns>True if saving succeeded, false if failed</returns>
    public static bool SavePlainTextToFile(string strFile, string strText, bool blnReplaceExists)
    {
        try
        {
            bool blnRet = true;
            if (File.Exists(path: strFile))
            {
                if (blnReplaceExists)
                {
                    File.Delete(path: strFile);
                }
                else
                {
                    blnRet = false;
                }
            }
            if (blnRet == true)
            {
                StreamWriter sw = File.CreateText(path: strFile);
                sw.Write(value: strText);
                sw.Close();
            }
            return blnRet;
        }
        catch (Exception e)
        {
            Utility.LogError(strText: "SavePlainTextToFile():" + e.Message);
            return false;
        }
    }

    /// <summary>
    /// Read text content from a file
    /// </summary>
    /// <param name="strFile">File to be read from</param>
    /// <param name="strText">Read text content</param>
    /// <returns>True if reading succeeded, false if failed</returns>
    public static bool ReadPlainTextFromFile(string strFile, ref string strText)
    {
        if (File.Exists(path: strFile))
        {
            StreamReader fs = new StreamReader(path: strFile);
            strText = fs.ReadToEnd();
            fs.Close();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sepearte header name and header value
    /// </summary>
    /// <param name="strRawHeader"></param>
    /// <returns></returns>
    public static string[] GetHeadersValue(string strRawHeader)
    {
        if (strRawHeader == null)
        {
            throw new ArgumentNullException(
                paramName: "strRawHeader",
                message: "Argument was null"
            );
        }

        string[] array = new string[2] { "", "" };
        int indexOfColon = strRawHeader.IndexOf(value: ":");
        try
        {
            array[0] = strRawHeader.Substring(startIndex: 0, length: indexOfColon).Trim();
            array[1] = strRawHeader.Substring(startIndex: indexOfColon + 1).Trim();
        }
        catch (Exception) { }
        return array;
    }

    /// <summary>
    /// Get quoted text
    /// </summary>
    /// <param name="strText">Text with quotes</param>
    /// <param name="strSplitter">Splitter</param>
    /// <param name="strTag">Target tag</param>
    /// <returns>Text without quote</returns>
    public static string GetQuotedValue(string strText, string strSplitter, string strTag)
    {
        if (strText == null)
        {
            throw new ArgumentNullException(paramName: "strText", message: "Argument was null");
        }

        string[] array = new string[2] { "", "" };
        int indexOfstrSplitter = strText.IndexOf(value: strSplitter);
        try
        {
            array[0] = strText.Substring(startIndex: 0, length: indexOfstrSplitter).Trim();
            array[1] = strText.Substring(startIndex: indexOfstrSplitter + 1).Trim();
            int pos = array[1].IndexOf(value: "\"");
            if (pos != -1)
            {
                int pos2 = array[1].IndexOf(value: "\"", startIndex: pos + 1);
                array[1] = array[1].Substring(startIndex: pos + 1, length: pos2 - pos - 1);
            }
        }
        catch (Exception) { }
        //return array;
        if (array[0].ToLower() == strTag.ToLower())
        {
            return array[1].Trim();
        }

        return null;
        /*			string []array=null;
       try
       {
           array=Regex.Split(strText,strSplitter);
           //return array;
           if(array[0].ToLower()==strTag.ToLower())
               return RemoveQuote(array[1].Trim());
           else
               return null;
       }
       catch
       {return null;}*/
    }

    /// <summary>
    /// Change text encoding
    /// </summary>
    /// <param name="strText">Source encoded text</param>
    /// <param name="strCharset">New charset</param>
    /// <returns>Encoded text with new charset</returns>
    public static string Change(string strText, string strCharset)
    {
        if (strCharset == null || strCharset == "")
        {
            return strText;
        }

        byte[] b = Encoding.Default.GetBytes(s: strText);
        return new string(value: Encoding.GetEncoding(name: strCharset).GetChars(bytes: b));
    }

    /// <summary>
    /// Remove non-standard base 64 characters
    /// </summary>
    /// <param name="strText">Source text</param>
    /// <returns>standard base 64 text</returns>
    public static string RemoveNonB64(string strText)
    {
        return strText.Replace(oldValue: "\0", newValue: "");
    }

    /// <summary>
    /// Remove white blank characters
    /// </summary>
    /// <param name="strText">Source text</param>
    /// <returns>Text with white blanks</returns>
    public static string RemoveWhiteBlanks(string strText)
    {
        return strText
            .Replace(oldValue: "\0", newValue: "")
            .Replace(oldValue: "\r\n", newValue: "");
    }

    /// <summary>
    /// Remove quotes
    /// </summary>
    /// <param name="strText">Text with quotes</param>
    /// <returns>Text without quotes</returns>
    public static string RemoveQuote(string strText)
    {
        string strRet = strText;
        if (strRet.StartsWith(value: "\""))
        {
            strRet = strRet.Substring(startIndex: 1);
        }

        if (strRet.EndsWith(value: "\""))
        {
            strRet = strRet.Substring(startIndex: 0, length: strRet.Length - 1);
        }

        return strRet;
    }

    /// <summary>
    /// Decode one line of text
    /// </summary>
    /// <param name="strText">Encoded text</param>
    /// <returns>Decoded text</returns>
    public static string DecodeLine(string strText)
    {
        return DecodeText(strText: RemoveWhiteBlanks(strText: strText));
    }

    /// <summary>
    /// Verifies wether the text is a valid MIME Text or not
    /// </summary>
    /// <param name="strText">Text to be verified</param>
    /// <returns>True if MIME text, false if not</returns>
    private static bool IsValidMIMEText(string strText)
    {
        int intPos = strText.IndexOf(value: "=?");
        return (
            intPos != -1
            && strText.IndexOf(value: "?=", startIndex: intPos + 6) != -1
            && strText.Length > 7
        );
    }

    /// <summary>
    /// Decode text
    /// </summary>
    /// <param name="strText">Source text</param>
    /// <returns>Decoded text</returns>
    public static string DecodeText(string strText)
    {
        /*try
        {
            string strRet="";
            string strBody="";
            MatchCollection mc=Regex.Matches(strText,@"\=\?(?<Charset>\S+)\?(?<Encoding>\w)\?(?<Content>\S+)\?\=");
            for(int i=0;i<mc.Count;i++)
            {
                if(mc[i].Success)
                {
                    strBody=mc[i].Groups["Content"].Value;
                    switch(mc[i].Groups["Encoding"].Value.ToUpper())
                    {
                        case "B":
                            strBody=deCodeB64s(strBody,mc[i].Groups["Charset"].Value);
                            break;
                        case "Q":
                            strBody=DecodeQP.ConvertHexContent(strBody);//, m.Groups["Charset"].Value);
                            break;
                        default:
                            break;
                    }
                    strRet+=strBody;
                }
                else
                {
                    strRet+=mc[i].Value;
                }
            }
            return strRet;
        }
        catch
        {return strText;}*/
        try
        {
            string strRet = "";
            string[] strParts = Regex.Split(input: strText, pattern: "\r\n");
            string strBody = "";
            const string strRegEx = @"\=\?(?<Charset>\S+)\?(?<Encoding>\w)\?(?<Content>\S+)\?\=";
            Match m = null;
            for (int i = 0; i < strParts.Length; i++)
            {
                m = Regex.Match(input: strParts[i], pattern: strRegEx);
                if (m.Success)
                {
                    strBody = m.Groups[groupname: "Content"].Value;
                    switch (m.Groups[groupname: "Encoding"].Value.ToUpper())
                    {
                        case "B":
                        {
                            strBody = deCodeB64s(
                                strText: strBody,
                                strEncoding: m.Groups[groupname: "Charset"].Value
                            );
                            break;
                        }

                        case "Q":
                        {
                            strBody = DecodeQP.ConvertHexContent(
                                Hexstring: strBody,
                                encoding: m.Groups[groupname: "Charset"].Value
                            );
                            break;
                        }

                        default:
                        {
                            break;
                        }
                    }
                    strRet += strBody;
                }
                else
                {
                    if (!IsValidMIMEText(strText: strParts[i]))
                    {
                        strRet += strParts[i];
                    }
                    //blank text
                }
            }
            return strRet;
        }
        catch
        {
            return strText;
        }
        /*
            {
                try
                {
                    if(strText!=null&&strText!="")
                    {
                        if(IsValidMIMEText(strText))
                        {
                            //position at the end of charset
                            int intPos=strText.IndexOf("=?");
                            int intPos2=strText.IndexOf("?",intPos+2);
                            if(intPos2>3)
                            {
                                string strCharset=strText.Substring(2,intPos2-2);
                                string strEncoding=strText.Substring(intPos2+1,1);
                                int intPos3=strText.IndexOf("?=",intPos2+3);
                                string strBody=strText.Substring(intPos2+3,intPos3-intPos2-3);
                                string strHead="";
                                if(intPos>0)
                                {
                                    strHead=strText.Substring(0,intPos-1);
                                }
                                string strEnd="";
                                if(intPos3<strText.Length-2)
                                {
                                    strEnd=strText.Substring(intPos3+2);
                                }
                                switch(strEncoding.ToUpper())
                                {
                                    case "B":
                                        strBody=deCodeB64s(strBody);
                                        break;
                                    case "Q":
                                        strBody=DecodeQP.ConvertHexContent(strBody);
                                        break;
                                    default:
                                        break;
                                }
                                strText=strHead+strBody+strEnd;
                                if(IsValidMIMEText(strText))
                                    return DecodeText(strText);
                                else
                                    return strText;
                            }
                            else
                            {return strText;}
                        }
                        else
                        {return strText;}
                    }
                    else
                    {return strText;}
                }
                catch
                {return strText;}*/
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="strText"></param>
    /// <returns></returns>
    public static string deCodeB64s(string strText)
    {
        return Encoding.Default.GetString(bytes: deCodeB64(strText: strText));
    }

    public static string deCodeB64s(string strText, string strEncoding)
    {
        try
        {
            if (strEncoding.ToLower() == "ISO-8859-1".ToLower())
            {
                return deCodeB64s(strText: strText);
            }

            return Encoding
                .GetEncoding(name: strEncoding)
                .GetString(bytes: deCodeB64(strText: strText));
        }
        catch
        {
            return deCodeB64s(strText: strText);
        }
    }

    private static byte[] deCodeB64(string strText)
    {
        byte[] by = null;
        try
        {
            if (strText != "")
            {
                by = Convert.FromBase64String(s: strText);
                //strText=Encoding.Default.GetString(by);
            }
        }
        catch (Exception e)
        {
            by = Encoding.Default.GetBytes(s: "\0");
            LogError(strText: "deCodeB64():" + e.Message);
        }
        return by;
    }

    /// <summary>
    /// Turns file logging on and off.
    /// </summary>
    /// <remarks>Comming soon.</remarks>
    public static bool Log
    {
        get { return m_blnLog; }
        set { m_blnLog = value; }
    }

    internal static void LogError(string strText)
    {
        //Log=true;
        if (Log)
        {
            FileInfo file = null;
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                file = new FileInfo(fileName: m_strLogFile);
                sw = file.AppendText();
                //fs = new FileStream(m_strLogFile, FileMode.OpenOrCreate, FileAccess.Write);
                //sw = new StreamWriter(fs);
                sw.WriteLine(value: DateTime.Now);
                sw.WriteLine(value: strText);
                sw.WriteLine(value: "\r\n");
                sw.Flush();
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                    sw = null;
                }
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
            }
        }
    }

    public static bool IsQuotedPrintable(string strText)
    {
        if (strText != null)
        {
            return (strText.ToLower() == "quoted-printable".ToLower());
        }

        return false;
    }

    public static bool IsBase64(string strText)
    {
        if (strText != null)
        {
            return (strText.ToLower() == "base64".ToLower());
        }

        return false;
    }

    public static string[] SplitOnSemiColon(string strText)
    {
        if (strText == null)
        {
            throw new ArgumentNullException(paramName: "strText", message: "Argument was null");
        }

        string[] array = null;
        int indexOfColon = strText.IndexOf(value: ";");
        if (indexOfColon < 0)
        {
            array = new string[1];
            array[0] = strText;
            return array;
        }

        array = new string[2];
        try
        {
            array[0] = strText.Substring(startIndex: 0, length: indexOfColon).Trim();
            array[1] = strText.Substring(startIndex: indexOfColon + 1).Trim();
        }
        catch (Exception) { }
        return array;
    }

    public static bool IsNotNullText(string strText)
    {
        try
        {
            return (strText != null && strText != "");
        }
        catch
        {
            return false;
        }
    }

    public static bool IsNotNullTextEx(string strText)
    {
        try
        {
            return (strText != null && strText.Trim() != "");
        }
        catch
        {
            return false;
        }
    }

    public static bool IsOrNullTextEx(string strText)
    {
        try
        {
            return (strText == null || strText.Trim() == "");
        }
        catch
        {
            return false;
        }
    }
}
