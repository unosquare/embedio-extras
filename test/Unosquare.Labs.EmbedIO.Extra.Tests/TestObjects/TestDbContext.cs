using Unosquare.Labs.LiteLib;

namespace EmbedIO.Extra.Tests.TestObjects
{
    internal class TestDbContext : LiteDbContext
    {
        public TestDbContext() 
            : base("testDB.db")
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