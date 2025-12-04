using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    internal enum AnimalType
    {
        [Description("Chat")]
        Cat = 1,
        [Description("Chien")]
        Dog = 2,
        [Description("Inconnu")]
        Unknown = 0,
    }
}
