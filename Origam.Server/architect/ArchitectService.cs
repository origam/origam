#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using FluorineFx;
using Origam.Workbench.Services;
using Origam.UI;
using Origam.Schema;
using System.Collections;

namespace Origam.Server.Architect
{
    [RemotingService]
    public class ArchitectService
    {
        private const string NODETYPE_PROVIDER = "PROVIDER";
        private const string NODETYPE_SCHEMA_ITEM = "SCHEMA_ITEM";
        private const string NODETYPE_GROUP = "GROUP";
        private const string NODETYPE_ANCESTOR_GROUP = "ANCESTOR_GROUP";
        private const string NODETYPE_ANCESTOR = "ANCESTOR";

        public IList<ModelBrowserElement> ModelBrowserRootElements(string perspectiveId)
        {
            SchemaService ss = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            List<ModelBrowserElement> result = new List<ModelBrowserElement>();

            switch (perspectiveId)
            {
                case "cz.advantages.architect.perspectives.data":
                    result.Add(BrowserElement(ss.GetProvider("Origam.Schema.EntityModel.EntityModelSchemaItemProvider")));
                    result.Add(BrowserElement(ss.GetProvider("Origam.Schema.EntityModel.DataStructureSchemaItemProvider")));
                    break;

                default:
                    break;
            }

            return result;
        }

        public IList<ModelBrowserElement> ModelBrowserElements(string type, string parentId)
        {
            IBrowserNode2 node;

            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            SchemaService ss = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

            switch (type)
            {
                case NODETYPE_ANCESTOR:
                    node = ps.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(new Guid(parentId))) as IBrowserNode2;
                    break;

                case NODETYPE_ANCESTOR_GROUP:
                    node = ps.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(new Guid(parentId))) as IBrowserNode2;
                    foreach (IBrowserNode2 child in node.ChildNodes())
                    {
                        if (child is NonpersistentSchemaItemNode)
                        {
                            node = child;
                        }
                    }
                    break;

                case NODETYPE_GROUP:
                    node = ps.SchemaProvider.RetrieveInstance(typeof(SchemaItemGroup), new ModelElementKey(new Guid(parentId))) as IBrowserNode2;
                    break;

                case NODETYPE_PROVIDER:
                    node = ss.GetProvider(parentId);
                    break;

                case NODETYPE_SCHEMA_ITEM:
                    node = ps.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(new Guid(parentId))) as IBrowserNode2;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("type", type, "Unknown type");
            }

            List<ModelBrowserElement> result = new List<ModelBrowserElement>();

            ArrayList childNodes = new ArrayList(node.ChildNodes());
            childNodes.Sort();

            foreach (IBrowserNode2 childNode in childNodes)
            {
                result.Add(BrowserElement(childNode));
            }
            

            return result;
        }

        public ModelBrowserElement BrowserElement(IBrowserNode2 node)
        {
            ModelBrowserElement result = new ModelBrowserElement(node.NodeId, ResolveType(node), node.NodeText, node.Icon, node.HasChildNodes, true);

            return result;
        }

        public string ResolveType(object o)
        {
            if (o is ISchemaItem)
            {
                return NODETYPE_SCHEMA_ITEM;
            }
            else if (o is SchemaItemGroup)
            {
                return NODETYPE_GROUP;
            }
            else if (o is NonpersistentSchemaItemNode)
            {
                return NODETYPE_ANCESTOR_GROUP;
            }
            else if (o is ISchemaItemProvider)
            {
                return NODETYPE_PROVIDER;
            }
            else
            {
                throw new ArgumentOutOfRangeException("object", o, "Unknown type");
            }
        }
    }
}
