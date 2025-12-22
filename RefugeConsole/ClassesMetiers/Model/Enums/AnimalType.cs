using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    internal enum AnimalType
    {
        [Description("chat")]
        Cat = 1,
        [Description("chien")]
        Dog = 2,
        [Description("Inconnu")]
        Unknown = 0,
    }
}
