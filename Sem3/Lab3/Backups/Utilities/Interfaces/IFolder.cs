using System;
namespace Backups.Utilities;

public interface IFolder : IRepoObject
{
    IReadOnlyList<IRepoObject> Contents { get; }
}