using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Helper
{
    internal class MenuHelper
    {
        /// <summary>
        /// Reads user input from the console and parses it into a
        /// <see cref="T:RefugeConsole.CouchePresentation.View." /> enumeration value.
        /// </summary>
        /// <returns>
        /// The <see cref="T:CarRentalConsole.MenuChoices" /> enumeration value corresponding to
        /// the user input.
        /// If the input cannot be parsed into a valid enumeration value, returns
        /// <see cref="F:CarRentalConsole.MenuChoices.Unknown" />.
        /// </returns>
        /// <remarks>
        /// This method reads a line of text from the console input and attempts to parse
        /// it into a <see cref="T:CarRentalConsole.MenuChoices" /> enumeration value.
        /// <para />
        /// If the input matches any of the enumeration values, the corresponding
        /// enumeration value is returned.
        /// <para />
        /// If the input cannot be parsed into a valid enumeration value, the method
        /// returns <see cref="F:CarRentalConsole.MenuChoices.Unknown" />.
        /// </remarks>
        public static T GetMenuChoices<T>() where T : struct, Enum
        {
            // Read input from user
            var input = Console.ReadLine();

            ArgumentNullException.ThrowIfNull(input, "input");

            // Try to parse the choice according to the enumuration
            return Enum.TryParse(input, true, out T choice)
                ? choice
                : default;
        }

        /**
         * 
         * <summary>
         *   Display enum elements as a menu with enum element value as marker for choice
         * </summary>
         * <typeparam name="T"></typeparam>
         */ 
        public static void DisplayMenu<T>() where T : struct, Enum
        {
            
            Console.WriteLine("Please choose an action : ");
            var menuItemNumber = 1;

            foreach(T choice in Enum.GetValues(typeof(T)))
            {
                if (!MyEnumHelper.EqualsDefaultValue(choice))
                {
                    
                    var description = MyEnumHelper.GetEnumDescription(choice);
                    Console.WriteLine($"[{menuItemNumber}] : {description}");
                    menuItemNumber++;

                }
            }
        }


    }
}
