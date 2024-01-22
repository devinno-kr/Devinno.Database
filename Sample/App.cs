using Devinno.Communications.Modbus;
using Devinno.Communications.Modbus.RTU;
using Devinno.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample
{
    public class App
    {
        public App()
        {

            var db = new MySQL
            {
                Host = "localhost",
                ID = "root",
                Password = "mysql",
                DatabaseName = "db_sensio",
                Port = 3317,
                ConnectStringOptions = "Character Set=utf8"
            };

            db.CreateTable<IComm>("tblComms");
            
        }



        #region enum : CommType
        public enum CommType { SlaveRTU, MasterRTU, SlaveTCP, MasterTCP }
        #endregion

        #region interface : IComm
        public interface IComm
        {
            [SqlKey(AutoIncrement = false)] string CommID { get; set; }
            string? Name { get; set; }
            [SqlIgnore] CommType Type { get; }
            [SqlIgnore] bool IsStart { get; }

            void Start();
            void Stop();
        }
        #endregion
    }
}
