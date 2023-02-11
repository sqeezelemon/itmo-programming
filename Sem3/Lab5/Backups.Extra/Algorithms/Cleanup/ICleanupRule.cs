using Backups.Backups;

namespace Backups.Extra.Algorithms;

public interface ICleanupRule
{
    bool ShouldDelete(IRestorePoint restorePoint, int index, int total);
}