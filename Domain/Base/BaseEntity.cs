/// <summary>
/// Class abstarta de primeiro grau, todas as entidades têm acesso, herdam esta base.
/// </summary>
namespace School_System.Domain.Base;

using static System.Console;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using School_System.Application.Utils;

using School_System.Infrastructure.FileManager;

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

        // Se for uma lista de objetos BaseEntity (como Subject ou Teacher)
        if (value is IEnumerable<BaseEntity> entityList)
            return string.Join(", ", entityList.Select(e => e.Name_s));

        // Se for uma lista de objetos genérica
        if (value is IEnumerable<object> objList)
            return string.Join(", ", objList.Select(o => o?.ToString() ?? "null"));

        // Se for enum
        if (value.GetType().IsEnum)
            return value.ToString();

        // Se for DateTime
        if (value is DateTime dt)
            return dt.ToString("yyyy-MM-dd");

        // Char
        if (value is char c)
            return c.ToString();

        // Qualquer outro tipo (int, float, string etc.)
        return value.ToString() ?? "null";
    }


    /// <summary> Fabrica global para criar qualquer objeto que herde de BaseEntity.
    /// Só precisa passar o nome, os campos específicos via Action, e a função que cria o objeto a partir do dicionário.
    /// </summary>
    protected static E? CreateEntity<E>(
        string typeObject,
        FileManager.DataBaseType dbType,
        Action<Dictionary<string, object>> collectSpecificFields,
        Func<Dictionary<string, object>, E> factory
    ) where E : BaseEntity
    {
        var parameters = new Dictionary<string, object>();

        // Nome padrão para todos
        parameters["Name"] = InputParameters.InputName($"Escreva o nome {typeObject}");

        // Campos específicos da subclasse
        collectSpecificFields(parameters);

        // Resumo final
        WriteLine($"\nResumo {typeObject}:");
        foreach (var kv in parameters)
            WriteLine($" {kv.Key}: {FormatParameter(kv.Value)}");

        // Confirmação
        while (true)
        {
            Write("Tem a certeza que quer criar? (S/N): ");
            string? input = ReadLine()?.Trim().ToUpper();

            if (input == "S") break;
            else if (input == "N") return null;
            else WriteLine("Por favor, responda apenas 'S' ou 'N'.");
        }

        // Criação do ID
        int newID = FileManager.GetTheNextAvailableID(dbType);
        if (newID == -1) { WriteLine(InputParameters.ProblemGetTheId); return null; }
        parameters["ID"] = newID;

        // Criação do objeto
        var obj = factory(parameters);

        // Gravação na base de dados
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
        var matches = AskAndSearch<E>(typeName, dbType, returnAll: true, allowMultiple: true);


        if (matches.Count == 0) return;

        // 2. Permitir escolher múltiplos indices
        Write($"Escolha os números dos {typeName}s a remover (ex: 1 2 3): ");
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

    // --- Pergunta ao usuário e pesquisa na base de dados ---
    internal protected static List<T> AskAndSearch<T>(string typeName, FileManager.DataBaseType dbType, bool returnAll = false, bool allowMultiple = false) where T : BaseEntity
    {
        Write($"Digite o nome ou ID do {typeName}: ");
        string? input_s = ReadLine();

        bool isId_b = int.TryParse(input_s, out int idInput);

        var matches = isId_b
            ? FileManager.Search<T>(dbType, id: idInput)
            : FileManager.Search<T>(dbType, name: input_s);

        if (matches.Count == 0)
        {
            WriteLine($"Nenhum {typeName} encontrado.");
            return new List<T>();
        }

        // Caso seja apenas 1 resultado ou returnAll, retorna todos
        if (matches.Count == 1 || returnAll)
        {
            WriteLine($"{matches.Count} {typeName}(s) encontrado(s):");
            for (int i = 0; i < matches.Count; i++)
                WriteLine($"[{i + 1}]: {matches[i].FormatToString()}");
            return allowMultiple ? matches : new List<T> { matches[0] };
        }

        // Mais de um resultado encontrado e não é para retornar todos
        WriteLine($"Foram encontrados {matches.Count} {typeName}s:");
        for (int i = 0; i < matches.Count; i++)
            WriteLine($"[{i + 1}]: {matches[i].FormatToString()}");

        if (allowMultiple)
        {
            Write($"Escolha quais deseja selecionar (números separados por vírgula, Enter para cancelar): ");
            string? multiInput = ReadLine()?.Trim();
            if (string.IsNullOrEmpty(multiInput)) return new List<T>();

            var selected = new List<T>();
            foreach (var part in multiInput.Split(',').Select(s => s.Trim()))
            {
                if (int.TryParse(part, out int idx) && idx >= 1 && idx <= matches.Count)
                {
                    var item = matches[idx - 1];
                    if (!selected.Contains(item))
                        selected.Add(item);
                }
                else
                {
                    WriteLine($"Número inválido: {part}. Ignorado.");
                }
            }
            return selected;
        }
        else
        {
            Write($"Escolha qual deseja selecionar (1 - {matches.Count}): ");
            if (!int.TryParse(ReadLine(), out int choice) || choice < 1 || choice > matches.Count)
            {
                WriteLine("Entrada inválida. Nenhum selecionado.");
                return new List<T>();
            }
            return new List<T> { matches[choice - 1] };
        }
    }
}
