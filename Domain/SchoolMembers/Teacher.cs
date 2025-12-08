namespace School_System.Domain.SchoolMembers;

using static System.Console; // Permite usar Write e WriteLine diretamente
using System.Text.Json.Serialization;

using School_System.Infrastructure.FileManager;
using Schoo_lSystem.Application.Menu;
using School_System.Application.Utils;

internal class Teacher : SchoolMember
{
    [JsonInclude] internal string Department_s { get; private set; } = "";

    protected override string FormatToString()
    {
        string baseDesc = BaseFormat();
        return $"{baseDesc}, Idade={Age_by}, Gênero={Gender_c},Nascimento={BirthDate_dt:yyyy-MM-dd}, Nacionalidade={Nationality}, Email={Email_s}, Departamento:{Department_s}.";
    }

    protected override void Introduce() { Write($"\n👨‍🏫 New Teacher: "); WriteLine(FormatToString()); }

    public Teacher() : base() { }
    private Teacher(string name, byte age, int id, char gender, DateTime birthDate, Nationality_e nat, string email,
                    string department)
                    : base(id, name, age, gender, birthDate, nationality: nat, email)
    {
        Department_s = department;
        Introduce();
    }

    // Fábrica de objetos Teacher. Pode retornar null se o utilizador cancelar
    internal static Teacher? Create()
    {
        return CreateEntity("do(a) professor(a)", FileManager.DataBaseType.Teacher,
            // Primeiro os campos específicos
            parameters =>
            {
                // --- Variáveis temporárias ---
                DateTime? _trash = null;

                // --- Campos base ---
                byte age = InputParameters.InputAge($"Escreva a idade do(a) professor(a)", ref _trash, null, false, InputParameters.MinAge);
                DateTime birthDate = InputParameters.InputBirthDate($"Escreva a data de nascimento do(a) professor(a)", ref age, InputParameters.MinAge);

                parameters["Age"] = age;
                parameters["Gender"] = InputParameters.InputGender($"Escreva o gênero do(a) professor(a)");
                parameters["BirthDate"] = birthDate;
                parameters["Nationality"] = InputParameters.InputNationality($"Escreva a nacionalidade do(a) professor(a)");
                parameters["Email"] = InputParameters.InputEmail($"Escreva o email do(a) professor(a)");

                parameters["Department"] = InputParameters.InputName("Departamento do(a) professor(a)");
            },

            // Depois o factory para criar o objeto
            parameters => new Teacher(
                (string)parameters["Name"],
                (byte)parameters["Age"],
                (int)parameters["ID"],
                (char)parameters["Gender"],
                (DateTime)parameters["BirthDate"],
                (Nationality_e)parameters["Nationality"],
                (string)parameters["Email"],
                (string)parameters["Department"]
            )
        );
    }

    internal static void Remove() { RemoveEntity<Teacher>("professor", FileManager.DataBaseType.Teacher); }

    internal static void Select() { SelectEntity<Teacher>("disciplina", FileManager.DataBaseType.Teacher, EditTeacher); }

    private static void PrintTeacherComparison(Teacher current, dynamic original)
    {
        WriteLine("\n===== 🛈 ESTADO DO PROFESSOR =====");
        WriteLine($"{"Campo",-15} | {"Atual",-25} | {"Original"}");
        WriteLine(new string('-', 60));

        void Show(string label, object? now, object? old) => WriteLine($"{label,-15} | {now,-25} | {old}");

        Show("Nome", current.Name_s, original.Name_s);
        Show("Idade", current.Age_by, original.Age_by);
        Show("Género", current.Gender_c, original.Gender_c);
        Show("Nascimento", current.BirthDate_dt, original.BirthDate_dt);
        Show("Nacionalidade", current.Nationality, original.Nationality);
        Show("Email", current.Email_s, original.Email_s);
        Show("Departamento", current.Department_s, original.Department_s);

        WriteLine(new string('=', 60));
    }

    private static void EditTeacher(Teacher teacher)
    {
        // 1. Guardar estado original
        var original = new
        {
            teacher.Name_s,
            teacher.Age_by,
            teacher.Gender_c,
            teacher.BirthDate_dt,
            teacher.Nationality,
            teacher.Email_s,
            teacher.Department_s
        };
        bool hasChanged = false;

        // 2. Mostrar menu inicial
        Write(Menu.GetMenuEditTeacher());

        // 3. Loop de edição
        while (true)
        {
            var option = Menu.MenuEditTeacher();
            if (option == Menu.EditParamTeacher_e.Back) break;

            switch (option)
            {
                case Menu.EditParamTeacher_e.Help:
                    PrintTeacherComparison(teacher, original);
                    break;

                case Menu.EditParamTeacher_e.Name:
                    teacher.Name_s = InputParameters.InputName("Escreva o nome do(a) professor(a)", teacher.Name_s, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamTeacher_e.Age:
                    DateTime? tmp = teacher.BirthDate_dt;
                    teacher.Age_by = InputParameters.InputAge("Escreva a idade do(a) professor(a)", ref tmp, teacher.Age_by, true, InputParameters.MinAge);
                    if (tmp.HasValue) teacher.BirthDate_dt = tmp.Value;
                    hasChanged = true;
                    break;

                case Menu.EditParamTeacher_e.Gender:
                    teacher.Gender_c = InputParameters.InputGender("Escreva o gênero do(a) professor(a)", teacher.Gender_c, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamTeacher_e.BirthDate:
                    byte ageTemp = teacher.Age_by;
                    teacher.BirthDate_dt = InputParameters.InputBirthDate("Escreva a data de nascimento do(a) professor(a)", ref ageTemp, 20, teacher.BirthDate_dt, true);
                    teacher.Age_by = ageTemp;
                    hasChanged = true;
                    break;

                case Menu.EditParamTeacher_e.Nationality:
                    teacher.Nationality = InputParameters.InputNationality("Escreva a nacionalidade do(a) professor(a)", teacher.Nationality, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamTeacher_e.Email:
                    teacher.Email_s = InputParameters.InputEmail("Escreva o email do professor(a)", teacher.Email_s, true);
                    hasChanged = true;
                    break;

                case Menu.EditParamTeacher_e.Department:
                    teacher.Department_s = InputParameters.InputName("Escreva o nome do departamento", teacher.Department_s, true);
                    hasChanged = true;
                    break;
            }
        }

        // 4. Concluir alterações
        if (!hasChanged) return;

        Write("\nGuardar alterações? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) == "S")
        {
            FileManager.WriteOnDataBase(FileManager.DataBaseType.Teacher, teacher);
            WriteLine("✔️ Alterações salvas.");
        }
        else
        {
            WriteLine("❌ Alterações descartadas.");
            // Reverter para valores originais
            teacher.Name_s = original.Name_s;
            teacher.Age_by = original.Age_by;
            teacher.Gender_c = original.Gender_c;
            teacher.BirthDate_dt = original.BirthDate_dt;
            teacher.Nationality = original.Nationality;
            teacher.Email_s = original.Email_s;
            teacher.Department_s = original.Department_s;
        }
    }
}

