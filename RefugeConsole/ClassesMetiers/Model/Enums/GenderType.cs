using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    internal enum GenderType
    {
        [Description("Male")]
        Male = 1,
        [Description("Femelle")]
        Female = 2,
        [Description("Inconnu")]
        Unknown = 0,
    }
}
