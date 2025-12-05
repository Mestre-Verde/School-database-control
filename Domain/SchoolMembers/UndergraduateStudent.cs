namespace School_System.Domain.SchoolMembers;

using static System.Console;
using School_System.Infrastructure.FileManager;
using School_System.Domain.CourseProgram;
using School_System.Application.Utils;
using Schoo_lSystem.Application.Menu;

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
        Introduce();
    }

    internal static UndergraduateStudent? Create()
    {
        return CreateEntity("do(a) estudante de CETEsP ou Licenciatura", FileManager.DataBaseType.UndergraduateStudent, parameters =>
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
            parameters["Year"] = InputParameters.InputInt($"Escreva o ano atual do(a) estudante", 1, 4);
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
            (int)dict["Year"],
            null
        ));
    }

    internal static void Remove() { RemoveEntity<UndergraduateStudent>("estudante de CETEsP ou Licenciatura", FileManager.DataBaseType.UndergraduateStudent); }

    internal static void Select()
    {
        // Pesquisa um estudante usando AskAndSearch
        var selected = AskAndSearch<UndergraduateStudent>("estudante CETEsP/Licenciatura", FileManager.DataBaseType.UndergraduateStudent);
        if (selected.Count == 0) return;

        UndergraduateStudent student = selected[0];
        EditUndergraduateStudent(student); // Menu de edição específico da classe
    }

    private static void PrintUndergraduateStudentComparison(UndergraduateStudent current, dynamic original)
    {
        WriteLine("\n===== 🛈 ESTADO DO ESTUDANTE =====");
        WriteLine($"{"Campo",-15} | {"Atual",-25} | {"Original"}");
        WriteLine(new string('-', 60));

        void Show(string label, object? now, object? old) => WriteLine($"{label,-15} | {now,-25} | {old}");

        Show("Nome", current.Name_s, original.Name_s);
        Show("Idade", current.Age_by, original.Age_by);
        Show("Género", current.Gender_c, original.Gender_c);
        Show("Nascimento", current.BirthDate_dt, original.BirthDate_dt);
        Show("Nacionalidade", current.Nationality, original.Nationality);
        Show("Email", current.Email_s, original.Email_s);

        WriteLine(new string('=', 60));
    }

    private static void EditUndergraduateStudent(UndergraduateStudent student)
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
        };
        bool hasChanged = false;

        // 2. Mostrar menu inicial
        Write(Menu.GetMenuEditUndergraduateStudent());

        // 3. Loop de edição
        while (true)
        {
            var option = Menu.MenuEditUndergraduateStudent();
            if (option == Menu.EditParamStudent_e.Back) break;

            switch (option)
            {
                case Menu.EditParamStudent_e.Help:
                    PrintUndergraduateStudentComparison(student, original);
                    break;

                case Menu.EditParamStudent_e.Name:
                    student.Name_s = InputParameters.InputName("Escreva o nome do estudante", student.Name_s, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamStudent_e.Age:
                    DateTime? tmp = student.BirthDate_dt;
                    student.Age_by = InputParameters.InputAge("Escreva a idade do estudante", ref tmp, student.Age_by, true, MinAge);
                    if (tmp.HasValue) student.BirthDate_dt = tmp.Value;
                    hasChanged = true;
                    break;

                case Menu.EditParamStudent_e.Gender:
                    student.Gender_c = InputParameters.InputGender("Escreva o gênero do estudante", student.Gender_c, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamStudent_e.BirthDate:
                    byte ageTemp = student.Age_by;
                    student.BirthDate_dt = InputParameters.InputBirthDate("Escreva a data de nascimento do estudante", ref ageTemp, MinAge, student.BirthDate_dt, true);
                    student.Age_by = ageTemp;
                    hasChanged = true;
                    break;

                case Menu.EditParamStudent_e.Nationality:
                    student.Nationality = InputParameters.InputNationality("Escreva a nacionalidade do estudante", student.Nationality, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamStudent_e.Email:
                    student.Email_s = InputParameters.InputEmail("Escreva o email do estudante", student.Email_s, true);
                    hasChanged = true;
                    break;
            }
        }

        // 4. Concluir alterações
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
            // Reverter para valores originais
            student.Name_s = original.Name_s;
            student.Age_by = original.Age_by;
            student.Gender_c = original.Gender_c;
            student.BirthDate_dt = original.BirthDate_dt;
            student.Nationality = original.Nationality;
            student.Email_s = original.Email_s;
        }
    }

    protected override decimal CalculateTuition()
    {
        // Propina base
        return 1000m;
    }
}