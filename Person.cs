using static System.Console; // Permite usar Write e WriteLine diretamente
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

internal abstract class SchoolMembers
{
    [JsonInclude] internal protected string Name_s { get; private set; } = "";// string porque um nome é uma sequência dinâmica de caracteres
    [JsonInclude] internal protected byte Age_by { get; private set; } = default;// byte (0-255) porque a idade nunca é negativa e não passa de 255.
    [JsonInclude] internal protected char Gender_c { get; private set; } = default;// char 'M' ou 'F' (sempre um único caractere)
    [JsonInclude] internal protected DateTime BirthDate_dt { get; private set; } = default;// Data de nascimento (struct DateTime) 
    [JsonInclude] internal protected Nationality_e Nationality { get; private set; } = default;// Nacionalidade (enum)

    static readonly string InvalidEntrance = "Entrada inválida. Tente novamente.";
    static readonly string EmptyEntrance = "Entrada nula ou em branco, valor default utilizado.";

    protected SchoolMembers() { }
    // Construtor principal da classe base
    internal protected SchoolMembers(
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

    protected static void Remove(string typeObject, FileManager.DataBaseType dbType,object obj) 
    {
        Write("Digite o nome ou ID do aluno para remover: ");
        string input = ReadLine() ?? "";
        

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
    [JsonInclude] internal int ID_i { get; private set; }

    // Construtor parameterless obrigatório para JSON
    public Student() : base() { }

    private Student(string name, byte age, int id, char gender, DateTime birthDate, Nationality_e nat) : base(name, age, gender, birthDate, nationality: nat)
    {
        ID_i = id;
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

    internal static void Remove()
    {
        Write("Digite o nome ou ID do aluno para remover: ");
        string input = ReadLine() ?? "";

        bool isId = int.TryParse(input, out int idInput);
        var dbType = FileManager.DataBaseType.Student;

        // Busca usando enum
        var matches = isId ? FileManager.Search<Student>(dbType, id: idInput) : FileManager.Search<Student>(dbType, name: input);

        if (matches.Count == 0)
        {
            WriteLine("Nenhum aluno encontrado.");
            return;
        }

        // Mostra todos os matches com detalhes
        WriteLine("Foram encontrados os seguintes alunos:");
        for (int i = 0; i < matches.Count; i++)
        {
            var s = matches[i];
            WriteLine($"{i + 1}: ID={s.ID_i}, Nome='{s.Name_s}', Idade={s.Age_by}, Gênero={s.Gender_c}, Nascimento={s.BirthDate_dt:yyyy-MM-dd}, Nacionalidade={s.Nationality}");
        }

        Write("Escolha os números dos alunos a remover (ex: 1,2,3 ou 1 2 3): ");
        string choiceInput = ReadLine() ?? "";

        // Divide a string por vírgula ou espaço
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

        // Confirmação global
        WriteLine("Você selecionou os seguintes alunos para remoção:");
        foreach (var idx in indices)
        {
            var s = matches[idx - 1];
            WriteLine($"- ID={s.ID_i}, Nome='{s.Name_s}', Idade={s.Age_by}, Gênero={s.Gender_c}");
        }
        Write("Tem certeza que deseja remover todos esses alunos? (S/N): ");
        string confirm = ReadLine()?.Trim().ToUpper() ?? "N"; // Se estiver vazio, passa a ser "N". 

        if (confirm != "S") { WriteLine("Operação cancelada."); return; }

        // Remove todos selecionados
        foreach (var idx in indices)
        {
            var s = matches[idx - 1];
            bool removed = FileManager.RemoveById<Student>(dbType, s.ID_i);
            if (removed) WriteLine($"✅ Aluno removido: ID={s.ID_i}, Nome='{s.Name_s}'");
            else WriteLine($"❌ Erro ao remover: ID={s.ID_i}, Nome='{s.Name_s}'");
        }
    }

    internal static void Select()
    {
        // seleciona um estudante e professor, e manuseia os dados
    }

    internal override void Introduce()
    { WriteLine($"🎓 New Student: {Name_s}, ID: {ID_i}, Age: {Age_by}, Genero: {Gender_c}, Data de nascimento: {BirthDate_dt.Date}, Nacionalidade: {Nationality}."); }
}


// Classe derivada: Teacher
internal class Teacher : SchoolMembers
{
    [JsonInclude] internal int ID_i { get; private set; }

    public Teacher() : base() { }
    private Teacher(string name, byte age, int id, char gender, DateTime birthDate, Nationality_e nat) : base(name, age, gender, birthDate, nationality: nat)
    {
        ID_i = id;
        Introduce();
    }

    // Fábrica de objetos Teacher.
    internal static Teacher? Create() // Pode retornar null se o utilizador cancelar
    {
        return CreateMember<Teacher>("professor", FileManager.DataBaseType.Teacher, (n, a, id, g, d, nat) => new Teacher(n, a, id, g, d, nat));
    }
    /*
    internal static Teacher? Create()
    {
        //WriteLine("DEBUG: Inside of Teacher.Create()");

        string? input_s;
        string name = "";                  // Valor default: vazio
        byte age = default;                // Valor default: 0
        char gender = default;             // Valor default: '\0'
        DateTime date = default;           // Valor default: 01/01/0001
        Nationality_e nationality = default; // Valor default: Other (0)

        // --- Nome ---
        Write("Escreva o nome do professor(a) (deixe vazio para default): ");
        input_s = ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(input_s)) name = input_s;

        // --- Idade ---
        Write("Escreva a idade do professor(a): ");
        input_s = ReadLine();
        if (!string.IsNullOrWhiteSpace(input_s) && !byte.TryParse(input_s, out age))
        {
            WriteLine($"DEBUG: Falha ao converter idade: \"{input_s}\" não é número válido. Usando default 0.");
        }

        // --- Gênero ---
        Write("Escreva o seu género (M/F): ");
        input_s = ReadLine()?.Trim().ToUpper();
        if (input_s == "M" || input_s == "F") gender = input_s[0];

        // --- Data de nascimento ---
        Write("Escreva a data de nascimento (dd-MM-yyyy): ");
        input_s = ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(input_s) &&
            !DateTime.TryParseExact(input_s, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out date))
        {
            WriteLine($"DEBUG: Falha ao converter birthdate: \"{input_s}\". Usando valor default.");
        }
        date = date.Date; // remove a parte do tempo, mantendo só a data

        // --- Nacionalidade ---
        while (true)
        {
            Write("Escreva a sua nacionalidade ('Ajuda' para opções): ");
            input_s = ReadLine()?.Trim();

            if (string.Equals(input_s, "Ajuda", StringComparison.OrdinalIgnoreCase))
            {
                WriteLine("Opções possíveis:");
                foreach (var country in Enum.GetValues<Nationality_e>())
                    WriteLine($" - {country} ({(int)country})");
                continue;
            }

            if (string.IsNullOrWhiteSpace(input_s)) break; // usa default Other

            if (int.TryParse(input_s, out int numeric) && Enum.IsDefined(typeof(Nationality_e), numeric))
            {
                nationality = (Nationality_e)numeric;
                break;
            }

            WriteLine("Entrada inválida. Tente novamente.");
        }

        // --- Confirmação final ---
        WriteLine($"\nResumo do professor:");
        WriteLine($" Nome: {(string.IsNullOrEmpty(name) ? "<default>" : name)}");
        WriteLine($" Idade: {age}");
        WriteLine($" Gênero: {(gender == default ? "<default>" : gender.ToString())}");
        WriteLine($" Data de nascimento: {date}");
        WriteLine($" Nacionalidade: {nationality}");
        Write("Tem a certeza que quer criar este professor? (S/N): ");
        input_s = ReadLine()?.Trim().ToUpper();
        if (input_s != "S") return null; // Cancela criação

        // --- Criação do objeto ---
        if (!FileManager.StartupCheckFilesWithProgress(false)) { return null; } // Verifica se os ficheiros essenciais existem

        int newID = FileManager.GetTheNextAvailableID(FileManager.DataBaseType.Teacher);
        if (newID == -1) { WriteLine("❌ Erro: Não foi possível obter um ID válido para o curso. Criação cancelada."); return null; }
        Teacher teacher = new(name, age, newID, gender, date, nationality);

        // --- Escrever no banco de dados ---
        FileManager.WriteOnDataBase(FileManager.DataBaseType.Teacher, teacher);

        return teacher; // Retorna o objeto criado
    }
*/
    internal static void Remove()
    {
        Write("Digite o nome ou ID do professor para remover: ");
        string input = ReadLine() ?? "";

        bool isId = int.TryParse(input, out int idInput);
        var dbType = FileManager.DataBaseType.Teacher;

        // Busca usando enum
        var matches = isId ? FileManager.Search<Teacher>(dbType, id: idInput) : FileManager.Search<Teacher>(dbType, name: input);

        if (matches.Count == 0)
        {
            WriteLine("Nenhum professor encontrado.");
            return;
        }

        // Mostra todos os matches com detalhes
        WriteLine("Foram encontrados os seguintes professores:");
        for (int i = 0; i < matches.Count; i++)
        {
            var t = matches[i];
            WriteLine($"{i + 1}: ID={t.ID_i}, Nome='{t.Name_s}', Idade={t.Age_by}, Gênero={t.Gender_c}, Nascimento={t.BirthDate_dt:yyyy-MM-dd}, Nacionalidade={t.Nationality}");
        }

        Write("Escolha os números dos professores a remover (ex: 1,2,3 ou 1 2 3): ");
        string choiceInput = ReadLine() ?? "";

        // Divide a string por vírgula ou espaço
        var indices = choiceInput
            .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out int x) ? x : -1)
            .Where(x => x >= 1 && x <= matches.Count)
            .Distinct()
            .ToList();

        if (indices.Count == 0)
        {
            WriteLine("Nenhuma seleção válida. Operação cancelada.");
            return;
        }

        // Confirmação global
        WriteLine("Você selecionou os seguintes professores para remoção:");
        foreach (var idx in indices)
        {
            var t = matches[idx - 1];
            WriteLine($"- ID={t.ID_i}, Nome='{t.Name_s}', Idade={t.Age_by}, Gênero={t.Gender_c}");
        }
        Write("Tem certeza que deseja remover todos esses professores? (S/N): ");
        string confirm = ReadLine()?.Trim().ToUpper() ?? "N"; // Se estiver vazio, passa a ser "N".

        if (confirm != "S") { WriteLine("Operação cancelada."); return; }

        // Remove todos selecionados
        foreach (var idx in indices)
        {
            var t = matches[idx - 1];
            bool removed = FileManager.RemoveById<Teacher>(dbType, t.ID_i);
            if (removed) WriteLine($"✅ Professor removido: ID={t.ID_i}, Nome='{t.Name_s}'");
            else WriteLine($"❌ Erro ao remover: ID={t.ID_i}, Nome='{t.Name_s}'");
        }
    }

    internal static void Select()
    {
        // seleciona um estudante e professor, e manuseia os dados
    }

    internal override void Introduce() { WriteLine($"👨‍🏫 New Teacher: {Name_s}, ID: {ID_i}, Age: {Age_by}, Genero: {Gender_c}, Data de nascimento: {BirthDate_dt.Date}, Nacionalidade: {Nationality}."); }
}
