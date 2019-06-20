using System.IO;
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

    internal class TestDbContext : LiteDbContext
    {
        public TestDbContext(string name = "testDb") 
            : base(Path.Combine(Path.GetTempPath(), $"{name}.db"))
        {
            if (Orders.Count() != 0) return;

            Orders.Insert(new Order { CustomerName = "Unosquare" });
            Orders.Insert(new Order { CustomerName = "Apple" });
            Orders.Insert(new Order { CustomerName = "Microsoft" });
            Orders.Insert(new Order { CustomerName = "Unosquare" });
            Orders.Insert(new Order { CustomerName = "Unosquare" });
        }

        public LiteDbSet<Order> Orders { get; set; }
    }
}
