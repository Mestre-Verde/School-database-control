namespace School_System.Domain.Scholarship
{
    using static System.Console;

    using School_System.Infrastructure.FileManager;
    using Schoo_lSystem.Application.Menu;
    using School_System.Domain.Base;
    using School_System.Domain.CourseProgram;
    using School_System.Domain.SchoolMembers;
    using School_System.Application.Utils;

    internal class Bolsa
    {
        // ---------- Propriedades privadas ----------
        private string _name;
        private decimal _amount;
        private string _requirements;

        // ---------- Construtor ----------
        public Bolsa(string name, decimal amount, string requirements)
        {
            _name = name;
            _amount = amount;
            _requirements = requirements;
        }

        // ---------- Propriedades públicas somente leitura ----------
        public string Name => _name;
        public decimal Amount => _amount;
        public string Requirements => _requirements;

        // ---------- Método para verificar elegibilidade ----------
        internal bool IsEligible(Student student)
        {
            return false;
        }

        // ---------- ToString para exibir informações ----------
        public override string ToString()
        {
            return $"Bolsa: {Name}, Valor: {Amount:C}, Requisitos: {Requirements}";
        }
    }
}
