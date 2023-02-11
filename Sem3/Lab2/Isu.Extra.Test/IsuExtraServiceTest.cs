using Isu.Extra.Entities;
using Isu.Extra.Exceptions;
using Isu.Extra.Models;
using Isu.Extra.Service;
using Isu.Models;
using Xunit;

namespace Isu.Extra.Test;

public class IsuExtraServiceTest
{
    private IsuExtraService isu = new IsuExtraService();

    [Fact]
    public void ReachMaxOgnpCount_ThrowException()
    {
        GroupName groupName = new GroupName("Z3200");
        ExtraGroup group = isu.AddGroup(groupName);
        ExtraStudent student = isu.AddStudent(group, "Любс Любознателев");

        OgnpName ognpName1 = new OgnpName("A001");
        Ognp ognp1 = isu.AddOgnp(ognpName1);
        OgnpGroup ognpGroup1 = isu.AddOgnpGroup(ognpName1, ognp1);

        OgnpName ognpName2 = new OgnpName("B002");
        Ognp ognp2 = isu.AddOgnp(ognpName2);
        OgnpGroup ognpGroup2 = isu.AddOgnpGroup(ognpName2, ognp2);

        OgnpName ognpName3 = new OgnpName("C003");
        Ognp ognp3 = isu.AddOgnp(ognpName3);
        OgnpGroup ognpGroup3 = isu.AddOgnpGroup(ognpName3, ognp3);

        Exception? exception = null;
        exception = Record.Exception(() => isu.AddStudentToOgnp(ognpGroup1, student));
        Assert.Null(exception);
        exception = Record.Exception(() => isu.AddStudentToOgnp(ognpGroup2, student));
        Assert.Null(exception);
        Assert.Throws<IsuOgnpOverloadException>(() => isu.AddStudentToOgnp(ognpGroup3, student));
    }

    [Fact]
    public void LessonsOverlap_ThrowException()
    {
        GroupName groupName = new GroupName("M3204");
        ExtraGroup group = isu.AddGroup(groupName);
        Lesson groupLesson = new Lesson("Name", new TimeOnly(10, 00), new TimeOnly(11, 30));
        group.Schedule.AddLesson(groupLesson);
        ExtraStudent student = isu.AddStudent(group, "Группин Группинс");

        OgnpName ognpName1 = new OgnpName("K123");
        Ognp ognp1 = isu.AddOgnp(ognpName1);
        OgnpGroup ognpGroup1 = isu.AddOgnpGroup(ognpName1, ognp1);
        Lesson ognpLesson1 = new Lesson("Name", new TimeOnly(10, 10), new TimeOnly(11, 40));
        ognpGroup1.Schedule.AddLesson(ognpLesson1);

        Assert.Throws<IsuExtraLessonOverlapException>(() => isu.AddStudentToOgnp(ognpGroup1, student));

        OgnpName ognpName2 = new OgnpName("K122");
        Ognp ognp2 = isu.AddOgnp(ognpName2);
        OgnpGroup ognpGroup2 = isu.AddOgnpGroup(ognpName2, ognp2);
        Lesson ognpLesson2 = new Lesson("Name", new TimeOnly(9, 00), new TimeOnly(10, 30));
        ognpGroup2.Schedule.AddLesson(ognpLesson2);

        Assert.Throws<IsuExtraLessonOverlapException>(() => isu.AddStudentToOgnp(ognpGroup2, student));
    }

    [Fact]
    public void OgnpFaculty_ThrowsOnMatch()
    {
        GroupName groupName = new GroupName("K3204");
        ExtraGroup group = isu.AddGroup(groupName);
        ExtraStudent student = isu.AddStudent(group, "Досугов Досугарь");

        OgnpName ognpName1 = new OgnpName("K321");
        Ognp ognp1 = isu.AddOgnp(ognpName1);
        OgnpGroup ognpGroup1 = isu.AddOgnpGroup(ognpName1, ognp1);

        Assert.Throws<IsuExtraDuplicateFaculty>(() => isu.AddStudentToOgnp(ognpGroup1, student));

        OgnpName ognpName2 = new OgnpName("M321");
        Ognp ognp2 = isu.AddOgnp(ognpName2);
        OgnpGroup ognpGroup2 = isu.AddOgnpGroup(ognpName2, ognp2);

        var exception = Record.Exception(() => isu.AddStudentToOgnp(ognpGroup2, student));
        Assert.Null(exception);

        OgnpName ognpName3 = new OgnpName("K323");
        Ognp ognp3 = isu.AddOgnp(ognpName3);
        OgnpGroup ognpGroup3 = isu.AddOgnpGroup(ognpName3, ognp3);

        Assert.Throws<IsuExtraDuplicateFaculty>(() => isu.AddStudentToOgnp(ognpGroup3, student));
    }
}