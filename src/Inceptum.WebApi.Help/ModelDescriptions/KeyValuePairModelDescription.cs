namespace Inceptum.WebApi.Help.ModelDescriptions
{
    internal class KeyValuePairModelDescription : ModelDescription
    {
        public ModelDescription KeyModelDescription { get; set; }

        public ModelDescription ValueModelDescription { get; set; }
    }
}