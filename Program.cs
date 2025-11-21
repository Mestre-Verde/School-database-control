using static System.Console; // <-- isto importa Write e WriteLine
//using Newtonsoft.Json;
using System.Globalization;

internal enum Nationality_e
{
    Other,      // 0
    PT,         // Portugal
    ES,         // Espanha
    FR,         // França
    US,         // Estados Unidos
    GB,         // Reino Unido
    DE,         // Alemanha
    IT,         // Itália
    BR,         // Brasil
    JP,         // Japão
    CN,         // China
    IN,         // Índia
    CA,         // Canadá
    AU,         // Austrália
    RU          // Rússia
}
internal enum CourseType_e
{
    NONE = 0,
    CTESP = 5, // nivel 5
    Licenciatura = 6,
    Mestrado = 7,
    Doutoramento = 8
}

internal enum MainMenuCommands_e
{
    Exit,        // Sair do programa
    Help,       // Mostrar ajuda
    Add,        // Adicionar algo
    Remove,     // Remover algo
    Select,     // Selecionar um item
    Search,       // Mostrar todos os dados
    None       // Caso não reconheça o comando
}
internal enum GlobalObjectCommands_e
{
    Back,        // Voltar ao menu principal
    Help,       // Mostrar ajuda
    Student,    // Adicionar aluno
    Teacher,    // Adicionar professor
    Course,     // Adicionar curso
    None       // Caso não reconheça o comando
}

interface IMustExist // não pode ter modificadores de acesso
{
}

class MenuRelated_cl
{
    internal const string UnknowonCommand_s = "❌ Comando desconhecido.\n";
    internal const string BackToMenu_s = "🔙 A voltar ao menu principal...\n";
    internal const string MainMenuCommands_s = @"
    Comandos disponíveis:
        [0] Exit      -> Sair do programa
        [1] Help      -> Mostrar este texto
        [2] Add       -> Adicionar aluno, professor, etc.
        [3] Remove    -> Remover item
        [4] Select    -> Selecionar item
        [5] Search        -> Mostrar todos os dados
    ";
    private const string MenuAddObject_s = @"
    Comandos para Adicionar:
        [0] Back       -> Voltar ao menu principal
        [1] Help       -> Mostrar este texto
        [2] Student    -> Adicionar um novo aluno
        [3] Teacher    -> Adicionar um novo professor
        [4] Course     -> Adicionar um novo curso
    ";
    private const string MenuRemoveObject_s = @"
    Comandos para Remover:
        [0] Back       -> Voltar ao menu principal
        [1] Help       -> Mostrar este texto
        [2] Student    -> Remove um aluno existente.
        [3] Teacher    -> Remove um professor existente
        [4] Course     -> Remove um curso existente
    ";
    private const string MenuSelectObject_s = @"
    Comandos para Selecionar:
        [0] Back       -> Voltar ao menu principal
        [1] Help       -> Mostrar este texto
        [2] Student    -> Remove um aluno existente.
        [3] Teacher    -> Remove um professor existente
        [4] Course     -> Remove um curso existente
    ";

    private static void RunMenu(string menuText, Dictionary<GlobalObjectCommands_e, Action> actions)
    {
        WriteLine(menuText);

        while (true)
        {
            Write("\n(menu)> ");
            string? input_s = ReadLine()?.Trim().ToLower();

            switch (input_s) // converte números para texto
            {
                case "0": input_s = "Back"; break;
                case "1": input_s = "Help"; break;
                case "2": input_s = "Student"; break;
                case "3": input_s = "Teacher"; break;
                case "4": input_s = "Course"; break;
            }

            if (!Enum.TryParse(input_s, true, out GlobalObjectCommands_e command)) { command = GlobalObjectCommands_e.None; }

            if (command == GlobalObjectCommands_e.Back)
            {
                WriteLine(BackToMenu_s);
                break;
            }

            if (command == GlobalObjectCommands_e.Help)
            {
                WriteLine(menuText);
                continue;
            }

            if (actions.TryGetValue(command, out Action? action))
            {
                action();
            }
            else
            {
                Write(UnknowonCommand_s);
            }
        }
    }

    internal static void MenuAddObject()
    {
        // Cria um dicionário que associa cada comando do menu a uma ação
        // Aqui usamos Action, que é um delegate que representa um método sem parâmetros e sem retorno
        var actions = new Dictionary<GlobalObjectCommands_e, Action>
        {
            { GlobalObjectCommands_e.Student, () => _ = Student.Create() },// Para "Student", chamamos Student.Create() e descartamos o objeto retornado com "_ ="
            { GlobalObjectCommands_e.Teacher, () => _ = Teacher.Create() },// Para "Teacher", chamamos Teacher.Create() e descartamos o objeto retornado
            { GlobalObjectCommands_e.Course,  () => _ = Course.Create() }// Para "Course", chamamos Course.Create() e descartamos o objeto retornado
        };
        // Chama a função genérica que executa o loop do menu, passando o texto do menu e o dicionário de ações
        RunMenu(MenuAddObject_s, actions);
    }

    internal static void MenuRemoveObject()
    {
        // Dicionário de ações para o menu de remoção
        var actions = new Dictionary<GlobalObjectCommands_e, Action>
        {
            // Neste caso Remove() já é void, então não precisamos de "_ ="
            { GlobalObjectCommands_e.Student, Student.Remove },
            { GlobalObjectCommands_e.Teacher, Teacher.Remove },
            { GlobalObjectCommands_e.Course,  Course.Remove }
        };

        // Executa o menu de remoção usando a função genérica
        RunMenu(MenuRemoveObject_s, actions);
    }

    internal static void MenuSelectObject()
    {
        // Dicionário de ações para o menu de seleção
        var actions = new Dictionary<GlobalObjectCommands_e, Action>
        {
            // Para Select()
            { GlobalObjectCommands_e.Student, Student.Select },
            { GlobalObjectCommands_e.Teacher, Teacher.Select },
            { GlobalObjectCommands_e.Course, Course.Select }
        };

        // Executa o menu de seleção usando a função genérica
        RunMenu(MenuSelectObject_s, actions);
    }
}

class Program
{
    static void Setup()
    {
        WriteLine("Programa Students Manager iniciado.");
        FileManager.StartupCheckFilesWithProgress();// Verifica se os ficheiros essenciais existem
        WriteLine(MenuRelated_cl.MainMenuCommands_s);// Mostra o menu de comandos
    }

    static void Loop()
    {
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
                case MainMenuCommands_e.Help: Write(MenuRelated_cl.MainMenuCommands_s); break;
                case MainMenuCommands_e.Add: MenuRelated_cl.MenuAddObject(); break;
                case MainMenuCommands_e.Remove: MenuRelated_cl.MenuRemoveObject(); break;
                case MainMenuCommands_e.Select: MenuRelated_cl.MenuSelectObject(); break;
                case MainMenuCommands_e.Search:
                    WriteLine("📋  [ls] Mostrando todos os dados..."); break;
                default: Write(MenuRelated_cl.UnknowonCommand_s); break;
            }
        }
    }

    static void Main()
    {
        Thread.Sleep(2000); // delay 2s
        Setup();
        Loop();
    }
}

/*
string      -> var_s     (ex: var_sName)
char        -> var_c     (ex: var_cGender)
int         -> var_i     (ex: var_iAge)
long        -> var_lg    (ex: var_lgPopulation)
short       -> var_sh    (ex: var_shYear)
byte        -> var_by    (ex: var_byLevel)
float       -> var_f     (ex: var_fHeight)
double      -> var_d     (ex: var_dWeight)
decimal     -> var_dc    (ex: var_dcPrice)
bool        -> var_b     (ex: var_bIsActive)
enum        -> var_e     (ex: var_eGender)
DateTime    -> var_dt    (ex: var_dtBirthDate)
TimeSpan    -> var_ts    (ex: var_tsDuration)

array (T[])              -> var_a     (ex: var_aScores)
List<T>                  -> var_l     (ex: var_lStudents)
Dictionary<TKey, TValue>  -> var_dic   (ex: var_dicGrades)
HashSet<T>               -> var_hs    (ex: var_hsEmails)
Queue<T>                 -> var_q     (ex: var_qTasks)
Stack<T>                 -> var_st    (ex: var_stHistory)
Tuple<T1, T2>            -> var_t     (ex: var_tPair)
KeyValuePair<TKey, TValue>-> var_kv   (ex: var_kvEntry)
Record / struct          -> var_sct   (ex: var_sctPerson)

class        -> var_cl     (ex: var_cStudent)
interface    -> var_i     (ex: var_iRepository)
object       -> var_o     (ex: var_oItem)


*/
/*

internal static void MenuAddObject()
{
    Write(MenuAddObject_s);
    while (true)
    {
        Write("\n(add menu)> ");
        string? input_s = ReadLine()?.Trim().ToLower();// remove espaçamentos e converte a string para lowerCase,alem de poder ser nulo
        switch (input_s)// converte números para texto do enum,para humman interface friendly
        {
            case "0": input_s = "Back"; break;
            case "1": input_s = "Help"; break;
            case "2": input_s = "Student"; break;
            case "3": input_s = "Teacher"; break;
            case "4": input_s = "Course"; break;

        }
        if (!Enum.TryParse(input_s, true, out GlobalObjectCommands_e command)) { command = GlobalObjectCommands_e.None; }
        switch (command)// Executa o comando
        {
            case GlobalObjectCommands_e.Back: Write(BackToMenu_s); break;
            case GlobalObjectCommands_e.Help: Write(MenuAddObject_s); break;
            case GlobalObjectCommands_e.Student: Student.Create(); break;
            case GlobalObjectCommands_e.Teacher: Teacher.Create(); break;
            case GlobalObjectCommands_e.Course: Course.Create(); break;
            default: Write(UnknowonCommand_s); break;
        }
        if (command == GlobalObjectCommands_e.Back) { break; }
    }

}
internal static void MenuRemoveObject()
{
    WriteLine(MenuRemoveObject_s);
    while (true)
    {
        Write("\n(remove menu)> ");
        string? input_s = ReadLine()?.Trim().ToLower();// remove espaçamentos e converte a string para lowerCase,alem de poder ser nulo
        switch (input_s)// converte números para texto do enum,para humman interface friendly
        {
            case "0": input_s = "Back"; break;
            case "1": input_s = "Help"; break;
            case "2": input_s = "Student"; break;
            case "3": input_s = "Teacher"; break;
            case "4": input_s = "Course"; break;

        }
        // Agora tenta converter para enum
        if (!Enum.TryParse(input_s, true, out GlobalObjectCommands_e command)) { command = GlobalObjectCommands_e.None; }


        switch (command)// Executa o comando
        {
            case GlobalObjectCommands_e.Back: Write(BackToMenu_s); break;
            case GlobalObjectCommands_e.Help: Write(MenuRemoveObject_s); break;
            case GlobalObjectCommands_e.Student: Student.Remove(); break;
            case GlobalObjectCommands_e.Teacher: Teacher.Remove(); break;
            case GlobalObjectCommands_e.Course: Course.Remove(); break;
            default: Write(UnknowonCommand_s); break;
        }
        if (command == GlobalObjectCommands_e.Back) { break; }
    }
}
internal static void MenuSelectObject()
{

}
*/
