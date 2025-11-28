namespace School_System.Domain.SchoolMembers;

using static System.Console; // Permite usar Write e WriteLine diretamente
using System.Text.Json.Serialization;

using School_System.Infrastructure.FileManager;
using Schoo_lSystem.Application.Menu;
using School_System.Domain.Base;
using School_System.Domain.CourseProgram;
using School_System.Domain.SchoolMembers;

class Student : SchoolMember
{
    [JsonInclude] internal int TutorId_i { get; private set; } = -1;
    [JsonInclude] internal List<double> Grades_i { get; private set; } = [];// Lista de notas
    [JsonIgnore] internal decimal GPA_d = default;//GPA = média das notas. vai ser calculado em ls não precisa de ser guardado
    protected override string Describe() //ToString
    {
        return $"ID={ID_i}, Nome='{Name_s}', Idade={Age_by}, Gênero={Gender_c},Nascimento={BirthDate_dt:yyyy-MM-dd}, Nacionalidade={Nationality}, Email={Email_s}, Tutor:{TutorId_i}.";
    }

    // Construtor parameterless obrigatório para JSON
    public Student() : base() { }

    protected Student(string name, byte age, int id, char gender, DateTime birthDate, Nationality_e nat, string email, int tutorId_i)
     : base(id, name, age, gender, birthDate, nat, email)
    {
        TutorId_i = tutorId_i;
        Introduce();
    }

    //----------------------------------
    // funções para mudança de Atributos
    //----------------------------------

    /// <summary> Solicita ao usuário que informe ou edite o ID do tutor. </summary>
    /// <param name="prompt">Mensagem a exibir ao usuário.</param>
    /// <param name="currentValue">Valor atual do TutorId_i, usado em edição.</param>
    /// <param name="isToEdit">Indica se é edição (true) ou criação (false).</param>
    /// <returns>O ID do tutor fornecido pelo usuário ou o valor atual/default se Enter for pressionado.</returns>
    // TODO: implementar uma procura na base de dadso do professor para obter o id.
    protected static int InputTutorId(string prompt, int? currentValue = null, bool isToEdit = false)
    // depois de deixares a função select super modular (colocar na class de 1º grau) colocar aqui para uma melhor seleção dos professores.
    {
        while (true)
        {
            if (isToEdit && currentValue.HasValue && currentValue.Value != -1) Write($"{prompt} (Enter para manter '{currentValue.Value}'): ");
            else Write($"{prompt} (Enter para default '-1'): ");

            string? input = ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                // Mantém valor atual em edição ou default na criação
                return isToEdit && currentValue.HasValue ? currentValue.Value : -1;
            }

            if (int.TryParse(input, out int id) && id >= 0)
                return id;

            WriteLine(InvalidEntrance);
        }
    }

    // Função para ler e retornar notas a adicionar
    private static List<double> EditAddGrades(string prompt = "Adicionar notas (ex: 12.5 15 9) ou Enter para cancelar")
    {
        WriteLine(prompt + ": ");
        string? input = ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            WriteLine(EmptyEntrance);
            return new List<double>(); // lista vazia
        }

        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var added = new List<double>();

        foreach (string p in parts)
        {
            if (double.TryParse(p, out double grade) && grade >= 0 && grade <= 20)
            {
                added.Add(grade);
            }
            else
            {
                WriteLine($"❌ Ignorado: '{p}' não é uma nota válida (0 a 20).");
            }
        }

        if (added.Count > 0) WriteLine($"✅ {added.Count} nota(s) adicionada(s) com sucesso.");
        else WriteLine("Nenhuma nota válida foi adicionada.");

        return added;
    }
    private static void EditGrades(Student student)
    {
        while (true)
        {
            WriteLine("\n--- Edição de Notas ---");
            if (student.Grades_i.Count == 0)
                WriteLine("Nenhuma nota registrada.");
            else
            {
                for (int i = 0; i < student.Grades_i.Count; i++)
                    WriteLine($"[{i}] {student.Grades_i[i]}");
            }

            WriteLine("\nComandos:");
            WriteLine("[A] Adicionar notas");
            WriteLine("[R] Remover nota por índice");
            WriteLine("[S] Substituir nota por índice");
            WriteLine("[B] Voltar");

            Write("(notas)> ");
            string? input = ReadLine()?.Trim().ToUpper();

            switch (input)
            {
                case "A": // Adicionar
                    var added = EditAddGrades(); // mesma função de adicionar notas
                    if (added.Count > 0)
                        student.Grades_i.AddRange(added);
                    break;

                case "R": // Remover
                    Write("Índice da nota a remover: ");
                    if (int.TryParse(ReadLine(), out int removeIdx) && removeIdx >= 0 && removeIdx < student.Grades_i.Count)
                    {
                        WriteLine($"❌ Nota {student.Grades_i[removeIdx]} removida.");
                        student.Grades_i.RemoveAt(removeIdx);
                    }
                    else
                        WriteLine("Índice inválido.");
                    break;

                case "S": // Substituir
                    Write("Índice da nota a substituir: ");
                    if (int.TryParse(ReadLine(), out int replaceIdx) && replaceIdx >= 0 && replaceIdx < student.Grades_i.Count)
                    {
                        Write($"Nova nota para índice {replaceIdx}: ");
                        if (double.TryParse(ReadLine(), out double newGrade) && newGrade >= 0 && newGrade <= 20)
                        {
                            student.Grades_i[replaceIdx] = newGrade;
                            WriteLine("✅ Nota atualizada.");
                        }
                        else WriteLine("❌ Nota inválida.");
                    }
                    else WriteLine("Índice inválido.");
                    break;

                case "B": return;

                default:
                    WriteLine("Comando desconhecido.");
                    break;
            }
        }
    }


    // Fábrica de objetos Student. Pode retornar null se o utilizador cancelar
    internal override BaseEntity? CreateInstance() => Create();
    internal static Student? Create()
    {
        return CreateMember("estudante", FileManager.DataBaseType.Student,
            dict =>
            {
                dict["TutorId_i"] = InputTutorId("ID do tutor");
            },
            dict => new Student(
                (string)dict["Name"],
                (byte)dict["Age"],
                (int)dict["ID"],
                (char)dict["Gender"],
                (DateTime)dict["BirthDate"],
                (Nationality_e)dict["Nationality"],
                (string)dict["Email"],
                (int)dict["TutorId_i"]
            )
        );
    }

    //----------------------------------
    // funções Globais
    //----------------------------------
    internal static void Remove() { RemoveEntity<Student>("aluno", FileManager.DataBaseType.Student); }
    internal override void Introduce()
    {
        WriteLine($"\n🎓 New Student: {Name_s}, ID: {ID_i}, Age: {Age_by}, Genero: {Gender_c}, Data de nascimento: {BirthDate_dt.Date}, Nacionalidade: {Nationality}.");
    }

    internal static void Select()
    {
        var selected = AskAndSearch<Student>("estudante", FileManager.DataBaseType.Student);
        if (selected.Count == 0) return;
        Student student = selected[0];
        EditStudent(student);// Aqui começa o menu de edição específico da classe
    }
    // este métudo é bastante interessante!!!
    internal static void PrintStudentComparison(Student current, dynamic original)
    {
        WriteLine("\n===== 🛈 ESTADO DO ESTUDANTE =====");
        WriteLine($"{"Campo",-15} | {"Atual",-25} | {"Original"}");
        WriteLine(new string('-', 60));

        void Show(string label, object? now, object? old)
            => WriteLine($"{label,-15} | {now,-25} | {old}");

        Show("Nome", current.Name_s, original.Name_s);
        Show("Idade", current.Age_by, original.Age_by);
        Show("Género", current.Gender_c, original.Gender_c);
        Show("Nascimento", current.BirthDate_dt, original.BirthDate_dt);
        Show("Nacionalidade", current.Nationality, original.Nationality);
        Show("Email", current.Email_s, original.Email_s);
        Show("TutorId", current.TutorId_i, original.TutorId_i);

        string nowGrades = current.Grades_i is null ? "(nenhuma)" : string.Join(", ", current.Grades_i);
        string oldGrades = original.Grades is null ? "(nenhuma)" : string.Join(", ", original.Grades);

        Show("Notas", nowGrades, oldGrades);

        WriteLine(new string('=', 60));
    }
    /*
    EditStudent()
     ├─ chama → MenuRelated_cl.GetEditChoiceForStudent()
     │       (mostra menu, lê input, devolve enum UNIFICADO)
     └─ faz switch(baseado no enum devolvido)

    */
    private static void EditStudent(Student student)
    {
        // --- 1. Guardar valores originais ---
        var original = new
        {
            student.Name_s,
            student.Age_by,
            student.Gender_c,
            student.BirthDate_dt,
            student.Nationality,
            student.Email_s,
            student.TutorId_i,
            Grades = student.Grades_i?.ToList() // cópia profunda
        };
        bool hasChanged = false;

        // --- 2. Mostrar menu inicial ---
        Write(MenuRelated_cl.GetMenuEditStudents());

        // --- 3. Loop de edição ---
        while (true)
        {
            var option = MenuRelated_cl.MenuEditStudent();

            if (option == MenuRelated_cl.EditParamStudent_e.Back)
                break;

            switch (option)
            {
                case MenuRelated_cl.EditParamStudent_e.Help:
                    WriteLine(MenuRelated_cl.GetMenuEditStudents());
                    PrintStudentComparison(student, original);
                    break;

                case MenuRelated_cl.EditParamStudent_e.Name:
                    student.Name_s = InputName("Escreva o nome do(a) estudante", student.Name_s, true);
                    hasChanged = true;
                    break;

                case MenuRelated_cl.EditParamStudent_e.Age:
                    DateTime? tmp = student.BirthDate_dt;
                    student.Age_by = InputAge("Escreva a idade do(a) estudante", ref tmp, student.Age_by, true, MinAge);
                    if (tmp.HasValue) student.BirthDate_dt = tmp.Value;
                    hasChanged = true;
                    break;

                case MenuRelated_cl.EditParamStudent_e.Gender:
                    student.Gender_c = InputGender("Escreva o género do(a) estudante", student.Gender_c, true);
                    hasChanged = true;
                    break;

                case MenuRelated_cl.EditParamStudent_e.BirthDate:
                    byte ageTemp = student.Age_by;
                    student.BirthDate_dt = InputBirthDate("Escreva a data de nascimento do(a) estudante", ref ageTemp, student.BirthDate_dt, true);
                    student.Age_by = ageTemp;
                    hasChanged = true;
                    break;

                case MenuRelated_cl.EditParamStudent_e.Nationality:
                    student.Nationality = InputNationality("Escreva a nacionalidade do(a) estudante", student.Nationality, true);
                    hasChanged = true;
                    break;

                case MenuRelated_cl.EditParamStudent_e.Email:
                    student.Email_s = InputEmail("Escreva o email do(a) estudante", student.Email_s, true);
                    hasChanged = true;
                    break;

                case MenuRelated_cl.EditParamStudent_e.Tutor:
                    student.TutorId_i = InputTutorId("Escreva o nome do tutor");
                    hasChanged = true;
                    break;

                case MenuRelated_cl.EditParamStudent_e.Grades:
                    EditGrades(student);
                    hasChanged = true;
                    break;
            }
        }

        // --- 4. Concluir (guardar ou descartar) ---
        if (!hasChanged) return;

        Write("\nGuardar alterações? (S/N): ");
        if ((ReadLine()?.Trim().ToUpper()) == "S")
        {
            FileManager.WriteOnDataBase(FileManager.DataBaseType.Student, student);
            WriteLine("✔️ Alterações salvas.");
        }
        else
        {
            WriteLine("❌ Alterações descartadas.");

            // Reverter estado antigo
            student.Name_s = original.Name_s;
            student.Age_by = original.Age_by;
            student.Gender_c = original.Gender_c;
            student.BirthDate_dt = original.BirthDate_dt;
            student.Nationality = original.Nationality;
            student.Email_s = original.Email_s;
            student.TutorId_i = original.TutorId_i;
            student.Grades_i = original.Grades?.ToList() ?? [];

        }

    }






}
