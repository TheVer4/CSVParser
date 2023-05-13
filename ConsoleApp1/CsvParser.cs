using System.Reflection;

namespace ConsoleApp1;

class CsvParser
{
    private const BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

    public static List<T> Parse<T>(string filePath) where T : class
    {
        Type type = typeof(T);
        var objectiveTable = ObjectiveTable.newInstance(filePath).AsCollectible();

        FieldInfo[] fields = type.GetFields(FLAGS);

        List<T> result = new List<T>();

        foreach (Dictionary<string, string?> dictionary in objectiveTable)
        {
            T? obj = CreateObject<T>();
            foreach (FieldInfo fieldInfo in fields)
            {
                Type fieldType = fieldInfo.FieldType;
                var parsedValue = dictionary[fieldInfo.Name];
                object? value = ParseIfPossible(fieldType, parsedValue);

                fieldInfo.SetValue(obj, value);
            }

            result.Add(obj);
        }

        return result;
    }

    private static object? ParseIfPossible(Type type, object? value)
    {
        MethodInfo? methodParse = type
            .GetMethods(FLAGS)
            .FirstOrDefault(x => x.Name == "Parse");
        return (methodParse is not null) ?
             methodParse.Invoke(null, new[] { value }) : value;
    }

    private static T? CreateObject<T>() where T : class =>
        Activator.CreateInstance(typeof(T)) as T;


    private class ObjectiveTable
    {
        public readonly String[] columns;
        public readonly List<String[]> values;

        private ObjectiveTable(string[] columns, List<string[]> values)
        {
            this.columns = columns;
            this.values = values;
        }

        public static ObjectiveTable newInstance(String filename)
        {
            IEnumerable<String> lines = File.ReadAllLines(filename);
            return ObjectiveTable.newInstance(lines.ToArray());
        }

        public static ObjectiveTable newInstance(String[] strArray)
        {
            IEnumerable<string> lines = strArray.Where(x => !x.Trim().StartsWith("#"));
            var heading = lines.FirstOrDefault()?.Split(';').Select(TryConvertToLanguageCase).ToArray();
            if (heading is null)
                throw new InvalidDataException("Не найдена строка заголовков");
            var values = lines.Skip(1).Select(x => x.Split(';')).ToList();
            return new ObjectiveTable(heading, values);
        }

        private static string TryConvertToLanguageCase(string name)
        {
            if (name.Contains('_') || name.Contains('-'))
            {
                string[] split = name.Split("_-".ToCharArray());
                return split[0] + string.Join("",
                    split
                        .Skip(1)
                        .Select
                        (
                            x => x.First().ToString().ToUpper() + x.Substring(1)
                        )
                    );
            }
            return name;
        }

        public List<Dictionary<string, string>> AsCollectible()
        {
            return values.Select(x =>
            {
                Dictionary<string, string> res = new Dictionary<string, string>();
                for (int i = 0; i < x.Length; i++)
                {
                    res.Add(columns[i], x[i]);
                }

                return res;
            }).ToList();
        }
    }
}