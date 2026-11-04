namespace KmsDev.Robokassa.Responses
{
    public static class RobokassaResponseNameCache<TResponse>
        where TResponse : IRobokassaResponse
    {
        public static readonly string Value = typeof(TResponse).FullName!;
    }
}
