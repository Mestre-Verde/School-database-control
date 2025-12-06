/// <summary> Class abstrata de segundo grau, membros (Pessoas) da instituição tem esta class herdada. </summary>
namespace School_System.Domain.SchoolMembers;

using System.Text.Json.Serialization;
using School_System.Domain.Base;
using School_System.Application.Utils;
using School_System.Domain.Scholarship;

internal abstract class SchoolMember : BaseEntity
{
    [JsonInclude] internal protected byte Age_by;// byte (0-255) porque a idade nunca é negativa e não passa de 255.
    [JsonInclude] internal protected char Gender_c { get; protected set; }// char 'M' ou 'F' (sempre um único caractere)
    [JsonInclude] internal protected DateTime BirthDate_dt;// Data de nascimento (struct DateTime) 
    [JsonInclude] internal protected Nationality_e Nationality { get; protected set; }// Nacionalidade (enum) incorpurado para todos os tipos
    [JsonInclude] internal protected string Email_s { get; protected set; } = "";

    protected override string FormatToString()
    {
        string baseDesc = BaseFormat();
        return $"{baseDesc}, Idade={Age_by}, Gênero={Gender_c},Nascimento={BirthDate_dt:yyyy-MM-dd}, Nacionalidade={Nationality}, Email={Email_s ?? "N/A"}";
    }

    // vazia para não dar erro(abstract no baseEntity)
    protected override void Introduce() { }

    // Construtor parameterless obrigatório para descerialização JSON
    public SchoolMember() : base(0, "") { }

    // Construtor principal da classe base
    protected SchoolMember(int id, string name = "",
     byte age = default, char gender = default, DateTime? birthDate = default, Nationality_e nationality = default, string email = "")
     : base(id, name)
    {
        Age_by = age;
        Gender_c = gender;
        BirthDate_dt = birthDate ?? DateTime.Now;
        Nationality = nationality;
        Email_s = email;
    }

}






