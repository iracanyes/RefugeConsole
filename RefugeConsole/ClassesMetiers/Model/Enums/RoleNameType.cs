using System.ComponentModel;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    internal enum RoleNameType
    {
        [Description("Autres contact")]
        OtherContact = 1,
        [Description("Volontaire")]
        Volunteer = 2,
        [Description("Candidat")]
        Candidate = 3,
        [Description("Famille d'accueil")]
        FosterFamily = 4,
        [Description("Adoptant")]
        Adopter = 5,
        Unknown = 0,
    }
}
