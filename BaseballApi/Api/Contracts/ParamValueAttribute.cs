namespace BaseballApi.Contracts;

[AttributeUsage(AttributeTargets.Field)]
public class ParamValueAttribute(string text) : Attribute
{
    private string Text { get; } = text;

    public override string ToString()
    {
        return this.Text;
    }
}

public static class EnumExtensions
{
    public static T ToEnumOrDefault<T, W>(this string? givenString)
            where T : Enum
            where W : Attribute
    {
        foreach (var field in typeof(T).GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(W)) is W attribute)
            {
                if (string.Equals(attribute.ToString(), givenString, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)field.GetValue(null);
                }
            }
        }

        return default;
    }

    public static T ToEnum<T, W>(this string givenString)
            where T : Enum
            where W : Attribute
    {
        foreach (var field in typeof(T).GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(W)) is W attribute)
            {
                if (string.Equals(attribute.ToString(), givenString, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)field.GetValue(null);
                }
            }
        }

        throw new ArgumentException("Not a valid enum type conversion.");
    }
}
