using System.Reflection;
using Backups.Backups;
using Backups.Exceptions;
using Backups.Storages;
using Backups.Utilities;

namespace Backups.Extra.Algorithms;

public abstract class MergeAlgorithm
{
    private List<Type> allowedStorageAlgorithms = new List<Type>();

    public abstract IRestorePoint Merge(IRestorePoint x, IRestorePoint y, IFolder backupsFolder, IRepository repository, string? name = null);

    public void AddAllowedAlgo(Type algo)
    {
        ArgumentNullException.ThrowIfNull(algo);
        if (!algo.IsAssignableTo(typeof(IStorageAlgorithm)))
            throw new ArgumentException("Provided type does not conform to IStorageAlgo");
        if (allowedStorageAlgorithms.Contains(algo))
            throw new BackupsDuplicateException("The algorithm is already in the allowed algorithms list");
        allowedStorageAlgorithms.Add(algo);
    }

    public void RemoveAllowedAlgo(Type algo)
    {
        ArgumentNullException.ThrowIfNull(algo);
        if (!allowedStorageAlgorithms.Remove(algo))
            throw new BackupsNotFoundException("Algorithm wasn't allowed");
    }

    public bool CanAcceptAlgorithm(Type algo) => algo.IsAssignableTo(typeof(IStorageAlgorithm)) && allowedStorageAlgorithms.Any(a => a == algo || algo.IsSubclassOf(a));

    protected string DefaultName(IRestorePoint x, IRestorePoint y) => $"{x.Name}+{y.Name}";
}