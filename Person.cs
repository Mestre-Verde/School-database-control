using static System.Console; // Permite usar Write e WriteLine diretamente
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

internal abstract class SchoolMembers
{
    [JsonInclude] internal protected int ID_i { get; protected set; }
    [JsonInclude] internal protected string Name_s { get; protected set; } = "";// string porque um nome é uma sequência dinâmica de caracteres
    [JsonInclude] internal protected byte Age_by { get; protected set; } = default;// byte (0-255) porque a idade nunca é negativa e não passa de 255.
    [JsonInclude] internal protected char Gender_c { get; protected set; } = default;// char 'M' ou 'F' (sempre um único caractere)
    [JsonInclude] internal protected DateTime BirthDate_dt { get; protected set; } = default;// Data de nascimento (struct DateTime) 
    [JsonInclude] internal protected Nationality_e Nationality { get; protected set; } = default;// Nacionalidade (enum)

    protected static string BuildEditMenu(string typeName)// menu para parametros
    {
        return $@"
    Editar {typeName}:
        [0] Voltar
        [1] Nome
        [2] Idade
        [3] Género
        [4] Data de nascimento
        [5] Nacionalidade
    ";
    }
    static readonly string InvalidEntrance = "Entrada inválida. Tente novamente.";
    static readonly string EmptyEntrance = "Entrada nula ou em branco, valor default utilizado.";


    protected SchoolMembers() { }// construtor para Desserialização
    // Construtor principal da classe base
    internal protected SchoolMembers(
        int id,
        string name = "",
        byte age = default,
        char gender = default,
        DateTime? birthDate = default,
        Nationality_e nationality = default)
    {
        ID_i = id;
        Name_s = name;
        Age_by = age;
        Gender_c = gender;
        BirthDate_dt = birthDate ?? DateTime.Now;
        Nationality = nationality;
    }

    internal virtual void Introduce() { WriteLine($"👤 I'm a Person named {Name_s}, {Age_by} years old."); }

    // Factory para criar objetos em subclasses
    protected static O? CreateMember<O>(
        string typeObject,
        FileManager.DataBaseType dbType,
        Func<string, byte, int, char, DateTime, Nationality_e, O> factory
    ) where O : SchoolMembers
    {
        string? input_s;
        string name = "";
        byte age = default;
        char gender = default;
        DateTime date = default;
        Nationality_e nationality = default;

        // --- Nome ---
        Write($"Escreva o nome do(a) {typeObject} (deixe vazio para default): ");
        input_s = ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(input_s)) name = input_s;

        // --- Idade ---
        while (true)
        {
            Write("Escreva a idade: ");
            input_s = ReadLine();
            if (string.IsNullOrWhiteSpace(input_s)) { WriteLine(EmptyEntrance); }
            else if (!byte.TryParse(input_s, out age)) { WriteLine(InvalidEntrance); continue; }
            break;
        }
        // --- Gênero ---
        while (true)
        {
            Write("Escreva o gênero (M/F): ");
            input_s = ReadLine()?.Trim().ToUpper();
            /* Truth table
                M | F | S|
                0   0 = 0| 
                0   1 = 1| 
                1   0 = 1| 
                1   1 = inpossivel
                */
            if (input_s == "M" || input_s == "F") { gender = input_s[0]; break; }
            else if (string.IsNullOrWhiteSpace(input_s)) { WriteLine(EmptyEntrance); break; }
            else { WriteLine(InvalidEntrance); }
        }
        // --- Data de nascimento ---
        while (true)
        {
            Write("Escreva a data de nascimento (dd-MM-yyyy): ");
            input_s = ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(input_s)) { WriteLine(EmptyEntrance); }
            else if (!DateTime.TryParseExact(input_s, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out date)) { WriteLine(InvalidEntrance); continue; }
            date = date.Date;
            break;
        }
        // --- Nacionalidade ---
        while (true)
        {
            Write("Escreva a nacionalidade ('Ajuda' para opções): ");
            input_s = ReadLine()?.Trim();
            if (string.Equals(input_s, "Ajuda", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var country in Enum.GetValues<Nationality_e>())
                    WriteLine($" - {country} ({(int)country})");
                continue;
            }
            if (string.IsNullOrWhiteSpace(input_s)) { WriteLine(EmptyEntrance); break; }
            if (int.TryParse(input_s, out int numeric) && Enum.IsDefined(typeof(Nationality_e), numeric))
            {
                nationality = (Nationality_e)numeric;
                break;
            }
            WriteLine(InvalidEntrance);
        }

        // --- Confirmação final ---
        WriteLine($"\nResumo do {typeObject}:");
        WriteLine($" Nome: {(string.IsNullOrEmpty(name) ? "<default>" : name)}");
        WriteLine($" Idade: {age}");
        WriteLine($" Gênero: {(gender == default ? "<default>" : gender.ToString())}");
        WriteLine($" Data de nascimento: {date}");
        WriteLine($" Nacionalidade: {nationality}");
        Write("Tem a certeza que quer criar? (S/N): ");
        input_s = ReadLine()?.Trim().ToUpper();
        if (input_s != "S") return null;

        // --- Criação do ID ---
        int newID = FileManager.GetTheNextAvailableID(dbType);
        if (newID == -1) { WriteLine("❌ Erro: Não foi possível obter ID."); return null; }

        var objeto = factory(name, age, newID, gender, date, nationality); // Cria o objeto usando factory

        FileManager.WriteOnDataBase(dbType, objeto); // Escreve na DB
        return objeto;
    }

    protected static void RemoveMember<M>(
        string typeName,
        FileManager.DataBaseType dbType
    ) where M : SchoolMembers
    {
        Write($"Digite o nome ou ID do {typeName} para remover: ");
        string input = ReadLine() ?? "";

        bool isId = int.TryParse(input, out int idInput);

        // Busca genérica
        var matches = isId
            ? FileManager.Search<M>(dbType, id: idInput)
            : FileManager.Search<M>(dbType, name: input);

        if (matches.Count == 0)
        {
            WriteLine($"Nenhum {typeName} encontrado.");
            return;
        }

        // Mostra as opções
        WriteLine($"Foram encontrados os seguintes {typeName}s:");
        for (int i = 0; i < matches.Count; i++)
        {
            var m = matches[i];
            WriteLine($"{i + 1}: ID={m.ID_i}, Nome='{m.Name_s}', Idade={m.Age_by}, " +
                $"Gênero={m.Gender_c}, Nascimento={m.BirthDate_dt:yyyy-MM-dd}, Nacionalidade={m.Nationality}");
        }

        Write($"Escolha os números dos {typeName}s a remover (ex: 1,2,3 ou 1 2 3): ");
        string choiceInput = ReadLine() ?? "";

        // Processa a lista de índices
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

        WriteLine($"Você selecionou os seguintes {typeName}s para remoção:");
        foreach (var idx in indices)
        {
            var m = matches[idx - 1];
            WriteLine($"- ID={m.ID_i}, Nome='{m.Name_s}', Idade={m.Age_by}, Gênero={m.Gender_c}");
        }

        Write($"Tem certeza que deseja remover todos esses {typeName}s? (S/N): ");
        string confirm = ReadLine()?.Trim().ToUpper() ?? "N";

        if (confirm != "S")
        {
            WriteLine("Operação cancelada.");
            return;
        }

        // Remove
        foreach (var idx in indices)
        {
            var m = matches[idx - 1];
            bool removed = FileManager.RemoveById<M>(dbType, m.ID_i);

            if (removed)
                WriteLine($"✅ {typeName} removido: ID={m.ID_i}, Nome='{m.Name_s}'");
            else
                WriteLine($"❌ Erro ao remover: ID={m.ID_i}, Nome='{m.Name_s}'");
        }
    }
    /*
        protected static void SelectMember<M>(
        FileManager.DataBaseType dbType,
        string typeName,
        string menuText // não vai ser mais necessário visto que o texto de menu foi alterado para a class abstrata
        ) where M : SchoolMembers
        {
            // 1) mostrar menuText aqui

            // 2) procurar pessoa (igual ao RemoveMember)
            // 3) escolher a pessoa (igual ao RemoveMember)
            // 4)o user vai selecionar uma das opções que se encontram no enum EditParamSchoolMember_e
            // 5) no final pergunta se quer emmso alterar

        }
        */

    protected static void SelectMember<M>(
      FileManager.DataBaseType dbType,
      string typeName
  ) where M : SchoolMembers
    {
        // --- procurar membro ---
        Write($"Digite o nome ou ID do {typeName} que quer selecionar: ");
        string input = ReadLine() ?? "";

        bool isId_b = int.TryParse(input, out int idInput);

        var matches = isId_b
            ? FileManager.Search<M>(dbType, id: idInput) // caso seja ID
            : FileManager.Search<M>(dbType, name: input); // caso seja nome

        if (matches.Count == 0) { WriteLine($"Nenhum {typeName} encontrado."); return; }

        // --- escolher item ---
        WriteLine($"Resultados encontrados ({matches.Count}):");
        for (int i = 0; i < matches.Count; i++)
        {
            var m = matches[i];
            WriteLine($"{i + 1}: ID={m.ID_i}, Nome={m.Name_s}, Idade={m.Age_by}, Género={m.Gender_c}, Nasc={m.BirthDate_dt:dd-MM-yyyy}");
        }

        Write($"Escolha qual deseja editar (1 - {matches.Count}): ");
        if (!int.TryParse(ReadLine(), out int choice) || choice < 1 || choice > matches.Count)
        {
            WriteLine("Escolha inválida.");
            return;
        }

        M member = matches[choice - 1];

        // --- loop do menu interno ---
        while (true)
        {
            string menuText = BuildEditMenu(typeName);
            WriteLine(menuText);


            string? cmd = ReadLine()?.Trim();
            if (!Enum.TryParse(cmd, out EditParamSchoolMember_e option))
            {
                WriteLine("Comando inválido.");
                continue;
            }

            if (option == EditParamSchoolMember_e.Back)
                break;

            switch (option)
            {
                case EditParamSchoolMember_e.Name:
                    Write("Novo nome: ");
                    string? newName = ReadLine()?.Trim();
                    if (!string.IsNullOrWhiteSpace(newName))
                        member.Name_s = newName;
                    break;

                case EditParamSchoolMember_e.Age:
                    Write("Nova idade: ");
                    if (byte.TryParse(ReadLine(), out byte newAge))
                        member.Age_by = newAge;
                    break;

                case EditParamSchoolMember_e.Gender:
                    Write("Novo gênero (M/F): ");
                    string g = (ReadLine() ?? "").Trim().ToUpper();
                    if (g == "M" || g == "F")
                        member.Gender_c = g[0];
                    break;

                case EditParamSchoolMember_e.BirthDate:
                    Write("Nova data (dd-MM-yyyy): ");
                    if (DateTime.TryParseExact(ReadLine(), "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime newDate))
                        member.BirthDate_dt = newDate;
                    break;

                case EditParamSchoolMember_e.Nationality:
                    Write("Nova nacionalidade (número): ");
                    if (int.TryParse(ReadLine(), out int natId) && Enum.IsDefined(typeof(Nationality_e), natId))
                        member.Nationality = (Nationality_e)natId;
                    break;
            }
        }

        // --- confirmar alterações ---
        Write("\nDeseja salvar as alterações? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) == "S")
        {
            //FileManager.UpdateInDataBase(dbType, member); // depois implementa a atualização específica
            WriteLine("✔️ AIndaa por implementar Alterações salvas.");
        }
        else
        {
            WriteLine("❌ Alterações descartadas.");
        }
    }



}
/*
internal class Person
{
    [JsonInclude] internal protected string Name_s { get; private set; }// string porque um nome é uma sequência dinâmica de caracteres
    [JsonInclude] internal protected byte Age_by { get; private set; }// byte (0-255) porque a idade nunca é negativa e não passa de 255.
    [JsonInclude] internal protected char Gender_c { get; private set; }// char 'M' ou 'F' (sempre um único caractere)
    [JsonInclude] internal protected DateTime BirthDate_dt { get; private set; }// Data de nascimento (struct DateTime) 
    [JsonInclude] internal protected Nationality_e Nationality { get; private set; }// Nacionalidade (enum)

    // Construtor principal da classe base
    internal protected Person(
        string name = "",
        byte age = default,
        char gender = default,
        DateTime? birthDate = default,
        Nationality_e nationality = default)
    {
        Name_s = name;
        Age_by = age;
        Gender_c = gender;
        BirthDate_dt = birthDate ?? DateTime.Now;
        Nationality = nationality;
    }

    // Método virtual — pode ser sobrescrito em subclasses
    internal virtual void Introduce() { WriteLine($"👤 I'm a Person named {Name_s}, {Age_by} years old."); }
}
*/
// Classe derivada: Student
internal class Student : SchoolMembers
{
    // Construtor parameterless obrigatório para JSON
    public Student() : base() { }

    private Student(string name, byte age, int id, char gender, DateTime birthDate, Nationality_e nat) : base(id, name, age, gender, birthDate, nationality: nat)
    {
        Introduce();
    }
    // Fábrica de objetos Student.
    internal static Student? Create() // Pode retornar null se o utilizador cancelar
    {
        return CreateMember<Student>(
            "estudante",
            FileManager.DataBaseType.Student,
            (n, a, id, g, d, nat) => new Student(n, a, id, g, d, nat)
        );
    }

    internal static void Remove() { RemoveMember<Student>("aluno", FileManager.DataBaseType.Student); }

    internal static void Select() { SelectMember<Student>(FileManager.DataBaseType.Student, "aluno"); }

    internal override void Introduce() { WriteLine($"🎓 New Student: {Name_s}, ID: {ID_i}, Age: {Age_by}, Genero: {Gender_c}, Data de nascimento: {BirthDate_dt.Date}, Nacionalidade: {Nationality}."); }
}


// Classe derivada: Teacher
internal class Teacher : SchoolMembers
{
    public Teacher() : base() { }
    private Teacher(string name, byte age, int id, char gender, DateTime birthDate, Nationality_e nat) : base(id, name, age, gender, birthDate, nationality: nat)
    {
        Introduce();
    }

    // Fábrica de objetos Teacher. Pode retornar null se o utilizador cancelar
    internal static Teacher? Create()
    {
        return CreateMember<Teacher>(
            "professor",
            FileManager.DataBaseType.Teacher,
            (n, a, id, g, d, nat) => new Teacher(n, a, id, g, d, nat));
    }

    internal static void Remove() { RemoveMember<Teacher>("professor", FileManager.DataBaseType.Teacher); }

    internal static void Select() { SelectMember<Teacher>(FileManager.DataBaseType.Teacher, "professor"); }

    internal override void Introduce() { WriteLine($"👨‍🏫 New Teacher: {Name_s}, ID: {ID_i}, Age: {Age_by}, Genero: {Gender_c}, Data de nascimento: {BirthDate_dt.Date}, Nacionalidade: {Nationality}."); }
}

