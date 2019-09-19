using Unosquare.Labs.LiteLib;

namespace EmbedIO.Extra.Tests.TestObjects
{
    internal class Order : LiteModel
    {
        [LiteIndex]
        public string CustomerName { get; set; }
        
        public string ShipperCity { get; set; }

        public bool IsShipped { get; set; }
        public int Amount { get; set; }
        public string ShippedDate { get; set; }
    }
}
