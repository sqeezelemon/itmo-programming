using Isu.Entities;
using Isu.Models;

namespace Isu.Services;

public interface IIsuService
{
    Group AddGroup(GroupName name);
    Student AddStudent(Group group, string name);

    Student GetStudent(int id);
    Student? FindStudent(int id);
    IReadOnlyList<Student> FindStudents(GroupName groupName);
    IReadOnlyList<Student> FindStudents(CourseNumber courseNumber);

    Group? FindGroup(GroupName groupName);
    IReadOnlyList<Group> FindGroups(CourseNumber courseNumber);

    void ChangeStudentGroup(Student student, Group newGroup);
}