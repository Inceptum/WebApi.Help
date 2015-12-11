using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Inceptum.WebApi.Help.ModelDescriptions
{
    internal static class ModelNameHelper
    {
        public static string GetModelName(Type type)
        {
            var displayNameAttribute = type.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
            if (displayNameAttribute != null && !string.IsNullOrWhiteSpace(displayNameAttribute.DisplayName))
            {
                return displayNameAttribute.DisplayName;
            }

            if (type.IsArray)
            {
                var arrayElementType = type.GetElementType();
                return "ArrayOf" + GetModelName(arrayElementType);
            }

            string modelName = type.Name;
            if (type.IsGenericType)
            {
                // Format the generic type name to something like: GenericOfAgurment1AndArgument2
                Type genericType = type.GetGenericTypeDefinition();
                Type[] genericArguments = type.GetGenericArguments();
                string genericTypeName = genericType.Name;

                // Trim the generic parameter counts from the name
                genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
                string[] argumentTypeNames = genericArguments.Select(GetModelName).ToArray();
                modelName = String.Format(CultureInfo.InvariantCulture, "{0}Of{1}", genericTypeName, string.Join("And", argumentTypeNames));
            }

            return modelName;
        }
    }
}