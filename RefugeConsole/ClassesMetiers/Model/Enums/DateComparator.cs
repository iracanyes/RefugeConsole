using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Model.Enums
{
    /**
     * <summary>
     *  Enumère les signes de comparaison des dates
     * </summary>
     */ 
    internal enum DateComparator
    {
        Equal = 1, 
        NotEqual = 2,        
        GreaterThan = 3, 
        GreaterThanOrEqual = 4,
        LessThan = 5,
        LessThanOrEqual = 6,
        None = 0
    }
}
