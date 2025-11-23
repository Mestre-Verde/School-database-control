using static System.Console;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Data.Common;
using System.Globalization;


internal class Course : BaseEntity
{
    [JsonInclude] internal CourseType_e Type_e { get; private set; }
    [JsonInclude] internal float Duration_f { get; set; } // duração em anos pode ser 0,5
    [JsonInclude] internal List<Discipline> Subjects_l { get; private set; } = []; // o curso tem disciplinas

    internal protected const short MinCourseEct = 60;  // mínimo razoável para um curso
    internal protected const short MaxCourseEct = 360; // máximo típico de licenciatura prolongada

    internal protected const short MaxEctsPerYear = 60;
    internal protected const short MaxEctsPerSemester = MaxEctsPerYear / 2;

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
            if (isToEdit && currentValue.HasValue) Write($"{prompt} (Enter para manter '{currentValue}'): ");
            else Write($"{prompt} (ex: 0,5 para 1 semestre, Enter para default): ");

            string? input = ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                WriteLine(EmptyEntrance);
                if (isToEdit && currentValue.HasValue) return currentValue.Value;
                else return default;
            }

            input = input.Replace(',', '.');

            if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float duration))
            {
                if (duration >= 0) return duration;
                else WriteLine(InvalidEntrance + " Não pode ser negativa.");
            }
            else { WriteLine(InvalidEntrance + " Use um número válido (ex: 1 ou 0,5)."); }
        }
    }

    internal static Course? Create()
    {
        string prompt;

        // --- Nome ---
        prompt = "Escreva o nome do curso: ";
        string name = InputName(prompt);

        // --- type ---
        prompt = "Escreva o tipo de curso ";
        CourseType_e type = InputCourseType(prompt);

        // --- Duração ---
        prompt = "Escreva a duração do curso em anos ";
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
        if (newID == -1) { WriteLine("❌ Erro: Não foi possível obter um ID válido para o curso. Criação cancelada."); return null; }
        // --- Criação do objeto ---
        Course curso = new(name, newID, type, duration);

        FileManager.WriteOnDataBase(FileManager.DataBaseType.Course, curso);

        return curso;
    }

    internal static void Remove()
    {
        Write("Digite o nome ou ID do curso para remover: ");
        string input = ReadLine() ?? "";

        bool isId = int.TryParse(input, out int idInput);
        var dbType = FileManager.DataBaseType.Course;

        var matches = isId ? FileManager.Search<Course>(dbType, id: idInput) : FileManager.Search<Course>(dbType, name: input);

        if (matches.Count == 0)
        {
            WriteLine("Nenhum curso encontrado.");
            return;
        }

        WriteLine("Foram encontrados os seguintes cursos:");
        for (int i = 0; i < matches.Count; i++)
        {
            var s = matches[i];
            WriteLine($"{i + 1}: ID={s.ID_i}, Nome='{s.Name_s}'");
        }

        Write("Escolha o(s) número(s) do(s) curso(s) a remover: ");
        string choiceInput = ReadLine() ?? "";

        var indices = choiceInput
            .Split([',', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out int x) ? x : -1)
            .Where(x => x >= 1 && x <= matches.Count)
            .Distinct()
            .ToList();

        if (indices.Count == 0)
        {
            WriteLine("Nenhuma seleção válida. Operação cancelada.");
            return;
        }

        WriteLine("Você selecionou os seguintes cursos para remoção:");
        foreach (var idx in indices)
        {
            var s = matches[idx - 1];
            WriteLine($"- ID={s.ID_i}, Nome='{s.Name_s}'");
        }

        Write("Tem certeza que deseja remover todos esses cursos? (S/N): ");
        string confirm = ReadLine()?.Trim().ToUpper() ?? "N";
        if (confirm != "S") { WriteLine("Operação cancelada."); return; }

        foreach (var idx in indices)
        {
            var s = matches[idx - 1];
            bool removed = FileManager.RemoveById<Course>(dbType, s.ID_i);
            if (removed) WriteLine($"✅ Curso removido: ID={s.ID_i}, Nome='{s.Name_s}'");
            else WriteLine($"❌ Erro ao remover: ID={s.ID_i}, Nome='{s.Name_s}'");
        }
    }

    internal static void Select()
    {
        // aqui vai ser diferente, equanto nos alunos 
    }

}


internal class Discipline : BaseEntity
{
    internal short ECTS_i { get; private set; }
    internal List<Teacher> Professor_l { get; private set; } = [];
    internal List<Student> Students_l { get; private set; } = [];

    internal protected const short MinEtc = 3;
    internal protected const short MaxSemesterEtc = 30;


    public Discipline() : base(0, "") { }

    internal Discipline(int id, short ects, string name = "") : base(id, name)
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
    private static short InputDisciplineECTS(string prompt, short? currentValue = null, bool isToEdit = false)
    {
        while (true)
        {
            // Prompt correto dependendo se está a editar
            if (isToEdit && currentValue.HasValue)
                Write($"{prompt} (Enter para manter '{currentValue}'): ");
            else
                Write($"{prompt} ({Discipline.MinEtc}–{Discipline.MaxSemesterEtc} ECTS, Enter para default): ");

            string? input = ReadLine()?.Trim();

            // Entrada vazia
            if (string.IsNullOrWhiteSpace(input))
            {
                if (isToEdit && currentValue.HasValue)
                    return currentValue.Value; // mantém valor atual

                WriteLine(EmptyEntrance);
                return Discipline.MinEtc; // default: mínimo permitido
            }

            // Tenta converter
            if (short.TryParse(input, out short ects))
            {
                if (ects >= Discipline.MinEtc && ects <= Discipline.MaxSemesterEtc)
                    return ects;

                WriteLine($"Valor inválido. Insira entre {Discipline.MinEtc} e {Discipline.MaxSemesterEtc} ECTS.");
            }
            else
            {
                WriteLine($"{InvalidEntrance} Insira um número inteiro.");
            }
        }
    }


    internal static Discipline? Create()
    {
        string? input_s;
        string prompt;

        // --- Nome da disciplina ---
        prompt = "Escreva o nome da disciplina: ";
        string name = InputName(prompt);

        // --- ECTS ---
        prompt = "Escreva o número de ECTS da disciplina";
        short ects = InputDisciplineECTS(prompt);


        // --- Resumo ---
        WriteLine("\nResumo da Disciplina:");
        WriteLine($" Nome: {(string.IsNullOrEmpty(name) ? "<default>" : name)}");
        WriteLine($" ECTS: {ects}");
        Write("Tem a certeza que quer criar esta disciplina? (S/N): ");

        input_s = ReadLine()?.Trim().ToUpper();
        if (input_s != "S") return null;

        // --- Criar ID ---
        int newID = FileManager.GetTheNextAvailableID(FileManager.DataBaseType.Discipline);
        if (newID == -1)
        {
            WriteLine("❌ Erro: Não foi possível obter um ID válido. Criação cancelada.");
            return null;
        }

        // --- Criar objeto ---
        Discipline disc = new(newID, ects, name);

        // Guardar na base de dados
        FileManager.WriteOnDataBase(FileManager.DataBaseType.Discipline, disc);

        return disc;
    }

    internal static void Remove()
    {

    }
    internal static void Select()
    {

    }
}

/*
Um curso completo normalmente tem 180 a 360 ECTS, dependendo do nível:

CTESP → 120 ECTS (2 anos)

Licenciatura → 180–240 ECTS (3–4 anos)

Mestrado → 60–120 ECTS (1–2 anos)

Doutoramento → varia, normalmente 0–60 ECTS formalmente, pois foca em investigação.


Mínimo: 3 ECTS → disciplinas curtas, seminários ou laboratórios

Médio: 5–6 ECTS → a maioria das disciplinas regulares

Máximo: 15 ECTS → apenas para projetos longos ou módulos especiais

1 ano letivo normalmente tem 60 ECTS.

1 semestre = metade do ano → 30 ECTS.
*/


