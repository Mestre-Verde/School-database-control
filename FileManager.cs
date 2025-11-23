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
    // 🗂️ Caminhos dos ficheiros principais do programa
    internal static string CourseFilePath { get; } = "Course.cs";                 // Código fonte de cursos
    internal static string StudentFilePath { get; } = "Person.cs";                // Código fonte de pessoas
    private static string DataBaseDirectory { get; } = "data/";               // Pasta onde ficam os ficheiros de dados
    private static string StudentsJSONPath { get; } = "data/students.json";      // Dados dos estudantes
    private static string TeachersJSONPath { get; } = "data/teachers.json";      // Dados dos professores
    private static string CoursesJSONPath { get; } = "data/courses.json";         // Dados dos cursos
    private static readonly string[] files =
    [
         CourseFilePath,
         StudentFilePath,
         StudentsJSONPath,
         TeachersJSONPath,
         CoursesJSONPath
    ];
    internal enum DataBaseType// Enum para percorrer os caminhos das base de dados.
    {
        Student,
        Teacher,
        Course
    }

    /// <summary> Retorna o caminho do arquivo JSON correspondente ao tipo de base de dados fornecido.</summary>
    /// <returns>
    /// Retorna uma variavel do tipo <see cref="string"/> com o caminho completo do arquivo JSON correspondente ao <paramref name="baseType"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Lançada quando o <paramref name="baseType"/> não corresponde a nenhum valor definido no enum <see cref="DataBaseType"/>.
    /// </exception>
    private static string GetFilePath(DataBaseType baseType)
    {
        return baseType switch
        {
            DataBaseType.Student => StudentsJSONPath,
            DataBaseType.Teacher => TeachersJSONPath,
            DataBaseType.Course => CoursesJSONPath,
            _ => throw new ArgumentOutOfRangeException(nameof(baseType))
        };
    }

    // 🎨 Desenha uma barra de progresso simples no terminal
    private static void DrawProgressBar(int value, int max)
    {
        int totalBlocks = 30; // Tamanho da barra (número de blocos)
        int filledBlocks = (int)Math.Round((double)value / max * totalBlocks); // Percentagem preenchida

        // Cria visual da barra com '#' e '-'
        string bar = "[" +
            new string('#', filledBlocks) +
            new string('-', totalBlocks - filledBlocks) +
            $"] {value * 100 / max}%";

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
            string content = File.ReadAllText(path).Trim();
            if (string.IsNullOrWhiteSpace(content)) return false;

            JsonDocument.Parse(content);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Verifica se todos os ficheiros necessários existem e cria os que faltam
    internal static bool StartupCheckFilesWithProgress(bool setup = true)
    {
        int total = files.Length;
        int count = 0;
        List<string> missingFiles = [];

        // Variável que guarda se houve algum erro
        bool errorDetected = false;

        if (setup) WriteLine("A verificar os ficheiros...");

        // Garantir diretoria
        if (!Directory.Exists(DataBaseDirectory))
        {
            Directory.CreateDirectory(DataBaseDirectory);
            missingFiles.Add(DataBaseDirectory);
        }

        foreach (var file in files)
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
                    string content = File.ReadAllText(file).Trim();

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
            Console.WriteLine("\n📄 Ficheiros criados:");
            foreach (var f in missingFiles)
                Console.WriteLine(" - " + f);
        }
        else if (setup)
        {
            Console.WriteLine("\n✅ Nenhum ficheiro em falta.");
        }

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
    private static int GetNextAvailableIDFromFile<X>(string filePath)
    {
        string json = ReadFile(filePath);// Lê o conteúdo do ficheiro JSON indicado
        if (string.IsNullOrWhiteSpace(json)) { return 0; }// Caso o ficheiro esteja vazio, nulo ou só com espaços, não há dados — começa do ID 0

        // Desserializa o JSON num dicionário de pares [ID → objeto T]
        var dict = JsonSerializer.Deserialize<Dictionary<int, X>>(json);// O tipo genérico <T> torna este método reutilizável para qualquer classe de dados. Exemplo: se T for Student, ficamos com Dictionary<int, Student>
        if (dict == null || dict.Count == 0) { return 0; }// Se o ficheiro estiver vazio ou o JSON não tiver entradas válidas, retorna 0 como o primeiro ID disponível.

        // Enquanto o ID atual existir no dicionário, incrementa para procurar o próximo livre. Esta lógica garante que se houver "buracos" (ex: IDs 0,1,3,4), o método retorna 2.
        int nextID = 0;
        while (dict.ContainsKey(nextID)) { nextID++; }
        return nextID;//  Retorna o menor ID que ainda não foi usado.
    }

    // Método principal que decide qual base de dados usar (Student, Teacher, Course)
    // e chama a função auxiliar genérica com o tipo e ficheiro corretos.
    internal static int GetTheNextAvailableID(DataBaseType baseType)
    {
        if (!StartupCheckFilesWithProgress(false)) { return -1; } // Verifica se os ficheiros essenciais existem

        // Usa expressão 'switch' moderna do C# para selecionar o caminho e tipo correto. Cada caso chama a função genérica com o tipo correspondente.
        return baseType switch
        {
            // 📘 Base de dados de estudantes → lê students.json e trata como Dictionary<int, Student>
            DataBaseType.Student => GetNextAvailableIDFromFile<Student>(StudentsJSONPath),

            // 📗 Base de dados de professores → lê teachers.json e trata como Dictionary<int, Teacher>
            DataBaseType.Teacher => GetNextAvailableIDFromFile<Teacher>(TeachersJSONPath),

            // 📙 Base de dados de cursos → lê courses.json e trata como Dictionary<int, Course>
            DataBaseType.Course => GetNextAvailableIDFromFile<Course>(CoursesJSONPath),

            _ => throw new ArgumentOutOfRangeException(nameof(baseType))// Caso o tipo de base de dados não seja reconhecido, lança exceção descritiva.
        };
    }

    //-----------------------
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
            WriteLine("[DEBUG] Dicionário nulo após desserialização!");
            return false;
        }

        var keyToRemove = dict.FirstOrDefault(kvp =>
        {
            if (kvp.Value == null) return false;  // evita CS8602

            var prop = kvp.Value.GetType().GetProperty("ID_i", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null) return false;

            var valObj = prop.GetValue(kvp.Value);
            if (valObj == null) return false;

            int value = (int)valObj;
            return value == id;
        }).Key;


        if (keyToRemove != null)
        {
            dict.Remove(keyToRemove);
            File.WriteAllText(path, JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true }));
            WriteLine($"[DEBUG] ID={id} removido com sucesso.");
            return true;
        }

        WriteLine("[DEBUG] Nenhum objeto correspondente encontrado para remover.");
        return false;
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
        WriteLine($"[DEBUG] Procurando em {path} | name='{name}' | id='{id}'");

        if (!File.Exists(path))
        {
            WriteLine("[DEBUG] Arquivo não existe!");
            return new List<T>();
        }

        string json = File.ReadAllText(path);
        var dict = JsonSerializer.Deserialize<Dictionary<string, T>>(json);
        if (dict == null)
        {
            WriteLine("[DEBUG] Dicionário nulo após desserialização!");
            return new List<T>();
        }

        IEnumerable<T> list = dict.Values;

        // --- Filtro por nome ---
        if (name != null) // user realmente quis procurar por nome
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

                // 📌 Caso 1 — user enviou string vazia → procurar apenas campos vazios OU null
                if (trimmed == "")
                {
                    return string.IsNullOrEmpty(value);
                }

                // 📌 Caso 2 — user enviou texto → comparar normalmente
                if (value == null) return false;
                return value.Contains(trimmed, StringComparison.OrdinalIgnoreCase);
            });
        }


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
                WriteLine($"[DEBUG] Comparando ID {value} com {id.Value}");
                return value == id.Value;
            });
        }

        var results = list.ToList();
        WriteLine($"[DEBUG] Encontrados {results.Count} resultados");
        return results;
    }
    //--------------------

}
