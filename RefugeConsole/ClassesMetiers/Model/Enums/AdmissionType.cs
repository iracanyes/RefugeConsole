using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    internal enum AdmissionType
    {
        [Description("Abandon")]
        Abandon,
        [Description("Décès propriétaire")]
        DeathOwner,
        [Description("Errant")]
        Stray,
        [Description("Saisie")]
        Seizure,
        [Description("Retour adoption")]
        ReturnAdoption
    }
}
