/// <summary>
/// Class abstarta de primeiro grau, todas as entidades têm acesso, herdam esta base.
/// </summary>
namespace School_System.Domain.Base;

using static System.Console;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using School_System.Infrastructure.FileManager;

public abstract class BaseEntity(int id, string name)
{
    [JsonInclude] internal int ID_i { get; private set; } = id;
    [JsonInclude] internal protected string Name_s { get; set; } = name;
    protected abstract string Describe(); // função para mostrar os parametros de um objto, ecencial para modulação!!

    protected static readonly string InvalidEntrance = "Entrada inválida. Tente novamente.";
    protected static readonly string EmptyEntrance = "Entrada nula ou em branco, valor default utilizado.";
    protected const string ProblemGetTheId = "❗ Erro: Não foi possível obter um ID válido. Criação cancelada.❗";

    //----------------------------------
    // funções para mudança de Atributos
    //----------------------------------

    /// <summary>
    /// Pede ao usuário para inserir ou alterar um nome.
    /// </summary>
    /// <param name="prompt">Mensagem a exibir para o usuário.</param>
    /// <param name="isToEdit">Indica se é edição (true) ou criação (false).</param>
    /// <param name="currentValue">Valor atual, caso seja edição (null se criação).</param>
    /// <returns>O nome fornecido ou o valor atual/default caso não seja alterado.</returns>
    protected static string InputName(string prompt, string? currentValue = null, bool isToEdit = false)
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

    //----------------------------------
    // funções Globais
    //----------------------------------

    internal abstract BaseEntity? CreateInstance();// Fábrica obrigatória (não estática)

    /// <summary>
    /// Remove um ou mais objetos de uma base de dados, dado o nome ou ID.
    /// Pode ser usado para qualquer classe que herde de BaseEntity.
    /// </summary>
    /// <typeparam name="E">Tipo da entidade (aluno, tutor, disciplina, etc.).</typeparam>
    /// <param name="typeName">Nome da entidade para mostrar ao utilizador (ex: "aluno").</param>
    /// <param name="dbType">Base de dados onde procurar e remover.</param>
    protected static void RemoveEntity<E>(string typeName, FileManager.DataBaseType dbType) where E : BaseEntity
    {
        // 1. Procurar entidades (já imprime resultados)
        var matches = AskAndSearch<E>(typeName, dbType, true);

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
            WriteLine(matches[idx - 1].Describe());

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
                ? $"✔️ {typeName} removido: {ent.Describe()}"
                : $"❗ Erro ao remover: {ent.Describe()}");
        }
    }

    // --- Pergunta ao usuário e pesquisa na base de dados ---
    protected static List<T> AskAndSearch<T>(string typeName, FileManager.DataBaseType dbType, bool returnAll = false) where T : BaseEntity
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
            return [];
        }

        if (matches.Count == 1 || returnAll)
        {
            WriteLine($"{matches.Count} {typeName}(s) encontrado(s):");
            for (int i = 0; i < matches.Count; i++)
                WriteLine($"[{i + 1}]: {matches[i].Describe()}");
            return matches;
        }


        // Mais de um objeto encontrado e não é para remover todos
        WriteLine($"Foram encontrados {matches.Count} {typeName}s:");
        for (int i = 0; i < matches.Count; i++)
            WriteLine($"{i + 1}: {matches[i].Describe()}");

        Write($"Escolha qual deseja selecionar (1 - {matches.Count}): ");
        if (!int.TryParse(ReadLine(), out int choice) || choice < 1 || choice > matches.Count)
        {
            WriteLine("Entrada inválida. Nenhum selecionado.");
            return new List<T>();
        }

        return new List<T> { matches[choice - 1] };
    }


    internal virtual void Introduce() { }
}
