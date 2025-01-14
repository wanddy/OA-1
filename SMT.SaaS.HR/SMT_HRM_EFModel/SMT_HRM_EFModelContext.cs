﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using SMT.Foundation.Core;
using System.Data.Objects.DataClasses;
using System.Threading;
using SMT.Foundation.Log;
using System.Reflection;
using System.Data.Objects;
using System.Data;

namespace SMT_HRM_EFModel
{
    public partial class SMT_HRM_EFModelContext :  IDAL
    {
        #region IDAL 增删改成员
        public SMT_HRM_EFModelContext lbc
        {
            get
            {
                return this;
            }
        }

        int IDAL.Add(object obj)
        {
            int i = 0;
            try
            {
                lbc.AddObject(obj.GetType().Name, obj);
                i= lbc.SaveChanges();
            }
            catch (System.Exception ex)
            {
                lbc.DeleteObject(obj);
                Tracer.Debug(DateTime.Now.ToShortTimeString() + " Add Err:" + ex.InnerException.Message);
                throw ex;
            }
            return i;
        }

        int IDAL.Update(object obj)
        {
            int i = 0;
            try
            {
                i = lbc.SaveChanges();
            }
            catch (Exception ex)
            {
                Tracer.Debug(DateTime.Now.ToShortTimeString() + " Update Err:" + ex.InnerException.Message);
                lbc.Refresh(RefreshMode.StoreWins, obj);
                throw ex;
            }
            return i;
        }

        int IDAL.Delete(object obj)
        {
            EntityObject entity = (EntityObject)obj;
            int i = 0;
            try
            {
                lbc.TryGetObjectByKey(entity.EntityKey, out obj);
                lbc.DeleteObject(obj);
                i = lbc.SaveChanges();
            }
            catch (Exception ex)
            {
                Tracer.Debug(DateTime.Now.ToShortTimeString() + " Delete Err:" + ex.InnerException.Message);
                lbc.Refresh(RefreshMode.StoreWins, obj);
                throw ex;
            }
            return i;
        }

        IQueryable<TEntity> IDAL.GetTable<TEntity>()
        {
            ObjectQuery<TEntity> objs = lbc.CreateQuery<TEntity>("[" + typeof(TEntity).Name + "]");
            objs.MergeOption = MergeOption.NoTracking;
            return objs;
        }

        ObjectQuery<TEntity> IDAL.GetObjects<TEntity>()
        {
            ObjectQuery<TEntity> objs = lbc.CreateQuery<TEntity>("[" + typeof(TEntity).Name + "]");
            objs.MergeOption = MergeOption.NoTracking;
            return objs;
        }

        public object GetObjectByEntityKey(EntityKey entityKey)
        {
            object obj = null;
            try
            {
                lbc.TryGetObjectByKey(entityKey, out obj);
                return obj;
            }
            catch (Exception ex)
            {
                Tracer.Debug(DateTime.Now.ToShortTimeString() + " GetObjectByEntityKey Err:" + ex.InnerException.Message);               
                throw;
            }
        }

        int IDAL.SaveChanges()
        {
            return this.SaveChanges();
        }


        ObjectContext IDAL.GetDataContext()
        {
            return lbc;
        }

        #endregion

        #region 事务处理IDAL 成员

        private System.Data.Common.DbTransaction tran = null;

        /// <summary>
        /// 开始事务处理
        /// </summary>
        void IDAL.BeginTransaction()
        {
            ObjectContext edm = this;
            if (edm.Connection.State == System.Data.ConnectionState.Closed)
            {
                edm.Connection.Open();
            }
            tran = edm.Connection.BeginTransaction();
        }
        /// <summary>
        /// 确认事务处理
        /// </summary>
        void IDAL.CommitTransaction()
        {
            tran.Commit();
        }
        /// <summary>
        /// 回滚事务处理
        /// </summary>
        void IDAL.RollbackTransaction()
        {
            tran.Rollback();
        }
        #endregion

        #region  "特殊方法"
        /// <summary>
        /// 手动更新实体属性
        /// </summary>
        /// <param name="strEntityName">实体名称,需要带DataCotext前缀</param>
        /// <param name="EntityKeyName">实体主键</param>
        /// <param name="EntityKeyValue">实体主键值</param>
        /// <param name="CheckState">需要更新的实体属性和值的集合</param>
        public void UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, Dictionary<object, object> Prameters)
        {

            try
            {
                System.Data.EntityKey entityKey = new System.Data.EntityKey(strEntityName, EntityKeyName, EntityKeyValue);
                object obj = null;
                //var q=from a
                lbc.TryGetObjectByKey(entityKey, out obj);
                if (obj != null)
                {
                    Type a = obj.GetType();

                    PropertyInfo[] infos = a.GetProperties();
                    foreach (PropertyInfo prop in infos)
                    {
                        if (Prameters.ContainsKey(prop.Name))
                        {
                            prop.SetValue(obj, Prameters[prop.Name], null);
                        }
                    }
                    // context.Update(entity);
                    lbc.ApplyPropertyChanges(strEntityName, obj);
                    lbc.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.Message);
                throw (ex);
            }

        }
        #endregion
    }
}
