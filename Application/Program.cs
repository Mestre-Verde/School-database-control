/// <summary>
///  Esta class é a principal e serve sómente para tratar do setup e loop do programa.
/// No setup é chamado a função de verificação de ficheiros exenciais.
/// No loop é chamado a função MainMenu que é um loop infinito até o utilizador dar um exit.
/// </summary>
namespace School_System.Application;

using static System.Console;

using School_System.Infrastructure.FileManager;
using Schoo_lSystem.Application.Menu;
using School_System.Domain.Base;
using School_System.Domain.CourseProgram;
using School_System.Domain.SchoolMembers;
using School_System.Application.Utils;
using School_System.Domain.Scholarship;

class Program
{
    static void Setup()
    {
        WriteLine("Programa Students Manager iniciado.");
        WriteLine("Link do GitHub deste projeto:https://github.com/Mestre-Verde/School-database-control/tree/main");
        FileManager.StartupCheckFilesWithProgress();// Verifica se os ficheiros essenciais existem
    }

    static void Loop()
    {
        Menu.MainMenu();
    }

    static void Main()
    {
        Thread.Sleep(2000); // delay 2s
        Setup();
        Loop();
    }
}

interface IMustExist { } // não pode ter modificadores de acesso



