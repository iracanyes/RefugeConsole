using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    internal enum AdmissionType
    {
        [Description("abandon")]
        Abandon = 1,
        [Description("deces_proprietaire")]
        DeathOwner = 2,
        [Description("errant")]
        Stray = 3,
        [Description("saisie")]
        Seizure = 4,
        [Description("retour_adoption")]
        ReturnAdoption = 5,
        [Description("retour_famille_accueil")]
        ReturnFosterFamily = 6,
        [Description("inconnu")]
        Unknown = 0
    }
}
