using Isu.Exceptions;
using Isu.Extra.Exceptions;
using Isu.Extra.Models;
using Isu.Models;

namespace Isu.Extra.Entities;

public class Schedule
{
    private List<Lesson> _lessons = new List<Lesson>();

    public Schedule() { }

    public IReadOnlyList<Lesson> Lessons => _lessons;

    public void AddLesson(Lesson lesson)
    {
        ArgumentNullException.ThrowIfNull(lesson);

        if (!CanAddLesson(lesson))
            throw new IsuExtraLessonOverlapException($"Lesson {lesson} overlaps with existing lessons");

        _lessons.Add(lesson);
        SortLessons();
    }

    public void RemoveLession(Lesson lesson)
    {
        ArgumentNullException.ThrowIfNull(lesson);

        if (_lessons.Remove(lesson))
            throw new IsuNotFoundException($"Can't remove lesson {lesson}: not found.");
    }

    public bool HasLesson(Lesson lesson) => _lessons.Any(l => l == lesson);

    public bool Overlaps(Schedule other) => _lessons.Any(l1 => other.Lessons.Any(l2 => l1.Overlaps(l2)));

    internal void SortLessons() => _lessons.Sort((l, r) => l.Start.CompareTo(r.Start));

    internal bool CanAddLesson(Lesson lesson) => !_lessons.Any(l => l.Overlaps(lesson));
}