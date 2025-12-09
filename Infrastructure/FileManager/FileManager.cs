/// <summary>
/// Esta class é estática e é responsavel por gerir os ficheiros de base de dados deste sistema.
/// Sendo que , a sua função principal é ler e escrever nos ficheiros JSON que servem de base de dados.
/// Sen esquecer que tambem é responsável por garantir que os ficheiros e diretórios existem no arranque do programa e duante a sua execução.
/// Também fornece funcionalidades para obter o próximo ID disponível para novos objetos, remover registos por ID e pesquisar registos com filtros específicos.
/// Esta class usa bastante acesso LINQ para percorrer JSONs e dicionários com Caminhos(PATHs).
/// Ao remover dados e a tentar reparar ficheiros josn, cria um backup com o nome do json.
/// </summary>
namespace School_System.Infrastructure.FileManager;

using static System.Console;
using System.Text.Json;
using System.Reflection;

using School_System.Domain.Base;
using School_System.Domain.SchoolMembers;
using School_System.Domain.CourseProgram;

// Classe estática — não precisa ser instanciada
public static class FileManager
{
    public enum DataBaseType// Enum para percorrer os caminhos das base de dados.
    {
        UndergraduateStudent,
        GraduateStudent,
        InternationalStudent,
        Teacher,
        Course,
        Subject
    }

    private static readonly Dictionary<string, string> DomainDirectories = new()
    {
        { "Domain",              "Domain" },
        { "Base",                "Domain/Base" },
        { "SchoolMembers",       "Domain/SchoolMembers" },
        { "CourseProgram",       "Domain/CourseProgram" },
        { "Interfaces",          "Domain/Interfaces" },
        { "Scholarship",         "Domain/Scholarship" }

    };
    private static readonly Dictionary<string, string> ApplicationDirectories = new()
    {
        { "Application",         "Application" },
        { "Menu",                "Application/Menu" },
        { "Utils",               "Application/Utils"}
    };
    private static readonly Dictionary<string, string> InfrastructureDirectories = new()
    {
        { "Infrastructure",      "Infrastructure" },
        { "FileManager",         "Infrastructure/FileManager" },
        { "Data",                "Infrastructure/Data" },
        { "Backup",              "Infrastructure/Data/backup" }
    };

    // Combina todos os diretórios definidos nos dicionários individuais e devolve como uma sequência (IEnumerable<string>) de caminhos.
    private static IEnumerable<string> GetAllDirectories()
    {
        return DomainDirectories.Values// Pega todos os valores do dicionário DomainDirectories
    .Concat(ApplicationDirectories.Values)   // Concatena os valores do ApplicationDirectories
    .Concat(InfrastructureDirectories.Values); // Concatena os valores do InfrastructureDirectories
    }

    private static readonly Dictionary<string, string> Files = new()
    {
        // Applications
        { "Menu",                "Application/Menu/Menu.cs"},
        { "InputParameters",     "Application/Utils/InputParameters.cs"},

        // Domain
        { "BaseEntity",          "Domain/Base/BaseEntity.cs" },
        { "SchoolMember",        "Domain/SchoolMembers/SchoolMember.cs" },
        { "Student",             "Domain/SchoolMembers/Student.cs" },
        { "Teacher",             "Domain/SchoolMembers/Teacher.cs" },
        { "Course",              "Domain/CourseProgram/Course.cs" },
        { "Subject",             "Domain/CourseProgram/Subject.cs" },
        { "UndergraduateStudent","Domain/SchoolMembers/UndergraduateStudent.cs"},
        { "GraduateStudent",     "Domain/SchoolMembers/GraduateStudent.cs"},
        { "InternationalStudent","Domain/SchoolMembers/InternationalStudent.cs"},
        { "Scholarship",         "Domain/Scholarship/Scholarship.cs"},

        // Infrastructure
        { "FileManager",         "Infrastructure/FileManager/FileManager.cs" },

        // Data
        { "TeachersJSON",               "Infrastructure/Data/teachers.json" },
        { "CoursesJSON",                "Infrastructure/Data/courses.json" },
        { "SubjectsJSON",               "Infrastructure/Data/subjects.json" },
        { "UndergraduateStudentsJSON",  "Infrastructure/Data/undergraduate_students.json" },
        { "GraduateStudentsJSON",       "Infrastructure/Data/graduate_students.json" },
        { "InternationalStudentsJSON",  "Infrastructure/Data/international_students.json" }
    };
    // Retorna todos os caminhos de arquivos definidos no dicionário Files
    private static IEnumerable<string> AllFiles => Files.Values;

    //---------------------

    // Função para devolver o caminho para uma base de dados e um Tipo de class
    private static (string path, Type type) GetDataBaseInfo(DataBaseType baseType)
    {
        return baseType switch
        {
            DataBaseType.UndergraduateStudent => (Files["UndergraduateStudentsJSON"], typeof(UndergraduateStudent)),
            DataBaseType.GraduateStudent => (Files["GraduateStudentsJSON"], typeof(GraduateStudent)),
            DataBaseType.InternationalStudent => (Files["InternationalStudentsJSON"], typeof(InternationalStudent)),
            DataBaseType.Teacher => (Files["TeachersJSON"], typeof(Teacher)),
            DataBaseType.Course => (Files["CoursesJSON"], typeof(Course)),
            DataBaseType.Subject => (Files["SubjectsJSON"], typeof(Subject)),

            _ => throw new ArgumentOutOfRangeException(nameof(baseType),
                $"Base de dados não encontrada para o valor recebido: {baseType}. " +
                "Se adicionou um novo DataBaseType, lembre-se de atualizar esta função!")
        };
    }

    // function to get the FilePath.
    private static string GetFilePath(DataBaseType baseType) => GetDataBaseInfo(baseType).path;

    // Desenha uma barra de progresso simples no terminal
    private static void DrawProgressBar(int value, int max)
    {
        int totalBlocks = 50; // Tamanho da barra (número de blocos)
        int filledBlocks = (int)Math.Round((double)value / max * totalBlocks); // Percentagem preenchida

        // Cria um visual da barra com '#' e '-'
        string bar = "[" + new string('#', filledBlocks) + new string('-', totalBlocks - filledBlocks) + $"] {value * 100 / max}% ";

        CursorLeft = 0; // Reposiciona o cursor para sobrescrever a linha anterior
        Write(bar);     // Escreve a barra atualizada
    }

    /// <summary> Verifica se o ficheiro contém JSON válido. </summary>
    private static bool IsValidJson(string json)
    {
        try { JsonDocument.Parse(json); return true; }
        catch { return false; }
    }

    /// <summary> Verifica se todos os ficheiros necessários existem e cria/corrige os que faltam. </summary>
    /// <param name="setup">True para mostrar strings de progresso/status, false para esconder.</param>
    /// <returns>true se não houver problemas graves irrecuperáveis, false se houver falhas críticas.</returns>
    internal static bool StartupCheckFilesWithProgress(bool setup = true)
    {
        //Listas para guardar ficheiros usados pela função
        var missing = new List<string>();
        var fixedJsons = new List<string>();
        var failedJsons = new List<string>();

        // Define que se houver falha no I/O, a flag de erro grave é ativada.
        bool ioErrorOccurred = false;

        var dirs = GetAllDirectories().ToList();
        var files = AllFiles.ToList();

        int total = dirs.Count + files.Count;
        int count = 0;

        // Tenta criar o diretório de backup
        string backupDir = InfrastructureDirectories["Backup"];
        try { Directory.CreateDirectory(backupDir); }
        catch (Exception ex)
        {
            if (setup) WriteLine($"\n❌ Erro crítico: Não foi possível criar o diretório de backup {backupDir}. {ex.Message}");
            ioErrorOccurred = true;
        }

        if (setup) WriteLine("A verificar diretórios e ficheiros...");

        // 1) Criar diretórios
        foreach (var dir in dirs)
        {
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    missing.Add(dir);
                }
            }
            catch (Exception ex)
            {
                if (setup) WriteLine($"\n❌ Falha ao criar diretório {dir}: {ex.Message}");
                ioErrorOccurred = true;
            }

            count++;
            DrawProgressBar(count, total);
            Thread.Sleep(100);
        }

        // 2) Criar/verificar ficheiros
        foreach (var file in files)
        {

            try
            {
                // Obtém o caminho do diretório onde o ficheiro deve residir.
                string? parent = Path.GetDirectoryName(file);

                // Verifica se o caminho do diretório pai é válido (não é nulo nem espaços em branco).
                if (!string.IsNullOrWhiteSpace(parent))
                    // Tenta criar o diretório pai. Se já existir, este método não faz nada.
                    Directory.CreateDirectory(parent);
            }
            catch (Exception ex)
            {
                WriteLine($"\n❌ Falha ao garantir o diretório pai de {file}: {ex.Message}");
                ioErrorOccurred = true;
            }

            // Tenta criar ficheiros inexistentes
            if (!File.Exists(file))
            {
                try
                {
                    if (file.EndsWith(".json")) File.WriteAllText(file, "{}");
                    else File.WriteAllText(file, "");
                    missing.Add(file);
                }
                catch (Exception ex)
                {
                    if (setup) WriteLine($"\n❌ Falha ao criar ficheiro {file}: {ex.Message}");
                    ioErrorOccurred = true;
                }
            }
            // Verificar JSON para ficheiros existentes
            else if (file.EndsWith(".json"))
            {
                string content = "";
                bool readSuccess = false;

                try
                {
                    content = File.ReadAllText(file).Trim();
                    readSuccess = true;
                }
                catch (Exception ex)
                {
                    WriteLine($"\n❌ Falha ao ler ficheiro {file}: {ex.Message}");
                    ioErrorOccurred = true;
                }


                if (readSuccess)
                {
                    // Lógica original de correção (se estiver vazio OU inválido)
                    if (string.IsNullOrWhiteSpace(content) || !IsValidJson(content))
                    {
                        if (!ioErrorOccurred) // Se não houver erros anteriores, tenta corrigir
                        {
                            // 1) Fazer backup antes de corrigir se for inválido e não apenas vazio
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                WriteLine($"\n❌ JSON inválido → {file}");

                                try
                                {
                                    string fileName = Path.GetFileName(file);
                                    string backupName = Path.Combine(
                                        backupDir,
                                        $"{fileName}.bak_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}"
                                    );

                                    File.Copy(file, backupName, false);
                                    WriteLine($"📦 Backup criado → {backupName}");

                                    // Adiciona à lista de falhas graves
                                    failedJsons.Add(file);
                                }
                                catch (Exception ex)
                                {
                                    WriteLine($"\n❌ Falha ao criar backup para {file}: {ex.Message}");
                                    ioErrorOccurred = true;
                                }
                            }

                            // 2) Corrigir/Recriar o ficheiro
                            try
                            {
                                File.WriteAllText(file, "{}");
                                fixedJsons.Add(file);
                            }
                            catch (Exception ex)
                            {
                                WriteLine($"\n❌ Falha ao reescrever ficheiro {file}: {ex.Message}");
                                ioErrorOccurred = true;
                            }
                        }
                    }
                }
            }

            count++;
            DrawProgressBar(count, total);
            Thread.Sleep(50);
        }
        // 3) Mensagem final
        WriteLine();

        if (missing.Count > 0 && setup)
        {
            WriteLine("📄 Itens Criados:");
            missing.ForEach(m => WriteLine(" - " + m));
        }

        if (fixedJsons.Count > 0)
        {
            WriteLine("\n🔧 JSON corrigidos (inicializados com {}):");
            fixedJsons.ForEach(j => WriteLine(" - " + j));
        }

        if (failedJsons.Count > 0)
        {
            WriteLine("\n⚠ JSONs corrompidos (backup criado e ficheiro recriado):");
            failedJsons.ForEach(j => WriteLine(" - " + j));
        }

        if (setup)
        {
            if (ioErrorOccurred)
            {
                // Mensagem de alerta de erro grave
                WriteLine("\n\n🔴 ════════════ ERRO GRAVE DE INICIALIZAÇÃO ════════════ 🔴");
                WriteLine("   Falha Crítica no I/O: Alguma(s) operações falharam .");
                WriteLine("⛔ NÃO USE O PROGRAMA! Ele não está em condições estáveis.Espera por uma nova versão.");
                WriteLine("🔴 ══════════════════════════════════════════════════ 🔴\n");
            }
            else if (failedJsons.Count > 0)
            {
                WriteLine("\n⚠ Atenção: Alguns JSON foram reconstruídos/corrigidos. Consulte os backups se necessário.");
            }
            else
            {
                WriteLine("\n✅ Tudo OK.");
            }
        }
        // Retorna false apenas se tiver havido falhas graves de I/O que impeçam a operação.
        return !ioErrorOccurred;
    }

    //-----------------------

    //  Lê o conteúdo de um ficheiro, devolvendo "{}" se não existir
    internal static string ReadFile(string path) { return File.Exists(path) ? File.ReadAllText(path) : "{}"; }


    //Cache do JsonSerializerOptions | Declaramos uma instância estática e readonly para ser reutilizada. Isso evita alocações desnecessárias a cada chamada de WriteOnDataBase.
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };// Mantém a formatação legível
    // Esta função assume que o objeto já contém um ID válido.| Serve para adicionar ou atualizar no ficheiro JSON.
    internal static void WriteOnDataBase<T>(DataBaseType baseType, T obj)
    {
        string path = GetFilePath(baseType);
        string json = ReadFile(path);

        // Nota: As opções de indentação (WriteIndented) não afetam a desserialização, mas é mais seguro usar a mesma instância se houver outras opções definidas.
        var dict = string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<Dictionary<int, T>>(json, JsonOptions) ?? [];

        // Obtém ID via reflection
        // A propriedade ID_i precisa ser pública, ou o BindingFlags deve ser ajustado para NonPublic.
        var idProperty = typeof(T).GetProperty("ID_i", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"A classe {typeof(T).Name} não contém a propriedade 'ID_i'.");

        int id = (int)(idProperty.GetValue(obj) ?? -1);
        //WriteLine($"DEBUG(FileManager.WriteOnDataBase): ID do objeto = {id}");

        if (id < 0) throw new InvalidOperationException("O ID do objeto é inválido.");

        // Adiciona / atualiza
        dict[id] = obj;
        // WriteLine($"DEBUG: Objeto ID {id} adicionado/atualizado.");

        // Usar a instância em cache para Serializar
        string updatedJson = JsonSerializer.Serialize(dict, JsonOptions);

        File.WriteAllText(path, updatedJson);

        //WriteLine($"DEBUG: Base de dados '{baseType}' atualizada com sucesso.");
    }

    //-----------------------

    // Método auxiliar genérico que calcula o próximo ID disponível num ficheiro JSON.
    private static int GetNextAvailableIDFromFile<T>(string filePath)
    {
        string json = ReadFile(filePath);// Lê o conteúdo do ficheiro JSON indicado
        if (string.IsNullOrWhiteSpace(json)) { return 0; }// Caso o ficheiro esteja vazio, nulo ou só com espaços, não há dados — começa do ID 0

        // Desserializa o JSON num dicionário de pares [ID → objeto T]
        var dict = JsonSerializer.Deserialize<Dictionary<int, T>>(json);// O tipo genérico <T> torna este método reutilizável para qualquer classe de dados. Exemplo: se T for Student, ficamos com Dictionary<int, Student>
        if (dict == null || dict.Count == 0) { return 0; }// Se o ficheiro estiver vazio ou o JSON não tiver entradas válidas, retorna 0 como o primeiro ID disponível.

        // Enquanto o ID atual existir no dicionário, incrementa para procurar o próximo livre. Esta lógica garante que se houver "buracos" (ex: IDs 0,1,3,4), o método retorna 2.
        int nextID = 0;
        while (dict.ContainsKey(nextID)) { nextID++; }
        return nextID;//  Retorna o menor ID que ainda não foi usado.
    }

    // Função principal para obter o próximo ID disponível
    // O retorno é INT (o próximo ID disponível)
    internal static int GetTheNextAvailableID(DataBaseType baseType)
    {
        //Verifica se os arquivos necessários estão OK
        if (!StartupCheckFilesWithProgress(false)) return -1;

        // Obtém o CAMINHO do JSON (Retorna string path)
        string path = GetFilePath(baseType);

        // Usa um switch para mapear o DataBaseType para a chamada genérica correta.
        // O resultado da chamada GetNextAvailableIDFromFile<T>(path) é um INT.
        return baseType switch
        {
            DataBaseType.UndergraduateStudent => GetNextAvailableIDFromFile<UndergraduateStudent>(path),
            DataBaseType.GraduateStudent => GetNextAvailableIDFromFile<GraduateStudent>(path),
            DataBaseType.InternationalStudent => GetNextAvailableIDFromFile<InternationalStudent>(path),
            DataBaseType.Teacher => GetNextAvailableIDFromFile<Teacher>(path),
            DataBaseType.Course => GetNextAvailableIDFromFile<Course>(path),
            DataBaseType.Subject => GetNextAvailableIDFromFile<Subject>(path),

            _ => throw new ArgumentOutOfRangeException(nameof(baseType),
                $"Tipo de base de dados '{baseType}'é desconhecida para obter ID. verifique se adicionou o caminho nas funções!")
        };
    }

    //--------------

    // Remove um objeto de uma base de dados pelo ID
    internal static bool RemoveById<T>(DataBaseType baseType, int id) where T : BaseEntity
    {
        try
        {
            string path = GetFilePath(baseType);

            if (!File.Exists(path))
            {
                WriteLine("[DEBUG] Arquivo não existe!");
                return false;
            }

            string json = File.ReadAllText(path);

            Dictionary<string, T>? dict = JsonSerializer.Deserialize<Dictionary<string, T>>(json);
            if (dict == null)
            {
                WriteLine("[DEBUG] Dicionário nulo!");
                return false;
            }

            string keyToRemove = dict.FirstOrDefault(kvp =>
            {
                try
                {
                    PropertyInfo? prop = kvp.Value?.GetType().GetProperty("ID_i", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (prop == null) { return false; }

                    object? valObj = prop.GetValue(kvp.Value);
                    return valObj != null && Convert.ToInt32(valObj) == id;
                }
                catch (Exception ex)
                {
                    WriteLine($"[DEBUG] Erro ao ler propriedade: {ex.Message}");
                    return false;
                }
            }).Key;

            if (keyToRemove != null)
            {
                // Backup
                string backupDir = InfrastructureDirectories["Backup"];
                _ = Directory.CreateDirectory(backupDir);
                string backupPath = Path.Combine(backupDir, $"{baseType}.txt");

                string backupEntry = JsonSerializer.Serialize(dict[keyToRemove]);
                File.AppendAllText(backupPath, backupEntry + Environment.NewLine);

                // Remove e salva
                _ = dict.Remove(keyToRemove);
                File.WriteAllText(path, JsonSerializer.Serialize(dict, JsonOptions));

                return true;
            }

            WriteLine("[DEBUG] Nenhum objeto correspondente encontrado.");
            return false;
        }
        catch (Exception ex)
        {
            WriteLine($"[ERROR] RemoveById falhou: {ex.Message}");
            return false;
        }
    }

    //--------------------
    // função, caso uma base da dados esteja corrompida, não deixa o programa ir abaixo.
    private static Dictionary<string, T> SafeReadDatabase<T>(string path)
    {
        if (!File.Exists(path))
        {
            WriteLine($"[DEBUG] Arquivo não existe: {path}");
            return [];
        }

        string json = File.ReadAllText(path);

        // Tenta desserializar normalmente
        try
        {
            Dictionary<string, T>? dict = JsonSerializer.Deserialize<Dictionary<string, T>>(json);
            if (dict != null)
            {
                return dict;
            }

            WriteLine($"[DEBUG] Dicionário vazio após desserialização: {path}");
            return [];
        }
        catch (JsonException ex)
        {
            WriteLine($"[WARNING] JSON inválido detectado em {path}: {ex.Message}");
        }

        // Tentativa de reparo simples: remove vírgulas iniciais ou finais, linhas em branco
        string cleaned = string.Join("\n",
            json.Split('\n')
                .Select(static line => line.Trim())
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(static line => line.TrimStart(',').TrimEnd(','))
        );

        try
        {
            Dictionary<string, T>? dict = JsonSerializer.Deserialize<Dictionary<string, T>>(cleaned);
            if (dict != null)
            {
                WriteLine($"[INFO] JSON reparado com sucesso: {path}");
                return dict;
            }
        }
        catch
        {
            WriteLine($"[ERROR] Falha ao reparar JSON em {path}. Base vazia retornada.");
        }

        return [];
    }

    /// <summary>Restricted Search Engine
    /// Realiza uma pesquisa na base de dados correspondente,permitindo procurar objetos por nome e/ou ID. Indica também se a base de dados está vazia.
    /// </summary>
    /// <typeparam name="T"> Tipo do objeto armazenado, devendo herdar de <see cref="BaseEntity"/>. </typeparam>
    /// <param name="baseType">
    /// Tipo da base de dados selecionada (ex.: Student, Teacher, Course).
    /// Determina qual ficheiro JSON será lido.
    /// </param>
    /// <param name="isEmpty">
    /// Parâmetro por referência que é definido como <c>true</c> caso a base de dados esteja vazia ou não possa ser carregada.
    /// Caso contrário mantém-se <c>false</c>.
    /// </param>
    /// <param name="name">
    /// Nome a procurar. Se for <c>null</c>, vazio ou apenas espaços, o filtro por nome é ignorado ou adaptado conforme a lógica interna.
    /// A pesquisa é parcial e insensível a maiúsculas/minúsculas.
    /// </param>
    /// <param name="id">ID a procurar. Se <c>null</c>, o filtro por ID é ignorado. </param>
    /// <returns>
    /// Lista de objetos encontrados após aplicar os filtros.
    /// Retorna uma lista vazia caso nenhum item satisfaça os critérios ou caso a base de dados esteja vazia.
    /// </returns>
    internal static List<T> Search<T>(DataBaseType baseType, ref bool isEmpty, string? name = null, int? id = null) where T : BaseEntity
    {
        string path = GetFilePath(baseType);
        // Lê a base de dados de forma segura

        Dictionary<string, T> dict = SafeReadDatabase<T>(path);

        if (dict.Count == 0)
        {
            WriteLine($"[DEBUG] Base de dados '{baseType}' está vazia ou não pôde ser carregada.");
            isEmpty = true;
            return [];
        }

        IEnumerable<T> list = dict.Values;

        // --- Filtro por nome ---
        if (name != null)
        {
            string trimmed = name.Trim();
            list = trimmed == ""
                ? list.Where(x =>
                {
                    if (x == null)
                    {
                        WriteLine("[DEBUG] Objeto nulo na lista de valores.");
                        return false;
                    }

                    if (x.Name_s == null)
                    {
                        WriteLine($"[DEBUG] Objeto ID={x.ID_i} tem Name_s nulo.");
                        return false;
                    }
                    return x.Name_s == "";
                })
                : list.Where(x =>
                {
                    if (x == null)
                    {
                        WriteLine("[DEBUG] Objeto nulo na lista de valores.");
                        return false;
                    }
                    if (x.Name_s == null)
                    {
                        WriteLine($"[DEBUG] Objeto ID={x.ID_i} tem Name_s nulo.");
                        return false;
                    }
                    return x.Name_s.Contains(trimmed, StringComparison.OrdinalIgnoreCase);
                });
        }

        // --- Filtro por ID ---
        if (id.HasValue)
        {
            list = list.Where(x =>
            {
                if (x == null)
                {
                    WriteLine("[DEBUG] Objeto nulo na lista de valores.");
                    return false;
                }
                return x.ID_i == id.Value;
            });
        }

        List<T> result = [.. list];
        WriteLine($"[DEBUG] Pesquisa retornou {result.Count} objeto(s).");
        return result;
    }

    // Exemplo de uma função dedicada (fora do escopo de Search<T>)
    internal static List<T> GetAll<T>(DataBaseType baseType) where T : BaseEntity
    {
        // Apenas obtém o caminho e chama SafeReadDatabase.
        string path = GetFilePath(baseType);
        Dictionary<string, T> dict = SafeReadDatabase<T>(path);

        //WriteLine($"[DEBUG] Função GetAll retornou {dict.Count} objeto(s) de '{baseType}'.");
        return [.. dict.Values];
    }
}

