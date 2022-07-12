using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Aiia.Sample.AiiaClient.Models;
using Aiia.Sample.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aiia.Sample.Models;

public class ObjectDetailsViewModel
{
    public ObjectDetailsViewModel(object item)
    {
        Item = item;
    }

    public object Item { get; set; }

    public IEnumerable<KeyValuePair<string, string>> FlattenedObjectProperties => ObjectTableBuilding.FlattenObject(JObject.FromObject(Item, SerializerSettings), "");

    private JsonSerializer SerializerSettings => JsonSerializer.CreateDefault(new JsonSerializerSettings()
        { NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include});


}