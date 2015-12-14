using System;
using System.Collections.Generic;
using System.ComponentModel;
using Inceptum.WebApi.Help.ModelDescriptions;
using NUnit.Framework;

namespace Inceptum.WebApi.Help.Tests.ModelDescriptions
{
    [TestFixture]
    public class ModelNameHelperTests
    {
        [Test]
        [TestCase(typeof(string), Result = "String")]
        [TestCase(typeof(int), Result = "Int32")]
        [TestCase(typeof(TypeCode), Result = "TypeCode")]
        [TestCase(typeof(ModelNameHelperTests), Result = "ModelNameHelperTests")]
        [TestCase(typeof(List<int>), Result = "ListOfInt32")]
        [TestCase(typeof(TypeAInternal[]), Result = "ArrayOfTypeA")]
        [TestCase(typeof(List<TypeAInternal>), Result = "ListOfTypeA")]
        [TestCase(typeof(IEnumerable<TypeAInternal>), Result = "IEnumerableOfTypeA")]
        [TestCase(typeof(ICollection<TypeAInternal>), Result = "ICollectionOfTypeA")]
        [TestCase(typeof(Dictionary<int, TypeAInternal>), Result = "DictionaryOfInt32AndTypeA")]
        [TestCase(typeof(TypeBbb<int, string>[]), Result = "ArrayOfTypeBbbOfInt32AndString")]
        public string ShouldGetCorrectModelName(Type type)
        {
            return ModelNameHelper.GetModelName(type);
        }
    }

    [DisplayName(@"TypeA")]
    public class TypeAInternal
    {
        
    }

    public class TypeBbb<T, U>
    {
        
    }
}