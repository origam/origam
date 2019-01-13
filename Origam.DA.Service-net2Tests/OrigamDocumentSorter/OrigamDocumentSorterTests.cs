using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Framework;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.TestCommon;

namespace Origam.DA.Service_net2Tests
{
    [TestFixture]
    class OrigamDocumentSorterTests : AbstractFileTestClass
    {
        [Test]
        public void ShouldSortAtributes()
        {

            var doc = new XmlDocument();
            string path = Path.Combine(TestFilesDir.FullName, "Unsorted.origam");
            string xml = File.ReadAllText(path);
            doc.LoadXml(xml);

            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                NewLineOnAttributes = true
            };

            string sortedDoc = OrigamDocumentSorter
                .CopyAndSort(doc)
                .ToBeautifulString(xmlWriterSettings);
            File.WriteAllText(Path.Combine(TestFilesDir.FullName, "Sorted.origam"), sortedDoc);
        }

        protected override TestContext TestContext => TestContext.CurrentContext;
        protected override string DirName => "OrigamDocumentSorter";
    }
}
