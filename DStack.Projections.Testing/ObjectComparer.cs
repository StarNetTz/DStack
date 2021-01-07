using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DStack.Projections.Testing
{
    public class ObjectComparer
    {
        public static string FindDifferences(object expected, object actual)
        {
            var compare = new ObjectComparer
            {
                MaxDifferences = 10
            };

            if (compare.Compare(expected, actual))
                return null;

            return compare.DifferencesString
                .Trim('\r', '\n')
                .Replace("object1", "expected")
                .Replace("object2", "actual");
        }

        readonly List<object> _parents = new List<object>();

        readonly Dictionary<Type, PropertyInfo[]> _propertyCache = new Dictionary<Type, PropertyInfo[]>();

        readonly Dictionary<Type, FieldInfo[]> _fieldCache = new Dictionary<Type, FieldInfo[]>();

        public List<string> ElementsToIgnore { get; set; }

        public bool ComparePrivateProperties { get; set; }

        public bool ComparePrivateFields { get; set; }

        public bool CompareStaticProperties { get; set; }

        public bool CompareStaticFields { get; set; }

        public bool CompareChildren { get; set; }

        public bool CompareReadOnly { get; set; }

        public bool CompareFields { get; set; }

        public bool CompareProperties { get; set; }

        public int MaxDifferences { get; set; }

        public List<String> Differences { get; set; }

        public string DifferencesString
        {
            get
            {
                StringBuilder sb = new StringBuilder(4096);

                sb.Append("\r\nBegin Differences:\r\n");

                foreach (string item in Differences)
                {
                    sb.AppendFormat("{0}\r\n", item);
                }

                sb.AppendFormat("End Differences (Maximum of {0} differences shown).", MaxDifferences);

                return sb.ToString();
            }
        }

        public bool AutoClearCache { get; set; }

        public bool Caching { get; set; }

        public List<Type> AttributesToIgnore { get; set; }

        public ObjectComparer()
        {
            Differences = new List<string>();
            ElementsToIgnore = new List<string>();
            AttributesToIgnore = new List<Type>();
            CompareStaticFields = true;
            CompareStaticProperties = true;
            ComparePrivateProperties = false;
            ComparePrivateFields = false;
            CompareChildren = true;
            CompareReadOnly = true;
            CompareFields = true;
            CompareProperties = true;
            Caching = true;
            AutoClearCache = true;
            MaxDifferences = 1;
        }

        public bool Compare(object object1, object object2)
        {
            string defaultBreadCrumb = string.Empty;

            Differences.Clear();
            Compare(object1, object2, defaultBreadCrumb);

            if (AutoClearCache)
                ClearCache();

            return Differences.Count == 0;
        }

        public void ClearCache()
        {
            _propertyCache.Clear();
            _fieldCache.Clear();
        }

        void Compare(object object1, object object2, string breadCrumb)
        {
            if (object1 == null && object2 == null)
                return;
            if (object1 == null)
            {
                Differences.Add(string.Format("object1{0} == null && object2{0} != null ((null),{1})", breadCrumb,
                    cStr(object2)));
                return;
            }

            if (object2 == null)
            {
                Differences.Add(string.Format("object1{0} != null && object2{0} == null ({1},(null))", breadCrumb,
                    cStr(object1)));
                return;
            }

            Type t1 = object1.GetType();
            Type t2 = object2.GetType();

            if (t1 != t2)
            {
                Differences.Add(string.Format("Different Types:  object1{0}.GetType() != object2{0}.GetType()",
                    breadCrumb));
                return;
            }

            else if (IsIList(t1))
            {
                CompareIList(object1, object2, breadCrumb);
            }
            else if (IsIDictionary(t1))
            {
                CompareIDictionary(object1, object2, breadCrumb);
            }
            else if (IsEnum(t1))
            {
                CompareEnum(object1, object2, breadCrumb);
            }
            else if (IsPointer(t1))
            {
                ComparePointer(object1, object2, breadCrumb);
            }
            else if (IsSimpleType(t1))
            {
                CompareSimpleType(object1, object2, breadCrumb);
            }
            else if (IsClass(t1))
            {
                CompareClass(object1, object2, breadCrumb);
            }
            else if (IsTimespan(t1))
            {
                CompareTimespan(object1, object2, breadCrumb);
            }
            else if (IsStruct(t1))
            {
                CompareStruct(object1, object2, breadCrumb);
            }
            else
            {
                throw new NotImplementedException("Cannot compare object of type " + t1.Name);
            }
        }

        bool IgnoredByAttribute(Type type)
        {
            foreach (Type attributeType in AttributesToIgnore)
            {
                if (type.GetCustomAttributes(attributeType, false).Length > 0)
                    return true;
            }

            return false;
        }

        bool IsTimespan(Type t)
        {
            return t == typeof(TimeSpan);
        }

        bool IsPointer(Type t)
        {
            return t == typeof(IntPtr) || t == typeof(UIntPtr);
        }

        bool IsEnum(Type t)
        {
            return t.IsEnum;
        }

        bool IsStruct(Type t)
        {
            return t.IsValueType && !IsSimpleType(t);
        }

        bool IsSimpleType(Type t)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                t = Nullable.GetUnderlyingType(t);
            }

            return t.IsPrimitive
                || t == typeof(DateTime)
                    || t == typeof(decimal)
                        || t == typeof(string)
                            || t == typeof(Guid);
        }

        bool ValidStructSubType(Type t)
        {
            return IsSimpleType(t)
                || IsEnum(t)
                    || IsArray(t)
                        || IsClass(t)
                            || IsIDictionary(t)
                                || IsTimespan(t)
                                    || IsIList(t);
        }

        bool IsArray(Type t)
        {
            return t.IsArray;
        }

        bool IsClass(Type t)
        {
            return t.IsClass;
        }

        bool IsIDictionary(Type t)
        {
            return t.GetInterface("System.Collections.IDictionary", true) != null;
        }

        bool IsIList(Type t)
        {
            return t.GetInterface("System.Collections.IList", true) != null;
        }

        bool IsChildType(Type t)
        {
            return !IsSimpleType(t)
                && (IsClass(t)
                    || IsArray(t)
                        || IsIDictionary(t)
                            || IsIList(t)
                                || IsStruct(t));
        }

        void CompareTimespan(object object1, object object2, string breadCrumb)
        {
            if (((TimeSpan)object1).Ticks != ((TimeSpan)object2).Ticks)
            {
                Differences.Add(string.Format("object1{0}.Ticks != object2{0}.Ticks", breadCrumb));
            }
        }

        void ComparePointer(object object1, object object2, string breadCrumb)
        {
            if (
                (object1.GetType() == typeof(IntPtr) && object2.GetType() == typeof(IntPtr) &&
                    ((IntPtr)object1) != ((IntPtr)object2)) ||
                        (object1.GetType() == typeof(UIntPtr) && object2.GetType() == typeof(UIntPtr) &&
                            ((UIntPtr)object1) != ((UIntPtr)object2))
                )
            {
                Differences.Add(string.Format("object1{0} != object2{0}", breadCrumb));
            }
        }

        void CompareEnum(object object1, object object2, string breadCrumb)
        {
            if (object1.ToString() != object2.ToString())
            {
                string currentBreadCrumb = AddBreadCrumb(breadCrumb, object1.GetType().Name, string.Empty, -1);
                Differences.Add(string.Format("object1{0} != object2{0} ({1},{2})", currentBreadCrumb, object1, object2));
            }
        }

        void CompareSimpleType(object object1, object object2, string breadCrumb)
        {
            if (object2 == null)
                throw new ArgumentNullException("object2");

            var valOne = object1 as IComparable;

            if (valOne == null)
                throw new ArgumentNullException("object1");

            if (valOne.CompareTo(object2) != 0)
            {
                Differences.Add(string.Format("object1{0} != object2{0} ({1},{2})", breadCrumb, object1, object2));
            }
        }

        void CompareStruct(object object1, object object2, string breadCrumb)
        {
            try
            {
                _parents.Add(object1);
                _parents.Add(object2);

                Type t1 = object1.GetType();

                IEnumerable<FieldInfo> currentFields = GetFieldInfo(t1);

                foreach (FieldInfo item in currentFields)
                {
                    if (!ValidStructSubType(item.FieldType))
                    {
                        continue;
                    }

                    string currentCrumb = AddBreadCrumb(breadCrumb, item.Name, string.Empty, -1);

                    Compare(item.GetValue(object1), item.GetValue(object2), currentCrumb);

                    if (Differences.Count >= MaxDifferences)
                        return;
                }

                PerformCompareProperties(t1, object1, object2, breadCrumb);
            }
            finally
            {
                _parents.Remove(object1);
                _parents.Remove(object2);
            }
        }

        void CompareClass(object object1, object object2, string breadCrumb)
        {
            try
            {
                _parents.Add(object1);
                _parents.Add(object2);

                Type t1 = object1.GetType();

                if (ElementsToIgnore.Contains(t1.Name) || IgnoredByAttribute(t1))
                    return;

                if (CompareProperties)
                    PerformCompareProperties(t1, object1, object2, breadCrumb);

                if (CompareFields)
                    PerformCompareFields(t1, object1, object2, breadCrumb);
            }
            finally
            {
                _parents.Remove(object1);
                _parents.Remove(object2);
            }
        }

        void PerformCompareFields(Type t1,
            object object1,
            object object2,
            string breadCrumb)
        {
            IEnumerable<FieldInfo> currentFields = GetFieldInfo(t1);

            foreach (FieldInfo item in currentFields)
            {
                if (!CompareChildren && IsChildType(item.FieldType))
                    continue;

                if (ElementsToIgnore.Contains(item.Name) || IgnoredByAttribute(item.FieldType))
                    continue;

                object objectValue1 = item.GetValue(object1);
                object objectValue2 = item.GetValue(object2);

                bool object1IsParent = objectValue1 != null &&
                    (objectValue1 == object1 || _parents.Contains(objectValue1));
                bool object2IsParent = objectValue2 != null &&
                    (objectValue2 == object2 || _parents.Contains(objectValue2));

                if (IsClass(item.FieldType)
                    && (object1IsParent || object2IsParent))
                {
                    continue;
                }

                string currentCrumb = AddBreadCrumb(breadCrumb, item.Name, string.Empty, -1);

                Compare(objectValue1, objectValue2, currentCrumb);

                if (Differences.Count >= MaxDifferences)
                    return;
            }
        }

        IEnumerable<FieldInfo> GetFieldInfo(Type type)
        {
            if (Caching && _fieldCache.ContainsKey(type))
                return _fieldCache[type];

            FieldInfo[] currentFields;

            if (ComparePrivateFields && !CompareStaticFields)
                currentFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            else if (ComparePrivateFields && CompareStaticFields)
                currentFields =
                    type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic |
                        BindingFlags.Static);
            else
                currentFields = type.GetFields();

            if (Caching)
                _fieldCache.Add(type, currentFields);

            return currentFields;
        }

        void PerformCompareProperties(Type t1,
            object object1,
            object object2,
            string breadCrumb)
        {
            IEnumerable<PropertyInfo> currentProperties = GetPropertyInfo(t1);

            foreach (PropertyInfo info in currentProperties)
            {

                if (info.CanRead == false)
                    continue;

                if (!CompareChildren && IsChildType(info.PropertyType))
                    continue;

                if (ElementsToIgnore.Contains(info.Name) || IgnoredByAttribute(info.PropertyType))
                    continue;


                if (!CompareReadOnly && info.CanWrite == false)
                    continue;

                object objectValue1;
                object objectValue2;
                if (!IsValidIndexer(info, breadCrumb))
                {
                    objectValue1 = info.GetValue(object1, null);
                    objectValue2 = info.GetValue(object2, null);
                }
                else
                {
                    CompareIndexer(info, object1, object2, breadCrumb);
                    continue;
                }

                bool object1IsParent = objectValue1 != null &&
                    (objectValue1 == object1 || _parents.Contains(objectValue1));
                bool object2IsParent = objectValue2 != null &&
                    (objectValue2 == object2 || _parents.Contains(objectValue2));

                //Skip properties where both point to the corresponding parent
                if ((IsClass(info.PropertyType) || IsStruct(info.PropertyType)) && (object1IsParent && object2IsParent))
                {
                    continue;
                }

                string currentCrumb = AddBreadCrumb(breadCrumb, info.Name, string.Empty, -1);

                Compare(objectValue1, objectValue2, currentCrumb);

                if (Differences.Count >= MaxDifferences)
                    return;
            }
        }

        IEnumerable<PropertyInfo> GetPropertyInfo(Type type)
        {
            if (Caching && _propertyCache.ContainsKey(type))
                return _propertyCache[type];

            PropertyInfo[] currentProperties;

            if (ComparePrivateProperties && !CompareStaticProperties)
                currentProperties =
                    type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            else if (ComparePrivateProperties && CompareStaticProperties)
                currentProperties =
                    type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic |
                        BindingFlags.Static);
            else if (!CompareStaticProperties)
                currentProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            else
                currentProperties = type.GetProperties();

            if (Caching)
                _propertyCache.Add(type, currentProperties);

            return currentProperties;
        }

        bool IsValidIndexer(PropertyInfo info, string breadCrumb)
        {
            ParameterInfo[] indexers = info.GetIndexParameters();

            if (indexers.Length == 0)
            {
                return false;
            }

            if (indexers.Length > 1)
            {
                throw new Exception("Cannot compare objects with more than one indexer for object " + breadCrumb);
            }

            if (indexers[0].ParameterType != typeof(Int32))
            {
                throw new Exception("Cannot compare objects with a non integer indexer for object " + breadCrumb);
            }

            if (info.ReflectedType.GetProperty("Count") == null)
            {
                throw new Exception("Indexer must have a corresponding Count property for object " + breadCrumb);
            }

            if (info.ReflectedType.GetProperty("Count").PropertyType != typeof(Int32))
            {
                throw new Exception("Indexer must have a corresponding Count property that is an integer for object " +
                    breadCrumb);
            }

            return true;
        }

        void CompareIndexer(PropertyInfo info, object object1, object object2, string breadCrumb)
        {
            string currentCrumb;
            int indexerCount1 =
                (int)info.ReflectedType.GetProperty("Count").GetGetMethod().Invoke(object1, new object[] { });
            int indexerCount2 =
                (int)info.ReflectedType.GetProperty("Count").GetGetMethod().Invoke(object2, new object[] { });

            if (indexerCount1 != indexerCount2)
            {
                currentCrumb = AddBreadCrumb(breadCrumb, info.Name, string.Empty, -1);
                Differences.Add(string.Format("object1{0}.Count != object2{0}.Count ({1},{2})", currentCrumb,
                    indexerCount1, indexerCount2));

                if (Differences.Count >= MaxDifferences)
                    return;
            }

            for (int i = 0; i < indexerCount1; i++)
            {
                currentCrumb = AddBreadCrumb(breadCrumb, info.Name, string.Empty, i);
                object objectValue1 = info.GetValue(object1, new object[] { i });
                object objectValue2 = info.GetValue(object2, new object[] { i });
                Compare(objectValue1, objectValue2, currentCrumb);

                if (Differences.Count >= MaxDifferences)
                    return;
            }
        }

        void CompareIDictionary(object object1, object object2, string breadCrumb)
        {
            IDictionary iDict1 = object1 as IDictionary;
            IDictionary iDict2 = object2 as IDictionary;

            if (iDict1 == null)
                throw new ArgumentNullException("object1");

            if (iDict2 == null)
                throw new ArgumentNullException("object2");

            try
            {
                _parents.Add(object1);
                _parents.Add(object2);

                if (iDict1.Count != iDict2.Count)
                {
                    Differences.Add(string.Format("object1{0}.Count != object2{0}.Count ({1},{2})", breadCrumb,
                        iDict1.Count, iDict2.Count));

                    if (Differences.Count >= MaxDifferences)
                        return;
                }

                IDictionaryEnumerator enumerator1 = iDict1.GetEnumerator();
                IDictionaryEnumerator enumerator2 = iDict2.GetEnumerator();

                while (enumerator1.MoveNext() && enumerator2.MoveNext())
                {
                    string currentBreadCrumb = AddBreadCrumb(breadCrumb, "Key", string.Empty, -1);

                    Compare(enumerator1.Key, enumerator2.Key, currentBreadCrumb);

                    if (Differences.Count >= MaxDifferences)
                        return;

                    currentBreadCrumb = AddBreadCrumb(breadCrumb, "Value", string.Empty, -1);

                    Compare(enumerator1.Value, enumerator2.Value, currentBreadCrumb);

                    if (Differences.Count >= MaxDifferences)
                        return;
                }
            }
            finally
            {
                _parents.Remove(object1);
                _parents.Remove(object2);
            }
        }

        string cStr(object obj)
        {
            try
            {
                if (obj == null)
                    return "(null)";

                if (obj == DBNull.Value)
                    return "System.DBNull.Value";

                return obj.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        void CompareIList(object object1, object object2, string breadCrumb)
        {
            IList ilist1 = object1 as IList;
            IList ilist2 = object2 as IList;

            if (ilist1 == null)
                throw new ArgumentNullException("object1");

            if (ilist2 == null)
                throw new ArgumentNullException("object2");

            try
            {
                _parents.Add(object1);
                _parents.Add(object2);

                if (ilist1.Count != ilist2.Count)
                {
                    Differences.Add(string.Format("object1{0}.Count != object2{0}.Count ({1},{2})", breadCrumb,
                        ilist1.Count, ilist2.Count));

                    if (Differences.Count >= MaxDifferences)
                        return;
                }

                IEnumerator enumerator1 = ilist1.GetEnumerator();
                IEnumerator enumerator2 = ilist2.GetEnumerator();
                int count = 0;

                while (enumerator1.MoveNext() && enumerator2.MoveNext())
                {
                    string currentBreadCrumb = AddBreadCrumb(breadCrumb, string.Empty, string.Empty, count);

                    Compare(enumerator1.Current, enumerator2.Current, currentBreadCrumb);

                    if (Differences.Count >= MaxDifferences)
                        return;

                    count++;
                }
            }
            finally
            {
                _parents.Remove(object1);
                _parents.Remove(object2);
            }
        }

        string AddBreadCrumb(string existing, string name, string extra, string index)
        {
            bool useIndex = !String.IsNullOrEmpty(index);
            bool useName = name.Length > 0;
            StringBuilder sb = new StringBuilder();

            sb.Append(existing);

            if (useName)
            {
                sb.AppendFormat(".");
                sb.Append(name);
            }

            sb.Append(extra);

            if (useIndex)
            {
                int result = -1;
                sb.AppendFormat(Int32.TryParse(index, out result) ? "[{0}]" : "[\"{0}\"]", index);
            }

            return sb.ToString();
        }

        string AddBreadCrumb(string existing, string name, string extra, int index)
        {
            return AddBreadCrumb(existing, name, extra, index >= 0 ? index.ToString() : null);
        }

    }
}
