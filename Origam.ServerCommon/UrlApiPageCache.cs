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

﻿using Origam.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using Origam.Schema.GuiModel;

namespace Origam.ServerCommon
{
    class UrlApiPageCache
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, AbstractPage> parameterlessPages = null;
        private List<AbstractPage> parameterPages = null;

        public UrlApiPageCache(List<AbstractSchemaItem> pages)
        {
            parameterlessPages = new Dictionary<string, AbstractPage>();
            parameterPages = new List<AbstractPage>();
            foreach (AbstractPage page in pages)
            {
                if (page.Url.Contains("{"))
                {
                    parameterPages.Add(page);
                }
                else
                {
                    if (parameterlessPages.ContainsKey(page.Url))
                    {
                        throw new OrigamException(
                            string.Format(
                                "Can't initialize API Url resolver. Duplicate API route '{0}'",
                                page.Url));
                    }
                    parameterlessPages.Add(page.Url, page);
                }
            }
            if (log.IsDebugEnabled)
            {
                log.Debug("parameterless URL Api resolver initialised.");
            }
        }

        public AbstractPage GetParameterlessPage(string incommingPath)
        {            
            return parameterlessPages.ContainsKey(incommingPath) ?
                parameterlessPages[incommingPath]
                : null;
        }
        
        public List<AbstractPage> GetParameterPages()
        {
            return parameterPages;
        }
    }
}
