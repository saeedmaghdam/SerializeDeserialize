using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using SerializeDeserialize.Domain;

namespace SerializeDeserialize
{
    public class MicrosoftSerializer
    {
        public string Serialize(Customer customer)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new INodeConverter());
            options.WriteIndented = true;

            return System.Text.Json.JsonSerializer.Serialize(customer, options);
        }

        public Customer Deserialize(string json)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new INodeConverter());
            options.WriteIndented = true;

            return System.Text.Json.JsonSerializer.Deserialize<Customer>(json, options);
        }

        private class INodeConverter : JsonConverter<INode>
        {
            private readonly Assembly _assembly;

            public INodeConverter()
            {
                _assembly = Assembly.LoadFrom("SerializeDeserialize.Domain.dll");
            }

            public override bool CanConvert(Type typeToConvert)
            {
                return typeof(INode).IsAssignableFrom(typeToConvert);
            }

            public override INode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                using var jsonDocument = JsonDocument.ParseValue(ref reader);

                if (jsonDocument.RootElement.TryGetProperty("_Type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String)
                {
                    var typeName = typeElement.GetString();
                    var targetType = _assembly.GetType(typeName);
                    if (targetType == null)
                    {
                        throw new JsonException($"Unknown type '{typeName}' during deserialization.");
                    }

                    var targetObject = (INode)Activator.CreateInstance(targetType);
                    DeserializeProperties(jsonDocument.RootElement, targetObject, options);
                    return targetObject;
                }

                throw new JsonException("Unknown object type during deserialization.");
            }

            private void DeserializeProperties(JsonElement jsonElement, object targetObject, JsonSerializerOptions options)
            {
                var targetType = targetObject.GetType();
                var properties = targetType.GetProperties();
                foreach (var property in properties)
                {
                    if (property.Name == "_Type")
                    {
                        continue;
                    }

                    if (property.PropertyType == typeof(List<INode>))
                    {
                        var nodeList = new List<INode>();
                        var documentsJson = jsonElement.GetProperty(property.Name).GetRawText();
                        using var documentsDocument = JsonDocument.Parse(documentsJson);
                        foreach (var nodeElement in documentsDocument.RootElement.EnumerateArray())
                        {
                            var nodeTypeName = nodeElement.GetProperty("_Type").GetString();
                            var nodeTargetType = _assembly.GetType(nodeTypeName);
                            if (nodeTargetType == null)
                            {
                                throw new JsonException($"Unknown type '{nodeTypeName}' during deserialization.");
                            }

                            var nodeTargetObject = Activator.CreateInstance(nodeTargetType);
                            DeserializeProperties(nodeElement, nodeTargetObject, options);
                            nodeList.Add((INode)nodeTargetObject);
                        }
                        property.SetValue(targetObject, nodeList);
                    }
                    else
                    {
                        var propertyValue = jsonElement.GetProperty(property.Name);
                        var value = JsonSerializer.Deserialize(propertyValue.GetRawText(), property.PropertyType, options);
                        property.SetValue(targetObject, value);
                    }
                }
            }

            public override void Write(Utf8JsonWriter writer, INode value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                Type objectType = value.GetType();
                writer.WriteString("_Type", objectType.FullName);

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
                        writer.WritePropertyName(property.Name);
                        writer.WriteStartArray();
                        foreach (var node in nodeList)
                        {
                            JsonSerializer.Serialize(writer, node, options);
                        }
                        writer.WriteEndArray();
                    }
                    else
                    {
                        writer.WritePropertyName(property.Name);
                        JsonSerializer.Serialize(writer, property.GetValue(value), options);
                    }
                }

                writer.WriteEndObject();
            }
        }
    }
}