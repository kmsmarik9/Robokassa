using System.Security.Cryptography.X509Certificates;

namespace KmsDev.Robokassa
{
    public static partial class RobokassaExtensions
    {
        /// <summary>
        /// Проверяет подпись ResultURL. Сумма сравнивается с исходной строкой <c>OutSum</c>.
        /// </summary>
        public static VerifiedNotification VerifyResultUrl(
            this RobokassaClientApiContainer.NotificationsSection notificationsSection,
            IEnumerable<KeyValuePair<string, string>> fields,
            decimal? expectedAmount = null)
        {
            return RobokassaNotificationVerifier.VerifyResultUrl(notificationsSection.RobokassaClient.Credentials, fields, expectedAmount);
        }

        /// <summary>
        /// Проверяет подпись SuccessURL. Этот переход не заменяет серверное подтверждение оплаты.
        /// </summary>
        public static VerifiedNotification VerifySuccessUrl(
            this RobokassaClientApiContainer.NotificationsSection notificationsSection,
            IEnumerable<KeyValuePair<string, string>> fields)
        {
            return RobokassaNotificationVerifier.VerifySuccessUrl(notificationsSection.RobokassaClient.Credentials, fields);
        }

        /// <summary>
        /// Проверяет компактный JWS ResultUrl2 сертификатом, который приложение получило заранее.
        /// </summary>
        public static ResultUrl2Notification VerifyResultUrl2(
            this RobokassaClientApiContainer.NotificationsSection notificationsSection,
            string compactJws,
            X509Certificate2 trustedCertificate,
            string expectedAlgorithm = "RS256",
            TimeSpan? maxAge = null,
            DateTimeOffset? now = null)
        {
            return RobokassaNotificationVerifier.VerifyResultUrl2(
                notificationsSection.RobokassaClient.Credentials,
                compactJws,
                trustedCertificate,
                expectedAlgorithm,
                maxAge,
                now);
        }

        /// <summary>
        /// Ответ <c>OK{InvId}</c> для ResultURL. Для SuccessURL выбрасывает <see cref="ArgumentException"/>.
        /// </summary>
        public static string ResultResponse(this RobokassaClientApiContainer.NotificationsSection notificationsSection, VerifiedNotification notification)
        {
            ArgumentNullException.ThrowIfNull(notificationsSection);
            ArgumentNullException.ThrowIfNull(notification);
            if (!notification.IsResultNotification)
            {
                throw new ArgumentException("для подтверждения нужен ResultURL");
            }

            return "OK" + notification.InvId;
        }
    }
}
