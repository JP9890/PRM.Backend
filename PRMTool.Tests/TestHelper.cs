using System;
using System.Reflection;

namespace PRMTool.Tests
{
    public static class TestHelper
    {
        public static T CreateEntity<T>(int id = 0) where T : class
        {
            var entity = (T)Activator.CreateInstance(typeof(T), true)!;
            if (id > 0)
            {
                entity.SetPrivate("Id", id);
            }
            return entity;
        }

        public static T SetPrivate<T>(this T obj, string propertyName, object value)
        {
            var propInfo = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propInfo != null)
            {
                propInfo.SetValue(obj, value, null);
                return obj;
            }
            
            var fieldInfo = typeof(T).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }

            return obj;
        }
    }
}
