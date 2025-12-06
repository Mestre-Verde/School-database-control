namespace School_System.Domain.SchoolMembers;

using static System.Console; // Permite usar Write e WriteLine diretamente
using System.Text.Json.Serialization;

using School_System.Domain.CourseProgram;
using School_System.Domain.Scholarship;
using School_System.Application.Utils;

internal abstract class Student : SchoolMember
{
    [JsonInclude] protected decimal GPA = default;
    [JsonInclude] protected Course? Major { get; set; }
    [JsonInclude] protected int Year { get; set; }
    [JsonInclude] protected List<Subject> EnrolledSubjects = [];// o estudante vai estar inscrito em disicplinas
    [JsonInclude] List<Bolsa> Scholarships = [];

    protected override string FormatToString()
    {
        string baseDesc = base.FormatToString();
        string? courseName = Major?.Name_s ?? "N/A";
        return $"{baseDesc}, Curso: {courseName}, Ano: {Year}, Disciplinas inscrito(a): {EnrolledSubjects?.Count ?? 0}, GPA: {GPA}";
    }

    protected override void Introduce() { Write($"\n🎓 New Student: "); WriteLine(FormatToString()); }

    // Construtor parameterless obrigatório para descerialização JSON
    public Student() : base() { }

    protected Student(int id, string name, byte age, char gender, DateTime? birthDate, Nationality_e nationality, string email,
         Course? major = null, int year = default, List<Subject>? enrolledSubjects = default)
        : base(id, name, age, gender, birthDate, nationality, email)
    {
        Major = major;
        Year = year;
        EnrolledSubjects = enrolledSubjects ?? [];
    }

    //----------------------------------

    protected decimal CalculateGPA() { return 1m; }
    protected void AddGradeToSubject(Subject dsiciplina, decimal grade) { }
    protected virtual decimal CalculateTuition() { return 0m; }
}