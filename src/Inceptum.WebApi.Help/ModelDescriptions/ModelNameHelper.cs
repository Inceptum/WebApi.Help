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

            string modelName;

            if (type.IsArray)
            {
                var arrayElementType = type.GetElementType();
                modelName = getName(typeof(Array), arrayElementType);
            }
            else if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                Type[] genericArguments = type.GetGenericArguments();

                modelName = getName(genericType, genericArguments);
            }
            else
            {
                modelName = type.Name;
            }

            return modelName;
        }

        static string getName(Type type, params Type[] typeArgs)
        {
            string typeName = type.Name;

            // Trim the generic parameter counts from the name
            var genericParamsPrefixIndex = typeName.IndexOf('`');
            if (genericParamsPrefixIndex > -1)
            {
                typeName = typeName.Substring(0, genericParamsPrefixIndex);
            }

            string[] argumentTypeNames = typeArgs.Select(GetModelName).ToArray();

            return string.Format(CultureInfo.InvariantCulture, "{0}Of{1}", typeName, string.Join("And", argumentTypeNames));
        }
    }
}