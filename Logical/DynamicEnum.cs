using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;

public class EnumGenerator
{
    public static Type GenerateEnumFromTable(string connectionString, string tableName)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            var query = $"SELECT index, name FROM {tableName}";
            using (var command = new SQLiteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    var enumType = CreateEnumType();

                    while (reader.Read())
                    {
                        int index = reader.GetInt32(0);
                        string name = reader.GetString(1);

                        // Check if the index is not already assigned to an enum value
                        if (!Enum.IsDefined(enumType, index.ToString()))
                        {
                            // Add the new enum value
                            AddEnumValue(enumType, index, name);
                        }
                    }
                }
            }
        }

        return enumType;
    }

    private static Type CreateEnumType()
    {
        var enumType = typeof(MyDynamicEnum);
        var enumBuilder = EnumBuilder.Create(enumType);
        return enumBuilder.Create();
    }

    private static void AddEnumValue(Type enumType, int index, string name)
    {
        var fieldInfo = enumType.GetField(index.ToString());
        if (fieldInfo != null)
        {
            fieldInfo.SetValue(null, name);
        }
        else
        {
            var enumBuilder = EnumBuilder.Create(enumType);
            enumBuilder.DefineEnumValue(index.ToString(), name);
            enumBuilder.Create();
        }
    }
}

public class EnumBuilder
{
    private static readonly Lazy<Dictionary<Type, EnumBuilder>> EnumBuilders = new Lazy<Dictionary<Type, EnumBuilder>>(() =>
    {
        var builders = new Dictionary<Type, EnumBuilder>();
        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
        {
            if (type.IsEnum)
            {
                builders[type] = new EnumBuilder(type);
            }
        }
        return builders;
    });

    private readonly Type _enumType;

    private EnumBuilder(Type enumType)
    {
        _enumType = enumType;
    }

    public static EnumBuilder Create(Type enumType)
    {
        return EnumBuilders.Value[_enumType];
    }

    public void DefineEnumValue(string valueName, string displayName)
    {
        var fieldInfo = _enumType.GetField(valueName);
        if (fieldInfo != null)
        {
            fieldInfo.SetValue(null, displayName);
        }
        else
        {
            var fieldBuilder = _enumType.GetTypeInfo().DeclaredTypes.First(t => t.Name == "EnumBuilder").GetField("fields");
            fieldBuilder.SetValue(fieldBuilder.GetValue(null), fieldBuilder.GetValue(null).GetValue(0).Add(new KeyValuePair<string, string>(valueName, displayName)));
        }
    }
}

public enum MyDynamicEnum
{
    None = 0,
    Value1 = 1,
    Value2 = 2,
    Value3 = 3
}