using System.Collections.ObjectModel;

namespace Inceptum.WebApi.Help.ModelDescriptions
{
    internal class ComplexTypeModelDescription : ModelDescription
    {
        public ComplexTypeModelDescription()
        {
            Properties = new Collection<ParameterDescription>();
        }

        public Collection<ParameterDescription> Properties { get; private set; }
    }
}