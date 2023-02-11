using System;
namespace Backups.Utilities;

public interface IFile : IRepoObject
{
    void Read(Stream outputStream);
    void Write(Stream inputStream);
    void Append(Stream inputStream);
}