/// <summary>
/// Class abstrata de segundo grau, membros (Pessoas) da instituição tem esta class herdada.
/// </summary>
namespace School_System.Domain.SchoolMembers;

using static System.Console; // Permite usar Write e WriteLine diretamente
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

using School_System.Infrastructure.FileManager;
using School_System.Domain.Base;


public abstract class SchoolMember : BaseEntity
{
    [JsonInclude] internal protected byte Age_by { get; protected set; }// byte (0-255) porque a idade nunca é negativa e não passa de 255.
    [JsonInclude] internal protected char Gender_c { get; protected set; }// char 'M' ou 'F' (sempre um único caractere)
    [JsonInclude] internal protected DateTime BirthDate_dt { get; protected set; }// Data de nascimento (struct DateTime) 
    [JsonInclude] internal protected Nationality_e Nationality { get; protected set; }// Nacionalidade (enum) incorpurado para todos os tipos
    [JsonInclude] internal protected string Email_s { get; protected set; } = "";

    protected override string Describe()
    {
        return $"ID={ID_i}, Nome='{Name_s}', Idade={Age_by}, Gênero={Gender_c},Nascimento={BirthDate_dt:yyyy-MM-dd}, Nacionalidade={Nationality}, Email={Email_s}";
    }

    [JsonIgnore] protected const byte MinAge = 6;

    internal protected enum Nationality_e
    {
        Other,      // 0
        PT,         // Portugal
        ES,         // Espanha
        FR,         // França
        US,         // Estados Unidos
        GB,         // Reino Unido
        DE,         // Alemanha
        IT,         // Itália
        BR,         // Brasil
        JP,         // Japão
        CN,         // China
        IN,         // Índia
        CA,         // Canadá
        AU,         // Austrália
        RU          // Rússia
    }

    // construtor para desserialização
    protected SchoolMember() : base(0, "") { }
    // Construtor principal da classe base
    internal protected SchoolMember(int id, string name = "", byte age = default, char gender = default, DateTime? birthDate = default, Nationality_e nationality = default, string email = "")
     : base(id, name)
    {
        Age_by = age;
        Gender_c = gender;
        BirthDate_dt = birthDate ?? DateTime.Now;
        Nationality = nationality;
        Email_s = email;
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
                WriteLine(EmptyEntrance);
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


}

