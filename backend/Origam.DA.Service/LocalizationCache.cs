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
using System.Collections;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using Origam.Extensions;

namespace Origam.DA.ObjectPersistence;

/// <summary>
/// Summary description for LocalizationCache.
/// </summary>
public class LocalizationCache : ILocalizationCache, IDisposable
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private readonly Hashtable languages = new Hashtable();
    private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

    public LocalizationCache()
    {
        rwLock.RunWriter(LoadFromLocalizationFolder);
    }

    /// <summary>
    /// Initializes the localization cache with a single file.
    /// </summary>
    /// <param name="filePath">Path to the file where 5 last characeters of the file name must define a locale
    /// e.g. "MyPackageName-en-US.xml</param>
    public LocalizationCache(string filePath)
    {
        rwLock.RunWriter(() => LoadFile(filePath));
    }

    private void LoadFromLocalizationFolder()
    {
#if !ORIGAM_CLIENT
        return;
#else
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        if (settings.LocalizationFolder == String.Empty)
        {
            if (log.IsInfoEnabled)
            {
                log.Info("Localization folder not specified.");
            }
            return;
        }
        else
        {
            if (log.IsInfoEnabled)
            {
                log.Info(
                    "Loading localization strings from folder: " + settings.LocalizationFolder
                );
            }
        }
        DirectoryInfo di = new DirectoryInfo(settings.LocalizationFolder);
        if (di.Exists)
        {
            foreach (FileInfo fi in di.GetFiles("*.xml"))
            {
                if (log.IsInfoEnabled)
                {
                    log.Info("Loading localization file: " + fi.FullName);
                }
                LoadFile(fi.FullName);
            }
        }
        else
        {
            if (log.IsErrorEnabled)
            {
                log.Error("Localization folder specified in OrigamSettings not found.");
            }
            throw new ArgumentOutOfRangeException(
                "LocalizationFolder",
                settings.LocalizationFolder,
                "Localization folder specified in OrigamSettings not found."
            );
        }
#endif
    }

    /// <summary>
    /// Loads a file.
    /// </summary>
    /// <param name="path">Path to the file where 5 last characeters of the file name must define a locale
    /// e.g. "MyPackageName-en-US.xml</param>
    private void LoadFile(string path)
    {
        string locale = Path.GetFileNameWithoutExtension(path);
        if (locale.Length != 5)
        {
            locale = locale.Substring(locale.Length - 5);
        }
        Hashtable language = (Hashtable)languages[locale];
        if (language == null)
        {
            language = new Hashtable();
            languages.Add(locale, language);
        }
        XmlDocument doc = new XmlDocument();
        doc.Load(path);
        XPathNavigator nav = doc.CreateNavigator();
        XPathNodeIterator it = nav.Evaluate("/OrigamLocalization/Element") as XPathNodeIterator;
        while (it.MoveNext())
        {
            string id = it.Current.GetAttribute("Id", "");
            Hashtable element = new Hashtable();
            Guid guidId = new Guid(id);
            if (language.Contains(guidId))
            {
                throw new System.ArgumentException(
                    string.Format(
                        "Cannot add localization key {0} from file {1}. The key is already loaded.",
                        id,
                        path
                    )
                );
            }
            language.Add(guidId, element);
            if (it.Current.MoveToFirstChild())
            {
                do
                {
                    string memberName = it.Current.Name;
                    if (memberName == "Documentation")
                    {
                        memberName = memberName + " " + it.Current.GetAttribute("Category", "");
                    }
                    string memberValue = it.Current.Value;
                    if (element.ContainsKey(memberName))
                    {
                        throw new Exception(
                            $"Error when loading file {path}. Element id {id} contains category {memberName} multiple times. Please remove the duplicates from the file and try again."
                        );
                    }
                    element.Add(memberName, memberValue);
                } while (it.Current.MoveToNext());
            }
        }
    }

    public string GetLocalizedString(Guid elementId, string memberName, string defaultString)
    {
        string locale = Thread.CurrentThread.CurrentUICulture.Name;
        return GetLocalizedString(elementId, memberName, defaultString, locale);
    }

    public void Reload()
    {
        rwLock.RunWriter(ReloadCache);
    }

    private void ReloadCache()
    {
        languages.Clear();
        LoadFromLocalizationFolder();
    }

    public string GetLocalizedString(
        Guid elementId,
        string memberName,
        string defaultString,
        string locale
    )
    {
        return rwLock.RunReader(() =>
            GetLocalizedStringPrivate(elementId, memberName, defaultString, locale)
        );
    }

    private string GetLocalizedStringPrivate(
        Guid elementId,
        string memberName,
        string defaultString,
        string locale
    )
    {
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(
                "Resolving localized string. ElementId: {0}, Member: {1}, Default: {2}, Locale: {3}",
                elementId,
                memberName,
                defaultString,
                locale
            );
        }
        if (languages.Contains(locale))
        {
            Hashtable elements = languages[locale] as Hashtable;
            if (elements.Contains(elementId))
            {
                Hashtable members = elements[elementId] as Hashtable;
                if (members.Contains(memberName))
                {
                    return members[memberName] as string;
                }
                else
                {
                    return defaultString;
                }
            }
            else
            {
                return defaultString;
            }
        }
        else
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat(
                    "Locale {0} not available, returning default string \"{1}\"",
                    locale,
                    defaultString
                );
            }
            return defaultString;
        }
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            rwLock?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
