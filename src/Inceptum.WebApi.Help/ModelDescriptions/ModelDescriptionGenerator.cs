using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Inceptum.WebApi.Help.ModelDescriptions
{
    /// <summary>
    /// Generates model descriptions for given types.
    /// </summary>
    internal class ModelDescriptionGenerator
    {
        // Modify this to support more data annotation attributes.
        private readonly IDictionary<Type, Func<object, string>> m_AnnotationTextGenerator = new Dictionary
            <Type, Func<object, string>>
        {
            {typeof (RequiredAttribute), a => Strings.Required},
            {
                typeof (RangeAttribute), a =>
                {
                    var range = (RangeAttribute) a;
                    return String.Format(CultureInfo.CurrentCulture, Strings.Range, range.Minimum, range.Maximum);
                }
            },
            {
                typeof (MaxLengthAttribute), a =>
                {
                    var maxLength = (MaxLengthAttribute) a;
                    return String.Format(CultureInfo.CurrentCulture, Strings.MaxLength, maxLength.Length);
                }
            },
            {
                typeof (MinLengthAttribute), a =>
                {
                    var minLength = (MinLengthAttribute) a;
                    return String.Format(CultureInfo.CurrentCulture, Strings.MinLength, minLength.Length);
                }
            },
            {
                typeof (StringLengthAttribute), a =>
                {
                    var strLength = (StringLengthAttribute) a;
                    return String.Format(CultureInfo.CurrentCulture, Strings.StringLength, strLength.MinimumLength, strLength.MaximumLength);
                }
            },
            {
                typeof (DataTypeAttribute), a =>
                {
                    var dataType = (DataTypeAttribute) a;
                    return String.Format(CultureInfo.CurrentCulture, Strings.DataType, dataType.CustomDataType ?? dataType.DataType.ToString());
                }
            },
            {
                typeof (RegularExpressionAttribute), a =>
                {
                    var regularExpression = (RegularExpressionAttribute) a;
                    return String.Format(CultureInfo.CurrentCulture, Strings.RegularExpression, regularExpression.Pattern);
                }
            },
        };

        // Modify this to add more default documentations.
        private readonly IDictionary<Type, string> m_DefaultTypeDocumentation = new Dictionary<Type, string>
        {
            {typeof (Int16), "integer"},
            {typeof (Int32), "integer"},
            {typeof (Int64), "integer"},
            {typeof (UInt16), "unsigned integer"},
            {typeof (UInt32), "unsigned integer"},
            {typeof (UInt64), "unsigned integer"},
            {typeof (Byte), "byte"},
            {typeof (Char), "character"},
            {typeof (SByte), "signed byte"},
            {typeof (Uri), "URI"},
            {typeof (Single), "decimal number"},
            {typeof (Double), "decimal number"},
            {typeof (Decimal), "decimal number"},
            {typeof (String), "string"},
            {typeof (Guid), "globally unique identifier"},
            {typeof (TimeSpan), "time interval"},
            {typeof (DateTime), "date"},
            {typeof (DateTimeOffset), "date"},
            {typeof (Boolean), "boolean"},
        };

        private readonly Lazy<IModelDocumentationProvider> m_DocumentationProvider;

        public ModelDescriptionGenerator(HttpConfiguration config)
        {
            if (config == null) throw new ArgumentNullException("config");

            m_DocumentationProvider = new Lazy<IModelDocumentationProvider>(() => config.Services.GetDocumentationProvider() as IModelDocumentationProvider);
            GeneratedModels = new Dictionary<string, ModelDescription>(StringComparer.OrdinalIgnoreCase);
        }

        public Dictionary<string, ModelDescription> GeneratedModels { get; private set; }

        private IModelDocumentationProvider DocumentationProvider
        {
            get { return m_DocumentationProvider.Value; }
        }

        public ModelDescription GetOrCreateModelDescription(Type modelType)
        {
            if (modelType == null)
            {
                throw new ArgumentNullException("modelType");
            }

            Type underlyingType = Nullable.GetUnderlyingType(modelType);
            if (underlyingType != null)
            {
                modelType = underlyingType;
            }

            ModelDescription modelDescription;
            string modelName = ModelNameHelper.GetModelName(modelType);
            if (GeneratedModels.TryGetValue(modelName, out modelDescription))
            {
                if (modelType != modelDescription.ModelType)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "A model description could not be created. Duplicate model name '{0}' was found for types '{1}' and '{2}'. " +
                            "Use the [ModelName] attribute to change the model name for at least one of the types so that it has a unique name.",
                            modelName,
                            modelDescription.ModelType.FullName,
                            modelType.FullName));
                }

                return modelDescription;
            }

            if (m_DefaultTypeDocumentation.ContainsKey(modelType))
            {
                return generateSimpleTypeModelDescription(modelType);
            }

            if (modelType.IsEnum)
            {
                return generateEnumTypeModelDescription(modelType);
            }

            if (modelType.IsGenericType)
            {
                Type[] genericArguments = modelType.GetGenericArguments();

                if (genericArguments.Length == 1)
                {
                    Type enumerableType = typeof(IEnumerable<>).MakeGenericType(genericArguments);
                    if (enumerableType.IsAssignableFrom(modelType))
                    {
                        return generateCollectionModelDescription(modelType, genericArguments[0]);
                    }
                }
                if (genericArguments.Length == 2)
                {
                    Type dictionaryType = typeof(IDictionary<,>).MakeGenericType(genericArguments);
                    if (dictionaryType.IsAssignableFrom(modelType))
                    {
                        return generateDictionaryModelDescription(modelType, genericArguments[0], genericArguments[1]);
                    }

                    Type keyValuePairType = typeof(KeyValuePair<,>).MakeGenericType(genericArguments);
                    if (keyValuePairType.IsAssignableFrom(modelType))
                    {
                        return generateKeyValuePairModelDescription(modelType, genericArguments[0], genericArguments[1]);
                    }
                }
            }

            if (modelType.IsArray)
            {
                Type elementType = modelType.GetElementType();
                return generateCollectionModelDescription(modelType, elementType);
            }

            if (modelType == typeof(NameValueCollection))
            {
                return generateDictionaryModelDescription(modelType, typeof(string), typeof(string));
            }

            if (typeof(IDictionary).IsAssignableFrom(modelType))
            {
                return generateDictionaryModelDescription(modelType, typeof(object), typeof(object));
            }

            if (typeof(IEnumerable).IsAssignableFrom(modelType))
            {
                return generateCollectionModelDescription(modelType, typeof(object));
            }

            return generateComplexTypeModelDescription(modelType);
        }

        // Change this to provide different name for the member.
        private static string getMemberName(MemberInfo member, bool hasDataContractAttribute)
        {
            var jsonProperty = member.GetCustomAttribute<JsonPropertyAttribute>();
            if (jsonProperty != null && !String.IsNullOrEmpty(jsonProperty.PropertyName))
            {
                return jsonProperty.PropertyName;
            }

            if (hasDataContractAttribute)
            {
                var dataMember = member.GetCustomAttribute<DataMemberAttribute>();
                if (dataMember != null && !String.IsNullOrEmpty(dataMember.Name))
                {
                    return dataMember.Name;
                }
            }

            return member.Name;
        }

        private static bool shouldDisplayMember(MemberInfo member, bool hasDataContractAttribute)
        {
            var jsonIgnore = member.GetCustomAttribute<JsonIgnoreAttribute>();
            var xmlIgnore = member.GetCustomAttribute<XmlIgnoreAttribute>();
            var ignoreDataMember = member.GetCustomAttribute<IgnoreDataMemberAttribute>();
            var nonSerialized = member.GetCustomAttribute<NonSerializedAttribute>();
            var apiExplorerSetting = member.GetCustomAttribute<ApiExplorerSettingsAttribute>();

            bool hasMemberAttribute = member.DeclaringType.IsEnum
                ? member.GetCustomAttribute<EnumMemberAttribute>() != null
                : member.GetCustomAttribute<DataMemberAttribute>() != null;

            // Display member only if all the followings are true:
            // no JsonIgnoreAttribute
            // no XmlIgnoreAttribute
            // no IgnoreDataMemberAttribute
            // no NonSerializedAttribute
            // no ApiExplorerSettingsAttribute with IgnoreApi set to true
            // no DataContractAttribute without DataMemberAttribute or EnumMemberAttribute
            return jsonIgnore == null &&
                   xmlIgnore == null &&
                   ignoreDataMember == null &&
                   nonSerialized == null &&
                   (apiExplorerSetting == null || !apiExplorerSetting.IgnoreApi) &&
                   (!hasDataContractAttribute || hasMemberAttribute);
        }

        private string createDefaultDocumentation(Type type)
        {
            string documentation;
            if (m_DefaultTypeDocumentation.TryGetValue(type, out documentation))
            {
                return documentation;
            }
            if (DocumentationProvider != null)
            {
                documentation = DocumentationProvider.GetDocumentation(type);
            }

            return documentation;
        }

        private void generateAnnotations(MemberInfo property, ParameterDescription propertyModel)
        {
            var annotations = new List<ParameterAnnotation>();

            IEnumerable<Attribute> attributes = property.GetCustomAttributes();
            foreach (Attribute attribute in attributes)
            {
                Func<object, string> textGenerator;
                if (m_AnnotationTextGenerator.TryGetValue(attribute.GetType(), out textGenerator))
                {
                    annotations.Add(
                        new ParameterAnnotation
                        {
                            AnnotationAttribute = attribute,
                            Documentation = textGenerator(attribute)
                        });
                }
            }

            // Rearrange the annotations
            annotations.Sort((x, y) =>
            {
                // Special-case RequiredAttribute so that it shows up on top
                if (x.AnnotationAttribute is RequiredAttribute)
                {
                    return -1;
                }
                if (y.AnnotationAttribute is RequiredAttribute)
                {
                    return 1;
                }

                // Sort the rest based on alphabetic order of the documentation
                return String.Compare(x.Documentation, y.Documentation, StringComparison.OrdinalIgnoreCase);
            });

            foreach (ParameterAnnotation annotation in annotations)
            {
                propertyModel.Annotations.Add(annotation);
            }
        }

        private CollectionModelDescription generateCollectionModelDescription(Type modelType, Type elementType)
        {
            ModelDescription elementModelDescription = GetOrCreateModelDescription(elementType);
            if (elementModelDescription != null)
            {
                return new CollectionModelDescription
                {
                    Name = ModelNameHelper.GetModelName(modelType),                    
                    ModelType = modelType,
                    ElementDescription = elementModelDescription,
                    Documentation = string.Format(Strings.CollectionDocumentationTemplate, elementModelDescription.Name)
                };
            }

            return null;
        }

        private ModelDescription generateComplexTypeModelDescription(Type modelType)
        {
            var complexModelDescription = new ComplexTypeModelDescription
            {
                Name = ModelNameHelper.GetModelName(modelType),
                ModelType = modelType,
                Documentation = createDefaultDocumentation(modelType)
            };

            GeneratedModels.Add(complexModelDescription.Name, complexModelDescription);
            bool hasDataContractAttribute = modelType.GetCustomAttribute<DataContractAttribute>() != null;
            PropertyInfo[] properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (shouldDisplayMember(property, hasDataContractAttribute))
                {
                    var propertyModel = new ParameterDescription
                    {
                        Name = getMemberName(property, hasDataContractAttribute)
                    };

                    if (DocumentationProvider != null)
                    {
                        propertyModel.Documentation = DocumentationProvider.GetDocumentation(property);
                    }

                    generateAnnotations(property, propertyModel);
                    complexModelDescription.Properties.Add(propertyModel);
                    propertyModel.TypeDescription = GetOrCreateModelDescription(property.PropertyType);
                }
            }

            FieldInfo[] fields = modelType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (shouldDisplayMember(field, hasDataContractAttribute))
                {
                    var propertyModel = new ParameterDescription
                    {
                        Name = getMemberName(field, hasDataContractAttribute)
                    };

                    if (DocumentationProvider != null)
                    {
                        propertyModel.Documentation = DocumentationProvider.GetDocumentation(field);
                    }

                    complexModelDescription.Properties.Add(propertyModel);
                    propertyModel.TypeDescription = GetOrCreateModelDescription(field.FieldType);
                }
            }

            return complexModelDescription;
        }

        private DictionaryModelDescription generateDictionaryModelDescription(Type modelType, Type keyType, Type valueType)
        {
            ModelDescription keyModelDescription = GetOrCreateModelDescription(keyType);
            ModelDescription valueModelDescription = GetOrCreateModelDescription(valueType);

            return new DictionaryModelDescription
            {
                Name = ModelNameHelper.GetModelName(modelType),                
                ModelType = modelType,
                KeyModelDescription = keyModelDescription,
                ValueModelDescription = valueModelDescription,
                Documentation = string.Format(Strings.DictionaryDocumentationTemplate, valueModelDescription.Name, keyModelDescription.Name)
            };
        }

        private EnumTypeModelDescription generateEnumTypeModelDescription(Type modelType)
        {
            var enumDescription = new EnumTypeModelDescription
            {
                Name = ModelNameHelper.GetModelName(modelType),
                ModelType = modelType,
                Documentation = createDefaultDocumentation(modelType)
            };
            bool hasDataContractAttribute = modelType.GetCustomAttribute<DataContractAttribute>() != null;
            foreach (FieldInfo field in modelType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (shouldDisplayMember(field, hasDataContractAttribute))
                {
                    var enumValue = new EnumValueDescription
                    {
                        Name = field.Name,
                        Value = field.GetRawConstantValue().ToString()
                    };
                    if (DocumentationProvider != null)
                    {
                        enumValue.Documentation = DocumentationProvider.GetDocumentation(field);
                    }
                    enumDescription.Values.Add(enumValue);
                }
            }
            GeneratedModels.Add(enumDescription.Name, enumDescription);

            return enumDescription;
        }

        private KeyValuePairModelDescription generateKeyValuePairModelDescription(Type modelType, Type keyType, Type valueType)
        {
            ModelDescription keyModelDescription = GetOrCreateModelDescription(keyType);
            ModelDescription valueModelDescription = GetOrCreateModelDescription(valueType);

            return new KeyValuePairModelDescription
            {
                Name = ModelNameHelper.GetModelName(modelType),
                ModelType = modelType,
                KeyModelDescription = keyModelDescription,
                ValueModelDescription = valueModelDescription,
                Documentation = string.Format(Strings.KeyValuePairDocumentationTemplate, valueModelDescription.Name, keyModelDescription.Name)
            };
        }

        private ModelDescription generateSimpleTypeModelDescription(Type modelType)
        {
            var simpleModelDescription = new SimpleTypeModelDescription
            {
                Name = ModelNameHelper.GetModelName(modelType),
                ModelType = modelType,
                Documentation = createDefaultDocumentation(modelType)
            };
            GeneratedModels.Add(simpleModelDescription.Name, simpleModelDescription);

            return simpleModelDescription;
        }
    }
}