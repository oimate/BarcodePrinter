using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace BarcodePrinter.Cfg
{
    public partial class Config
    {
        [JsonProperty("triggerBit")]
        public string TriggerTag { get; set; }

        [JsonProperty("responseBit")]
        public string ResponseTag { get; set; }

        [JsonProperty("skidId")]
        public string SkidIdTag { get; set; }

        [JsonProperty("errorBit")]
        public string ErrorTag { get; set; }

        [JsonProperty("server")]
        public string Server { get; set; }

        [JsonProperty("db")]
        public string Db { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("pwd")]
        public string Pwd { get; set; }
    }

    public partial class Config
    {
        public static Config FromJson(string json) => JsonConvert.DeserializeObject<Config>(json, BarcodePrinter.Cfg.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Config self) => JsonConvert.SerializeObject(self, BarcodePrinter.Cfg.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}