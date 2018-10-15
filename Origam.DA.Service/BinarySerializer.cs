using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Origam.DA.ObjectPersistence.Providers
{
    public class BinarySerializer<T>
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Save(T toSerialize,string pathToOutFile)
        {
            var bformatter = new BinaryFormatter();
            var fileStream = new FileStream(
                pathToOutFile,
                FileMode.Create,
                FileAccess.Write, 
                FileShare.None);
            
            using (fileStream)  
            {  
                bformatter.Serialize(fileStream, toSerialize);  
            }  
        }

        public T Load(string pathToInFile)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            var bformatter = new BinaryFormatter();
            var fileStream = new FileStream(
                pathToInFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None);
            
            object deserializedObject;
            try
            {
                using (fileStream)
                {
                    deserializedObject = bformatter.Deserialize(fileStream);
                }
            } catch (SerializationException ex)
            {
                if (log.IsInfoEnabled)
                {
                    log.Info($"A SerializationException was caught when trying to load {typeof(T)} form {pathToInFile}.");
                }
                return default(T);
            }
            
            sw.Stop();
            Console.WriteLine("Serialization finished in: {0}",sw.Elapsed);
            
            return (T)deserializedObject;
        }
    }
}