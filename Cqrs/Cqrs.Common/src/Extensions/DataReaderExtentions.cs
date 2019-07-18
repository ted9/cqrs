using System.Collections;
using System.Collections.Generic;


namespace System.Data
{
    public static class DataReaderExtentions
    {
        public static IDictionary ToDictionary(this IDataReader reader)
        {
            return reader.ToDictionary(false);
        }
        public static IDictionary ToDictionary(this IDataReader reader, bool closedReader)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

            if (reader.Read()) {
                for (int i = 0; i < reader.FieldCount; i++) {
                    dict.Add(reader.GetName(i), reader.GetValue(i));
                }
            }

            if (closedReader) reader.Close();

            return dict;
        }

        public static IList ToList(this IDataReader reader)
        {
            return reader.ToList(false);
        }
        public static IList ToList(this IDataReader reader, bool closedReader)
        {
            ArrayList list = new ArrayList();

            while (true) {
                var row = reader.ToDictionary();
                if (row.Count == 0)
                    break;

                list.Add(row);
            }

            if (closedReader) reader.Close();

            return list;
        }

        
        public static IList<T> ToList<T>(this IDataReader reader)
            where T : class
        {
            return reader.ToList<T>(false);
        }
        public static IList<T> ToList<T>(this IDataReader reader, IDictionary map)
            where T : class
        {
            return reader.ToList<T>(false, map);
        }
        public static IList<T> ToList<T>(this IDataReader reader, bool closedReader)
            where T : class
        {
            return reader.ToList<T>(closedReader, System.Collections.EnumerableExtentions.EmptyDict);
        }
        public static IList<T> ToList<T>(this IDataReader reader, bool closedReader, IDictionary map)
            where T : class
        {
            reader.ToList(closedReader);

            List<T> list = new List<T>();

            while (reader.Read()) {
                var result = reader.ToDictionary().ToEntity<T>(map);
                list.Add(result);
            }

            if (closedReader)
                reader.Close();

            return list;
        }
    }
}
