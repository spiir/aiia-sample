using System.Collections.Generic;
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