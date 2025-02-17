﻿using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
using SMT.SaaS.OA.DAL.Views;
//using SMT.SaaS.OA.Services.FlowService;
using SMT.SaaS.BLLCommonServices.FlowWFService;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {
        #region 事项审批增删改查
        /// <summary>
        /// 获取事项审批信息
        /// </summary>
        /// <param name="id">事项审批ID</param>
        /// <returns>返回事项审批实体</returns>
        [OperationContract]
        public T_OA_APPROVALINFO Get_Apporval(string id)
        {
            ApprovalManagementBll approvalBll = new ApprovalManagementBll();
            //using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
            //{
            return approvalBll.Get_Apporval(id);
            //}
        }
        /// <summary>
        /// 获取事项审批列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="flagState"></param>
        /// <param name="loginUserInfo"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_OA_APPROVALINFO> GetApporvalList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string flagState, LoginUserInfo loginUserInfo)//0待审核  1已审核
        {
            ApprovalManagementBll approvalBll = new ApprovalManagementBll();
            //using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
            //{
            if (flagState != "4")//草稿,审核完成(已过,未过)   建立人操作
            {
                IQueryable<T_OA_APPROVALINFO> approvalList = approvalBll.GetApprovalList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, null, flagState);
                if (approvalList == null)
                {
                    return null;
                }
                else
                {
                    return approvalList.ToList();
                }
            }
            else//审批人
            {
                ServiceClient workFlowWS = new ServiceClient();
                string isView = "1";
                if (flagState == "4")
                {
                    isView = "0";
                }
                FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "1", "0", "T_OA_APPROVALINFO", "", loginUserInfo.userID);
                if (flowList == null)
                {
                    return null;
                }
                List<string> guidStringList = new List<string>();
                foreach (FLOW_FLOWRECORDDETAIL_T f in flowList)
                {
                    guidStringList.Add(f.FLOW_FLOWRECORDMASTER_T.FORMID);
                }
                if (guidStringList.Count < 1)
                {
                    return null;
                }
                IQueryable<T_OA_APPROVALINFO> approList = approvalBll.GetApprovalList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, guidStringList, flagState);
                if (approList == null)
                {
                    return null;
                }
                else
                {
                    return approList.ToList();
                }
            }
            //}
        }


        [OperationContract]
        public List<T_OA_APPROVALINFO> GetApporvalListforMVC(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string flagState, LoginUserInfo loginUserInfo,ref  int recordTotals)//0待审核  1已审核
        {
            ApprovalManagementBll approvalBll = new ApprovalManagementBll();
            //using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
            //{
            if (flagState != "4")//草稿,审核完成(已过,未过)   建立人操作
            {
                IQueryable<T_OA_APPROVALINFO> approvalList = approvalBll.GetApprovalListForMVC(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, null, flagState, ref recordTotals);
                if (approvalList == null)
                {
                    return null;
                }
                else
                {
                    return approvalList.ToList();
                }
            }
            else//审批人
            {
                ServiceClient workFlowWS = new ServiceClient();
                string isView = "1";
                if (flagState == "4")
                {
                    isView = "0";
                }
                FLOW_FLOWRECORDDETAIL_T[] flowList = workFlowWS.GetFlowInfo("", "", "1", "0", "T_OA_APPROVALINFO", "", loginUserInfo.userID);
                if (flowList == null)
                {
                    return null;
                }
                List<string> guidStringList = new List<string>();
                foreach (FLOW_FLOWRECORDDETAIL_T f in flowList)
                {
                    guidStringList.Add(f.FLOW_FLOWRECORDMASTER_T.FORMID);
                }
                if (guidStringList.Count < 1)
                {
                    return null;
                }
                IQueryable<T_OA_APPROVALINFO> approList = approvalBll.GetApprovalList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID, guidStringList, flagState);
                if (approList == null)
                {
                    return null;
                }
                else
                {
                    return approList.ToList();
                }
            }
            //}
        }


        /// <summary>
        /// 费用报销查看个人事项审批单数据
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页数量</param>        
        /// <param name="filterString">字符串</param>
        /// <param name="paras">参数</param>
        /// <param name="pageCount">页数</param>
        /// <param name="userID">用户ID</param>
        /// <param name="approvalCode">事项编号</param>        
        /// <returns></returns>
        //public List<T_OA_APPROVALINFO> GetApporvalListforMVCForReimbursement(int pageIndex, int pageSize, string filterString, object[] paras, ref int pageCount, string userID, string approvalCode)
        //{            
        //    using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
        //    {            
        //        var ents = approvalBll.GetApporvalListforMVCForReimbursement(pageIndex,pageSize,  filterString, paras, ref pageCount, userID, approvalCode, "2");
        //        if (ents == null)
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            return ents.ToList();
        //        }

        //    }
        //}
        /// <summary>
        /// 添加事项审批类型
        /// </summary>
        /// <param name="approvakInfo">实体</param>
        /// <param name="ApprovalCode">返回事项审批编号</param>
        /// <returns></returns>
        [OperationContract]
        public int AddApporval(T_OA_APPROVALINFO approvakInfo,ref string ApprovalCode)
        {
            ApprovalManagementBll approvalBll = new ApprovalManagementBll();
            //using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
            //{
            bool sucess = approvalBll.AddApproval(approvakInfo,ref ApprovalCode);
            if (sucess == false)
                return -1;
            return 1;
            //}
        }

        [OperationContract]
        public bool DeleteApporval(T_OA_APPROVALINFO approvakInfo)
        {
            ApprovalManagementBll approvalBll = new ApprovalManagementBll();
            //using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
            //{
            return approvalBll.DeleteApproval(approvakInfo);
            //}
        }
        /// <summary>
        /// 更新事项审批实体
        /// </summary>
        /// <param name="approvakInfo">需要更新的实现审批实体</param>
        /// <returns>成功返回 1 ，失败为-1</returns>
        [OperationContract]
        public int UpdateApporval(T_OA_APPROVALINFO approvakInfo)
        {
            ApprovalManagementBll approvalBll = new ApprovalManagementBll();
            //using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
            //{
            if (approvalBll.UpdateApproval(approvakInfo) != -1)
                return 1;
            else
                return -1;
            //}
        }
        [OperationContract]
        public int DeleteApporvalList(List<T_OA_APPROVALINFO> approvakInfo)
        {
            ApprovalManagementBll approvalBll = new ApprovalManagementBll();
            //using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
            //{
            foreach (T_OA_APPROVALINFO obj in approvakInfo)
            {
                if (!approvalBll.DeleteApproval(obj))
                {
                    return -1;
                }
            }
            return 1;
            //}
        }

        #region 事项审批自定义流程设置
        /// <summary>
        /// 添加自定义流程信息
        /// 2014-2-17 add
        /// </summary>
        /// <param name="listFlowUsers"></param>
        /// <param name="formID"></param>
        /// <returns></returns>
        //[OperationContract]
        //public bool BatchAddApprovalInfoFLowUsers(List<T_OA_APPROVALFLOWUSERS> listFlowUsers, string formID)
        //{
        //    using (ApprovalFlowUserBll approvalFlowUser = new ApprovalFlowUserBll())
        //    {
        //        return approvalFlowUser.BatchAddFlowUsers(listFlowUsers,formID);
        //    }
        //}
        /// <summary>
        /// 删除单条自定义流程记录
        /// </summary>
        /// <param name="approvalFlowUserID"></param>
        /// <returns></returns>
        //[OperationContract]
        //public bool DeleteApprovalFlowUser(string approvalFlowUserID)
        //{
        //    using (ApprovalFlowUserBll approvalFlowUser = new ApprovalFlowUserBll())
        //    {
        //        return approvalFlowUser.DeleteApprovalFlowUser(approvalFlowUserID);
        //    }
        //}
        /// <summary>
        /// 删除某条事项审批的自定义流程记录
        /// </summary>
        /// <param name="formID"></param>
        /// <returns></returns>
        //[OperationContract]
        //public bool DeleteAllApprovalFlowUserByFormID(string formID)
        //{
        //    using (ApprovalFlowUserBll approvalFlowUser = new ApprovalFlowUserBll())
        //    {
        //        return approvalFlowUser.DeleteAllApprovalFlowUserByFormID(formID);
        //    }
        //}

        #endregion
        #endregion

        #region 事项审批类型设置
        ///// <summary>
        ///// 根据id, 删除附件记录 
        ///// </summary>
        ///// <param name="ids"></param>
        ///// <returns></returns>
        //[OperationContract]
        //public int DelFiles(string[] ids)
        //{
        //    ApprovalManagementBll approvalBll = new ApprovalManagementBll();
        //    //using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
        //    //{
        //    return approvalBll.DelFiles(ids);
        //    //}
        //}

        /// <summary>
        /// 添加事项审批设置
        /// </summary>
        /// <param name="listset"></param>
        /// <param name="lstcompany"></param>
        /// <param name="lstdepartment"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddApprovalSet(List<T_OA_APPROVALTYPESET> listset, List<string> lstcompany, List<string> lstdepartment)
        { 
            using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
            {
                return approvalBll.AddApprovalSet(listset,lstcompany,lstdepartment);
            }
        }
        [OperationContract]
        public List<T_OA_APPROVALTYPESET> GetApprovalSetByOrgType(string orgid, string orgtype)
        {
            using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
            {
                //if (orgtype == "Company")
                //{
                //    return approvalBll.GetApprovalSetByOrgType(orgid, "0");
                //}
                //else
                //{
                //    return approvalBll.GetApprovalSetByOrgType(orgid, "1");
                //}
                return approvalBll.GetApprovalSetByOrgType(orgid, orgtype);
            }
        }
        /// <summary>
        /// 根据公司ID 和部门ID获取  事项审批的类型
        /// </summary>
        /// <param name="companyid">公司ID</param>
        /// <param name="departmentid">部门ID</param>
        /// <returns></returns>
        [OperationContract]
        public List<string> GetApprovalTypeByCompanyandDepartmentid(string companyid, string departmentid)
        {
            using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
            {
                return approvalBll.GetApprovalTypeByCompanyandDepartmentid(companyid,departmentid);
            }
        }

        /// <summary>
        /// add by ljx 2013-12-3
        /// 获取子公司的事项审批类型
        /// </summary>
        /// <param name="companyid">公司ID</param>
        /// <returns>返回事项审批类型ID集合</returns>
        [OperationContract]
        public List<string> GetApprovalTypeByCompanyid(string companyid)
        {
            using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
            {
                return approvalBll.GetApprovalTypeByCompanyid(companyid);
            }
        }

        /// <summary>
        /// 通过公司名称获取公司下的部门 -by luojie
        /// </summary>
        /// <param name="companies">字符串形式的公司名称，以‘，’作为间隔</param>
        /// <returns>V_DepartmentWinthCompany列表,有部门id，公司id，公司名字及其简称</returns>
        [OperationContract]
        public List<V_DepartmentWithCompany> GetApprovalTypesByCompanyIDs(string companies)
        {
            using (ApprovalManagementBll approvalBll = new ApprovalManagementBll())
            {
                return approvalBll.GetApprovalTypesByCompanyIDs(companies);
            }
        }
        #endregion

       
    }
}