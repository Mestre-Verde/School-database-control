using static System.Console; // Permite usar Write e WriteLine diretamente

// Classe derivada: Teacher
internal class Teacher : SchoolMembers
{
    public Teacher() : base() { }
    private Teacher(string name, byte age, int id, char gender, DateTime birthDate, Nationality_e nat) : base(id, name, age, gender, birthDate, nationality: nat)
    {
        Introduce();
    }

    // Fábrica de objetos Teacher. Pode retornar null se o utilizador cancelar
    internal static Teacher? Create()
    {
        return CreateMember("professor", FileManager.DataBaseType.Teacher, (n, a, id, g, d, nat) => new Teacher(n, a, id, g, d, nat));
    }

    internal static void Remove() { RemoveMember<Teacher>("professor", FileManager.DataBaseType.Teacher); }

    internal static void Select() { SelectMember<Teacher>("professor", FileManager.DataBaseType.Teacher); }

    internal override void Introduce() { WriteLine($"\n👨‍🏫 New Teacher: {Name_s}, ID: {ID_i}, Age: {Age_by}, Genero: {Gender_c}, Data de nascimento: {BirthDate_dt.Date}, Nacionalidade: {Nationality}."); }
}
