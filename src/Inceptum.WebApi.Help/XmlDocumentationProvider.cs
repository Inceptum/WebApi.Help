using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;
using Inceptum.WebApi.Help.ModelDescriptions;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// A custom <see cref="IDocumentationProvider" /> that reads the API documentation from an XML documentation file.
    /// </summary>
    public class XmlDocumentationProvider : IExtendedDocumentationProvider, IModelDocumentationProvider
    {
        private readonly string m_XmlDocRootFolder;
        private readonly Dictionary<Assembly, XPathNavigator> m_CreatedNavigators = new Dictionary<Assembly, XPathNavigator>();

        public XmlDocumentationProvider(string xmlDocRootFolder = null)
        {
            m_XmlDocRootFolder = xmlDocRootFolder ?? Path.Combine(Environment.CurrentDirectory, "Content");
        }

        public string GetName(HttpActionDescriptor actionDescriptor)
        {
            // NOTE[tv]! Provider follows the convention:
            // 1. Get short method/controller description from summary tag.
            // 2. Get detailed documentation from remarks tag.
            var name = getTagValueWithLocalization(getMethodNode(actionDescriptor), "summary");
            return name != null ? name.TrimEnd('.') : null;
        }

        public string GetName(HttpControllerDescriptor controllerDescriptor)
        {
            var name = getTagValueWithLocalization(getTypeNode(controllerDescriptor.ControllerType), "summary");
            return name != null ? name.TrimEnd('.') : null;
        }

        public string GetDocumentation(HttpActionDescriptor actionDescriptor)
        {
            return getTagValueWithLocalization(getMethodNode(actionDescriptor), "remarks");
        }

        public string GetDocumentation(HttpControllerDescriptor controllerDescriptor)
        {
            return getTagValueWithLocalization(getTypeNode(controllerDescriptor.ControllerType), "remarks");
        }

        public string GetDocumentation(HttpParameterDescriptor parameterDescriptor)
        {
            var reflectedParameterDescriptor = parameterDescriptor as ReflectedHttpParameterDescriptor;
            if (reflectedParameterDescriptor != null)
            {
                XPathNavigator methodNode = getMethodNode(reflectedParameterDescriptor.ActionDescriptor);
                if (methodNode != null)
                {
                    string parameterName = reflectedParameterDescriptor.ParameterInfo.Name;
                    string tagName = string.Format(CultureInfo.InvariantCulture, "param[@name='{0}']", parameterName);
                    return getTagValueWithLocalization(methodNode, tagName);
                }
            }
            return null;
        }

        public string GetResponseDocumentation(HttpActionDescriptor actionDescriptor)
        {
            return getTagValueWithLocalization(getMethodNode(actionDescriptor), "returns");
        }

        public string GetDocumentation(MemberInfo member)
        {
            string memberName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", getTypeName(member.DeclaringType), member.Name);
            string expression = (member.MemberType == MemberTypes.Field)
                ? "/doc/members/member[@name='F:{0}']"
                : "/doc/members/member[@name='P:{0}']";
            string selectExpression = string.Format(CultureInfo.InvariantCulture, expression, memberName);
            XPathNavigator navigator = getDocumentationNavigator(member.DeclaringType.Assembly);
            return navigator != null ? getTagValueWithLocalization(navigator.SelectSingleNode(selectExpression), "summary") : null;
        }

        public string GetDocumentation(Type type)
        {
            return getTagValueWithLocalization(getTypeNode(type), "summary");
        }

        private XPathNavigator getDocumentationNavigator(Assembly assembly)
        {
            XPathNavigator navigator;
            if (!m_CreatedNavigators.TryGetValue(assembly, out navigator))
            {
                string documentationFileName = Path.Combine(m_XmlDocRootFolder, assembly.GetName().Name + ".xml");
                m_CreatedNavigators[assembly] = navigator = File.Exists(documentationFileName) ? new XPathDocument(documentationFileName).CreateNavigator() : null;
            }
            return navigator;
        }

        private static string getMemberName(MethodInfo method)
        {
            string name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", getTypeName(method.DeclaringType), method.Name);
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 0)
            {
                string[] parameterTypeNames = parameters.Select(x => getTypeNameConsiderGenericTypeParams(x.ParameterType)).ToArray();
                name = name + string.Format(CultureInfo.InvariantCulture, "({0})", string.Join(",", parameterTypeNames));
            }
            return name;
        }

        private XPathNavigator getMethodNode(HttpActionDescriptor actionDescriptor)
        {
            var reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            if (reflectedActionDescriptor != null)
            {
                string selectExpression = string.Format(CultureInfo.InvariantCulture, "/doc/members/member[@name='M:{0}']", getMemberName(reflectedActionDescriptor.MethodInfo));
                XPathNavigator navigator = getDocumentationNavigator(actionDescriptor.ControllerDescriptor.ControllerType.Assembly);
                return navigator != null ? navigator.SelectSingleNode(selectExpression) : null;
            }
            return null;
        }

        private static string getTagValueWithLocalization(XPathNavigator parentNode, string tagName)
        {
            if (parentNode != null)
            {
                XPathNavigator node;
                var lang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(lang))
                {
                    node = parentNode.SelectSingleNode(string.Format("{0}[@lang='{1}']", tagName, lang));
                    if (node != null)
                    {
                        return node.Value.Trim();
                    }
                }

                node = parentNode.SelectSingleNode(tagName);
                if (node != null)
                {
                    return node.Value.Trim();
                }
            }
            return null;
        }

        private static string getTypeName(Type type)
        {
            string name = type.FullName;
            if (type.IsGenericType)
            {
                name = type.GetGenericTypeDefinition().FullName;
            }
            if (type.IsNested)
            {
                name = name.Replace("+", ".");
            }
            return name;
        }

        private static string getTypeNameConsiderGenericTypeParams(Type type)
        {
            string name = type.FullName;
            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                Type[] genericArguments = type.GetGenericArguments();
                string genericTypeName = genericType.FullName;
                genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
                string[] argumentTypeNames = genericArguments.Select(getTypeNameConsiderGenericTypeParams).ToArray();
                name = string.Format(CultureInfo.InvariantCulture, "{0}{{{1}}}", genericTypeName, string.Join(",", argumentTypeNames));
            }
            if (type.IsNested)
            {
                name = name.Replace("+", ".");
            }
            return name;
        }

        private XPathNavigator getTypeNode(Type type)
        {
            string xpath = string.Format(CultureInfo.InvariantCulture, "/doc/members/member[@name='T:{0}']", getTypeNameConsiderGenericTypeParams(type));
            XPathNavigator navigator = getDocumentationNavigator(type.Assembly);
            return navigator != null ? navigator.SelectSingleNode(xpath) : null;
        }
    }
}