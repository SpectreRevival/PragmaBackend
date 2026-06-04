using Model;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Tests;

[TestClass]
public class DatabaseSyncTest()
{
    public static IEnumerable<object[]> GetClassesToTest()
    {
        var interfaceType = typeof(IDatabaseSyncable<>);
        List<Type> implementingTypes = interfaceType.Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType))
            .ToList();
        foreach (Type implementingType in implementingTypes)
        {
            yield return new object[]
            {
                implementingType.AssemblyQualifiedName!
            };
        }
    }

    public static object? CreateArgument(Type t)
    {
        if (t == typeof(Guid))
            return Guid.NewGuid();

        if (t == typeof(int))
            return Random.Shared.Next();
        if (t == typeof(long))
            return (long)Random.Shared.Next() << 32 | (long)Random.Shared.Next();
        if (t == typeof(double))
            return Random.Shared.NextDouble();
        if (t == typeof(float))
            return (float)Random.Shared.NextDouble();
        if (t == typeof(bool))
            return Random.Shared.Next(2) == 0;

        if (t.IsValueType)
            return Activator.CreateInstance(t);

        if (t == typeof(string))
        {
            byte[] bytes = new byte[16];
            Random.Shared.NextBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        if (t.IsArray)
        {
            var elementType = t.GetElementType();
            var arr = Array.CreateInstance(elementType, 3);
            for (int i = 0; i < arr.Length; i++)
                arr.SetValue(CreateArgument(elementType), i);
            return arr;
        }

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
        {
            var list = (IList)Activator.CreateInstance(t);
            var elementType = t.GetGenericArguments()[0];

            int count = Random.Shared.Next(1, 4);
            for (int i = 0; i < count; i++)
                list.Add(CreateArgument(elementType));

            return list;
        }

        if (t.IsSealed)
        {
            var ctor = t.GetConstructor(Type.EmptyTypes);
            if (ctor != null)
                return Activator.CreateInstance(t);

            return null;
        }

        var mockType = typeof(Moq.Mock<>).MakeGenericType(t);
        var mock = Activator.CreateInstance(mockType);
        return mockType.GetProperty("Object")!.GetValue(mock);
    }

    public static string GetCustomTestName(MethodInfo methodInfo, object[] data)
    {
        if (data != null && data.Length > 0 && data[0] is string typeName)
        {
            var shortName = typeName.Split(',')[0].Split('.').Last();
            return $"{methodInfo.Name}_{shortName}";
        }
        return methodInfo.Name;
    }

    [TestMethod]
    [DynamicData(nameof(GetClassesToTest), DynamicDataDisplayName = nameof(GetCustomTestName))]
    public async Task TestDatabaseSyncClass(string syncableClassName)
    {
        Type syncableClass = Type.GetType(syncableClassName);
        var ctor = syncableClass.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
        var args = new List<object>();
        foreach (var param in ctor.GetParameters())
        {
            args.Add(CreateArgument(param.ParameterType));
        }
        var obj = ctor.Invoke(args.ToArray());
        var syncMethod = obj.GetType().GetMethod("SyncToDatabase", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var syncTask = (Task)syncMethod.Invoke(obj, new object[] {});
        await syncTask;
        var keyMethod = obj.GetType().GetMethod("GetKey", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var key = keyMethod.Invoke(obj, new object[] { });
        var fetchMethod = obj.GetType().GetMethod("RetrieveFromDatabase", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        var fetched = await (Task<object?>)fetchMethod.Invoke(null, new object[] { key });
        Assert.AreEqual(fetched, obj);
    }
}