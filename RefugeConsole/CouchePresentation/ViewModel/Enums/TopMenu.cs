using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RefugeConsole.CouchePresentation.ViewModel.Enums
{
    internal enum TopMenu
    {
        [Description("Animals")]
        Animal = 1,
        [Description("Contacts")]
        Contact = 2,
        [Description("Refuges")]
        Refuge = 3,
        [Description("Au revoir!")]
        Exit = 4,
        Unkown = 0,
    }
}
