namespace KmsDev.Robokassa
{
    /// <summary>
    /// Учётные данные одного магазина. Пароли проверяются в момент вызова, которому они нужны.
    /// <see cref="IsTest"/> по умолчанию <c>true</c>.
    /// </summary>
    public sealed class RobokassaMerchantCredentials
    {
        public required string MerchantLogin { get; init; }

        public string? Password1 { get; init; }

        public string? Password2 { get; init; }

        public string? Password3 { get; init; }

        public string? TestPassword1 { get; init; }

        public string? TestPassword2 { get; init; }

        public bool IsTest { get; init; } = true;

        public RobokassaHashAlgorithm HashAlgorithm { get; init; } = RobokassaHashAlgorithm.Md5;

        public RobokassaHashAlgorithm RefundHashAlgorithm { get; init; } = RobokassaHashAlgorithm.Sha256;

        public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(15);

        public int MaxResponseBytes { get; init; } = 4 * 1024 * 1024;

        internal string PaymentPassword => Require(
            IsTest ? TestPassword1 : Password1,
            IsTest ? nameof(TestPassword1) : nameof(Password1));

        internal string NotificationPassword => Require(
            IsTest ? TestPassword2 : Password2,
            IsTest ? nameof(TestPassword2) : nameof(Password2));

        internal static string Require(string? value, string name) =>
            !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException($"не задан {name}");

        internal void Validate()
        {
            Require(MerchantLogin, nameof(MerchantLogin));
            if (!Enum.IsDefined(HashAlgorithm) || !Enum.IsDefined(RefundHashAlgorithm))
            {
                throw new ArgumentOutOfRangeException(nameof(HashAlgorithm));
            }

            if (RefundHashAlgorithm is not (RobokassaHashAlgorithm.Sha256 or RobokassaHashAlgorithm.Sha384 or RobokassaHashAlgorithm.Sha512))
            {
                throw new ArgumentException("для возврата допустимы sha256, sha384, sha512");
            }

            if (Timeout <= TimeSpan.Zero || Timeout.TotalMilliseconds > uint.MaxValue - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Timeout));
            }

            if (MaxResponseBytes < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxResponseBytes));
            }
        }
    }
}
