/// <summary>
/// Class onde se encontra a maioria da lógica dos menus.
/// </summary>
namespace Schoo_lSystem.Application.Menu;

using static System.Console;

using School_System.Infrastructure.FileManager;
using Schoo_lSystem.Application.Menu;
using School_System.Domain.Base;
using School_System.Domain.CourseProgram;
using School_System.Domain.SchoolMembers;
// enums de Course
internal enum EditParamCourse_e
{
    Back,
    Help,
    Name,
    Type,
    Duration,
    ManageSubjects,
}
internal enum EditParamSubjects_e
{
    Back,
    Help,
    Name,
    ECTS,
    ManageStudents,
    ManageTeachers
}


public static class MenuRelated_cl
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
        Search,       // Mostrar todos os dados
        None       // Caso não reconheça o comando
    }
    private const string MainMenuCommands_s = @"
    Comandos disponíveis:
        [0] Exit      -> Sair do programa
        [1] Help      -> Mostrar este texto
        [2] Add       -> Adicionar aluno, professor, etc.
        [3] Remove    -> Remover item
        [4] Select    -> Selecionar item
        [5] Search    -> Mostrar todos os dados
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
                case "5": input_s = "Search"; break;

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
                case MainMenuCommands_e.Search: //FileManager.SuperSearch;
                    WriteLine("📋  [ls] Mostrando todos os dados..."); break;
                default: Write(UnknowonCommand_s); break;
            }
        }
    }

    // Menu 2º grau , seleção do tipo de objeto
    private enum GlobalObjectCommands_e
    {
        Back,        // Voltar ao menu principal
        Help,       // Mostrar ajuda
        Student,    // Adicionar aluno
        Teacher,    // Adicionar professor
        Course,     // Adicionar curso
        Subject,
        None       // Caso não reconheça o comando
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
        [0] Back        -> Voltar ao menu principal
        [1] Help        -> Mostrar este texto
        [2] Student     -> {typeFunction} um {novo}aluno
        [3] Teacher     -> {typeFunction} um {novo}professor
        [4] Course      -> {typeFunction} um {novo}curso
        [5] Subject    -> {typeFunction} uma {nova}disciplina
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
                case "2": input_s = "Student"; break;
                case "3": input_s = "Teacher"; break;
                case "4": input_s = "Course"; break;
                case "5": input_s = "Subject"; break;
            }

            // Tenta converter o texto para um comando válido enum
            if (!Enum.TryParse(input_s, true, out GlobalObjectCommands_e command))
            {
                command = GlobalObjectCommands_e.None;
            }
            // Voltar ao menu anterior
            if (command == GlobalObjectCommands_e.Back)
            {
                WriteLine(BackToMenu_s);
                break;
            }
            // Mostrar novamente o menu
            if (command == GlobalObjectCommands_e.Help)
            {
                WriteLine(menuString);
                continue;
            }
            // Executa a ação associada ao comando
            if (actions.TryGetValue(command, out Action? action)) { action(); }
            else { Write(UnknowonCommand_s); } // comando inválido
        }
    }
    private static void MenuAddObject()
    {
        // Cria um dicionário que associa cada comando do menu a uma ação
        // Aqui usamos Action, que é um delegate que representa um método sem parâmetros e sem retorno
        var actions = new Dictionary<GlobalObjectCommands_e, Action>
        {
            { GlobalObjectCommands_e.Student, () => _ = Student.Create() },// Para "Student", chamamos Student.Create() e descartamos o objeto retornado com "_ ="
            { GlobalObjectCommands_e.Teacher, () => _ = Teacher.Create() },// Para "Teacher", chamamos Teacher.Create() e descartamos o objeto retornado
            { GlobalObjectCommands_e.Course,  () => _ = Course.Create() },// Para "Course", chamamos Course.Create() e descartamos o objeto retornado
            { GlobalObjectCommands_e.Subject,() => _ = Subject.Create() }
        };
        // Chama a função genérica que executa o loop do menu, passando o texto do menu e o dicionário de ações
        RunMenu("Add", BuildObjectMenu("Adiciona"), actions);
    }
    private static void MenuRemoveObject()
    {
        // Dicionário de ações para o menu de remoção
        var actions = new Dictionary<GlobalObjectCommands_e, Action>
        {
            // Neste caso Remove() já é void, então não precisamos de "_ ="
            { GlobalObjectCommands_e.Student, Student.Remove },
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
            // Para Select()
            { GlobalObjectCommands_e.Student, Student.Select },
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
        Tutor = 8,
        Grades = 9
    }
    private const string MenuEditStudents_s = @"
    Editar dados dos estudantes:
        [0] Back        -> Voltar ao menu principal
        [1] Help        -> Mostrar este texto
        [2] Name        -> Alterar o nome
        [3] Age         -> Alterar a idade
        [4] Gender      -> Alterar o género
        [5] BirthDate   -> Alterar a data de nascimento
        [6] Nationality -> Alterar a nacionalidade
        [7] Email       -> Alterar o email
        [8] Tutor       -> Alterar o tutor
        [9] Grades      -> Alterar notas
";
    internal static string GetMenuEditStudents() => MenuEditStudents_s;
    internal static EditParamStudent_e MenuEditStudent()
    {
        while (true)
        {
            Write("\n(edit student)> ");
            string? input_s = ReadLine()?.Trim();

            // Atalhos numéricos
            switch (input_s)
            {
                case "0": return EditParamStudent_e.Back;
                case "1": return EditParamStudent_e.Help;
                case "2": return EditParamStudent_e.Name;
                case "3": return EditParamStudent_e.Age;
                case "4": return EditParamStudent_e.Gender;
                case "5": return EditParamStudent_e.BirthDate;
                case "6": return EditParamStudent_e.Nationality;
                case "7": return EditParamStudent_e.Email;
                case "8": return EditParamStudent_e.Tutor;
                case "9": return EditParamStudent_e.Grades;
            }

            // Se for texto OU número, validar se existe no enum
            if (Enum.TryParse(input_s, true, out EditParamStudent_e result) && Enum.IsDefined(typeof(EditParamStudent_e), result)) { return result; }
            WriteLine(UnknowonCommand_s);
        }
    }

}
