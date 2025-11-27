namespace School_System.Domain.SchoolMembers;

using static System.Console; // Permite usar Write e WriteLine diretamente
using System.Text.Json.Serialization;

using School_System.Domain.Base;
using School_System.Infrastructure.FileManager;

internal class Teacher : SchoolMember
{
    [JsonInclude] internal string Department_s { get; private set; } = "";
    protected override string Describe()
    {
        return $"ID={ID_i}, Nome='{Name_s}', Idade={Age_by}, Gênero={Gender_c},Nascimento={BirthDate_dt:yyyy-MM-dd}, Nacionalidade={Nationality}, Email={Email_s}, Departamento:{Department_s}.";
    }

    public Teacher() : base() { }
    private Teacher(string name, byte age, int id, char gender, DateTime birthDate, Nationality_e nat, string email, string department)
     : base(id, name, age, gender, birthDate, nationality: nat, email)
    {
        Department_s = department;
        Introduce();
    }

    // Fábrica de objetos Teacher. Pode retornar null se o utilizador cancelar
    internal override BaseEntity? CreateInstance() => Create();
    internal static Teacher? Create()
    {
        return CreateMember("professor", FileManager.DataBaseType.Teacher,
            // Primeiro os campos específicos
            dict =>
            {
                dict["Department"] = InputName("Departamento do professor");
            },

            // Depois o factory para criar o objeto
            dict => new Teacher(
                (string)dict["Name"],
                (byte)dict["Age"],
                (int)dict["ID"],
                (char)dict["Gender"],
                (DateTime)dict["BirthDate"],
                (Nationality_e)dict["Nationality"],
                (string)dict["Email"],
                (string)dict["Department"]
            )
        );
    }

    internal static void Remove() { RemoveEntity<Teacher>("professor", FileManager.DataBaseType.Teacher); }
    internal static void Select() { }
    internal override void Introduce() { WriteLine($"\n👨‍🏫 New Teacher: {Name_s}, ID: {ID_i}, Age: {Age_by}, Genero: {Gender_c}, Data de nascimento: {BirthDate_dt.Date}, Nacionalidade: {Nationality}."); }


}
