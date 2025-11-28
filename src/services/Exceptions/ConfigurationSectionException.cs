public class ConfigurationSectionException : Exception
{
    public ConfigurationSectionException(string sectionName, Exception? innerException = null)
    : base($"Configuration section '{sectionName}' is missing or invalid.", innerException)
    {
        SectionName = sectionName;
    }

    public string SectionName { get; init; }
}