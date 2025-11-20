using static System.Console;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Data.Common;


internal class Course
{
    [JsonInclude] internal string Name_s { get; private set; } = "";
    [JsonInclude] internal int ID_i { get; private set; }
    [JsonInclude] internal CourseType_e Type_e { get; private set; }
    [JsonInclude] internal float Duration_f { get; set; } // duração em anos pode ser 0,5
    [JsonInclude] internal List<Discipline> Subjects_l { get; private set; } = []; // o curso tem disciplinas

    public Course() { } // construtor vazio para desserialização
    private Course(string name = "", int id = default, CourseType_e type = default, float duracao = default)
    {
        Name_s = name;
        ID_i = id;
        Type_e = type;
        Duration_f = duracao;

    }

    internal static Course? Create()
    {
        //WriteLine("Inside Course.Create()");

        string? input_s;
        string name = "";
        CourseType_e type = default;
        float duration = default;

        // --- Nome ---
        while (true)
        {
            Write("Escreva o nome do curso: ");
            input_s = ReadLine()?.Trim();

            // Se vazio, apenas continua sem atualizar o nome
            if (string.IsNullOrEmpty(input_s)) { break; }

            // Validar caracteres permitidos
            if (!Regex.IsMatch(input_s, @"^[a-zA-Z0-9À-ÿ \-']+$"))
            {
                WriteLine("❌ Nome inválido. Apenas letras, números, espaços, hífen e apóstrofo são permitidos.");
                continue;
            }

            // Nome válido → atualizar e sair do loop
            name = input_s;
            break;
        }

        // --- type ---
        while (true)
        {
            Write("Escreva o tipo de curso('Ajuda' para listar os tipos): ");
            input_s = ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input_s)) { break; }
            if (input_s.Equals("Ajuda", StringComparison.OrdinalIgnoreCase))
            {
                WriteLine("Tipos de curso disponíveis:");
                foreach (var cty in Enum.GetValues<CourseType_e>()) { WriteLine($"- {cty} ({(int)cty})"); }
                continue;
            }
            if (int.TryParse(input_s, out int typeInt) && Enum.IsDefined(typeof(CourseType_e), typeInt))
            {
                type = (CourseType_e)typeInt;
                break; // Sai do loop se a conversão for bem-sucedida
            }
            else
            {
                WriteLine("Tipo de curso inválido. Tente novamente.");
            }
        }
        // --- Duração ---
        while (true)
        {
            Write("Escreva a duração do curso em anos (1 semestre= 0,5): ");
            input_s = ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input_s)) { break; }
            if (float.TryParse(input_s, out duration)) { break; } // Sai do loop se a conversão for bem-sucedida
            else { WriteLine("Duração inválida. Tente novamente."); }
        }
        // --- Confirmação final ---
        WriteLine($"\nResumo do Curso:");
        WriteLine($" Nome: {(string.IsNullOrEmpty(name) ? "<default>" : name)}");
        WriteLine($" Grau do curso:{type}");
        WriteLine($" Duração: {duration} anos");
        Write("Tem a certeza que quer criar este curso? (S/N): ");
        input_s = ReadLine()?.Trim().ToUpper();
        if (input_s != "S") return null; // Cancela criação se não confirmar


        int newID = FileManager.GetTheNextAvailableID(FileManager.DataBaseType.Course);
        if (newID == -1) { WriteLine("❌ Erro: Não foi possível obter um ID válido para o curso. Criação cancelada."); return null; }
        // --- Criação do objeto ---
        Course curso = new(name, newID, type, duration);

        FileManager.WriteOnDataBase(FileManager.DataBaseType.Course, curso, true);

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
}


internal class Discipline
{
    internal string Name_s { get; private set; }
    internal int ECTS_i { get; private set; }
    internal string Professor_s { get; private set; }
    private Dictionary<int ,Student> alunosInscritos = [];

    private Discipline(string name, int ects, string professor)
    {
        Name_s = name;
        ECTS_i = ects;
        Professor_s = professor;
    }

    public override string ToString() => $"{Name_s} ({ECTS_i} ECTS) - Prof. {Professor_s}";
}




