using Model;
using Model.Persistence;
using Npgsql;
using System.Collections;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Extensions;

namespace Tests;

[TestClass]
public class DatabaseSyncTest()
{
    public static IEnumerable<object[]> GetClassesToTest()
    {
        Type interfaceType = typeof(IDatabaseSyncable<,>);
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
            return ((long)Random.Shared.Next() << 32) | (long)Random.Shared.Next();
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
            if (elementType == typeof(PartyMember))
            {
                Array members = Array.CreateInstance(typeof(PartyMember), 1);
                members.SetValue(new PartyMember(Guid.NewGuid(), (bool)CreateArgument(typeof(bool)), true, (string)CreateArgument(typeof(string)), (bool)CreateArgument(typeof(bool)), (long)CreateArgument(typeof(long))), 0);
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
    [DynamicData(nameof(GetClassesToTest), DynamicDataDisplayName = nameof(GetCustomTestName))]
    public async Task TestDatabaseSyncClass(string syncableClassName)
    {
        Type syncableClass = Type.GetType(syncableClassName);
        object obj1 = CreateFromConstructor(syncableClass);
        object obj2 = CreateFromConstructor(syncableClass);
        object obj3 = CreateFromConstructor(syncableClass);
        if (syncableClass == typeof(MatchHistoryData))
        {
            FixupMatchHistoryObject((MatchHistoryData)obj1);
            FixupMatchHistoryObject((MatchHistoryData)obj2);
            FixupMatchHistoryObject((MatchHistoryData)obj3);
        }
        MethodInfo? syncMethod = obj1.GetType().GetMethod("SyncToDatabase", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        Task? syncTask = (Task)syncMethod.Invoke(obj1, new object[] { });
        Task? syncTask2 = (Task)syncMethod.Invoke(obj2, new object[] { });
        Task? syncTask3 = (Task)syncMethod.Invoke(obj3, new object[] { });
        await syncTask;
        await syncTask2;
        await syncTask3;
        MethodInfo? keyMethod = obj1.GetType().GetMethod("GetKey", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        object? key = keyMethod.Invoke(obj1, new object[] { });
        MethodInfo? fetchMethod = obj1.GetType().GetMethod("RetrieveFromDatabase", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        dynamic task = fetchMethod.Invoke(null, new object[] { key });
        dynamic fetched = await task;
        ((object)fetched).Should().BeEquivalentTo(obj1, options => options
    .Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1.Seconds()))
    .WhenTypeIs<DateTimeOffset>()
    .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1.Seconds()))
    .WhenTypeIs<DateTime>());
        Assert.AreNotEqual(fetched, obj2);
        Assert.AreNotEqual(fetched, obj3);
    }

    void FixupMatchHistoryObject(MatchHistoryData historyData)
    {
        historyData.TeamData.ForEach(teamData => teamData.MatchId = historyData.MatchId);
        historyData.TeamData.ForEach(teamData => teamData.PlayerData.ForEach(playerData => playerData.MatchId = historyData.MatchId));
        historyData.TeamData.ForEach(teamData => teamData.PlayerData.ForEach(playerData => playerData.TeamNumber = historyData.TeamData[0].TeamNumber));
        for(int i = 0; i < historyData.TeamData.Length; i++)
        {
            historyData.TeamData[i].TeamNumber = i;
            historyData.TeamData[i].PlayerData.ForEach(playerData => playerData.TeamNumber = i);
        }
        historyData.TeamData.ForEach(teamData =>
        {
            for (int i = 0; i < teamData.PlayerData.Length; i++)
            {
                teamData.PlayerData[i].TeammateIndex = i;
            }
        });
    }

    [TestMethod]
    [Retry(3)]
    [DynamicData(nameof(GetClassesToTest), DynamicDataDisplayName = nameof(GetCustomTestName))]
    public async Task TestDatabaseBatchSyncClass(string syncableClassName)
    {
        Type syncableClass = Type.GetType(syncableClassName);
        object obj1 = CreateFromConstructor(syncableClass);
        object obj2 = CreateFromConstructor(syncableClass);
        object obj3 = CreateFromConstructor(syncableClass);
        if (syncableClass == typeof(MatchHistoryData))
        {
            FixupMatchHistoryObject((MatchHistoryData)obj1);
            FixupMatchHistoryObject((MatchHistoryData)obj2);
            FixupMatchHistoryObject((MatchHistoryData)obj3);
        }
        NpgsqlBatch batch = PostgresDatabase.CreateBatch();
        MethodInfo? createBatchCmdMethod = obj1.GetType().GetMethod("CreateBatchSyncCommand", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        ((IEnumerable<NpgsqlBatchCommand>)createBatchCmdMethod.Invoke(obj1, [])).ForEach(command => batch.BatchCommands.Add(command));
        ((IEnumerable<NpgsqlBatchCommand>)createBatchCmdMethod.Invoke(obj2, [])).ForEach(command => batch.BatchCommands.Add(command));
        ((IEnumerable<NpgsqlBatchCommand>)createBatchCmdMethod.Invoke(obj3, [])).ForEach(command => batch.BatchCommands.Add(command));
        await batch.ExecuteNonQueryAsync();
        MethodInfo? keyMethod = obj1.GetType().GetMethod("GetKey", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        object? key = keyMethod.Invoke(obj1, new object[] { });
        MethodInfo? fetchMethod = obj1.GetType().GetMethod("RetrieveFromDatabase", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        dynamic task = fetchMethod.Invoke(null, new object[] { key });
        dynamic fetched = await task;
        ((object)fetched).Should().BeEquivalentTo(obj1, options => options
    .Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1.Seconds()))
    .WhenTypeIs<DateTimeOffset>()
    .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1.Seconds()))
    .WhenTypeIs<DateTime>());
        Assert.AreNotEqual(fetched, obj2);
        Assert.AreNotEqual(fetched, obj3);
    }
}