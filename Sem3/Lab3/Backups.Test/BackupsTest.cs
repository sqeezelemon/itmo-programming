using Backups.Backups;
using Backups.Exceptions;
using Backups.Storages;
using Backups.Utilities;
using Xunit;

namespace Backups.Test;

public class BackupsTest
{
    [Fact]
    public void BackupTask_PerformOnMemoryRepo()
    {
        var repo = new MemoryRepository();
        var backupDir = repo.MakeFolder(Path.Combine("mem", "backups"));
        var encdr = new ZipEncoder();
        var stAlgo = new SplitStorageAlgorithm();
        var backup = new Backup();
        var task = new BackupTask(repo, backupDir, encdr, stAlgo, backup);

        var sourceDir = repo.MakeFolder(Path.Combine("mem", "source"));
        var fA = repo.MakeFile(Path.Combine("mem", "source", "A.txt"));
        fA.Write(GetStream("AAAAAAAAAAA"));
        task.AddBackupObject(fA, "A");
        var fB = repo.MakeFile(Path.Combine("mem", "source", "B.txt"));
        fA.Write(GetStream("BBBBBBBBB"));
        task.AddBackupObject(fB, "B");

        task.Perform("1");
        var task1dir = (repo.GetObject(Path.Combine("mem", "backups", "1")) as IFolder) !;

        Assert.Equal(2, task1dir.Contents.Count);
        Assert.Equal(1, backupDir.Contents.Count);
        Assert.Equal(2, sourceDir.Contents.Count);

        task.RemoveBackupObject("B");

        task.Perform("2");
        var task2dir = (repo.GetObject(Path.Combine("mem", "backups", "2")) as IFolder) !;

        Assert.Equal(1, task2dir.Contents.Count);
        Assert.Equal(2, backupDir.Contents.Count);
        Assert.Equal(2, sourceDir.Contents.Count);
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
}