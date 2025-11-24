using static System.Console; // Permite usar Write e WriteLine diretamente
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

internal abstract class SchoolMembers : BaseEntity
{
    [JsonInclude] internal protected byte Age_by { get; protected set; }// byte (0-255) porque a idade nunca é negativa e não passa de 255.
    [JsonInclude] internal protected char Gender_c { get; protected set; }// char 'M' ou 'F' (sempre um único caractere)
    [JsonInclude] internal protected DateTime BirthDate_dt { get; protected set; }// Data de nascimento (struct DateTime) 
    [JsonInclude] internal protected Nationality_e Nationality { get; protected set; }// Nacionalidade (enum) incorpurado para todos os tipos
    [JsonInclude] internal protected string email_s { get; private set; } = "";
    [JsonIgnore] private const byte MinAge = 6;
    internal override string Describe()
    {
        return $"ID={ID_i}, Nome='{Name_s}', Idade={Age_by}, Gênero={Gender_c},Nascimento={BirthDate_dt:yyyy-MM-dd}, Nacionalidade={Nationality}, Email={email_s}";
    }

    // construtor para desserialização
    protected SchoolMembers() : base(0, "") { }
    // Construtor principal da classe base
    internal protected SchoolMembers(int id, string name = "", byte age = default, char gender = default, DateTime? birthDate = default, Nationality_e nationality = default) : base(id, name)
    {
        Age_by = age;
        Gender_c = gender;
        BirthDate_dt = birthDate ?? DateTime.Now;
        Nationality = nationality;
    }

    //----------------------------------
    // funções para mudança de Atributos
    //----------------------------------

    /// <summary>
    /// Pede ao usuário para inserir ou alterar a idade.
    /// </summary>
    /// <param name="prompt">Mensagem a exibir para o usuário.</param>
    /// <param name="currentValue">Valor atual, caso seja edição (null se criação).</param>
    /// <param name="isToEdit">Indica se é edição (true) ou criação (false).</param>
    /// <param name="minValue">Valor mínimo permitido.</param>
    /// <returns>A idade fornecida ou o valor atual caso não seja alterada.</returns>
    protected static byte InputAge(string prompt, ref DateTime? currentBirthDate, byte? currentValue = null, bool isToEdit = false, byte minValue = MinAge)
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
                WriteLine(EmptyEntrance);
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

            /* Truth table(Or)
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
    protected static DateTime InputBirthDate(string prompt, ref byte age, DateTime? currentValue = null, bool isToEdit = false)
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
                    WriteLine(EmptyEntrance);
                    if (isToEdit && currentValue.HasValue) return currentValue.Value;// mantém valor atual
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
                        date = (isToEdit && currentValue.HasValue) ? currentValue.Value : new DateTime(anoEstimado, 1, 1);
                        break;
                    }

                    input_s = input_s.Replace(',', ' ');
                    input_s = Regex.Replace(input_s, @"\s+", " ");
                    string[] parts = input_s.Split(' ');

                    if (parts.Length < 2) { WriteLine(BaseEntity.InvalidEntrance); continue; }

                    if (!int.TryParse(parts[0], out int mesTmp) || mesTmp < 1 || mesTmp > 12) { WriteLine(BaseEntity.InvalidEntrance); continue; }
                    if (!int.TryParse(parts[1], out int diaTmp) || diaTmp < 1 || diaTmp > DateTime.DaysInMonth(anoEstimado, mesTmp)) { WriteLine(BaseEntity.InvalidEntrance); continue; }

                    date = new DateTime(anoEstimado, mesTmp, diaTmp);
                    break;
                }
            }

            // Ajusta idade se necessário
            if (age == 0 && date != default) age = (byte)(anoAtual - date.Year);
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
                Write(EmptyEntrance);
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

    /// <summary>
    /// Solicita ao usuário para inserir ou alterar o email.
    /// </summary>
    /// <param name="prompt">Mensagem a exibir para o usuário.</param>
    /// <param name="currentValue">Valor atual do email (usado apenas em edição).</param>
    /// <param name="isToEdit">Indica se é edição (true) ou criação (false).</param>
    /// <returns>Email válido como string.</returns>
    protected static string InputEmail(string prompt, string? currentValue = null, bool isToEdit = false)
    {
        while (true)
        {
            if (isToEdit && !string.IsNullOrEmpty(currentValue))
                Write($"{prompt} (Enter para manter '{currentValue}'): ");
            else
                Write($"{prompt} (Enter para default vazio): ");

            string? input = ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                WriteLine(EmptyEntrance);
                if (isToEdit && !string.IsNullOrEmpty(currentValue))
                    return currentValue; // mantém valor atual
                return ""; // valor default vazio
            }

            // Validação simples de email
            if (!Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                WriteLine("❌ Email inválido. Certifique-se de usar o formato correto (ex: nome@dominio.com).");
                continue;
            }

            return input;
        }
    }

    //----------------------------------
    // funções Globais
    //----------------------------------

    // Factory para criar objetos em subclasses
    protected static M? CreateMember<M>(string typeObject, FileManager.DataBaseType dbType, Action<Dictionary<string, object>> collectSpecificFields, Func<Dictionary<string, object>, M> factory) where M : BaseEntity
    {
        var parameters = new Dictionary<string, object>
        {
            // ---------- CAMPOS COMUNS ----------
            ["Name"] = InputName($"Escreva o nome do(a) {typeObject}")
        };

        DateTime? trash = null;
        parameters["Age"] = InputAge($"Escreva a idade do(a) {typeObject}", ref trash);
        byte age = (byte)parameters["Age"];

        parameters["Gender"] = InputGender($"Escreva o gênero do(a) {typeObject}");

        parameters["BirthDate"] = InputBirthDate("", ref age);

        parameters["Nationality"] = InputNationality($"Escreva a nacionalidade {typeObject}");

        parameters["Email"] = InputEmail($"Escreva o email do(a) {typeObject}");

        // ---------- CAMPOS ESPECÍFICOS ----------
        collectSpecificFields(parameters);

        // ---------- RESUMO FINAL ----------
        WriteLine($"\nResumo do {typeObject}:");
        foreach (var kv in parameters)
            WriteLine($" {kv.Key}: {kv.Value}");

        Write("Tem a certeza que quer criar? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) != "S") return null;

        // ---------- CRIA ID ----------
        int newID = FileManager.GetTheNextAvailableID(dbType);
        if (newID == -1) { WriteLine(ProblemGetTheId); return null; }

        parameters["ID"] = newID;

        // ---------- CRIA OBJETO ----------
        var objeto = factory(parameters);

        FileManager.WriteOnDataBase(dbType, objeto);
        return objeto;
    }

    protected static void SelectMember<M>(string typeName, FileManager.DataBaseType dbType) where M : SchoolMembers
    {
        // --- Procurar membro ---
        Write($"Digite o nome ou ID do {typeName} que quer selecionar: ");
        string? input_s = ReadLine();

        bool isId_b = int.TryParse(input_s, out int idInput);
        var matches = isId_b
            ? FileManager.Search<M>(dbType, id: idInput)
            : FileManager.Search<M>(dbType, name: input_s);

        if (matches.Count == 0) { WriteLine($"Nenhum {typeName} encontrado."); return; }
        // --- Escolher item ---
        WriteLine($"Resultados encontrados ({matches.Count}):");
        for (int i = 0; i < matches.Count; i++)
        {
            var m = matches[i];
            WriteLine($"{i + 1}: ID={m.ID_i}, Nome={m.Name_s}, Idade={m.Age_by}, Género={m.Gender_c}, Nasc={m.BirthDate_dt:dd-MM-yyyy}, Nacio={m.Nationality}");
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
        WriteLine(MenuRelated_cl.BuildEditMenu(typeName));
        while (true)
        {
            EditParamSchoolMember_e option = MenuRelated_cl.MenuSchoolMembersParameters(typeName);

            if (option == EditParamSchoolMember_e.Back) break;

            switch (option)
            {
                case EditParamSchoolMember_e.Name:
                    member.Name_s = InputName($"Escreva o nome do(a) {typeName}", member.Name_s, true);
                    hasChanged = true;
                    break;

                case EditParamSchoolMember_e.Age:
                    DateTime? temporary = member.BirthDate_dt;
                    member.Age_by = InputAge($"Escreva a idade do {typeName}", ref temporary, member.Age_by, true, MinAge);
                    if (temporary.HasValue) member.BirthDate_dt = temporary.Value;
                    hasChanged = true;
                    break;

                case EditParamSchoolMember_e.Gender:
                    member.Gender_c = InputGender($"Escreva o gênero do(a) {typeName}", member.Gender_c, true);
                    hasChanged = true;
                    break;

                case EditParamSchoolMember_e.BirthDate:
                    byte tempAge = member.Age_by; // variável local
                    member.BirthDate_dt = InputBirthDate($"Escreva a data de nascimento do(a) {typeName}", ref tempAge, member.BirthDate_dt, true); member.Age_by = tempAge; // atualiza a propriedade
                    hasChanged = true;
                    break;


                case EditParamSchoolMember_e.Nationality:
                    member.Nationality = InputNationality($"Escreva a nacionalidade do(a) {typeName}", member.Nationality, true);
                    hasChanged = true;
                    break;

                case EditParamSchoolMember_e.Help:
                    WriteLine("\n--- Dados atuais ---");
                    WriteLine($"ID: {member.ID_i}");
                    WriteLine($"Nome: {member.Name_s}");
                    WriteLine($"Idade: {member.Age_by}");
                    WriteLine($"Género: {member.Gender_c}");
                    WriteLine($"Nascimento: {member.BirthDate_dt:dd-MM-yyyy}");
                    WriteLine($"Nacionalidade: {member.Nationality}");
                    break;
            }
        }
        // --- Confirmar alterações apenas se houve modificações ---
        if (!hasChanged) return;

        Write("\nDeseja salvar as alterações? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) == "S")
        {
            FileManager.WriteOnDataBase(dbType, member);  // <-- SALVA
            WriteLine("✔️ Alterações salvas.");
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

internal class Student : SchoolMembers
{
    [JsonInclude] internal int TutorId_i { get; private set; } = -1;
    [JsonInclude] internal List<double> Grades_i { get; private set; } = [];// Lista de notas
    [JsonIgnore] internal decimal GPA_d = default;//GPA = média das notas. vai ser calculado em ls não precisa de ser guardado
    internal override string Describe()
    {
        return $"ID={ID_i}, Nome='{Name_s}', Idade={Age_by}, Gênero={Gender_c},Nascimento={BirthDate_dt:yyyy-MM-dd}, Nacionalidade={Nationality}, Email={email_s}, Tutor:{TutorId_i}.";
    }
    // Construtor parameterless obrigatório para JSON
    public Student() : base() { }

    protected Student(string name, byte age, int id, char gender, DateTime birthDate, Nationality_e nat, int tutorId_i) : base(id, name, age, gender, birthDate, nat)
    {
        TutorId_i = tutorId_i;
        Introduce();
    }

    //----------------------------------
    // funções para mudança de Atributos
    //----------------------------------

    /// <summary>
    /// Solicita ao usuário que informe ou edite o ID do tutor.
    /// </summary>
    /// <param name="prompt">Mensagem a exibir ao usuário.</param>
    /// <param name="currentValue">Valor atual do TutorId_i, usado em edição.</param>
    /// <param name="isToEdit">Indica se é edição (true) ou criação (false).</param>
    /// <returns>O ID do tutor fornecido pelo usuário ou o valor atual/default se Enter for pressionado.</returns>
    protected static int InputTutorId(string prompt, int? currentValue = null, bool isToEdit = false)
    // depois de deixares a função select super modular (colocar na class de 1º grau) colocar aqui para uma melhor seleção dos professores.
    {
        while (true)
        {
            if (isToEdit && currentValue.HasValue && currentValue.Value != -1) Write($"{prompt} (Enter para manter '{currentValue.Value}'): ");
            else Write($"{prompt} (Enter para default '-1'): ");

            string? input = ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                // Mantém valor atual em edição ou default na criação
                return isToEdit && currentValue.HasValue ? currentValue.Value : -1;
            }

            if (int.TryParse(input, out int id) && id >= 0)
                return id;

            WriteLine(InvalidEntrance);
        }
    }

    // Fábrica de objetos Student. Pode retornar null se o utilizador cancelar
    internal override BaseEntity? CreateInstance() => Create();

    internal static Student? Create()
    {
        return CreateMember("estudante", FileManager.DataBaseType.Student,
            dict =>
            {
                dict["TutorId_i"] = InputTutorId("ID do tutor");
            },
            dict => new Student(
                (string)dict["Name"],
                (byte)dict["Age"],
                (int)dict["ID"],
                (char)dict["Gender"],
                (DateTime)dict["BirthDate"],
                (Nationality_e)dict["Nationality"],
                (int)dict["TutorId_i"]
            )
        );
    }

    internal static void Remove() { RemoveEntity<Student>("aluno", FileManager.DataBaseType.Student); }

    internal static void Select() { SelectMember<Student>("aluno", FileManager.DataBaseType.Student); }

    internal override void Introduce() { WriteLine($"\n🎓 New Student: {Name_s}, ID: {ID_i}, Age: {Age_by}, Genero: {Gender_c}, Data de nascimento: {BirthDate_dt.Date}, Nacionalidade: {Nationality}."); }
}

internal class Teacher : SchoolMembers
{
    [JsonInclude] internal string Department_s { get; private set; } = "";

    internal override string Describe()
    {
        return $"ID={ID_i}, Nome='{Name_s}', Idade={Age_by}, Gênero={Gender_c},Nascimento={BirthDate_dt:yyyy-MM-dd}, Nacionalidade={Nationality}, Email={email_s}, Departamento:{Department_s}.";
    }

    public Teacher() : base() { }
    private Teacher(string name, byte age, int id, char gender, DateTime birthDate, Nationality_e nat, string department) : base(id, name, age, gender, birthDate, nationality: nat)
    {
        Department_s = department;
        Introduce();
    }

    // Fábrica de objetos Teacher. Pode retornar null se o utilizador cancelar
    internal override BaseEntity? CreateInstance() => Create();

    internal static Teacher? Create()
    {
        return CreateMember(
            "professor",
            FileManager.DataBaseType.Teacher,

            // Primeiro os campos específicos
            dict =>
            {
                dict["Department"] = InputName("Departamento do professor");
            },

            // Depois o factory para criar o objeto
            dict => new Teacher(
                (string)dict["Name"],
                (byte)dict["Age"],
                (int)dict["ID"],
                (char)dict["Gender"],
                (DateTime)dict["BirthDate"],
                (Nationality_e)dict["Nationality"],
                (string)dict["Department"]
            )
        );
    }


    internal static void Remove() { RemoveEntity<Teacher>("professor", FileManager.DataBaseType.Teacher); }

    internal static void Select() { SelectMember<Teacher>("professor", FileManager.DataBaseType.Teacher); }

    internal override void Introduce() { WriteLine($"\n👨‍🏫 New Teacher: {Name_s}, ID: {ID_i}, Age: {Age_by}, Genero: {Gender_c}, Data de nascimento: {BirthDate_dt.Date}, Nacionalidade: {Nationality}."); }
}
