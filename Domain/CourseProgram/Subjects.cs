/// <summary>Class para criar objetos do tipo Disciplinas</summary>
namespace School_System.Domain.CourseProgram;

using static System.Console;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Data.Common;

using School_System.Infrastructure.FileManager;
using School_System.Domain.Base;
using School_System.Domain.SchoolMembers;


internal class Subject : BaseEntity
{
    [JsonInclude] internal short ECTS_i { get; private set; }
    [JsonInclude] internal List<Teacher> Professor_l { get; private set; } = [];
    [JsonInclude] internal List<Student> Students_l { get; private set; } = [];
    [JsonIgnore] private const short MinEct = 3;
    protected override string Describe()
    {
        return $"ID={ID_i}, Nome='{Name_s}', ECTS={ECTS_i}, Professores={Professor_l.Count}, Alunos={Students_l.Count}";
    }


    // construtor para desserialização
    public Subject() : base(0, "") { }
    // construtor principal
    internal Subject(int id, short ects, string name = "") : base(id, name)
    {
        ECTS_i = ects;
    }

    /// <summary>
    /// Solicita ao usuário o número de ECTS de uma disciplina.
    /// </summary>
    /// <param name="prompt">Mensagem a exibir ao usuário.</param>
    /// <param name="currentValue">
    /// Valor atual dos ECTS (usado apenas quando <paramref name="isToEdit"/> for true).
    /// Se o usuário pressionar Enter, esse valor será mantido.
    /// </param>
    /// <param name="isToEdit">Indica se a função está sendo usada em modo de edição.</param>
    /// <returns>O valor de ECTS como <see cref="short"/>.</returns>
    private static short InputSubjectsECTS(string prompt, short? currentValue = null, bool isToEdit = false)
    {
        while (true)
        {
            // Prompt correto dependendo se está a editar
            if (isToEdit && currentValue.HasValue) Write($"{prompt} (Enter para manter '{currentValue}'): ");
            else Write($"{prompt} ({MinEct}-{Course.MaxEctsPerSemester} ECTS, Enter para default): ");

            string? input = ReadLine()?.Trim();

            // Entrada vazia
            if (string.IsNullOrWhiteSpace(input))
            {
                if (isToEdit && currentValue.HasValue) return currentValue.Value; // mantém valor atual
                WriteLine(EmptyEntrance);
                return MinEct; // default: mínimo permitido
            }
            // Tenta converter
            if (short.TryParse(input, out short ects))
            {
                if (ects >= MinEct && ects <= Course.MaxEctsPerSemester)
                    return ects;
                WriteLine($"Valor inválido. Insira entre {MinEct} e {Course.MaxEctsPerSemester} ECTS.");
            }
            else
            {
                WriteLine($"{InvalidEntrance} Insira um número inteiro.");
            }
        }
    }

    internal override BaseEntity? CreateInstance() => Create();
    internal static Subject? Create()
    {
        string? input_s;
        string prompt;

        // --- Nome da disciplina ---
        prompt = "Escreva o nome da disciplina";
        string name = InputName(prompt);

        // --- ECTS ---
        prompt = "Escreva o número de ECTS da disciplina";
        short ects = InputSubjectsECTS(prompt);

        // --- Resumo ---
        WriteLine("\nResumo da Disciplina:");
        WriteLine($" Nome: {(string.IsNullOrEmpty(name) ? "<default>" : name)}");
        WriteLine($" ECTS: {ects}");
        Write("Tem a certeza que quer criar esta disciplina? (S/N): ");

        input_s = ReadLine()?.Trim().ToUpper();
        if (input_s != "S") return null;

        // --- Criar ID ---
        int newID = FileManager.GetTheNextAvailableID(FileManager.DataBaseType.Subject);
        if (newID == -1) { WriteLine(ProblemGetTheId); return null; }

        // --- Criar objeto ---
        Subject disc = new(newID, ects, name);

        // Guardar na base de dados
        FileManager.WriteOnDataBase(FileManager.DataBaseType.Subject, disc);

        return disc;
    }
    internal static void Remove() { RemoveEntity<Subject>("Disciplina", FileManager.DataBaseType.Subject); }
    internal static void Select() { }
}