namespace KmsDev.Robokassa.Requests
{
    public sealed class RobokassaRequestHttpSettings
    {
        public required HttpMethod Method { get; init; }

        public required string Url { get; init; }
    }
}
