using System;
namespace Backups.Utilities;

public interface IRepository
{
    IFile MakeFile(string path);
    IFolder MakeFolder(string path);
    IRepoObject GetObject(string path);
    void Delete(string path);
}