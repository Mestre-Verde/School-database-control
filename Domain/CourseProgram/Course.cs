/// <summary>Class que cria objetos do tipo Curso</summary>
namespace School_System.Domain.CourseProgram;

using static System.Console;
using System.Text.Json.Serialization;

using School_System.Infrastructure.FileManager;
using School_System.Domain.Base;
using School_System.Application.Utils;
using Schoo_lSystem.Application.Menu;

public class Course : BaseEntity
{
    [JsonInclude] private CourseType_e Type_e;
    [JsonInclude] private float Duration_f { get; set; } // duração em anos pode ser 0,5

    protected override string FormatToString()
    {
        string baseDesc = BaseFormat();
        return $"{baseDesc}, Tipo: {Type_e}, Duração: {Duration_f} anos";
    }

    protected override void Introduce() { Write("\nNovo Curso: "); WriteLine(FormatToString()); }

    // Construtor parameterless obrigatório para descerialização JSON
    public Course() : base(0, "") { }

    // Construtor principal
    private Course(string name = "", int id = default, CourseType_e type = default, float duracao = default)
        : base(id, name)
    {
        Type_e = type;
        Duration_f = duracao;

        Introduce();
    }

    internal static Course? Create()
    {
        return CreateEntity("do Curso", FileManager.DataBaseType.Course,
            dict =>
            {
                dict["Type"] = InputParameters.InputCourseType("Escreva o tipo de curso");
                dict["Duration"] = InputParameters.InputCourseDuration("Escreva a duração do curso em anos");
            },
            dict => new Course(
                (string)dict["Name"],
                (int)dict["ID"],
                (CourseType_e)dict["Type"],
                (float)dict["Duration"]
            )
        );
    }

    internal static void Remove() { RemoveEntity<Course>("Curso", FileManager.DataBaseType.Course); }

    internal static void Select()
    {
        var selected = AskAndSearch<Course>("curso", FileManager.DataBaseType.Course);
        if (selected.Count == 0) return;

        Course course = selected[0];
        EditCourse(course);
    }
    private static void PrintCourseComparison(Course current, dynamic original)
    {
        WriteLine("\n===== ESTADO DO CURSO =====");
        WriteLine($"{"Campo",-15} | {"Atual",-25} | {"Original"}");
        WriteLine(new string('-', 70));

        void Show(string label, object? now, object? old) => WriteLine($"{label,-15} | {now,-25} | {old}");

        Show("Nome", current.Name_s, original.Name_s);
        Show("Tipo", current.Type_e, original.Type_e);
        Show("Duração", current.Duration_f, original.Duration_f);

        WriteLine(new string('=', 70));
    }
    internal static void EditCourse(Course course)
    {
        // 1. Guardar original
        var original = new
        {
            course.Name_s,
            course.Type_e,
            course.Duration_f,
        };

        bool hasChanged = false;

        Write(Menu.GetMenuEditCourse());

        while (true)
        {
            var option = Menu.MenuEditCourse();
            if (option == Menu.EditParamCourse_e.Back) break;

            switch (option)
            {
                case Menu.EditParamCourse_e.Help:
                    PrintCourseComparison(course, original);
                    break;

                case Menu.EditParamCourse_e.Name:
                    course.Name_s = InputParameters.InputName(
                        "Escreva o nome do curso",
                        course.Name_s,
                        true);
                    hasChanged = true;
                    break;

                case Menu.EditParamCourse_e.Type:
                    course.Type_e = InputParameters.InputCourseType(
                        "Escreva o tipo do curso",
                        course.Type_e,
                        true);
                    hasChanged = true;
                    break;

                case Menu.EditParamCourse_e.Duration:
                    course.Duration_f = InputParameters.InputCourseDuration(
                        "Escreva a duração do curso em anos",
                        course.Duration_f,
                        true);
                    hasChanged = true;
                    break;
            }
        }

        // 3. Guardar
        if (!hasChanged) return;

        Write("\nGuardar alterações? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) == "S")
        {
            FileManager.WriteOnDataBase(FileManager.DataBaseType.Course, course);
            WriteLine("✔️ Alterações salvas.");
        }
        else
        {
            WriteLine("❌ Alterações descartadas.");
            course.Name_s = original.Name_s;
            course.Type_e = original.Type_e;
            course.Duration_f = original.Duration_f;
        }
    }


}
