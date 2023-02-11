using System.Text.Json;
using System.Text.Json.Serialization;
using Backups.Utilities;
using Microsoft.VisualBasic.CompilerServices;

namespace Backups.Extra.Serializers;

public class MemoryRepositoryConverter : JsonConverter<MemoryRepository>
{
    public override MemoryRepository? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        var repo = new MemoryRepository();
        foreach (var obj in root.EnumerateArray())
        {
            string path = obj.GetProperty("path").GetString() !;
            if (obj.GetProperty("isDirectory").GetBoolean())
            {
                repo.MakeFolder(path);
            }
            else
            {
                var file = repo.MakeFile(path);
                file.Read(new MemoryStream(Convert.FromHexString(obj.GetProperty("data").GetString() !)));
            }
        }

        return repo;
    }

    public override void Write(Utf8JsonWriter writer, MemoryRepository value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        if (value.GetObject("mem") is not IFolder rootFolder)
        {
            writer.WriteEndArray();
            return;
        }

        foreach (var obj in ListObjects(rootFolder))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("path");
            writer.WriteStringValue(obj.Path);
            writer.WritePropertyName("isDirectory");
            writer.WriteBooleanValue(obj is IFolder);
            writer.WritePropertyName("data");
            if (obj is IFile file)
            {
                var fileStream = new MemoryStream();
                file.Read(fileStream);
                writer.WriteStringValue(Convert.ToHexString(fileStream.ToArray()));
            }
            else
            {
                writer.WriteStringValue(string.Empty);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    public List<IRepoObject> ListObjects(IFolder root)
    {
        var result = new List<IRepoObject>();
        foreach (var obj in root.Contents)
        {
            if (obj is IFile file)
            {
                result.Add(file);
            }
            else if (obj is IFolder folder)
            {
                result.Add(folder);
                result.AddRange(ListObjects(folder));
            }
        }

        return result;
    }
}