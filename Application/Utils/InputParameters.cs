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

            if (!Regex.IsMatch(input, @"^[a-zA-Z0-9À-ÿ \-']+$"))
            {
                WriteLine("❌ Nome inválido. Apenas letras, números, espaços, hífen e apóstrofo são permitidos.");
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
    public static char InputGender(string prompt, char? currentValue = null, bool isToEdit = false)
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
    public static DateTime InputBirthDate(string prompt, ref byte age, DateTime? currentValue = null, bool isToEdit = false)
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

                    if (parts.Length < 2) { WriteLine(InvalidEntrance); continue; }

                    if (!int.TryParse(parts[0], out int mesTmp) || mesTmp < 1 || mesTmp > 12) { WriteLine(InvalidEntrance); continue; }
                    if (!int.TryParse(parts[1], out int diaTmp) || diaTmp < 1 || diaTmp > DateTime.DaysInMonth(anoEstimado, mesTmp)) { WriteLine(InvalidEntrance); continue; }

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
        // Validação rigorosa de email
        var emailPattern = @"^(?!.*\.\.)(?!.*\.$)(?!^\.)[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";
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
            if (isToEdit && currentCourse != null)
                Write($"{prompt} (Enter para manter '{currentCourse.Name_s}'): ");
            else
                Write($"{prompt} (Enter para cancelar): ");

            // Pesquisa cursos na base de dados, retornando todos os matches
            var matches = BaseEntity.AskAndSearch<Course>("curso", FileManager.DataBaseType.Course);

            // 🚨 Nenhum curso encontrado
            if (matches.Count == 0)
            {
                if (isToEdit && currentCourse != null)
                {
                    WriteLine($"Nenhum curso encontrado. Mantendo '{currentCourse.Name_s}'.");
                    return currentCourse;
                }

                WriteLine("Nenhum curso disponível. Saltando seleção de curso...");
                return null;
            }

            // Se houver apenas 1 resultado, pedir confirmação
            if (matches.Count == 1)
            {
                var selected = matches[0];
                Write($"Confirmar o curso '{selected.Name_s}' (ID {selected.ID_i})? (S/N): ");
                if ((ReadLine()?.Trim().ToUpper()) == "S") return selected;

                WriteLine("Seleção cancelada. Vamos tentar novamente.\n");
                continue;
            }

            // Mais de 1 resultado → pede escolha
            WriteLine("\nCursos encontrados:");
            for (int i = 0; i < matches.Count; i++)
            {
                string name = matches[i]?.Name_s ?? "Sem nome";
                WriteLine($"{i + 1}. {name} (ID {matches[i].ID_i})");
            }

            Write($"Escolha qual curso deseja selecionar (1 - {matches.Count}, Enter para cancelar): ");
            string? input = ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input)) return currentCourse;

            if (!int.TryParse(input, out int choice) || choice < 1 || choice > matches.Count)
            {
                WriteLine(InvalidEntrance);
                continue;
            }

            var selectedCourse = matches[choice - 1];
            Write($"Confirmar o curso '{selectedCourse.Name_s}' (ID {selectedCourse.ID_i})? (S/N): ");
            if ((ReadLine()?.Trim().ToUpper()) == "S") return selectedCourse;

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
                Write($"Confirmar o professor '{selected.Name_s}' (ID {selected.ID_i})? (S/N): ");
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
                WriteLine("Entrada inválida. Tente novamente.\n");
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

    /// <summary> Solicita ao usuário o estado do visto (VisaState_e).</summary>
    /// <summary> Solicita ao usuário o estado do visto (VisaState_e).</summary>
    public static VisaState_e InputVisaStatus(string prompt, VisaState_e? currentValue = null, bool isToEdit = false)
    {
        // Dicionário de mapeamento de nomes do enum (ignora maiúsculas/minúsculas)
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
                if (isToEdit && currentValue.HasValue) return currentValue.Value; // mantém valor atual
                return default; // valor default
            }

            // Se digitou "Ajuda", exibe todas as opções
            if (string.Equals(input, "Ajuda", StringComparison.OrdinalIgnoreCase))
            {
                WriteLine("\nOpções disponíveis para Status do Visto:");
                foreach (var status in Enum.GetValues(typeof(VisaState_e)))
                    WriteLine($" - {(int)status} = {status}");
                continue;
            }

            // Tenta converter pelo nome do enum
            if (visaStatusMap.TryGetValue(input, out VisaState_e result)) return result;

            // Tenta converter pelo número do enum
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

    public static List<Subject> InputSubjects(string prompt = "Selecione as disciplinas do curso", List<Subject>? currentSubjects = null, bool isToEdit = false)
    {
        var selectedSubjects = currentSubjects != null ? new List<Subject>(currentSubjects) : new List<Subject>();

        if (isToEdit && selectedSubjects.Count > 0) WriteLine($"{prompt} (Enter para manter as disciplinas já selecionadas: {selectedSubjects.Count})");
        else WriteLine(prompt);

        // Pergunta ao usuário e pesquisa na base de dados, permitindo selecionar múltiplos
        var matches = BaseEntity.AskAndSearch<Subject>(
            "disciplina",
            FileManager.DataBaseType.Subject,
            returnAll: false,
            allowMultiple: true
        );

        if (matches.Count == 0)
        {
            WriteLine("Nenhuma disciplina selecionada.");
            return selectedSubjects; // retorna a lista atual ou vazia
        }

        // Adiciona as disciplinas selecionadas à lista existente, evitando duplicados
        foreach (var s in matches)
        {
            if (!selectedSubjects.Contains(s))
                selectedSubjects.Add(s);
        }

        // Mostra resumo final
        WriteLine("\nDisciplinas selecionadas:");
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
            else Write($"{prompt} ({minEct}-{Course.MaxEctsPerSemester} ECTS, Enter para default): ");

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
                if (ects >= minEct && ects <= Course.MaxEctsPerSemester)
                    return ects;
                WriteLine($"Valor inválido. Insira entre {minEct} e {Course.MaxEctsPerSemester} ECTS.");
            }
            else
            {
                WriteLine($"{InvalidEntrance} Insira um número inteiro.");
            }
        }
    }



}