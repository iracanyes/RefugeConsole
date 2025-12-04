using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace RefugeConsole.ClassesMetiers.Helper
{
    internal static class MyEnumHelper
    {
        /**
         * <summary>
         *  Retrieve the descritption attribute value associated with the specified enum value
         * </summary>
         * 
         * <param name="value">
         *  The <see langword="enum"/> value for which to retrieve the description
         * </param>
         * 
         * <returns>
         *  The description associated with the <see langword="enum"/>, if available;
         *  otherwise, the string representation of the <see langword="enum"/> value.
         * </returns>
         * 
         * <remarks>
         *  This method retrieves the description attribute value, if present, 
         *  associated with the specified  <see langword="enum"/> value.
         *  <para/>
         *  If no description attribute is found, it returns the string representation
         *  of  <see langword="enum"/> value.
         * </remarks>
         * 
         */
        public static string GetEnumDescription<T>(T value) where T : Enum
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute = (DescriptionAttribute[]) field!.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false
            );

            return attribute == null ? value.ToString() : attribute[0].Description;
        }

        /**
         * <summary>
         *  Check if enum value provided is equal to the default value for that enum type (enum value : 0)
         * </summary>
         * 
         */
        public static bool EqualsDefaultValue<T>(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default);
        }


        public static T GetEnumFromDescription<T>(string description)
        {
            T? result = default;
            var type = typeof(T);

            try
            {
                foreach(var field in type.GetFields())
                {
                    if(Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                    {
                        if (attribute.Description == description) 
                            result = (T) field.GetValue(null)!;
                    }
                    else
                    {
                        if (field.Name == description) 
                            result = (T) field.GetValue(null)!;
                    }

                }

                if (EqualsDefaultValue<T>(result!))
                {
                    throw new Exception($"Enum ({nameof(T)}) doesn't contain the Enum Description provided ");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while retrieving the enum from enum description. Reason : {ex.Message}");
            }

            if (result == null) throw new Exception($"Unable to get the enum's default value for {nameof(T)}.");

            return result;
        }

    }
}
