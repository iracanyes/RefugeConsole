using Microsoft.Extensions.Logging;
using RefugeConsole.ClassesMetiers.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefugeConsole.CouchePresentation.View
{
    internal class SharedView
    {
        private static readonly ILogger MyLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(SharedView));
        public static string InputString(string label)
        {
            string? result = null;
            bool correct = false;
            
            Console.WriteLine(label);

            do {
                try
                {
                    result = Console.ReadLine()!.Trim();
                    correct = true;
                }
                catch (Exception ex)
                {
                    MyLogger.LogError("Error while reading input from user. Reason : {0}", ex.Message);
                }
            } while (!correct);

            if (result == null) throw new Exception("Error while reading input of type datetime");

            return (string) result;
        }

        public static string InputMultipleLines(string label)
        {
            StringBuilder sb = new StringBuilder();
            string[] keywords = ["quit", "exit", "Quit", "Exit", "q"];
            string? line;

            Console.WriteLine(label);

            try
            {
                while((line = Console.ReadLine()) != null)
                {
                    if (keywords.Any(k => line.Trim().Equals(k))) break;
                    sb.Append(line + "\n");
                }
            }
            catch (Exception ex)
            {
                MyLogger.LogError($"Error while reading input on multiple line. Reason : {ex.Message}");
                
            }

            return sb.ToString();
            
        }

        public static int InputInteger(string label)
        {
            int? result = null;
            bool correct = false;

            do {
                try
                {
                    Console.WriteLine(label);
                    result = Convert.ToInt32(Console.ReadLine()!.Trim());
                    correct = true;
                }
                catch (Exception ex)
                {

                    Console.Error.WriteLine(ex.Message);
                }
            } while(!correct);

            if (result == null) throw new Exception("Error while reading input of type integer");

            return (int) result;
        }

        public static bool InputBoolean(string label)
        {
            bool result = false;
            String[] vrai = { "OUI","Oui", "O", "oui", "o", "YES","Yes", "Y", "yes", "y" };
            String[] faux = { "NON", "Non", "N", "non", "n", "NO", "No" };
            bool correct = false;

            do {
                try
                {
                    string response = InputString(label);

                    if (faux.Contains(response)) correct = true;

                    if (vrai.Contains(response))
                    {
                        result = true;
                        correct = true;
                    }
                }
                catch (Exception ex)
                {

                    Console.Error.WriteLine(ex.Message);
                }
            } while (!correct);

            
            return result;
        }

        public static DateTime InputDateTime(string label)
        {
            DateTime? result = null;
            bool correct = false;

            do {
                try
                {
                    Console.WriteLine(label);
                    result = Convert.ToDateTime(Console.ReadLine()).ToUniversalTime();
                    correct = true;
                }
                catch (Exception ex)
                {

                    Console.Error.WriteLine(ex.Message);
                }
            } while (correct != true);

            if (result == null) throw new Exception("Error while reading input of type datetime");

            return (DateTime) result;
        }

        public static DateOnly InputDateOnly(string label)
        {
            DateOnly result = default;
            string? line;
            bool correct = false;

            do
            {
                try
                {
                    Console.WriteLine(label);
                    line = Console.ReadLine();

                    correct = DateOnly.TryParse(line, System.Globalization.CultureInfo.GetCultureInfo("fr-FR"), out result);



                }
                catch (Exception ex)
                {

                    Console.Error.WriteLine(ex.Message);
                }
            } while (correct != true);

            if (result == default) throw new Exception($"Line provided is not default");

            return result;
        }

        /**
         * <summary>
         *  Display all member of an enum with their corresponding value. 
         *  Capture the response value from user with must be equal to the value of one enum member and return it
         * </summary>
         */ 
        public static T EnumChoice<T>(string label) where T : struct, Enum
        {
            T result = default;
            var type = typeof(T);

            Console.WriteLine(label);
            MenuHelper.DisplayMenu<T>();
            result = MenuHelper.GetMenuChoices<T>();

            Console.WriteLine("Vous avez choisi : {0}", MyEnumHelper.GetEnumDescription(result));

            return result;

        }

        /**
         * Lock console screen until the user press a key
         */ 
        public static void WaitForKeyPress()
        {
            Console.WriteLine("Pressez une touche pour continuer...");
            Console.ReadKey(true);
        }
    }
}
