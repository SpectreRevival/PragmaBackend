using Model;
using System.Collections;
using System.Reflection;

namespace Tests;

[TestClass]
public class InterchangeableTest()
{
    public static IEnumerable<object[]> GetInterchangeableClasses()
    {
        Type interfaceType = typeof(IInterchangeable<,>);
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

    public static IEnumerable<object[]> GetInterchangeableKeyedClasses()
    {
        Type interfaceType = typeof(IInterchangeableKeyed<,,>);
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
        {
            return Guid.NewGuid();
        }

        if (t == typeof(int))
        {
            return Random.Shared.Next();
        }

        if (t == typeof(long))
        {
            return (long)Random.Shared.Next();
        }

        if (t == typeof(double))
        {
            return Random.Shared.NextDouble();
        }

        if (t == typeof(float))
        {
            return (float)Random.Shared.NextDouble();
        }

        if (t == typeof(bool))
        {
            return Random.Shared.Next(2) == 0;
        }

        if (t == typeof(byte))
        {
            return (byte)Random.Shared.Next(256);
        }
        if (t == typeof(DateTimeOffset))
        {
            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            return utcNow.AddMinutes(Random.Shared.Next(0, 1440));
        }
        if (t == typeof(string))
        {
            byte[] bytes = new byte[16];
            Random.Shared.NextBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
        if (t.IsEnum)
        {
            Array values = Enum.GetValues(t);
            return values.GetValue(Random.Shared.Next(values.Length));
        }

        if (t.IsValueType)
        {
            return CreateFromConstructor(t);
        }

        if (t.IsArray)
        {
            Type? elementType = t.GetElementType();
            if (elementType == typeof(Model.PartyMember))
            {
                Array members = Array.CreateInstance(typeof(Model.PartyMember), 1);
                members.SetValue(new Model.PartyMember(Guid.NewGuid(), (bool)CreateArgument(typeof(bool)), true, (string)CreateArgument(typeof(string)), (bool)CreateArgument(typeof(bool)), (long)CreateArgument(typeof(long))), 0);
                return members;
            }
            Array arr = Array.CreateInstance(elementType, 3);
            for (int i = 0; i < arr.Length; i++)
            {
                arr.SetValue(CreateArgument(elementType), i);
            }

            return arr;
        }
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            Type keyType = t.GenericTypeArguments[0];
            Type valueType = t.GenericTypeArguments[1];
            IDictionary? dict = (IDictionary)Activator.CreateInstance(t);
            for (int i = 0; i < Random.Shared.Next(5); i++)
            {
                object instKey = CreateArgument(keyType);
                object instValue = CreateArgument(valueType);
                dict.Add(instKey, instValue);
            }
            return dict;
        }
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
        {
            IList? list = (IList)Activator.CreateInstance(t);
            Type elementType = t.GetGenericArguments()[0];

            int count = Random.Shared.Next(1, 4);
            for (int i = 0; i < count; i++)
            {
                list.Add(CreateArgument(elementType));
            }

            return list;
        }

        if (t.IsSealed)
        {
            ConstructorInfo? ctor = t.GetConstructor(Type.EmptyTypes);
            return ctor != null ? Activator.CreateInstance(t) : null;
        }

        return CreateFromConstructor(t);
    }

    public static object CreateFromConstructor(Type classToConstruct)
    {
        ConstructorInfo ctor = classToConstruct.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
        List<object> args = [];
        foreach (ParameterInfo param in ctor.GetParameters())
        {
            args.Add(CreateArgument(param.ParameterType));
        }
        return ctor.Invoke(args.ToArray());
    }

    public static string GetCustomTestName(MethodInfo methodInfo, object[] data)
    {
        if (data != null && data.Length > 0 && data[0] is string typeName)
        {
            string shortName = typeName.Split(',')[0].Split('.').Last();
            return $"{methodInfo.Name}_{shortName}";
        }
        return methodInfo.Name;
    }

    [TestMethod]
    [Retry(3)]
    [DynamicData(nameof(GetInterchangeableClasses), DynamicDataDisplayName = nameof(GetCustomTestName))]
    public async Task TestInterchangeableClass(string modelClassName)
    {
        Type modelClass = Type.GetType(modelClassName);
        Type interfaceType = typeof(IInterchangeable<,>);
        Type packetClass = modelClass.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType).First().GetGenericArguments().Last();
        Assert.AreNotEqual(packetClass, modelClass);
        object modelobj1 = CreateFromConstructor(modelClass);
        object modelobj2 = CreateFromConstructor(modelClass);
        Assert.AreEqual(modelobj1, modelobj1);
        Assert.AreNotEqual(modelobj1, modelobj2);
        MethodInfo? toPacketMethod = modelClass.GetMethod("ToPacket", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        MethodInfo? fromPacketMethod = modelClass.GetMethod("FromPacket", BindingFlags.Public | BindingFlags.Static);
        object? packetObj1 = toPacketMethod.Invoke(modelobj1, new object[] { });
        object? recreatedObj1 = fromPacketMethod.Invoke(null, new object[] { packetObj1 });
        Assert.AreEqual(modelobj1, recreatedObj1);
        Assert.AreNotEqual(modelobj2, recreatedObj1);
    }

    [TestMethod]
    [Retry(3)]
    [DynamicData(nameof(GetInterchangeableKeyedClasses), DynamicDataDisplayName = nameof(GetCustomTestName))]
    public async Task TestInterchangeableKeyedClass(string modelClassName)
    {
        Type modelClass = Type.GetType(modelClassName);
        if (modelClass.IsSubclassOf(typeof(Item)))
        {
            await TestInterchangeableKeyedItemClass(modelClass);
            return;
        }
        Type interfaceType = typeof(IInterchangeableKeyed<,,>);
        Type packetClass = modelClass.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType).First().GetGenericArguments()[1];
        Assert.AreNotEqual(packetClass, modelClass);
        object modelobj1 = CreateFromConstructor(modelClass);
        object modelobj2 = CreateFromConstructor(modelClass);
        Assert.AreEqual(modelobj1, modelobj1);
        Assert.AreNotEqual(modelobj1, modelobj2);
        MethodInfo? toPacketMethod = modelClass.GetMethod("ToPacket", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        MethodInfo? fromPacketMethod = modelClass.GetMethod("FromPacket", BindingFlags.Public | BindingFlags.Static);
        MethodInfo? getKeyMethod = modelClass.GetMethod("GetKey", BindingFlags.Public | BindingFlags.Instance);
        object? key = getKeyMethod.Invoke(modelobj1, new object[] { });
        object? packetObj1 = toPacketMethod.Invoke(modelobj1, new object[] { });
        object? recreatedObj1 = fromPacketMethod.Invoke(null, new object[] { packetObj1, key });
        Assert.AreEqual(modelobj1, recreatedObj1);
        Assert.AreNotEqual(modelobj2, recreatedObj1);
    }

    public async Task TestInterchangeableKeyedItemClass(Type modelClass)
    {
        Type interfaceType = typeof(IInterchangeableKeyed<,,>);
        Type packetClass = modelClass.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType).First().GetGenericArguments()[1];
        Assert.AreNotEqual(packetClass, modelClass);
        object modelobj1 = CreateFromConstructor(modelClass);
        object modelobj2 = CreateFromConstructor(modelClass);
        Assert.AreEqual(modelobj1, modelobj1);
        Assert.AreNotEqual(modelobj1, modelobj2);
        MethodInfo? toPacketMethod = modelClass.GetMethod("ToPacket", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        MethodInfo? fromPacketMethod = modelClass.GetMethod("FromPacket", BindingFlags.Public | BindingFlags.Static);
        PropertyInfo? pidProp = modelClass.GetProperty("OwningPlayerId");
        object? pid = pidProp.GetValue(modelobj1);
        object? packetObj1 = toPacketMethod.Invoke(modelobj1, new object[] { });
        object? recreatedObj1 = fromPacketMethod.Invoke(null, new object[] { packetObj1, pid });
        Assert.AreEqual(modelobj1, recreatedObj1);
        Assert.AreNotEqual(modelobj2, recreatedObj1);
    }
}