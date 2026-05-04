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
*Name:			OpenPOP.MIMEParser.Message
*Function:		Message Parser
*Author:		Hamid Qureshi
*Created:		2003/8
*Modified:		2004/6/16 18:34 GMT+8 by Unruled Boy
*Description:
*Changes:
*				2004/6/16 18:34 GMT+8 by Unruled Boy
*					1.fixed a loop in message body decoding by .
*				2004/5/17 14:20 GMT+8 by Unruled Boy
*					1.Again, fixed something but do not remember :(
*				2004/5/11 17:00 GMT+8 by Unruled Boy
*					1.Fixed a bug in parsing ContentCharset
*					2.Fixed a bug in ParseStreamLines
*				2004/5/10 10:00 GMT+8 by Unruled Boy
*					1.Well, fixed something but do not remember :(
*				2004/5/8 17:00 GMT+8 by Unruled Boy
*					1.Fixed a bug in parsing boundary
*				2004/5/1 14:13 GMT+8 by Unruled Boy
*					1.Adding three more constructors
*					2.Adding descriptions to every public functions/property/void
*				2004/4/29 19:05 GMT+8 by Unruled Boy
*					1.Fixed the bug parsing headers/boundary
*				2004/4/28 19:06 GMT+8 by Unruled Boy
*					1.Adding DateTimeInfo property
*					2.Maybe we correct the HTML content type bug
*				2004/4/23 21:13 GMT+8 by Unruled Boy
*					1.New Contructor
*					2.Tidy up the codes to follow Hungarian Notation
*				2004/3/29 10:38 GMT+8 by Unruled Boy
*					1.removing bugs in decoding message
*				2004/3/29 17:22 GMT+8 by Unruled Boy
*					1.adding support for reply message using ms-tnef
*					2.adding support for all custom headers
*					3.rewriting the header parser(adding 3 ParseStreamLines)
*					4.adding detail description for every function
*					5.cleaning up the codes
*				2004/3/30 09:15 GMT+8 by Unruled Boy
*					1.Adding ImportanceType
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenPOP.MIMEParser;

/// <summary>
/// Message Parser.
/// </summary>
public class Message
{
    #region Member Variables
    private List<Attachment> _attachments = new();
    private string _rawHeader = null;
    private string _rawMessage = null;
    private string _rawMessageBody = null;
    private int _attachmentCount = 0;
    private string _replyTo = null;
    private string _replyToEmail = null;
    private string _from = null;
    private string _fromEmail = null;
    private string _date = null;
    private string _dateTimeInfo = null;
    private string _subject = null;
    private string[] _to = new string[0];
    private string[] _cc = new string[0];
    private string[] _bcc = new string[0];
    private List<string> _keywords = new();
    private string _contentType = null;
    private string _contentCharset = null;
    private string _reportType = null;
    private string _contentTransferEncoding = null;
    private bool _html = false;
    private long _contentLength = 0;
    private string _contentEncoding = null;
    private string _returnPath = null;
    private string _mimeVersion = null;
    private string _received = null;
    private string _importance = null;
    private string _messageID = null;
    private string _attachmentboundry = null;
    private string _attachmentboundry2 = null;
    private bool _hasAttachment = false;
    private string _dispositionNotificationTo = null;
    private List<string> _messageBody = new();
    private string _basePath = null;
    private bool _autoDecodeMSTNEF = false;
    private Hashtable _customHeaders = new Hashtable();
    #endregion
    #region Properties
    /// <summary>
    /// custom headers
    /// </summary>
    public Hashtable CustomHeaders
    {
        get { return _customHeaders; }
        set { _customHeaders = value; }
    }

    /// <summary>
    /// whether auto decoding MS-TNEF attachment files
    /// </summary>
    public bool AutoDecodeMSTNEF
    {
        get { return _autoDecodeMSTNEF; }
        set { _autoDecodeMSTNEF = value; }
    }

    /// <summary>
    /// path to extract MS-TNEF attachment files
    /// </summary>
    public string BasePath
    {
        get { return _basePath; }
        set
        {
            try
            {
                if (value.EndsWith(value: "\\"))
                {
                    _basePath = value;
                }
                else
                {
                    _basePath = value + "\\";
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// message keywords
    /// </summary>
    public List<string> Keywords
    {
        get { return _keywords; }
    }

    /// <summary>
    /// disposition notification
    /// </summary>
    public string DispositionNotificationTo
    {
        get { return _dispositionNotificationTo; }
    }

    /// <summary>
    /// received server
    /// </summary>
    public string Received
    {
        get { return _received; }
    }

    /// <summary>
    /// importance level
    /// </summary>
    public string Importance
    {
        get { return _importance; }
    }

    /// <summary>
    /// importance level type
    /// </summary>
    public MessageImportanceType ImportanceType
    {
        get
        {
            switch (_importance.ToUpper())
            {
                case "5":
                case "HIGH":
                {
                    return MessageImportanceType.HIGH;
                }
                case "3":
                case "NORMAL":
                {
                    return MessageImportanceType.NORMAL;
                }
                case "1":
                case "LOW":
                {
                    return MessageImportanceType.LOW;
                }
                default:
                {
                    return MessageImportanceType.NORMAL;
                }
            }
        }
    }

    /// <summary>
    /// Content Charset
    /// </summary>
    public string ContentCharset
    {
        get { return _contentCharset; }
    }

    /// <summary>
    /// Content Transfer Encoding
    /// </summary>
    public string ContentTransferEncoding
    {
        get { return _contentTransferEncoding; }
    }

    /// <summary>
    /// Message Bodies
    /// </summary>
    public List<string> MessageBody
    {
        get { return _messageBody; }
    }

    /// <summary>
    /// Attachment Boundry
    /// </summary>
    public string AttachmentBoundry
    {
        get { return _attachmentboundry; }
    }

    /// <summary>
    /// Alternate Attachment Boundry
    /// </summary>
    public string AttachmentBoundry2
    {
        get { return _attachmentboundry2; }
    }

    /// <summary>
    /// Attachment Count
    /// </summary>
    public int AttachmentCount
    {
        get { return _attachmentCount; }
    }

    /// <summary>
    /// Attachments
    /// </summary>
    public List<Attachment> Attachments
    {
        get { return _attachments; }
    }

    /// <summary>
    /// CC
    /// </summary>
    public string[] CC
    {
        get { return _cc; }
    }

    /// <summary>
    /// BCC
    /// </summary>
    public string[] BCC
    {
        get { return _bcc; }
    }

    /// <summary>
    /// TO
    /// </summary>
    public string[] TO
    {
        get { return _to; }
    }

    /// <summary>
    /// Content Encoding
    /// </summary>
    public string ContentEncoding
    {
        get { return _contentEncoding; }
    }

    /// <summary>
    /// Content Length
    /// </summary>
    public long ContentLength
    {
        get { return _contentLength; }
    }

    /// <summary>
    /// Content Type
    /// </summary>
    public string ContentType
    {
        get { return _contentType; }
    }

    /// <summary>
    /// Report Type
    /// </summary>
    public string ReportType
    {
        get { return _reportType; }
    }

    /// <summary>
    /// HTML
    /// </summary>
    public bool HTML
    {
        get { return _html; }
    }

    /// <summary>
    /// Date
    /// </summary>
    public string Date
    {
        get { return _date; }
    }

    /// <summary>
    /// DateTime Info
    /// </summary>
    public string DateTimeInfo
    {
        get { return _dateTimeInfo; }
    }

    /// <summary>
    /// From name
    /// </summary>
    public string From
    {
        get { return _from; }
    }

    /// <summary>
    /// From Email
    /// </summary>
    public string FromEmail
    {
        get { return _fromEmail; }
    }

    /// <summary>
    /// Reply to name
    /// </summary>
    public string ReplyTo
    {
        get { return _replyTo; }
    }

    /// <summary>
    /// Reply to email
    /// </summary>
    public string ReplyToEmail
    {
        get { return _replyToEmail; }
    }

    /// <summary>
    /// whether has attachment
    /// </summary>
    public bool HasAttachment
    {
        get { return _hasAttachment; }
    }

    /// <summary>
    /// raw message body
    /// </summary>
    public string RawMessageBody
    {
        get { return _rawMessageBody; }
    }

    /// <summary>
    /// Message ID
    /// </summary>
    public string MessageID
    {
        get { return _messageID; }
    }

    /// <summary>
    /// MIME version
    /// </summary>
    public string MimeVersion
    {
        get { return _mimeVersion; }
    }

    /// <summary>
    /// raw header
    /// </summary>
    public string RawHeader
    {
        get { return _rawHeader; }
    }

    /// <summary>
    /// raw message
    /// </summary>
    public string RawMessage
    {
        get { return _rawMessage; }
    }

    /// <summary>
    /// return path
    /// </summary>
    public string ReturnPath
    {
        get { return _returnPath; }
    }

    /// <summary>
    /// subject
    /// </summary>
    public string Subject
    {
        get { return _subject; }
    }
    #endregion
    /// <summary>
    /// release all objects
    /// </summary>
    ~Message()
    {
        _attachments.Clear();
        _attachments = null;
        _keywords.Clear();
        _keywords = null;
        _messageBody.Clear();
        _messageBody = null;
        _customHeaders.Clear();
        _customHeaders = null;
    }

    /// <summary>
    /// New Message
    /// </summary>
    /// <param name="blnFinish">reference for the finishing state</param>
    /// <param name="strBasePath">path to extract MS-TNEF attachment files</param>
    /// <param name="blnAutoDecodeMSTNEF">whether auto decoding MS-TNEF attachments</param>
    /// <param name="blnOnlyHeader">whether only decode the header without body</param>
    /// <param name="strEMLFile">file of email content to load from</param>
    public Message(
        ref bool blnFinish,
        string strBasePath,
        bool blnAutoDecodeMSTNEF,
        bool blnOnlyHeader,
        string strEMLFile
    )
    {
        string strMessage = null;
        if (Utility.ReadPlainTextFromFile(strFile: strEMLFile, strText: ref strMessage))
        {
            NewMessage(
                blnFinish: ref blnFinish,
                strBasePath: strBasePath,
                blnAutoDecodeMSTNEF: blnAutoDecodeMSTNEF,
                strMessage: strMessage,
                blnOnlyHeader: blnOnlyHeader
            );
        }
        else
        {
            blnFinish = true;
        }
    }

    /// <summary>
    /// New Message
    /// </summary>
    /// <param name="blnFinish">reference for the finishing state</param>
    /// <param name="strBasePath">path to extract MS-TNEF attachment files</param>
    /// <param name="blnAutoDecodeMSTNEF">whether auto decoding MS-TNEF attachments</param>
    /// <param name="strMessage">raw message content</param>
    /// <param name="blnOnlyHeader">whether only decode the header without body</param>
    public Message(
        ref bool blnFinish,
        string strBasePath,
        bool blnAutoDecodeMSTNEF,
        string strMessage,
        bool blnOnlyHeader
    )
    {
        NewMessage(
            blnFinish: ref blnFinish,
            strBasePath: strBasePath,
            blnAutoDecodeMSTNEF: blnAutoDecodeMSTNEF,
            strMessage: strMessage,
            blnOnlyHeader: blnOnlyHeader
        );
    }

    /// <summary>
    /// New Message
    /// </summary>
    /// <param name="blnFinish">reference for the finishing state</param>
    /// <param name="strMessage">raw message content</param>
    /// <param name="blnOnlyHeader">whether only decode the header without body</param>
    public Message(ref bool blnFinish, string strMessage, bool blnOnlyHeader)
    {
        NewMessage(
            blnFinish: ref blnFinish,
            strBasePath: "",
            blnAutoDecodeMSTNEF: false,
            strMessage: strMessage,
            blnOnlyHeader: blnOnlyHeader
        );
    }

    /// <summary>
    /// New Message
    /// </summary>
    /// <param name="blnFinish">reference for the finishing state</param>
    /// <param name="strMessage">raw message content</param>
    public Message(ref bool blnFinish, string strMessage)
    {
        NewMessage(
            blnFinish: ref blnFinish,
            strBasePath: "",
            blnAutoDecodeMSTNEF: false,
            strMessage: strMessage,
            blnOnlyHeader: false
        );
    }

    /// <summary>
    /// get valid attachment
    /// </summary>
    /// <param name="intAttachmentNumber">attachment index in the attachments collection</param>
    /// <returns>attachment</returns>
    public Attachment GetAttachment(int intAttachmentNumber)
    {
        if (
            intAttachmentNumber < 0
            || intAttachmentNumber > _attachmentCount
            || intAttachmentNumber > _attachments.Count
        )
        {
            Utility.LogError(strText: "GetAttachment():attachment not exist");
            throw new ArgumentOutOfRangeException(paramName: "intAttachmentNumber");
        }
        return (Attachment)_attachments[index: intAttachmentNumber];
    }

    /// <summary>
    /// New Message
    /// </summary>
    /// <param name="blnFinish">reference for the finishing state</param>
    /// <param name="strBasePath">path to extract MS-TNEF attachment files</param>
    /// <param name="blnAutoDecodeMSTNEF">whether auto decoding MS-TNEF attachments</param>
    /// <param name="strMessage">raw message content</param>
    /// <param name="blnOnlyHeader">whether only decode the header without body</param>
    /// <returns>construction result whether successfully new a message</returns>
    private bool NewMessage(
        ref bool blnFinish,
        string strBasePath,
        bool blnAutoDecodeMSTNEF,
        string strMessage,
        bool blnOnlyHeader
    )
    {
        StringReader srdReader = new StringReader(s: strMessage);
        StringBuilder sbdBuilder = new StringBuilder();
        _basePath = strBasePath;
        _autoDecodeMSTNEF = blnAutoDecodeMSTNEF;
        _rawMessage = strMessage;
        string strLine = srdReader.ReadLine();
        while (Utility.IsNotNullTextEx(strText: strLine))
        {
            sbdBuilder.Append(value: strLine + "\r\n");
            ParseHeader(sbdBuilder: sbdBuilder, srdReader: srdReader, strLine: ref strLine);
            if (Utility.IsOrNullTextEx(strText: strLine))
            {
                break;
            }

            strLine = srdReader.ReadLine();
        }
        _rawHeader = sbdBuilder.ToString();

        SetAttachmentBoundry2(strBuffer: _rawMessage);
        if (_contentLength == 0)
        {
            _contentLength = strMessage.Length; //_rawMessageBody.Length;
        }

        if (blnOnlyHeader == false)
        {
            _rawMessageBody = srdReader.ReadToEnd().Trim();
            //the auto reply mail by outlook uses ms-tnef format
            if (
                (_hasAttachment == true && _attachmentboundry != null)
                || MIMETypes.IsMSTNEF(strContentType: _contentType)
            )
            {
                set_attachments();
                if (this.Attachments.Count > 0)
                {
                    Attachment at = this.GetAttachment(intAttachmentNumber: 0);
                    if (at != null && at.NotAttachment)
                    {
                        this.GetMessageBody(strBuffer: at.DecodeAsText());
                    }

                    //in case body parts as text[0] html[1]
                    if (this.Attachments.Count > 1 && !this.IsReport())
                    {
                        at = this.GetAttachment(intAttachmentNumber: 1);
                        if (at != null && at.NotAttachment)
                        {
                            this.GetMessageBody(strBuffer: at.DecodeAsText());
                        }
                    }
                }
            }
            else
            {
                GetMessageBody(strBuffer: _rawMessageBody);
            }
        }
        blnFinish = true;
        return true;
    }

    /// <summary>
    /// parse message body
    /// </summary>
    /// <param name="strBuffer">raw message body</param>
    /// <returns>message body</returns>
    public string GetTextBody(string strBuffer)
    {
        if (strBuffer.EndsWith(value: "\r\n."))
        {
            return strBuffer.Substring(startIndex: 0, length: strBuffer.Length - "\r\n.".Length);
        }

        return strBuffer;
    }

    /// <summary>
    /// parse message body
    /// </summary>
    /// <param name="strBuffer">raw message body</param>
    public void GetMessageBody(string strBuffer)
    {
        int end,
            begin;
        string body;
        string encoding = "";

        begin = end = 0;
        _messageBody.Clear();
        try
        {
            if (Utility.IsOrNullTextEx(strText: strBuffer))
            {
                return;
            }

            if (Utility.IsOrNullTextEx(strText: _contentType) && _contentTransferEncoding == null)
            {
                _messageBody.Add(item: GetTextBody(strBuffer: strBuffer));
            }
            else if (_contentType != null && _contentType.IndexOf(value: "digest") >= 0)
            {
                // this is a digest method
                //ParseDigestMessage(strBuffer);
                _messageBody.Add(item: GetTextBody(strBuffer: strBuffer));
            }
            else if (_attachmentboundry2 == null)
            {
                body = GetTextBody(strBuffer: strBuffer);
                if (Utility.IsQuotedPrintable(strText: _contentTransferEncoding))
                {
                    body = DecodeQP.ConvertHexContent(Hexstring: body);
                }
                else if (Utility.IsBase64(strText: _contentTransferEncoding))
                {
                    body = Utility.deCodeB64s(strText: Utility.RemoveNonB64(strText: body));
                }
                else if (Utility.IsNotNullText(strText: _contentCharset))
                {
                    body = Encoding
                        .GetEncoding(name: _contentCharset)
                        .GetString(bytes: Encoding.Default.GetBytes(s: body));
                }
                _messageBody.Add(item: Utility.RemoveNonB64(strText: body));
            }
            else
            {
                begin = 0;
                while (begin != -1)
                {
                    // find "\r\n\r\n" denoting end of header
                    begin = strBuffer.IndexOf(value: "--" + _attachmentboundry2, startIndex: begin);
                    if (begin != -1)
                    {
                        encoding = MIMETypes.GetContentTransferEncoding(
                            strBuffer: strBuffer,
                            pos: begin
                        );
                        string charset = MIMETypes.GetContentCharset(
                            strBuffer: strBuffer,
                            pos: begin
                        );
                        begin = strBuffer.IndexOf(value: "\r\n\r\n", startIndex: begin + 1); //strBuffer.LastIndexOfAny(ALPHABET.ToCharArray());

                        // find end of text
                        end = strBuffer.IndexOf(
                            value: "--" + _attachmentboundry2,
                            startIndex: begin + 1
                        );
                        if (begin != -1)
                        {
                            if (end != -1)
                            {
                                begin += 4;
                                if (begin >= end)
                                {
                                    continue;
                                }

                                if (
                                    this._contentEncoding != null
                                    && this._contentEncoding.IndexOf(value: "8bit") != -1
                                )
                                {
                                    body = Utility.Change(
                                        strText: strBuffer.Substring(
                                            startIndex: begin,
                                            length: end - begin - 2
                                        ),
                                        strCharset: _contentCharset
                                    );
                                }
                                else
                                {
                                    body = strBuffer.Substring(
                                        startIndex: begin,
                                        length: end - begin - 2
                                    );
                                }
                            }
                            else
                            {
                                body = strBuffer.Substring(startIndex: begin);
                            }
                            if (Utility.IsQuotedPrintable(strText: encoding))
                            {
                                string ret = body;
                                ret = DecodeQP.ConvertHexContent(Hexstring: ret, encoding: charset);
                                _messageBody.Add(item: ret);
                            }
                            else if (Utility.IsBase64(strText: encoding))
                            {
                                string ret = Utility.RemoveNonB64(strText: body);
                                ret = Utility.deCodeB64s(strText: ret, strEncoding: charset);
                                if (ret != "\0")
                                {
                                    _messageBody.Add(item: ret);
                                }
                                else
                                {
                                    _messageBody.Add(item: body);
                                }
                            }
                            else
                            {
                                _messageBody.Add(item: body);
                            }

                            if (end == -1)
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (_messageBody.Count == 0)
                        {
                            _messageBody.Add(item: strBuffer);
                        }
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Utility.LogError(strText: "GetMessageBody():" + e.Message);
            _messageBody.Add(item: Utility.deCodeB64s(strText: strBuffer));
        }
        if (_messageBody.Count > 1)
        {
            _html = true;
        }
    }

    /// <summary>
    /// verify if the message is a report
    /// </summary>
    /// <returns>if it is a report message, return true, else, false</returns>
    public bool IsReport()
    {
        if (Utility.IsNotNullText(strText: _contentType))
        {
            return (_contentType.ToLower().IndexOf(value: "report".ToLower()) != -1);
        }

        return false;
    }

    /// <summary>
    /// verify if the attachment is MIME Email file
    /// </summary>
    /// <param name="attItem">attachment</param>
    /// <returns>if MIME Email file, return true, else, false</returns>
    public bool IsMIMEMailFile(Attachment attItem)
    {
        try
        {
            return (
                attItem.ContentFileName.ToLower().EndsWith(value: ".eml".ToLower())
                || attItem.ContentType.ToLower() == "message/rfc822".ToLower()
            );
        }
        catch (Exception e)
        {
            Utility.LogError(strText: "IsMIMEMailFile():" + e.Message);
            return false;
        }
    }

    /// <summary>
    /// translate pictures url within the body
    /// </summary>
    /// <param name="strBody">message body</param>
    /// <param name="hsbFiles">pictures collection</param>
    /// <returns>translated message body</returns>
    public string TranslateHTMLPictureFiles(string strBody, Hashtable hsbFiles)
    {
        try
        {
            for (int i = 0; i < this.AttachmentCount; i++)
            {
                Attachment att = this.GetAttachment(intAttachmentNumber: i);
                if (Utility.IsPictureFile(strFile: att.ContentFileName) == true)
                {
                    if (Utility.IsNotNullText(strText: att.ContentID))
                    {
                        //support for embedded pictures
                        strBody = strBody.Replace(
                            oldValue: "cid:" + att.ContentID,
                            newValue: hsbFiles[key: att.ContentFileName].ToString()
                        );
                    }

                    strBody = strBody.Replace(
                        oldValue: att.ContentFileName,
                        newValue: hsbFiles[key: att.ContentFileName].ToString()
                    );
                }
            }
        }
        catch (Exception e)
        {
            Utility.LogError(strText: "TranslateHTMLPictureFiles():" + e.Message);
        }
        return strBody;
    }

    /// <summary>
    /// translate pictures url within the body
    /// </summary>
    /// <param name="strBody">message body</param>
    /// <param name="strPath">path of the pictures</param>
    /// <returns>translated message body</returns>
    public string TranslateHTMLPictureFiles(string strBody, string strPath)
    {
        try
        {
            if (!strPath.EndsWith(value: "\\"))
            {
                strPath += "\\";
            }
            for (int i = 0; i < this.AttachmentCount; i++)
            {
                Attachment att = this.GetAttachment(intAttachmentNumber: i);
                if (Utility.IsPictureFile(strFile: att.ContentFileName) == true)
                {
                    if (Utility.IsNotNullText(strText: att.ContentID))
                    {
                        //support for embedded pictures
                        strBody = strBody.Replace(
                            oldValue: "cid:" + att.ContentID,
                            newValue: strPath + att.ContentFileName
                        );
                    }

                    strBody = strBody.Replace(
                        oldValue: att.ContentFileName,
                        newValue: strPath + att.ContentFileName
                    );
                }
            }
        }
        catch (Exception e)
        {
            Utility.LogError(strText: "TranslateHTMLPictureFiles():" + e.Message);
        }
        return strBody;
    }

    /// <summary>
    /// Get the proper attachment file name
    /// </summary>
    /// <param name="attItem">attachment</param>
    /// <returns>propery attachment file name</returns>
    public string GetAttachmentFileName(Attachment attItem)
    {
        int items = 0;
        //return unique body file names
        for (int i = 0; i < _attachments.Count; i++)
        {
            if (attItem.ContentFileName == attItem.DefaultFileName)
            {
                items++;
                attItem.ContentFileName = attItem.DefaultFileName2.Replace(
                    oldValue: "*",
                    newValue: items.ToString()
                );
            }
        }
        string name = attItem.ContentFileName;

        return (
            name == null || name == ""
                ? (
                    IsReport() == true
                        ? (
                            this.IsMIMEMailFile(attItem: attItem) == true
                                ? attItem.DefaultMIMEFileName
                                : attItem.DefaultReportFileName
                        )
                        : (attItem.ContentID != null ? attItem.ContentID : attItem.DefaultFileName)
                )
                : name
        );
    }

    /// <summary>
    /// save attachments to a defined path
    /// </summary>
    /// <param name="strPath">path to have attachments to be saved to</param>
    /// <returns>true if save successfully, false if failed</returns>
    public bool SaveAttachments(string strPath)
    {
        if (Utility.IsNotNullText(strText: strPath))
        {
            try
            {
                bool blnRet = true;
                if (!strPath.EndsWith(value: "\\"))
                {
                    strPath += "\\";
                }
                for (int i = 0; i < this.Attachments.Count; i++)
                {
                    Attachment att = GetAttachment(intAttachmentNumber: i);
                    blnRet = SaveAttachment(
                        attItem: att,
                        strFileName: strPath + GetAttachmentFileName(attItem: att)
                    );
                    if (!blnRet)
                    {
                        break;
                    }
                }
                return blnRet;
            }
            catch (Exception e)
            {
                Utility.LogError(strText: e.Message);
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// save attachment to file
    /// </summary>
    /// <param name="attItem">Attachment</param>
    /// <param name="strFileName">File to be saved to</param>
    /// <returns>true if save successfully, false if failed</returns>
    public bool SaveAttachment(Attachment attItem, string strFileName)
    {
        byte[] da;
        try
        {
            //				FileStream fs=File.Create(strFileName);
            //				byte[] da;
            //				if(attItem.ContentFileName.Length>0)
            //				{
            //					da=attItem.DecodedAttachment;
            //				}
            //				else
            //				{
            //					this.GetMessageBody(attItem.DecodeAttachmentAsText());
            //					da=Encoding.Default.GetBytes((string)this.MessageBody[this.MessageBody.Count-1]);
            //				}
            //				fs.Write(da,0,da.Length);
            //				fs.Close();
            //				return true;
            if (attItem.InBytes)
            {
                da = attItem.RawBytes;
            }
            else if (attItem.ContentFileName.Length > 0)
            {
                da = attItem.DecodedAttachment;
            }
            else if (attItem.ContentType.ToLower() == "message/rfc822".ToLower())
            {
                da = Encoding.Default.GetBytes(s: attItem.RawAttachment);
            }
            else
            {
                this.GetMessageBody(strBuffer: attItem.DecodeAsText());
                da = Encoding.Default.GetBytes(s: MessageBody[index: MessageBody.Count - 1]);
            }
            return Utility.SaveByteContentToFile(strFile: strFileName, bytContent: da);
        }
        catch
        {
            /*Utility.LogError("SaveAttachment():"+e.Message);
            return false;*/
            da = Encoding.Default.GetBytes(s: attItem.RawAttachment);
            return Utility.SaveByteContentToFile(strFile: strFileName, bytContent: da);
        }
    }

    /// <summary>
    /// set attachments
    /// </summary>
    private void set_attachments()
    {
        int indexOf_attachmentstart = 0;
        int indexOfAttachmentEnd = 0;
        bool processed = false;
        Attachment att = null;
        SetAttachmentBoundry2(strBuffer: _rawMessageBody);
        while (!processed)
        {
            if (Utility.IsNotNullText(strText: _attachmentboundry))
            {
                indexOf_attachmentstart =
                    _rawMessageBody.IndexOf(
                        value: _attachmentboundry,
                        startIndex: indexOf_attachmentstart
                    ) + _attachmentboundry.Length;
                if (_rawMessageBody == "" || indexOf_attachmentstart < 0)
                {
                    return;
                }

                indexOfAttachmentEnd = _rawMessageBody.IndexOf(
                    value: _attachmentboundry,
                    startIndex: indexOf_attachmentstart + 1
                );
            }
            else
            {
                indexOfAttachmentEnd = -1;
            }
            //if(indexOfAttachmentEnd<0)return;
            if (indexOfAttachmentEnd != -1) { }
            else if (indexOfAttachmentEnd == -1 && !processed && _attachmentCount == 0)
            {
                processed = true;
                indexOfAttachmentEnd = _rawMessageBody.Length;
            }
            else
            {
                return;
            }

            if (indexOf_attachmentstart == indexOfAttachmentEnd - 9)
            {
                indexOf_attachmentstart = 0;
                processed = true;
            }
            string strLine = _rawMessageBody.Substring(
                startIndex: indexOf_attachmentstart,
                length: (indexOfAttachmentEnd - indexOf_attachmentstart - 2)
            );
            bool isMSTNEF;
            isMSTNEF = MIMETypes.IsMSTNEF(strContentType: _contentType);
            att = new Attachment(
                strAttachment: strLine.Trim(),
                strContentType: _contentType,
                blnParseHeader: !isMSTNEF
            );
            //ms-tnef format might contain multiple attachments
            if (
                MIMETypes.IsMSTNEF(strContentType: att.ContentType)
                && AutoDecodeMSTNEF
                && !isMSTNEF
            )
            {
                Utility.LogError(strText: "set_attachments():found ms-tnef file");
                TNEFParser tnef = new TNEFParser();
                TNEFAttachment tatt = new TNEFAttachment();
                Attachment attNew = null;

                tnef.Verbose = false;
                tnef.BasePath = this.BasePath;
                //tnef.LogFilePath=this.BasePath + "OpenPOP.TNEF.log";
                if (tnef.OpenTNEFStream(bytContents: att.DecodedAsBytes()))
                {
                    if (tnef.Parse())
                    {
                        for (
                            IDictionaryEnumerator i = tnef.Attachments().GetEnumerator();
                            i.MoveNext();

                        )
                        {
                            tatt = (TNEFAttachment)i.Value;
                            attNew = new Attachment(
                                bytAttachment: tatt.FileContent,
                                lngFileLength: tatt.FileLength,
                                strFileName: tatt.FileName,
                                strContentType: MIMETypes.GetMimeType(strFileName: tatt.FileName)
                            );
                            _attachmentCount++;
                            _attachments.Add(item: attNew);
                        }
                    }
                    else
                    {
                        Utility.LogError(strText: "set_attachments():ms-tnef file parse failed");
                    }
                }
                else
                {
                    Utility.LogError(strText: "set_attachments():ms-tnef file open failed");
                }
            }
            else
            {
                _attachmentCount++;
                _attachments.Add(item: att);
            }
            indexOf_attachmentstart++;
        }
    }

    /// <summary>
    /// Set alternative attachment boundry
    /// </summary>
    /// <param name="strBuffer">raw message</param>
    private void SetAttachmentBoundry2(string strBuffer)
    {
        int indexOfAttachmentBoundry2Begin = 0;
        int indexOfAttachmentBoundry2End = 0;
        indexOfAttachmentBoundry2Begin = strBuffer
            .ToLower()
            .IndexOf(value: "Multipart/Alternative".ToLower());
        if (indexOfAttachmentBoundry2Begin != -1)
        {
            /*				indexOfAttachmentBoundry2Begin=strBuffer.IndexOf("boundary=\"");
                            indexOfAttachmentBoundry2End=strBuffer.IndexOf("\"",indexOfAttachmentBoundry2Begin+10);
                            if(indexOfAttachmentBoundry2Begin!=-1&&indexOfAttachmentBoundry2End!=-1)
                                _attachmentboundry2=strBuffer.Substring(indexOfAttachmentBoundry2Begin+10,indexOfAttachmentBoundry2End-indexOfAttachmentBoundry2Begin-10).Trim();
            */
            indexOfAttachmentBoundry2Begin = strBuffer.IndexOf(value: "boundary=");
            if (indexOfAttachmentBoundry2Begin != -1)
            {
                int p = strBuffer.IndexOf(
                    value: "\r\n",
                    startIndex: indexOfAttachmentBoundry2Begin
                );
                string s = strBuffer.Substring(
                    startIndex: indexOfAttachmentBoundry2Begin + 29,
                    length: 4
                );
                indexOfAttachmentBoundry2End = strBuffer.IndexOf(
                    value: "\r\n",
                    startIndex: indexOfAttachmentBoundry2Begin + 9
                );
                if (indexOfAttachmentBoundry2End == -1)
                {
                    indexOfAttachmentBoundry2End = strBuffer.Length;
                }

                _attachmentboundry2 = Utility.RemoveQuote(
                    strText: strBuffer.Substring(
                        startIndex: indexOfAttachmentBoundry2Begin + 9,
                        length: indexOfAttachmentBoundry2End - indexOfAttachmentBoundry2Begin - 9
                    )
                );
            }
        }
        else
        {
            _attachmentboundry2 = _attachmentboundry;
        }
    }

    /// <summary>
    /// Save message content to eml file
    /// </summary>
    /// <param name="strFile"></param>
    /// <returns></returns>
    public bool SaveToMIMEEmailFile(string strFile, bool blnReplaceExists)
    {
        return Utility.SavePlainTextToFile(
            strFile: strFile,
            strText: _rawMessage,
            blnReplaceExists: blnReplaceExists
        );
    }

    /// <summary>
    /// parse multi-line header
    /// </summary>
    /// <param name="sbdBuilder">string builder to hold header content</param>
    /// <param name="srdReader">string reader to get each line of the header</param>
    /// <param name="strValue">first line content</param>
    /// <param name="strLine">reference header line</param>
    /// <param name="alCollection">collection to hold every content line</param>
    private void ParseStreamLines(
        StringBuilder sbdBuilder,
        StringReader srdReader,
        string strValue,
        ref string strLine,
        List<string> alCollection
    )
    {
        string strFormmated;
        int intLines = 0;
        alCollection.Add(item: strValue);
        sbdBuilder.Append(value: strLine);
        strLine = srdReader.ReadLine();
        while (
            strLine.Trim() != ""
            && (strLine.StartsWith(value: "\t") || strLine.StartsWith(value: " "))
        )
        {
            strFormmated = strLine.Substring(startIndex: 1);
            alCollection.Add(item: Utility.DecodeLine(strText: strFormmated));
            sbdBuilder.Append(value: strLine);
            strLine = srdReader.ReadLine();
            intLines++;
        }
        if (strLine != "")
        {
            sbdBuilder.Append(value: strLine);
        }
        else if (intLines == 0)
        {
            strLine = srdReader.ReadLine();
            sbdBuilder.Append(value: strLine);
        }
        ParseHeader(sbdBuilder: sbdBuilder, srdReader: srdReader, strLine: ref strLine);
    }

    /// <summary>
    /// parse multi-line header
    /// </summary>
    /// <param name="sbdBuilder">string builder to hold header content</param>
    /// <param name="srdReader">string reader to get each line of the header</param>
    /// <param name="strName">collection key</param>
    /// <param name="strValue">first line content</param>
    /// <param name="strLine">reference header line</param>
    /// <param name="hstCollection">collection to hold every content line</param>
    private void ParseStreamLines(
        StringBuilder sbdBuilder,
        StringReader srdReader,
        string strName,
        string strValue,
        ref string strLine,
        Hashtable hstCollection
    )
    {
        string strFormmated;
        string strReturn = strValue;
        int intLines = 0;
        //sbdBuilder.Append(strLine);
        strLine = srdReader.ReadLine();
        while (
            strLine.Trim() != ""
            && (strLine.StartsWith(value: "\t") || strLine.StartsWith(value: " "))
        )
        {
            strFormmated = strLine.Substring(startIndex: 1);
            strReturn += Utility.DecodeLine(strText: strFormmated);
            sbdBuilder.Append(value: strLine + "\r\n");
            strLine = srdReader.ReadLine();
            intLines++;
        }
        if (!hstCollection.ContainsKey(key: strName))
        {
            hstCollection.Add(key: strName, value: strReturn);
        }

        if (strLine != "")
        {
            sbdBuilder.Append(value: strLine + "\r\n");
        }
        else if (intLines == 0)
        {
            //					strLine=srdReader.ReadLine();
            //					sbdBuilder.Append(strLine + "\r\n");
        }
        ParseHeader(sbdBuilder: sbdBuilder, srdReader: srdReader, strLine: ref strLine);
    }

    /// <summary>
    /// parse multi-line header
    /// </summary>
    /// <param name="sbdBuilder">string builder to hold header content</param>
    /// <param name="srdReader">string reader to get each line of the header</param>
    /// <param name="strValue">first line content</param>
    /// <param name="strLine">reference header line</param>
    /// <param name="strReturn">return value</param>
    /// <param name="blnLineDecode">decode each line</param>
    private void ParseStreamLines(
        StringBuilder sbdBuilder,
        StringReader srdReader,
        string strValue,
        ref string strLine,
        ref string strReturn,
        bool blnLineDecode
    )
    {
        string strFormmated;
        int intLines = 0;
        strReturn = strValue;
        sbdBuilder.Append(value: strLine + "\r\n");
        if (blnLineDecode == true)
        {
            strReturn = Utility.DecodeLine(strText: strReturn);
        }

        strLine = srdReader.ReadLine();
        while (
            strLine.Trim() != ""
            && (strLine.StartsWith(value: "\t") || strLine.StartsWith(value: " "))
        )
        {
            strFormmated = strLine.Substring(startIndex: 1);
            strReturn += (
                blnLineDecode == true
                    ? Utility.DecodeLine(strText: strFormmated)
                    : "\r\n" + strFormmated
            );
            sbdBuilder.Append(value: strLine + "\r\n");
            strLine = srdReader.ReadLine();
            intLines++;
        }
        if (strLine != "")
        {
            sbdBuilder.Append(value: strLine + "\r\n");
        }
        else if (intLines == 0)
        {
            strLine = srdReader.ReadLine();
            sbdBuilder.Append(value: strLine + "\r\n");
        }
        if (!blnLineDecode)
        {
            strReturn = Utility.RemoveWhiteBlanks(strText: Utility.DecodeText(strText: strReturn));
        }

        ParseHeader(sbdBuilder: sbdBuilder, srdReader: srdReader, strLine: ref strLine);
    }

    /// <summary>
    /// Parse the headers populating respective member fields
    /// </summary>
    /// <param name="sbdBuilder">string builder to hold the header content</param>
    /// <param name="srdReader">string reader to get each line of the header</param>
    /// <param name="strLine">reference header line</param>
    private void ParseHeader(StringBuilder sbdBuilder, StringReader srdReader, ref string strLine)
    {
        string[] array = Utility.GetHeadersValue(strRawHeader: strLine); //Regex.Split(strLine,":");
        switch (array[0].ToUpper())
        {
            case "TO":
            {
                _to = array[1].Split(separator: ',');
                for (int i = 0; i < _to.Length; i++)
                {
                    _to[i] = Utility.DecodeLine(strText: _to[i].Trim());
                }
                break;
            }

            case "CC":
            {
                _cc = array[1].Split(separator: ',');
                for (int i = 0; i < _cc.Length; i++)
                {
                    _cc[i] = Utility.DecodeLine(strText: _cc[i].Trim());
                }
                break;
            }

            case "BCC":
            {
                _bcc = array[1].Split(separator: ',');
                for (int i = 0; i < _bcc.Length; i++)
                {
                    _bcc[i] = Utility.DecodeLine(strText: _bcc[i].Trim());
                }
                break;
            }

            case "FROM":
            {
                Utility.ParseEmailAddress(
                    strEmailAddress: array[1],
                    strUser: ref _from,
                    strAddress: ref _fromEmail
                );
                break;
            }

            case "REPLY-TO":
            {
                Utility.ParseEmailAddress(
                    strEmailAddress: array[1],
                    strUser: ref _replyTo,
                    strAddress: ref _replyToEmail
                );
                break;
            }

            case "KEYWORDS": //ms outlook keywords
            {
                ParseStreamLines(
                    sbdBuilder: sbdBuilder,
                    srdReader: srdReader,
                    strValue: array[1].Trim(),
                    strLine: ref strLine,
                    alCollection: _keywords
                );
                break;
            }

            case "RECEIVED":
            {
                ParseStreamLines(
                    sbdBuilder: sbdBuilder,
                    srdReader: srdReader,
                    strValue: array[1].Trim(),
                    strLine: ref strLine,
                    strReturn: ref _received,
                    blnLineDecode: true
                );
                break;
            }

            case "IMPORTANCE":
            {
                _importance = array[1].Trim();
                break;
            }

            case "DISPOSITION-NOTIFICATION-TO":
            {
                _dispositionNotificationTo = array[1].Trim();
                break;
            }

            case "MIME-VERSION":
            {
                _mimeVersion = array[1].Trim();
                break;
            }

            case "SUBJECT":
            case "THREAD-TOPIC":
            {
                string strRet = null;
                for (int i = 1; i < array.Length; i++)
                {
                    strRet += array[i];
                }
                ParseStreamLines(
                    sbdBuilder: sbdBuilder,
                    srdReader: srdReader,
                    strValue: strRet,
                    strLine: ref strLine,
                    strReturn: ref _subject,
                    blnLineDecode: false
                );
                break;
            }

            case "RETURN-PATH":
            {
                _returnPath = array[1].Trim().Trim(trimChars: '>').Trim(trimChars: '<');
                break;
            }

            case "MESSAGE-ID":
            {
                _messageID = array[1].Trim().Trim(trimChars: '>').Trim(trimChars: '<');
                break;
            }

            case "DATE":
            {
                for (int i = 1; i < array.Length; i++)
                {
                    _dateTimeInfo += array[i];
                }
                _dateTimeInfo = _dateTimeInfo.Trim();
                _date = Utility.ParseEmailDate(strDate: _dateTimeInfo);
                break;
            }

            case "CONTENT-LENGTH":
            {
                _contentLength = Convert.ToInt32(value: array[1]);
                break;
            }

            case "CONTENT-TRANSFER-ENCODING":
            {
                _contentTransferEncoding = array[1].Trim();
                break;
            }

            case "CONTENT-TYPE":
            {
                //if already content type has been assigned
                if (_contentType != null)
                {
                    return;
                }

                strLine = array[1];
                _contentType = strLine.Split(separator: ';')[0];
                _contentType = _contentType.Trim();
                int intCharset = strLine.IndexOf(value: "charset=");
                if (intCharset != -1)
                {
                    int intBound2 = strLine
                        .ToLower()
                        .IndexOf(value: ";", startIndex: intCharset + 8);
                    if (intBound2 == -1)
                    {
                        intBound2 = strLine.Length;
                    }

                    intBound2 -= (intCharset + 8);
                    _contentCharset = strLine.Substring(
                        startIndex: intCharset + 8,
                        length: intBound2
                    );
                    _contentCharset = Utility.RemoveQuote(strText: _contentCharset);
                }
                else
                {
                    intCharset = strLine.ToLower().IndexOf(value: "report-type=".ToLower());
                    if (intCharset != -1)
                    {
                        int intPos = strLine.IndexOf(value: ";", startIndex: intCharset + 13);
                        _reportType = strLine.Substring(
                            startIndex: intCharset + 12,
                            length: intPos - intCharset - 13
                        );
                    }
                    else if (strLine.ToLower().IndexOf(value: "boundary=".ToLower()) == -1)
                    {
                        strLine = srdReader.ReadLine();
                        if (strLine == "")
                        {
                            return;
                        }

                        intCharset = strLine.ToLower().IndexOf(value: "charset=".ToLower());
                        if (intCharset != -1)
                        {
                            _contentCharset = strLine.Substring(
                                startIndex: intCharset + 9,
                                length: strLine.Length - intCharset - 10
                            );
                        }
                        else if (strLine.IndexOf(value: ":") != -1)
                        {
                            sbdBuilder.Append(value: strLine + "\r\n");
                            ParseHeader(
                                sbdBuilder: sbdBuilder,
                                srdReader: srdReader,
                                strLine: ref strLine
                            );
                            return;
                        }
                        else
                        {
                            sbdBuilder.Append(value: strLine + "\r\n");
                        }
                    }
                }
                if (_contentType == "text/plain")
                {
                    return;
                }

                if (
                    _contentType.ToLower() == "text/html"
                    || _contentType.ToLower().IndexOf(value: "multipart/") != -1
                )
                {
                    _html = true;
                }

                if (
                    strLine.Trim().Length == _contentType.Length + 1
                    || strLine.ToLower().IndexOf(value: "boundary=".ToLower()) == -1
                )
                {
                    strLine = srdReader.ReadLine();
                    if (strLine == null || strLine == "" || strLine.IndexOf(value: ":") != -1)
                    {
                        sbdBuilder.Append(value: strLine + "\r\n");
                        ParseHeader(
                            sbdBuilder: sbdBuilder,
                            srdReader: srdReader,
                            strLine: ref strLine
                        );
                        return;
                    }

                    sbdBuilder.Append(value: strLine + "\r\n");
                    if (strLine.ToLower().IndexOf(value: "boundary=".ToLower()) == -1)
                    {
                        _attachmentboundry = srdReader.ReadLine();
                        sbdBuilder.Append(value: _attachmentboundry + "\r\n");
                    }
                    _attachmentboundry = strLine;
                }
                else
                {
                    /*if(strLine.IndexOf(";")!=-1)
                        _attachmentboundry=strLine.Split(';')[1];
                    else*/
                    _attachmentboundry = strLine;
                }
                int intBound = _attachmentboundry.ToLower().IndexOf(value: "boundary=");
                if (intBound != -1)
                {
                    int intBound2 = _attachmentboundry
                        .ToLower()
                        .IndexOf(value: ";", startIndex: intBound + 10);
                    if (intBound2 == -1)
                    {
                        intBound2 = _attachmentboundry.Length;
                    }

                    intBound2 -= (intBound + 9);
                    _attachmentboundry = _attachmentboundry.Substring(
                        startIndex: intBound + 9,
                        length: intBound2
                    );
                }
                _attachmentboundry = Utility.RemoveQuote(strText: _attachmentboundry);
                _hasAttachment = true;
                break;
            }

            default:
            {
                if (array.Length > 1) //here we parse all custom headers
                {
                    string headerName = array[0].Trim();
                    if (headerName.ToUpper().StartsWith(value: "X")) //every custom header starts with "X"
                    {
                        ParseStreamLines(
                            sbdBuilder: sbdBuilder,
                            srdReader: srdReader,
                            strName: headerName,
                            strValue: array[1].Trim(),
                            strLine: ref strLine,
                            hstCollection: _customHeaders
                        );
                    }
                }
                break;
            }
        }
    }
}
