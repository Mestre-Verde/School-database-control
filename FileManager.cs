/// <summary>
/// Nesta class ela é responsável por tudo que seja relacionado com a base de dados, nenhuma class sem ser esta pode manusiar nos ficheiros.
/// Tambem é responsavel por verificar se os ficheiros existem 
/// </summary>
using static System.Console; // <-- Permite usar Write e WriteLine sem precisar de Console.
using System.Text.Json;
using System.Reflection;
using System.Linq.Expressions;


// Classe estática — não precisa ser instanciada
internal static class FileManager
{
    // 🗂️ Caminhos dos ficheiros principais do Programa
    private static string MainAbstractClassDirectory { get; } = "Class_1st_degree";
    private static string SecundAbstractClassDirectory { get; } = "Class_2nd_degree";
    private static string SchoolMembersDirectory { get; } = "schoolMembers";
    private static string DataBaseDirectory { get; } = "data/";               // Pasta onde ficam os ficheiros de dados
    private static string CourseDirectory { get; } = "courseProgram/";
    private static readonly string[] directorys = [
        MainAbstractClassDirectory,
        SecundAbstractClassDirectory,
        CourseDirectory,
        DataBaseDirectory,
        SchoolMembersDirectory,
    ];
    //------------------
    private static string SchoolMemberFilePath { get; } = "Class_2nd_degree/SchoolMember.cs";// Código fonte para memebros da escola
    private static string StudentFilePath { get; } = "schoolMembers/Student.cs";
    private static string TeacherFilePath { get; } = "schoolMembers/Teacher.cs";
    private static string CourseFilePath { get; } = "courseProgram/Course.cs";                 // Código fonte de cursos
    private static string DisciplineFilePath { get; } = "courseProgram/Discipline.cs";
    //------------------
    private static string StudentsJSONPath { get; } = "data/students.json";      // Dados dos estudantes
    private static string TeachersJSONPath { get; } = "data/teachers.json";      // Dados dos professores
    private static string CoursesJSONPath { get; } = "data/courses.json";         // Dados dos cursos
    private static string SubjectsJSONPath { get; } = "data/subjects.json";
    private static readonly string[] files_s =
        [
            StudentFilePath,
            TeacherFilePath,
            CourseFilePath,
            SchoolMemberFilePath,
            DisciplineFilePath,

            StudentsJSONPath,
            TeachersJSONPath,
            CoursesJSONPath,
            SubjectsJSONPath
        ];

    internal enum DataBaseType// Enum para percorrer os caminhos das base de dados.
    {
        Student,
        Teacher,
        Course,
        Discipline
    }
    /// <summary> Retorna o caminho do arquivo JSON correspondente ao tipo de base de dados fornecido.</summary>
    /// <returns>
    /// Retorna uma variavel do tipo <see cref="string"/> com o caminho completo do arquivo JSON correspondente ao <paramref name="baseType"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Lançada quando o <paramref name="baseType"/> não corresponde a nenhum valor definido no enum <see cref="DataBaseType"/>.
    /// </exception>
    /*private static string GetFilePath(DataBaseType baseType)
    {
        return baseType switch
        {
            DataBaseType.Student => StudentsJSONPath,
            DataBaseType.Teacher => TeachersJSONPath,
            DataBaseType.Course => CoursesJSONPath,
            DataBaseType.Discipline => SubjectsJSONPath,

            _ => throw new ArgumentOutOfRangeException(
                nameof(baseType),
                $"Na função FileManager.GetFilePath não foi encontrada uma base de dados correspondente ao valor recebido: {baseType}. " +
                "Se adicionou um novo DataBaseType, lembre-se de incluir o caminho correspondente aqui."
            )
        };
    }*/
    private static string GetFilePath(DataBaseType baseType)
    {
        return GetDataBaseInfo(baseType).path;
    }

    private static (string path, Type type) GetDataBaseInfo(DataBaseType baseType)
    {
        return baseType switch
        {
            DataBaseType.Student => (StudentsJSONPath, typeof(Student)),
            DataBaseType.Teacher => (TeachersJSONPath, typeof(Teacher)),
            DataBaseType.Course => (CoursesJSONPath, typeof(Course)),
            DataBaseType.Discipline => (SubjectsJSONPath, typeof(Discipline)),

            _ => throw new ArgumentOutOfRangeException(nameof(baseType),
                $"Base de dados não encontrada para o valor recebido: {baseType}. " +
                "Se adicionou um novo DataBaseType, lembre-se de incluir aqui.Seu distraido!")
        };
    }

    // 🎨 Desenha uma barra de progresso simples no terminal
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

    /// <summary>
    /// Verifica se o ficheiro contém JSON válido.
    /// </summary>
    private static bool IsValidJsonFile(string path)
    {
        try
        {
            string content = ReadFile(path).Trim();
            if (string.IsNullOrWhiteSpace(content)) return false;

            JsonDocument.Parse(content);
            return true;
        }
        catch { return false; }
    }

    // Verifica se todos os ficheiros necessários existem e cria os que faltam
    internal static bool StartupCheckFilesWithProgress(bool setup = true)
    {
        int total = files_s.Length;
        int count = 0;
        List<string> missingFiles = [];

        // Variável que guarda se houve algum erro
        bool errorDetected = false;

        if (setup) WriteLine("A verificar os ficheiros...");

        // verifica se as pastas existem e, se não existirem, cria as pastas que faltarem.
        foreach (var directory in directorys)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                missingFiles.Add(directory);
            }
        }

        foreach (var file in files_s)
        {
            if (!File.Exists(file) && setup)
            {
                File.WriteAllText(file, "{}");
                missingFiles.Add(file);
            }
            else if (File.Exists(file))
            {
                string ext = Path.GetExtension(file).ToLower();

                // Validar JSON
                if (ext == ".json")
                {
                    string content = ReadFile(file).Trim();

                    // 📌 Se estiver vazio → cria {}
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        WriteLine(); // empurra barra
                        WriteLine($"⚠️ Aviso: ficheiro JSON vazio corrigido -> {file}");
                        File.WriteAllText(file, "{}");
                    }
                    else
                    {
                        // 📌 Se estiver inválido → regista erro
                        if (!IsValidJsonFile(file))
                        {
                            WriteLine(); // empurra barra
                            WriteLine($"❌ Erro: ficheiro JSON inválido -> {file}");
                            errorDetected = true;
                        }
                    }
                }
                else if (ext == ".cs") { }
            }
            count++;
            DrawProgressBar(count, total);
            Thread.Sleep(200);
            if (setup)
            {

            }
        }

        if (missingFiles.Count > 0 && setup)
        {
            WriteLine("\n📄 Ficheiros criados:");
            foreach (var f in missingFiles) WriteLine(" - " + f);
        }
        else if (setup) { WriteLine("\n✅ Nenhum ficheiro em falta."); }

        // Se algum ficheiro inválido foi encontrado → devolve false
        return !errorDetected;
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
            string backupPath = Path.Combine("backup", $"{baseType}.txt");
            Directory.CreateDirectory("backup"); // garante pasta

            string backupEntry = JsonSerializer.Serialize(dict[keyToRemove]); // sem WriteIndented
            File.AppendAllText(backupPath, backupEntry + Environment.NewLine);

            // --- Remove do arquivo principal (mantendo indentação) ---
            dict.Remove(keyToRemove);
            File.WriteAllText(path, JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true }));

            //WriteLine($"[DEBUG] ID={id} removido com sucesso. Backup gravado em '{backupPath}'");
            return true;
        }


        WriteLine("[DEBUG] Nenhum objeto correspondente encontrado.");
        return false;
    }

    //--------------------
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
    internal static List<T> Search<T>(DataBaseType baseType, string? name = null, int? id = null)
    {
        string path = GetFilePath(baseType);

        if (!File.Exists(path))
        {
            WriteLine("[DEBUG] Arquivo não existe!");
            return new List<T>();
        }

        string json = ReadFile(path);
        var dict = SafeReadDatabase<T>(path);
        try { dict = JsonSerializer.Deserialize<Dictionary<string, T>>(json); }
        catch (JsonException ex)
        {
            WriteLine($"[ERROR] Falha ao ler o JSON em {path}: {ex.Message}");
            return new List<T>();
        }
        if (dict == null)
        {
            WriteLine("[DEBUG] Dicionário nulo após desserialização!");
            return new List<T>();
        }
        IEnumerable<T> list = dict.Values;

        // --- Filtro por nome ---
        if (!string.IsNullOrWhiteSpace(name))
        {
            string trimmed = name.Trim();
            list = list.Where(item =>
            {
                if (item == null) return false;

                var prop = item.GetType().GetProperty("Name_s",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (prop == null) return false;

                var valObj = prop.GetValue(item);
                string? value = valObj?.ToString();

                if (trimmed == "") return string.IsNullOrEmpty(value);

                return value != null && value.Contains(trimmed, StringComparison.OrdinalIgnoreCase);
            });
        }

        // --- Filtro por ID ---
        if (id.HasValue)
        {
            list = list.Where(item =>
            {
                if (item == null) return false;

                var prop = item.GetType().GetProperty("ID_i", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop == null)
                {
                    WriteLine("[DEBUG] Propriedade 'ID_i' não encontrada.");
                    return false;
                }

                var valObj = prop.GetValue(item);
                if (valObj == null)
                {
                    WriteLine("[DEBUG] Valor de 'ID_i' é nulo.");
                    return false;
                }

                int value = (int)valObj;
                return value == id.Value;
            });
        }

        return list.ToList();
    }

}
