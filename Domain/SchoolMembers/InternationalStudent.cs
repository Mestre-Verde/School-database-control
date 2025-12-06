namespace School_System.Domain.SchoolMembers;

using static System.Console; // Permite usar Write e WriteLine diretamente
using System.Text.Json.Serialization;// para incluir os atributos

using School_System.Infrastructure.FileManager;
using School_System.Domain.CourseProgram;
using School_System.Application.Utils;
using Schoo_lSystem.Application.Menu;

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

    private InternationalStudent(int id, string name, byte age, char gender, DateTime? birthDate, Nationality_e nationality, string email,
                                Course? major = null, int year = default, List<Subject>? enrolledSubjects = null,
                                Nationality_e country = default, VisaState_e visaStatus = default)
    : base(id, name, age, gender, birthDate, nationality, email, major, year, enrolledSubjects)
    {
        Country = country;
        VisaStatus = visaStatus;

        Introduce();
    }

    internal static InternationalStudent? Create()
    {
        return CreateEntity("estudante internacional", FileManager.DataBaseType.InternationalStudent, parameters =>
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
            // Student 
            parameters["Major"] = InputParameters.InputCourse();
            parameters["Year"] = InputParameters.InputInt($"Escreva ano atual do(a) estudante", 1, 4);
            // --- Campos específicos do InternationalStudent ---
            parameters["Country"] = InputParameters.InputNationality("Escreva o país de origem do(a) estudante");
            parameters["VisaStatus"] = InputParameters.InputVisaStatus("Escreva o estado do visto");

        },
        dict => new InternationalStudent(
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

            dict["Country"] is Nationality_e country ? country : Nationality_e.Other,
            (VisaState_e)dict["VisaStatus"]
        ));
    }

    internal static void Remove() { RemoveEntity<InternationalStudent>("estudante internacional", FileManager.DataBaseType.InternationalStudent); }

    internal static void Select() { SelectEntity<InternationalStudent>("estudante internacional", FileManager.DataBaseType.InternationalStudent, EditInternationalStudent); }

    private static void PrintInternationalStudentComparison(InternationalStudent current, dynamic original)
    {
        WriteLine("\n===== 🛈 ESTADO DO ESTUDANTE INTERNACIONAL =====");
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
        Show("País Origem", current.Country, original.Country);
        Show("Visto", current.VisaStatus, original.VisaStatus);

        WriteLine(new string('=', 60));
    }

    private static void EditInternationalStudent(InternationalStudent student)
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
            student.Year,
            student.Country,
            student.VisaStatus
        };

        bool hasChanged = false;

        // 2. Mostrar menu inicial
        Write(Menu.GetMenuEditInternationalStudent());

        // 3. Loop de edição
        while (true)
        {
            var option = Menu.MenuEditInternationalStudent();
            if (option == Menu.EditParamInternationalStudent_e.Back) break;

            switch (option)
            {
                case Menu.EditParamInternationalStudent_e.Help:
                    PrintInternationalStudentComparison(student, original);
                    break;

                case Menu.EditParamInternationalStudent_e.Name:
                    student.Name_s = InputParameters.InputName("Escreva o nome do(a) estudante", student.Name_s, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamInternationalStudent_e.Age:
                    DateTime? tmp = student.BirthDate_dt;
                    student.Age_by = InputParameters.InputAge("Escreva a idade do(a) estudante", ref tmp, student.Age_by, true, InputParameters.MinAge);
                    if (tmp.HasValue) student.BirthDate_dt = tmp.Value;
                    hasChanged = true;
                    break;

                case Menu.EditParamInternationalStudent_e.Gender:
                    student.Gender_c = InputParameters.InputGender("Escreva o gênero do(a) estudante", student.Gender_c, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamInternationalStudent_e.BirthDate:
                    byte ageTemp = student.Age_by;
                    student.BirthDate_dt = InputParameters.InputBirthDate("Escreva a data de nascimento do(a) estudante", ref ageTemp, InputParameters.MinAge, student.BirthDate_dt, true);
                    student.Age_by = ageTemp;
                    hasChanged = true;
                    break;

                case Menu.EditParamInternationalStudent_e.Nationality:
                    student.Nationality = InputParameters.InputNationality("Escreva a nacionalidade do(a) estudante", student.Nationality, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamInternationalStudent_e.Email:
                    student.Email_s = InputParameters.InputEmail("Escreva o email do(a) estudante", student.Email_s, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamInternationalStudent_e.Major:
                    student.Major = InputParameters.InputCourse(currentCourse: student.Major, isToEdit: true);
                    hasChanged = true;
                    break;

                case Menu.EditParamInternationalStudent_e.Year:
                    student.Year = InputParameters.InputInt("Escreva o ano atual", 1, InputParameters.MaxCourseYear, student.Year, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamInternationalStudent_e.Country:
                    student.Country = InputParameters.InputNationality("Escreva o país de origem", student.Country, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamInternationalStudent_e.VisaStatus:
                    student.VisaStatus = InputParameters.InputVisaStatus("Escreva o estado do visto", student.VisaStatus, true);
                    hasChanged = true;
                    break;
            }
        }

        // 4. Concluir alterações
        if (!hasChanged) return;

        Write("\nGuardar alterações? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) == "S")
        {
            FileManager.WriteOnDataBase(FileManager.DataBaseType.InternationalStudent, student);
            WriteLine("✔️ Alterações salvas.");
        }
        else
        {
            WriteLine("❌ Alterações descartadas.");

            // Reverter
            student.Name_s = original.Name_s;
            student.Age_by = original.Age_by;
            student.Gender_c = original.Gender_c;
            student.BirthDate_dt = original.BirthDate_dt;
            student.Nationality = original.Nationality;
            student.Email_s = original.Email_s;
            student.Major = null; // nome não volta, mas não tinhas referência ao objeto original
            student.Year = original.Year;
            student.Country = original.Country;
            student.VisaStatus = original.VisaStatus;
        }
    }

    protected override decimal CalculateTuition()
    {
        // Propina internacional (mais cara)
        return 3000m;
    }
}