﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.OA.UI.EngineDataSource;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MaintenanceAppForm_aud : BaseForm,IClient, IEntityEditor, IAudit
    {
        #region 全局变量
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        private FormTypes actions;

        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        private T_OA_MAINTENANCEAPP conserVation = null;
        public T_OA_MAINTENANCEAPP ConserVation { get { return conserVation; } set { conserVation = value; } }
        private string mappId = string.Empty;
        #endregion

        #region 构造函数
        public MaintenanceAppForm_aud(FormTypes actions)
        {
            InitializeComponent();
            this.actions = actions;
            _VM.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(vehicleInfoManager_GetVehicleInfoListCompleted);
            _VM.UpdateMaintenanceAppCompleted += new EventHandler<UpdateMaintenanceAppCompletedEventArgs>(cvmWS_UpdateMaintenanceAppCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
        }
        public MaintenanceAppForm_aud(FormTypes operationType, string SendDocID)
        {
            InitializeComponent();
            this.actions = operationType;
            this.mappId = SendDocID;
            _VM.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(vehicleInfoManager_GetVehicleInfoListCompleted);
            _VM.UpdateMaintenanceAppCompleted += new EventHandler<UpdateMaintenanceAppCompletedEventArgs>(cvmWS_UpdateMaintenanceAppCompleted);
            _VM.Get_VMAppCompleted += new EventHandler<Get_VMAppCompletedEventArgs>(Get_VMAppCompleted);
            
            Load_Data();
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Load_Data
        private void Load_Data()
        {
            _VM.GetVehicleInfoListAsync();
            _VM.Get_VMAppAsync(mappId);
            Utility.BindComboBox(cmbRepairName, "MAINTENANCENAME", 0);

            if (actions == FormTypes.Browse || actions == FormTypes.Audit)
            {
                this.dateREPAIRDATE.IsEnabled = false;
                this.dateRETRIEVEDATE.IsEnabled = false;
                this.txtREPAIRCOMPANY.IsEnabled = false;
                this.txtTel.IsEnabled = false;
                this.txtContent.IsEnabled = false;
                this.txtReMark.IsEnabled = false;
                this.ckbHasFee.IsEnabled = false;
            }
        }
        #endregion

        #region SetForms
        private void SetForms()
        {
            txtFee.IsReadOnly = true;
            cmbRepairName.IsEnabled = false;
            cmbVehicleAssetId.IsEnabled = false;
            //InitAudit(conserVation.MAINTENANCEAPPID);
            InitFBControl();
        }
        #endregion

        #region 费用控件 审核界面用
        private void InitFBControl()
        {
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.BorrowApply;//借款选择
            fbCtr.strExtOrderModelCode = "JGZC";

            fbCtr.Order.ORDERID = conserVation.MAINTENANCEAPPID;//费用对象

            fbCtr.Order.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            fbCtr.Order.CREATECOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            fbCtr.Order.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            fbCtr.Order.CREATEDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            fbCtr.Order.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            fbCtr.Order.CREATEPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            fbCtr.Order.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            fbCtr.Order.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            fbCtr.Order.OWNERCOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            fbCtr.Order.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            fbCtr.Order.OWNERDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            fbCtr.Order.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            fbCtr.Order.OWNERPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            fbCtr.Order.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;


            fbCtr.Order.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            fbCtr.InitDataComplete += (o, e) =>
            {
                Binding bding = new Binding();
                bding.Path = new PropertyPath("TOTALMONEY");
                this.txtFee.SetBinding(TextBox.TextProperty, bding);
                this.txtFee.DataContext = fbCtr.Order;
            };
            fbCtr.InitData(false);
        }
        #endregion

        #region 修改
        void cvmWS_UpdateMaintenanceAppCompleted(object sender, UpdateMaintenanceAppCompletedEventArgs e)
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
                if (e.Result > 0)
                {
                    if (ckbHasFee.IsChecked == true && Convert.ToInt32(txtFee.Text) > 0)
                    {
                        fbCtr.Order.ORDERID = conserVation.MAINTENANCEAPPID;
                        fbCtr.Save(_flwResult);//提交费用
                    }
                    else
                    {
                        Utility.ShowMessageBox("SUCCESSAUDIT", true, true);//审核成功
                        RefreshUI(RefreshedTypes.CloseAndReloadData);
                    }
                }
                else
                {
                    Utility.ShowMessageBox("FAILURETOAPPROVE", true, false);//审核失败
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        #endregion

        #region 费用保存完成
        void fbCtr_SaveCompleted(object sender, SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs e)
        {
            if (e.Message != null && e.Message.Count() > 0)
            {
                Utility.ShowMessageBox("UPDATE", true, false);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                Utility.ShowMessageBox("UPDATE", true, true);
                if (true)
                    saveType = RefreshedTypes.CloseAndReloadData;
                RefreshUI(saveType);
            }
        }
        #endregion

        #region 获取车辆信息
        void vehicleInfoManager_GetVehicleInfoListCompleted(object sender, GetVehicleInfoListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> vehicleInfoList = e.Result;
            if (vehicleInfoList != null)
            {
                if (conserVation == null)
                    SetComboBoxSelect(vehicleInfoList, null);
                else
                {
                    dateREPAIRDATE.Text = Convert.ToDateTime(conserVation.REPAIRDATE).ToShortDateString();
                    dateRETRIEVEDATE.Text = Convert.ToDateTime(conserVation.RETRIEVEDATE).ToShortDateString();

                    txtREPAIRCOMPANY.Text = conserVation.REPAIRCOMPANY;
                    txtTel.Text = conserVation.TEL;
                    txtContent.Text = conserVation.CONTENT;
                    txtReMark.Text = conserVation.REMARK == null ? "" : conserVation.REMARK;

                    ckbHasFee.IsChecked = conserVation.ISCHARGE == "1" ? true : false;
                    txtFee.Text = conserVation.CHARGEMONEY.ToString();
                    if (conserVation.ISCHARGE == "1")
                        txtFee.Visibility = Visibility.Visible;

                    if (conserVation.T_OA_VEHICLEReference.EntityKey != null)
                    {
                        SetComboBoxSelect(vehicleInfoList
                               , conserVation.T_OA_VEHICLEReference.EntityKey.EntityKeyValues[0].ToString());
                    }
                    //SetComboBoxSelect(vehicleInfoList, (new System.Collections.Generic.List<SMT.SaaS.OA.UI.SmtOACommonAdminService.EntityKeyMember>(((SMT.SaaS.OA.UI.SmtOACommonAdminService.EntityReference)(conserVation.T_OA_VEHICLEReference)).EntityKey.EntityKeyValues))[0].Value.ToString());
                }
            }
        }
        #endregion

        #region 根据ID获取维修申请单
        void Get_VMAppCompleted(object sender, Get_VMAppCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                conserVation = e.Result;

                txtFee.Text = conserVation.CHARGEMONEY.ToString();
                Utility.SetComboboxSelectByText(cmbRepairName, conserVation.MAINTENANCETYPE, -1);
                SetForms();
                ckbHasFee.IsChecked = conserVation.ISCHARGE == "1" ? true : false;

                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
                //viewApproval.XmlObject = DataObjectToXml<T_OA_MAINTENANCEAPP>.ObjListToXml(conserVation, "OA");
            }
        }
        #endregion

        #region 修改保存
        private void UpdateInfo()
        {
            conserVation.UPDATEDATE = System.DateTime.Now;
            conserVation.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            _VM.UpdateMaintenanceAppAsync(conserVation);
        }
        #endregion

        #region 获取车牌号
        private void SetComboBoxSelect(ObservableCollection<T_OA_VEHICLE> cmbData, string assetId)
        {
            cmbVehicleAssetId.Items.Clear();
            T_OA_VEHICLE selectObj = null;
            foreach (T_OA_VEHICLE obj in cmbData)
            {
                cmbVehicleAssetId.Items.Add(obj);
                if (obj.ASSETID == assetId)
                {
                    selectObj = obj;
                }
            }
            cmbVehicleAssetId.DisplayMemberPath = "VIN";
            if (selectObj != null)
            {
                cmbVehicleAssetId.SelectedItem = selectObj;
            }
            else
            {
                cmbVehicleAssetId.SelectedIndex = 0;
            }
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("MAINTENANCEAPP");
        }

        public string GetStatus()
        {
            return "";
        }
        private RefreshedTypes saveType;
        public void DoAction(string actionType)
        {

        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("InfoDetail"),
                Tooltip = Utility.GetResourceStr("InfoDetail")
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        #endregion

        #region 确定、取消
        private void Save()
        {
            UpdateInfo();
        }

        private void CancelAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        #region StarEngine
        public int StarEngine(object oldDataObject, object newDataObject, string nextUserID, string nFlowNode, string oFlowNode)
        {
            string xmlString = null;
            string errorString = null;
            if (GlobalFunction.ObjListToXml<SMT.SaaS.OA.UI.SmtOACommonAdminService.EntityKeyMember>(oldDataObject, newDataObject, ref xmlString, ref errorString, nextUserID, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, nFlowNode, oFlowNode) == -1)
            {
                return -1;//生成失败
            }
            EngineWcfGlobalFunctionClient wcfclient = new EngineWcfGlobalFunctionClient();
            wcfclient.SaveTriggerDataAsync(xmlString);
            wcfclient.SaveTriggerDataCompleted += new EventHandler<SaveTriggerDataCompletedEventArgs>(wcfclient_SaveTriggerDataCompleted);
            return 1;
        }
        #endregion

        #region wcfclient_SaveTriggerDataCompleted
        void wcfclient_SaveTriggerDataCompleted(object sender, SaveTriggerDataCompletedEventArgs e)
        {
            //if (e. == 1)
            //{
            //    MessageBox.Show("成功!");
            //}
        }
        #endregion

        #region CheckBox_Click
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true)
            {
                txbFee.Visibility = Visibility.Visible;
                txtFee.Visibility = Visibility.Visible;
                scvFB.Visibility = Visibility.Visible;
            }
            else
            {
                txbFee.Visibility = Visibility.Collapsed;
                txtFee.Visibility = Visibility.Collapsed;
                scvFB.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("BorrowMoney", fbCtr.GetParamenter().BorrowMoney.ToString());
            strXmlObjectSource = Utility.ObjListToXmlForTravel<T_OA_MAINTENANCEAPP>(conserVation, "OA", parameters);
            Utility.SetAuditEntity(entity, "T_OA_MAINTENANCEAPP", conserVation.MAINTENANCEAPPID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            conserVation.CHECKSTATE = state;
            Save();
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (conserVation != null)
                state = conserVation.CHECKSTATE;
            if (actions == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            _VM.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}