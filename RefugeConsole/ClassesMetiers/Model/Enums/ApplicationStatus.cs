using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    internal enum ApplicationStatus
    {
        [Description("Rejet comportement")]
        RejectBehavior = 0,
        [Description("Rejet demande")]
        RejectApplication = 1,
        [Description("En cours")]
        OnGoing = 2,
        [Description("Demande acceptée")]
        Accepted = 3,
    }
}
