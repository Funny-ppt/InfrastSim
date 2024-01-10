namespace InfrastSim.Localization;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class AliasAttribute(Language language, string alias) : Attribute {
    public Language Language { get; set; } = language;
    public string Alias { get; set; } = alias ?? throw new ArgumentNullException(nameof(alias));
}
