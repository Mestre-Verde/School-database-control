/// <summary>Class para criar objetos do tipo Disciplinas</summary>
namespace School_System.Domain.CourseProgram;

using static System.Console;
using System.Text.Json.Serialization;


using School_System.Infrastructure.FileManager;
using Schoo_lSystem.Application.Menu;
using School_System.Domain.Base;
using School_System.Domain.SchoolMembers;
using School_System.Application.Utils;

public class Subject : BaseEntity
{
    [JsonInclude] internal short ECTS_i { get; private set; }
    [JsonInclude] internal Teacher? Professor { get; private set; }
    [JsonInclude] internal int Grade = default;
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
        return CreateEntity("da Disciplina", FileManager.DataBaseType.Subject,
            dict =>
            {
                dict["ECTS"] = InputParameters.InputSubjectsECTS("Escreva o número de ECTS da disciplina", MinEct);
                dict["Professor"] = InputParameters.InputTeacher("Selecione o professor da disciplina");
            },
            factory: dict =>
            {
                return new Subject(
                    id: (int)dict["ID"],
                    name: (string)dict["Name"],
                    ects: (short)dict["ECTS"],
                    professor: dict["Professor"] as Teacher
                );
            }
        );
    }

    internal static void Remove() { RemoveEntity<Subject>("Disciplina", FileManager.DataBaseType.Subject); }

    internal static void Select() { SelectEntity<Subject>("disciplina", FileManager.DataBaseType.Subject, EditSubject); }

    private static void PrintSubjectComparison(Subject current, dynamic original)
    {
        WriteLine("\n===== ESTADO DA DISCIPLINA =====");
        WriteLine($"{"Campo",-12} | {"Atual",-25} | {"Original"}");
        WriteLine(new string('-', 60));

        void Show(string label, object? now, object? old)
            => WriteLine($"{label,-12} | {now,-25} | {old}");

        Show("Nome", current.Name_s, original.Name_s);
        Show("ECTS", current.ECTS_i, original.ECTS_i);
        Show("Professor", current.Professor?.Name_s, original.Professor);
        Show("Nota", current.Grade, original.Grade);

        WriteLine(new string('=', 60));
    }

    private static void EditSubject(Subject subject)
    {
        // 1. Guardar estado original
        var original = new
        {
            subject.Name_s,
            subject.ECTS_i,
            Professor = subject.Professor?.Name_s ?? "Nenhum",
            subject.Grade
        };

        bool hasChanged = false;

        // 2. Mostrar menu inicial
        Write(Menu.GetMenuEditSubject());

        // 3. Loop de edição
        while (true)
        {
            var option = Menu.MenuEditSubject();
            if (option == Menu.EditParamSubject_e.Back) break;

            switch (option)
            {
                case Menu.EditParamSubject_e.Help:
                    PrintSubjectComparison(subject, original);
                    break;

                case Menu.EditParamSubject_e.Name:
                    subject.Name_s = InputParameters.InputName("Escreva o nome da disciplina",
                                                                subject.Name_s, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamSubject_e.ECTS:
                    subject.ECTS_i = InputParameters.InputSubjectsECTS(
                        "Escreva o número de ECTS",
                        MinEct,
                        subject.ECTS_i,
                        true);
                    hasChanged = true;
                    break;

                case Menu.EditParamSubject_e.Professor:
                    subject.Professor = InputParameters.InputTeacher("Selecione o professor");
                    hasChanged = true;
                    break;

                case Menu.EditParamSubject_e.Grade:
                    subject.Grade = (byte)InputParameters.InputInt("Escreva a nota (0-20)", 0, 20, subject.Grade, true);
                    hasChanged = true;
                    break;
            }
        }

        // 4. Confirmar guarda
        if (!hasChanged) return;

        Write("\nGuardar alterações? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) == "S")
        {
            FileManager.WriteOnDataBase(FileManager.DataBaseType.Subject, subject);
            WriteLine("Alterações salvas.");
        }
        else
        {
            WriteLine("Alterações descartadas.");

            // Reverter
            subject.Name_s = original.Name_s;
            subject.ECTS_i = (short)original.ECTS_i;
            subject.Professor = null;
            subject.Grade = original.Grade;
        }
    }
}