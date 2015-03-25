using System;
using System.Reflection;

namespace Inceptum.WebApi.Help.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);
        string GetDocumentation(Type type);
    }
}