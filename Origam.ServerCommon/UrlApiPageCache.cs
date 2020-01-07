using Origam.Schema;
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
