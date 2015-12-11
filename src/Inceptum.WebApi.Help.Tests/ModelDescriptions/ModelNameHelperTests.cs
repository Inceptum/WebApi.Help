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
        [TestCase(typeof(Suffer[]), Result = "ArrayOfJoy")]
        [TestCase(typeof(List<Suffer>), Result = "ListOfJoy")]
        [TestCase(typeof(IEnumerable<Suffer>), Result = "IEnumerableOfJoy")]
        [TestCase(typeof(ICollection<Suffer>), Result = "ICollectionOfJoy")]
        [TestCase(typeof(Dictionary<int, Suffer>), Result = "DictionaryOfInt32AndJoy")]
        public string ShouldGetCorrectModelName(Type type)
        {
            return ModelNameHelper.GetModelName(type);
        }
    }

    [DisplayName(@"Joy")]
    public class Suffer
    {
        
    }
}