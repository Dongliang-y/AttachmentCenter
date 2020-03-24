using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Dapper;
using Microsoft.Data.Sqlite;

namespace AttachmentCenter.Utils
{
    public class  SQLiteBaseRepository
    {
        public static string DbFile
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VideoInfo.db");
            }
        }
        public static SqliteConnection SimpleDbConnection()
        {

            string connString = string.Format("Data Source={0};Password=******;", DbFile);
            var conn = new SqliteConnection(connString);
            //数据库存在则跳过
            if (!File.Exists(DbFile))
            {
                conn.Execute(@"
                        CREATE TABLE Player (
                            Id VARCHAR(16),
                            Name VARCHAR(32),
                            RegDate DATETIME,
                            Score INTEGER,
                            BinData BLOB,
                            CONSTRAINT Player_PK PRIMARY KEY (Id)
                        )");
                throw new Exception("数据库文件不存在！");
            }
            return conn;
        }
    }
    public class SQLiteDbHelper : IDisposable
    {
        /// <summary>
        /// 常量；
        /// </summary>
        const string INSERT_TABLE_ITEM_VALUE = "insert into {0} ({1}) values ({2})";
        const string DELETE_TABLE_WHERE = "delete from {0} where {1}";
        const string UPDATE_TABLE_EDITITEM = "update {0} set {1}";
        const string UPDATE_TABLE_EDITITEM_WHERE = "update {0} set {1} where {2}";
        const string Query_ITEM_TABLE_WHERE = "select {0} from {1} where {2}";

        private SqliteConnection conn;

        public SQLiteDbHelper()
        {
            conn = openDataConnection();
        }
        /// <summary>
        /// 打开数据库链接；
        /// </summary>
        /// <returns></returns>
        private SqliteConnection openDataConnection()
        {
            var conn = SQLiteBaseRepository.SimpleDbConnection();
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            return conn;
        }
        /// <summary>
        /// 1.1 新增实体；
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="model">实体</param>
        /// <param name="autoPrimaryKey">自增主键名称</param>
        /// <returns></returns>
        public int Add<T>(T model, string autoPrimaryKey = "id")
        {
            var insertSql = GetInsertSql<T>(model, autoPrimaryKey);
            return conn.Execute(insertSql);
        }
        /// <summary>
        /// 批量新增
        /// </summary>
        /// <typeparam name="T">实休类</typeparam>
        /// <param name="addData">实体数据列表</param>
        /// <param name="autoPrimaryKey">自增主键名称</param>
        /// <returns></returns>
        public int Adds<T>(List<T> models, string autoPrimaryKey = "id")
        {
            var type = typeof(T);
            int resultN = 0;
            var transaction = conn.BeginTransaction();
            try
            {
                models.ForEach(d =>
                {
                    var insertSql = GetInsertSql<T>(d);
                    resultN += conn.Execute(insertSql);
                });
                transaction.Commit();
            }
            catch (Exception)
            {
                resultN = 0;
                transaction.Rollback();
            }
            return resultN;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="where">删除条件</param>
        /// <returns></returns>
        public int Delete<T>(string where)
        {
            var type = typeof(T);
            string sqlStr = string.Format(DELETE_TABLE_WHERE, type.Name, where);
            return conn.Execute(sqlStr);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Delete(string tableName, string where)
        {
            string sqlStr = string.Format(DELETE_TABLE_WHERE, tableName, where);
            return conn.Execute(sqlStr);
        }
        /// <summary>
        /// 修改; 
        /// </summary>
        /// <typeparam name="T">实体 Type </typeparam>
        /// <param name="model">实体</param>
        /// <param name="where">修改条件</param>
        /// <param name="attrs">要修改的实休属性数组</param>
        /// <returns></returns>
        public int Edit<T>(T model, string where, params string[] attrs)
        {
            var sqlStr = GetUpdateSql<T>(model, where, attrs);
            return conn.Execute(sqlStr);
        }

        /// <summary>
        /// 根据条件查询单一实体;
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="where">查询条件；</param>
        /// <param name="attrs">要查询的字段(传入 * 为查询所有字段。)</param>
        /// <returns></returns>
        public T QeryByWhere<T>(string where, params string[] attrs)
        {
            Type type = typeof(T);
            string item = attrs.Length == 1 && attrs[0] == "*" ? "*" : string.Join(",", attrs);
            var sqlStr = string.Format(Query_ITEM_TABLE_WHERE, item, type.Name, where);
            return conn.Query<T>(sqlStr).FirstOrDefault();
        }

        /// <summary>
        /// 根据条件查询符合条件的所有实体;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public List<T> QueryMultiByWhere<T>(string where)
        {
            Type type = typeof(T);
            var sqlStr = string.Format(Query_ITEM_TABLE_WHERE, "*", type.Name, where);
            return conn.Query<T>(sqlStr).ToList();
        }

        /// <summary>
        /// 生成新增 sql 语句；
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="autoPrimaryKey"></param>
        /// <returns></returns>
        private string GetInsertSql<T>(T model, string autoPrimaryKey = "id")
        {
            Type t = typeof(T);
            var propertyInfo = t.GetProperties();
            var proDic = propertyInfo.Where(s => !s.Name.Equals(autoPrimaryKey, StringComparison.InvariantCultureIgnoreCase))
                .Select(s => new
                {
                    key = s.Name,
                    value = GetValue<T>(s, model)
                })
                .ToDictionary(s => s.key, s => s.value);
            proDic = proDic.Where(s => s.Value != "''").ToDictionary(s => s.Key, s => s.Value);
            var items = string.Join(",", proDic.Keys);
            var values = string.Join(",", proDic.Values);
            return string.Format(INSERT_TABLE_ITEM_VALUE, t.Name, items, values);
        }

        /// <summary>
        /// 获取属性值；
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="info">字段属性信息</param>
        /// <param name="model">实体</param>
        /// <returns></returns>
        private string GetValue<T>(PropertyInfo info, T model)
        {
            Type type = info.PropertyType;
            var tempStr = string.Empty;
            if (type == typeof(string))
            {
                tempStr = string.Format("'{0}'", info.GetValue(model));
                return tempStr;
            }
            if (type == typeof(DateTime))
            {
                tempStr = string.Format("'{0}'", ((DateTime)info.GetValue(model)).ToString("s"));
                return tempStr;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var types = type.GetGenericArguments();
                if (types[0] == typeof(DateTime))
                {
                    tempStr = string.Format("'{0}'", ((DateTime)info.GetValue(model)).ToString("s"));
                }
                tempStr = string.Format("'{0}'", info.GetValue(model));
                return tempStr;
            }
            tempStr = info.GetValue(model).ToString();
            return tempStr;
        }

        /// <summary>
        /// 生成更新 sql 语句；
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="pro"></param>
        /// <param name="attrs"></param>
        /// <returns></returns>
        private string GetUpdateSql<T>(T model, string where, params string[] attrs)
        {
            Type t = typeof(T);
            var propertyInfo = t.GetProperties();
            var updateInfo = propertyInfo
                .Where(s => attrs.Contains(s.Name))
                .Select(s =>
                {
                    if (s.PropertyType == typeof(string))
                    {
                        return string.Format("{0}='{1}'", s.Name, s.GetValue(model));
                    }
                    if (s.PropertyType == typeof(DateTime))
                    {
                        return string.Format("{0}='{1}'", s.Name, ((DateTime)s.GetValue(model)).ToString("s"));
                    }
                    if (s.PropertyType.IsGenericType && s.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        Type[] types = s.PropertyType.GetGenericArguments();
                        if (types[0] == typeof(DateTime))
                        {
                            return string.Format("{0}='{1}'", s.Name, ((DateTime)s.GetValue(model)).ToString("s"));
                        }
                        return string.Format("{0}={1}", s.Name, s.GetValue(model));
                    }
                    return string.Format("{0}={1}", s.Name, s.GetValue(model));
                })
                .ToArray();
            var setStr = string.Join(",", updateInfo);
            var sqlStr = string.Format(UPDATE_TABLE_EDITITEM_WHERE, t.Name, setStr, where);
            return sqlStr;
        }
        /// <summary>
        /// 释放数据连接；
        /// </summary>
        public void Dispose()
        {
            conn.Close();
            conn.Dispose();
        }

    }
}