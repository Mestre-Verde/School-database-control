/// <summary>Class abstrata de 3º grau, todos os tipos de estudantes herdam esta class</summary>
namespace School_System.Domain.SchoolMembers;

using static System.Console; // Permite usar Write e WriteLine diretamente
using System.Text.Json.Serialization;

using School_System.Domain.CourseProgram;
using School_System.Domain.Scholarship;
using School_System.Application.Utils;
using School_System.Infrastructure.FileManager;
using Schoo_lSystem.Application.Menu;

internal abstract class Student : SchoolMember
{
    [JsonInclude] protected decimal GPA = default; // não vai para o construtor
    [JsonInclude] protected decimal Tuition = default; // não vai para o construtor
    [JsonInclude] protected Course? Major { get; set; }
    [JsonInclude] protected int Year { get; set; }
    [JsonInclude] protected List<Subject> EnrolledSubjects = [];// o estudante vai estar inscrito em disicplinas
    [JsonInclude] List<Bolsa> Scholarships = [];

    protected override string FormatToString()
    {
        string baseDesc = base.FormatToString();
        string? courseName = Major?.Name_s ?? "N/A";
        return $"{baseDesc}, Curso: {courseName}, Ano: {Year}, Disciplinas inscrito(a): {EnrolledSubjects?.Count ?? 0}, GPA: {GPA}, Proprina:{Tuition}€.";
    }

    protected override void Introduce() { Write($"\n🎓 New Student: "); WriteLine(FormatToString()); }

    // Construtor parameterless obrigatório para descerialização JSON
    public Student() : base() { }

    protected Student(int id, string name, byte age, char gender, DateTime? birthDate, Nationality_e nationality, string email,
         Course? major = null, int year = default)
        : base(id, name, age, gender, birthDate, nationality, email)
    {
        Major = major;
        Year = year;
    }

    //----------------------------------
    protected abstract decimal CalculateTuition();

    // esta função vai buscar as notas de cada disciplina, armazena em uma lista e faz a média.
    protected decimal CalculateGPA()
    {
        if (EnrolledSubjects.Count == 0) return 0m;
        decimal totalGrades = 0m;
        // adicionan e soma a nota de cada disciplina
        foreach (Subject subject in EnrolledSubjects) { totalGrades += subject.Grade; }
        return totalGrades / EnrolledSubjects.Count;
    }
    //----------------------------------

    protected static void ManageStudentSubjects(Student student)
    {
        Write(Menu.GetMenuStudentSubjects());
        while (true)
        {
            var option = Menu.MenuStudentSubjects();

            if (option == Menu.EditParamStudentSubjects_e.Back) break;

            switch (option)
            {
                case Menu.EditParamStudentSubjects_e.ListSubjects:
                    ListStudentSubjects(student);
                    break;

                case Menu.EditParamStudentSubjects_e.AddSubject:
                    AddSubjectToStudent(student);
                    break;

                case Menu.EditParamStudentSubjects_e.RemoveSubject:
                    RemoveSubjectFromStudent(student);
                    break;

                case Menu.EditParamStudentSubjects_e.EditSubjectGrade:
                    EditStudentSubjectGrade(student);
                    break;
            }
        }
    }

    protected static void ListStudentSubjects(Student student)
    {
        WriteLine("\n===== Disciplinas inscritas =====");

        if (student.EnrolledSubjects.Count == 0)
        {
            WriteLine("(Nenhuma disciplina inscrita)");
            return;
        }

        for (int i = 0; i < student.EnrolledSubjects.Count; i++)
        {
            var subj = student.EnrolledSubjects[i];
            WriteLine($"[{i}] {subj.Name_s} | ECTS: {subj.ECTS_i} | Professor: {subj.Professor?.Name_s}");
        }
    }

    protected static void AddSubjectToStudent(Student student)
    {
        var search = AskAndSearch<Subject>("disciplina", FileManager.DataBaseType.Subject, allowListAll: true, allowUserSelection: true);

        if (search.IsDatabaseEmpty || search.Results.Count == 0) return;

        var subject = search.Results[0];

        // Verifica se o estudante já está inscrito
        if (student.EnrolledSubjects.Any(s => s.ID_i == subject.ID_i))
        {
            WriteLine("⚠️ O estudante já está inscrito nesta disciplina.");
            return;
        }

        // Verifica se não ultrapassa o limite de ECTS por semestre
        int currentEcts = student.EnrolledSubjects.Sum(s => s.ECTS_i);
        int totalEctsAfterAdd = currentEcts + subject.ECTS_i;

        if (totalEctsAfterAdd > InputParameters.MaxEctsPerSemester)
        {
            WriteLine($"⚠️ Não é possível adicionar '{subject.Name_s}' ({subject.ECTS_i} ECTS). " +
                      $"Total atual: {currentEcts} ECTS, limite por semestre: {InputParameters.MaxEctsPerSemester} ECTS.");
            return;
        }

        // Adiciona a disciplina
        student.EnrolledSubjects.Add(subject);

        // Atualiza Tuition
        student.Tuition = student.CalculateTuition();

        WriteLine($"✅ Disciplina '{subject.Name_s}' adicionada. Total de ECTS agora: {totalEctsAfterAdd}.");
    }

    protected static void RemoveSubjectFromStudent(Student student)
    {
        if (student.EnrolledSubjects.Count == 0)
        {
            WriteLine("O estudante não possui disciplinas.");
            return;
        }

        ListStudentSubjects(student);

        // O prompt deve indicar ao utilizador que 0 ou vazio cancela.
        int userInput = InputParameters.InputInt(
            $"Escolha o índice da disciplina a remover (1 a {student.EnrolledSubjects.Count}, ou vazio para CANCELAR):",
            0,
            student.EnrolledSubjects.Count // Máximo é o número total de itens (Count)
        );

        // Verifica se o utilizador cancelou (input vazio, ou se introduziu 0).
        if (userInput == 0)
        {
            WriteLine("\nOperação de remoção cancelada.");
            return;
        }

        // 4. Converte o input de base 1 para índice de base 0
        // Como 1 é o primeiro item válido, ele deve mapear para o índice 0.
        int listIndex = userInput - 1;

        // 5. Remove a disciplina usando o índice de base 0
        var removed = student.EnrolledSubjects[listIndex];
        student.EnrolledSubjects.RemoveAt(listIndex);

        // 6. Atualiza dados
        student.GPA = student.CalculateGPA();
        student.Tuition = student.CalculateTuition();

        WriteLine($"✔️ Disciplina '{removed.Name_s}' removida com sucesso.");
    }

    protected static void EditStudentSubjectGrade(Student student)
    {
        if (student.EnrolledSubjects.Count == 0)
        {
            WriteLine("O estudante não está inscrito em nenhuma disciplina.");
            return;
        }

        // Listar disciplinas
        WriteLine("\nDisciplinas inscritas:");
        for (int i = 0; i < student.EnrolledSubjects.Count; i++)
            WriteLine($"[{i + 1}] {student.EnrolledSubjects[i].Name_s} (Nota: {student.EnrolledSubjects[i].Grade})");

        // Escolher disciplina
        Write($"Escolha a disciplina para adicionar/alterar nota (1-{student.EnrolledSubjects.Count}, Enter para cancelar): ");
        string? choiceInput = ReadLine()?.Trim();
        if (string.IsNullOrEmpty(choiceInput)) return;

        if (!int.TryParse(choiceInput, out int choice) || choice < 1 || choice > student.EnrolledSubjects.Count)
        {
            WriteLine("Entrada inválida. Operação cancelada.");
            return;
        }

        var subject = student.EnrolledSubjects[choice - 1];

        // Pedir nota usando InputInt
        int grade = InputParameters.InputInt($"Escreva a nota para '{subject.Name_s}' (0-20)", 0, 20, subject.Grade, true);

        // Atualizar diretamente a nota da disciplina
        subject.Grade = grade;

        // Recalcular GPA automaticamente
        student.GPA = student.CalculateGPA();

        WriteLine($"Nota atualizada: {subject.Name_s} → {subject.Grade}");
    }


}