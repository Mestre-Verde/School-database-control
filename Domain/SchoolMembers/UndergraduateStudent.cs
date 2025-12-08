namespace School_System.Domain.SchoolMembers;

using static System.Console;

using School_System.Infrastructure.FileManager;
using School_System.Domain.CourseProgram;
using School_System.Application.Utils;
using Schoo_lSystem.Application.Menu;
using System.Text.Json;

internal class UndergraduateStudent : Student
{
    // Propriedades deste objeto
    protected override string FormatToString() { return base.FormatToString(); }
    protected override void Introduce() { base.Introduce(); }

    // Construtor parameterless obrigatório para JSON
    public UndergraduateStudent() { }

    private UndergraduateStudent(int id, string name, byte age, char gender, DateTime? birthDate, Nationality_e nationality, string email,
                                Course? major = null, int year = default)
        : base(id, name, age, gender, birthDate, nationality, email, major, year)
    {
        Introduce();
    }

    internal static UndergraduateStudent? Create()
    {
        return CreateEntity("do(a) estudante de CETEsP ou Licenciatura", FileManager.DataBaseType.UndergraduateStudent, static parameters =>
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

            Course? course = InputParameters.InputCourse();
            parameters["Major"] = course!;
            parameters["Year"] = InputParameters.InputInt($"Escreva o ano atual do(a) estudante", 1, InputParameters.MaxCourseYear);
        },
        dict => new UndergraduateStudent(
            (int)dict["ID"],
            (string)dict["Name"],
            (byte)dict["Age"],
            (char)dict["Gender"],
            dict["BirthDate"] is DateTime dt ? dt : null,
            (Nationality_e)dict["Nationality"],
            (string)dict["Email"],
            dict["Major"] is Course c ? c : null,
            (int)dict["Year"]
        ));
    }

    internal static void Remove() { RemoveEntity<UndergraduateStudent>("estudante de CETEsP ou Licenciatura", FileManager.DataBaseType.UndergraduateStudent); }

    internal static void Select() { SelectEntity<UndergraduateStudent>("estudante CETEsP/Licenciatura", FileManager.DataBaseType.UndergraduateStudent, EditUndergraduateStudent); }

    private static void EditUndergraduateStudent(UndergraduateStudent student)
    {
        // 1. Guardar estado original (deep copy via JSON)
        var original = JsonSerializer.Deserialize<UndergraduateStudent>(JsonSerializer.Serialize(student))!;
        bool hasChanged = false;

        Write(Menu.GetMenuEditUndergraduateStudent());

        while (true)
        {
            var option = Menu.MenuEditUndergraduateStudent();
            if (option == Menu.EditParamStudent_e.Back) break;

            switch (option)
            {
                case Menu.EditParamStudent_e.Help:
                    PrintComparison(student, original);
                    break;

                case Menu.EditParamStudent_e.Name:
                    student.Name_s = InputParameters.InputName("Escreva o nome do estudante", student.Name_s, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamStudent_e.Age:
                    DateTime? tmp = student.BirthDate_dt;
                    student.Age_by = InputParameters.InputAge("Escreva a idade", ref tmp, student.Age_by, true, InputParameters.MinAge);
                    if (tmp.HasValue) student.BirthDate_dt = tmp.Value;
                    hasChanged = true;
                    break;

                case Menu.EditParamStudent_e.Gender:
                    student.Gender_c = InputParameters.InputGender("Escreva o gênero", student.Gender_c, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamStudent_e.BirthDate:
                    byte ageTemp = student.Age_by;
                    student.BirthDate_dt = InputParameters.InputBirthDate("Escreva a data de nascimento", ref ageTemp, InputParameters.MinAge, student.BirthDate_dt, true);
                    student.Age_by = ageTemp;
                    hasChanged = true;
                    break;

                case Menu.EditParamStudent_e.Nationality:
                    student.Nationality = InputParameters.InputNationality("Escreva a nacionalidade", student.Nationality, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamStudent_e.Email:
                    student.Email_s = InputParameters.InputEmail("Escreva o email", student.Email_s, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamStudent_e.Major:
                    student.Major = InputParameters.InputCourse(currentCourse: student.Major, isToEdit: true);
                    hasChanged = true;
                    break;

                case Menu.EditParamStudent_e.Year:
                    student.Year = InputParameters.InputInt("Escreva o ano atual", 1, InputParameters.MaxCourseYear, student.Year, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamStudent_e.ManageSubjects:
                    ManageStudentSubjects(student);
                    hasChanged = true;
                    break;
            }
        }

        if (!hasChanged) return;

        Write("\nGuardar alterações? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) == "S")
        {
            FileManager.WriteOnDataBase(FileManager.DataBaseType.UndergraduateStudent, student);
            WriteLine("✔️ Alterações salvas.");
        }
        else
        {
            WriteLine("❌ Alterações descartadas.");

            // reverte o objeto inteiro (copia cada campo mas para ficar igual ao original)
            student.Name_s = original.Name_s;
            student.Age_by = original.Age_by;
            student.Gender_c = original.Gender_c;
            student.BirthDate_dt = original.BirthDate_dt;
            student.Nationality = original.Nationality;
            student.Email_s = original.Email_s;
            student.Major = original.Major;
            student.Year = original.Year;
            student.EnrolledSubjects = original.EnrolledSubjects;
        }
    }

    // calculo da propina
    protected override decimal CalculateTuition()
    {
        const decimal pricePerEcts = 50m;
        int totalEcts = 0;
        // Somar os ECTS de cada disciplina inscrita
        foreach (Subject subject in EnrolledSubjects) { totalEcts += subject.ECTS_i; }
        return totalEcts * pricePerEcts;
    }
}