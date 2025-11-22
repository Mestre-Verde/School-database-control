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
    [JsonInclude] internal protected byte Age_by { get; protected set; }// byte (0-255) porque a idade nunca é negativa e não passa de 255.
    [JsonInclude] internal protected char Gender_c { get; protected set; }// char 'M' ou 'F' (sempre um único caractere)
    [JsonInclude] internal protected DateTime BirthDate_dt { get; protected set; }// Data de nascimento (struct DateTime) 
    [JsonInclude] internal protected Nationality_e Nationality { get; protected set; }// Nacionalidade (enum)

    protected static string BuildEditMenu(string typeName)
    {
        return $@"Editar {typeName}:
        [0] Voltar
        [1] Help
        [2] Nome
        [3] Idade
        [4] Género
        [5] Data de nascimento
        [6] Nacionalidade
    ";
    }
    static readonly string InvalidEntrance = "Entrada inválida. Tente novamente.";
    static readonly string EmptyEntrance = "Entrada nula ou em branco, valor default utilizado.";

    private const byte MinAge = 6;

    protected SchoolMembers() { }// construtor para Desserialização
    // Construtor principal da classe base
    internal protected SchoolMembers(int id, string name = "", byte age = default, char gender = default, DateTime? birthDate = default, Nationality_e nationality = default)
    {
        ID_i = id;
        Name_s = name;
        Age_by = age;
        Gender_c = gender;
        BirthDate_dt = birthDate ?? DateTime.Now;
        Nationality = nationality;
    }

    internal virtual void Introduce() { WriteLine($"👤 I'm a Person named {Name_s}, {Age_by} years old."); }

    /// <summary>
    /// Pede ao usuário para inserir ou alterar um nome.
    /// </summary>
    /// <param name="prompt">Mensagem a exibir para o usuário.</param>
    /// <param name="isToEdit">Indica se é edição (true) ou criação (false).</param>
    /// <param name="currentValue">Valor atual, caso seja edição (null se criação).</param>
    /// <returns>O nome fornecido ou o valor atual/default caso não seja alterado.</returns>
    protected static string InputName(string prompt, string? currentValue = null, bool isToEdit = false)
    {
        while (true)
        {
            if (isToEdit && !string.IsNullOrEmpty(currentValue))
                Write($"{prompt} (Enter para manter '{currentValue}'): ");
            else
                Write($"{prompt} (Enter para default): ");

            string? input = ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                WriteLine(EmptyEntrance); // mostra aviso de valor default
                                          // Se vazio, mantém valor atual em edição, ou default na criação
                return isToEdit && !string.IsNullOrEmpty(currentValue) ? currentValue : "";
            }

            // Aqui você pode adicionar validações extras (ex: tamanho mínimo, caracteres permitidos)
            return input;
        }
    }

    /// <summary>
    /// Pede ao usuário para inserir ou alterar a idade.
    /// </summary>
    /// <param name="prompt">Mensagem a exibir para o usuário.</param>
    /// <param name="currentValue">Valor atual, caso seja edição (null se criação).</param>
    /// <param name="isToEdit">Indica se é edição (true) ou criação (false).</param>
    /// <param name="minValue">Valor mínimo permitido.</param>
    /// <returns>A idade fornecida ou o valor atual caso não seja alterada.</returns>
    protected static byte InputAge(string prompt, byte? currentValue = null, bool isToEdit = false, byte minValue = MinAge, DateTime? currentBirthDate = null)
    {
        while (true)
        {
            if (isToEdit && currentValue.HasValue)
                Write($"{prompt} (Enter para manter {currentValue}): ");
            else
                Write($"{prompt} (Enter para calcular pela data de nascimento): ");

            string? input = ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                if (isToEdit && currentValue.HasValue)
                    return currentValue.Value; // mantém valor atual
                else
                    return 0; // default → será calculado a partir da data de nascimento
            }

            if (byte.TryParse(input, out byte age) && age >= minValue)
            {
                // Se houver data de nascimento atual, ajusta o ano
                if (currentBirthDate.HasValue)
                {
                    int anoAtual = DateTime.Now.Year;
                    currentBirthDate = new DateTime(anoAtual - age, currentBirthDate.Value.Month, currentBirthDate.Value.Day);
                }
                return age;
            }

            WriteLine(InvalidEntrance);
        }
    }

    /// <summary>
    /// Pede ao usuário para inserir ou alterar o gênero (M/F).
    /// </summary>
    /// <param name="prompt">Mensagem a exibir para o usuário.</param>
    /// <param name="currentValue">Valor atual, caso seja edição (null se criação).</param>
    /// <param name="isToEdit">Indica se é edição (true) ou criação (false).</param>
    /// <returns>O gênero fornecido ou valor default '\0' caso vazio.</returns>
    protected static char InputGender(string prompt, char? currentValue = null, bool isToEdit = false)
    {
        while (true)
        {
            if (isToEdit && currentValue.HasValue && currentValue != default)
                Write($"{prompt}(M/F) (Enter para manter '{currentValue}'): ");
            else
                Write($"{prompt}(M/F) (Enter para default): ");

            string? input = ReadLine()?.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(input))
            {
                WriteLine(EmptyEntrance); // mostra aviso de valor default
                                          // Se vazio, mantém valor atual em edição, ou default na criação
                return isToEdit && currentValue.HasValue ? currentValue.Value : default;
            }

            /* Truth table
                M | F | S|
                0   0 = 0| 
                0   1 = 1| 
                1   0 = 1| 
                1   1 = impossível
            */
            if (input == "M" || input == "F") return input[0];
            WriteLine(InvalidEntrance);
        }
    }

    /// <summary>
    /// Solicita ao usuário a data de nascimento de um membro, permitindo criação ou edição.
    /// </summary>
    /// <param name="prompt">Mensagem a exibir ao usuário. Se vazio, será usado um prompt padrão dependendo do contexto (criação/edição e se a idade é conhecida).</param>
    /// <param name="age">Idade do membro (opcional). Se fornecida (> 0), a função pedirá apenas o mês e o dia e calculará o ano automaticamente. Se não fornecida (0 ou default), a função pedirá a data completa (dia, mês e ano).</param>
    /// <param name="currentValue">Data atual do membro, usada quando em modo de edição para permitir manter o valor existente.Se null, assume default (DateTime.MinValue). </param>
    /// <param name="isToEdit">
    /// Indica se a função está sendo chamada em modo de edição (true) ou criação de novo objeto (false).
    /// Em edição, o usuário pode pressionar Enter para manter o valor atual.
    /// </param>
    /// <returns>
    /// Retorna um objeto <see cref="DateTime"/> representando a data de nascimento informada pelo usuário.
    /// - Se em criação e usuário não fornece entrada, retorna <see cref="DateTime.MinValue"/>.
    /// - Se em edição e usuário pressiona Enter, mantém o <paramref name="currentValue"/>.
    /// </returns>
    /// <remarks>
    /// Comportamento detalhado:
    /// - Caso <paramref name="age"/> seja fornecida (>0):
    ///     - Calcula o ano estimado como <c>anoAtual - age</c>.
    ///     - Pede apenas mês e dia.
    ///     - Se o usuário não fornecer, usa 1º de janeiro ou mantém o valor atual em edição.
    /// - Caso <paramref name="age"/> não seja fornecida (0 ou default):
    ///     - Pede a data completa (dia, mês e ano) ou Enter para default.
    ///     - Valida se a data é válida, repetindo até obter uma entrada correta.
    /// - A função ajusta a idade se não fornecida, calculando a partir do ano informado.
    /// </remarks>
    protected static DateTime InputBirthDate(string prompt, byte age = default, DateTime? currentValue = null, bool isToEdit = false)
    {
        DateTime date = currentValue ?? default;
        int anoAtual = DateTime.Now.Year;

        while (true)
        {
            int anoEstimado = (age > 0) ? anoAtual - age : 0;

            if (age == 0) // idade não fornecida
            {
                Write(prompt != "" ? prompt : "Escreva a data de nascimento (ex: 5 11 1980, 1980-11-05, ou Enter para default): ");
                string? input_s = ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input_s))
                {
                    // mantém valor atual
                    if (isToEdit && currentValue.HasValue) return currentValue.Value;
                    return default; // default ao criar
                }

                input_s = input_s.Replace(',', ' ');
                input_s = Regex.Replace(input_s, @"\s+", " ");

                if (!DateTime.TryParse(input_s, out DateTime parsedDate))
                {
                    WriteLine(InvalidEntrance);
                    continue;
                }

                date = parsedDate.Date;
            }
            else // idade fornecida → pede mês e dia
            {
                WriteLine($"Ano de nascimento estimado: {anoEstimado}");
                while (true)
                {
                    Write("Escreva o mês e o dia (ex: 12 31 ou 11,30) ou Enter para manter default: ");
                    string? input_s = ReadLine()?.Trim();

                    if (string.IsNullOrWhiteSpace(input_s))
                    {
                        // mantém valor atual em edição, ou usa 1º de janeiro
                        date = (isToEdit && currentValue.HasValue) ? currentValue.Value : new DateTime(anoEstimado, 1, 1);
                        break;
                    }

                    input_s = input_s.Replace(',', ' ');
                    input_s = Regex.Replace(input_s, @"\s+", " ");
                    string[] parts = input_s.Split(' ');

                    if (parts.Length < 2) { WriteLine("Você precisa fornecer mês e dia."); continue; }

                    if (!int.TryParse(parts[0], out int mesTmp) || mesTmp < 1 || mesTmp > 12) { WriteLine("Mês inválido."); continue; }
                    if (!int.TryParse(parts[1], out int diaTmp) || diaTmp < 1 || diaTmp > DateTime.DaysInMonth(anoEstimado, mesTmp)) { WriteLine("Dia inválido."); continue; }

                    date = new DateTime(anoEstimado, mesTmp, diaTmp);
                    break;
                }
            }
            // Ajusta idade se necessário
            if (age == 0 && date != default) { age = (byte)(anoAtual - date.Year); }
            break; // data válida obtida
        }

        return date;
    }

    /// <summary>
    /// Solicita ao usuário que informe a nacionalidade de um indivíduo.
    /// Pode ser usada tanto na criação de um novo objeto quanto na edição de um existente.
    /// Aceita entradas como número, sigla (ex: "PT") ou nome completo (ex: "Portugal"), sem diferenciar maiúsculas de minúsculas.
    /// </summary>
    /// <param name="prompt">Mensagem a exibir para o usuário antes da entrada.</param>
    /// <param name="currentValue">
    /// Valor atual da nacionalidade (usado somente se <paramref name="isToEdit"/> for true). 
    /// Caso o usuário pressione Enter, esse valor será mantido.
    /// </param>
    /// <param name="isToEdit">Indica se a função está sendo chamada para edição (true) ou criação (false).</param>
    /// <returns>
    /// O valor da nacionalidade escolhido pelo usuário como um <see cref="Nationality_e"/>.
    /// Se a entrada for vazia na criação, retorna <see cref="Nationality_e.Other"/>.
    /// </returns>
    /// <remarks>
    /// - Digitar "Ajuda" exibirá todas as opções disponíveis, incluindo números, siglas e nomes.
    /// - A entrada não diferencia maiúsculas de minúsculas.
    /// - Se a entrada não for reconhecida, será exibida a mensagem de erro <see cref="InvalidEntrance"/> e o usuário será solicitado novamente.
    /// </remarks>
    protected static Nationality_e InputNationality(string prompt, Nationality_e? currentValue = null, bool isToEdit = false)
    {
        // Dicionário case-insensitive
        var nationalityMap = new Dictionary<string, Nationality_e>(StringComparer.OrdinalIgnoreCase)
        {
            { "0", Nationality_e.Other }, { "other", Nationality_e.Other },
            { "pt", Nationality_e.PT }, { "portugal", Nationality_e.PT },
            { "es", Nationality_e.ES }, { "espanha", Nationality_e.ES },
            { "fr", Nationality_e.FR }, { "frança", Nationality_e.FR },
            { "us", Nationality_e.US }, { "estados unidos", Nationality_e.US },
            { "gb", Nationality_e.GB }, { "reino unido", Nationality_e.GB },
            { "de", Nationality_e.DE }, { "alemanha", Nationality_e.DE },
            { "it", Nationality_e.IT }, { "itália", Nationality_e.IT },
            { "br", Nationality_e.BR }, { "brasil", Nationality_e.BR },
            { "jp", Nationality_e.JP }, { "japão", Nationality_e.JP },
            { "cn", Nationality_e.CN }, { "china", Nationality_e.CN },
            { "in", Nationality_e.IN }, { "índia", Nationality_e.IN },
            { "ca", Nationality_e.CA }, { "canadá", Nationality_e.CA },
            { "au", Nationality_e.AU }, { "austrália", Nationality_e.AU },
            { "ru", Nationality_e.RU }, { "rússia", Nationality_e.RU }
        };
        while (true)
        {
            if (isToEdit && currentValue.HasValue)
                Write($"{prompt} (Enter para manter '{currentValue}'): ");
            else
                Write($"{prompt} (país ou sigla)('Ajuda' para opções): ");

            string? input = ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                if (isToEdit && currentValue.HasValue)
                    return currentValue.Value; // mantém valor atual
                else
                    return Nationality_e.Other; // valor default
            }

            if (nationalityMap.TryGetValue(input, out Nationality_e result))
                return result;

            WriteLine(InvalidEntrance);
            WriteLine("Digite 'Ajuda' para ver todas as opções.");
            if (string.Equals(input, "Ajuda", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var country in nationalityMap)
                    WriteLine($" - {country.Key} = {country.Value}");
            }
        }
    }


    // Factory para criar objetos em subclasses
    protected static M? CreateMember<M>(string typeObject,
        FileManager.DataBaseType dbType,
        Func<string, byte, int, char, DateTime, Nationality_e, M> factory) where M : SchoolMembers

    {
        string? input_s;
        string prompt;

        // --- Nome ---
        prompt = $"Escreva o nome do(a) {typeObject}";
        string name = InputName(prompt);

        // --- Idade ---
        prompt = $"Escreva a idade do {typeObject}";
        byte age = InputAge(prompt);

        // --- Gênero ---
        prompt = $"Escreva o gênero do {typeObject}";
        char gender = InputGender(prompt);

        // --- Data de nascimento ---
        prompt = "";
        DateTime date = InputBirthDate(prompt, age);

        // --- Nacionalidade ---
        prompt = $"Escreva a nacionalidade {typeObject}";
        Nationality_e nationality = InputNationality(prompt);


        // --- Confirmação final ---
        WriteLine($"\nResumo do {typeObject}:");
        WriteLine($" Nome: {(string.IsNullOrEmpty(name) ? "<default>" : name)}");
        WriteLine($" Idade: {age}");
        WriteLine($" Gênero: {(gender == default ? "<default>" : gender.ToString())}");
        WriteLine($" Data de nascimento: {date.Date}");
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
        // --- Procurar membro ---
        Write($"Digite o nome ou ID do {typeName} que quer selecionar: ");
        string? input_s = ReadLine();

        bool isId_b = int.TryParse(input_s, out int idInput);

        var matches = isId_b
            ? FileManager.Search<M>(dbType, id: idInput)
            : FileManager.Search<M>(dbType, name: input_s);

        if (matches.Count == 0)
        {
            WriteLine($"Nenhum {typeName} encontrado.");
            return;
        }

        // --- Escolher item ---
        WriteLine($"Resultados encontrados ({matches.Count}):");
        for (int i = 0; i < matches.Count; i++)
        {
            var m = matches[i];
            WriteLine($"{i + 1}: ID={m.ID_i}, Nome={m.Name_s}, Idade={m.Age_by}, Género={m.Gender_c}, Nasc={m.BirthDate_dt:dd-MM-yyyy}");
        }

        Write($"Escolha qual deseja editar (1 - {matches.Count}): ");
        if (!int.TryParse(ReadLine(), out int choice) || choice < 1 || choice > matches.Count)
        {
            WriteLine("Entrada inválida.");
            return;
        }

        M member = matches[choice - 1];

        // --- Guardar cópia dos valores originais ---
        var originalParameters = new
        {
            member.Name_s,
            member.Age_by,
            member.Gender_c,
            member.BirthDate_dt,
            member.Nationality
        };
        bool hasChanged = false;

        // --- Loop do menu interno ---
        WriteLine(BuildEditMenu(typeName));
        while (true)
        {

            Write("(edit menu)> ");
            input_s = ReadLine()?.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(input_s)) continue;

            // Converter números → nomes do enum
            switch (input_s)
            {
                case "0": input_s = "Back"; break;
                case "1": input_s = "Help"; break;
                case "2": input_s = "Name"; break;
                case "3": input_s = "Age"; break;
                case "4": input_s = "Gender"; break;
                case "5": input_s = "BirthDate"; break;
                case "6": input_s = "Nationality"; break;
            }

            if (!Enum.TryParse(input_s, true, out EditParamSchoolMember_e option))
            {
                WriteLine("Comando inválido.");
                continue;
            }

            if (option == EditParamSchoolMember_e.Back) break;

            // ===== Manipulação de opções =====
            switch (option)
            {
                case EditParamSchoolMember_e.Name:
                    Write("Novo nome: ");
                    string? newName = ReadLine()?.Trim();
                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        member.Name_s = newName;
                        hasChanged = true;
                    }
                    break;

                case EditParamSchoolMember_e.Age:
                    Write("Nova idade: ");
                    if (byte.TryParse(ReadLine(), out byte newAge))
                    {
                        member.Age_by = newAge;
                        hasChanged = true;
                    }
                    break;

                case EditParamSchoolMember_e.Gender:
                    Write("Novo gênero (M/F): ");
                    string g = (ReadLine() ?? "").Trim().ToUpper();
                    if (g == "M" || g == "F")
                    {
                        member.Gender_c = g[0];
                        hasChanged = true;
                    }
                    break;

                case EditParamSchoolMember_e.BirthDate:
                    Write("Nova data (dd-MM-yyyy): ");
                    if (DateTime.TryParseExact(ReadLine(), "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime newDate))
                    {
                        member.BirthDate_dt = newDate;
                        hasChanged = true;
                    }
                    break;

                case EditParamSchoolMember_e.Nationality:
                    Write("Nova nacionalidade (número): ");
                    if (int.TryParse(ReadLine(), out int natId) && Enum.IsDefined(typeof(Nationality_e), natId))
                    {
                        member.Nationality = (Nationality_e)natId;
                        hasChanged = true;
                    }
                    break;

                case EditParamSchoolMember_e.Help:
                    WriteLine(BuildEditMenu(typeName));
                    WriteLine("\n--- Dados originais ---");
                    WriteLine($"ID: {member.ID_i}");
                    WriteLine($"Nome: {originalParameters.Name_s}");
                    WriteLine($"Idade: {originalParameters.Age_by}");
                    WriteLine($"Género: {originalParameters.Gender_c}");
                    WriteLine($"Nascimento: {originalParameters.BirthDate_dt:dd-MM-yyyy}");
                    WriteLine($"Nacionalidade: {originalParameters.Nationality}");
                    break;
            }
        }

        // --- Confirmar alterações apenas se houve modificações ---
        if (!hasChanged) return;

        Write("\nDeseja salvar as alterações? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) == "S")
        {
            //FileManager.UpdateInDataBase(dbType, member);
            WriteLine("✔️ Alterações salvas (por implementar).");
        }
        else
        {
            WriteLine("❌ Alterações descartadas.");
            // reverter para valores originais (opcional)
            member.Name_s = originalParameters.Name_s;
            member.Age_by = originalParameters.Age_by;
            member.Gender_c = originalParameters.Gender_c;
            member.BirthDate_dt = originalParameters.BirthDate_dt;
            member.Nationality = originalParameters.Nationality;
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

/* perfect but not modular
        while (true)
        {
            int anoEstimado = (age > 0) ? anoAtual - age : 0;

            if (age == default) // idade não foi fornecida
            {
                Write("Escreva a data de nascimento (ex: 5 11 1980, 1980-11-05, ou Enter para default): ");
                input_s = ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input_s)) break; // vazio → usa default

                // Remove múltiplos espaços e vírgulas
                input_s = input_s.Replace(',', ' ');
                input_s = Regex.Replace(input_s, @"\s+", " ");

                if (!DateTime.TryParse(input_s, out DateTime parsedDate))
                {
                    WriteLine(InvalidEntrance);
                    continue; // força a digitar novamente
                }

                date = parsedDate.Date;
            }
            else // idade fornecida → pede mês e dia
            {
                WriteLine($"Ano de nascimento de acordo com idade: {anoEstimado}");

                while (true)
                {
                    Write("Escreva o mês e o dia (ex: 12 31 ou 11,30) ou Enter para manter default: ");
                    input_s = ReadLine()?.Trim();
                    // Usuário não forneceu mês/dia → cria data temporária com 1º de janeiro
                    if (string.IsNullOrWhiteSpace(input_s)) { date = new DateTime(anoEstimado, 1, 1); break; }
                    input_s = input_s.Replace(',', ' ');
                    input_s = Regex.Replace(input_s, @"\s+", " ");
                    string[] parts = input_s.Split(' ');

                    if (parts.Length < 2) { WriteLine("Você precisa fornecer mês e dia."); continue; }

                    if (!int.TryParse(parts[0], out int mesTmp) || mesTmp < 1 || mesTmp > 12) { WriteLine("Mês inválido."); continue; }
                    if (!int.TryParse(parts[1], out int diaTmp) || diaTmp < 1 || diaTmp > DateTime.DaysInMonth(anoEstimado, mesTmp)) { WriteLine("Dia inválido."); continue; }

                    mes = mesTmp;
                    dia = diaTmp;
                    date = new DateTime(anoEstimado, mes, dia);
                    break;
                }
            }

            // Calcula idade se não fornecida
            if (age == default)
            {
                int idadeCalculada = anoAtual - date.Year;
                if (idadeCalculada < MinAge) { WriteLine($"Erro: idade calculada ({idadeCalculada}) menor que {MinAge} anos."); continue; }
                age = (byte)idadeCalculada;
            }
            else
            {
                // valida consistência idade × data
                int idadeCalculada = anoAtual - date.Year;
                if (idadeCalculada != age)
                {
                    WriteLine($"Erro: idade ({age}) e data de nascimento ({date.ToShortDateString()}) não coincidem. Tente novamente.");
                    continue;
                }
            }
            break; // data válida obtida
        }
*/