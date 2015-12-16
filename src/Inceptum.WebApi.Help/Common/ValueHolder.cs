using System;

namespace Inceptum.WebApi.Help.Common
{
    public class ValueHolder
    {
        private readonly object m_Value;

        public static readonly ValueHolder Null = new ValueHolder(null);

        ValueHolder(object value)
        {
            m_Value = value;
        }

        public static ValueHolder Create(object value)
        {
            if (value == null)
            {
                return Null;
            }

            return value as ValueHolder ?? new ValueHolder(value);
        }

        public static ValueHolder Create(Func<object> factory)
        {
            if (factory == null)
            {
                return Null;
            }

            return new ValueHolder(factory);
        }

        public object Value
        {
            get
            {
                if (m_Value != null)
                {
                    var valueType = m_Value.GetType();
                    if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Func<>))
                    {
                        return ((Func<object>)m_Value)();
                    }
                }

                return m_Value;
            }
        }
    }
}