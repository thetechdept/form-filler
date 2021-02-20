using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Techdept.FormFiller.Functions
{
    public static class Defaults
    {
        static Defaults()
        {
            JsonSerializerSettings = new JsonSerializerSettings { 
                ContractResolver = new CamelCasePropertyNamesContractResolver() 
            };
            JsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }

        public static JsonSerializerSettings JsonSerializerSettings;
    }
}