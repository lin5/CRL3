/**
* CRL 快速开发框架 V4.0
* Copyright (c) 2016 Hubro All rights reserved.
* GitHub https://github.com/hubro-xx/CRL3
* 主页 http://www.cnblogs.com/hubro
* 在线文档 http://crl.changqidongli.com/
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Data.Common;
using CRL.LambdaQuery;
namespace CRL.DBExtend.RelationDB
{
    /// <summary>
    /// 对象数据访问
    /// </summary>
    public sealed partial class DBExtend : AbsDBExtend
    {
        public DBExtend(DbContext _dbContext)
            : base(_dbContext)
        {
        }
        protected override LambdaQuery<TModel> CreateLambdaQuery<TModel>()
        {
            return new RelationLambdaQuery<TModel>(dbContext); 
        }
        #region 参数处理
        /// <summary>
        /// 清除参数
        /// </summary>
        public override void ClearParame()
        {
            dbHelper.ClearParams();
        }
        /// <summary>
        /// 增加参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public override void AddParam(string name, object value)
        {
            value = ObjectConvert.CheckNullValue(value);
            dbHelper.AddParam(name,value);
        }
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public override void SetParam(string name, object value)
        {
            value = ObjectConvert.CheckNullValue(value);
            dbHelper.SetParam(name, value);
        }
        /// <summary>
        /// 增加输出参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value">对应类型任意值</param>
        public override void AddOutParam(string name, object value = null)
        {
            dbHelper.AddOutParam(name, value);
        }
        /// <summary>
        /// 获取存储过程return的值
        /// </summary>
        /// <returns></returns>
        public override int GetReturnValue()
        {
            return dbHelper.GetReturnValue();
        }
        /// <summary>
        /// 获取OUTPUT的值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object GetOutParam(string name)
        {
            return dbHelper.GetOutParam(name);
        }
        /// <summary>
        /// 获取OUT值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public override T GetOutParam<T>(string name)
        {
            var obj = dbHelper.GetOutParam(name);
            return ObjectConvert.ConvertObject<T>(obj);
        }
        #endregion

        /// <summary>
        /// 通过关键类型,格式化SQL语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args">格式化SQL语句的关键类型</param>
        /// <returns></returns>
        string AutoFormat(string sql, params Type[] args)
        {
            if (args == null)
            {
                return sql;
            }
            if (args.Length == 0)
            {
                return sql;
            }
            //System.Object
            Regex r = new Regex(@"\$(\w+)", RegexOptions.IgnoreCase);//like $table
            Match m;
            List<string> pars = new List<string>();
            for (m = r.Match(sql); m.Success; m = m.NextMatch())
            {
                string par = m.Groups[1].ToString();
                if (!pars.Contains(par))
                {
                    pars.Add(par);
                }
            }
            foreach (string par in pars)
            {
                foreach (Type type in args)
                {
                    string tableName = TypeCache.GetTableName(type,null);
                    //string fullTypeName = type.FullName.Replace("+", ".") + ".";//like classA+classB
                    string fullTypeName = GetTypeFullName(type);
                    if (fullTypeName.IndexOf("." + par + ".") > -1)
                    {
                        sql = sql.Replace("$" + par, tableName);
                    }
                }
            }
            if (sql.IndexOf("$") > -1)
            {
                throw new Exception("格式化SQL语句时发生错误,表名未被替换:" + sql);
            }
            return sql;
        }
        static string GetTypeFullName(Type type)
        {
            string str = "";
            while (type != typeof(IModel))
            {
                str += "." + type.FullName + ".;";
                type = type.BaseType;
            }
            return str;
        }

        #region 语句查询
        /// <summary>
        /// 指定替换对象查询,并返回对象列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public override List<T> ExecList<T>(string sql, params Type[] types)
        {
            sql = _DBAdapter.SqlFormat(sql);
            var reader = GetDataReader(sql, types);
            double runTime;
            return ObjectConvert.DataReaderToList<T>(reader, out runTime);
        }
        
        DbDataReader GetDataReader(string sql, params Type[] types)
        {
            sql = AutoFormat(sql, types);
            sql = _DBAdapter.SqlFormat(sql);
            var  reader = dbHelper.ExecDataReader(sql);
            ClearParame();
            return reader;
        }
        public override Dictionary<TKey, TValue> ExecDictionary<TKey, TValue>(string sql, params Type[] types)
        {
            var reader = GetDataReader(sql, types);
            return ObjectConvert.DataReadToDictionary<TKey, TValue>(reader);
        }

        /// <summary>
        /// 指定替换对象更新
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public override int Execute(string sql, params Type[] types)
        {
            sql = AutoFormat(sql, types);
            sql = _DBAdapter.SqlFormat(sql);
            int count = dbHelper.Execute(sql);
            ClearParame();
            return count;
        }
        /// <summary>
        /// 指定替换对象返回单个结果
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="types">格式化SQL语句的关键类型</param>
        /// <returns></returns>
        public override object ExecScalar(string sql, params Type[] types)
        {
            sql = AutoFormat(sql, types);
            sql = _DBAdapter.SqlFormat(sql);
            object obj = dbHelper.ExecScalar(sql);
            ClearParame();
            return obj;
        }
        /// <summary>
        /// 指定替换对象返回单个结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public override T ExecScalar<T>(string sql, params Type[] types)
        {
            sql = _DBAdapter.SqlFormat(sql);
            var obj = ExecScalar(sql, types);
            return ObjectConvert.ConvertObject<T>(obj);
        }
        /// <summary>
        /// 返回首个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public override T ExecObject<T>(string sql, params Type[] types)
        {
            var list = ExecList<T>(sql, types);
            if (list.Count == 0)
                return null;
            return list[0];
        }
        #endregion

        #region 存储过程
        /// <summary>
        /// 执行存储过程返回结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        public override List<T> RunList<T>(string sp)
        {
            var reader = dbHelper.RunDataReader(sp);
            ClearParame();
            double runTime;
            return ObjectConvert.DataReaderToList<T>(reader, out runTime);
        }
        /// <summary>
        /// 执行一个存储过程
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public override int Run(string sp)
        {
            int count = dbHelper.Run(sp);
            ClearParame();
            return count;
        }
        /// <summary>
        /// 返回首个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        public override T RunObject<T>(string sp)
        {
            var list = RunList<T>(sp);
            if (list.Count == 0)
                return null;
            return list[0];
        }
        /// <summary>
        /// 执行存储过程并返回结果
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public override object RunScalar(string sp)
        {
            object obj = dbHelper.RunScalar(sp);
            ClearParame();
            return obj;
        }

        #endregion

        #region 事务控制

        /// <summary>
        /// 开始物务
        /// </summary>
        public override void BeginTran()
        {
            if (currentTransStatus != TranStatus.未开始)
            {
                throw new Exception("事务开始失败,已有未完成的事务");
            }
            dbHelper.BeginTran();
            currentTransStatus = TranStatus.已开始;
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public override void RollbackTran()
        {
            if (currentTransStatus != TranStatus.已开始)
            {
                throw new Exception("事务回滚失败,没有需要回滚的事务");
            }
            dbHelper.RollbackTran();
            currentTransStatus = TranStatus.未开始;
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public override void CommitTran()
        {
            if (currentTransStatus != TranStatus.已开始)
            {
                throw new Exception("事务提交失败,没有需要提交的事务");
            }
            dbHelper.CommitTran();
            currentTransStatus = TranStatus.未开始;
        }
        #endregion

        #region 检查表是否被创建
        internal void CheckTableCreated<T>() where T : IModel, new()
        {
            CheckTableCreated(typeof(T));
        }
        //static Dictionary<string, List<string>> tableCache = new Dictionary<string, List<string>>();
        /// <summary>
        /// 检查表是否被创建
        /// </summary>
        internal override void CheckTableCreated(Type type)
        {
            if (!SettingConfig.CheckModelTableMaping)
            {
                return;
            }
            //TypeCache.SetDBAdapterCache(type, _DBAdapter);
            var dbName = DatabaseName;
            var cacheInstance =CRL.ExistsTableCache.ExistsTableCache.Instance;
            var table = TypeCache.GetTable(type);
            AbsDBExtend db;
            if (!cacheInstance.DataBase.ContainsKey(dbName))
            {
                db = GetBackgroundDBExtend();
                #region 初始表
                lock (lockObj)
                {
                    //BackupParams();
                    string sql = _DBAdapter.GetAllTablesSql();
                    var dic = db.ExecDictionary<string, int>(sql);
                    //RecoveryParams();
                    cacheInstance.InitTable(dbName, dic.Keys.ToList());
                }
                #endregion
            }
            var tableName = TypeCache.GetTableName(table.TableName, dbContext);
            var tb = cacheInstance.GetTable(dbName, tableName);
            if (tb == null)//没有创建表
            {
                db = GetBackgroundDBExtend();
                #region 创建表
                //BackupParams();
                var obj = System.Activator.CreateInstance(type) as IModel;
                var initDatas = obj.GetInitData();
                if (initDatas != null)
                {
                    foreach (IModel item in initDatas)
                    {
                        CheckData(item);
                    }
                }
                string msg;
                obj.CreateTable(db, out msg);
                //RecoveryParams();
                //if (!a)
                //{
                //    return;
                //    throw new Exception(msg);
                //}
                cacheInstance.SaveTable(dbName, table, tableName);
                if (initDatas != null)
                {
                    _DBAdapter.BatchInsert(initDatas, false);
                }
                return;
                #endregion
            }
            if (tb.ColumnChecked)
            {
                return;
            }
            //判断字段是否一致
            var needCreates = CRL.ExistsTableCache.ExistsTableCache.Instance.CheckFieldExists(dbName, table, tableName);
            if (needCreates.Count > 0)
            {
                db = GetBackgroundDBExtend();
                #region 创建列
                //BackupParams();
                foreach (var item in needCreates)
                {
                    IModel.SetColumnDbType(_DBAdapter, item);
                    string str = IModel.CreateColumn(db, item);
                }
                //RecoveryParams();
                #endregion
            }
           //二次检查,对照表结构
            if (!tb.ColumnChecked2)
            {
                var db2 = copyDBExtend();
                ExistsTableCache.ColumnBackgroundCheck.Add(db2, type);
                tb.ColumnChecked2 = true;
            }
        }
        #endregion


    }
}
