using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace StudentGroupManagement;

public sealed class Student
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex RecordBookRegex = new(@"^\d{8}$", RegexOptions.Compiled);

    private string _fullName = string.Empty;
    private DateTime _dateOfBirth;
    private string _recordBookNumber = string.Empty;
    private double _averageGrade;
    private string _personalEmail = string.Empty;
    private string _notes = string.Empty;

    public required string FullName
    {
        get => _fullName;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim().Length < 5)
            {
                throw new ArgumentException("ПІБ має містити мінімум 5 символів.", nameof(value));
            }

            _fullName = value.Trim();
        }
    }

    public DateTime DateOfBirth
    {
        get => _dateOfBirth;
        set
        {
            if (value.Date > DateTime.Today)
            {
                throw new ArgumentException("Дата народження не може бути в майбутньому.", nameof(value));
            }

            if (value.Date < DateTime.Today.AddYears(-120))
            {
                throw new ArgumentException("Вік студента не може перевищувати 120 років.", nameof(value));
            }

            _dateOfBirth = value.Date;
        }
    }

    public required string RecordBookNumber
    {
        get => _recordBookNumber;
        init
        {
            var normalized = value ?? string.Empty;
            if (!RecordBookRegex.IsMatch(normalized))
            {
                throw new ArgumentException("Номер залікової має складатися рівно з 8 цифр.", nameof(value));
            }

            _recordBookNumber = normalized;
        }
    }

    [JsonInclude]
    public double AverageGrade
    {
        get => _averageGrade;
        private set
        {
            ValidateGrade(value);
            _averageGrade = Math.Round(value, 2);
        }
    }

    public StudentStatus Status { get; set; } = StudentStatus.Active;

    public DateTime EnrollmentDate { get; init; } = DateTime.Today;

    public string PersonalEmail
    {
        get => _personalEmail;
        set
        {
            var normalized = value ?? string.Empty;
            if (!EmailRegex.IsMatch(normalized))
            {
                throw new ArgumentException("Email має бути у правильному форматі.", nameof(value));
            }

            _personalEmail = normalized.Trim();
        }
    }

    public string Notes
    {
        get => _notes;
        set => _notes = value?.Trim() ?? string.Empty;
    }

    [JsonInclude]
    public GradeJournal GradeJournal { get; private set; } = new();

    [JsonIgnore]
    public int Age => CalculateAge();

    public string ShowDetailedInfo()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"ПІБ: {FullName}");
        builder.AppendLine($"Залікова книжка: {RecordBookNumber}");
        builder.AppendLine($"Дата народження: {DateOfBirth:dd.MM.yyyy} ({Age} років)");
        builder.AppendLine($"Email: {PersonalEmail}");
        builder.AppendLine($"Статус: {Status.ToDisplayName()}");
        builder.AppendLine($"Дата зарахування: {EnrollmentDate:dd.MM.yyyy}");
        builder.AppendLine($"Середній бал: {AverageGrade:F2}");
        builder.AppendLine($"Років до випуску: {GetYearsToGraduation()}");
        builder.AppendLine($"Нотатки: {(string.IsNullOrWhiteSpace(Notes) ? "-" : Notes)}");

        if (GradeJournal.Grades.Count > 0)
        {
            builder.AppendLine("Оцінки:");
            foreach (var grade in GradeJournal.Grades.OrderBy(g => g.Key))
            {
                builder.AppendLine($"  {grade.Key}: {grade.Value:F2}");
            }
        }

        return builder.ToString();
    }

    public void UpdateAverageGrade(double newGrade)
    {
        AverageGrade = newGrade;
    }

    public void RecalculateAverageGradeFromJournal()
    {
        if (GradeJournal.Grades.Count == 0)
        {
            return;
        }

        AverageGrade = GradeJournal.CalculateAverage();
    }

    public bool IsExcellent()
    {
        return AverageGrade >= 9 && Status == StudentStatus.Active;
    }

    public bool IsFailing()
    {
        return AverageGrade < 6 && Status == StudentStatus.Active;
    }

    public int CalculateAge()
    {
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Year;

        if (DateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    public int GetYearsToGraduation()
    {
        if (Status == StudentStatus.Graduated)
        {
            return 0;
        }

        var expectedGraduation = EnrollmentDate.AddYears(4);
        return Math.Max(0, expectedGraduation.Year - DateTime.Today.Year);
    }

    private static void ValidateGrade(double grade)
    {
        if (grade is < 0 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(grade), "Середній бал має бути від 0 до 10.");
        }
    }
}
