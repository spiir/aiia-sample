using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Aiia.Sample.Utilities;

public class ObjectTableBuilding
{
    
    public static IEnumerable<KeyValuePair<string,string>> FlattenObject(JToken o, string prefix)
    {
        IEnumerable<KeyValuePair<string, string>> HandleObject(JObject obj)
        {
            var newPrefix = prefix + (string.IsNullOrEmpty(prefix) ? "" : ".");
            
            return obj
                .Select<KeyValuePair<string, JToken>, IEnumerable<KeyValuePair<string, string>>>(
                    kv=> FlattenObject(kv.Value, newPrefix + kv.Key))
                .SelectMany(dict => dict);
        }

        IEnumerable<KeyValuePair<string, string>> HandleArray(JArray array)
        {
            var items = array
                .Select((token, index) => FlattenObject(token, prefix + "[" + index + "]"))
                .SelectMany(dict => dict);
            return new Dictionary<string, string>(items);
        }
        
        IEnumerable<KeyValuePair<string, string>> HandleValue(JValue val)
        {
            if (val.Value is not null)
                return new KeyValuePair<string, string>[] { new(prefix, val.Value.ToString()) };
            else
                return new KeyValuePair<string, string>[] { new(prefix, "<null>") };
        }   

        return o switch
        {
            JObject obj => HandleObject(obj),
            JValue val => HandleValue(val),
            JArray array => HandleArray(array),
            _ => throw new NotImplementedException($"Not implemented json element {o.Type}")
        };
    }


}