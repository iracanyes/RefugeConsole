using System.ComponentModel;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    internal enum ContactType
    {
        [Description("Autres contact")]
        OtherContact = 0,
        [Description("Volontaire")]
        Volunteer = 1,
        [Description("Candidat")]
        Candidate = 2,
        [Description("Famille d'accueil")]
        FosterFamily = 3,
        [Description("Adoptant")]
        Adopter = 4,
    }
}
