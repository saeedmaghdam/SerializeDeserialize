using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SerializeDeserialize.Domain;
using System.Reflection;

namespace SerializeDeserialize
{
    public class NewtonsoftSerializer
    {
        public string Serialize(Customer customer)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            settings.Converters.Add(new INodeConverter());

            return JsonConvert.SerializeObject(customer, settings);
        }

        public Customer Deserialize(string json)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            settings.Converters.Add(new INodeConverter());

            return JsonConvert.DeserializeObject<Customer>(json, settings);
        }

        private class INodeConverter : JsonConverter
        {
            private readonly Assembly _assembly;

            public INodeConverter()
            {
                _assembly = Assembly.LoadFrom("SerializeDeserialize.Domain.dll");
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(INode).IsAssignableFrom(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject jsonObject = JObject.Load(reader);

                if (jsonObject.TryGetValue("_Type", out var typeToken))
                {
                    var typeName = typeToken.Value<string>();

                    Type targetType = _assembly.GetType(typeName);
                    if (targetType == null)
                    {
                        throw new JsonSerializationException($"Unknown type '{typeName}' during deserialization.");
                    }

                    var targetObject = Activator.CreateInstance(targetType);
                    serializer.Populate(jsonObject.CreateReader(), targetObject);
                    return targetObject;
                }

                throw new JsonSerializationException("Unknown object type during deserialization.");
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                JObject jsonObject = new JObject();

                Type objectType = value.GetType();
                jsonObject.Add("_Type", objectType.FullName);

                PropertyInfo[] properties = objectType.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.Name == "_Type")
                    {
                        continue;
                    }

                    if (property.PropertyType == typeof(List<INode>))
                    {
                        var nodeList = (List<INode>)property.GetValue(value);
                        var nodeArray = new JArray();
                        foreach (var node in nodeList)
                        {
                            var nodeJson = JToken.FromObject(node, serializer);
                            nodeArray.Add(nodeJson);
                        }
                        jsonObject.Add(property.Name, nodeArray);
                    }
                    else
                    {
                        object propertyValue = property.GetValue(value);
                        jsonObject.Add(property.Name, JToken.FromObject(propertyValue, serializer));
                    }
                }

                serializer.Serialize(writer, jsonObject);
            }
        }
    }
}
