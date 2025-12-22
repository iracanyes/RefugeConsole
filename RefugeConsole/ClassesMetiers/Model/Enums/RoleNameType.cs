using System.ComponentModel;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    internal enum RoleNameType
    {
        [Description("autres")]
        OtherContact = 1,
        [Description("benevole")]
        Volunteer = 2,
        [Description("candidat")]
        Candidate = 3,
        [Description("famille_accueil")]
        FosterFamily = 4,
        [Description("adoptant")]
        Adopter = 5,
        Unknown = 0,
    }
}
