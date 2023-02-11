using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Backups.Backups;
using Backups.Exceptions;
using Backups.Extra.Algorithms;
using Backups.Extra.Serializers;
using Backups.Extra.Utils;
using Backups.Storages;
using Backups.Utilities;

namespace Backups.Extra.Backups;

public class BackupExtraTask : IBackupTask, ILoggable
{
    public BackupExtraTask(IBackupTask inner, IDecodableStorageAlgorithm decodableStorageAlgorithm, IDecoder decoder)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(decodableStorageAlgorithm);
        ArgumentNullException.ThrowIfNull(decoder);
        (Inner, DecodableStorageAlgorithm, Decoder) = (inner, decodableStorageAlgorithm, decoder);
    }

    public IRepository Repository => Inner.Repository;
    public IFolder BackupFolder => Inner.BackupFolder;
    public IEncoder Encoder => Inner.Encoder;
    public IStorageAlgorithm StorageAlgorithm => Inner.StorageAlgorithm;
    public IBackup Backup => Inner.Backup;
    public IDecodableStorageAlgorithm DecodableStorageAlgorithm { get; }
    public IDecoder Decoder { get; }
    public CleanupTask CleanupTask { get; private set; } = new CleanupTask();

    public Logger? Logger { get; set; }

    private IBackupTask Inner { get; }

    public static string Encode(BackupExtraTask task, JsonSerializerOptions? opts = null)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new IRepositoryConverter());
        options.Converters.Add(new MemoryRepositoryConverter());
        options.Converters.Add(new BackupExtraTaskConverter());
        options.Converters.Add(new IBackupConverter());

        if (opts != null)
        {
            foreach (var converter in opts.Converters)
                options.Converters.Add(converter);
        }

        return JsonSerializer.Serialize(task, options);
    }

    public static BackupExtraTask? Decode(string source, JsonSerializerOptions? opts = null)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new IRepositoryConverter());
        options.Converters.Add(new MemoryRepositoryConverter());
        options.Converters.Add(new BackupExtraTaskConverter());
        options.Converters.Add(new IBackupConverter());

        if (opts != null)
        {
            foreach (var converter in opts.Converters)
                options.Converters.Add(converter);
        }

        return JsonSerializer.Deserialize<BackupExtraTask>(source, options);
    }

    public void SetCleanupTask(CleanupTask task)
    {
        ArgumentNullException.ThrowIfNull(task);
        CleanupTask = task;
    }

    public void AddBackupObject(IRepoObject obj, string? name = null)
    {
        Inner.AddBackupObject(obj, name);
    }

    public void RemoveBackupObject(string name)
    {
        Inner.RemoveBackupObject(name);
    }

    public RestorePoint Perform(string name)
    {
        Logger?.Info($"BackupTask \"{name}\" started");
        var result = Inner.Perform(name);
        Logger?.Info($"Created restore point \"{name}\", starting cleanup");
        CleanupTask.Perform(Backup, Repository);
        Logger?.Info($"Cleanup finished, BackupTask completed.");
        return result;
    }

    public bool ContainsRepoObject(IRepoObject obj)
    {
        return Inner.ContainsRepoObject(obj);
    }

    public bool ContainsBackupObject(string name)
    {
        return Inner.ContainsBackupObject(name);
    }

    public void Restore(IRestorePoint restorePoint, Dictionary<string, string> locationOverrides)
    {
        var locations = new Dictionary<string, string>();
        foreach (var bObj in restorePoint.BackupObjects)
        {
            if (locationOverrides.ContainsKey(bObj.Name))
                locations[bObj.Name] = locationOverrides[bObj.Name];
            else
                locations[bObj.Name] = Path.GetDirectoryName(bObj.Value.Path) !.ToString();
        }

        DecodableStorageAlgorithm.Decode(restorePoint.Storage, Repository, locations, Decoder);
    }

    public IRestorePoint Merge(IRestorePoint x, IRestorePoint y, MergeAlgorithm algorithm, string? name = null)
    {
        if (!algorithm.CanAcceptAlgorithm(StorageAlgorithm.GetType()))
            Logger?.Warning($"Algorithm {StorageAlgorithm.GetType()} is not supported by {algorithm.GetType()}");
        if (x == y)
            throw new BackupsDuplicateException("Can't merge equal restore points");
        var mergedRp = algorithm.Merge(x, y, BackupFolder, Repository, name);
        Backup.AddRestorePoint(mergedRp);
        Backup.RemoveRestorePoint(x.Name);
        Backup.RemoveRestorePoint(y.Name);
        CleanupTask.Perform(Backup, Repository);
        return mergedRp;
    }

    public string LogString()
    {
        throw new NotImplementedException();
    }
}