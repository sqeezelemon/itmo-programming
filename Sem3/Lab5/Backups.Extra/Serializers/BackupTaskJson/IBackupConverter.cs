using System.Text.Json;
using System.Text.Json.Serialization;
using Backups.Backups;

namespace Backups.Extra.Serializers;

public class IBackupConverter : JsonConverter<IBackup>
{
    public override IBackup? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, IBackup value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var rp in value.RestorePoints)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("date");
            writer.WriteStringValue(rp.DateTime);

            writer.WritePropertyName("name");
            writer.WriteStringValue(rp.Name);

            writer.WritePropertyName("objects");
            writer.WriteStartArray();
            foreach (var bo in rp.BackupObjects)
            {
                writer.WriteStartObject();
                writer.WriteString("name", bo.Name);
                writer.WriteString("sourcePath", bo.Value.Path);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteString("location", rp.Storage.Location.Path);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}