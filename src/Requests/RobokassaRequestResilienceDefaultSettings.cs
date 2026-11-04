namespace KmsDev.Robokassa.Requests
{
    /// <summary>
    /// Настройки одного вызова. Повтор по умолчанию выключен: денежные запросы нельзя отправлять ещё раз,
    /// пока приложение само не выяснило судьбу предыдущего.
    /// </summary>
    public class RobokassaRequestResilienceDefaultSettings : IRobokassaRequestResilienceSettings
    {
        public bool EnableRetryStrategy { get; set; }

        public int RetryMaxAttempts { get; set; }

        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// По умолчанию 15 секунд. При регистрации клиента берётся timeout магазина.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(15);
    }
}
