// using System;
// using System.IO;
// using Newtonsoft.Json;
//
// namespace Origam.Schema.WorkflowModel
// {
//     public interface ITraceConfig
//     {
//         Trace GetTraceOption(string path);
//     }
//
//     public class TraceConfig : ITraceConfig
//     {
//         private static readonly ITraceConfig instace = new TraceConfig();
//
//         private readonly string configPath = "origamWorkflowTraceConfig.json";
//         public static ITraceConfig Instance => instace;
//         public Trace GetTraceOption(string path)
//         {
//             throw new System.NotImplementedException();
//         }
//
//         public TraceConfig()
//         {
//             // if (!File.Exists(configPath))
//             // {
//             //     throw new Exception("The trace config \"{configPath}\" was not found");
//             // }
//
//             Product deserializedProduct = JsonConvert.DeserializeObject<Product>(output);
//         }
//     }
// }