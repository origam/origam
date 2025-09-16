using System;
using System.Resources;
using System.Globalization;
using System.Reflection;

namespace Origam.Workflow
{
    internal static class Strings
    {
        private static readonly ResourceManager resourceManager = new ResourceManager("Origam.Workflow.Strings", Assembly.GetExecutingAssembly());
        private static CultureInfo resourceCulture;
        public static CultureInfo Culture { get => resourceCulture; set => resourceCulture = value; }
        private static string Get(string name) => resourceManager.GetString(name, resourceCulture) ?? name;
        public static string ErrorParsingConnectionString => Get("ErrorParsingConnectionString");
        public static string UnknownReadType => Get("UnknownReadType");
        public static string ArchiveEntryTooLarge => Get("ArchiveEntryTooLarge");
        public static string ArchiveEntrySuspiciousCompressionRatio => Get("ArchiveEntrySuspiciousCompressionRatio");
        public static string StreamExceedsAllowedSize => Get("StreamExceedsAllowedSize");
        public static string ErrorInvalidConnectionString => Get("ErrorInvalidConnectionString");
        public static string ErrorNoPath => Get("ErrorNoPath");
        public static string ErrorNoSearchPattern => Get("ErrorNoSearchPattern");
        public static string ErrorSplitFileByRows => Get("ErrorSplitFileByRows");
        public static string SplitBinaryFilesNotSupported => Get("SplitBinaryFilesNotSupported");
        public static string AggregateCompressedFilesButNoCompression => Get("AggregateCompressedFilesButNoCompression");
        public static string EntryNotFoundInArchive => Get("EntryNotFoundInArchive");
        public static string EntryOutsideTargetDir => Get("EntryOutsideTargetDir");
        public static string ErrorNoIndexFile => Get("ErrorNoIndexFile");
    }
}
