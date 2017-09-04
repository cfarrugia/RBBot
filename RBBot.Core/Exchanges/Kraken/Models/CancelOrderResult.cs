namespace RBBot.Core.Exchanges.Kraken.Common
{
    public class CancelOrderResult
    {
        /// <summary>
        /// Number of orders cancelled.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// If set, order(s) is/are pending cancellation.
        /// </summary>
        public bool? Pending { get; set; }
    }
}