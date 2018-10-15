using System;
using System.Collections.Generic;
using System.Linq;
using Origam.Schema;

namespace Origam.DA.Service_net2Tests
{
    internal class SchemaItemsToCompare
    {
        public AbstractSchemaItem FromDb { get;} 
        public AbstractSchemaItem FromXml { get;}

        public SchemaItemsToCompare(AbstractSchemaItem fromDb, AbstractSchemaItem fromXml)
        {
            FromDb = fromDb;
            FromXml = fromXml;
        }

        public override string ToString()
        {
            Dictionary<string, object> xmlDict = FromXml.GetAllProperies();
            Dictionary<string, object> dbDict = FromDb.GetAllProperies();
            
            return dbDict.Keys
                .Select(key => $"{key,-25}: {dbDict[key],-70} {xmlDict[key],-70}")
                .Aggregate("", (outString,str) =>
                    outString + str + Environment.NewLine) ;
        }

        public string Type => FromDb.GetType().ToString();
    }
}