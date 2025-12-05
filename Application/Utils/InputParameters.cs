namespace School_System.Application.Utils;

using static System.Console; // Permite usar Write e WriteLine diretamente
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using School_System.Infrastructure.FileManager;
using Schoo_lSystem.Application.Menu;
using School_System.Domain.Base;
using School_System.Domain.CourseProgram;
using School_System.Domain.SchoolMembers;
using School_System.Application.Utils;


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
    NONE,
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

    internal const short MinCourseEct = 60;  // mínimo razoável para um curso
    internal  const short MaxCourseEct = 360; // máximo típico de licenciatura prolongada

    internal  const short MaxEctsPerYear = 60;
    internal  const short MaxEctsPerSemester = MaxEctsPerYear / 2;

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
            [a-zA-Z0-9À-ÿ \-'\.ºª,]+ -> conjunto de caracteres permitidos, um ou mais:
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
            if (!Regex.IsMatch(input, @"^[a-zA-Z0-9À-ÿ \-'\.ºª,]+$"))
            {
                WriteLine("❌ Nome inválido. Apenas letras, números, espaços, hífen, apóstrofo, ponto, º e ª são permitidos.");
                continue;
            }

            return input;
        }
    }

    // SchoolMember

    /// <summary>
    /// Pede ao usuário para inserir ou alterar a idade.
    /// </summary>
    /// <param name="prompt">Mensagem a exibir para o usuário.</param>
    /// <param name="currentValue">Valor atual, caso seja edição (null se criação).</param>
    /// <param name="isToEdit">Indica se é edição (true) ou criação (false).</param>
    /// <param name="minValue">Valor mínimo permitido.</param>
    /// <returns>A idade fornecida ou o valor atual caso não seja alterada.</returns>
    public static byte InputAge(string prompt, ref DateTime? currentBirthDate, byte? currentValue = null, bool isToEdit = false, byte minValue = default)
    {
        while (true)
        {
            if (isToEdit && currentValue.HasValue) Write($"{prompt} (Enter para manter {currentValue}): ");
            else Write($"{prompt} (Enter para calcular pela data de nascimento): ");

            string? input = ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                WriteLine(EmptyEntrance);
                if (isToEdit && currentValue.HasValue) return currentValue.Value; // mantém valor atual
                else return 0; // default → será calculado a partir da data de nascimento
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
    /// Solicita ao usuário a data de nascimento, suportando criação e edição.
    /// Pode funcionar a partir da idade (pedindo apenas mês/dia) ou a partir da data completa.
    /// </summary>
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
            // CASO 1 — Idade não conhecida → pedir data completa
            if (age == 0)
            {
                if (isToEdit && currentValue.HasValue)
                    Write($"{prompt} (Enter para manter '{currentValue.Value:yyyy-MM-dd}'): ");
                else
                    Write($"{prompt} (ex: 5 11 1980 ou 1980-11-05) (Enter para default): ");

                string? input = ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input))
                {
                    if (isToEdit && currentValue.HasValue)
                        return currentValue.Value;

                    WriteLine(EmptyEntrance);
                    return default;
                }

                input = Regex.Replace(input.Replace(',', ' '), @"\s+", " ");

                if (!DateTime.TryParse(input, out DateTime parsed))
                {
                    WriteLine(InvalidEntrance);
                    continue;
                }

                // Calcula idade
                int calculatedAge = currentYear - parsed.Year;

                // Verifica validade
                if (calculatedAge < minAge || calculatedAge > 120)
                {
                    WriteLine($"Idade inválida. Deve estar entre {minAge} e 120 anos.");
                    continue;
                }

                age = (byte)calculatedAge;
                return parsed.Date;
            }

            // CASO 2 — Idade já conhecida → pedir apenas mês e dia
            int estimatedYear = currentYear - age;
            WriteLine($"Ano de nascimento estimado: {estimatedYear}");

            if (isToEdit && currentValue.HasValue)
                Write($"Insira mês e dia de nascimento (ex: 12 31 ou 11,30) (Enter para manter '{currentValue.Value:MM-dd}'): ");
            else
                Write("Insira mês e dia de nascimento (ex: 12 31 ou 11,30) (Enter para default): ");

            string? inputMD = ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(inputMD))
            {
                return (isToEdit && currentValue.HasValue)
                    ? currentValue.Value
                    : new DateTime(estimatedYear, 1, 1);
            }

            inputMD = Regex.Replace(inputMD.Replace(',', ' '), @"\s+", " ");

            string[] parts = inputMD.Split(' ');

            if (parts.Length < 2)
            {
                WriteLine(InvalidEntrance);
                continue;
            }

            if (!int.TryParse(parts[0], out int month) || month < 1 || month > 12)
            {
                WriteLine(InvalidEntrance);
                continue;
            }

            if (!int.TryParse(parts[1], out int day) || day < 1 || day > DateTime.DaysInMonth(estimatedYear, month))
            {
                WriteLine(InvalidEntrance);
                continue;
            }

            return new DateTime(estimatedYear, month, day);
        }
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
    public static Nationality_e InputNationality(string prompt, Nationality_e? currentValue = null, bool isToEdit = false)
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
    public static string InputEmail(string prompt, string? currentValue = null, bool isToEdit = false)
    {
        /*
        ^ → início da string
        .+ → um ou mais caracteres (qualquer coisa) antes do @
        @ → obrigatoriamente o @
        .+ → um ou mais caracteres depois do @
        \. → um ponto
        .+ → um ou mais caracteres depois do ponto
        $ → fim da string
        */
        var emailPattern = @"^.+@.+\..+$";

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
            if (!Regex.IsMatch(input, emailPattern))
            {
                WriteLine("❌ Email inválido. Certifique-se que não tem '..', não começa/termina com '.', e que o domínio é válido.");
                continue;
            }
            return input;
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

            // ► Edição: Enter → manter o valor atual
            if (isToEdit && string.IsNullOrEmpty(input) && currentValue.HasValue)
                return currentValue.Value;

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

    public static Course? InputCourse(string prompt = "Escreva o nome do Curso", Course? currentCourse = null, bool isToEdit = false)
    {
        while (true)
        {
            var matches = BaseEntity.AskAndSearch<Course>("curso", FileManager.DataBaseType.Course);

            // No course found → message + return default/current
            if (matches.Count == 0)
            {
                WriteLine("Valor default utilizado.");
                return isToEdit ? currentCourse : default;
            }

            // Only one course → ask confirmation
            if (matches.Count == 1)
            {
                var selected = matches[0];
                Write($"Escolher o curso '{selected.Name_s}' (ID {selected.ID_i})? (S/N): ");
                string? answer = ReadLine()?.Trim().ToUpper();

                if (string.IsNullOrWhiteSpace(answer)) return isToEdit ? currentCourse : default;

                if (answer == "S") return selected;

                WriteLine("Seleção cancelada. Vamos tentar novamente.\n");
                continue;
            }

            //  Multiple courses → let user choose by number
            WriteLine("\nCursos encontrados:");
            for (int i = 0; i < matches.Count; i++)
                WriteLine($"{i + 1}. {matches[i].Name_s} (ID {matches[i].ID_i})");

            Write($"Escolha qual curso deseja selecionar (1 - {matches.Count}, Enter para cancelar): ");
            string? choiceInput = ReadLine()?.Trim();

            if (string.IsNullOrEmpty(choiceInput))
                return isToEdit ? currentCourse : default;

            if (!int.TryParse(choiceInput, out int choice) || choice < 1 || choice > matches.Count)
            {
                WriteLine(InvalidEntrance);
                continue;
            }

            var selectedCourse = matches[choice - 1];
            Write($"Confirmar o curso '{selectedCourse.Name_s}' (ID {selectedCourse.ID_i})? (S/N): ");
            if ((ReadLine()?.Trim().ToUpper()) == "S")
                return selectedCourse;

            WriteLine("Seleção cancelada. Vamos tentar novamente.\n");
        }
    }


    //graduate

    internal static Teacher? InputTeacher(string prompt = "Escreva o nome do Tutor", Teacher? currentTeacher = null, bool isToEdit = false)
    {
        while (true)
        {
            if (isToEdit && currentTeacher != null) Write($"{prompt} (Enter para manter '{currentTeacher.Name_s}'): ");
            else Write($"{prompt} (Enter para default): ");

            var matches = BaseEntity.AskAndSearch<Teacher>("professor", FileManager.DataBaseType.Teacher);

            if (matches.Count == 0)
            {
                WriteLine("Nenhum professor encontrado. Usando valor padrão.");
                return currentTeacher; // mantém o anterior ou null
            }

            Teacher selected;

            if (matches.Count == 1)
            {
                selected = matches[0];
                Write($"Confirmar o(a) professor(a) '{selected.Name_s}' (ID {selected.ID_i})? (S/N): ");
                string? confirm = ReadLine()?.Trim().ToUpper();
                if (confirm == "S") return selected;

                WriteLine("Seleção cancelada. Pode tentar novamente.\n");
                continue;
            }

            // Mais de um resultado → pede escolha
            WriteLine("\nProfessores encontrados:");
            for (int i = 0; i < matches.Count; i++)
            {
                string name = matches[i]?.Name_s ?? "Sem nome";
                WriteLine($"{i + 1}. {name} (ID {matches[i].ID_i})");
            }

            Write($"Escolha qual professor deseja selecionar (1 - {matches.Count}, Enter para cancelar): ");
            string? input = ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                WriteLine(EmptyEntrance);
                return currentTeacher;
            }

            if (!int.TryParse(input, out int choice) || choice < 1 || choice > matches.Count)
            {
                WriteLine(InvalidEntrance);
                continue;
            }

            selected = matches[choice - 1];
            Write($"Confirmar o professor '{selected.Name_s}' (ID {selected.ID_i})? (S/N): ");
            string? finalConfirm = ReadLine()?.Trim().ToUpper();
            if (finalConfirm == "S") return selected;

            WriteLine("Seleção cancelada. Pode tentar novamente.\n");
        }
    }


    //InternationalStudent

    public static VisaState_e InputVisaStatus(string prompt, VisaState_e? currentValue = null, bool isToEdit = false)
    {
        var visaStatusMap = Enum.GetValues(typeof(VisaState_e))
                                 .Cast<VisaState_e>()
                                 .ToDictionary(v => v.ToString(), v => v, StringComparer.OrdinalIgnoreCase);

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

                WriteLine("⚠ O estado do visto é obrigatório.");
                continue;
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
    /// <param name="prompt">Mensagem a exibir para o usuário.</param>
    /// <param name="currentValue">
    /// Valor atual da duração (usado somente se <paramref name="isToEdit"/> for true). 
    /// Caso o usuário pressione Enter, esse valor será mantido.
    /// </param>
    /// <param name="isToEdit">Indica se a função está sendo chamada para edição (true) ou criação (false).</param>
    /// <returns>
    /// A duração do curso em anos como <see cref="float"/>. Se a entrada for vazia na criação, retorna 0.
    /// </returns>
    public static float InputCourseDuration(string prompt, float? currentValue = null, bool isToEdit = false)
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
                return default;
            }

            // Aqui usamos a cultura atual do sistema — PT-PT se o PC estiver em PT
            if (float.TryParse(input, out float duration))
            {
                if (duration >= 0) return duration;
                WriteLine(InvalidEntrance + " A duração não pode ser negativa.");
            }
            else
            {
                WriteLine(InvalidEntrance + " Use um número válido com vírgula (ex: 1 ou 0,5).");
            }
        }
    }

    /// <summary>  Só deve ser usado no modo Edit</summary>
    /// <param name="prompt"></param>
    /// <param name="currentSubjects"></param>
    /// <returns></returns>
    public static List<Subject> InputSubjects(string prompt = "Editar disciplinas do curso.", List<Subject>? currentSubjects = null)
    {
        // A lista de disciplinas atual (as que estão no curso antes da edição).
        var selectedSubjects = currentSubjects != null ? new List<Subject>(currentSubjects) : new List<Subject>();

        // 1. Apresentação inicial e instruções.
        WriteLine("==========================================");
        WriteLine(prompt);

        if (selectedSubjects.Count > 0)
        {
            WriteLine($"Disciplinas atualmente selecionadas (Total: {selectedSubjects.Count}):");
            foreach (var s in selectedSubjects)
                WriteLine($" - {s.Name_s} (ID {s.ID_i})");
        }
        else
        {
            WriteLine("Nenhuma disciplina está atualmente selecionada.");
        }

        WriteLine("\nInstruções: Digite o ID ou nome da disciplina para ADICIONAR. Digite 'REMOVER [ID/Nome]' para remover. Digite 'FIM' para terminar.");
        WriteLine("==========================================");

        // Loop de seleção contínua
        while (true)
        {
            Write("Ação (Adicionar/Remover/FIM): ");
            string? input = ReadLine()?.Trim();

            // 2. CONDIÇÃO DE SAÍDA: Enter vazio ou 'FIM'
            if (string.IsNullOrWhiteSpace(input) || input.Equals("FIM", StringComparison.OrdinalIgnoreCase))
                break;

            // 3. Processamento de REMOÇÃO
            if (input.StartsWith("REMOVER", StringComparison.OrdinalIgnoreCase))
            {
                var removalPart = input.Substring("REMOVER".Length).Trim();

                if (string.IsNullOrWhiteSpace(removalPart))
                {
                    WriteLine("❌ Por favor, especifique o ID ou nome da disciplina a remover.");
                    continue;
                }

                continue;
            }

            // 4. Processamento de ADIÇÃO (Input normal)
            // Chama AskAndSearch para encontrar o item
            // A busca é por item único (allowMultiple: false) para evitar o parsing complexo.
            var matches = BaseEntity.AskAndSearch<Subject>(
                "disciplina",
                FileManager.DataBaseType.Subject,
                returnAll: false,
                allowMultiple: false
            );

            // 5. Adicionar o item se encontrado
            if (matches.Count == 1)
            {
                var subject = matches[0];
                if (!selectedSubjects.Contains(subject))
                {
                    selectedSubjects.Add(subject);
                    WriteLine($"✅ Disciplina '{subject.Name_s}' (ID {subject.ID_i}) adicionada.");
                }
                else
                {
                    WriteLine($"⚠️ Disciplina '{subject.Name_s}' já estava selecionada.");
                }
            }
            else if (matches.Count > 1)
            {
                // Isto pode acontecer se a pesquisa por nome for muito genérica.
                WriteLine("⚠️ Múltiplas disciplinas encontradas. Por favor, use o ID ou nome exato.");
            }
        }

        // Resumo final da edição
        WriteLine("\n--- Edição Concluída ---");
        WriteLine($"Disciplinas Finais (Total: {selectedSubjects.Count}):");
        foreach (var s in selectedSubjects)
            WriteLine($" - {s.Name_s} (ID {s.ID_i})");

        return selectedSubjects;
    }


    // Disciplinas

    public static short InputSubjectsECTS(string prompt, short minEct, short? currentValue = null, bool isToEdit = false)
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



}