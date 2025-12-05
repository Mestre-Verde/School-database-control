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

        Introduce();
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
            DateTime birthDate = InputParameters.InputBirthDate($"Escreva a data de nascimento do(a) estudante", ref age, MinAge);
            parameters["BirthDate"] = birthDate;
            parameters["Nationality"] = InputParameters.InputNationality($"Escreva a nacionalidade do(a) estudante");
            parameters["Email"] = InputParameters.InputEmail($"Escreva o email do(a) estudante");
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
            dict["BirthDate"] is DateTime dt ? dt : DateTime.Now,
            (Nationality_e)dict["Nationality"],
            (string)dict["Email"],
            dict["Major"] is Course c ? c : null,
            (int)dict["Year"],
            null,
            (string)dict["ThesisTopic"],
            dict["Advisor"] is Teacher t ? t : null
        ));
    }

    public static void Remove() { RemoveEntity<GraduateStudent>("estudante de mestrado ou doutoramento", FileManager.DataBaseType.GraduateStudent); }

    internal static void Select()
    {
        // Pesquisa um estudante de pós-graduação usando AskAndSearch
        var selected = AskAndSearch<GraduateStudent>(
            "estudante de mestrado/doutoramento",
            FileManager.DataBaseType.GraduateStudent);

        if (selected.Count == 0) return;

        GraduateStudent student = selected[0];
        EditGraduateStudent(student);
    }

    private static void PrintGraduateStudentComparison(GraduateStudent current, dynamic original)
    {
        WriteLine("\n===== ESTADO DO ESTUDANTE Mestrado/Douturamento =====");
        WriteLine($"{"Campo",-15} | {"Atual",-25} | {"Original"}");
        WriteLine(new string('-', 60));

        void Show(string label, object? now, object? old) => WriteLine($"{label,-15} | {now,-25} | {old}");

        Show("Nome", current.Name_s, original.Name_s);
        Show("Idade", current.Age_by, original.Age_by);
        Show("Género", current.Gender_c, original.Gender_c);
        Show("Nasc.", current.BirthDate_dt, original.BirthDate_dt);
        Show("Nacionalidade", current.Nationality, original.Nationality);
        Show("Email", current.Email_s, original.Email_s);
        Show("Curso", current.Major?.Name_s, original.Major_s);
        Show("Ano", current.Year, original.Year);
        Show("Tese", current.ThesisTopic, original.ThesisTopic);
        Show("Orientador", current.Advisor?.Name_s, original.Advisor);

        WriteLine(new string('=', 60));
    }

    private static void EditGraduateStudent(GraduateStudent student)
    {
        // 1. Guardar estado original
        var original = new
        {
            student.Name_s,
            student.Age_by,
            student.Gender_c,
            student.BirthDate_dt,
            student.Nationality,
            student.Email_s,
            Major_s = student.Major?.Name_s ?? "Nenhum",
            Year = student.Year,
            student.ThesisTopic,
            Advisor = student.Advisor?.Name_s ?? "Nenhum"
        };

        bool hasChanged = false;

        // 2. Mostrar menu inicial
        Write(Menu.GetMenuEditGraduateStudent());

        // 3. Loop de edição
        while (true)
        {
            var option = Menu.MenuEditGraduateStudent();
            if (option == Menu.EditParamGraduateStudent_e.Back) break;

            switch (option)
            {
                case Menu.EditParamGraduateStudent_e.Help:
                    PrintGraduateStudentComparison(student, original);
                    break;

                case Menu.EditParamGraduateStudent_e.Name:
                    student.Name_s = InputParameters.InputName("Escreva o nome do(a) estudante", student.Name_s, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.Age:
                    DateTime? tmp = student.BirthDate_dt;
                    student.Age_by = InputParameters.InputAge("Escreva a idade do(a) estudante", ref tmp, student.Age_by, true, MinAge);
                    if (tmp.HasValue) student.BirthDate_dt = tmp.Value;
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.Gender:
                    student.Gender_c = InputParameters.InputGender("Escreva o gênero do(a) estudante", student.Gender_c, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.BirthDate:
                    byte ageTemp = student.Age_by;
                    student.BirthDate_dt = InputParameters.InputBirthDate("Escreva a data de nascimento do(a) estudante", ref ageTemp, MinAge, student.BirthDate_dt, true);
                    student.Age_by = ageTemp;
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.Nationality:
                    student.Nationality = InputParameters.InputNationality("Escreva a nacionalidade do(a) estudante", student.Nationality, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.Email:
                    student.Email_s = InputParameters.InputEmail("Escreva o email do(a) estudante", student.Email_s, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.Major:
                    student.Major = InputParameters.InputCourse();
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.Year:
                    student.Year = InputParameters.InputInt("Escreva o ano atual", 1, 4, student.Year, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.ThesisTopic:
                    student.ThesisTopic = InputParameters.InputName("Escreva o tema da dissertação/tese", student.ThesisTopic, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.Advisor:
                    student.Advisor = InputParameters.InputTeacher("Selecione o(a) orientador(a)");
                    hasChanged = true;
                    break;
            }
        }

        // 4. Concluir alterações
        if (!hasChanged) return;

        Write("\nGuardar alterações? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) == "S")
        {
            FileManager.WriteOnDataBase(FileManager.DataBaseType.GraduateStudent, student);
            WriteLine("Alterações salvas.");
        }
        else
        {
            WriteLine("Alterações descartadas.");

            student.Name_s = original.Name_s;
            student.Age_by = original.Age_by;
            student.Gender_c = original.Gender_c;
            student.BirthDate_dt = original.BirthDate_dt;
            student.Nationality = original.Nationality;
            student.Email_s = original.Email_s;
            student.Major = null;
            student.Year = original.Year;
            student.ThesisTopic = original.ThesisTopic;
            student.Advisor = null;
        }
    }

    protected override decimal CalculateTuition()
    {
        // Propina mais alta
        return 2000m;
    }
}