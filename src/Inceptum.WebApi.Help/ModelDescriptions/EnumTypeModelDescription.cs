using System.Collections.ObjectModel;

namespace Inceptum.WebApi.Help.ModelDescriptions
{
    internal class EnumTypeModelDescription : ModelDescription
    {
        public EnumTypeModelDescription()
        {
            Values = new Collection<EnumValueDescription>();
        }

        public Collection<EnumValueDescription> Values { get; private set; }
    }
}