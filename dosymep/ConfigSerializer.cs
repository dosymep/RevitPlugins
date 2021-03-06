using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using pyRevitLabs.Json;

using dosymep.Bim4Everyone.ProjectConfigs;

namespace dosymep.Serializers {
    internal class ConfigSerializer : IConfigSerializer {
        public T Deserialize<T>(string text) {
            return JsonConvert.DeserializeObject<T>(text);
        }

        public string Serialize<T>(T @object) {
            return JsonConvert.SerializeObject(@object);
        }
    }
}