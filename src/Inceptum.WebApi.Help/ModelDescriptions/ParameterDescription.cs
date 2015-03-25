using System.Collections.ObjectModel;

namespace Inceptum.WebApi.Help.ModelDescriptions
{
    public class ParameterDescription
    {
        public ParameterDescription()
        {
            Annotations = new Collection<ParameterAnnotation>();
        }

        public string Name { get; set; }
        
        public string Documentation { get; set; }

        public ModelDescription TypeDescription { get; set; }

        public Collection<ParameterAnnotation> Annotations { get; private set; }
    }
}