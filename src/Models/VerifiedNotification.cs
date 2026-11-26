using System.Collections.ObjectModel;
using System.Globalization;

namespace KmsDev.Robokassa.Models
{
    public sealed class VerifiedNotification
    {
        public string OutSum { get; }

        public decimal Amount { get; }

        public string InvIdRaw { get; }

        public long InvId { get; }

        public IReadOnlyDictionary<string, string> ShpFields { get; }

        public IReadOnlyDictionary<string, string> UserFields { get; }

        public bool IsResultNotification { get; }

        internal VerifiedNotification(
            string sum,
            string invIdRaw,
            Dictionary<string, string> shpFields,
            Dictionary<string, string> userFields,
            bool isResult)
        {
            OutSum = sum;
            Amount = RobokassaWire.Amount(sum);
            InvIdRaw = invIdRaw;
            InvId = long.Parse(invIdRaw, CultureInfo.InvariantCulture);
            ShpFields = new ReadOnlyDictionary<string, string>(shpFields);
            UserFields = new ReadOnlyDictionary<string, string>(userFields);
            IsResultNotification = isResult;
        }
    }
}
