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
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Xml;
using EDIFACT2XML;
using log4net;

namespace Origam.Workflow;

public class EDIFACT2XMLServiceAgent : AbstractServiceAgent
{
    private static readonly ILog log = LogManager.GetLogger(
        type: MethodBase.GetCurrentMethod().DeclaringType
    );
    private object result;
    public override object Result => result;

    public override void Run()
    {
        EDIFACTDataElement.Tag = "e";
        switch (MethodName)
        {
            case "ParseString":
            {
                ParseString();
                break;
            }

            case "ParseFile":
            {
                ParseFile();
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "MethodName",
                    actualValue: MethodName,
                    message: ResourceUtils.GetString(key: "InvalidMethodName")
                );
            }
        }
    }

    private void ParseFile()
    {
        ValidateParseFileParameters();
        using (EDIFACTParser parser = new EDIFACTParser())
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(message: "Initializing parser...");
            }
            parser.Init(
                messageFormat: GetMessageFormat(),
                messageGrammar: EDIFACTMessageGrammar.FromString(
                    grammar: Parameters[key: "Grammar"] as string
                ),
                input: GetStreamReader()
            );
            if (log.IsDebugEnabled)
            {
                log.DebugFormat(
                    format: "Processing input file {0}...",
                    arg0: Parameters.ContainsKey(key: "Filename") ? Parameters[key: "Filename"] : ""
                );
            }
            GenerateXMLFiles(parser: parser);
        }
    }

    private StreamReader GetStreamReader()
    {
        FileInfo fileInfo = new FileInfo(fileName: Parameters[key: "Filename"] as string);
        switch (fileInfo.Extension)
        {
            case ".gz":
            {
                return new StreamReader(
                    stream: new GZipStream(
                        stream: File.OpenRead(path: Parameters[key: "Filename"] as string),
                        mode: CompressionMode.Decompress
                    )
                );
            }
            default:
            {
                return new StreamReader(path: Parameters[key: "Filename"] as string);
            }
        }
    }

    private void GenerateXMLFiles(EDIFACTParser parser)
    {
        StreamWriter writer = null;
        DateTime timestamp = DateTime.Now;
        int counter = 0;
        int position = 0;
        int limit = 0;
        if (Parameters[key: "Limit"] != null)
        {
            limit = (int)Parameters[key: "Limit"];
        }
        string filename = GetOutputFilename(timestamp: timestamp, counter: counter);
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(format: "Generating output file {0}...", arg0: filename);
        }
        try
        {
            writer = new StreamWriter(path: filename, append: false);
            writer.WriteLine(value: "<EDIFACT>");
            EDIFACTSegment segment = parser.GetNextTopLevelSegment();
            while (segment != null)
            {
                if ((limit != 0) && (position > 0) && ((position % 1000) == 0))
                {
                    writer.Write(value: "</EDIFACT>");
                    writer.Close();
                    counter++;
                    filename = GetOutputFilename(timestamp: timestamp, counter: counter);
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat(format: "Generating output file {0}...", arg0: filename);
                    }
                    writer = new StreamWriter(path: filename, append: false);
                    writer.WriteLine(value: "<EDIFACT>");
                }
                segment.ToXML(writer: writer);
                position++;
                segment = parser.GetNextTopLevelSegment();
            }
            writer.Write(value: "</EDIFACT>");
            writer.Close();
            if (log.IsDebugEnabled)
            {
                log.Debug(message: "Done...");
            }
        }
        finally
        {
            if (writer != null)
            {
                writer.Close();
            }
        }
    }

    private string GetOutputFilename(DateTime timestamp, int counter)
    {
        FileInfo fileInfo = new FileInfo(fileName: Parameters[key: "Filename"] as string);
        StringBuilder output = new StringBuilder();
        output.Append(value: Parameters[key: "OutputFolder"] as string);
        if (!(Parameters[key: "OutputFolder"] as string).EndsWith(value: "\\"))
        {
            output.Append(value: "\\");
        }
        output.Append(value: Path.GetFileNameWithoutExtension(path: fileInfo.Name));
        output.Append(value: "_");
        output.Append(value: timestamp.ToString(format: "yyyyMMddHHmmss"));
        output.Append(value: "_");
        output.Append(value: String.Format(format: "{0:000}", arg0: counter));
        output.Append(value: ".xml");
        return output.ToString();
    }

    private void ValidateParseFileParameters()
    {
        if (!(Parameters[key: "Grammar"] is string))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorGrammarNotString")
            );
        }
        if (!(Parameters[key: "Filename"] is string))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorFilenameNotString")
            );
        }
        if (!(Parameters[key: "OutputFolder"] is string))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorOutputFolderNotString")
            );
        }
        if (!((Parameters[key: "Limit"] == null) || (Parameters[key: "Limit"] is int)))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorLimitNotInt")
            );
        }
    }

    private void ParseString()
    {
        ValidateParseStringParameters();
        using (EDIFACTParser parser = new EDIFACTParser())
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(message: "Initializing parser...");
            }
            parser.Init(
                messageFormat: GetMessageFormat(),
                messageGrammar: EDIFACTMessageGrammar.FromString(
                    grammar: Parameters[key: "Grammar"] as string
                ),
                input: new StringReader(s: Parameters[key: "Message"] as string)
            );
            StringWriter writer = new StringWriter();
            writer.WriteLine(value: "<EDIFACT>");
            EDIFACTSegment segment = parser.GetNextTopLevelSegment();
            while (segment != null)
            {
                segment.ToXML(writer: writer);
                segment = parser.GetNextTopLevelSegment();
            }
            writer.WriteLine(value: "</EDIFACT>");
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml: writer.ToString());
            result = xmlDocument;
        }
    }

    private void ValidateParseStringParameters()
    {
        if (!(Parameters[key: "Grammar"] is string))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorGrammarNotString")
            );
        }
        if (!(Parameters[key: "Message"] is string))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorMessageNotString")
            );
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
