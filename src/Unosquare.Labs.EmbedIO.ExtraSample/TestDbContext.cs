namespace EmbedIO.ExtraSample
{
    using System.IO;
    using Unosquare.Labs.LiteLib;

    internal class TestDbContext : LiteDbContext
    {
        public TestDbContext(string name = "testDb") 
            : base(Path.Combine(Path.GetTempPath(), $"{name}.db"))
        {
            if (Orders.Count() != 0) return;

            Orders.Insert(new Order {UniqueId = "1", CustomerName = "Unosquare"});
            Orders.Insert(new Order {UniqueId = "2", CustomerName = "Apple"});
            Orders.Insert(new Order {UniqueId = "3", CustomerName = "Microsoft"});
            Orders.Insert(new Order {UniqueId = "4", CustomerName = "Unosquare"});
            Orders.Insert(new Order {UniqueId = "5", CustomerName = "Unosquare"});
        }

        public LiteDbSet<Order> Orders { get; set; }
    }
}