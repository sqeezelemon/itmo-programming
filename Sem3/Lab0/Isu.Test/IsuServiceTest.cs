using Isu.Entities;
using Isu.Exceptions;
using Isu.Models;
using Isu.Services;
using Xunit;

namespace Isu.Test;

public class IsuServiceTest
{
    private IsuService isu = new IsuService();

    [Fact]
    public void AddStudentToGroup_StudentHasGroupAndGroupContainsStudent()
    {
        GroupName name = new GroupName("M32001");
        Group group = isu.AddGroup(name);

        Student student = isu.AddStudent(group, "Константин Константинопольский");

        // Student has group.
        Assert.Equal(student.Group, group);

        // Group contains student
        Assert.True(group.HasStudent(student));
    }

    [Fact]
    public void ReachMaxStudentPerGroup_ThrowException()
    {
        GroupName name = new GroupName("M32002");
        Group group = isu.AddGroup(name);

        for (int i = 1; i <= Group.MaxStudents; i++)
        {
            try
            {
                isu.AddStudent(group, "Константин Константинопольский");
            }
            catch (IsuOvercrowdException)
            {
                Assert.True(false, $"IsuOvercrowdException thrown for student {i}, before reaching MaxStudents ({Group.MaxStudents}.");
            }
        }

        Assert.Throws<IsuOvercrowdException>(() => isu.AddStudent(group, "Лиш Лишневский"));
    }

    [Fact]
    public void CreateGroupWithInvalidName_ThrowException()
    {
        // No prefix
        Assert.ThrowsAny<Exception>(() => new GroupName("32041"));

        // Prefix too long
        Assert.ThrowsAny<Exception>(() => new GroupName("MMM32041"));

        // Name smaller than the minimum
        Assert.ThrowsAny<Exception>(() => new GroupName($"M{GroupName.MinNumber - 1}"));

        // Perfect group name
        var exception = Record.Exception(() => new GroupName("M3104"));
        Assert.Null(exception);

        // Wrong year, but shouldn't trigger here
        exception = Record.Exception(() => new GroupName("M3904"));
        Assert.Null(exception);

        // Perfect name, shouldn't trigger.
        GroupName name = new GroupName("M32041");
        exception = Record.Exception(() => isu.AddGroup(name));
        Assert.Null(exception);

        // Wrong year, should catch
        name = new GroupName("M39041");
        exception = Record.Exception(() => isu.AddGroup(name));
        Assert.NotNull(exception);
    }

    [Fact]
    public void TransferStudentToAnotherGroup_GroupChanged()
    {
        GroupName nameOne = new GroupName("T1111");
        Group groupOne = isu.AddGroup(nameOne);

        GroupName nameTwo = new GroupName("T2222");
        Group groupTwo = isu.AddGroup(nameTwo);

        Student student = isu.AddStudent(groupOne, "Перех Переходин");

        // Check that student is only in group one
        Assert.Equal(student.Group, groupOne);
        Assert.True(groupOne.HasStudent(student));
        Assert.False(groupTwo.HasStudent(student));

        // student should be able to transfer to groupTwo
        var exception = Record.Exception(() => isu.ChangeStudentGroup(student, groupTwo));
        Assert.Null(exception);

        // Check that student is now in group two
        Assert.Equal(student.Group, groupTwo);
        Assert.True(groupTwo.HasStudent(student));
        Assert.False(groupOne.HasStudent(student));

        // Fill up groupOne
        for (int i = 0; i < Group.MaxStudents; i++)
            isu.AddStudent(groupOne, "Уник Уникальнов");

        // Should be unable to transfer
        Assert.ThrowsAny<Exception>(() => isu.ChangeStudentGroup(student, groupOne));

        // Check that he wasn't moved despite exception
        Assert.Equal(student.Group, groupTwo);
        Assert.True(groupTwo.HasStudent(student));
        Assert.False(groupOne.HasStudent(student));
    }
}