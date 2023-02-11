using System.Text.Json;
using System.Text.Json.Serialization;
using Backups.Backups;
using Backups.Extra.Backups;
using Backups.Storages;
using Backups.Utilities;

namespace Backups.Extra.Serializers;

public class BackupExtraTaskConverter : JsonConverter<BackupExtraTask>
{
    public static Dictionary<string, object> DecodeDefaults { get; } = new Dictionary<string, object>()
    {
        [typeof(ZipEncoder).ToString()] = new ZipEncoder(),
        [typeof(ZipDecoder).ToString()] = new ZipDecoder(),

        [typeof(SplitStorageAlgorithm).ToString()] = new SplitStorageAlgorithm(),
        [typeof(SingleStorageAlgorithm).ToString()] = new SingleStorageAlgorithm(),

        [typeof(SplitStorageDecodableAlgorithm).ToString()] = new SplitStorageDecodableAlgorithm(),
        [typeof(SingleStorageDecodableAlgorithm).ToString()] = new SingleStorageDecodableAlgorithm(),
    };

    public override BackupExtraTask? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        IRepository repository = JsonSerializer.Deserialize<IRepository>(root.GetProperty("repository").GetRawText(), options) !;
        IFolder backupFolder = (repository.GetObject(root.GetProperty("backupFolder").GetString() !) as IFolder) !;

        var encoder = (DecodeDefaults[root.GetProperty("encoder").GetString() !] as IEncoder) !;
        var decoder = (DecodeDefaults[root.GetProperty("decoder").GetString() !] as IDecoder) !;

        var storageAlgo = (DecodeDefaults[root.GetProperty("storageAlgo").GetString() !] as IStorageAlgorithm) !;
        var decodableStorageAlgo = (DecodeDefaults[root.GetProperty("decodableStorageAlgo").GetString() !] as IDecodableStorageAlgorithm) !;

        var backup = new Backup();
        foreach (var jrp in root.GetProperty("backup").EnumerateArray())
        {
            var backupObjects = jrp.GetProperty("objects").EnumerateArray()
                .Select(je => new BackupObject(
                    je.GetProperty("name").GetString() !,
                    repository.GetObject(je.GetProperty("sourcePath").GetString() !)))
                .ToList();
            var rp = new RestorePoint(
                jrp.GetProperty("date").GetDateTime(),
                jrp.GetProperty("name").GetString() !,
                new Storage(repository, (repository.GetObject(jrp.GetProperty("location").GetString() !) as IFolder) !),
                backupObjects);
            backup.AddRestorePoint(rp);
        }

        var res = new BackupExtraTask(
            new BackupTask(repository, backupFolder, encoder, storageAlgo, backup),
            decodableStorageAlgo,
            decoder);
        return res;
    }

    public override void Write(Utf8JsonWriter writer, BackupExtraTask value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("repository");
        writer.WriteRawValue(JsonSerializer.Serialize(value.Repository, options));

        writer.WritePropertyName("backupFolder");
        writer.WriteStringValue(value.BackupFolder.Path);

        writer.WritePropertyName("encoder");
        writer.WriteStringValue(value.Encoder.GetType().ToString());

        writer.WritePropertyName("storageAlgo");
        writer.WriteStringValue(value.StorageAlgorithm.GetType().ToString());

        writer.WritePropertyName("backup");
        writer.WriteRawValue(JsonSerializer.Serialize(value.Backup, options));

        writer.WritePropertyName("decoder");
        writer.WriteStringValue(value.Decoder.GetType().ToString());

        writer.WritePropertyName("decodableStorageAlgo");
        writer.WriteStringValue(value.DecodableStorageAlgorithm.GetType().ToString());
        writer.WriteEndObject();
    }
}