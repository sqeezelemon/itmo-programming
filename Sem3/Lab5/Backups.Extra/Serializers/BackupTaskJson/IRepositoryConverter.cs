using System.Text.Json;
using System.Text.Json.Serialization;
using Backups.Utilities;

namespace Backups.Extra.Serializers;

public class IRepositoryConverter : JsonConverter<IRepository>
{
    public Dictionary<string, Type> RepoTypes { get; } = new Dictionary<string, Type>()
    {
        [typeof(MemoryRepository).ToString()] = typeof(MemoryRepository),
        [typeof(DiskRepository).ToString()] = typeof(DiskRepository),
    };
    public override IRepository? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;
        var repoType = RepoTypes[root.GetProperty("type").GetString() !] !;
        return JsonSerializer.Deserialize(root.GetProperty("data").GetRawText(), repoType, options) as IRepository;
    }

    public override void Write(Utf8JsonWriter writer, IRepository value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("type");
        writer.WriteStringValue(value.GetType().ToString());
        writer.WritePropertyName("data");
        writer.WriteRawValue(JsonSerializer.Serialize(Convert.ChangeType(value, value.GetType()), options));
        writer.WriteEndObject();
    }
}