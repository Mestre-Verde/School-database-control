using static System.Console;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Data.Common;

internal class Subjects : BaseEntity
{
    [JsonInclude] internal short ECTS_i { get; private set; }
    [JsonInclude] internal List<Teacher> Professor_l { get; private set; } = [];
    [JsonInclude] internal List<Student> Students_l { get; private set; } = [];
    [JsonIgnore] private const short MinEct = 3;
    internal override string Describe()
    {
        return $"ID={ID_i}, Nome='{Name_s}', ECTS={ECTS_i}, Professores={Professor_l.Count}, Alunos={Students_l.Count}";
    }


    // construtor para desserialização
    public Subjects() : base(0, "") { }
    // construtor principal
    internal Subjects(int id, short ects, string name = "") : base(id, name)
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

    private static void ManageTeachers(Subjects disc)
    {
        WriteLine("\n--- Gestão de Professores ---");
        WriteLine("Função ainda não implementada.");
        // aqui vamos criar menu: adicionar/remover professor
    }

    private static void ManageStudents(Subjects disc)
    {
        WriteLine("\n--- Gestão de Alunos ---");
        WriteLine("Função ainda não implementada.");
        // aqui vamos criar menu: adicionar/remover aluno
    }

    internal override BaseEntity? CreateInstance() => Create();
    internal static Subjects? Create()
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
        int newID = FileManager.GetTheNextAvailableID(FileManager.DataBaseType.Subjects);
        if (newID == -1) { WriteLine(ProblemGetTheId); return null; }

        // --- Criar objeto ---
        Subjects disc = new(newID, ects, name);

        // Guardar na base de dados
        FileManager.WriteOnDataBase(FileManager.DataBaseType.Subjects, disc);

        return disc;
    }

    internal static void Remove() { RemoveEntity<Subjects>("Disciplina", FileManager.DataBaseType.Subjects); }

    internal static void Select()
    {
        // --- Procurar disciplina ---
        Write("Digite o nome ou ID da disciplina que quer selecionar: ");
        string? input = ReadLine();

        bool isId = int.TryParse(input, out int idInput);
        var dbType = FileManager.DataBaseType.Subjects;

        var matches = isId
            ? FileManager.Search<Subjects>(dbType, id: idInput)
            : FileManager.Search<Subjects>(dbType, name: input);

        if (matches.Count == 0) { WriteLine("Nenhuma disciplina encontrada."); return; }

        // --- Escolher disciplina ---
        WriteLine($"\nResultados encontrados ({matches.Count}):");
        for (int i = 0; i < matches.Count; i++)
        {
            var d = matches[i];
            WriteLine($"{i + 1}: ID={d.ID_i}, Nome='{d.Name_s}', ECTS={d.ECTS_i}, Professores={d.Professor_l.Count}, Alunos={d.Students_l.Count}");
        }

        Write($"Escolha qual deseja editar (1 - {matches.Count}): ");
        if (!int.TryParse(ReadLine(), out int choice) || choice < 1 || choice > matches.Count)
        {
            WriteLine("Entrada inválida.");
            return;
        }

        Subjects disc = matches[choice - 1];

        // --- Guardar original para possível rollback ---
        var original = new
        {
            disc.Name_s,
            disc.ECTS_i,
            Teachers = new List<Teacher>(disc.Professor_l),
            Students = new List<Student>(disc.Students_l)
        };

        bool changed = false;

        // --- Mostrar menu interno ---
        WriteLine(MenuRelated_cl.BuildEditSubjectsMenu(original.Name_s));

        while (true)
        {
            EditParamSubjects_e option = MenuRelated_cl.MenuSubjectsParameters(original.Name_s);

            if (option == EditParamSubjects_e.Back)
                break;

            switch (option)
            {
                case EditParamSubjects_e.Help:
                    WriteLine("\n--- Dados atuais ---");
                    WriteLine($"ID: {disc.ID_i}");
                    WriteLine($"Nome: {disc.Name_s}");
                    WriteLine($"ECTS: {disc.ECTS_i}");
                    WriteLine($"Professores: {disc.Professor_l.Count}");
                    WriteLine($"Alunos: {disc.Students_l.Count}");
                    break;

                case EditParamSubjects_e.Name:
                    disc.Name_s = InputName("Escreva o nome da disciplina", disc.Name_s, true);
                    changed = true;
                    break;

                case EditParamSubjects_e.ECTS:
                    disc.ECTS_i = InputSubjectsECTS("Escreva os ECTS", disc.ECTS_i, true);
                    changed = true;
                    break;

                case EditParamSubjects_e.ManageTeachers:
                    ManageTeachers(disc);
                    changed = true;
                    break;

                case EditParamSubjects_e.ManageStudents:
                    ManageStudents(disc);
                    changed = true;
                    break;
            }
        }

        if (!changed) return;

        // --- Confirmar alterações ---
        Write("\nDeseja salvar as alterações? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) == "S")
        {
            FileManager.WriteOnDataBase(dbType, disc);
            WriteLine("✔️ Alterações salvas.");
        }
        else
        {
            // rollback
            disc.Name_s = original.Name_s;
            disc.ECTS_i = original.ECTS_i;

            disc.Professor_l.Clear();
            disc.Professor_l.AddRange(original.Teachers);

            disc.Students_l.Clear();
            disc.Students_l.AddRange(original.Students);

            WriteLine("❌ Alterações descartadas.");
        }
    }



}