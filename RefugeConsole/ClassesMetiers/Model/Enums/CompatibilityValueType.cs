using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    internal enum CompatibilityValueType
    {
        [Description("Oui")]
        NO = 2,
        [Description("Non")]
        YES = 3,
        [Description("Non testé")]
        NOT_TESTED = 1,
        [Description("Inconnu")]
        Unknown = 0,

    }
}
