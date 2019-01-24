using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;

namespace Origam.Gui
{
    public static class FormTools
    {
        public static bool IsFormMenuReadOnly(FormReferenceMenuItem formRef)
        {
            bool result = formRef.ReadOnlyAccess;

            if (!result)
            {
                string authContext = SecurityManager.GetReadOnlyRoles(formRef.AuthorizationContext);
                result = SecurityManager.GetAuthorizationProvider().Authorize(SecurityManager.CurrentPrincipal, authContext);
            }

            return result;
        }

        public static ControlSetItem GetItemFromControlSet(AbstractControlSet controlSet)
        {
            ArrayList children = new ArrayList(controlSet.Alternatives);
            children.Sort(new AlternativeControlSetItemComparer());

            foreach (ControlSetItem item in children)
            {
                if (IsValid(item.Features, item.Roles))
                {
                    return item;
                }
            }

            return controlSet.MainItem;
        }

        public static bool IsValid(string features, string roles)
        {
            IParameterService parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
            if (!parameterService.IsFeatureOn(features)) return false;
            if (roles != null && roles != String.Empty)
            {
                if (!SecurityManager.GetAuthorizationProvider().Authorize(SecurityManager.CurrentPrincipal, roles))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get read only status.
        /// If parent was read only, it will be re-examined here. That means, that when there is a menu item with ReadOnly
        /// set to "true" and there exist some fields or complete panels/groups/tabs inside that form that have a "Roles" property
        /// set, these might get not-read-only, unless they are also set ReadOnly in the user's security settings.
        /// </summary>
        /// <param name="cntrlSet"></param>
        /// <returns></returns>
        public static bool GetReadOnlyStatus(ControlSetItem cntrlSet, bool currentReadOnlyStatus)
        {
            if (cntrlSet.Roles != "" && cntrlSet.Roles != null)
            {
                OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();

                if (settings.ActivateReadOnlyRoles)
                {
                    string authContext = SecurityManager.GetReadOnlyRoles(cntrlSet.Roles);
                    return SecurityManager.GetAuthorizationProvider().Authorize(SecurityManager.CurrentPrincipal, authContext);
                }
            }

            return currentReadOnlyStatus;
        }

        public static string FindTableByDataMember(DataSet ds, string member)
        {
            if (member == null) return "";
            if (ds == null) return "";

            string tableName = "";

            if (member.IndexOf(".") > 0)
            {
                string[] path = member.Split(".".ToCharArray());
                DataTable table = ds.Tables[path[0]];

                if (table == null)
                {
                    throw new Exception(ResourceUtils.GetString("ErrorGenerateForm", path[0]));
                }

                for (int i = 1; i < path.Length; i++)
                {
                    if (table.ChildRelations.Count > 0 &&
                        table.ChildRelations[path[i]] != null &&
                        table.ChildRelations[path[i]].ChildTable != null)
                    {
                        table = table.ChildRelations[path[i]].ChildTable;
                    }
                    else
                    {
                        // if editing screen sections the last part of the member is
                        // the column name so we try to find it in the last table
                        if (!table.Columns.Contains(path[i]))
                        {
                            throw (new ArgumentOutOfRangeException("DataMember", String.Format("Could not find entity `{0}' in data structure id `{1}'.", path[i], ds.ExtendedProperties["Id"])));
                        }
                    }
                }
                tableName = table.TableName;
            }
            else
                tableName = member;

            return tableName;

        }


    }
}
