using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Helper
{
    internal class MenuHelper
    {
        /// <summary>
        /// Méthode générique pour la capture de la saisie par l'utilisateur du choix parmi l'énumaration fournit T
        /// 
        /// </summary>
        /// <returns>
        /// La valeur <see cref="T" /> de l'énumeration correspondant à la saisie de l'utilisateur.
        /// Si la valeur saisie par l'utilisateur ne correspond pas à une valeur de l'énumération,
        /// la valeur par défaut de l'énumération sera retourné.
        /// </returns>
        /// 
        public static T GetMenuChoices<T>() where T : struct, Enum
        {
            // Capture de la saisie de l'utilisateur
            var input = Console.ReadLine();

            ArgumentNullException.ThrowIfNull(input, "input");

            // Essaie de convertir la saisie dans l'énumération correspondante
            // Sinon retourne la valeur par défaut de l'énumération
            return Enum.TryParse(input, true, out T choice)
                ? choice
                : default;
        }

        /**
         * 
         * <summary>
         *   Affice un menu en console contenant les descriptions des éléments de l'énumération
         * </summary>
         * <typeparam name="T"></typeparam>
         */ 
        public static void DisplayMenu<T>() where T : struct, Enum
        {
            
            Console.WriteLine("Entrez un numéro parmi les propositions suivantes  : ");
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
