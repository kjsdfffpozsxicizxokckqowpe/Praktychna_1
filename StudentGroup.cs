namespace StudentGroupManagement;

public sealed class StudentGroup
{
    private readonly List<Student> _students = [];
    private string _name = string.Empty;
    private string _specialty = string.Empty;
    private int _course = 1;

    public required string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Назва групи не може бути порожньою.", nameof(value));
            }

            _name = value.Trim();
        }
    }

    public required string Specialty
    {
        get => _specialty;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Спеціальність не може бути порожньою.", nameof(value));
            }

            _specialty = value.Trim();
        }
    }

    public int Course
    {
        get => _course;
        set
        {
            if (value is < 1 or > 6)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Курс має бути від 1 до 6.");
            }

            _course = value;
        }
    }

    public int GroupSize => _students.Count;

    public double AverageGroupGrade => _students.Count == 0
        ? 0
        : Math.Round(_students.Average(student => student.AverageGrade), 2);

    public IReadOnlyList<Student> Students => _students.AsReadOnly();

    public void AddStudent(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);

        if (_students.Any(existing => existing.RecordBookNumber == student.RecordBookNumber))
        {
            throw new InvalidOperationException("Студент з таким номером залікової вже існує в групі.");
        }

        _students.Add(student);
    }

    public bool RemoveStudent(string recordBookNumber)
    {
        var student = FindStudent(recordBookNumber);
        return student is not null && _students.Remove(student);
    }

    public Student? FindStudent(string recordBookNumber)
    {
        return _students.FirstOrDefault(student => student.RecordBookNumber == recordBookNumber);
    }

    public IReadOnlyList<Student> FindStudent(string query, StudentSearchMode mode)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var normalized = query.Trim();
        return mode switch
        {
            StudentSearchMode.ByRecordBookNumber => _students
                .Where(student => student.RecordBookNumber.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .ToList(),
            StudentSearchMode.ByFullName => _students
                .Where(student => student.FullName.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .ToList(),
            _ => []
        };
    }

    public Student? FindStudent(Predicate<Student> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _students.Find(predicate);
    }

    public IReadOnlyList<Student> GetExcellentStudents()
    {
        return _students.Where(student => student.IsExcellent()).ToList();
    }

    public IReadOnlyList<Student> GetFailingStudents()
    {
        return _students.Where(student => student.IsFailing()).ToList();
    }

    public IReadOnlyList<Student> GetStudentsByStatus(StudentStatus status)
    {
        return _students.Where(student => student.Status == status).ToList();
    }

    public double GetExcellentPercentage()
    {
        return GroupSize == 0 ? 0 : Math.Round(GetExcellentStudents().Count * 100.0 / GroupSize, 2);
    }
}
