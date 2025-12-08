namespace School_System.Domain.Scholarship
{
    using School_System.Domain.SchoolMembers;

    internal class Bolsa
    {
        // ---------- Propriedades privadas ----------
        internal string Name;
        internal decimal Amount;
        internal List<string> Requirements;

        // ---------- Construtor ----------
        public Bolsa(string name, decimal amount, List<string> requirements)
        {
            Name = name;
            Amount = amount;
            Requirements = requirements ?? [];
        }

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
