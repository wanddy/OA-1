﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_FB_EFModel;
using System.Data.Objects.DataClasses;
using System.Threading;
using SMT.FBEntityBLL.BLL;
using System.Configuration;
using SMT.Foundation.Log;

namespace SMT.FB.BLL
{
    public class SystemBLL
    {
        public const string DEFAULTORDERCODE = "<自动生成>";
        //static FBEntityBLL bll = null;
        //static FBEntityBLL bllOrder = null;
        static List<T_FB_ORDERCODE> listOrderCode;
        static T_FB_SYSTEMSETTINGS etityT_FB_SYSTEMSETTINGS = null; // 不可直接调用，可访问GetSetting()方式获取
        
        
        static SystemBLL()
        {
            //bll = new FBEntityBLL(true);
            //bllOrder = new FBEntityBLL(true);
            try
            {
                if (ConfigurationManager.AppSettings["DebugMode"].ToString() == "true")
                {
                    DebugMode = true;
                }
                else
                {
                    DebugMode = false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("ConfigurationManager 找不到DebugMode");
            }
           
            InitOrderBLL();
        }

        #region 1.	系统参数类操作方法
        public static T_FB_SYSTEMSETTINGS GetSetting(QueryExpression qe)
        {
            try
            {
                if (etityT_FB_SYSTEMSETTINGS == null)
                {
                    using (FBEntityBLL bll = new FBEntityBLL())
                    {
                        etityT_FB_SYSTEMSETTINGS = bll.GetTable<T_FB_SYSTEMSETTINGS>().FirstOrDefault();
                    }
                }
                etityT_FB_SYSTEMSETTINGS.Settings = Settings;

                return etityT_FB_SYSTEMSETTINGS;
            }
            catch (Exception ex)
            {
                Tracer.Debug("获取预算初始化信息出错，出错信息：" + ex.ToString());
                throw ex;
            }
        }

        public static string GetFBSetting(string key)
        {
            var result = "";
            if (Settings.ContainsKey(key))
            {
                result = Settings[key];
            }
            return result;

        }
        public static void SaveSetting()
        {
            using (FBEntityBLL bll = new FBEntityBLL())
            {
                bll.BassBllSave(etityT_FB_SYSTEMSETTINGS, FBEntityState.Modified);
            }
        }
        #endregion 
        
        #region 2.	自动编号生成操作方法
        public static void InitOrderBLL()
        {
            using (FBEntityBLL bll = new FBEntityBLL())
            {
                listOrderCode = bll.GetTable<T_FB_ORDERCODE>().ToList();
            }
        }

        public static object lockObject = new object();
        
        public static string GetOrderCode(T_FB_ORDERCODE orderCode)
        {
            lock (lockObject)
            {
                DateTime CurrentDate = orderCode.CURRENTDATE.Value;
                if (CurrentDate.Date != System.DateTime.Now.Date)
                {
                    orderCode.CURRENTDATE = System.DateTime.Now.Date;
                    orderCode.RUNNINGNUMBER = 1;

                }

                string shortName = orderCode.PRENAME;
                decimal curNumber = orderCode.RUNNINGNUMBER.Value;
                string strDate = orderCode.CURRENTDATE.Value.ToString("yyyyMMdd");
                string code = shortName + "_" + strDate + curNumber.ToString().PadLeft(6, '0');

                orderCode.RUNNINGNUMBER = curNumber + 1;
                using (FBEntityBLL bll = new FBEntityBLL())
                {
                    bll.BassBllSave(orderCode, FBEntityState.Modified);
                }
                return code;
            }

        }

        public static void AddAutoOrderCode(EntityObject entity)
        {

            string tablename = GetTableName(entity);

            T_FB_ORDERCODE orderCode = listOrderCode.FirstOrDefault(item =>
            {
                return item.TABLENAME == tablename;
            });
            if (orderCode == null)
            {
                Tracer.Debug("生成编号失败：在T_FB_ORDERCODE表中未找到此表配置项目：" + tablename);
                return;
            }

            //string code = GetOrderCode(orderCode);张秉福旧方法
            //新方法
            string code = string.Empty;
            using (OrderCodeBLL bll = new OrderCodeBLL())
            {
                code = bll.GetAutoOrderCode(tablename);
            } 
            string codePropertyName = orderCode.FIELDNAME;
            entity.SetValue(codePropertyName, code);
            
            //开始更新元数据单号
            string Formid = string.Empty;
            Tracer.Debug("开始更新元数据："+tablename);
            try
            {
                  switch (tablename)
                    {

                        case "T_FB_TRAVELEXPAPPLYMASTER":
                            //T_FB_TRAVELEXPAPPLYMASTER item = (T_FB_TRAVELEXPAPPLYMASTER)entity;
                            //Formid = item.T_FB_EXTENSIONALORDER.ORDERID;
                            //出差报销在OA中出差报销审核的业务逻辑中处理
                            break;
                        default:
                            Formid =entity.EntityKey.EntityKeyValues[0].Value.ToString();

                            SMT.SaaS.BLLCommonServices.FlowWFService.ServiceClient client =
                                new SaaS.BLLCommonServices.FlowWFService.ServiceClient();
                            Tracer.Debug("开始调用元数据获取接口：FlowWFService.GetMetadataByFormid("+Formid+")");
                            string xml=string.Empty;
                            xml=client.GetMetadataByFormid(Formid);
                            if (string.IsNullOrEmpty(xml))
                            {
                                Tracer.Debug("XML元数据为空，跳过:" + xml);
                                break;
                            }
                            Tracer.Debug("获取到的元数据："+xml);
                            xml=xml.Replace("自动生成", code);
                            Tracer.Debug("替换单号后的XML:"+xml);
                            client.UpdateMetadataByFormid(Formid, xml);
                            break;
                    }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
            }
        }

        public static string GetTableName(EntityObject entity)
        {
            string tableName = entity.GetType().Name;
            //T_FB_CHARGEAPPLYMASTER entityCharge = entity as T_FB_CHARGEAPPLYMASTER;
            //if (entityCharge != null)
            //{
            //    entityCharge.T_FB_EXTENSIONALORDERReference.Load();
            //    entityCharge.T_FB_EXTENSIONALORDER.T_FB_EXTENSIONALTYPEReference.Load();
            //    if (entityCharge.T_FB_EXTENSIONALORDER.T_FB_EXTENSIONALTYPE.EXTENSIONALTYPECODE == "CCBX")
            //    {
            //        tableName = typeof(T_FB_TRAVELEXPAPPLYMASTER).Name;
            //    }
            //}
            return tableName;
            
        }
        #endregion
        
        #region 3.	是否月结标记
        /// <summary>
        /// 当前月是否已做过月结
        /// </summary>
        public static bool IsChecked
        {
            get
            {
                #region 是否本月有结算
                T_FB_SYSTEMSETTINGS systemSetting = SystemBLL.GetSetting(null);

                var isChecked = false;
                if (systemSetting.LASTCHECKDATE != null)
                {
                    var checkDate = systemSetting.LASTCHECKDATE.Value;
                    var nowDate = System.DateTime.Now.Date;
                    if (checkDate.Year == nowDate.Year && checkDate.Month == nowDate.Month)
                    {
                        isChecked = true;
                    }
                }
                return isChecked;
                #endregion
            }
        }
        #endregion


        #region 4. Setting 预算的默认设置
        private static Dictionary<string, string> static_Settings;
        public static Dictionary<string, string> Settings
        {
            get
            {
                if (static_Settings == null)
                {
                    static_Settings = new Dictionary<string, string>();
                }
                return static_Settings;
            }

        }
        #endregion

        #region DebugMode
        public static bool DebugMode { get; set; }
        public static int FBDebugLevel { get; set; }
        public static void Debug(string msg)
        {
            try
            {
                if (ConfigurationManager.AppSettings["DebugMode"].ToString() == "true")
                {
                    DebugMode = true;
                }
                else
                {
                    DebugMode = false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("ConfigurationManager 找不到DebugMode");
            }

            if (DebugMode)
            {
                InnerDebug(msg);
            }
        }

        public static void Debug(Func<string> func)
        {
            if (DebugMode)
            {
                try
                {
                    InnerDebug(func());
                }
                catch (Exception ex)
                {
                    InnerDebug("日志出错, 异常: " + ex.ToString());
                }
            }
        }

        private static void InnerDebug(string msg)
        {
            Tracer.Debug(msg);
        }
        #endregion
    }
}
