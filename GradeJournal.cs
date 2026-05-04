using System.Text.Json.Serialization;

namespace StudentGroupManagement;

public sealed class GradeJournal
{
    [JsonInclude]
    public Dictionary<string, double> Grades { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public void AddOrUpdateGrade(string subject, double grade)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Назва предмета не може бути порожньою.", nameof(subject));
        }

        ValidateGrade(grade);
        Grades[subject.Trim()] = Math.Round(grade, 2);
    }

    public bool RemoveGrade(string subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            return false;
        }

        return Grades.Remove(subject.Trim());
    }

    public double CalculateAverage()
    {
        return Grades.Count == 0 ? 0 : Math.Round(Grades.Values.Average(), 2);
    }

    private static void ValidateGrade(double grade)
    {
        if (grade is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(grade), "Оцінка має бути від 0 до 100.");
        }
    }
}
