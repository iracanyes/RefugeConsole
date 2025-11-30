using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    internal enum ApplicationType
    {
        [Description("Famille d'accueil")]
        FosterFamily = 1,
        [Description("Adoption")]
        Adoption = 2,
    }
}
