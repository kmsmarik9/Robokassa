using System.Collections.ObjectModel;
using System.Globalization;

namespace KmsDev.Robokassa.Models
{
    public sealed class VerifiedNotification
    {
        public string OutSum { get; }

        public decimal Amount { get; }

        public string InvId { get; }

        public long InvoiceId { get; }

        public IReadOnlyDictionary<string, string> ShpFields { get; }

        public bool IsResultNotification { get; }

        internal VerifiedNotification(string sum, string id, Dictionary<string, string> shp, bool isResult)
        {
            OutSum = sum;
            Amount = RobokassaWire.Amount(sum);
            InvId = id;
            InvoiceId = long.Parse(id, CultureInfo.InvariantCulture);
            ShpFields = new ReadOnlyDictionary<string, string>(shp);
            IsResultNotification = isResult;
        }
    }
}
