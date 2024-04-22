#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.IO;
using System.Reflection;
using System.Xml;
using EDIFACT2XML;
using log4net;
using System.Text;
using System.IO.Compression;

namespace Origam.Workflow;

public class EDIFACT2XMLServiceAgent : AbstractServiceAgent
{
    private static readonly ILog log
        = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private object result;
    public override object Result => result;

    public override void Run()
    {
            EDIFACTDataElement.Tag = "e";
            switch(MethodName)
            {
                case "ParseString":
                    ParseString();
                    break;
                case "ParseFile":
                    ParseFile();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        "MethodName", MethodName,
                        ResourceUtils.GetString("InvalidMethodName"));
            }
        }
    private void ParseFile()
    {
            ValidateParseFileParameters();
            using (EDIFACTParser parser = new EDIFACTParser())
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("Initializing parser...");
                }
                parser.Init(
                    GetMessageFormat(),
                    EDIFACTMessageGrammar.FromString(Parameters["Grammar"] as string),
                    GetStreamReader());
                if(log.IsDebugEnabled)
                {
                    log.DebugFormat(
                        "Processing input file {0}...",Parameters.ContainsKey("Filename") ? Parameters["Filename"] : "");
                }
                GenerateXMLFiles(parser);
            }
        }

    private StreamReader GetStreamReader()
    {
            FileInfo fileInfo = new FileInfo(Parameters["Filename"] as string);
            switch(fileInfo.Extension)
            {
                case ".gz":
                    return new StreamReader(
                        new GZipStream(
                            File.OpenRead(Parameters["Filename"] as string), 
                            CompressionMode.Decompress));
                default:
                    return new StreamReader(Parameters["Filename"] as string);
            }
        }

    private void GenerateXMLFiles(EDIFACTParser parser)
    {
            StreamWriter writer = null;
            DateTime timestamp = DateTime.Now;
            int counter = 0;
            int position = 0;
            int limit = 0;
            if(Parameters["Limit"] != null)
            {
                limit = (int)Parameters["Limit"];
            }
            string filename = GetOutputFilename(timestamp, counter);
            if(log.IsDebugEnabled)
            {
                log.DebugFormat("Generating output file {0}...", filename);
            }
            try
            {
                writer = new StreamWriter(filename, false);
                writer.WriteLine("<EDIFACT>");
                EDIFACTSegment segment = parser.GetNextTopLevelSegment();
                while (segment != null)
                {
                    if ((limit != 0) && (position > 0) && ((position % 1000) == 0))
                    {
                        writer.Write("</EDIFACT>");
                        writer.Close();
                        counter++;
                        filename = GetOutputFilename(timestamp, counter);
                        if(log.IsDebugEnabled)
                        {
                            log.DebugFormat(
                                "Generating output file {0}...", filename);
                        }
                        writer = new StreamWriter(filename, false);
                        writer.WriteLine("<EDIFACT>");
                    }
                    segment.ToXML(writer);
                    position++;
                    segment = parser.GetNextTopLevelSegment();
                }
                writer.Write("</EDIFACT>");
                writer.Close();
                if(log.IsDebugEnabled)
                {
                    log.Debug("Done...");
                }
            }
            finally
            {
                if(writer != null)
                {
                    writer.Close();
                }
            }
        }
    private string GetOutputFilename(DateTime timestamp, int counter)
    {
            FileInfo fileInfo = new FileInfo(Parameters["Filename"] as string);
            StringBuilder output = new StringBuilder();
            output.Append(Parameters["OutputFolder"] as string);
            if(!(Parameters["OutputFolder"] as string).EndsWith("\\"))
            {
                output.Append("\\");
            }
            output.Append(Path.GetFileNameWithoutExtension(fileInfo.Name));
            output.Append("_");
            output.Append(timestamp.ToString("yyyyMMddHHmmss"));
            output.Append("_");
            output.Append(String.Format("{0:000}", counter));
            output.Append(".xml");
            return output.ToString();
        }
    private void ValidateParseFileParameters()
    {
            if(!(Parameters["Grammar"] is string))
            {
                throw new InvalidCastException(
                    ResourceUtils.GetString("ErrorGrammarNotString"));
            }
            if(!(Parameters["Filename"] is string))
            {
                throw new InvalidCastException(
                    ResourceUtils.GetString("ErrorFilenameNotString"));
            }
            if (!(Parameters["OutputFolder"] is string))
            {
                throw new InvalidCastException(
                    ResourceUtils.GetString("ErrorOutputFolderNotString"));
            }
            if (!((Parameters["Limit"] == null) 
            || (Parameters["Limit"] is int)))
            {
                throw new InvalidCastException(
                    ResourceUtils.GetString("ErrorLimitNotInt"));
            }
        }
    private void ParseString()
    {
            ValidateParseStringParameters();
            using(EDIFACTParser parser = new EDIFACTParser())
            {
                if(log.IsDebugEnabled)
                {
                    log.Debug("Initializing parser...");
                }
                parser.Init(
                    GetMessageFormat(),
                    EDIFACTMessageGrammar.FromString(Parameters["Grammar"] as string),
                    new StringReader(Parameters["Message"] as string));
                StringWriter writer = new StringWriter();
                writer.WriteLine("<EDIFACT>");
                EDIFACTSegment segment = parser.GetNextTopLevelSegment();
                while(segment != null)
                {
                    segment.ToXML(writer);
                    segment = parser.GetNextTopLevelSegment();
                }
                writer.WriteLine("</EDIFACT>");
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(writer.ToString());
                result = xmlDocument;
            }
        }

    private void ValidateParseStringParameters()
    {
            if(!(Parameters["Grammar"] is string))
            {
                throw new InvalidCastException(
                    ResourceUtils.GetString("ErrorGrammarNotString"));
            }
            if(!(Parameters["Message"] is string))
            {
                throw new InvalidCastException(
                    ResourceUtils.GetString("ErrorMessageNotString"));
            }
        }

    private EDIFACTMessageFormat GetMessageFormat()
    {
            EDIFACTMessageFormat messageFormat = new EDIFACTMessageFormat();
            messageFormat.SegmentTerminator = 0x1C; // '
            messageFormat.AlternativeSegmentTerminator = 0x1A;
            messageFormat.DataElementSeparator = 0x1D; // +
            messageFormat.ComponentDataElementSeparator = 0x1F; // :
            messageFormat.RepetitionDataElementSeparator = 0x19; // *
            return messageFormat;
        }
        
}