namespace DependencyPropertyToolkit
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DependencyPropertyAttribute(object? defaultValue = default) : Attribute
    {
        public object? DefaultValue { get; } = defaultValue;
    }
}
