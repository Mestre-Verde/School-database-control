using static System.Console; // Permite usar Write e WriteLine diretamente
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

internal abstract class BaseEntity
{
    [JsonInclude] internal int ID_i { get; private set; }
    [JsonInclude] internal protected string Name_s { get; set; } = "";

    internal static readonly string InvalidEntrance = "Entrada inválida. Tente novamente.";
    internal static readonly string EmptyEntrance = "Entrada nula ou em branco, valor default utilizado.";
    // Construtor protegido para ser usado pelas classes derivadas
    protected BaseEntity(int id, string name)
    {
        ID_i = id;
        Name_s = name;
    }

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

}