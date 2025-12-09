/// <summary>
/// Class static que contem as funções para Inputs de parámetros das classes de baixo grau.
/// A maioria das funções que aqui se encontram podem tanto ser usadas para a criação de objetos como tambem para a sua edição.
/// </summary>
namespace School_System.Application.Utils;

using static System.Console; // Permite usar Write e WriteLine diretamente
using System.Text.RegularExpressions;

using School_System.Infrastructure.FileManager;
using School_System.Domain.Base;
using School_System.Domain.CourseProgram;
using School_System.Domain.SchoolMembers;

public enum Nationality_e
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
public enum VisaState_e
{
    NONE = 0,
    ValidStudentVisa,
    PendingRenewal,
    Expired,
    Temporary
}
public enum CourseType_e
{
    NONE = 0,
    CTESP = 5, // nivel 5
    Licenciatura = 6,
    Mestrado = 7,
    Doutoramento = 8
}
// enums de Course


/// <summary>
/// Contém funções estáticas para auxiliar na obtenção e validação de 
/// valores de entrada do utilizador (parâmetros/atributos) para as entidades de domínio.
/// </summary>
public static class InputParameters  // Nome da classe alterado
{
    // Mensagens de erro e aviso (ajuste os namespaces se necessário)
    private const string InvalidEntrance = "Entrada inválida. Tente novamente.";
    private const string EmptyEntrance = "Entrada nula ou em branco, valor default utilizado.";
    internal const string ProblemGetTheId = "❗ Erro: Não foi possível obter um ID válido. Criação cancelada.❗";

    internal const short MinEct = 3;
    internal const short MinCourseEct = 60;  // mínimo razoável para um curso
    internal const short MaxCourseEct = 360; // máximo típico de licenciatura prolongada

    internal const short MaxEctsPerYear = 60;
    internal const short MaxEctsPerSemester = MaxEctsPerYear / 2;

    internal const byte MinAge = 8;// variavel para defenir a idade minima que um estudante pode ter.
    internal const int MaxCourseYear = 4;


    /// <summary>  Pede ao usuário para inserir ou alterar um nome. </summary>
    /// <param name="prompt">Mensagem a exibir para o usuário.</param>
    /// <param name="isToEdit">Indica se é edição (true) ou criação (false).</param>
    /// <param name="currentValue">Valor atual, caso seja edição (null se criação).</param>
    /// <returns>O nome fornecido ou o valor atual/default caso não seja alterado.</returns>
    public static string InputName(string prompt, string? currentValue = null, bool isToEdit = false)
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
                WriteLine(EmptyEntrance);
                return isToEdit && !string.IsNullOrEmpty(currentValue) ? currentValue : "";
            }

            /*
            ^    -> início da string
            [a-zA-Z0-9À-ÿ \-'\.ºª,ç]+ -> conjunto de caracteres permitidos, um ou mais:
                a-z           -> letras minúsculas
                A-Z           -> letras maiúsculas
                0-9           -> dígitos
                À-ÿ           -> letras acentuadas (latinas)
                (espaço)      -> espaço
                \-            -> hífen
                '             -> apóstrofo
                \.            -> ponto
                º            -> símbolo º
                ª            -> símbolo ª
                ,             -> vírgula
            $   -> fim da string
            */
            if (!Regex.IsMatch(input, @"^[a-zA-Z0-9À-ÿ \-'\.ºª,ç]+$"))
            {
                WriteLine("❌ Nome inválido. Apenas letras, números, espaços, hífen, apóstrofo, ponto, º e ª são permitidos.");
                continue;
            }

            return input;
        }
    }

    // SchoolMember

    /// <summary> Pede ao usuário para inserir ou alterar a idade.</summary>
    /// <param name="prompt">Mensagem a exibir para o usuário.</param>
    /// <param name="currentValue">Valor atual, caso seja edição (null se criação).</param>
    /// <param name="isToEdit">Indica se é edição (true) ou criação (false).</param>
    /// <param name="minValue">Valor mínimo permitido.</param>
    /// <returns>A idade fornecida ou o valor atual caso não seja alterada.</returns>
    public static byte InputAge(string prompt, ref DateTime? currentBirthDate, byte? currentValue = null, bool isToEdit = false, byte minValue = MinAge)
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
                    return currentValue.Value;

                return 0;
            }

            // Tenta converter para byte
            if (byte.TryParse(input, out byte age))
            {
                if (age < minValue || age > byte.MaxValue) // valida limites
                {
                    WriteLine($"Idade inválida. Deve estar entre {minValue} e {byte.MaxValue}."); continue;
                }
                // Ajusta ano da data de nascimento, se existir
                if (currentBirthDate.HasValue)
                {
                    int anoAtual = DateTime.Now.Year;
                    currentBirthDate = new DateTime(
                        anoAtual - age,
                        currentBirthDate.Value.Month,
                        currentBirthDate.Value.Day
                    );
                    // Apenas mostrar o ano atualizado
                    WriteLine($"Ano calculado: {currentBirthDate.Value.Year}");
                }
                return age;
            }
            // Se não for byte, aviso de entrada inválida
            WriteLine(InvalidEntrance);
        }
    }

    /// <summary>Pede ao usuário para inserir ou alterar o gênero (M/F).</summary>
    /// <param name="prompt">Mensagem a exibir para o usuário.</param>
    /// <param name="currentValue">Valor atual, caso seja edição (null se criação).</param>
    /// <param name="isToEdit">Indica se é edição (true) ou criação (false).</param>
    /// <returns>O gênero fornecido ou valor default '\0' caso vazio.</returns>
    public static char InputGender(string prompt, char? currentValue = null, bool isToEdit = false)
    {
        while (true)
        {
            if (isToEdit && currentValue.HasValue && currentValue != default)
                Write($"{prompt} (M/F),(Enter para manter '{currentValue}'): ");
            else
                Write($"{prompt} (M/F),(Enter para default): ");

            string? input = ReadLine()?.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(input))
            {
                WriteLine(EmptyEntrance); // mostra aviso de valor default                          
                return isToEdit && currentValue.HasValue ? currentValue.Value : default;// Se vazio, mantém valor atual em edição, ou default na criação
            }

            /* Truth table(Or)
                M | F | S|
                0   0 = 0| 
                0   1 = 1| 
                1   0 = 1| 
                1   1 = impossível)neste caso)
            */
            if (input == "M" || input == "F") return input[0];
            WriteLine(InvalidEntrance);
        }
    }

    /// <summary> Solicita ao usuário a data de nascimento, suportando criação e edição.Pode funcionar a partir da idade (pedindo apenas mês/dia) ou a partir da data completa. </summary>
    /// <param name="prompt">Mensagem inicial exibida ao usuário.</param>
    /// <param name="age">Idade já conhecida (0 = pedir data completa).</param>
    /// <param name="currentValue">Data atual (usada apenas em edição).</param>
    /// <param name="isToEdit">Indica se a função está a editar (true) ou criar (false).</param>
    /// <returns>A data de nascimento obtida ou mantida.</returns>
    public static DateTime InputBirthDate(string prompt, ref byte age, byte minAge, DateTime? currentValue = null, bool isToEdit = false)
    {
        int currentYear = DateTime.Now.Year;
        while (true)
        {
            // CASO 1 — Idade desconhecida → pedir data completa
            if (age == default)
            {
                if (isToEdit && currentValue.HasValue)
                    Write($"{prompt} (Enter para manter '{currentValue.Value:yyyy-MM-dd}'): ");
                else
                    Write($"{prompt} (ex: 5 11 1980 ou 1980-11-05) (Enter para default): ");

                string? input = ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input))
                {
                    if (isToEdit && currentValue.HasValue) return currentValue.Value;
                    WriteLine(EmptyEntrance);
                    return default;
                }

                input = Regex.Replace(
                    input.Replace(',', ' '),  // Substitui todas as vírgulas por espaços 
                    @"\s+",                    // Expressão regular: \s+ = um ou mais espaços em branco 
                    " "                        // Substitui múltiplos espaços consecutivos por um único espaço 
                );

                if (!DateTime.TryParse(input, out DateTime parsed)) { WriteLine(InvalidEntrance); continue; }

                // Calcula idade
                int calculatedAge = currentYear - parsed.Year;

                // Mostrar idade calculada
                WriteLine($"Idade calculada: {calculatedAge} anos");

                // Validar idade
                if (calculatedAge < minAge || calculatedAge > byte.MaxValue)
                {
                    WriteLine($"Idade inválida. Deve estar entre {minAge} e {byte.MaxValue} anos.");
                    continue;
                }

                age = (byte)calculatedAge;
                return parsed.Date;
            }

            // CASO 2 — Idade já conhecida → pedir apenas mês e dia
            int estimatedYear = currentYear - age;
            WriteLine($"Ano de nascimento estimado: {estimatedYear}");

            // Ler mês
            int month;
            while (true)
            {
                Write("Digite o mês de nascimento (1-12): ");
                string? monthInput = ReadLine()?.Trim();
                if (int.TryParse(monthInput, out month) && month >= 1 && month <= 12) break;
                WriteLine("Mês inválido. Tente novamente.");
            }

            // Ler dia
            int day;
            while (true)
            {
                Write($"Digite o dia de nascimento (1-{DateTime.DaysInMonth(estimatedYear, month)}): ");
                string? dayInput = ReadLine()?.Trim();
                if (int.TryParse(dayInput, out day) && day >= 1 && day <= DateTime.DaysInMonth(estimatedYear, month)) break;
                WriteLine("Dia inválido. Tente novamente.");
            }

            return new DateTime(estimatedYear, month, day);

        }
    }

    /// <summary> Solicita ao usuário a nacionalidade de uma pessoa. Pode ser usada para criar ou editar um registro.
    /// Aceita número, sigla (ex: "PT") ou nome completo (ex: "Portugal"), sem diferenciar maiúsculas/minúsculas.
    /// </summary>
    /// <param name="prompt">Mensagem exibida antes da entrada.</param>
    /// <param name="currentValue"> Valor atual da nacionalidade (usado apenas em edição). Se o usuário pressionar Enter, este valor é mantido.</param>
    /// <param name="isToEdit">Indica se a função é para edição (true) ou criação (false).</param>
    /// <returns>O valor escolhido pelo usuário como <see cref="Nationality_e"/>. Retorna <see cref="Nationality_e.Other"/> se vazio na criação.</returns>
    public static Nationality_e InputNationality(string prompt, Nationality_e? currentValue = null, bool isToEdit = false)
    {
        // Dicionário estático para todas as funções de nacionalidade, case-insensitive
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
            // Mostra o prompt
            if (isToEdit && currentValue.HasValue)
                Write($"{prompt} (Enter para manter '{currentValue}'): ");
            else
                Write($"{prompt} (país ou sigla)('Ajuda' para opções): ");

            string? input = ReadLine()?.Trim();

            // Entrada vazia
            if (string.IsNullOrWhiteSpace(input))
            {
                WriteLine(EmptyEntrance);
                if (isToEdit && currentValue.HasValue) return currentValue.Value;
                return default; // Other
            }

            // Entrada válida
            if (nationalityMap.TryGetValue(input, out Nationality_e result)) return result;

            // Mostrar ajuda
            if (string.Equals(input, "Ajuda", StringComparison.OrdinalIgnoreCase))
            {
                // Agrupa por valor para evitar duplicações
                var grouped = nationalityMap.GroupBy(kv => kv.Value).OrderBy(g => g.Key.ToString());

                foreach (var group in grouped) WriteLine($" - {string.Join(", ", group.Select(kv => kv.Key))} = {group.Key}");
                continue;
            }
            WriteLine(InvalidEntrance);
        }
    }

    /// <summary> Lê um email do utilizador, validando o formato e garantindo que o domínio pertence à lista de domínios autorizados.
    /// Regras de validação:
    ///  - O email deve conter exatamente um '@'.
    ///  - A parte antes do '@' não pode começar/terminar com '.' nem conter '..'.
    ///  - O domínio deve ter pelo menos um ponto e nenhuma secção vazia.
    ///  - O domínio deve pertencer à lista de domínios permitidos (IPVC e escolas).
    /// </summary>
    /// <param name="prompt">Texto mostrado ao utilizador antes da leitura.</param>
    /// <param name="currentEmail">Email atual (apenas usado quando se está a editar).</param>
    /// <param name="isEditing">Define se o comportamento deve permitir manter o valor atual.</param>
    /// <returns>O email validado e autorizado.</returns>
    public static string InputEmail(string prompt, string? currentEmail = null, bool isEditing = false)
    {
        string[] allowedDomains =
        [
            "ipvc.pt", "estg.ipvc.pt", "estg.pt", "esa.ipvc.pt", "esa.pt", "ese.ipvc.pt", "ese.pt",
            "ess.ipvc.pt", "ess.pt", "esce.ipvc.pt", "esce.pt","esdl.ipvc.pt", "esdl.pt"
        ];

        while (true)
        {
            // Mostrar prompt ao utilizador
            if (isEditing && !string.IsNullOrEmpty(currentEmail))
                Write($"{prompt} (Enter para manter '{currentEmail}'): ");
            else
                Write($"{prompt} (Enter para default vazio): ");

            string? userInput = ReadLine()?.Trim();

            // Entrada vazia
            if (string.IsNullOrWhiteSpace(userInput))
            {
                WriteLine(EmptyEntrance);
                return (isEditing && !string.IsNullOrEmpty(currentEmail)) ? currentEmail : "";
            }

            // Encontrar o '@'
            int arrobaPosition = userInput.IndexOf('@');
            bool hasSingleArroba = arrobaPosition > 0 && arrobaPosition == userInput.LastIndexOf('@');

            if (!hasSingleArroba)
            {
                WriteLine("❌ Email inválido: deve conter exatamente um '@' e não pode começar com '@'.");
                continue;
            }

            // Dividir o email nas duas partes
            string usernamePart = userInput[..arrobaPosition];
            string domainPart = userInput[(arrobaPosition + 1)..];

            // Validar a parte antes do '@'
            bool invalidUsername = usernamePart.StartsWith('.') || usernamePart.EndsWith('.') || usernamePart.Contains("..");

            if (invalidUsername)
            {
                WriteLine("❌ Parte antes do '@' inválida.");
                continue;
            }

            // Validar o domínio
            string[] domainSections = domainPart.Split('.');
            bool hasDotInDomain = domainSections.Length >= 2;
            bool domainSectionsAreValid = !domainSections.Any(section => section.Length == 0);

            if (!hasDotInDomain || !domainSectionsAreValid)
            {
                WriteLine("❌ Domínio inválido.");
                continue;
            }

            // Confirmar que o domínio é permitido
            bool isDomainAllowed = allowedDomains.Contains(domainPart, StringComparer.OrdinalIgnoreCase);

            if (!isDomainAllowed)
            {
                WriteLine("❌ Domínio não autorizado.");
                continue;
            }

            return userInput;
        }
    }

    // usado para o parametro Year
    public static int InputInt(string prompt, int? min = null, int? max = null, int? currentValue = null, bool isToEdit = false)
    {
        while (true)
        {
            if (isToEdit && currentValue.HasValue) { Write($"{prompt} (Enter para manter '{currentValue.Value}'): "); }
            else { Write($"{prompt} (Enter para default vazio): "); }

            string? input = ReadLine()?.Trim();

            // Edição: Enter → manter o valor atual
            if (isToEdit && string.IsNullOrEmpty(input) && currentValue.HasValue) return currentValue.Value;

            if (string.IsNullOrEmpty(input))
            {
                WriteLine(EmptyEntrance);
                return default;
            }

            if (!int.TryParse(input, out int number))
            {
                WriteLine(InvalidEntrance);
                continue;
            }

            if (min.HasValue && number < min.Value)
            {
                WriteLine($"O valor deve ser >= {min.Value}");
                continue;
            }

            if (max.HasValue && number > max.Value)
            {
                WriteLine($"O valor deve ser <= {max.Value}");
                continue;
            }

            return number;
        }
    }

    //InternationalStudent

    public static VisaState_e InputVisaStatus(string prompt, VisaState_e? currentValue = null, bool isToEdit = false)
    {
        var visaStatusMap = Enum.GetValues<VisaState_e>() // Obtem os valores do enum e tranforma em um array.
                                    .Cast<VisaState_e>()  // LINQ: converte todos os objetos para VisaState_e
                                    .ToDictionary(        // LINQ: transforma em dicionário
                                    v => v.ToString(),    // chave do dicionário
                                    v => v,               // valor do dicionário
                                    StringComparer.OrdinalIgnoreCase);        // comparador ignore case
        while (true)
        {
            if (isToEdit && currentValue.HasValue)
                Write($"{prompt} (Enter para manter '{currentValue}'): ");
            else
                Write($"{prompt} (nome ou número) ('Ajuda' para opções): ");

            string? input = ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                WriteLine(EmptyEntrance);

                if (isToEdit && currentValue.HasValue)
                    return currentValue.Value;

                return default;
            }

            if (string.Equals(input, "Ajuda", StringComparison.OrdinalIgnoreCase))
            {
                WriteLine("\nOpções disponíveis para Status do Visto:");
                foreach (var status in Enum.GetValues(typeof(VisaState_e)))
                    WriteLine($" - {(int)status} = {status}");
                continue;
            }

            if (visaStatusMap.TryGetValue(input, out VisaState_e result))
                return result;

            if (int.TryParse(input, out int numValue) && Enum.IsDefined(typeof(VisaState_e), numValue))
                return (VisaState_e)numValue;

            WriteLine(InvalidEntrance);
            WriteLine("Digite 'Ajuda' para ver todas as opções.");
        }
    }

    //Courses

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
    public static CourseType_e InputCourseType(string prompt, CourseType_e? currentValue = null, bool isToEdit = false)
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
    public static float InputCourseDuration(string prompt, float? currentValue = null, bool isToEdit = false, int maxduration = MaxCourseYear)
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
                // Retorna 0 (default) se não estiver em edição e o campo estiver vazio.
                return default;
            }

            // Tenta fazer o parsing para float
            if (float.TryParse(input, out float duration))
            {
                // 1. Validação: Não pode ser negativa.
                if (duration < 0)
                {
                    WriteLine(InvalidEntrance + " A duração não pode ser negativa.");
                    continue;
                }

                // 2. Validação: Não pode exceder a duração máxima.
                if (duration > maxduration)
                {
                    WriteLine(InvalidEntrance + $" A duração máxima do curso é de {maxduration} anos.");
                    continue;
                }

                // Se as validações passarem, retorna a duração.
                return duration;
            }
            else
            {
                WriteLine(InvalidEntrance + " Use um número válido com vírgula (ex: 1 ou 0,5).");
            }
        }
    }

    // Disciplinas

    public static short InputSubjectsECTS(string prompt, short minEct = MinEct, short? currentValue = null, bool isToEdit = false)
    {
        while (true)
        {
            // Prompt correto dependendo se está a editar
            if (isToEdit && currentValue.HasValue) Write($"{prompt} (Enter para manter '{currentValue}'): ");
            else Write($"{prompt} ({minEct}-{MaxEctsPerSemester} ECTS, Enter para default): ");

            string? input = ReadLine()?.Trim();

            // Entrada vazia
            if (string.IsNullOrWhiteSpace(input))
            {
                if (isToEdit && currentValue.HasValue) return currentValue.Value; // mantém valor atual
                WriteLine(EmptyEntrance);
                return minEct; // default: mínimo permitido
            }
            // Tenta converter
            if (short.TryParse(input, out short ects))
            {
                if (ects >= minEct && ects <= MaxEctsPerSemester)
                    return ects;
                WriteLine($"Valor inválido. Insira entre {minEct} e {MaxEctsPerSemester} ECTS.");
            }
            else
            {
                WriteLine($"{InvalidEntrance} Insira um número inteiro.");
            }
        }
    }

    // Usam AskAndSearch

    internal static Teacher? InputTeacher(string prompt = "", Teacher? currentTeacher = null, bool isEditing = false)
    {
        while (true)
        {
            // 1. Calcula prompt final
            string finalPrompt = isEditing && currentTeacher != null
                ? $"{prompt} (Enter para manter '{currentTeacher.Name_s}'): "
                : $"";

            // 2. Chama AskAndSearch (allowListAll = true para suportar '-a')
            var searchResult = BaseEntity.AskAndSearch<Teacher>(typeName: "professor", dbType: FileManager.DataBaseType.Teacher, allowListAll: true);

            // 3. Base vazia → mantém valor atual / null
            if (searchResult.IsDatabaseEmpty)
            {
                // não precisa de dizer que está vazia porque a função askandSearch já faz isso.
                return currentTeacher;
            }

            var matches = searchResult.Results;

            // 4. Nenhum resultado → mantém valor atual / null
            if (matches.Count == 0)
            {
                WriteLine("Nenhum professor encontrado. Mantendo valor atual.");
                return currentTeacher;
            }

            Teacher selected;

            // 5. Apenas um resultado → seleciona direto
            if (matches.Count == 1)
            {
                selected = matches[0];
            }
            else
            {
                // 6. Múltiplos resultados: Pede ao utilizador para escolher
                // A lista de matches já foi exibida pelo AskAndSearch (devido a allowListAll=true ou pesquisa genérica)
                Write($"Digite o número do professor desejado (1 - {matches.Count}, Enter para cancelar): ");
                string? choiceInput = ReadLine()?.Trim();

                if (string.IsNullOrEmpty(choiceInput))
                    return currentTeacher;

                if (!int.TryParse(choiceInput, out int choice) || choice < 1 || choice > matches.Count)
                {
                    WriteLine("Entrada inválida. Tente novamente.\n");
                    continue;
                }

                selected = matches[choice - 1];

            }

            // 7. Confirmação final
            Write($"Confirmar o professor '{selected.Name_s}' (ID {selected.ID_i})? (S/N): ");
            string? confirm = ReadLine()?.Trim().ToUpper();

            if (confirm == "S")
                return selected;

            WriteLine("Seleção cancelada. Pode tentar novamente.\n");
        }
    }

    public static Course? InputCourse(string prompt = "Escreva o nome ou ID do Curso", Course? currentCourse = null, bool isToEdit = false)
    {
        while (true)
        {
            // 1. Calcula prompt final
            string finalPrompt = isToEdit && currentCourse != null
                ? $"{prompt} (Enter para manter '{currentCourse.Name_s}'): "
                : $"{prompt} (Enter para default): ";

            // 2. Chama AskAndSearch (allowListAll = true para suportar '-a')
            var searchResult = BaseEntity.AskAndSearch<Course>(
                typeName: "curso",
                dbType: FileManager.DataBaseType.Course,
                allowListAll: true
            );

            // 3. Base vazia → mantém valor atual / null
            if (searchResult.IsDatabaseEmpty)
            {
                WriteLine("A base de dados de cursos está vazia.");
                return currentCourse;
            }

            var matches = searchResult.Results;

            // 4. Nenhum resultado → mantém valor atual / null
            if (matches.Count == 0)
                return currentCourse;

            Course selected;

            // 5. Apenas um resultado → seleciona direto
            if (matches.Count == 1)
            {
                selected = matches[0];
            }
            else
            {
                // Ask the user to pick a course from the already displayed list
                Write($"Digite o número do curso desejado (1 - {matches.Count}, Enter para cancelar): ");
                string? choiceInput = ReadLine()?.Trim();

                if (string.IsNullOrEmpty(choiceInput))
                    return currentCourse;

                if (!int.TryParse(choiceInput, out int choice) || choice < 1 || choice > matches.Count)
                {
                    WriteLine("Entrada inválida. Tente novamente.\n");
                    continue;
                }

                selected = matches[choice - 1];

            }

            // 7. Confirmação final
            Write($"Confirma o curso '{selected.Name_s}' (ID {selected.ID_i})? (S/N): ");
            string? confirm = ReadLine()?.Trim().ToUpper();

            if (confirm == "S")
                return selected;

            WriteLine("Seleção cancelada. Pode tentar novamente.\n");
        }
    }
}