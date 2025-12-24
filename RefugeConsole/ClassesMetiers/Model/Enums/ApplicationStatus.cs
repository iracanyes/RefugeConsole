using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    internal enum ApplicationStatus
    {
        
        [Description("rejet_comportement")]
        RejectBehavior = 1,
        [Description("rejet_environnement")]
        RejectEnvironment = 2,
        [Description("demande")]
        Application = 3,
        [Description("acceptee")]
        Accepted = 4,
    }
}
