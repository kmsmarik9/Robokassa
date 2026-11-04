namespace KmsDev.Robokassa
{
    internal static class RobokassaRequestResilience
    {
        public static TSettings Prepare<TSettings>(RobokassaClientApiContainer.SectionBase section, Action<TSettings>? configure)
            where TSettings : RobokassaRequestResilienceDefaultSettings, new()
        {
            var settings = new TSettings
            {
                Timeout = section.RobokassaClient.Credentials.Timeout
            };
            configure?.Invoke(settings);
            return settings;
        }
    }
}
