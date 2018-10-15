using System;
using System.IO;
using System.Text;
using Origam.DA.ObjectPersistence;
using Origam.DA.ObjectPersistence.Providers;
using static Origam.DA.ObjectPersistence.ExternalFileExtension;

namespace Origam.DA.Service
{
    internal abstract class ExternalFileWriter
    {
        public static ExternalFileWriter GetNew(ExternalFilePath filePath)
        {
            switch (filePath.Extension)
            {
                case Txt:
                case Xml:
                case Xslt:
                    return new TextFileWriter(filePath);
                case Bin:
                case Png:
                    return new BinaryFileWriter(filePath);
                default:    
                    throw new NotImplementedException();
            }
        }
        
        private readonly ExternalFilePath externalFilePath;

        protected ExternalFileWriter(ExternalFilePath externalFilePath)
        {
            this.externalFilePath = externalFilePath;
        }

        public void Write(object data)
        {
            if (!externalFilePath.Directory.Exists)
            {
                externalFilePath.Directory.Create();
            }
            WriteData(externalFilePath.Absolute, data);
        }

        public object Read()
        {
            return ReadData(externalFilePath.Absolute);
        }

        protected abstract void WriteData(string path,object dataToWrite);
        protected abstract object ReadData(string path);
    }

    internal class TextFileWriter: ExternalFileWriter 
    {
        public TextFileWriter(ExternalFilePath externalFilePath) : base(externalFilePath)
        {
        }

        protected override void WriteData(string path, object text)
        {
            if (! (text is string))
            {
                throw new ArgumentOutOfRangeException("text must be a string.");
            }
            File.WriteAllText(path, text as string, Encoding.UTF8);
        }

        protected override object ReadData(string path)
        {
            return File.ReadAllText(path,Encoding.UTF8);
        }
    }

    internal class BinaryFileWriter: ExternalFileWriter
    {
        public BinaryFileWriter(ExternalFilePath filePath) : base(filePath)
        {
        }

        protected override void WriteData(string path,object dataToWrite)
        {
            File.WriteAllBytes(path, (byte[])dataToWrite);
        }

        protected override object ReadData(string path)
        {
            return File.ReadAllBytes(path);
        }
    }
}