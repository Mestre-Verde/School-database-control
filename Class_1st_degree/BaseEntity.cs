using static System.Console; // Permite usar Write e WriteLine diretamente
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

internal abstract class BaseEntity(int id, string name)
{
    [JsonInclude] internal int ID_i { get; private set; } = id;
    [JsonInclude] internal protected string Name_s { get; set; } = name;

    internal virtual string Describe() => $"ID={ID_i}, Nome={Name_s}"; // função para mostrar os parametros de um objto, ecencial para modulação!!

    internal static readonly string InvalidEntrance = "Entrada inválida. Tente novamente.";
    internal static readonly string EmptyEntrance = "Entrada nula ou em branco, valor default utilizado.";
    protected const string ProblemGetTheId = "❌ Erro: Não foi possível obter um ID válido. Criação cancelada.";

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
    /// Remove um ou mais objetos de qualquer tipo que derive de <see cref="BaseEntity"/> de forma genérica.
    /// </summary>
    /// <typeparam name="E">O tipo da entidade a ser removida, deve herdar de <see cref="BaseEntity"/>.</typeparam>
    /// <param name="typeName">Nome descritivo da entidade, usado para mensagens ao usuário (ex: "aluno", "disciplina").</param>
    /// <param name="dbType">O tipo de base de dados em que a entidade está armazenada, utilizado pelo <see cref="FileManager"/> para busca e remoção.</param>
    /// <remarks>
    /// Esta função é modular e reutilizável para qualquer tipo de entidade que herde de <see cref="BaseEntity"/> desde que implemente o método <see cref="BaseEntity.Describe"/>.
    /// </remarks>
    protected static void RemoveEntity<E>(string typeName, FileManager.DataBaseType dbType) where E : BaseEntity
    {
        Write($"Digite o nome ou ID do {typeName} para remover: ");
        string input = ReadLine() ?? "";

        bool isId = int.TryParse(input, out int idInput);

        var matches = isId
            ? FileManager.Search<E>(dbType, id: idInput)
            : FileManager.Search<E>(dbType, name: input);

        if (matches.Count == 0) { WriteLine($"Nenhum {typeName} encontrado."); return; }

        WriteLine($"Foram encontrados os seguintes {typeName}s:");
        for (int i = 0; i < matches.Count; i++)
            WriteLine($"{i + 1}: {matches[i].Describe()}");

        Write($"Escolha os números dos {typeName}s a remover (ex: 1,2,3 ou 1 2 3): ");
        var indices = (ReadLine() ?? "")
            .Split([',', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out int x) ? x : -1)
            .Where(x => x >= 1 && x <= matches.Count)
            .Distinct()
            .ToList();

        if (indices.Count == 0) { WriteLine("Nenhuma seleção válida. Operação cancelada."); return; }

        WriteLine($"Você selecionou os seguintes {typeName}s para remoção:");
        foreach (var idx in indices) WriteLine($"{matches[idx - 1].Describe()}");

        Write($"Tem certeza que deseja remover todos esses {typeName}s? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) != "S") { WriteLine("Operação cancelada."); return; }

        foreach (var idx in indices)
        {
            var m = matches[idx - 1];
            bool removed = FileManager.RemoveById<E>(dbType, m.ID_i);
            WriteLine(removed ? $"✅ {typeName} removido: {m.Describe()}" : $"❌ Erro ao remover: {m.Describe()}");
        }
    }

    internal virtual void Introduce() { }
}