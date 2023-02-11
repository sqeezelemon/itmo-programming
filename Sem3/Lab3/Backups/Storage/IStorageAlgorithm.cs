using System;
using Backups.Backups;
using Backups.Utilities;

namespace Backups.Storages;

public interface IStorageAlgorithm
{
    Storage Encode(List<IBackupObject> backupObjects, IRepository repository, IFolder outputFolder, IEncoder encoder);
}