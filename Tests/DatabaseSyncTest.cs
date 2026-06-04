using Model;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Tests;

[TestClass]
public class DatabaseSyncTest()
{
    public static IEnumerable<object[]> GetClassesToTest()
    {
        var interfaceType = typeof(IDatabaseSyncable<,>);
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
        if (t == typeof(byte))
        {
            return (byte)Random.Shared.Next(256);
        }
        if (t == typeof(DateTimeOffset))
        {
            var utcNow = DateTimeOffset.UtcNow;
            return utcNow.AddMinutes(Random.Shared.Next(0, 1440));
        }
        if (t == typeof(string))
        {
            byte[] bytes = new byte[16];
            Random.Shared.NextBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        if (t.IsValueType)
            return CreateFromConstructor(t);

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

        return CreateFromConstructor(t);
    }

    public static object CreateFromConstructor(Type classToConstruct)
    {
        var ctor = classToConstruct.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
        var args = new List<object>();
        foreach (var param in ctor.GetParameters())
        {
            args.Add(CreateArgument(param.ParameterType));
        }
        return ctor.Invoke(args.ToArray());
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
        var obj1 = CreateFromConstructor(syncableClass);
        var obj2 = CreateFromConstructor(syncableClass);
        var obj3 = CreateFromConstructor(syncableClass);
        var syncMethod = obj1.GetType().GetMethod("SyncToDatabase", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var syncTask = (Task)syncMethod.Invoke(obj1, new object[] {});
        var syncTask2 = (Task)syncMethod.Invoke(obj2, new object[] { });
        var syncTask3 = (Task)syncMethod.Invoke(obj3, new object[] { });
        await syncTask;
        await syncTask2;
        await syncTask3;
        var keyMethod = obj1.GetType().GetMethod("GetKey", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var key = keyMethod.Invoke(obj1, new object[] { });
        var fetchMethod = obj1.GetType().GetMethod("RetrieveFromDatabase", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        dynamic task = fetchMethod.Invoke(null, new object[] { key });
        var fetched = await task;
        Assert.AreEqual(fetched, obj1);
    }
}