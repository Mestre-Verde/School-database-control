/// <summary>Class que cria objetos do tipo Curso</summary>
namespace School_System.Domain.CourseProgram;

using static System.Console;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Data.Common;

using School_System.Infrastructure.FileManager;
using Schoo_lSystem.Application.Menu;
using School_System.Domain.Base;
using School_System.Domain.CourseProgram;
using School_System.Domain.SchoolMembers;
using School_System.Application.Utils;

public enum CourseType_e
{
    NONE = 0,
    CTESP = 5, // nivel 5
    Licenciatura = 6,
    Mestrado = 7,
    Doutoramento = 8
}

public class Course : BaseEntity
{
    [JsonInclude] internal CourseType_e Type_e { get; private set; }
    [JsonInclude] internal float Duration_f { get; set; } // duração em anos pode ser 0,5
    [JsonInclude] internal List<Subject> Subjects_l { get; private set; } = []; // o curso tem disciplinas default
    internal protected const short MinCourseEct = 60;  // mínimo razoável para um curso
    internal protected const short MaxCourseEct = 360; // máximo típico de licenciatura prolongada

    internal protected const short MaxEctsPerYear = 60;
    internal protected const short MaxEctsPerSemester = MaxEctsPerYear / 2;

    protected override string FormatToString()
    {
        string baseDesc = BaseFormat();
        return $"{baseDesc}, Tipo: {Type_e}, Duração: {Duration_f} anos, Disciplinas: {Subjects_l.Count()}";
    }

    protected override void Introduce() { Write("\nNovo Curso: "); WriteLine(FormatToString()); }

    // Construtor parameterless obrigatório para descerialização JSON
    public Course() : base(0, "") { }

    // Construtor principal
    private Course(string name = "", int id = default, CourseType_e type = default, float duracao = default, List<Subject>? subjects = null)
        : base(id, name)
    {
        Type_e = type;
        Duration_f = duracao;
        Subjects_l = subjects ?? new List<Subject>();// Se a lista passada for null, inicializa vazia

        Introduce();
    }

    internal static Course? Create()
    {
        return CreateEntity<Course>(
            "do curso",
            FileManager.DataBaseType.Course,
            dict =>
            {
                dict["Type"] = InputParameters.InputCourseType("Escreva o tipo de curso");
                dict["Duration"] = InputParameters.InputCourseDuration("Escreva a duração do curso em anos");
                dict["Subjects"] = InputParameters.InputSubjects("Selecione as disciplinas do curso");
            },
            dict => new Course(
                (string)dict["Name"],
                (int)dict["ID"],
                (CourseType_e)dict["Type"],
                (float)dict["Duration"],
                (List<Subject>)dict["Subjects"]
            )
        );
    }

    internal static void Remove() { RemoveEntity<Course>("Curso", FileManager.DataBaseType.Course); }

    internal static void Select()
    {
        // aqui vai ser diferente, equanto nos alunos 
    }

}
