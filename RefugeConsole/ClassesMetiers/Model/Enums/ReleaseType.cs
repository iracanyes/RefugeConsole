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
        [Description("Adoption")]
        Adoption
    }
}
