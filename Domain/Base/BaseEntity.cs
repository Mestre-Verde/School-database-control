/// <summary>
/// Class abstarta de primeiro grau, todas as entidades têm acesso, herdam esta base.
/// </summary>
namespace School_System.Domain.Base;

using static System.Console;
using System.Text.Json.Serialization;
using School_System.Application.Utils;

using School_System.Infrastructure.FileManager;
using System.Text.Json;

public abstract class BaseEntity(int id, string name)
{
    [JsonInclude] internal int ID_i { get; private set; } = id;
    [JsonInclude] internal protected string Name_s { get; set; } = name;

    // OBRIGATÓRIO para todas as classes
    protected abstract string FormatToString();

    // serve somente para mostrar quendo um novo objeto é criado
    protected abstract void Introduce();

    // ToString universal para mostrar as informações do objeto(Obrigat]orio ser public(pois subescreve uma função existente))
    public override string ToString() => FormatToString();

    // Descrição global para todos os objetos que heram esta class
    protected string BaseFormat() { return $"ID={ID_i}, Nome='{Name_s ?? "N/A"}'"; }

    //-----------------------------

    // função helper para poder imprimir qualquer tipo de variavel
    private static string FormatParameter(object? value)
    {
        if (value == null) return "Nenhum";

        // Lista de BaseEntity
        if (value is IEnumerable<BaseEntity> entityList)
            return string.Join(", ", entityList.Select(e => e.Name_s ?? "N/A"));

        // Lista genérica
        if (value is IEnumerable<object> objList)
            return string.Join(", ", objList.Select(o => o?.ToString() ?? "null"));

        // Enum
        if (value?.GetType().IsEnum == true)
            return value.ToString() ?? "N/A";

        // DateTime
        if (value is DateTime dt)
            return dt.ToString("yyyy-MM-dd");

        // Char
        if (value is char c)
            return c.ToString();

        // Outros tipos (int, float, string, etc.)
        return value?.ToString() ?? "null";
    }

    /// <summary> Fábrica global para criar *qualquer entidade* que herde de BaseEntity (Student, Course, Subject, etc). 
    /// O funcionamento é o seguinte:
    /// 1) A função pede o nome (campo comum a todas as entidades)
    /// 2) A subclasse (Student, Course, etc.) fornece os seus campos específicos através de um delegate chamado "collectSpecificFields"
    /// 3) No final, a função "factory" converte o dicionário de parâmetros num objeto real
    /// 4) O objeto é gravado na base de dados automaticamente.
    /// Isto permite eliminar código duplicado e centralizar toda a lógica de criação.
    /// </summary>
    protected static E? CreateEntity<E>(
        string typeObject,                                 // Ex.: "curso", "estudante", "disciplina"
        FileManager.DataBaseType dbType,                    // Tipo de BD onde o objeto será gravado
        Action<Dictionary<string, object>> collectSpecificFields, // Função da SUBCLASSE que recolhe os campos dela
        Func<Dictionary<string, object>, E> factory         // Função que transforma o dicionário num objeto real
    ) where E : BaseEntity
    {
        // 1) Criamos sempre um dicionário para armazenar os parâmetros. O objetivo é armazenar TODA a informação necessária antes de criar o objeto.
        var parameters = new Dictionary<string, object>
        {
            // Campo comum a todas as entidades: Name
            ["Name"] = InputParameters.InputName($"Escreva o nome {typeObject}")
        };
        /*2) Chamamos o delegate da subclasse para recolher os campos específicos.
        Exemplo:
        Para Course → Course.CollectFields(parameters)
        Para Student → Student.CollectFields(parameters)
        Isto injeta os campos adicionais dentro do dicionário, mas sem a função CreateEntity precisar saber quais são.
        ESTE É O SEGREDO DO POLIMORFISMO AQUI.
        */
        collectSpecificFields(parameters);
        /*3) Mostrar um resumo final antes de gravar.
        O dicionário agora contém TUDO:
        - Nome
        - Campos específicos (ex.: Year, Major, Credits, Duration…)
        - ID (ainda não gerado)
        */
        WriteLine($"\nResumo {typeObject}:");
        foreach (var kv in parameters)
            WriteLine($" {kv.Key}: {FormatParameter(kv.Value)}");

        // 4) Confirmar se o utilizador realmente quer criar o objeto.
        while (true)
        {
            Write("Tem a certeza que quer criar? (S/N): ");
            string? input = ReadLine()?.Trim().ToUpper();

            if (input == "S") break;
            else if (input == "N") return null;
            else WriteLine("Por favor, responda apenas 'S' ou 'N'.");
        }

        // 5) Geração do ID automático.
        int newID = FileManager.GetTheNextAvailableID(dbType);
        if (newID == -1)
        {
            WriteLine(InputParameters.ProblemGetTheId);
            return null;
        }
        parameters["ID"] = newID;
        /*6) Finalmente criamos o objeto real.
        A função "factory" pega no dicionário e transforma em:
        new Course(...)
        new Student(...)
        new Subject(...)
        Dependendo da subclasse.
        Isto permite usar uma única função CreateEntity para TODOS os tipos.
        */
        var obj = factory(parameters);

        // 7) Guardar no ficheiro da base de dados.
        FileManager.WriteOnDataBase(dbType, obj);

        return obj;
    }

    /// <summary> Remove um ou mais objetos de uma base de dados, dado o nome ou ID.Pode ser usado para qualquer classe que herde de BaseEntity. </summary>
    /// <typeparam name="E">Tipo da entidade (aluno, tutor, disciplina, etc.).</typeparam>
    /// <param name="typeName">Nome da entidade para mostrar ao utilizador (ex: "aluno").</param>
    /// <param name="dbType">Base de dados onde procurar e remover.</param>
    protected static void RemoveEntity<E>(string typeName, FileManager.DataBaseType dbType) where E : BaseEntity
    {
        // 1. Procurar entidades (já imprime resultados)
        var searchResult = AskAndSearch<E>(typeName, dbType, allowListAll: true);

        if (searchResult.IsDatabaseEmpty)
        {
            WriteLine($"A base de dados de {typeName} está vazia. Nada para remover.");
            return;
        }

        var matches = searchResult.Results;
        if (matches.Count == 0)
        {
            WriteLine($"Nenhum {typeName} encontrado. Operação cancelada.");
            return;
        }

        // 2. Permitir escolher múltiplos índices
        Write($"Escolha os números dos {typeName}s a remover (ex: 1 2,3): ");
        var indices = (ReadLine() ?? "")
            .Split([' ', ','], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out int x) ? x : -1)
            .Where(x => x >= 1 && x <= matches.Count)
            .Distinct()
            .ToList();

        if (indices.Count == 0)
        {
            WriteLine("Nenhuma seleção válida. Operação cancelada.");
            return;
        }

        // 3. Confirmar
        WriteLine($"Você selecionou os seguintes {typeName}s:");
        foreach (var idx in indices)
            WriteLine(matches[idx - 1].FormatToString());

        Write($"Tem certeza que deseja remover todos esses {typeName}s? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) != "S")
        {
            WriteLine("Operação cancelada.");
            return;
        }

        // 4. Remover
        foreach (var idx in indices)
        {
            var ent = matches[idx - 1];
            bool ok = FileManager.RemoveById<E>(dbType, ent.ID_i);
            WriteLine(ok
                ? $"✔️ {typeName} removido: {ent.FormatToString()}"
                : $"❗ Erro ao remover: {ent.FormatToString()}");
        }
    }

    /// <summary>Imprime comparação simples entre dois objetos BaseEntity. Para listas, mostra apenas os nomes separados por ponto e vírgula.</summary>
    protected static void PrintComparison<T>(T current, T original) where T : BaseEntity
    {
        WriteLine("===== 🛈 ESTADO DO OBJETO =====");
        WriteLine("{0,-20} | {1,-25} | {2}", "Campo", "Atual", "Original");
        WriteLine(new string('-', 70));

        var type = typeof(T);
        var members = type.GetMembers(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            .Where(m =>
                (m.MemberType == System.Reflection.MemberTypes.Property || m.MemberType == System.Reflection.MemberTypes.Field)
                && !(m.Name.StartsWith("<") && m.Name.EndsWith("k__BackingField")) // Ignora backing fields
            );

        foreach (var member in members)
        {
            object? now = member.MemberType == System.Reflection.MemberTypes.Property
                        ? ((System.Reflection.PropertyInfo)member).GetValue(current)
                        : ((System.Reflection.FieldInfo)member).GetValue(current);
            object? old = member.MemberType == System.Reflection.MemberTypes.Property
                        ? ((System.Reflection.PropertyInfo)member).GetValue(original)
                        : ((System.Reflection.FieldInfo)member).GetValue(original);

            static string FormatValue(object? value)
            {
                if (value is IEnumerable<BaseEntity> entityList) return string.Join(", ", entityList.Select(e => e.Name_s ?? "N/A"));
                if (value is BaseEntity entity) return entity.Name_s ?? "N/A";
                return FormatParameter(value);
            }

            WriteLine($"{member.Name,-20} | {FormatValue(now),-25} | {FormatValue(old)}");
        }
        WriteLine(new string('=', 70));
    }

    protected static void SelectEntity<E>(
        string entityName,
        FileManager.DataBaseType dbType,
        Action<E> editAction,
        bool allowListAll = true,
        bool allowUserSelection = true) where E : BaseEntity
    {
        // Pesquisa entidades usando AskAndSearch
        var searchResult = AskAndSearch<E>(entityName, dbType, allowListAll: allowListAll, allowUserSelection: allowUserSelection);

        // Base de dados vazia ou usuário cancelou → nada a fazer
        if (searchResult.IsDatabaseEmpty || searchResult.Results.Count == 0) return;

        // Chama função de edição com a entidade selecionada
        editAction(searchResult.Results[0]);
    }


    // Struct para resultado de busca, indicando se a base está vazia
    internal protected struct SearchResult<E>(List<E> results, bool isEmpty) where E : BaseEntity
    {
        public List<E> Results = results;
        public bool IsDatabaseEmpty = isEmpty;
    }

    /// <summary>Função modular para perguntar para o utilizador por objetos em uma base de dados usando caracteres(strings) ou id</summary>
    /// <typeparam name="E">O tipo de entidade (BaseEntity) a ser pesquisada.</typeparam> 
    /// <param name="typeName">O nome descritivo do tipo (ex: "curso","estudante x").</param>
    /// <param name="dbType">O tipo de base de dados para a pesquisa.</param>
    /// <param name="prompt">Prompt de entrada personalizada a ser exibida. Opcional.</param>
    /// <param name="allowListAll">Se pode permitir o input '-a' para listar todos.</param>
    /// <param name="allowUserSelection">True para o utilizador poder selecionar 1 objeto dentro desta função </param>
    /// <returns>Um SearchResult<E> contendo a lista de correspondências e um indicador se a BD estava vazia.</returns>
    internal protected static SearchResult<E> AskAndSearch<E>(
        string typeName,
         FileManager.DataBaseType dbType,
          string? prompt = null,
           bool allowListAll = false,
            bool allowUserSelection = false) where E : BaseEntity
    {
        string finalPrompt = prompt ??
        $"Digite o nome ou ID do {typeName}" + (allowListAll ?
                                                " (ou '-a' para listar todos): " :
                                                ": ");

        Write(finalPrompt);
        string? input_s = ReadLine()?.Trim();


        // --- Lógica para '-a' ---
        List<E> matches;
        bool isEmpty = false;
        if (allowListAll && !string.IsNullOrEmpty(input_s) && input_s.Equals("-a", StringComparison.OrdinalIgnoreCase))
        {
            matches = FileManager.GetAll<E>(dbType);
            isEmpty = matches.Count == 0;
        }
        else
        {
            bool isId_b = int.TryParse(input_s, out int idInput);
            matches = isId_b
                ? FileManager.Search<E>(dbType, ref isEmpty, id: idInput)
                : FileManager.Search<E>(dbType, ref isEmpty, name: input_s);
        }

        // --- Nenhum resultado ---
        if (matches.Count == 0)
        {
            if (isEmpty)
                WriteLine($"A base de dados de {typeName} está vazia.");
            else
                WriteLine($"Nenhum {typeName} encontrado.");
            return new SearchResult<E>([], isEmpty);
        }

        // --- Exibe resultados encontrados ---
        WriteLine($"{matches.Count} {typeName}(s) encontrado(s):");
        for (int i = 0; i < matches.Count; i++)
            WriteLine($"[{i + 1}]: {matches[i].FormatToString()}");

        // --- Permitir seleção do usuário se necessário ---
        if (allowUserSelection && matches.Count > 1)
        {
            Write($"Digite o número do {typeName} desejado (1 - {matches.Count}, Enter para cancelar): ");
            string? choiceInput = ReadLine()?.Trim();
            if (string.IsNullOrEmpty(choiceInput))
                return new SearchResult<E>([], false); // cancelou

            if (!int.TryParse(choiceInput, out int choice) || choice < 1 || choice > matches.Count)
            {
                WriteLine("Entrada inválida. Operação cancelada.");
                return new SearchResult<E>([], false);
            }

            matches = new List<E> { matches[choice - 1] }; // mantém apenas o selecionado
        }

        return new SearchResult<E>(matches, false);
    }

    // TODO: not implemented , ignore this function
    protected static void EditEntity<T>(
        T entity,                                  // O objeto que vai ser editado
        Func<T, string> getMenu,                   // Função que retorna o menu como string para este objeto
        Func<T, Enum> getOption,                   // Função que lê a opção escolhida pelo utilizador e retorna como Enum
        Dictionary<Enum, Action<T>> editActions,  // Dicionário: cada opção do menu tem a sua ação correspondente
        FileManager.DataBaseType dbType,           // Tipo de base de dados onde gravar a entidade
        Action<T, T>? revertAction = null          // Ação opcional para reverter alterações caso o utilizador cancele
    ) where T : BaseEntity
    {
        // Criar uma cópia profunda do objeto para backup (usando JSON)
        var original = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(entity))!;

        bool hasChanged = false; // Flag para saber se o objeto foi alterado

        // Mostrar menu inicial
        Write(getMenu(entity));

        while (true)
        {
            // Lê a opção do utilizador
            var option = getOption(entity);

            // Se o utilizador escolher "Back", sair do loop
            if (option.ToString() == "Back") break;

            // Se a opção existir no dicionário de ações, executa a ação correspondente
            if (editActions.TryGetValue(option, out var action))
            {
                action(entity);   // Executa a ação
                hasChanged = true; // Marca que houve alteração
            }
            else if (option.ToString() == "Help")
            {
                // Se o utilizador pedir ajuda, imprime comparação entre estado atual e original
                PrintComparison(entity, original);
            }
        }

        // Se não houve alteração, apenas sai
        if (!hasChanged) return;

        // Perguntar ao utilizador se quer guardar alterações
        Write("\nGuardar alterações? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) == "S")
        {
            // Grava o objeto alterado na base de dados
            FileManager.WriteOnDataBase(dbType, entity);
            WriteLine("✔️ Alterações salvas.");
        }
        else
        {
            // Cancelou: mostrar mensagem
            WriteLine("❌ Alterações descartadas.");

            // Se a função de revert estiver definida, invoca para restaurar estado original
            revertAction?.Invoke(entity, original);
        }
    }



}
