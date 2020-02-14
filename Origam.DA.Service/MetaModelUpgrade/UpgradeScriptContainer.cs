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
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace Origam.DA.Service.MetaModelUpgrade
{  
    class UpgradeScriptContainer<T>
    {
        protected readonly List<UpgradeScript> upgradeScripts = new List<UpgradeScript>();
        
        public void Upgrade(XmlDocument doc, XmlNode classNode, Version fromVersion, Version toVersion)
        {
            var scriptsToRun = upgradeScripts
                .Where(script => script.FromVersion >= fromVersion && script.ToVersion <= toVersion)
                .OrderBy(script => script.FromVersion)
                .ToList();

            if (scriptsToRun.Count == 0)
            {
                throw new Exception($"There is no script to upgrade class {typeof(T).Name} from version {fromVersion} to {toVersion}");
            }
            if (scriptsToRun[0].FromVersion != fromVersion)
            {
                throw new Exception($"Script to upgrade class {typeof(T).Name} from version {fromVersion} to the next version was not found");
            }
            if (scriptsToRun.Last().ToVersion != toVersion)
            {
                throw new Exception($"Script to upgrade class {typeof(T).Name} to version {toVersion} was not found");
            }

            CheckScriptsFormContinuousSequence(scriptsToRun);

            foreach (var upgradeScript in scriptsToRun)
            {
                upgradeScript.Upgrade(classNode, doc);
            }

            SetVersion(classNode, toVersion);
        }

        private static void CheckScriptsFormContinuousSequence(List<UpgradeScript> scriptsToRun)
        {
            for (int i = 0; i < scriptsToRun.Count - 1; i++)
            {
                if (scriptsToRun[i].ToVersion != scriptsToRun[i + 1].FromVersion)
                {
                    throw new ClassUpgradeException(
                        $"There is no script to upgrade class {typeof(T).Name} from version {scriptsToRun[i].ToVersion} to {scriptsToRun[i + 1].FromVersion}");
                }
            }
        }

        private void SetVersion(XmlNode classNode, Version toVersion)
        {
            ((XmlElement) classNode).SetAttribute(
                "version",
                toVersion.ToString());
        }
    }
    
    [DebuggerDisplay("Form: {FromVersion}, To: {ToVersion}")]
    public class UpgradeScript
    {
        public Version FromVersion { get; }
        public Version ToVersion { get;}

        private readonly Func<XmlNode, XmlDocument, XmlNode> transformation;

        public UpgradeScript(Version fromVersion, Version toVersion,
            Func<XmlNode, XmlDocument, XmlNode> transformation)
        {
            this.transformation = transformation;
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }

        public void Upgrade(XmlNode classNode, XmlDocument doc)
        {
            transformation(classNode, doc);
        } 
    }
}