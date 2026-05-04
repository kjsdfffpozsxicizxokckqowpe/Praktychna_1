using System.Globalization;
using System.Text;
using StudentGroupManagement;

Console.OutputEncoding = Encoding.UTF8;

var storagePath = Path.Combine(AppContext.BaseDirectory, "students.json");
var group = new StudentGroup
{
    Name = "КН-11",
    Specialty = "Інженерія програмного забезпечення",
    Course = 1
};

while (true)
{
    PrintMenu(group);
    var choice = ReadRequired("Ваш вибір: ");

    try
    {
        switch (choice)
        {
            case "1":
                AddStudent(group);
                break;
            case "2":
                RemoveStudent(group);
                break;
            case "3":
                PrintStudentsWithPagination(group.Students);
                break;
            case "4":
                SearchStudent(group);
                break;
            case "5":
                EditStudent(group);
                break;
            case "6":
                PrintExcellentAndFailing(group);
                break;
            case "7":
                PrintStatistics(group);
                break;
            case "8":
                group.SaveToFile(storagePath);
                Console.WriteLine($"Дані збережено: {storagePath}");
                break;
            case "9":
                group = StudentGroup.LoadFromFile(storagePath);
                Console.WriteLine($"Дані завантажено: {storagePath}");
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Невідомий пункт меню.");
                break;
        }
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException)
    {
        Console.WriteLine($"Помилка: {ex.Message}");
    }

    Pause();
}

static void PrintMenu(StudentGroup group)
{
    ClearScreen();
    Console.WriteLine("Student Group Management System");
    Console.WriteLine($"Група: {group.Name}, спеціальність: {group.Specialty}, курс: {group.Course}");
    Console.WriteLine();
    Console.WriteLine("1. Додати студента");
    Console.WriteLine("2. Видалити студента");
    Console.WriteLine("3. Вивести всіх студентів");
    Console.WriteLine("4. Пошук студента");
    Console.WriteLine("5. Редагування даних студента");
    Console.WriteLine("6. Вивести відмінників / тих, хто має < 60 балів");
    Console.WriteLine("7. Вивести статистику групи");
    Console.WriteLine("8. Зберегти дані групи у файл");
    Console.WriteLine("9. Завантажити дані групи з файлу");
    Console.WriteLine("0. Вийти");
    Console.WriteLine();
}

static void AddStudent(StudentGroup group)
{
    Console.WriteLine("Додавання студента");
    var fullName = ReadRequired("ПІБ: ");
    var recordBookNumber = ReadRequired("Номер залікової (8 цифр): ");
    var dateOfBirth = ReadDate("Дата народження (дд.мм.рррр): ");
    var enrollmentDate = ReadDate("Дата зарахування (дд.мм.рррр): ");
    var email = ReadRequired("Email: ");
    var averageGrade = ReadDouble("Середній бал (0-100): ", 0, 100);
    var status = ReadStatus();
    var notes = ReadOptional("Нотатки: ");

    var student = new Student
    {
        FullName = fullName,
        RecordBookNumber = recordBookNumber,
        DateOfBirth = dateOfBirth,
        EnrollmentDate = enrollmentDate,
        PersonalEmail = email,
        Status = status,
        Notes = notes
    };

    student.UpdateAverageGrade(averageGrade);
    EditGradeJournal(student);
    student.RecalculateAverageGradeFromJournal();
    group.AddStudent(student);
    Console.WriteLine("Студента додано.");
}

static void RemoveStudent(StudentGroup group)
{
    var recordBookNumber = ReadRequired("Введіть номер залікової: ");
    Console.WriteLine(group.RemoveStudent(recordBookNumber)
        ? "Студента видалено."
        : "Студента з таким номером не знайдено.");
}

static void PrintStudentsWithPagination(IReadOnlyList<Student> students)
{
    if (students.Count == 0)
    {
        Console.WriteLine("У групі поки немає студентів.");
        return;
    }

    const int pageSize = 10;
    var page = 0;
    var totalPages = (int)Math.Ceiling(students.Count / (double)pageSize);

    while (true)
    {
        ClearScreen();
        Console.WriteLine($"Сторінка {page + 1} з {totalPages}");
        Console.WriteLine();

        foreach (var student in students.Skip(page * pageSize).Take(pageSize))
        {
            Console.WriteLine($"{student.RecordBookNumber} | {student.FullName} | {student.AverageGrade:F2} | {student.Status}");
        }

        Console.WriteLine();
        var command = ReadOptional("n - наступна, p - попередня, q - вихід: ").ToLowerInvariant();

        if (command == "n" && page < totalPages - 1)
        {
            page++;
        }
        else if (command == "p" && page > 0)
        {
            page--;
        }
        else if (command == "q")
        {
            break;
        }
    }
}

static void SearchStudent(StudentGroup group)
{
    Console.WriteLine("1. Пошук за номером залікової");
    Console.WriteLine("2. Пошук за ПІБ");
    var choice = ReadRequired("Ваш вибір: ");
    var query = ReadRequired("Пошуковий запит: ");

    var results = choice == "1"
        ? group.FindStudent(query, StudentSearchMode.ByRecordBookNumber)
        : group.FindStudent(query, StudentSearchMode.ByFullName);

    PrintStudentDetails(results);
}

static void EditStudent(StudentGroup group)
{
    var recordBookNumber = ReadRequired("Номер залікової студента для редагування: ");
    var student = group.FindStudent(recordBookNumber);

    if (student is null)
    {
        Console.WriteLine("Студента не знайдено.");
        return;
    }

    Console.WriteLine("1. ПІБ");
    Console.WriteLine("2. Дата народження");
    Console.WriteLine("3. Email");
    Console.WriteLine("4. Середній бал");
    Console.WriteLine("5. Статус");
    Console.WriteLine("6. Нотатки");
    Console.WriteLine("7. Журнал оцінок");
    var choice = ReadRequired("Що змінити: ");

    switch (choice)
    {
        case "1":
            student.FullName = ReadRequired("Новий ПІБ: ");
            break;
        case "2":
            student.DateOfBirth = ReadDate("Нова дата народження: ");
            break;
        case "3":
            student.PersonalEmail = ReadRequired("Новий email: ");
            break;
        case "4":
            student.UpdateAverageGrade(ReadDouble("Новий середній бал: ", 0, 100));
            break;
        case "5":
            student.Status = ReadStatus();
            break;
        case "6":
            student.Notes = ReadOptional("Нові нотатки: ");
            break;
        case "7":
            EditGradeJournal(student);
            student.RecalculateAverageGradeFromJournal();
            break;
        default:
            Console.WriteLine("Невідомий пункт.");
            return;
    }

    Console.WriteLine("Дані оновлено.");
}

static void PrintExcellentAndFailing(StudentGroup group)
{
    Console.WriteLine("Відмінники:");
    PrintStudentShortList(group.GetExcellentStudents());
    Console.WriteLine();
    Console.WriteLine("Студенти з балом < 60:");
    PrintStudentShortList(group.GetFailingStudents());
}

static void PrintStatistics(StudentGroup group)
{
    Console.WriteLine($"Кількість студентів: {group.GroupSize}");
    Console.WriteLine($"Середній бал групи: {group.AverageGroupGrade:F2}");
    Console.WriteLine($"Відсоток відмінників: {group.GetExcellentPercentage():F2}%");
    Console.WriteLine($"Активні: {group.GetStudentsByStatus(StudentStatus.Active).Count}");
    Console.WriteLine($"Академічна відпустка: {group.GetStudentsByStatus(StudentStatus.AcademicLeave).Count}");
    Console.WriteLine($"Відраховані: {group.GetStudentsByStatus(StudentStatus.Expelled).Count}");
    Console.WriteLine($"Випускники: {group.GetStudentsByStatus(StudentStatus.Graduated).Count}");
}

static void EditGradeJournal(Student student)
{
    Console.WriteLine("Журнал оцінок. Залиште назву предмета порожньою, щоб завершити.");

    while (true)
    {
        var subject = ReadOptional("Предмет: ");
        if (string.IsNullOrWhiteSpace(subject))
        {
            break;
        }

        var grade = ReadDouble("Оцінка (0-100): ", 0, 100);
        student.GradeJournal.AddOrUpdateGrade(subject, grade);
    }
}

static void PrintStudentDetails(IReadOnlyList<Student> students)
{
    if (students.Count == 0)
    {
        Console.WriteLine("Нічого не знайдено.");
        return;
    }

    foreach (var student in students)
    {
        Console.WriteLine(new string('-', 40));
        Console.WriteLine(student.ShowDetailedInfo());
    }
}

static void PrintStudentShortList(IReadOnlyList<Student> students)
{
    if (students.Count == 0)
    {
        Console.WriteLine("Список порожній.");
        return;
    }

    foreach (var student in students)
    {
        Console.WriteLine($"{student.RecordBookNumber} | {student.FullName} | {student.AverageGrade:F2}");
    }
}

static string ReadRequired(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var value = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }

        Console.WriteLine("Значення не може бути порожнім.");
    }
}

static string ReadOptional(string prompt)
{
    Console.Write(prompt);
    return Console.ReadLine()?.Trim() ?? string.Empty;
}

static DateTime ReadDate(string prompt)
{
    while (true)
    {
        var value = ReadRequired(prompt);
        if (DateTime.TryParse(value, new CultureInfo("uk-UA"), DateTimeStyles.None, out var date))
        {
            return date.Date;
        }

        Console.WriteLine("Введіть дату у форматі дд.мм.рррр.");
    }
}

static double ReadDouble(string prompt, double min, double max)
{
    while (true)
    {
        var value = ReadRequired(prompt);
        if (double.TryParse(value.Replace(',', '.'), CultureInfo.InvariantCulture, out var number)
            && number >= min
            && number <= max)
        {
            return number;
        }

        Console.WriteLine($"Введіть число від {min} до {max}.");
    }
}

static StudentStatus ReadStatus()
{
    Console.WriteLine("Статус:");
    Console.WriteLine("1. Active");
    Console.WriteLine("2. AcademicLeave");
    Console.WriteLine("3. Expelled");
    Console.WriteLine("4. Graduated");

    while (true)
    {
        var value = ReadRequired("Оберіть статус: ");
        if (int.TryParse(value, out var number)
            && Enum.IsDefined(typeof(StudentStatus), number))
        {
            return (StudentStatus)number;
        }

        Console.WriteLine("Оберіть число від 1 до 4.");
    }
}

static void Pause()
{
    Console.WriteLine();
    Console.Write("Натисніть Enter для продовження...");
    Console.ReadLine();
}

static void ClearScreen()
{
    if (!Console.IsInputRedirected && !Console.IsOutputRedirected)
    {
        Console.Clear();
    }
}
