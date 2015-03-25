using System;
using System.Collections.Generic;
using System.Linq;
using Inceptum.WebApi.Help.ModelDescriptions;

namespace Inceptum.WebApi.Help.Extensions
{
    internal static class UtilityExtensions
    {
        public static IList<ParameterDescription> GetParameterDescriptions(this ModelDescription modelDescription)
        {
            if (modelDescription == null) throw new ArgumentNullException("modelDescription");

            var complexTypeModelDescription = modelDescription as ComplexTypeModelDescription;
            if (complexTypeModelDescription != null)
            {
                return complexTypeModelDescription.Properties;
            }

            var collectionModelDescription = modelDescription as CollectionModelDescription;
            if (collectionModelDescription != null)
            {
                complexTypeModelDescription = collectionModelDescription.ElementDescription as ComplexTypeModelDescription;
                if (complexTypeModelDescription != null)
                {
                    return complexTypeModelDescription.Properties;
                }
            }

            return Enumerable.Empty<ParameterDescription>().ToList();
        }
    }
}