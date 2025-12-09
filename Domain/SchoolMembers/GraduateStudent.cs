/// <summary>Class que cria objetos do tipo GraduateStudent</summary>
namespace School_System.Domain.SchoolMembers;

using static System.Console; // Permite usar Write e WriteLine diretamente
using System.Text.Json.Serialization;// para incluir os atributos

using School_System.Infrastructure.FileManager;
using Schoo_lSystem.Application.Menu;
using School_System.Domain.CourseProgram;
using School_System.Application.Utils;
using System.Text.Json;

internal class GraduateStudent : Student
{
    // Propriedades deste objeto
    [JsonInclude] protected string ThesisTopic = "";// para quando descerializar n\ao ter nome null
    [JsonInclude] protected Teacher? Advisor = null;

    protected override string FormatToString() // Adiciona os campos específicos na descrição
    {
        string baseDesc = base.FormatToString();
        string advisorName = Advisor?.Name_s ?? "N/A";
        return $"{baseDesc}, Tema Dissertação/Tese: '{ThesisTopic}', Orientador: {advisorName}";
    }
    protected override void Introduce() { base.Introduce(); }

    // Construtor parameterless obrigatório para JSON
    public GraduateStudent() : base() { }

    private GraduateStudent(int id, string name, byte age, char gender, DateTime? birthDate, Nationality_e nationality, string email,
                            Course? major = null, int year = default,
                            string thesisTopic = default!, Teacher? advisor = default)
        : base(id, name, age, gender, birthDate, nationality, email, major, year)
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
            DateTime? _trash = null;

            // --- Campos base ---
            byte age = InputParameters.InputAge($"Escreva a idade do(a) estudante", ref _trash, null, false, InputParameters.MinAge);
            DateTime birthDate = InputParameters.InputBirthDate($"Escreva a data de nascimento do(a) estudante", ref age, InputParameters.MinAge);

            parameters["Age"] = age;
            parameters["Gender"] = InputParameters.InputGender($"Escreva o gênero do(a) estudante");
            parameters["BirthDate"] = birthDate;
            parameters["Nationality"] = InputParameters.InputNationality($"Escreva a nacionalidade do(a) estudante");
            parameters["Email"] = InputParameters.InputEmail($"Escreva o email do(a) estudante");
            //Student
            Course? course = InputParameters.InputCourse();
            parameters["Major"] = course!;
            parameters["Year"] = InputParameters.InputInt($"Escreva ano atual do(a) estudante", 1, InputParameters.MaxCourseYear);

            parameters["ThesisTopic"] = InputParameters.InputName("Escreva o tema da dissertação/tese");
            Teacher? teacher = InputParameters.InputTeacher("Selecione o(a) orientador(a)");
            parameters["Advisor"] = teacher!;
        },
        dict => new GraduateStudent(
            (int)dict["ID"],
            (string)dict["Name"],
            (byte)dict["Age"],
            (char)dict["Gender"],
            dict["BirthDate"] is DateTime dt ? dt : DateTime.Now,
            (Nationality_e)dict["Nationality"],
            (string)dict["Email"],

            (Course)dict["Major"],
            (int)dict["Year"],

            (string)dict["ThesisTopic"],
            (Teacher)dict["Advisor"]
        ));
    }

    public static void Remove() { RemoveEntity<GraduateStudent>("estudante de mestrado ou doutoramento", FileManager.DataBaseType.GraduateStudent); }

    internal static void Select() { SelectEntity<GraduateStudent>("estudante de mestrado/doutoramento", FileManager.DataBaseType.GraduateStudent, EditGraduateStudent); }

    private static void EditGraduateStudent(GraduateStudent student)
    {
        // 1. Guardar estado original (deep copy via JSON)
        var original = JsonSerializer.Deserialize<GraduateStudent>(JsonSerializer.Serialize(student))!;
        bool hasChanged = false;

        Write(Menu.GetMenuEditGraduateStudent());

        while (true)
        {
            var option = Menu.MenuEditGraduateStudent();
            if (option == Menu.EditParamGraduateStudent_e.Back) break;

            switch (option)
            {
                case Menu.EditParamGraduateStudent_e.Help:
                    PrintComparison(student, original);
                    break;

                case Menu.EditParamGraduateStudent_e.Name:
                    student.Name_s = InputParameters.InputName("Escreva o nome do(a) estudante", student.Name_s, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.Age:
                    DateTime? tmp = student.BirthDate_dt;
                    student.Age_by = InputParameters.InputAge("Escreva a idade do(a) estudante", ref tmp, student.Age_by, true, InputParameters.MinAge);
                    if (tmp.HasValue) student.BirthDate_dt = tmp.Value;
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.Gender:
                    student.Gender_c = InputParameters.InputGender("Escreva o gênero do(a) estudante", student.Gender_c, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.BirthDate:
                    byte ageTemp = student.Age_by;
                    student.BirthDate_dt = InputParameters.InputBirthDate("Escreva a data de nascimento do(a) estudante", ref ageTemp, InputParameters.MinAge, student.BirthDate_dt, true);
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

                case Menu.EditParamGraduateStudent_e.ManageSubjects:
                    ManageStudentSubjects(student);
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.Major:
                    student.Major = InputParameters.InputCourse(currentCourse: student.Major, isToEdit: true);
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.Year:
                    student.Year = InputParameters.InputInt("Escreva o ano atual", 1, InputParameters.MaxCourseYear, student.Year, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.ThesisTopic:
                    student.ThesisTopic = InputParameters.InputName("Escreva o tema da dissertação/tese", student.ThesisTopic, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamGraduateStudent_e.Advisor:
                    student.Advisor = InputParameters.InputTeacher(currentTeacher: student.Advisor, isEditing: true);
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
            WriteLine("✔️ Alterações salvas.");
        }
        else
        {
            WriteLine("❌ Alterações descartadas.");

            // Reverter objeto inteiro para o original
            student.Name_s = original.Name_s;
            student.Age_by = original.Age_by;
            student.Gender_c = original.Gender_c;
            student.BirthDate_dt = original.BirthDate_dt;
            student.Nationality = original.Nationality;
            student.Email_s = original.Email_s;
            student.Major = original.Major;
            student.Year = original.Year;
            student.ThesisTopic = original.ThesisTopic;
            student.Advisor = original.Advisor;
            student.EnrolledSubjects = original.EnrolledSubjects;
        }
    }

    // calculo da propina
    protected override decimal CalculateTuition()
    {
        const decimal pricePerEcts = 80m;
        int totalEcts = 0;
        // Somar os ECTS de cada disciplina inscrita
        foreach (Subject subject in EnrolledSubjects) { totalEcts += subject.ECTS_i; }
        return totalEcts * pricePerEcts;
    }
}