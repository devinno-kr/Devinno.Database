﻿using Devinno.Tools;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Devinno.Database
{
    #region class : MsSQL
    public class MsSQL
    {
        #region Properties
        public string Host { get; set; } = "localhost";
        public string ID { get; set; } = "root";
        public string Password { get; set; } = "1234";
        public string DatabaseName { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string ConnectStringOptions { get; set; } = "";
        public int Port { get; set; } = 1433;
        private string ConnectString => (IntegratedSecurity
            ? $"Server={Host};Database={DatabaseName};Integrated Security=True;" + ConnectStringOptions
            : $"Server={Host},{Port};Database={DatabaseName};Uid={ID};pwd={Password};" + ConnectStringOptions);
        #endregion

        #region Constructor
        public MsSQL()
        {

        }
        #endregion

        #region Method
        #region Table
        public void CreateTable<T>(string TableName) { Execute((conn, cmd, trans) => { MsSqlCommandTool.CreateTable<T>(cmd, TableName); }); }
        public void DropTable(string TableName) { Execute((conn, cmd, trans) => { MsSqlCommandTool.DropTable(cmd, TableName); }); }
        public bool ExistTable(string TableName) { bool ret = false; Execute((conn, cmd, trans) => { ret = MsSqlCommandTool.ExistTable(cmd, TableName); }); return ret; }
        #endregion

        #region Command
        #region Exist
        public bool Exist<T>(string TableName, T Data)
        {
            bool ret = false; 
            Execute((conn, cmd, trans) => { ret = MsSqlCommandTool.Exist<T>(cmd, TableName, Data); });
            return ret;
        }
        #endregion
        #region Check
        public bool Check(string TableName, string Where)
        {
            bool ret = false;
            Execute((conn, cmd, trans) => { ret = MsSqlCommandTool.Check(cmd, TableName, Where); });
            return ret;
        }
        #endregion
        #region Select
        public List<T> Select<T>(string TableName) => Select<T>(TableName, (string)null);
        public List<T> Select<T>(string TableName, string Where)
        {
            List<T> ret = null;
            Execute((conn, cmd, trans) => { ret = MsSqlCommandTool.Select<T>(cmd, TableName, Where); });
            return ret;
        }

        public List<T> Select<T>(string TableName, Func<SqlDataReader, T> parse) => Select<T>(TableName, null, parse);
        public List<T> Select<T>(string TableName, string Where, Func<SqlDataReader, T> parse)
        {
            List<T> ret = null;
            Execute((conn, cmd, trans) => { ret = MsSqlCommandTool.Select<T>(cmd, TableName, Where, parse); });
            return ret;
        }
        #endregion 
        #region Update
        public void Update<T>(string TableName, params T[] Datas)
        {
            Transaction((conn, trans) =>
            {
                foreach (var Data in Datas)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        MsSqlCommandTool.Update<T>(cmd, TableName, Data);
                    }
                }
            });
        }

        public void Update<T>(string TableName, Func<PropertyInfo, T, object> parse, params T[] Datas)
        {
            Transaction((conn, trans) =>
            {
                var kp = MsSqlCommandTool.GetKeysProps<T>();
                foreach (var Data in Datas)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        MsSqlCommandTool.Update<T>(cmd, TableName, kp, Data, parse);
                    }
                }
            });
        }

        public void Update<T>(string TableName, Action<List<PropertyInfo>, T, SqlParameterCollection> parse, params T[] Datas)
        {
            Transaction((conn, trans) =>
            {
                var kp = MsSqlCommandTool.GetKeysProps<T>();
                foreach (var Data in Datas)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        MsSqlCommandTool.Update<T>(cmd, TableName, kp, Data, parse);
                    }
                }
            });
        }
        #endregion
        #region Insert
        public void Insert<T>(string TableName, params T[] Datas)
        {
            Transaction((conn, trans) =>
            {
                var cols = MsSqlCommandTool.GetColumns<T>();
                foreach (var Data in Datas)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        MsSqlCommandTool.Insert<T>(cmd, TableName, cols, Data);
                    }
                }
            });
        }

        public void Insert<T>(string TableName, Func<PropertyInfo, T, object> parse, params T[] Datas)
        {
            Transaction((conn, trans) =>
            {
                var cols = MsSqlCommandTool.GetColumns<T>();
                foreach (var Data in Datas)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        MsSqlCommandTool.Insert<T>(cmd, TableName, cols, Data, parse);
                    }
                }
            });
        }

        public void Insert<T>(string TableName, Action<List<PropertyInfo>, T, SqlParameterCollection> parse, params T[] Datas)
        {
            Transaction((conn, trans) =>
            {
                var cols = MsSqlCommandTool.GetColumns<T>();
                foreach (var Data in Datas)
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        MsSqlCommandTool.Insert<T>(cmd, TableName, cols, Data, parse);
                    }
                }
            });
        }
        #endregion
        #region Delete
        public void Delete<T>(string TableName, params T[] Datas) { Execute((conn, cmd, trans) => { MsSqlCommandTool.Delete<T>(cmd, TableName, Datas); }); }
        public void Delete(string TableName, string Where)
        {
            Execute((conn, cmd, trans) => { MsSqlCommandTool.Delete(cmd, TableName, Where); });
        }
        #endregion
        #endregion

        #region Execute
        public void Execute(Action<SqlConnection, SqlCommand, SqlTransaction> ExcuteQuery)
        {
            using (var conn = new SqlConnection(ConnectString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        try
                        {
                            ExcuteQuery(conn, cmd, trans);
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            try { trans.Rollback(); }
                            catch (SqlException ex2) { }

                            throw ex;
                        }
                    }
                }
            }
        }

        public void Transaction(Action<SqlConnection, SqlTransaction> ExcuteTransaction)
        {
            using (var conn = new SqlConnection(ConnectString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        ExcuteTransaction(conn, trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { trans.Rollback(); }
                        catch (SqlException ex2) { }

                        throw ex;
                    }
                }
            }
        }
        #endregion
        #endregion
    }
    #endregion
    #region class : MsSqlCommandTool
    public class MsSqlCommandTool
    {
        #region Method
        #region Command
        #region CreateTable
        public static void CreateTable<T>(SqlCommand cmd, string TableName)
        {
            var keys = typeof(T).GetProperties().Where(x => x.CanRead && (x.CanWrite || Attribute.IsDefined(x, typeof(SqlReadOnlyAttribute))) && Attribute.IsDefined(x, typeof(SqlKeyAttribute))).ToList();
            var props = typeof(T).GetProperties().Where(x => x.CanRead && (x.CanWrite || Attribute.IsDefined(x, typeof(SqlReadOnlyAttribute))) && !Attribute.IsDefined(x, typeof(SqlIgnoreAttribute)) && !Attribute.IsDefined(x, typeof(SqlKeyAttribute))).ToList();

            if (props.Count > 0)
            {
                var sb = new StringBuilder();

                sb.AppendLine("IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + TableName + "')");
                sb.AppendLine("CREATE TABLE [" + TableName + "]");
                sb.AppendLine("(");

                foreach (var p in keys) sb.AppendLine(GetKeyText(p) + ",");
                foreach (var p in props) sb.AppendLine(GetTypeText(p) + ",");

                if (keys.Count > 0)
                {
                    var sK = "";
                    foreach (var v in keys) sK += "[" + v.Name + "],";
                    sK = sK.Trim(',');
                    sb.AppendLine($"    CONSTRAINT [PK_{TableName}] PRIMARY KEY({sK})");
                }

                sb.AppendLine(")");

                cmd.CommandText = sb.ToString();
                cmd.ExecuteNonQuery();
            }
        }
        #endregion
        #region DropTable
        public static void DropTable(SqlCommand cmd, string TableName)
        {
            cmd.CommandText = "DROP TABLE IF EXISTS [" + TableName + "]";
            cmd.ExecuteNonQuery();
        }
        #endregion
        #region ExistTable
        public static bool ExistTable(SqlCommand cmd, string TableName)
        {
            bool ret = false;
            cmd.CommandText = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + TableName + "'";
            using (var reader = cmd.ExecuteReader())
            {
                ret = reader.HasRows;
            }
            return ret;
        }
        #endregion
        #region Exists
        public static bool Exist<T>(SqlCommand cmd, string TableName, T Data)
        {
            bool ret = false;

            string sql = $"SELECT * FROM [{TableName}] {GetWhere<T>(Data)}";
            cmd.CommandText = sql;
            using (var rd = cmd.ExecuteReader())
            {
                ret = rd.HasRows;
            }

            return ret;
        }
        #endregion
        #region Check
        public static bool Check(SqlCommand cmd, string TableName, string Where)
        {
            bool ret = false;

            string sql = "SELECT * FROM [" + TableName + "] " + Where;

            cmd.CommandText = sql;
            using (var rd = cmd.ExecuteReader())
            {
                ret = rd.HasRows;
            }

            return ret;
        }
        #endregion
        #region Select
        public static List<T> Select<T>(SqlCommand cmd, string TableName, string Where)
        {
            List<T> ret = null;

            string sql = "SELECT * FROM [" + TableName + "]";
            if (!string.IsNullOrEmpty(Where)) sql += " " + Where;

            var props = typeof(T).GetProperties().Where(x => x.CanRead && (x.CanWrite || Attribute.IsDefined(x, typeof(SqlReadOnlyAttribute))) && !Attribute.IsDefined(x, typeof(SqlIgnoreAttribute))).ToList();

            cmd.CommandText = sql;

            using (var rd = cmd.ExecuteReader())
            {
                ret = new List<T>();
                while (rd.Read())
                {
                    var v = (T)Activator.CreateInstance(typeof(T));
                    Read(rd, props, v);
                    ret.Add(v);
                }
            }
            return ret;
        }

        public static List<T> Select<T>(SqlCommand cmd, string TableName, string Where, Func<SqlDataReader, T> parse)
        {
            List<T> ret = null;

            string sql = "SELECT * FROM `" + TableName + "`";
            if (!string.IsNullOrEmpty(Where)) sql += " " + Where;

            var props = typeof(T).GetProperties().Where(x => x.CanRead && (x.CanWrite || Attribute.IsDefined(x, typeof(SqlReadOnlyAttribute))) && !Attribute.IsDefined(x, typeof(SqlIgnoreAttribute))).ToList();

            cmd.CommandText = sql;

            using (var rd = cmd.ExecuteReader())
            {
                ret = new List<T>();
                while (rd.Read())
                {
                    var v = parse(rd);
                    if (v != null) ret.Add(v);
                }
            }

            return ret;
        }
        #endregion
        #region Update
        public static void Update<T>(SqlCommand cmd, string TableName, T Data)
        {
            if (Data != null)
            {
                var keys = typeof(T).GetProperties().Where(x => x.CanRead && (x.CanWrite || Attribute.IsDefined(x, typeof(SqlReadOnlyAttribute))) && Attribute.IsDefined(x, typeof(SqlKeyAttribute))).ToList();
                var props = typeof(T).GetProperties().Where(x => x.CanRead && (x.CanWrite || Attribute.IsDefined(x, typeof(SqlReadOnlyAttribute))) && !Attribute.IsDefined(x, typeof(SqlIgnoreAttribute)) && !Attribute.IsDefined(x, typeof(SqlKeyAttribute))).ToList();


                string sql = $"UPDATE [{TableName}] SET ";
                string where = GetWhere<T>(keys, Data);
                foreach (var p in props) sql += $" [{p.Name}] = @{p.Name},";
                sql = sql.Substring(0, sql.Length - 1) + where;

                cmd.CommandText = sql;
                foreach (var pi in props) cmd.Parameters.AddWithValue("@" + pi.Name, GetValue(Data, pi));
                cmd.ExecuteNonQuery();
            }
        }

        public static void Update<T>(SqlCommand cmd, string TableName, KeyProps kp, T Data, Func<PropertyInfo, T, object> parse)
        {
            if (Data != null)
            {
                var keys = kp.Keys;
                var props = kp.Props;

                string sql = $"UPDATE `{TableName}` SET ";
                string where = GetWhere<T>(keys, Data);
                foreach (var p in props) sql += $" `{p.Name}` = @{p.Name},";
                sql = sql.Substring(0, sql.Length - 1) + where;

                cmd.CommandText = sql;
                foreach (var pi in props) cmd.Parameters.AddWithValue("@" + pi.Name, parse(pi, Data));
                cmd.ExecuteNonQuery();
            }
        }

        public static void Update<T>(SqlCommand cmd, string TableName, KeyProps kp, T Data, Action<List<PropertyInfo>, T, SqlParameterCollection> parse)
        {
            if (Data != null)
            {
                var keys = kp.Keys;
                var props = kp.Props;

                string sql = $"UPDATE `{TableName}` SET ";
                string where = GetWhere<T>(keys, Data);
                foreach (var p in props) sql += $" `{p.Name}` = @{p.Name},";
                sql = sql.Substring(0, sql.Length - 1) + where;

                cmd.CommandText = sql;

                parse(props, Data, cmd.Parameters);

                cmd.ExecuteNonQuery();
            }
        }
        #endregion
        #region Insert
        public static void Insert<T>(SqlCommand cmd, string TableName, List<PropertyInfo> cols, T Data)
        {
            if (Data != null)
            {
                string s_insert_in = string.Concat(cols.Select(x => $" [{x.Name}],").ToArray());
                string s_values_in = string.Concat(cols.Select(x => $" @{x.Name},").ToArray());

                s_values_in = s_values_in.Substring(0, s_values_in.Length - 1);
                s_insert_in = s_insert_in.Substring(0, s_insert_in.Length - 1);

                string s_insert = $"INSERT INTO [{TableName}] ({s_insert_in})";
                string s_values = $"VALUES ({s_values_in})";
                string s_sql = s_insert + "\r\n" + s_values;

                cmd.CommandText = s_sql;
                foreach (var pi in cols) cmd.Parameters.AddWithValue("@" + pi.Name, GetValue(Data, pi));

                cmd.ExecuteNonQuery();
            }
        }

        public static void Insert<T>(SqlCommand cmd, string TableName, List<PropertyInfo> cols, T Data, Func<PropertyInfo, T, object> parse)
        {
            if (Data != null)
            {
                string s_insert_in = string.Concat(cols.Select(x => $" `{x.Name}`,").ToArray());
                string s_values_in = string.Concat(cols.Select(x => $" @{x.Name},").ToArray());

                s_values_in = s_values_in.Substring(0, s_values_in.Length - 1);
                s_insert_in = s_insert_in.Substring(0, s_insert_in.Length - 1);

                string s_insert = $"INSERT INTO `{TableName}` ({s_insert_in})";
                string s_values = $"VALUES ({s_values_in})";
                string s_sql = s_insert + "\r\n" + s_values;

                cmd.CommandText = s_sql;
                foreach (var pi in cols) cmd.Parameters.AddWithValue("@" + pi.Name, parse(pi, Data));

                cmd.ExecuteNonQuery();
            }
        }

        public static void Insert<T>(SqlCommand cmd, string TableName, List<PropertyInfo> cols, T Data, Action<List<PropertyInfo>, T, SqlParameterCollection> parse)
        {
            if (Data != null)
            {
                string s_insert_in = string.Concat(cols.Select(x => $" `{x.Name}`,").ToArray());
                string s_values_in = string.Concat(cols.Select(x => $" @{x.Name},").ToArray());

                s_values_in = s_values_in.Substring(0, s_values_in.Length - 1);
                s_insert_in = s_insert_in.Substring(0, s_insert_in.Length - 1);

                string s_insert = $"INSERT INTO `{TableName}` ({s_insert_in})";
                string s_values = $"VALUES ({s_values_in})";
                string s_sql = s_insert + "\r\n" + s_values;

                cmd.CommandText = s_sql;

                parse(cols, Data, cmd.Parameters);

                cmd.ExecuteNonQuery();
            }
        }
        #endregion
        #region Delete
        public static void Delete<T>(SqlCommand cmd, string TableName, params T[] Datas)
        {
            if (Datas.Length > 0)
            {
                string sql = "DELETE FROM [" + TableName + "]\r\n" + GetWhere<T>(Datas);

                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(SqlCommand cmd, string TableName, string Where)
        {
            string sql = "DELETE FROM [" + TableName + "]\r\n" + Where;

            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
        #endregion
        #endregion

        #region Tool
        #region GetTypeText
        static string GetTypeText(PropertyInfo pi)
        {
            string ret = null;
            var tp = pi.PropertyType;

            try
            {
                var ni = GetNullableInfo(pi);
                if (ni != null)
                {
                    string sType = null, sDefault = null;
                    #region sType / sDefault
                    if (ni.Type == typeof(bool)) { sType = "[bit]"; sDefault = "0"; }
                    else if (ni.Type == typeof(sbyte)) { sType = "[int]"; sDefault = "0"; }
                    else if (ni.Type == typeof(byte)) { sType = "[int]"; sDefault = "0"; }
                    else if (ni.Type == typeof(short)) { sType = "[int]"; sDefault = "0"; }
                    else if (ni.Type == typeof(ushort)) { sType = "[int]"; sDefault = "0"; }
                    else if (ni.Type == typeof(int)) { sType = "[int]"; sDefault = "0"; }
                    else if (ni.Type == typeof(uint)) { sType = "[bigint]"; sDefault = "0"; }
                    else if (ni.Type == typeof(long)) { sType = "[bigint]"; sDefault = "0"; }
                    else if (ni.Type == typeof(ulong)) { sType = "[bigint]"; sDefault = "0"; }
                    else if (ni.Type == typeof(float)) { sType = "[real]"; sDefault = "0"; }
                    else if (ni.Type == typeof(double)) { sType = "[float]"; sDefault = "0"; }
                    else if (ni.Type == typeof(decimal)) { sType = "[decimal]"; sDefault = "0"; }
                    else if (ni.Type == typeof(char)) { sType = "[char](1)"; sDefault = "' '"; }
                    else if (ni.Type == typeof(string)) { sType = "[varchar](max)"; sDefault = "''"; }
                    else if (ni.Type == typeof(DateTime)) { sType = "[datetime]"; sDefault = "'1970-01-01 00:00:00'"; }
                    else if (ni.Type == typeof(TimeSpan)) { sType = "[text]"; sDefault = "'00:00:00'"; }
                    else if (ni.Type.IsEnum) { sType = "[int]"; sDefault = "0"; }
                    else if (ni.Type == typeof(byte[])) { sType = "[varchar](max)"; sDefault = "NULL"; }
                    else if (ni.Type == typeof(Bitmap)) { sType = "[varchar](max)"; sDefault = "NULL"; }
                    else throw new Exception("Unknown Type");

                    var sqlType = pi.CustomAttributes.Where(x => x.AttributeType == typeof(SqlTypeAttribute)).FirstOrDefault()
                                            ?.NamedArguments.Where(x => x.MemberName == "TypeString").FirstOrDefault().TypedValue.Value ?? null;

                    if (sqlType != null && sqlType is string) sType = (string)sqlType;
                    #endregion

                    if (sType != null)
                    {
                        if (ni.IsNullable)
                        {
                            ret = $"     [{pi.Name}] {sType} DEFAULT NULL";
                        }
                        else
                        {
                            if (sDefault != null)
                            {
                                if (sDefault == "NULL")
                                    ret = $"     [{pi.Name}] {sType} DEFAULT NULL";
                                else
                                    ret = $"     [{pi.Name}] {sType} NOT NULL DEFAULT {sDefault}";
                            }
                            else ret = $"     [{pi.Name}] {sType}";
                        }
                    }
                }
                else throw new Exception("Unknown Type");
            }
            catch (Exception ex) { }

            return ret;
        }
        #endregion
        #region GetKeyText
        static string GetKeyText(PropertyInfo pi)
        {
            string ret = null;
            var tp = pi.PropertyType;


            var ni = GetNullableInfo(pi);
            if (ni != null)
            {
                string sType = null, sAutoInc = "";
                #region sType / sAutoInc
                if (ni.Type == typeof(int) && !ni.IsNullable) { sType = "[int]"; }
                else if (ni.Type == typeof(uint) && !ni.IsNullable) { sType = "[bigint]"; }
                else if (ni.Type == typeof(long) && !ni.IsNullable) { sType = "[bigint]"; }
                else if (ni.Type == typeof(ulong) && !ni.IsNullable) { sType = "[bigint]"; }
                else if (ni.Type == typeof(string) && !ni.IsNullable) { sType = "[varchar](40)"; }
                else new Exception("This type cannot be used as a key.");

                #region SqlType.TypeString
                var sqlType = pi.CustomAttributes.Where(x => x.AttributeType == typeof(SqlTypeAttribute)).FirstOrDefault()
                                            ?.NamedArguments.Where(x => x.MemberName == "TypeString").FirstOrDefault().TypedValue.Value ?? null;

                if (sqlType != null && sqlType is string) sType = (string)sqlType;
                #endregion
                #region SqlKey.AutoIncrement
                var bAutoInc = pi.CustomAttributes.Where(x => x.AttributeType == typeof(SqlKeyAttribute)).FirstOrDefault()
                                  ?.NamedArguments.Where(x => x.MemberName == "AutoIncrement").FirstOrDefault().TypedValue.Value ?? false;


                if (bAutoInc is bool && (bool)bAutoInc) sAutoInc = "IDENTITY(1,1)";
                #endregion
                #endregion

                if (sType != null)
                {
                    ret = $"     [{pi.Name}] {sType} {sAutoInc} NOT NULL";
                }
            }
            else throw new Exception("This type cannot be used as a key.");

            return ret;
        }
        #endregion
        #region GetValue
        static object GetValue(object Data, PropertyInfo pi)
        {
            var tp = pi.PropertyType;
            object ret = DBNull.Value;

            var ni = GetNullableInfo(pi);
            if (ni != null)
            {
                var value = pi.GetValue(Data, null);
                if (value != null)
                {
                    if (ni.Type == typeof(sbyte)) ret = Convert.ToInt32(value);
                    else if (ni.Type == typeof(ushort)) ret = Convert.ToInt32(value);
                    else if (ni.Type == typeof(uint)) ret = Convert.ToInt64(value);
                    else if (ni.Type == typeof(ulong)) ret = Convert.ToInt64(value);
                    else if (ni.Type.IsEnum) ret = (int)value;
                    else if (ni.Type == typeof(TimeSpan)) ret = ((TimeSpan)value).ToString();
                    else if (ni.Type == typeof(byte[])) ret = CryptoTool.EncodeBase64String((byte[])value);
                    else if (ni.Type == typeof(Bitmap)) ret = CryptoTool.EncodeBase64String((Bitmap)value);
                    else ret = value;
                }
            }
            return ret;
        }
        #endregion
        #region GetValueText
        static string GetValueText(PropertyInfo p, object Value)
        {
            string ret = "";

            var ni = GetNullableInfo(p);
            if (ni != null)
            {
                var v = p.GetValue(Value);

                if (ni.Type == typeof(bool)) ret = $"{(v != null ? ((bool)v ? "1" : "0") : "NULL")}";
                else if (ni.Type == typeof(sbyte)) ret = $"{(v != null ? ((sbyte)v).ToString() : "NULL")}";
                else if (ni.Type == typeof(byte)) ret = $"{(v != null ? ((byte)v).ToString() : "NULL")}";
                else if (ni.Type == typeof(short)) ret = $"{(v != null ? ((short)v).ToString() : "NULL")}";
                else if (ni.Type == typeof(ushort)) ret = $"{(v != null ? ((ushort)v).ToString() : "NULL")}";
                else if (ni.Type == typeof(int)) ret = $"{(v != null ? ((int)v).ToString() : "NULL")}";
                else if (ni.Type == typeof(uint)) ret = $"{(v != null ? ((uint)v).ToString() : "NULL")}";
                else if (ni.Type == typeof(long)) ret = $"{(v != null ? ((long)v).ToString() : "NULL")}";
                else if (ni.Type == typeof(ulong)) ret = $"{(v != null ? ((ulong)v).ToString() : "NULL")}";
                else if (ni.Type == typeof(float)) ret = $"{(v != null ? ((float)v).ToString() : "NULL")}";
                else if (ni.Type == typeof(double)) ret = $"{(v != null ? ((double)v).ToString() : "NULL")}";
                else if (ni.Type == typeof(decimal)) ret = $"{(v != null ? ((decimal)v).ToString() : "NULL")}";
                else if (ni.Type == typeof(char)) ret = $"{(v != null ? "'" + ((char)v) + "'" : "NULL")}";
                else if (ni.Type == typeof(string)) ret = $"{(v != null ? "'" + ((string)v) + "'" : "NULL")}";
                else if (ni.Type == typeof(DateTime)) ret = $"{(v != null ? "'" + ((DateTime)v).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'" : "NULL")}";
                else if (ni.Type == typeof(TimeSpan)) ret = $"{(v != null ? "'" + ((TimeSpan)v).ToString() + "'" : "NULL")}";
                else if (ni.Type.IsEnum) ret = $"{(v != null ? ((int)v).ToString() : "NULL")}";
                else if (ni.Type == typeof(byte[])) ret = $"{(v != null ? "'" + CryptoTool.EncodeBase64String((byte[])v) + "'" : "NULL")}";
                else if (ni.Type == typeof(Bitmap)) ret = $"{(v != null ? "'" + CryptoTool.EncodeBase64String((Bitmap)v) + "'" : "NULL")}";
                else throw new Exception("Unknown Type");

            }
            else throw new Exception("Unknown Type");

            return ret;
        }
        #endregion
        #region GetWhere
        public static string GetWhere<T>(params T[] Datas)
        {
            var keys = typeof(T).GetProperties().Where(x => x.CanRead && (x.CanWrite || Attribute.IsDefined(x, typeof(SqlReadOnlyAttribute))) && Attribute.IsDefined(x, typeof(SqlKeyAttribute))).ToList();

            return GetWhere(keys, Datas);
        }

        public static string GetWhere<T>(List<PropertyInfo> keys, params T[] Datas)
        {
            string ret = "";

            if (Datas != null && Datas.Length > 0)
            {
                ret += " WHERE ";

                for (int i = 0; i < Datas.Length; i++)
                {
                    var v = Datas[i];

                    ret += " (";
                    for (int j = 0; j < keys.Count; j++)
                    {
                        var k = keys[j];
                        ret += $" [{k.Name}] = {GetValueText(k, v)}" + (j < keys.Count - 1 ? " And " : "");
                    }

                    ret += " )" + (i < Datas.Length - 1 ? " Or " : "");
                }
            }

            return ret;
        }
        #endregion
        #region GetNullableInfo
        static NullableInfo GetNullableInfo(PropertyInfo pi)
        {
            NullableInfo ret = null;

            var tp = Nullable.GetUnderlyingType(pi.PropertyType);
            if (tp == null)
            {
                if (pi.CustomAttributes.Count() > 0)
                {
                    ret = new NullableInfo
                    {
                        IsNullable = pi.CustomAttributes.Where(x => x.AttributeType.ToString() == "System.Runtime.CompilerServices.NullableAttribute").FirstOrDefault() != null,
                        Type = pi.PropertyType,
                    };
                }
                else
                {
                    if (pi.PropertyType.CustomAttributes.Count() > 0)
                    {
                        ret = new NullableInfo
                        {
                            IsNullable = pi.PropertyType.CustomAttributes.Where(x => x.AttributeType.ToString() == "System.Runtime.CompilerServices.NullableAttribute").FirstOrDefault() != null,
                            Type = pi.PropertyType,
                        };
                    }
                    else
                    {
                        ret = new NullableInfo
                        {
                            IsNullable = false,
                            Type = pi.PropertyType,
                        };
                    }
                }
            }
            else
            {
                ret = new NullableInfo
                {
                    IsNullable = true,
                    Type = tp,
                };
            }

            return ret;
        }
        #endregion
        #region GetColumns
        public static List<PropertyInfo> GetColumns<T>()
        {
            #region cols
            var kp = GetKeysProps<T>();
            var keys = kp.Keys;
            var props = kp.Props;
            var cols = new List<PropertyInfo>();
            foreach (var v in keys)
            {
                var bAutoInc = v.CustomAttributes.Where(x => x.AttributeType == typeof(SqlKeyAttribute)).FirstOrDefault()
                                 ?.NamedArguments.Where(x => x.MemberName == "AutoIncrement").FirstOrDefault().TypedValue.Value ?? false;

                if (bAutoInc is bool && !(bool)bAutoInc) cols.Add(v);
            }
            cols.AddRange(props);
            #endregion

            return cols;
        }

        public static KeyProps GetKeysProps<T>()
        {
            var ret = new KeyProps();
            ret.Keys = typeof(T).GetProperties().Where(x => x.CanRead && (x.CanWrite || Attribute.IsDefined(x, typeof(SqlReadOnlyAttribute))) && Attribute.IsDefined(x, typeof(SqlKeyAttribute))).ToList();
            ret.Props = typeof(T).GetProperties().Where(x => x.CanRead && (x.CanWrite || Attribute.IsDefined(x, typeof(SqlReadOnlyAttribute))) && !Attribute.IsDefined(x, typeof(SqlIgnoreAttribute)) && !Attribute.IsDefined(x, typeof(SqlKeyAttribute))).ToList();
            return ret;
        }
        #endregion
        #region Read
        static void Read<T>(SqlDataReader? rd, List<PropertyInfo> props, T? v)
        {
            if (rd != null && v != null && props != null && props.Count > 0)
            {
                foreach (var p in props)
                {
                    var tp = p.PropertyType;
                    int idx = rd.GetOrdinal(p.Name);

                    var ni = GetNullableInfo(p);
                    if (ni != null)
                    {
                        try
                        {
                            #region bool
                            if (ni.Type == typeof(bool))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, rd.GetBoolean(idx));
                                }
                            }
                            #endregion
                            #region sbyte
                            else if (ni.Type == typeof(sbyte))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, Convert.ToSByte(rd.GetInt32(idx)));
                                }
                            }
                            #endregion
                            #region Byte
                            else if (ni.Type == typeof(byte))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, Convert.ToByte(rd.GetInt32(idx)));
                                }
                            }
                            #endregion
                            #region Short
                            else if (ni.Type == typeof(short))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, Convert.ToInt16(rd.GetInt32(idx)));
                                }
                            }
                            #endregion
                            #region UShort
                            else if (ni.Type == typeof(ushort))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, Convert.ToUInt16(rd.GetInt32(idx)));
                                }
                            }
                            #endregion
                            #region Int
                            else if (ni.Type == typeof(int))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, rd.GetInt32(idx));
                                }
                            }
                            #endregion
                            #region UInt
                            else if (ni.Type == typeof(uint))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, Convert.ToUInt32(rd.GetInt64(idx)));
                                }
                            }
                            #endregion
                            #region Long
                            else if (ni.Type == typeof(long))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, rd.GetInt64(idx));
                                }
                            }
                            #endregion
                            #region ULong
                            else if (ni.Type == typeof(ulong))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, Convert.ToUInt64(rd.GetInt64(idx)));
                                }
                            }
                            #endregion
                            #region Float
                            else if (ni.Type == typeof(float))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, rd.GetFloat(idx));
                                }
                            }
                            #endregion
                            #region Double
                            else if (ni.Type == typeof(double))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, rd.GetDouble(idx));
                                }
                            }
                            #endregion
                            #region Decimal
                            else if (ni.Type == typeof(decimal))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, rd.GetDecimal(idx));
                                }
                            }
                            #endregion
                            #region Char
                            else if (ni.Type == typeof(char))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    var s = rd.GetString(idx);
                                    if (s.Length >= 1) p.SetValue(v, s.First());
                                    else if (ni.IsNullable) p.SetValue(v, null);
                                }
                            }
                            #endregion
                            #region String
                            else if (ni.Type == typeof(string))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, rd.GetString(idx));
                                }
                            }
                            #endregion
                            #region DateTime
                            else if (ni.Type == typeof(DateTime))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, rd.GetDateTime(idx));
                                }
                            }
                            #endregion
                            #region TimeSpan
                            else if (ni.Type == typeof(TimeSpan))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, TimeSpan.Parse(rd.GetString(idx)));
                                }
                            }
                            #endregion
                            #region Enum
                            else if (ni.Type.IsEnum)
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    if (ni.IsNullable) p.SetValue(v, null);
                                }
                                else
                                {
                                    p.SetValue(v, Enum.ToObject(ni.Type, rd.GetInt32(idx)));
                                }
                            }
                            #endregion
                            #region Byte[]
                            else if (ni.Type == typeof(byte[]))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    p.SetValue(v, null);
                                }
                                else
                                {
                                    var ba = Convert.FromBase64String(rd.GetString(idx));
                                    p.SetValue(v, ba);
                                }
                            }
                            #endregion
                            #region Bitmap
                            else if (ni.Type == typeof(Bitmap))
                            {
                                if (rd.IsDBNull(idx))
                                {
                                    p.SetValue(v, null);
                                }
                                else
                                {
                                    using (var m = new MemoryStream(Convert.FromBase64String(rd.GetString(idx))))
                                    {
                                        p.SetValue(v, (Bitmap)Bitmap.FromStream(m));
                                    }
                                }
                            }
                            #endregion
                            #region Error
                            else throw new Exception("Unknown Type");
                            #endregion
                        }
                        catch (Exception ex) { throw new Exception("Parse Error"); }
                    }
                    else throw new Exception("Read Error");
                }
            }
        }
        #endregion
        #endregion
        #endregion
    }
    #endregion

}
