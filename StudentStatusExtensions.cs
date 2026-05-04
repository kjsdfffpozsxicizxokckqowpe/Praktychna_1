namespace StudentGroupManagement;

public static class StudentStatusExtensions
{
    public static string ToDisplayName(this StudentStatus status)
    {
        return status switch
        {
            StudentStatus.Active => "Активний",
            StudentStatus.AcademicLeave => "Академічна відпустка",
            StudentStatus.Expelled => "Відрахований",
            StudentStatus.Graduated => "Випускник",
            _ => status.ToString()
        };
    }
}
