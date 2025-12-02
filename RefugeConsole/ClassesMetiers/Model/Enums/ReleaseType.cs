using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    
    internal enum ReleaseType
    {
        [Description("Décès")]
        Death,
        [Description("Retour propriétaire")]
        ReturnToOwner,
        [Description("Famille d'accueil")]
        FosterFamily,
        [Description("Adoption")]
        Adoption
    }
}
