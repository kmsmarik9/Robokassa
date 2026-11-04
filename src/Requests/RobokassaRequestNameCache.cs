namespace KmsDev.Robokassa.Requests
{
    public static class RobokassaRequestNameCache<TRequest>
        where TRequest : IRobokassaRequest
    {
        public static readonly string Value = typeof(TRequest).FullName!;
    }
}
