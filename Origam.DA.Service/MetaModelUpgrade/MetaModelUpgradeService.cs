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
using System.Xml;
using System.Xml.Linq;
using CSharpFunctionalExtensions;
using Origam.Extensions;
using Origam.Workbench.Services;

namespace Origam.DA.Service.MetaModelUpgrade
{

    public interface IMetaModelUpgradeService : IWorkbenchService
    {
        event EventHandler<UpgradeProgressInfo> UpgradeProgress;
        event EventHandler UpgradeStarted;
        event EventHandler UpgradeFinished;
        List<XmlFileData> Upgrade(List<XmlFileData> xmlFileData);
        void Cancel();
    }

    public class NullMetaModelUpgradeService : IMetaModelUpgradeService
    {
        public event EventHandler<UpgradeProgressInfo> UpgradeProgress;
        public event EventHandler UpgradeStarted;
        public event EventHandler UpgradeFinished;
        public void InitializeService()
        {
        }

        public void UnloadService()
        {
        }

        public List<XmlFileData> Upgrade(List<XmlFileData> xmlFileData)
        {
            return xmlFileData;
        }

        public void Cancel()
        {
        }
    }

    public class MetaModelUpgradeService: IMetaModelUpgradeService
    {
        private MetaModelUpGrader metaModelUpGrader;
        public event EventHandler<UpgradeProgressInfo> UpgradeProgress;
        public event EventHandler UpgradeStarted;
        public event EventHandler UpgradeFinished;
        private bool canceled;

        public List<XmlFileData> Upgrade(List<XmlFileData> xmlFileData)
        {
            List<XFileData> xFileData = xmlFileData
                .Select(fileData => new XFileData(fileData))
                .ToList();
            metaModelUpGrader = new MetaModelUpGrader();
            
            metaModelUpGrader.UpgradeStarted += OnUpgradeStarted;
            metaModelUpGrader.UpgradeFinished += OnUpgradeFinished;
            metaModelUpGrader.UpgradeProgress += OnUpgradeProgress;
            metaModelUpGrader.TryUpgrade(xFileData);
            metaModelUpGrader.UpgradeProgress -= OnUpgradeProgress;
            metaModelUpGrader.UpgradeStarted -= OnUpgradeStarted;
            metaModelUpGrader.UpgradeFinished -= OnUpgradeFinished;

            if (canceled)
            {
                return new List<XmlFileData>();
            }
            return xFileData
                .Select(fileData => new XmlFileData(fileData))
                .ToList();
        }

        public void Cancel()
        {
            canceled = true;
            metaModelUpGrader?.Cancel();
        }

        private void OnUpgradeFinished(object sender, EventArgs e)
        {
            UpgradeFinished?.Invoke(null, EventArgs.Empty);
        }

        private void OnUpgradeStarted(object sender, EventArgs e)
        {
            UpgradeStarted?.Invoke(null, EventArgs.Empty);
        }

        private void OnUpgradeProgress(object sender, UpgradeProgressInfo info)
        {
            UpgradeProgress?.Invoke(null, info);
        }

        public void InitializeService()
        {
        }

        public void UnloadService()
        {
        }
    }

    public class XFileData
    {
        public OrigamXDocument Document { get; }
        public FileInfo File { get; }


        public XFileData(XmlFileData xmlFileData)
            : this(xmlFileData.XmlDocument, xmlFileData.FileInfo)
        {
        }

        public XFileData(OrigamXmlDocument xmlDocument, FileInfo file)
        {
            Document = new OrigamXDocument(xmlDocument);
            File = file;
        }

        public XFileData(OrigamXDocument document, FileInfo file)
        {
            Document = document;
            File = file;
        }
    }
}