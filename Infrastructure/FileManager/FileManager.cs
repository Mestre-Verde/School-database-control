/// <summary>
/// Nesta class ela é responsável por tudo que seja relacionado com a base de dados, nenhuma class sem ser esta pode manusiar nos ficheiros.
/// Tambem é responsavel por verificar se os ficheiros existem 
/// Também deve estar apar de todos os ficherios existentes, tendo o caminho como uma variavel.
/// </summary>
namespace School_System.Infrastructure.FileManager;

using static System.Console;
using System.Text.Json;
using System.Reflection;
using System.Linq.Expressions;

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
        { "Interfaces",          "Domain/Interfaces" }
    };
    private static readonly Dictionary<string, string> ApplicationDirectories = new()
    {
        { "Application",         "Application" },
        { "Menu",                "Application/Menu" }
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
        // Domain
        { "BaseEntity",          "Domain/Base/BaseEntity.cs" },
        { "SchoolMember",        "Domain/SchoolMembers/SchoolMember.cs" },
        { "Student",             "Domain/SchoolMembers/Student.cs" },
        { "Teacher",             "Domain/SchoolMembers/Teacher.cs" },
        { "Course",              "Domain/CourseProgram/Course.cs" },
        { "Subject",             "Domain/CourseProgram/Subject.cs" },

        // Infrastructure
        { "FileManager",         "Infrastructure/FileManager/FileManager.cs" },

        // Data
        { "StudentsJSON",        "Infrastructure/Data/students.json" },
        { "TeachersJSON",        "Infrastructure/Data/teachers.json" },
        { "CoursesJSON",         "Infrastructure/Data/courses.json" },
        { "SubjectsJSON",        "Infrastructure/Data/subjects.json" }
    };



    public enum DataBaseType// Enum para percorrer os caminhos das base de dados.
    {
        Student,
        Teacher,
        Course,
        Subject
    }
    // function to get the FilePath.
    private static string GetFilePath(DataBaseType baseType) { return GetDataBaseInfo(baseType).path; }
    // function to... return the path of the base type
    private static (string path, Type type) GetDataBaseInfo(DataBaseType baseType)
    {
        return baseType switch
        {
            DataBaseType.Student => (Files["StudentsJSON"], typeof(Student)),
            DataBaseType.Teacher => (Files["TeachersJSON"], typeof(Teacher)),
            DataBaseType.Course => (Files["CoursesJSON"], typeof(Course)),
            DataBaseType.Subject => (Files["SubjectsJSON"], typeof(Subject)), // nome correto da tua classe

            _ => throw new ArgumentOutOfRangeException(nameof(baseType),
                $"Base de dados não encontrada para o valor recebido: {baseType}. " +
                "Se adicionou um novo DataBaseType, lembre-se de atualizar esta função!")
        };
    }

    // Desenha uma barra de progresso simples no terminal
    private static void DrawProgressBar(int value, int max)
    {
        int totalBlocks = 50; // Tamanho da barra (número de blocos)
        int filledBlocks = (int)Math.Round((double)value / max * totalBlocks); // Percentagem preenchida

        // Cria visual da barra com '#' e '-'
        string bar = "[" +
            new string('#', filledBlocks) +
            new string('-', totalBlocks - filledBlocks) +
            $"] {value * 100 / max}% ";

        CursorLeft = 0; // Reposiciona o cursor para sobrescrever a linha anterior
        Write(bar);     // Escreve a barra atualizada
    }

    /// <summary>     Verifica se o ficheiro contém JSON válido.  </summary>
    private static bool IsValidJson(string json)
    {
        try { JsonDocument.Parse(json); return true; }
        catch { return false; }
    }

    /// <summary>  Verifica se todos os ficheiros necessários existem e cria os que faltam </summary>
    /// <param name="setup">True para mostrar strings, false para esconder</param>
    /// <returns></returns>
    internal static bool StartupCheckFilesWithProgress(bool setup = true)
    {
        var missing = new List<string>();
        bool error = false;

        var dirs = AllDirectories.ToList();
        var files = AllFiles.ToList();

        int total = dirs.Count + files.Count;
        int count = 0;

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
            Thread.Sleep(100); // 0,1s de delay para visualização
        }

        // 2) Criar ficheiros
        foreach (var file in files)
        {
            string? parent = Path.GetDirectoryName(file);
            if (!string.IsNullOrWhiteSpace(parent))
                Directory.CreateDirectory(parent);

            if (!File.Exists(file))
            {
                // JSON → criar {}
                if (file.EndsWith(".json"))
                    File.WriteAllText(file, "{}");
                else
                    File.WriteAllText(file, ""); // cs ou outros

                missing.Add(file);
            }
            else
            {
                // Validar JSON
                if (file.EndsWith(".json"))
                {
                    string content = File.ReadAllText(file).Trim();

                    if (string.IsNullOrWhiteSpace(content))
                    {
                        WriteLine($"\n⚠ JSON vazio corrigido → {file}");
                        File.WriteAllText(file, "{}");
                    }
                    else if (!IsValidJson(content))
                    {
                        WriteLine($"\n❌ JSON inválido → {file}");
                        error = true;
                    }
                }
            }

            count++;
            DrawProgressBar(count, total);
            Thread.Sleep(100); // 0,1s de delay para visualização
        }

        // Relatório final
        if (missing.Count > 0 && setup)
        {
            WriteLine("\n📄 Criados:");
            foreach (var m in missing)
                WriteLine(" - " + m);
        }
        else if (setup)
        {
            WriteLine("\n✅ Tudo OK.");
        }

        return !error;
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
            ? new Dictionary<int, T>()
            : JsonSerializer.Deserialize<Dictionary<int, T>>(json) ?? new Dictionary<int, T>();

        // Obtém ID via reflection
        var idProperty = typeof(T).GetProperty("ID_i",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"A classe {typeof(T).Name} não contém a propriedade 'ID_i'.");

        int id = (int)(idProperty.GetValue(obj) ?? -1);
        WriteLine($"DEBUG(FileManager.WriteOnDataBase): ID do objeto = {id}");

        if (id < 0)
            throw new InvalidOperationException("O ID do objeto é inválido.");

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

        var info = GetDataBaseInfo(baseType);

        var method = typeof(FileManager)
            .GetMethod(nameof(GetNextAvailableIDFromFile), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(info.type);

        return (int)method.Invoke(null, new object[] { info.path })!;
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
            return new Dictionary<string, T>();
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

    /// <summary> Restricted Search Engine
    /// Pesquisa objetos em uma base de dados pelo nome ou ID.
    /// </summary>
    /// <typeparam name="T">Tipo do objeto armazenado.</typeparam>
    /// <param name="baseType">Base de dados selecionada (Student, Teacher ou Course).</param>
    /// <param name="name">Nome a procurar. Ignorado se for nulo, vazio ou espaço.</param>
    /// <param name="id">ID a procurar. Ignorado se for nulo.</param>
    /// <returns>Lista de objetos encontrados.</returns>
    internal static List<T> Search<T>(DataBaseType baseType, string? name = null, int? id = null) where T : BaseEntity
    {
        string path = GetFilePath(baseType);

        // Lê a base de dados de forma segura
        var dict = SafeReadDatabase<T>(path);

        if (dict.Count == 0)
        {
            WriteLine($"[DEBUG] Base de dados '{baseType}' está vazia ou não pôde ser carregada.");
            return new List<T>();
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

}
