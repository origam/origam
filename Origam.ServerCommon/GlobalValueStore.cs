using System;
using System.Collections.Generic;
using System.Text;

namespace Origam.ServerCommon
{
    class GlobalValueStore
    {
        private static GlobalValueStore instance;
        private  static readonly Dictionary<string, object> valueStore = 
            new Dictionary<string, object>();
        public static GlobalValueStore Instance
            => instance ?? (instance = new GlobalValueStore());


        public void Add(string key, object value)
        {
            valueStore.Add(key, value);
        }
    }
}
