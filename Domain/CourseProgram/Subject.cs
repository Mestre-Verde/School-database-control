/// <summary>Class para criar objetos do tipo Disciplinas</summary>
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


public class Subject : BaseEntity
{
    [JsonInclude] internal short ECTS_i { get; private set; }
    [JsonInclude] internal Teacher Professor { get; private set; }
    [JsonInclude] internal byte Grade = default;
    [JsonIgnore] private const short MinEct = 3;

    protected override string FormatToString()
    {
        string baseDesc = BaseFormat();
        return $"{baseDesc}, ECTS={ECTS_i}, Professores={Professor}.";
    }

    protected override void Introduce() { Write("\nNova disciplina: "); WriteLine(FormatToString()); }

    // Construtor parameterless obrigatório para descerialização JSON
    public Subject() : base(0, "") { }

    // Construtor principal
    private Subject(int id, short ects, string name = "", Teacher? professor = null)
        : base(id, name)
    {
        ECTS_i = ects;
        Professor = professor;
    }

    internal static Subject? Create()
    {
        return BaseEntity.CreateEntity(
            typeObject: "da disciplina",
            dbType: FileManager.DataBaseType.Subject,
            collectSpecificFields: dict =>
            {
                dict["ECTS"] = InputParameters.InputSubjectsECTS("Escreva o número de ECTS da disciplina", MinEct);
                dict["Professor"] = InputParameters.InputTeacher("Selecione o professor da disciplina");
            },
            factory: dict =>
            {
                return new Subject(
                    id: (int)dict["ID"],
                    ects: (short)dict["ECTS"],
                    name: (string)dict["Name"],
                    professor: (Teacher?)dict["Professor"]
                );
            }
        );
    }

    internal static void Remove() { RemoveEntity<Subject>("Disciplina", FileManager.DataBaseType.Subject); }

    internal static void Select() { }
}