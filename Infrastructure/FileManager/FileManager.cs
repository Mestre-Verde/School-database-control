/// <summary>
/// Nesta class ela é responsável por tudo que seja relacionado com a base de dados, nenhuma class sem ser esta pode manusiar nos ficheiros.
/// Tambem é responsavel por verificar se os ficheiros existem 
/// Também deve estar apar de todos os ficherios existentes, tendo o caminho como uma variavel.
/// Esta class usa bastante acesso LINQ para percorrer JSONs e dicionários com Caminhos(PATHs).
/// </summary>
namespace School_System.Infrastructure.FileManager;

using static System.Console;
using System.Text.Json;
using System.Reflection;
using System.Text.RegularExpressions;

using School_System.Domain.Base;
using School_System.Domain.SchoolMembers;
using School_System.Domain.CourseProgram;

// Classe estática — não precisa ser instanciada
public static class FileManager
{
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

    // Combina todos os diretórios definidos nos dicionários individuais e devolve como uma sequência (IEnumerable<string>) de caminhos.
    private static IEnumerable<string> AllDirectories =>
        DomainDirectories.Values          // Pega todos os valores do dicionário DomainDirectories
        .Concat(ApplicationDirectories.Values)   // Concatena os valores do ApplicationDirectories
        .Concat(InfrastructureDirectories.Values); // Concatena os valores do InfrastructureDirectories

    // Retorna todos os caminhos de arquivos definidos no dicionário Files
    private static IEnumerable<string> AllFiles => Files.Values;

    public enum DataBaseType// Enum para percorrer os caminhos das base de dados.
    {
        UndergraduateStudent,
        GraduateStudent,
        InternationalStudent,
        Teacher,
        Course,
        Subject
    }
    // function to get the FilePath.
    private static string GetFilePath(DataBaseType baseType) { return GetDataBaseInfo(baseType).path; }
    // Função para devolver o caminho para uma base de dados e um Tipode de class
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

    //---------------------

    // Desenha uma barra de progresso simples no terminal
    private static void DrawProgressBar(int value, int max)
    {
        int totalBlocks = 50; // Tamanho da barra (número de blocos)
        int filledBlocks = (int)Math.Round((double)value / max * totalBlocks); // Percentagem preenchida

        // Cria visual da barra com '#' e '-'
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

    /// <summary> Tenta limpar ou reparar JSONs parcialmente corrompidos </summary>
    private static string TryCleanJson(string raw)
    {
        string c = raw.Trim().Trim('\uFEFF');

        // -------------------------
        // 1) Remover vírgula a mais
        // -------------------------
        c = Regex.Replace(c, ",\\s*}", "}");
        c = Regex.Replace(c, ",\\s*]", "]");

        // ---------------------------------------------------------
        // 2) ADICIONAR vírgula quando falta ENTRE OBJECTOS 
        // ---------------------------------------------------------
        // "}" imediatamente seguido de aspas → falta vírgula
        c = Regex.Replace(c, @"}\s*""", "},\n\"");

        // "]" seguido de aspas → falta vírgula num array
        c = Regex.Replace(c, @"]\s*""", "],\n\"");

        // ---------------------------------------------------------
        // 3) Fechar objetos/arrays que ficaram abertos
        // ---------------------------------------------------------
        int openObj = c.Count(ch => ch == '{');
        int closeObj = c.Count(ch => ch == '}');
        if (openObj > closeObj)
            c += "}";

        int openArr = c.Count(ch => ch == '[');
        int closeArr = c.Count(ch => ch == ']');
        if (openArr > closeArr)
            c += "]";

        // ---------------------------------------------------------
        // 4) Se isto não parece JSON → embrulhar em { }
        // ---------------------------------------------------------
        if (!c.StartsWith("{") && !c.StartsWith("["))
            c = "{" + c;

        if (!c.EndsWith("}") && !c.EndsWith("]"))
            c += "}";

        // ---------------------------------------------------------
        // 5) fallback: ainda inválido → return "{}"
        // ---------------------------------------------------------
        if (!IsValidJson(c))
            c = "{}";

        return c;
    }


    /// <summary> Verifica se todos os ficheiros necessários existem e cria/corrige os que faltam </summary>
    /// <param name="setup">True para mostrar strings, false para esconder</param>
    /// <returns>true se não houver problemas graves, false se houver um problema com os ficheiros</returns>
    internal static bool StartupCheckFilesWithProgress(bool setup = true)
    {
        var missing = new List<string>();
        var fixedJsons = new List<string>();
        var failedJsons = new List<string>();

        var dirs = AllDirectories.ToList();
        var files = AllFiles.ToList();

        int total = dirs.Count + files.Count;
        int count = 0;

        string backupDir = InfrastructureDirectories["Backup"];
        Directory.CreateDirectory(backupDir);

        if (setup) WriteLine("A verificar diretórios e ficheiros...");

        // 1) Criar diretórios
        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                missing.Add(dir);
            }

            count++;
            DrawProgressBar(count, total);
            Thread.Sleep(10);
        }

        // 2) Criar/verificar ficheiros
        foreach (var file in files)
        {
            string? parent = Path.GetDirectoryName(file);
            if (!string.IsNullOrWhiteSpace(parent))
                Directory.CreateDirectory(parent);

            // Criar ficheiros inexistentes
            if (!File.Exists(file))
            {
                if (file.EndsWith(".json"))
                    File.WriteAllText(file, "{}");
                else
                    File.WriteAllText(file, "");

                missing.Add(file);
            }
            // Verificar JSON
            else if (file.EndsWith(".json"))
            {
                string content = File.ReadAllText(file).Trim();

                if (string.IsNullOrWhiteSpace(content))
                {
                    File.WriteAllText(file, "{}");
                    fixedJsons.Add(file);
                }
                else if (!IsValidJson(content))
                {
                    WriteLine($"\n❌ JSON inválido → {file}");

                    // ------------------------------------------------------
                    // 1) Criar backup sempre antes de tocar no ficheiro
                    // ------------------------------------------------------
                    string fileName = Path.GetFileName(file);
                    string backupName = Path.Combine(
                        backupDir,
                        $"{fileName}.bak_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}"
                    );

                    File.Copy(file, backupName, overwrite: false);
                    WriteLine($"📦 Backup criado → {backupName}");

                    // ------------------------------------------------------
                    // 2) Tentar correções automáticas
                    // ------------------------------------------------------
                    string cleaned = TryCleanJson(content);

                    if (IsValidJson(cleaned))
                    {
                        File.WriteAllText(file, cleaned);
                        fixedJsons.Add(file);
                        WriteLine($"✔ Corrigido automaticamente → {file}");
                    }
                    else
                    {
                        // ------------------------------------------------------
                        // 3) Se não deu → reconstruir novo JSON mínimo
                        // (mas o original está guardado em backup)
                        // ------------------------------------------------------
                        File.WriteAllText(file, "{}");
                        failedJsons.Add(file);
                        WriteLine($"⚠ ficheiro em esatdo crítico. Novo JSON criado → {file}");
                    }
                }
            }

            count++;
            DrawProgressBar(count, total);
            Thread.Sleep(100);
        }
        // 3) Mensagem final
        WriteLine();

        if (missing.Count > 0 && setup)
        {
            WriteLine("📄 Criados:");
            missing.ForEach(m => WriteLine(" - " + m));
        }

        if (fixedJsons.Count > 0)
        {
            WriteLine("\n🔧 JSON corrigidos:");
            fixedJsons.ForEach(j => WriteLine(" - " + j));
        }

        if (failedJsons.Count > 0)
        {
            WriteLine("\n⚠ JSONs gravemente corrompidos (backup criado):");
            failedJsons.ForEach(j => WriteLine(" - " + j));
        }

        if (setup)
        {
            if (failedJsons.Count == 0)
                WriteLine("\n✅ Tudo OK.");
            else
                WriteLine("\n⚠ Atenção: Alguns JSON foram reconstruídos. Consulte os backups.");
        }

        return failedJsons.Count == 0;
    }

    //-----------------------

    //  Lê o conteúdo de um ficheiro, devolvendo "{}" se não existir
    internal static string ReadFile(string path) { return File.Exists(path) ? File.ReadAllText(path) : "{}"; }

    // Esta função assume que o objeto já contém um ID válido.
    // Serve apenas para adicionar ou atualizar no ficheiro JSON.
    internal static void WriteOnDataBase<T>(DataBaseType baseType, T obj)
    {
        string path = GetFilePath(baseType);
        string json = ReadFile(path);

        // Desserializa ou cria dicionário vazio
        var dict = string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<Dictionary<int, T>>(json) ?? [];

        // Obtém ID via reflection
        var idProperty = typeof(T).GetProperty("ID_i",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"A classe {typeof(T).Name} não contém a propriedade 'ID_i'.");

        int id = (int)(idProperty.GetValue(obj) ?? -1);
        WriteLine($"DEBUG(FileManager.WriteOnDataBase): ID do objeto = {id}");

        if (id < 0) throw new InvalidOperationException("O ID do objeto é inválido.");

        // Adiciona / atualiza
        dict[id] = obj;
        WriteLine($"DEBUG: Objeto ID {id} adicionado/atualizado.");

        // Serializa para JSON
        var options = new JsonSerializerOptions { WriteIndented = true };
        string updatedJson = JsonSerializer.Serialize(dict, options);

        File.WriteAllText(path, updatedJson);

        WriteLine($"DEBUG: Base de dados '{baseType}' atualizada com sucesso.");
    }

    //-----------------------

    // Método auxiliar genérico que calcula o próximo ID disponível num ficheiro JSON
    // <T> permite reutilizar o mesmo código para Student, Teacher, Course, etc.
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

    internal static int GetTheNextAvailableID(DataBaseType baseType)
    {
        if (!StartupCheckFilesWithProgress(false)) return -1;

        var (path, type) = GetDataBaseInfo(baseType);

        var method = typeof(FileManager)
            .GetMethod(nameof(GetNextAvailableIDFromFile), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(type);

        return (int)method.Invoke(null, new object[] { path })!;
    }    //-----------------------

    //--------------

    // Remove pelo enum e ID
    internal static bool RemoveById<T>(DataBaseType baseType, int id)
    {
        string path = GetFilePath(baseType);
        WriteLine($"[DEBUG] Tentando remover ID={id} do arquivo {path}");

        if (!File.Exists(path))
        {
            WriteLine("[DEBUG] Arquivo não existe!");
            return false;
        }

        string json = File.ReadAllText(path);
        var dict = JsonSerializer.Deserialize<Dictionary<string, T>>(json);
        if (dict == null)
        {
            WriteLine("[DEBUG] Dicionário nulo!");
            return false;
        }

        var keyToRemove = dict.FirstOrDefault(kvp =>
        {
            var prop = kvp.Value?.GetType().GetProperty(
                "ID_i", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );
            if (prop == null) return false;

            var valObj = prop.GetValue(kvp.Value);
            return valObj != null && Convert.ToInt32(valObj) == id;

        }).Key;
        if (keyToRemove != null)
        {
            // --- Backup sem indentação ---
            string backupDir = InfrastructureDirectories["Backup"]; // "Infrastructure/Data/backup"
            Directory.CreateDirectory(backupDir); // garante que a pasta existe

            string backupPath = Path.Combine(backupDir, $"{baseType}.txt"); // caminho completo do backup
            string backupEntry = JsonSerializer.Serialize(dict[keyToRemove]); // sem WriteIndented
            File.AppendAllText(backupPath, backupEntry + Environment.NewLine);

            // --- Remove do arquivo principal (mantendo indentação) ---
            dict.Remove(keyToRemove);
            File.WriteAllText(path, JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true }));

            return true;
        }
        WriteLine("[DEBUG] Nenhum objeto correspondente encontrado.");
        return false;
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
            var dict = JsonSerializer.Deserialize<Dictionary<string, T>>(json);
            if (dict != null) return dict;
            WriteLine($"[DEBUG] Dicionário vazio após desserialização: {path}");
            return new Dictionary<string, T>();
        }
        catch (JsonException ex)
        {
            WriteLine($"[WARNING] JSON inválido detectado em {path}: {ex.Message}");
        }

        // Tentativa de reparo simples: remove vírgulas iniciais ou finais, linhas em branco
        string cleaned = string.Join("\n",
            json.Split('\n')
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.TrimStart(',').TrimEnd(','))
        );

        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, T>>(cleaned);
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

        return new Dictionary<string, T>();
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
        var dict = SafeReadDatabase<T>(path);

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
            if (trimmed == "")
            {
                list = list.Where(x =>
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
                });
            }
            else
            {
                list = list.Where(x =>
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

        var result = list.ToList();
        WriteLine($"[DEBUG] Pesquisa retornou {result.Count} objeto(s).");
        return result;
    }

    // Exemplo de uma função dedicada (fora do escopo de Search<T>)
    internal static List<T> GetAll<T>(DataBaseType baseType) where T : BaseEntity
    {
        // Apenas obtém o caminho e chama SafeReadDatabase.
        string path = GetFilePath(baseType);
        var dict = SafeReadDatabase<T>(path);

        WriteLine($"[DEBUG] Função GetAll retornou {dict.Count} objeto(s) de '{baseType}'.");
        return [.. dict.Values];
    }
}

