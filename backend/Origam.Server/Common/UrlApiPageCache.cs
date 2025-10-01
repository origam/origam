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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.Server;

class UrlApiPageCache
{
    class PageCacheForOneCulture
    {
        public Dictionary<string, AbstractPage> parameterlessPages;
        public List<AbstractPage> parameterPages;

        public PageCacheForOneCulture()
        {
            parameterlessPages = new Dictionary<string, AbstractPage>();
            parameterPages = new List<AbstractPage>();
        }
    }

    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private Dictionary<string, PageCacheForOneCulture> pageCachesForPerCultures = null;
    private PagesSchemaItemProvider pageProvider = null;
    private readonly Mutex lck = new Mutex();

    public UrlApiPageCache(PagesSchemaItemProvider _pageProvider)
    {
        pageProvider = _pageProvider;
        pageCachesForPerCultures = new Dictionary<string, PageCacheForOneCulture>();
    }

    public AbstractPage GetParameterlessPage(string incommingPath)
    {
        CultureInfo currentCulture = Thread.CurrentThread.CurrentUICulture;
        lck.WaitOne();
        if (!pageCachesForPerCultures.ContainsKey(currentCulture.Name))
        {
            BuildCacheForCurrentCulture(currentCulture);
        }
        PageCacheForOneCulture cur = pageCachesForPerCultures[currentCulture.Name];
        AbstractPage ret = cur.parameterlessPages.ContainsKey(incommingPath)
            ? cur.parameterlessPages[incommingPath]
            : null;
        lck.ReleaseMutex();
        return ret;
    }

    public List<AbstractPage> GetParameterPages()
    {
        CultureInfo currentCulture = Thread.CurrentThread.CurrentUICulture;
        lck.WaitOne();
        if (!pageCachesForPerCultures.ContainsKey(currentCulture.Name))
        {
            BuildCacheForCurrentCulture(currentCulture);
        }
        PageCacheForOneCulture cur = pageCachesForPerCultures[currentCulture.Name];
        List<AbstractPage> ret = cur.parameterPages;
        lck.ReleaseMutex();
        return ret;
    }

    private void BuildCacheForCurrentCulture(CultureInfo currentCulture)
    {
        PageCacheForOneCulture newCulture = new PageCacheForOneCulture();
        foreach (AbstractPage page in pageProvider.ChildItems.ToList())
        {
            if (page.Url.Contains("{"))
            {
                newCulture.parameterPages.Add(page);
            }
            else
            {
                if (newCulture.parameterlessPages.ContainsKey(page.Url))
                {
                    throw new OrigamException(
                        string.Format(
                            "Can't initialize API Url resolver. Duplicate API route '{0}'",
                            page.Url
                        )
                    );
                }
                newCulture.parameterlessPages.Add(page.Url, page);
            }
        }
        pageCachesForPerCultures[currentCulture.Name] = newCulture;
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(
                "parameterless URL Api resolver initialised for culture {0}.",
                currentCulture
            );
        }
    }
}
