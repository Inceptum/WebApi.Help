using System;

namespace Inceptum.WebApi.Help.Description
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ApiExplorerOrderAttribute : Attribute
    {
        public int Order { get; set; }
    }
}