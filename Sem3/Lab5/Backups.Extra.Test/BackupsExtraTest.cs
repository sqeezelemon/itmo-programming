using System.Text.Json;
using Backups.Backups;
using Backups.Extra.Algorithms;
using Backups.Extra.Backups;
using Backups.Extra.Serializers;
using Backups.Storages;
using Backups.Utilities;
using Xunit;
namespace Backups.Extra.Test;

public class BackupsExtraTest
{
    [Fact]
    public void Cleanup_RemovesRequired()
    {
        var repo = new MemoryRepository();
        var backupDir = repo.MakeFolder(Path.Combine("mem", "backups"));
        var encdr = new ZipEncoder();
        var stAlgo = new SplitStorageAlgorithm();
        var backup = new BackupExtra();
        var task = new BackupExtraTask(
            new BackupTask(repo, backupDir, encdr, stAlgo, backup),
            new SplitStorageDecodableAlgorithm(),
            new ZipDecoder());

        var sourceDir = repo.MakeFolder(Path.Combine("mem", "source"));
        var fA = repo.MakeFile(Path.Combine("mem", "source", "A.txt"));
        fA.Write(GetStream("AAAAAAAAAAA"));
        task.AddBackupObject(fA, "A");
        var fB = repo.MakeFile(Path.Combine("mem", "source", "B.txt"));
        fA.Write(GetStream("BBBBBBBBB"));
        task.AddBackupObject(fB, "B");

        var rp1 = task.Perform("1");
        rp1.ChangeDate(rp1.DateTime.AddMonths(-1));

        task.CleanupTask.Threshold = 1;
        task.CleanupTask.AddRule(new DateCleanupRule(DateTime.Now.AddMonths(-2)));
        var rp2 = task.Perform("2");
        var task2dir = (repo.GetObject(Path.Combine("mem", "backups", "2")) as IFolder) !;

        // Shouldn't delete 1
        Assert.False(backupDir.Contents.All(o => o.Path != Path.Combine("mem", "backups", "1")));

        rp2.ChangeDate(DateTime.Now.AddDays(-1));
        task.CleanupTask.AddRule(new DateCleanupRule(DateTime.Now.AddMonths(1)));
        var rp3 = task.Perform("3");

        // Only 3 should stay
        Assert.True(backupDir.Contents.All(o => o.Path != "mem/backups/1"));
        Assert.True(backupDir.Contents.All(o => o.Path != "mem/backups/2"));
        Assert.False(backupDir.Contents.All(o => o.Path != "mem/backups/3"));
    }

    [Fact]
    public void Restore_AllFilesAreRestoredAndOverridesRespected()
    {
        var repo = new MemoryRepository();
        var backupDir = repo.MakeFolder(Path.Combine("mem", "backups"));
        var encdr = new ZipEncoder();
        var stAlgo = new SplitStorageAlgorithm();
        var backup = new BackupExtra();
        var task = new BackupExtraTask(
            new BackupTask(repo, backupDir, encdr, stAlgo, backup),
            new SplitStorageDecodableAlgorithm(),
            new ZipDecoder());

        var sourceDir = repo.MakeFolder(Path.Combine("mem", "source"));
        var otherDir = repo.MakeFolder(Path.Combine("mem", "other"));
        var fA = repo.MakeFile(Path.Combine("mem", "source", "A.txt"));
        fA.Write(GetStream("AAAAAAAAAAA"));
        task.AddBackupObject(fA, "A");
        var fB = repo.MakeFile(Path.Combine("mem", "source", "B.txt"));
        fB.Write(GetStream("BBBBBBBBB"));
        task.AddBackupObject(fB, "B");

        var rp = task.Perform("1");
        repo.Delete(Path.Combine("mem", "source", "A.txt"));
        repo.Delete(Path.Combine("mem", "source", "B.txt"));
        repo.GetObject(otherDir.Path);
        var locationOverrides = new Dictionary<string, string>()
        {
            ["A"] = Path.Combine("mem", "other"),
        };

        task.Restore(rp, locationOverrides);

        Assert.False(otherDir.Contents.All(o => o.Path != "mem/other/A.txt"));
        Assert.True(otherDir.Contents.All(o => o.Path != "mem/other/B.txt"));
        Assert.False(sourceDir.Contents.All(o => o.Path != "mem/source/B.txt"));
        Assert.True(sourceDir.Contents.All(o => o.Path != "mem/source/A.txt"));
    }

    [Fact]
    public void Serialization_CanSaveAndRead()
    {
        var repo = new MemoryRepository();
        var backupDir = repo.MakeFolder(Path.Combine("mem", "backups"));
        var encdr = new ZipEncoder();
        var stAlgo = new SplitStorageAlgorithm();
        var backup = new BackupExtra();
        var task = new BackupExtraTask(
            new BackupTask(repo, backupDir, encdr, stAlgo, backup),
            new SplitStorageDecodableAlgorithm(),
            new ZipDecoder());

        var sourceDir = repo.MakeFolder(Path.Combine("mem", "source"));
        var otherDir = repo.MakeFolder(Path.Combine("mem", "other"));
        var fA = repo.MakeFile(Path.Combine("mem", "source", "A.txt"));
        fA.Write(GetStream("AAAAAAAAAAA"));
        task.AddBackupObject(fA, "A");
        var fB = repo.MakeFile(Path.Combine("mem", "source", "B.txt"));
        fB.Write(GetStream("BBBBBBBBB"));
        task.AddBackupObject(fB, "B");
        task.Perform("1");

        string encoded = BackupExtraTask.Encode(task);
        var decoded = BackupExtraTask.Decode(encoded) !;

        Assert.Equal(task.Backup.RestorePoints.Count, decoded.Backup.RestorePoints.Count);
        Assert.Equal(task.StorageAlgorithm.GetType(), decoded.StorageAlgorithm.GetType());
        Assert.Equal(task.Repository.GetType(), decoded.Repository.GetType());
    }

    [Fact]
    public void Merge_KeepsNewestVersions()
    {
        var repo = new MemoryRepository();
        var backupDir = repo.MakeFolder(Path.Combine("mem", "backups"));
        var encdr = new ZipEncoder();
        var stAlgo = new SplitStorageAlgorithm();
        var backup = new BackupExtra();
        var task = new BackupExtraTask(
            new BackupTask(repo, backupDir, encdr, stAlgo, backup),
            new SplitStorageDecodableAlgorithm(),
            new ZipDecoder());

        var sourceDir = repo.MakeFolder(Path.Combine("mem", "source"));
        var otherDir = repo.MakeFolder(Path.Combine("mem", "other"));
        var fA = repo.MakeFile(Path.Combine("mem", "source", "A.txt"));
        fA.Write(GetStream("AAAAAAAAAAA"));
        task.AddBackupObject(fA, "A");
        var fB = repo.MakeFile(Path.Combine("mem", "source", "B.txt"));
        fB.Write(GetStream("BBBBBBBBB"));
        task.AddBackupObject(fB, "B");
        var t1 = task.Perform("1");

        var fC = repo.MakeFile(Path.Combine("mem", "source", "C.txt"));
        fC.Write(GetStream("CCCCC"));
        task.AddBackupObject(fC, "C");
        repo.Delete(fA.Path);
        task.RemoveBackupObject("A");
        fB.Write(GetStream("222222"));
        var t2 = task.Perform("2");
        var bSize = Size(Path.Combine("mem", "backups", "2", "B"), repo);

        // RP1: A, B
        // RP2: B, C, with B being modified
        var merged = task.Merge(t1, t2, new SplitStorageMergeAlgorithm(), "3");

        Assert.Equal(3, merged.BackupObjects.Count);
        Assert.Equal(bSize, Size(Path.Combine("mem", "backups", "3", "B"), repo));
    }

    private Stream GetStream(string str)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(str);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private string Read(string path, IRepository repo)
    {
        IFile file = (repo.GetObject(path) as IFile) !;
        var readStream = new MemoryStream();
        file.Read(readStream);
        readStream.Position = 0;
        var reader = new StreamReader(readStream);
        return reader.ReadToEnd();
    }

    private long Size(string path, IRepository repo)
    {
        IFile file = (repo.GetObject(path) as IFile) !;
        var readStream = new MemoryStream();
        file.Read(readStream);
        return readStream.Position;
    }
}