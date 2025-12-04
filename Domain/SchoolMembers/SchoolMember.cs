/// <summary>
/// Class abstrata de segundo grau, membros (Pessoas) da instituição tem esta class herdada.
/// </summary>
namespace School_System.Domain.SchoolMembers;

using static System.Console; // Permite usar Write e WriteLine diretamente
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using School_System.Infrastructure.FileManager;
using Schoo_lSystem.Application.Menu;
using School_System.Domain.Base;
using School_System.Domain.CourseProgram;
using School_System.Domain.SchoolMembers;
using School_System.Application.Utils;
using School_System.Domain.Scholarship;

public enum Nationality_e
{
    Other,      // 0
    PT,         // Portugal
    ES,         // Espanha
    FR,         // França
    US,         // Estados Unidos
    GB,         // Reino Unido
    DE,         // Alemanha
    IT,         // Itália
    BR,         // Brasil
    JP,         // Japão
    CN,         // China
    IN,         // Índia
    CA,         // Canadá
    AU,         // Austrália
    RU          // Rússia
}
public enum VisaState_e
{
    NONE,
    ValidStudentVisa,
    PendingRenewal,
    Expired,
    Temporary
}

internal abstract class SchoolMember : BaseEntity
{
    [JsonInclude] internal protected byte Age_by;// byte (0-255) porque a idade nunca é negativa e não passa de 255.
    [JsonInclude] internal protected char Gender_c { get; protected set; }// char 'M' ou 'F' (sempre um único caractere)
    [JsonInclude] internal protected DateTime BirthDate_dt;// Data de nascimento (struct DateTime) 
    [JsonInclude] internal protected Nationality_e Nationality { get; protected set; }// Nacionalidade (enum) incorpurado para todos os tipos
    [JsonInclude] internal protected string Email_s { get; protected set; } = "";
    [JsonIgnore] protected const byte MinAge = 6;

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
    /*
        // Factory para criar objetos em subclasses
        protected static M? CreateMember<M>(string typeObject, FileManager.DataBaseType dbType, Action<Dictionary<string, object>> collectSpecificFields, Func<Dictionary<string, object>, M> factory) where M : BaseEntity
        {
            // ---------- CAMPOS COMUNS ----------
            var parameters = new Dictionary<string, object>
            {
                ["Name"] = InputParameters.InputName($"Escreva o nome do(a) {typeObject}")
            };

            DateTime? trash = null;
            parameters["Age"] = InputParameters.InputAge($"Escreva a idade do(a) {typeObject}", ref trash, null, false, MinAge); byte age = (byte)parameters["Age"];

            parameters["Gender"] = InputParameters.InputGender($"Escreva o gênero do(a) {typeObject}");

            parameters["BirthDate"] = InputParameters.InputBirthDate("", ref age); parameters["Age"] = age;

            parameters["Nationality"] = InputParameters.InputNationality($"Escreva a nacionalidade {typeObject}");

            parameters["Email"] = InputParameters.InputEmail($"Escreva o email do(a) {typeObject}");


            // ---------- CAMPOS ESPECÍFICOS ----------
            collectSpecificFields(parameters);

            // ---------- RESUMO FINAL ----------
            WriteLine($"\nResumo do {typeObject}:");
            foreach (var kv in parameters)
                WriteLine($" {kv.Key}: {kv.Value}");

            Write("Tem a certeza que quer criar? (S/N): ");
            if ((ReadLine()?.Trim().ToUpper()) != "S") return null;

            // ---------- CRIA ID ----------
            int newID = FileManager.GetTheNextAvailableID(dbType);
            if (newID == -1) { WriteLine(ProblemGetTheId); return null; }

            parameters["ID"] = newID;

            // ---------- CRIA OBJETO ----------
            var objeto = factory(parameters);

            FileManager.WriteOnDataBase(dbType, objeto);
            return objeto;
        }
        */
}

internal class Student : SchoolMember
{
    [JsonInclude] protected decimal GPA_d;
    [JsonInclude] protected Course? Major { get; set; }
    [JsonInclude] protected int Year { get; set; }
    [JsonInclude] protected List<Subject> EnrolledSubjects_l = [];
    [JsonInclude] List<Bolsa> Scholarships = [];

    protected override string FormatToString()
    {
        string baseDesc = base.FormatToString();
        string? courseName = Major?.Name_s ?? "N/A";
        return $"{baseDesc}, Curso: {courseName}, Ano: {Year}, Disciplinas inscrito(a): {EnrolledSubjects_l?.Count ?? 0}, GPA: {GPA_d}";
    }

    protected override void Introduce() { Write($"\n🎓 New Student: "); WriteLine(FormatToString()); }

    // Construtor parameterless obrigatório para descerialização JSON
    public Student() : base() { }

    protected Student(int id, string name, byte age, char gender, DateTime? birthDate, Nationality_e nationality, string email,
         Course? major = null, int year = default, List<Subject>? enrolledSubjects = default)
        : base(id, name, age, gender, birthDate, nationality, email)
    {
        Major = major;
        Year = year;
        EnrolledSubjects_l = enrolledSubjects;
        Introduce();
    }

    //----------------------------------

    protected decimal CalculateGPA() { return 1m; }
    protected void AddGradeToSubject(Subject dsiciplina, decimal grade) { }
    protected virtual decimal CalculateTuition() { return 0m; }
}

internal class UndergraduateStudent : Student
{
    protected override string FormatToString() { return base.FormatToString(); }
    protected override void Introduce() { base.Introduce(); }

    // Construtor parameterless obrigatório para JSON
    public UndergraduateStudent() { }

    private UndergraduateStudent(int id, string name, byte age, char gender, DateTime? birthDate, Nationality_e nationality, string email, Course? major = null, int year = 1, List<Subject>? enrolledSubjects = null)
        : base(id, name, age, gender, birthDate, nationality, email, major, year, enrolledSubjects)
    {
        // Se NÃO foi dada uma lista → usar a lista oficial do curso
        if (enrolledSubjects is null)
            EnrolledSubjects_l = major?.Subjects_l?.Select(s => s).ToList() ?? new List<Subject>();
    }


    internal static UndergraduateStudent? Create()
    {
        return CreateEntity("estudante de CETEsP ou Licenciatura", FileManager.DataBaseType.UndergraduateStudent, parameters =>
        {
            // --- Variáveis temporárias ---
            DateTime? trash = null;

            // --- Campos base ---
            byte age = InputParameters.InputAge($"Escreva a idade do(a) estudante", ref trash, null, false, MinAge);
            parameters["Age"] = age;

            parameters["Gender"] = InputParameters.InputGender($"Escreva o gênero do(a) estudante");

            birthDate = InputParameters.InputBirthDate($"Escreva a data de nascimento do(a) estudante", ref age);
            parameters["BirthDate"] = birthDate;

            parameters["Nationality"] = InputParameters.InputNationality($"Escreva a nacionalidade do(a) estudante");
            parameters["Email"] = InputParameters.InputEmail($"Escreva o email do(a) estudante");

            // --- Campos específicos do UndergraduateStudent ---
            parameters["Major"] = InputParameters.InputCourse();
            parameters["Year"] = InputParameters.InputInt($"Escreva o ano atual do(a) estudante", 1, 4);

        },
        dict => new UndergraduateStudent(
            (int)dict["ID"],
            (string)dict["Name"],
            (byte)dict["Age"],
            (char)dict["Gender"],
            (DateTime?)dict["BirthDate"],
            (Nationality_e)dict["Nationality"],
            (string)dict["Email"],
            (Course?)dict["Major"],
            (int)dict["Year"],
            null
        ));
    }

    internal static void Remove() { RemoveEntity<UndergraduateStudent>("estudante de CETEsP ou Licenciatura", FileManager.DataBaseType.UndergraduateStudent); }

    internal static void Select() { }

    protected override decimal CalculateTuition()
    {
        // Propina base
        return 1000m;
    }
}

internal class GraduateStudent : Student
{
    // Propriedades deste objeto
    [JsonInclude] protected string ThesisTopic { get; set; } = "";
    [JsonInclude] protected Teacher? Advisor { get; set; }

    protected override string FormatToString() // Adiciona os campos específicos na descrição
    {
        string baseDesc = base.FormatToString();
        string advisorName = Advisor?.Name_s ?? "N/A";
        return $"{baseDesc}, Tema Dissertação/Tese: '{ThesisTopic}', Orientador: {advisorName}";
    }
    protected override void Introduce() { base.Introduce(); }

    // Construtor parameterless obrigatório para JSON
    public GraduateStudent() : base() { }

    private GraduateStudent(
        int id, string name, byte age, char gender, DateTime? birthDate, Nationality_e nationality, string email,
        Course? major = null, int year = 1, List<Subject>? enrolledSubjects = null,
        string thesisTopic = default!, Teacher? advisor = default)
        : base(id, name, age, gender, birthDate, nationality, email, major, year, enrolledSubjects)
    {
        ThesisTopic = thesisTopic;
        Advisor = advisor;
        if (enrolledSubjects is null) EnrolledSubjects_l = major?.Subjects_l?.Select(s => s).ToList() ?? new List<Subject>();
        else EnrolledSubjects_l = enrolledSubjects;
    }

    internal static GraduateStudent? Create()
    {
        return CreateEntity("estudante de mestrado ou doutoramento", FileManager.DataBaseType.GraduateStudent, parameters =>
        {
            // --- Variáveis temporárias ---
            DateTime? trash = null;

            // --- Campos base ---
            byte age = InputParameters.InputAge($"Escreva a idade do(a) estudante", ref trash, null, false, MinAge);
            parameters["Age"] = age;

            parameters["Gender"] = InputParameters.InputGender($"Escreva o gênero do(a) estudante");

            birthDate = InputParameters.InputBirthDate($"Escreva a data de nascimento do(a) estudante", ref age);
            parameters["BirthDate"] = birthDate;

            parameters["Nationality"] = InputParameters.InputNationality($"Escreva a nacionalidade do(a) estudante");
            parameters["Email"] = InputParameters.InputEmail($"Escreva o email do(a) estudante");

            // --- Campos específicos do GraduateStudent ---
            parameters["Major"] = InputParameters.InputCourse();
            parameters["Year"] = InputParameters.InputInt($"Escreva ano atual do(a) estudante", 1, 4);

            parameters["ThesisTopic"] = InputParameters.InputName("Escreva o tema da dissertação/tese");
            parameters["Advisor"] = InputParameters.InputTeacher("Selecione o(a) orientador(a)");

        },
        dict => new GraduateStudent(
            (int)dict["ID"],
            (string)dict["Name"],
            (byte)dict["Age"],
            (char)dict["Gender"],
            (DateTime)dict["BirthDate"],
            (Nationality_e)dict["Nationality"],
            (string)dict["Email"],
            (Course?)dict["Major"],
            (int)dict["Year"],
            null,
            (string)dict["ThesisTopic"],
            (Teacher?)dict["Advisor"]
        ));
    }




    public static void Remove() { RemoveEntity<GraduateStudent>("estudante de mestrado ou doutoramento", FileManager.DataBaseType.GraduateStudent); }

    internal static void Select() { }

    protected override decimal CalculateTuition()
    {
        // Propina mais alta
        return 2000m;
    }
}

internal class InternationalStudent : Student
{
    // Propriedades Específicas
    [JsonInclude] protected Nationality_e Country { get; set; }
    [JsonInclude] protected VisaState_e VisaStatus { get; set; }

    protected override string FormatToString()
    {
        string baseDesc = base.FormatToString();
        return $"{baseDesc}, País Origem: {Country}, Visto: {VisaStatus}";
    }

    protected override void Introduce() { base.Introduce(); }

    public InternationalStudent() : base() { }

    private InternationalStudent(
        int id, string name, byte age, char gender, DateTime? birthDate, Nationality_e nationality, string email,
         Course? major = null, int year = 1, List<Subject>? enrolledSubjects = null,
        Nationality_e country = default, VisaState_e visaStatus = default)
    : base(id, name, age, gender, birthDate, nationality, email, major, year, enrolledSubjects)
    {
        Country = country;
        VisaStatus = visaStatus;
        if (enrolledSubjects is null) EnrolledSubjects_l = major?.Subjects_l?.Select(s => s).ToList() ?? new List<Subject>();
        else EnrolledSubjects_l = enrolledSubjects;
    }
    internal static InternationalStudent? Create()
    {
        return CreateEntity("estudante internacional", FileManager.DataBaseType.InternationalStudent, parameters =>
        {
            // --- Variáveis temporárias ---



            // --- Campos base ---
            byte age = InputParameters.InputAge($"Escreva a idade do(a) estudante", ref age, null, false, MinAge);
            parameters["Age"] = age;

            parameters["Gender"] = InputParameters.InputGender($"Escreva o gênero do(a) estudante");

            birthDate = InputParameters.InputBirthDate($"Escreva a data de nascimento do(a) estudante", ref birthDate);
            parameters["BirthDate"] = birthDate;

            parameters["Nationality"] = InputParameters.InputNationality($"Escreva a nacionalidade do(a) estudante");
            parameters["Email"] = InputParameters.InputEmail($"Escreva o email do(a) estudante");

            // --- Campos específicos do InternationalStudent ---
            parameters["Major"] = InputParameters.InputCourse();
            parameters["Year"] = InputParameters.InputInt($"Escreva ano atual do(a) estudante", 1, 4);

            parameters["Country"] = InputParameters.InputNationality("Escreva o país de origem do(a) estudante");
            parameters["VisaStatus"] = InputParameters.InputVisaStatus("Escreva o estado do visto");

        },
        dict => new InternationalStudent(
            (int)dict["ID"],
            (string)dict["Name"],
            (byte)dict["Age"],
            (char)dict["Gender"],
            (DateTime)dict["BirthDate"],
            (Nationality_e)dict["Nationality"],
            (string)dict["Email"],
            (Course?)dict["Major"],
            (int)dict["Year"],
            null,
            (Nationality_e)dict["Country"],
            (VisaState_e)dict["VisaStatus"]
        ));
    }




    internal static void Remove() { RemoveEntity<InternationalStudent>("estudante internacional", FileManager.DataBaseType.InternationalStudent); }

    internal static void Select() { WriteLine("Seleção específica de InternationalStudent."); }

    protected override decimal CalculateTuition()
    {
        // Propina internacional (mais cara)
        return 3000m;
    }
}



