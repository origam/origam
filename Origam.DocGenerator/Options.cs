using CommandLine;
using CommandLine.Text;
using System;

namespace Origam.DocGenerator
{
    public class Options
    {
        public const string XmlCommandName = "xml";
        public const string XsltCommandName = "xslt";
        [VerbOption(Options.XmlCommandName, HelpText = "Generate Xml source.")]
        public XmlSubOptions Xml { get; set; }
        [VerbOption(Options.XsltCommandName, HelpText = "Generate output with xslt template")]
        public XsltSubOptions Xslt { get; set; }
        public BaseSubOption baseoptions;
        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            var help = new HelpText
            {
                Copyright = string.Format(Strings.ShortGNU, System.Reflection.Assembly.GetEntryAssembly().GetName().Name),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            object optionsObject = null;
            if (verb == XmlCommandName)
            {
                help.AddPreOptionsLine(Environment.NewLine + "Usage: Origam.DocGenerator xml ");
                optionsObject = new XmlSubOptions();
            }
            else if (verb == XsltCommandName)
            {
                help.AddPreOptionsLine(Environment.NewLine + "Usage: Origam.DocGenerator xslt ");
                optionsObject = new XsltSubOptions();
            }

            if (optionsObject != null)
            {
                help.AddOptions(optionsObject); ;
            }
            else
            {
                help.AddDashesToOption = false;
                help.AddOptions(this);
            }
            return help;
        }
    }
    public class BaseSubOption
    {
        [Option('s', "schema", Required = true, HelpText = "Input Origam project directory.")]
        public string Schema { get; set; }
        [Option('p', "packageid", Required = true, HelpText = "guidid of package.")]
        public string GuidPackage { get; set; }
        [Option('o', "output", Required = true, HelpText = "Output directory")]
        public string Dataout { get; set; }
        [Option('l', "language", Required = true, HelpText = "Localization(ie. cs-CZ).")]
        public string Language { get; set; }
    }
    public class XmlSubOptions : BaseSubOption
    {
        [Option('m', "xmlfilename", Required = false, HelpText = "Xml File for export source tree.")]
        public string XmlFile { get; set; }
    }
    public class XsltSubOptions : BaseSubOption
    {
        [Option('x', "xslt", Required = true, HelpText = "Xslt template")]
        public string Xslt { get; set; }
        [Option('r', "rootfilename", Required = true, HelpText = "Output File")]
        public string RootFile { get; set; }
    }
    public class ConfigOption
    {
        public string Schema { get; set; }
        public string GuidPackage { get; set; }
        public string Dataout { get; set; }
        public string Language { get; set; }
        public string XmlFile { get; set; }
        public string Xslt { get; set; }
        public string RootFile { get; set; }

        internal void SetOption(XmlSubOptions SubOptions)
        {
            Schema = SubOptions.Schema;
            GuidPackage = SubOptions.GuidPackage;
            Dataout = SubOptions.Dataout;
            Language = SubOptions.Language;
            XmlFile = SubOptions.XmlFile;
            Xslt = null;
            RootFile = null;
        }
        internal void SetOption(XsltSubOptions SubOptions)
        {
            Schema = SubOptions.Schema;
            GuidPackage = SubOptions.GuidPackage;
            Dataout = SubOptions.Dataout;
            Language = SubOptions.Language;
            XmlFile = null;
            Xslt = SubOptions.Xslt;
            RootFile = SubOptions.RootFile;
        }
    }
}
