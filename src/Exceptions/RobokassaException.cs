using System.Net;

namespace KmsDev.Robokassa.Exceptions
{
    public sealed class RobokassaException : Exception
    {
        public RobokassaErrorKind Kind { get; }

        public string Operation { get; }

        public HttpStatusCode? StatusCode { get; }

        public int? ApiCode { get; }

        internal RobokassaException(
            RobokassaErrorKind kind,
            string operation,
            HttpStatusCode? statusCode = null,
            int? apiCode = null,
            string? detail = null,
            Exception? innerException = null)
            : base(FormatMessage(kind, operation, detail), innerException)
        {
            Kind = kind;
            Operation = operation;
            StatusCode = statusCode;
            ApiCode = apiCode;
        }

        private static string FormatMessage(RobokassaErrorKind kind, string operation, string? detail)
        {
            var message = $"ошибка robokassa: {kind}, операция {operation}";
            return string.IsNullOrWhiteSpace(detail) ? message : $"{message}: {detail.Trim()}";
        }
    }
}
