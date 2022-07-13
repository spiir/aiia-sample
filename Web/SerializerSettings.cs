using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Aiia.Sample;

public static class SerializerSettings
{
    private static JsonSerializerSettings _settings = null;

    /// <summary>
    /// Singleton for the json serialization settings used to interact with Aiia.
    /// </summary>
    public static JsonSerializerSettings Settings
    {
        get
        {
            if (_settings == null)
            {
                _settings = new JsonSerializerSettings()
                {
                    Formatting = Formatting.None, Converters = new List<JsonConverter>()
                    {
                        new StringEnumConverter(),
                        new IsoDateTimeConverter()
                    },
                };
            }

            return _settings;
        }
    }

}