namespace KmsDev.Robokassa
{
    public sealed class RobokassaClientApiContainer
    {
        internal RobokassaClientApiContainer(IRobokassaClient robokassaClient)
        {
            Invoices = new(robokassaClient);
            Notifications = new(robokassaClient);
        }

        public InvoicesSection Invoices { get; }

        public NotificationsSection Notifications { get; }

        public abstract class SectionBase
        {
            internal IRobokassaClient RobokassaClient { get; }

            internal SectionBase(IRobokassaClient robokassaClient)
            {
                RobokassaClient = robokassaClient;
            }
        }

        public sealed class InvoicesSection : SectionBase
        {
            internal InvoicesSection(IRobokassaClient robokassaClient) : base(robokassaClient)
            {
            }
        }

        public sealed class NotificationsSection : SectionBase
        {
            internal NotificationsSection(IRobokassaClient robokassaClient) : base(robokassaClient)
            {
            }
        }
    }
}
