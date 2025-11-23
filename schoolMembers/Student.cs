using static System.Console; // Permite usar Write e WriteLine diretamente

// Classe derivada: Student
internal class Student : SchoolMembers
{
    // Construtor parameterless obrigatório para JSON
    public Student() : base() { }

    private Student(string name, byte age, int id, char gender, DateTime birthDate, Nationality_e nat) : base(id, name, age, gender, birthDate, nationality: nat)
    {
        Introduce();
    }
    // Fábrica de objetos Student.
    internal static Student? Create() // Pode retornar null se o utilizador cancelar
    {
        return CreateMember("estudante", FileManager.DataBaseType.Student, (n, a, id, g, d, nat) => new Student(n, a, id, g, d, nat));
    }

    internal static void Remove() { RemoveMember<Student>("aluno", FileManager.DataBaseType.Student); }

    internal static void Select() { SelectMember<Student>("aluno", FileManager.DataBaseType.Student); }

    internal override void Introduce() { WriteLine($"🎓 New Student: {Name_s}, ID: {ID_i}, Age: {Age_by}, Genero: {Gender_c}, Data de nascimento: {BirthDate_dt.Date}, Nacionalidade: {Nationality}."); }
}

