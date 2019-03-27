#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using log4net;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Threading;
using Microsoft.Web.Administration;
using Antlr.StringTemplate;
using System.Data.SqlClient;
using Antlr.StringTemplate.Language;
using System.Text.RegularExpressions;
using System.Data;

namespace Origam.hosting.inf
{
    [WebService(Namespace = "http://manager.simplicor.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Manager : System.Web.Services.WebService
    {
        private static readonly ILog log = LogManager.GetLogger(
            typeof(Manager));

        private static readonly string SITE = 
            ConfigurationManager.AppSettings["origamvdm_site"];

        private static readonly string APP_POOL =
            ConfigurationManager.AppSettings["origamvdm_pool"];

        private static readonly string PHYSICAL_PATH =
            ConfigurationManager.AppSettings["origamvdm_physical_path"];

        private static readonly string CONFIG_PATH =
            ConfigurationManager.AppSettings["origamvdm_config_path"];

        [WebMethod]
        public string CreateNewInstance(Guid companyId)
        {
            string result = "";
            log.InfoFormat("Request to create instance for company id:{0}.", 
                companyId);
            try
            {
                String defaultDBServerConnectionString;
                String createScriptPath;
                String defaultRole;

                log.Info("Loading configuration file...");
                using (FileStream stream = File.OpenRead(CONFIG_PATH))
                {
                    XmlDocument xmlConfig = new XmlDocument();
                    xmlConfig.Load(stream);
                    defaultDBServerConnectionString = xmlConfig.DocumentElement
                        .SelectSingleNode("defaultServerConnectionString").InnerText;
                    createScriptPath = xmlConfig.DocumentElement.SelectSingleNode(
                        "dbCreateScriptLocation").InnerText;
                    defaultRole = xmlConfig.DocumentElement.SelectSingleNode(
                        "defaultCreatorUserRole").InnerText;
                }
                log.Info("Configuration file loaded.");

                log.Info("Running non transactional script...");
                RunNonTsCreateScript(companyId, defaultDBServerConnectionString,
                    createScriptPath);
                log.Info("Non transactional script run.");

                log.Info("Running transactional script...");
                RunTsCreateScript(companyId, defaultDBServerConnectionString,
                    createScriptPath);
                log.Info("Transactional sript run.");

                log.Info("Creating web application...");
                CreateCompanyWebApplication(companyId);
                log.Info("Web application created.");
                log.Info("Company instance created.");

                result = defaultRole;
            }
            catch (Exception ex)
            {
                log.Fatal("Failed to create instance for company.", ex);
                throw new Exception("Failed to create instance for company.", ex);
            }
            return result;
        }
        private void CreateCompanyWebApplication(Guid companyId)
        {
            ServerManager serverManager = new ServerManager();
            serverManager.Sites[SITE].Applications.Add("/" + companyId, 
                PHYSICAL_PATH);
            serverManager.Sites[SITE].Applications["/" + companyId]
                .ApplicationPoolName = APP_POOL;
            serverManager.CommitChanges();
            // if there's no wait here - we end up with object moved
            Thread.Sleep(5000);
            // serverManager.Sites[SITE].Bindings[0].EndPoint.Port
        }

        private void RunTsCreateScript(Guid companyId, 
            String defaultDBServerConnectionString, String createScriptPath)
        {
            String sql;
            StringTemplateGroup group = new StringTemplateGroup("createScripts",
                createScriptPath, typeof (DefaultTemplateLexer));
            StringTemplate createScriptTemplate = group.GetInstanceOf(
                "createScriptTs");
            createScriptTemplate.SetAttribute("companyId", companyId.ToString()
                                                               .Replace("-", "_"));
            sql = createScriptTemplate.ToString();

            SqlConnection sqlConnection = new SqlConnection(defaultDBServerConnectionString);
            // todo: change database
            sqlConnection.Open();

            Regex regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] lines = regex.Split(sql);

            SqlTransaction transaction = sqlConnection.BeginTransaction();
            using (SqlCommand cmd = sqlConnection.CreateCommand())
            {
                foreach (string line in lines)
                {
                    if (line.Length > 0)
                    {
                        cmd.Transaction = transaction;
                        cmd.CommandText = line;
                        cmd.CommandType = CommandType.Text;
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            transaction.Commit();
            sqlConnection.Close();
        }

        private void RunNonTsCreateScript(Guid companyId, 
            String defaultDBServerConnectionString, String createScriptPath)
        {
            String sql;
            StringTemplateGroup group = new StringTemplateGroup("createScripts", 
                createScriptPath, typeof(DefaultTemplateLexer));
            StringTemplate createScriptTemplate = group.GetInstanceOf(
                "createScriptNonTs");
            createScriptTemplate.SetAttribute("companyId", companyId.ToString()
                .Replace("-", "_"));
            sql = createScriptTemplate.ToString();

            SqlConnection sqlConnection = new SqlConnection(defaultDBServerConnectionString);
            sqlConnection.Open();
            Regex regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] lines = regex.Split(sql);

            using (SqlCommand cmd = sqlConnection.CreateCommand())
            {
                foreach (string line in lines)
                {
                    if (line.Length > 0)
                    {
                        cmd.CommandText = line;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            sqlConnection.Close();
        }
    }
}