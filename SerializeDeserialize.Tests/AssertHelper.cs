using System.Reflection;

namespace SerializeDeserialize.Tests
{
    public static class AssertHelper
    {
        public static bool AreObjectsEqual<T>(this T expected, T actual)
        {
            if (ReferenceEquals(expected, actual))
                return true;

            if (expected == null || actual == null)
                return false;

            Type type = typeof(T);

            // Compare properties
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                object expectedValue = property.GetValue(expected);
                object actualValue = property.GetValue(actual);

                if (!Equals(expectedValue, actualValue))
                {
                    // If the property is a reference type, recursively check its values
                    if (!property.PropertyType.IsValueType && property.PropertyType != typeof(string))
                    {
                        if (!AreObjectsEqual(expectedValue, actualValue))
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            // Compare fields
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                object expectedValue = field.GetValue(expected);
                object actualValue = field.GetValue(actual);

                if (!Equals(expectedValue, actualValue))
                {
                    // If the field is a reference type, recursively check its values
                    if (!field.FieldType.IsValueType && field.FieldType != typeof(string))
                    {
                        if (!AreObjectsEqual(expectedValue, actualValue))
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool AreListsEqual<T>(this List<T> expected, List<T> actual)
        {
            if (ReferenceEquals(expected, actual))
                return true;

            if (expected == null || actual == null || expected.Count != actual.Count)
                return false;

            for (int i = 0; i < expected.Count; i++)
            {
                if (!AreObjectsEqual(expected[i], actual[i]))
                    return false;
            }

            return true;
        }

        public static bool AreEnumerablesEqual<T>(this IEnumerable<T> expected, IEnumerable<T> actual)
        {
            List<T> expectedList = expected.ToList();
            List<T> actualList = actual.ToList();
            return AreListsEqual(expectedList, actualList);
        }

        public static bool AreCollectionsEqual<T>(this ICollection<T> expected, ICollection<T> actual)
        {
            if (ReferenceEquals(expected, actual))
                return true;

            if (expected == null || actual == null || expected.Count != actual.Count)
                return false;

            using (var expectedEnumerator = expected.GetEnumerator())
            using (var actualEnumerator = actual.GetEnumerator())
            {
                while (expectedEnumerator.MoveNext() && actualEnumerator.MoveNext())
                {
                    if (!AreObjectsEqual(expectedEnumerator.Current, actualEnumerator.Current))
                        return false;
                }
            }

            return true;
        }
    }
}
