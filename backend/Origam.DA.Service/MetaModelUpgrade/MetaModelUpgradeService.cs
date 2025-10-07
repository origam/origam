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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Origam.Workbench.Services;

namespace Origam.DA.Service.MetaModelUpgrade;

public enum MetaModelUpgradeMode
{
    Upgrade,
    ThrowIfOutdated,
    Ignore,
}

public interface IMetaModelUpgradeService : IWorkbenchService
{
    event EventHandler<UpgradeProgressInfo> UpgradeProgress;
    event EventHandler UpgradeStarted;
    event EventHandler UpgradeFinished;
    List<XmlFileData> Upgrade(List<XmlFileData> xmlFileData, MetaModelUpgradeMode mode);
    void Cancel();
}

public class NullMetaModelUpgradeService : IMetaModelUpgradeService
{
#pragma warning disable CS0067
    public event EventHandler<UpgradeProgressInfo> UpgradeProgress;
    public event EventHandler UpgradeStarted;
    public event EventHandler UpgradeFinished;
#pragma warning restore CS0067
    public void InitializeService() { }

    public void UnloadService() { }

    public List<XmlFileData> Upgrade(List<XmlFileData> xmlFileData, MetaModelUpgradeMode mode)
    {
        return xmlFileData;
    }

    public void Cancel() { }
}

public class MetaModelUpgradeService : IMetaModelUpgradeService
{
    private MetaModelAnalyzer metaModelAnalyzer;
    public event EventHandler<UpgradeProgressInfo> UpgradeProgress;
    public event EventHandler UpgradeStarted;
    public event EventHandler UpgradeFinished;
    private bool canceled;
    private int filesProcessed;

    public List<XmlFileData> Upgrade(List<XmlFileData> xmlFileData, MetaModelUpgradeMode mode)
    {
        IMetaModelUpgrader metaModelUpgrader;
        switch (mode)
        {
            case MetaModelUpgradeMode.Ignore:
                return xmlFileData;
            case MetaModelUpgradeMode.Upgrade:
                metaModelUpgrader = new MetaModelUpgrader();
                break;
            case MetaModelUpgradeMode.ThrowIfOutdated:
                metaModelUpgrader = new DisabledMetaModelUpgrader();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Not implemented");
        }
        metaModelAnalyzer = new MetaModelAnalyzer(metaModelUpgrader);
        filesProcessed = 0;
        UpgradeStarted?.Invoke(null, EventArgs.Empty);
        List<XmlFileData> upgradedData = xmlFileData
            .AsParallel()
            .Where(x => !canceled)
            .Select(fileData => Upgrade(fileData, xmlFileData.Count))
            .Where(fileData => fileData.XmlDocument.FileElement.HasChildNodes)
            .ToList();
        UpgradeFinished?.Invoke(null, EventArgs.Empty);

        return canceled ? new List<XmlFileData>() : upgradedData;
    }

    public void Cancel()
    {
        canceled = true;
    }

    public void InitializeService() { }

    public void UnloadService() { }

    private DateTime lastProgressCalled = DateTime.Now;
    private readonly TimeSpan minProgressCallInterval = new TimeSpan(0, 0, 0, 0, 25);

    private XmlFileData Upgrade(XmlFileData fileData, int totalFileCount)
    {
        var xFileData = new XFileData(fileData);
        bool wasUpgraded;
        try
        {
            wasUpgraded = metaModelAnalyzer.TryUpgrade(xFileData);
        }
        catch (Exception ex)
        {
            UpgradeFinished?.Invoke(null, EventArgs.Empty);
            throw new Exception(
                $"An error has occured when processing file: {fileData.FileInfo.FullName}",
                ex
            );
        }
        filesProcessed += 1;
        if (DateTime.Now - lastProgressCalled > minProgressCallInterval)
        {
            UpgradeProgress?.Invoke(null, new UpgradeProgressInfo(totalFileCount, filesProcessed));
            lastProgressCalled = DateTime.Now;
        }
        return wasUpgraded ? new XmlFileData(xFileData) : fileData;
    }
}

public class XFileData
{
    public OrigamXDocument Document { get; }
    public FileInfo File { get; }

    public XFileData(XmlFileData xmlFileData)
        : this(xmlFileData.XmlDocument, xmlFileData.FileInfo) { }

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
