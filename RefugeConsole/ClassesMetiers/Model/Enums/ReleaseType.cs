using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    
    internal enum ReleaseType
    {
        
        [Description("retour_proprietaire")]
        ReturnToOwner = 1,
        [Description("famille_accueil")]
        FosterFamily = 2,
        [Description("adoption")]
        Adoption = 3,
        [Description("deces_animal")]
        Death = 4,
        [Description("inconnu")]
        Unknown = 0,
    }
}
