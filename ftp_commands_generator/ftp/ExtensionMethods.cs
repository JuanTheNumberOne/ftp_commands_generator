using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FtpExtensionMethods
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Adds an extension method to all primitive types as well as enum
        /// that gets the value of the attribute "Description".
        /// It only supports the enum for now.
        /// </summary>
        /// <typeparam name="T">Generic type that implements the IConvertible interface</typeparam>
        /// <param name="e">The type that gets the extension method</param>
        /// <returns></returns>
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttribute = memInfo[0]
                            .GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() as DescriptionAttribute;

                        if (descriptionAttribute != null)
                        {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return null; // could also return string.Empty
        }
    }
}

