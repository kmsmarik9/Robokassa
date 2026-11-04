namespace KmsDev.Robokassa.Requests
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class RobokassaHostAttribute : Attribute
    {
        public RobokassaHostAttribute(RobokassaHost host)
        {
            Host = host;
        }

        public RobokassaHost Host { get; }
    }
}
