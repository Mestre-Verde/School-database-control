/// <summary>Class que cria objetos do tipo Curso</summary>
namespace School_System.Domain.CourseProgram;

using static System.Console;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Data.Common;

using School_System.Domain.Base;
using School_System.Infrastructure.FileManager;

internal class Course : BaseEntity
{
    [JsonInclude] internal CourseType_e Type_e { get; private set; }
    [JsonInclude] internal float Duration_f { get; set; } // duração em anos pode ser 0,5
    [JsonInclude] internal List<Subject> Subjects_l { get; private set; } = []; // o curso tem disciplinas
    protected override string Describe()
    {
        throw new NotImplementedException();
    }

    internal protected const short MinCourseEct = 60;  // mínimo razoável para um curso
    internal protected const short MaxCourseEct = 360; // máximo típico de licenciatura prolongada

    internal protected const short MaxEctsPerYear = 60;
    internal protected const short MaxEctsPerSemester = MaxEctsPerYear / 2;
    internal enum CourseType_e
    {
        NONE = 0,
        CTESP = 5, // nivel 5
        Licenciatura = 6,
        Mestrado = 7,
        Doutoramento = 8
    }

    public Course() : base(0, "") { } // construtor para desserialização
    private Course(string name = "", int id = default, CourseType_e type = default, float duracao = default) : base(id, name)
    {
        Type_e = type;
        Duration_f = duracao;
    }

    /// <summary>
    /// Solicita ao usuário que selecione um tipo de curso.
    /// </summary>
    /// <param name="prompt">Mensagem a exibir para o usuário.</param>
    /// <param name="currentValue">
    /// Valor atual do tipo de curso (usado somente se <paramref name="isToEdit"/> for true). 
    /// Caso o usuário pressione Enter, esse valor será mantido.
    /// </param>
    /// <param name="isToEdit">Indica se a função está sendo chamada para edição (true) ou criação (false).</param>
    /// <returns>
    /// O tipo de curso escolhido pelo usuário como um <see cref="CourseType_e"/>.
    /// Se a entrada for vazia na criação, retorna <see cref="CourseType_e.NONE"/>.
    /// </returns>
    private static CourseType_e InputCourseType(string prompt, CourseType_e? currentValue = null, bool isToEdit = false)
    {
        var courseMap = new Dictionary<string, CourseType_e>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", CourseType_e.NONE }, { "none", CourseType_e.NONE },
            { "5", CourseType_e.CTESP }, { "ctesp", CourseType_e.CTESP },
            { "6", CourseType_e.Licenciatura }, { "licenciatura", CourseType_e.Licenciatura },
            { "7", CourseType_e.Mestrado }, { "mestrado", CourseType_e.Mestrado },
            { "8", CourseType_e.Doutoramento }, { "doutoramento", CourseType_e.Doutoramento }
        };

        while (true)
        {
            if (isToEdit && currentValue.HasValue) Write($"{prompt} (Enter para manter '{currentValue}'): ");
            else Write($"{prompt} (tipo ou número, 'Ajuda' para opções): ");

            string? input = ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                WriteLine(EmptyEntrance);
                if (isToEdit && currentValue.HasValue) return currentValue.Value;
                else return CourseType_e.NONE;
            }

            if (courseMap.TryGetValue(input, out CourseType_e result)) return result;

            WriteLine(InvalidEntrance);
            WriteLine("Digite 'Ajuda' para ver todas as opções.");
            if (string.Equals(input, "Ajuda", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var course in courseMap) WriteLine($" - {course.Key} = {course.Value}");
            }
        }
    }

    /// <summary>
    /// Solicita ao usuário a duração de um curso em anos.
    /// </summary>
    /// <param name="prompt">Mensagem a exibir para o usuário.</param>
    /// <param name="currentValue">
    /// Valor atual da duração (usado somente se <paramref name="isToEdit"/> for true). 
    /// Caso o usuário pressione Enter, esse valor será mantido.
    /// </param>
    /// <param name="isToEdit">Indica se a função está sendo chamada para edição (true) ou criação (false).</param>
    /// <returns>
    /// A duração do curso em anos como <see cref="float"/>. Se a entrada for vazia na criação, retorna 0.
    /// </returns>
    private static float InputCourseDuration(string prompt, float? currentValue = null, bool isToEdit = false)
    {
        while (true)
        {
            if (isToEdit && currentValue.HasValue)
                Write($"{prompt} (Enter para manter '{currentValue}'): ");
            else
                Write($"{prompt} (ex: 0,5 para 1 semestre, Enter para default): ");

            string? input = ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                WriteLine(EmptyEntrance);
                if (isToEdit && currentValue.HasValue) return currentValue.Value;
                return default;
            }

            // Aqui usamos a cultura atual do sistema — PT-PT se o PC estiver em PT
            if (float.TryParse(input, out float duration))
            {
                if (duration >= 0) return duration;
                WriteLine(InvalidEntrance + " A duração não pode ser negativa.");
            }
            else
            {
                WriteLine(InvalidEntrance + " Use um número válido com vírgula (ex: 1 ou 0,5).");
            }
        }
    }

    internal override BaseEntity? CreateInstance() => Create();
    internal static Course? Create()
    {
        string prompt;

        // --- Nome ---
        prompt = "Escreva o nome do curso";
        string name = InputName(prompt);

        // --- type ---
        prompt = "Escreva o tipo de curso";
        CourseType_e type = InputCourseType(prompt);

        // --- Duração ---
        prompt = "Escreva a duração do curso em anos";
        float duration = InputCourseDuration(prompt);

        // --- Confirmação final ---
        WriteLine($"\nResumo do Curso:");
        WriteLine($" Nome: {(string.IsNullOrEmpty(name) ? "<default>" : name)}");
        WriteLine($" Grau do curso: {type}");
        WriteLine($" Duração: {duration} anos");
        Write("Tem a certeza que quer criar este curso? (S/N): ");
        string? input_s = ReadLine()?.Trim().ToUpper();
        if (input_s != "S") return null; // Cancela criação se não confirmar


        int newID = FileManager.GetTheNextAvailableID(FileManager.DataBaseType.Course);
        if (newID == -1) { WriteLine(ProblemGetTheId); return null; }
        // --- Criação do objeto ---
        Course curso = new(name, newID, type, duration);

        FileManager.WriteOnDataBase(FileManager.DataBaseType.Course, curso);

        return curso;
    }

    internal static void Remove() { RemoveEntity<Course>("Curso", FileManager.DataBaseType.Course); }


    internal static void Select()
    {
        // aqui vai ser diferente, equanto nos alunos 
    }

}
