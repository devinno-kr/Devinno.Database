using Devinno.Database;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            var db = new SQLite { FileName = "db.sqlite" };
            var tbl = "tblTest";
            
            Console.WriteLine("DropTable");     db.DropTable(tbl);
            Console.WriteLine("CreateTable");   db.CreateTable<Data>(tbl);
            Console.WriteLine("Insert");        db.Insert<Data>(tbl, new Data
                                                {
                                                    Humidity = 70,
                                                    Temperature = 36.5,
                                                    Operation = true,
                                                    Count = 10,
                                                });
                                                Print(db.Select<Data>(tbl));
            Console.WriteLine("Update");        var d = db.Select<Data>(tbl).FirstOrDefault();
                                                d.Humidity = 100;
                                                d.OpenClose = true;
                                                d.Operation = false;
                                                d.Count = 15;
                                                d.Description = "Test";
                                                db.Update<Data>(tbl, d);
                                                Print(db.Select<Data>(tbl));
            Console.WriteLine("Delete");        db.Delete<Data>(tbl, d);
                                                Print(db.Select<Data>(tbl)); 
            */
            App app = new App();
            Console.ReadKey();
          var len=  Guid.NewGuid().ToString().Length;
        }

        static void Print(List<Data> ls) => ls.ForEach((v) => Console.WriteLine(v));

        #region class : Data
        public class Data
        {
            [SqlKey(AutoIncrement = true)]
            public int Id { get; set; }

            public double Humidity { get; set; }
            public double Temperature { get; set; }
            public bool OpenClose { get; set; }
            public bool Operation { get; set; }
            public int? Count { get; set; }
            public string? Description { get; set; }

            public override string ToString() =>
                $"{Id},{Humidity},{Temperature},{OpenClose},{Operation},{Count},{Description}";
        }
        #endregion
    }


}
