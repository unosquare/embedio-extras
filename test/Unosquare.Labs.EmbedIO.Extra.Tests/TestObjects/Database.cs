using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Labs.LiteLib;

namespace Unosquare.Labs.EmbedIO.Extra.Tests.TestObjects
{
    public class Order : LiteModel
    {
        [LiteUnique]
        public string UniqueId { get; set; }

        [LiteIndex]
        public string CustomerName { get; set; }

        [StringLength(30)]
        public string ShipperCity { get; set; }

        public bool IsShipped { get; set; }
        public int Amount { get; set; }
        public string ShippedDate { get; set; }

    }

    internal class TestDbContext : LiteDbContext
    {
        public TestDbContext(string name = "testDb") : base(Path.Combine(Path.GetTempPath(), $"{name}.db"))
        {
            if (Orders.Count() != 0) return;

            Orders.Insert(new Order() { UniqueId = "1", CustomerName = "Unosquare" });
            Orders.Insert(new Order() { UniqueId = "2", CustomerName = "Apple" });
            Orders.Insert(new Order() { UniqueId = "3", CustomerName = "Microsoft" });
            Orders.Insert(new Order() { UniqueId = "4", CustomerName = "Unosquare" });
            Orders.Insert(new Order() { UniqueId = "5", CustomerName = "Unosquare" });
        }

        public LiteDbSet<Order> Orders { get; set; }
    }
}
