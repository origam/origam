#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

            var doc = new OrigamXmlDocument();
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
