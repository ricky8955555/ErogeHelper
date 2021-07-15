using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErogeHelper.Tests
{
    static class TestContextExtensions
    {
        public static object Get(this TestContext testContext, string testAttributeName)
            => testContext.Properties.Cast<KeyValuePair<string, object>>().First(kvp => kvp.Key.Equals(testAttributeName, StringComparison.InvariantCultureIgnoreCase)).Value;

        public static T? Get<T>(this TestContext testContext, string testAttributeName, IFormatProvider? formatProvider = null)
            => Convert<T>(Get(testContext, testAttributeName), formatProvider);

        public static IEnumerable<T?> GetMany<T>(this TestContext testContext, string testAttributeName, IFormatProvider? formatProvider = null)
        {
            foreach (var kvp in GetTestContextPropertiesStartsWith(testContext, testAttributeName))
                yield return Convert<T>(kvp.Value, formatProvider);
        }

        public static IOrderedEnumerable<KeyValuePair<string, object>> GetTestContextPropertiesStartsWith(this TestContext testContext, string testAttributeName)
            => testContext.Properties.Cast<KeyValuePair<string, object>>().Where(kvp => kvp.Key.StartsWith(testAttributeName, StringComparison.InvariantCultureIgnoreCase)).OrderBy(kvp => kvp.Key);

        public static TestContextHelper<T> For<T>(this TestContext testContext)
            => new(testContext);

        public static T? Convert<T>(object? value, IFormatProvider? formatProvider = null)
        {
            if (value is null)
            {
                return default;
            }

            object? res = null;
            var type = typeof(T);

            if (type == typeof(Guid)) res = Guid.Parse(value.ToString()!);
            else if (type == typeof(string)) res = System.Convert.ToString(value, formatProvider);
            else if (type == typeof(long)) res = System.Convert.ToInt64(value, formatProvider);
            else if (type == typeof(int)) res = System.Convert.ToInt32(value, formatProvider);
            else if (type == typeof(DateTime)) res = System.Convert.ToDateTime(value, formatProvider);
            else if (type == typeof(double)) res = System.Convert.ToDouble(value, formatProvider);
            else if (type == typeof(bool)) res = System.Convert.ToBoolean(value, formatProvider);
            else if (type == typeof(byte)) res = System.Convert.ToByte(value, formatProvider);
            else if (type == typeof(sbyte)) res = System.Convert.ToSByte(value, formatProvider);
            else if (type == typeof(float)) res = System.Convert.ToSingle(value, formatProvider);
            else if (type == typeof(ushort)) res = System.Convert.ToUInt16(value, formatProvider);
            else if (type == typeof(uint)) res = System.Convert.ToUInt32(value, formatProvider);
            else if (type == typeof(ulong)) res = System.Convert.ToUInt64(value, formatProvider);
            else if (type == typeof(char)) res = System.Convert.ToChar(value, formatProvider);
            else if (type == typeof(decimal)) res = System.Convert.ToDecimal(value, formatProvider);

            return (T?)res;
        }
    }
}
