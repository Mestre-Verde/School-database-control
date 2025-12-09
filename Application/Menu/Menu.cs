/// <summary>Class onde se encontra a maioria da lógica dos menus. </summary>
namespace Schoo_lSystem.Application.Menu;

using static System.Console;

using School_System.Domain.CourseProgram;
using School_System.Domain.SchoolMembers;
using School_System.Infrastructure.FileManager;

public static class Menu
{
    private const string UnknowonCommand_s = "❌ Comando desconhecido.\n";
    private const string BackToMenu_s = "🔙 A voltar ao menu anterior...\n";

    // Menu 1º grau , funções principais
    private enum MainMenuCommands_e
    {
        Exit,        // Sair do programa
        Help,       // Mostrar ajuda
        Add,        // Adicionar algo
        Remove,     // Remover algo
        Select,     // Selecionar um item
        AllEntitys,       // Mostrar todos os dados
        None       // Caso não reconheça o comando
    }
    private const string MainMenuCommands_s = @"
    Comandos disponíveis:
        [0] Exit      -> Sair do programa
        [1] Help      -> Mostrar este texto
        [2] Add       -> Adicionar aluno, professor, etc.
        [3] Remove    -> Remover item
        [4] Select    -> Selecionar item
    ";
    internal static void MainMenu()
    {
        WriteLine(MainMenuCommands_s);// Mostra o menu de comandos
        while (true)
        {
            Write("\n(main menu)> ");
            string? input_s = ReadLine()?.Trim().ToLower();
            switch (input_s)// Primeiro, converte números para texto do enum
            {
                case "0": input_s = "Exit"; break;
                case "1": input_s = "Help"; break;
                case "2": input_s = "Add"; break;
                case "3": input_s = "Remove"; break;
                case "4": input_s = "Select"; break;
            }
            // Agora tenta converter para enum
            if (!Enum.TryParse(input_s, true, out MainMenuCommands_e command)) { command = MainMenuCommands_e.None; }
            switch (command)// Executa o comando
            {
                case MainMenuCommands_e.Exit: WriteLine("👋 A encerrar o programa..."); return;
                case MainMenuCommands_e.Help: Write(MainMenuCommands_s); break;
                case MainMenuCommands_e.Add: MenuAddObject(); break;
                case MainMenuCommands_e.Remove: MenuRemoveObject(); break;
                case MainMenuCommands_e.Select: MenuSelectObject(); break;
                default: Write(UnknowonCommand_s); break;
            }
        }
    }

    // Menu 2º grau , seleção do tipo de objeto
    private enum GlobalObjectCommands_e
    {
        Back,                   // Voltar ao menu principal
        Help,                   // Mostrar ajuda
        UndergraduateStudent,   // Adicionar estudante de graduação
        GraduateStudent,        // Adicionar estudante de pós-graduação
        InternationalStudent,   // Adicionar estudante internacional
        Teacher,                // Adicionar professor
        Course,                 // Adicionar curso
        Subject,
        None                    // Caso não reconheça o comando
    }
    private static string BuildObjectMenu(string typeFunction)
    {
        string novo = "", nova = "";
        if (typeFunction == "Adiciona")
        {
            novo = "novo ";
            nova = "nova ";
        }
        return $@"    Comandos para {typeFunction}r:
        [0] Back                    -> Voltar ao menu principal
        [1] Help                    -> Mostrar este texto
        [2] UndergraduateStudent    -> {typeFunction} um {novo}estudante de graduação
        [3] GraduateStudent         -> {typeFunction} um {novo}estudante de pós-graduação
        [4] InternationalStudent    -> {typeFunction} um {novo}estudante internacional
        [5] Teacher                 -> {typeFunction} um {novo}professor
        [6] Course                  -> {typeFunction} um {novo}curso
        [7] Subject                 -> {typeFunction} uma {nova}disciplina
    ";
    }

    /// <summary>/// Executa um menu interativo com comandos pré-definidos.</summary>
    /// <param name="mainMenuText">Texto identificador do menu (ex.: "add", "remove").</param>
    /// <param name="menuString">Texto a mostrar no menu, para listar os comandos disponíveis.</param>
    /// <param name="actions">
    /// Dicionário que associa cada comando (GlobalObjectCommands_e) a uma ação (Action) a executar.
    /// Cada Action é um delegate que representa um método sem parâmetros e sem retorno (void).
    /// Se o comando for digitado pelo utilizador e existir no dicionário, a Action correspondente é executada.
    /// Caso não exista, é mostrado "Comando desconhecido".
    /// </param>
    /// <remarks>
    /// Funciona assim:
    /// 1. O utilizador digita um comando (pode ser número ou texto).
    /// 2. O comando é convertido para enum GlobalObjectCommands_e.
    /// 3. Se o comando for "Back", sai do menu.
    /// 4. Se for "Help", imprime novamente o menu.
    /// 5. Para outros comandos, o dicionário actions é consultado com TryGetValue:
    ///    - Se existir a chave, executa a Action associada.
    ///    - Se não existir, mostra "Comando desconhecido".
    /// </remarks>
    private static void RunMenu(string mainMenuText, string menuString, Dictionary<GlobalObjectCommands_e, Action> actions)
    {
        WriteLine(menuString); // mostra o menu inicial
        while (true)
        {
            Write($"\n(menu {mainMenuText})> ");
            string? input_s = ReadLine()?.Trim().ToLower();
            // Permite usar números como atalhos no menu
            switch (input_s)
            {
                case "0": input_s = "Back"; break;
                case "1": input_s = "Help"; break;
                case "2": input_s = "UndergraduateStudent"; break;
                case "3": input_s = "GraduateStudent"; break;
                case "4": input_s = "InternationalStudent"; break;
                case "5": input_s = "Teacher"; break;
                case "6": input_s = "Course"; break;
                case "7": input_s = "Subject"; break;
            }
            // Tenta converter o texto para um comando válido enum
            if (!Enum.TryParse(input_s, true, out GlobalObjectCommands_e command)) { command = GlobalObjectCommands_e.None; }
            // Voltar ao menu anterior
            if (command == GlobalObjectCommands_e.Back) { WriteLine(BackToMenu_s); break; }
            // Mostrar novamente o menu
            if (command == GlobalObjectCommands_e.Help) { WriteLine(menuString); continue; }
            // Executa a ação associada ao comando
            if (actions.TryGetValue(command, out Action? action)) { action(); }
            else { Write(UnknowonCommand_s); } // comando inválido
        }
    }
    // Cria um dicionário que associa cada comando do menu a uma ação
    // Aqui usamos Action, que é um delegate que representa um método sem parâmetros e sem retorno
    private static void MenuAddObject()
    {
        var actions = new Dictionary<GlobalObjectCommands_e, Action>
        {
            { GlobalObjectCommands_e.UndergraduateStudent, () => _ = UndergraduateStudent.Create() },
            { GlobalObjectCommands_e.GraduateStudent, () => _ = GraduateStudent.Create() },
            { GlobalObjectCommands_e.InternationalStudent, () => _ = InternationalStudent.Create() },
            { GlobalObjectCommands_e.Teacher, () => _ = Teacher.Create() },
            { GlobalObjectCommands_e.Course,  () => _ = Course.Create() },
            { GlobalObjectCommands_e.Subject, () => _ = Subject.Create() }
        };
        // Chama a função genérica que executa o loop do menu, passando o texto do menu e o dicionário de ações
        RunMenu("Add", BuildObjectMenu("Adiciona"), actions);
    }
    private static void MenuRemoveObject()
    {
        // Dicionário de ações para o menu de remoção
        var actions = new Dictionary<GlobalObjectCommands_e, Action>
        {
            { GlobalObjectCommands_e.UndergraduateStudent, UndergraduateStudent.Remove },
            { GlobalObjectCommands_e.GraduateStudent, GraduateStudent.Remove },
            { GlobalObjectCommands_e.InternationalStudent, InternationalStudent.Remove },
            { GlobalObjectCommands_e.Teacher, Teacher.Remove },
            { GlobalObjectCommands_e.Course,  Course.Remove },
            { GlobalObjectCommands_e.Subject, Subject.Remove}
        };

        // Executa o menu de remoção usando a função genérica
        RunMenu("Remove", BuildObjectMenu("Remove"), actions);
    }
    private static void MenuSelectObject()
    {
        // Dicionário de ações para o menu de seleção
        var actions = new Dictionary<GlobalObjectCommands_e, Action>
        {
            { GlobalObjectCommands_e.UndergraduateStudent, UndergraduateStudent.Select },
            { GlobalObjectCommands_e.GraduateStudent, GraduateStudent.Select },
            { GlobalObjectCommands_e.InternationalStudent, InternationalStudent.Select },
            { GlobalObjectCommands_e.Teacher, Teacher.Select },
            { GlobalObjectCommands_e.Course, Course.Select },
            { GlobalObjectCommands_e.Subject,Subject.Select }
        };

        // Executa o menu de seleção usando a função genérica
        RunMenu("Select", BuildObjectMenu("Seleciona"), actions);
    }

    // Menu 3º grau , enums de SchoolMembers
    internal enum EditParamSchoolMember_e
    {
        Back = 0,
        Help = 1,
        Name = 2,
        Age = 3,
        Gender = 4,
        BirthDate = 5,
        Nationality = 6,
        Email = 7
    }
    internal enum EditParamStudent_e
    {
        Back = 0,
        Help = 1,
        Name = 2,
        Age = 3,
        Gender = 4,
        BirthDate = 5,
        Nationality = 6,
        Email = 7,
        Year = 8,
        Major = 9,
        ManageSubjects = 10
    }
    internal enum EditParamTeacher_e
    {
        Back = 0,
        Help = 1,
        Name = 2,
        Age = 3,
        Gender = 4,
        BirthDate = 5,
        Nationality = 6,
        Email = 7,
        Department = 8
    }
    internal enum EditParamGraduateStudent_e
    {
        Back = 0,
        Help = 1,
        Name = 2,
        Age = 3,
        Gender = 4,
        BirthDate = 5,
        Nationality = 6,
        Email = 7,
        ManageSubjects,
        Major,
        Year,
        ThesisTopic,
        Advisor
    }
    internal enum EditParamInternationalStudent_e
    {
        Back = 0,
        Help = 1,
        Name = 2,
        Age = 3,
        Gender = 4,
        BirthDate = 5,
        Nationality = 6,
        Email = 7,
        ManageSubjects = 8,
        Major = 9,
        Year = 10,
        Country = 11,
        VisaStatus = 12
    }
    internal enum EditParamCourse_e
    {
        Back = 0,
        Help = 1,
        Name = 2,
        Type = 3,
        Duration = 4,
    }
    internal enum EditParamSubject_e
    {
        Back = 0,
        Help = 1,
        Name = 2,
        ECTS = 3,
        Professor = 4,
        Grade = 5
    }

    /// <summary> Função genérica para ler opções de um menu baseado em enums.
    /// A restrição <c>where T : struct, Enum</c> garante que T é um tipo enum válido:
    /// - <c>struct</c> obriga T a ser um tipo por valor (como todos os enums),
    /// - <c>Enum</c> garante que T é especificamente um enum.
    /// Isto permite usar métodos como <c>Enum.TryParse</c> e <c>Enum.IsDefined</c>
    /// com segurança e evita que tipos inválidos sejam passados para a função.
    /// </summary>
    private static T AskMenu<T>(string prompt, string menuText) where T : struct, Enum
    {
        while (true)
        {
            Write($"\n({prompt})> ");
            string? input = ReadLine()?.Trim();

            // --- Help tem prioridade ---
            if (input == "1") // Mostrar menu antes do TryParse
            {
                WriteLine(menuText);
                return (T)Enum.ToObject(typeof(T), 1);
            }

            // --- Atalhos numéricos ---
            if (int.TryParse(input, out int num) && Enum.IsDefined(typeof(T), num)) { return (T)Enum.ToObject(typeof(T), num); }

            // --- Nome do enum ---
            if (Enum.TryParse(input, true, out T result) && Enum.IsDefined(typeof(T), result)) { return result; }

            // --- Entrada inválida ---
            WriteLine("Comando desconhecido.");
        }
    }

    // ------------------ Menus ------------------

    // Geração genérica de menus para membros de escola
    private static string GenerateSchoolMemberMenu(string memberType, string extraParameters = "")
    {
        return $@"
    Editar dados {memberType}:
        [0] Back          -> Voltar & salvar
        [1] Help          -> Mostrar este texto & uma comparação de dados
        [2] Name          -> Alterar o nome
        [3] Age           -> Alterar a idade
        [4] Gender        -> Alterar o género
        [5] BirthDate     -> Alterar a data de nascimento
        [6] Nationality   -> Alterar a nacionalidade
        [7] Email         -> Alterar o email
{extraParameters}";
    }

    // Textos específicos de cada tipo
    private const string MenuEditTeacherExtra = @"        [8] Department    -> Alterar o departamento";
    private const string MenuEditUndergradExtra = @"        [8] Year          -> Alterar o ano atual
        [9] Major         -> Alterar o curso        
        [10] ManageSubjects     -> Adicionar/Editar disciplinas(incluindo notas)
";
    private const string MenuEditGraduateExtra = @"        [8] ManageSubjects     -> Adicionar/Editar disciplinas(incluindo notas)
        [9] Major         -> Alterar o curso
        [10] Year          -> Alterar o ano atual
        [11] ThesisTopic  -> Alterar o tema da dissertação/tese
        [12] Advisor      -> Alterar o orientador
";
    private const string MenuEditInternationalExtra = @"        [8] ManageSubjects    -> Adicionar/Editar notas a dsiciplinas
        [9] Major        -> Alterar o curso
        [10] Year         -> Alterar o ano atual
        [11] Country     -> Alterar o país de origem
        [12] VisaStatus  -> Alterar o estado do visto
";

    // GetMenus
    internal static string GetMenuEditTeacher() => GenerateSchoolMemberMenu("do Professor", MenuEditTeacherExtra);
    internal static string GetMenuEditUndergraduateStudent() => GenerateSchoolMemberMenu("do estudante CETEsP/Licenciatura", MenuEditUndergradExtra);
    internal static string GetMenuEditGraduateStudent() => GenerateSchoolMemberMenu("do estudante de Mestrado/Doutoramento", MenuEditGraduateExtra);
    internal static string GetMenuEditInternationalStudent() => GenerateSchoolMemberMenu("do estudante internacional", MenuEditInternationalExtra);

    // ------------------ Inputs ------------------

    // Chamadas genéricas
    internal static EditParamTeacher_e MenuEditTeacher() => AskMenu<EditParamTeacher_e>("edit teacher", GetMenuEditTeacher());
    internal static EditParamStudent_e MenuEditUndergraduateStudent() => AskMenu<EditParamStudent_e>("edit undergraduate student", GetMenuEditUndergraduateStudent());
    internal static EditParamGraduateStudent_e MenuEditGraduateStudent() => AskMenu<EditParamGraduateStudent_e>("edit graduate student", GetMenuEditGraduateStudent());
    internal static EditParamInternationalStudent_e MenuEditInternationalStudent() => AskMenu<EditParamInternationalStudent_e>("edit international student", GetMenuEditInternationalStudent());

    // ------------------ Subjects & Courses (3º grau) ------------------

    private const string MenuEditSubject_s = @"
        Editar dados da disciplina:
            [0] Back       -> Voltar
            [1] Help       -> Mostrar estado atual vs original
            [2] Name       -> Alterar o nome da disciplina
            [3] ECTS       -> Alterar os ECTS
            [4] Professor  -> Alterar o professor responsável
            [5] Grade      -> Alterar a nota
        ";

    private const string MenuEditCourse_s = @"
        Editar dados do Curso:
            [0] Back       -> Voltar
            [1] Help       -> Mostrar estado atual vs original
            [2] Name       -> Alterar o nome
            [3] Type       -> Alterar o tipo de curso
            [4] Duration   -> Alterar a tempo total do curso
        ";

    internal static string GetMenuEditSubject() => MenuEditSubject_s;
    internal static string GetMenuEditCourse() => MenuEditCourse_s;

    internal static EditParamSubject_e MenuEditSubject() => AskMenu<EditParamSubject_e>("edit subject", MenuEditSubject_s);
    internal static EditParamCourse_e MenuEditCourse() => AskMenu<EditParamCourse_e>("edit course", MenuEditCourse_s);

    // Students Manage Subjects (4º grau)
    internal enum EditParamStudentSubjects_e
    {
        Back = 0,
        Help = 1,
        ListSubjects = 2,
        AddSubject = 3,
        RemoveSubject = 4,
        EditSubjectGrade = 5
    }


    private const string MenuStudentSubjects_s = @"
    Gerir Disciplinas do Estudante:
        [0] Back           -> Voltar ao menu de edição do estudante
        [1] Help           -> Mostrar este texto
        [2] ListSubjects   -> Mostrar disciplinas e notas do estudante
        [3] AddSubject     -> Adicionar uma nova disciplina (com nota opcional)
        [4] RemoveSubject  -> Remover uma disciplina
        [5] EditSubjectGrade -> Alterar a nota de uma disciplina
";

    internal static string GetMenuStudentSubjects() => MenuStudentSubjects_s;
    internal static EditParamStudentSubjects_e MenuStudentSubjects() => AskMenu<EditParamStudentSubjects_e>("manage student subjects", MenuStudentSubjects_s);

}



