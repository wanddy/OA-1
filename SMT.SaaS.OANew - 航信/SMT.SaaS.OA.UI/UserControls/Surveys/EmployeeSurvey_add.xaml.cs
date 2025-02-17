﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class EmployeeSurvey_add : BaseForm, IClient, IEntityEditor, IAudit
    {
        /// <summary>
        /// 方案
        /// </summary>
        private V_EmployeeSurvey _survey;
        public V_EmployeeSurvey _Survey { get { return _survey; } set { _survey = value; } }
        /// <summary>
        /// 题目
        /// </summary>
        private ObservableCollection<V_EmployeeSurveySubject> _osub = new ObservableCollection<V_EmployeeSurveySubject>();

        private bool isFlow = false;
        private RefreshedTypes saveType;
        public FormTypes types;
        private SmtOAPersonOfficeClient _VM ;

        public EmployeeSurvey_add(FormTypes type)
        {
            InitializeComponent();
            this.types = type;
            this.Loaded += new RoutedEventHandler(EmployeeSurvey_add_Loaded);
          
        }

        void EmployeeSurvey_add_Loaded(object sender, RoutedEventArgs e)
        {
            _VM = new SmtOAPersonOfficeClient();
            _VM.AddEmployeeSurveyViewCompleted += new EventHandler<AddEmployeeSurveyViewCompletedEventArgs>(employeeSurveyViewClient_AddEmployeeSurveyViewCompleted);
            _VM.Upd_ESurveyCompleted += new EventHandler<Upd_ESurveyCompletedEventArgs>(Upd_ESurveyCompleted);
            _VM.DeleteEmployeeSurveySubjectViewCompleted += new EventHandler<DeleteEmployeeSurveySubjectViewCompletedEventArgs>(DeleteEmployeeSurveySubjectViewCompleted);
            _Survey = new V_EmployeeSurvey();
            _Survey.RequireMaster = new T_OA_REQUIREMASTER();
            _Survey.RequireMaster.REQUIREMASTERID = System.Guid.NewGuid().ToString();
            _survey.RequireMaster.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();

            _Survey.SubjectViewList = new ObservableCollection<V_EmployeeSurveySubject>();

            NewSubject();
        }
       

        #region 题目
        /// <summary>
        /// 增加题目时  增加 初始化答案(复制)
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<T_OA_REQUIREDETAIL> InitAnswers(ref V_EmployeeSurveySubject sub)
        {
            ObservableCollection<T_OA_REQUIREDETAIL> O = new ObservableCollection<T_OA_REQUIREDETAIL>();

            T_OA_REQUIREDETAIL info = new T_OA_REQUIREDETAIL(); info.CODE = "A"; info.CONTENT = txtA.Text.Trim() == "" ? "无" : txtA.Text.Trim(); O.Add(info);
            T_OA_REQUIREDETAIL infoB = new T_OA_REQUIREDETAIL(); infoB.CODE = "B"; infoB.CONTENT = txtB.Text.Trim() == "" ? "无" : txtB.Text.Trim(); O.Add(infoB);
            T_OA_REQUIREDETAIL infoC = new T_OA_REQUIREDETAIL(); infoC.CODE = "C"; infoC.CONTENT = txtC.Text.Trim() == "" ? "无" : txtC.Text.Trim(); O.Add(infoC);
            T_OA_REQUIREDETAIL infoD = new T_OA_REQUIREDETAIL(); infoD.CODE = "D"; infoD.CONTENT = txtD.Text.Trim() == "" ? "无" : txtD.Text.Trim(); O.Add(infoD);
            T_OA_REQUIREDETAIL infoE = new T_OA_REQUIREDETAIL(); infoE.CODE = "E"; infoE.CONTENT = txtE.Text.Trim() == "" ? "无" : txtE.Text.Trim(); O.Add(infoE);
            T_OA_REQUIREDETAIL infoF = new T_OA_REQUIREDETAIL(); infoF.CODE = "F"; infoF.CONTENT = txtF.Text.Trim() == "" ? "无" : txtF.Text.Trim(); O.Add(infoF);
            T_OA_REQUIREDETAIL infoG = new T_OA_REQUIREDETAIL(); infoG.CODE = "G"; infoG.CONTENT = txtG.Text.Trim() == "" ? "无" : txtG.Text.Trim(); O.Add(infoG);
            T_OA_REQUIREDETAIL infoH = new T_OA_REQUIREDETAIL(); infoH.CODE = "H"; infoH.CONTENT = txtH.Text.Trim() == "" ? "无" : txtH.Text.Trim(); O.Add(infoH);

            for (int i = 0; i < O.Count; i++)
            {
                T_OA_REQUIREDETAIL v = O[i];
                v.REQUIREDETAILID = Guid.NewGuid().ToString();
                v.SUBJECTID = sub.SubjectInfo.SUBJECTID;
                v.REQUIREMASTERID = sub.SubjectInfo.REQUIREMASTERID;
                SetAnswer(ref v);
            }
            return O;
        }
        /// <summary>
        /// 添加一个题目,及答案
        /// </summary>
        private void NewSubject()
        {
            V_EmployeeSurveySubject sub = new V_EmployeeSurveySubject();
            sub.SubjectInfo = new T_OA_REQUIREDETAIL2();
            sub.SubjectInfo.T_OA_REQUIREMASTER = _Survey.RequireMaster;

            sub.SubjectInfo.REQUIREDETAIL2ID = Guid.NewGuid().ToString();
            sub.SubjectInfo.REQUIREMASTERID = _Survey.RequireMaster.REQUIREMASTERID;
            sub.IsAdd = true; //添加之后 更新时用
            if (_osub.Count > 0)
            {
                V_EmployeeSurveySubject i = _osub[_osub.Count - 1];
                sub.SubjectInfo.SUBJECTID = i.SubjectInfo.SUBJECTID + 1;
            }
            else
                sub.SubjectInfo.SUBJECTID = 1;// 添加保存后，不能全部删除所有题目。

            SetSubject(ref sub);
            sub.AnswerList = InitAnswers(ref sub);
            _osub.Add(sub);
        }
        //根据 回车键，判断 是否新增行，保存修改行。行加载后， 重新计算行题号  1
        private void txtSub_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (dg.SelectedIndex == _osub.Count - 1)
                {
                    V_EmployeeSurveySubject vsub = _osub.LastOrDefault();
                    if (vsub.SubjectInfo.CONTENT != null && vsub.SubjectInfo.CONTENT.Trim().Length > 0)
                        NewSubject();
                    else
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OAESURVEYSUBJECTNULL"));
                }
        }

        //行加载删除按钮 2
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            V_EmployeeSurveySubject tmp = (V_EmployeeSurveySubject)e.Row.DataContext;
            ImageButton MyButton_Delbaodao = dg.Columns[3].GetCellContent(e.Row).FindName("myDelete") as ImageButton;
            MyButton_Delbaodao.Margin = new Thickness(0);
            MyButton_Delbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png", Utility.GetResourceStr("DELETE"));
            MyButton_Delbaodao.Tag = tmp;
        }

        //题目更改后保存其值
        private void txtSub_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (txt != null && txt.Text.Trim().Length > 0)
                _osub[dg.SelectedIndex].SubjectInfo.CONTENT = txt.Text.ToString();
            else
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OAESURVEYSUBJECTNULL"));
        }

        //删除题目
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_osub.Count > 1) //必须有派车单，司机才能根据派车单提交费用 单
            {
                V_EmployeeSurveySubject i = ((Button)sender).DataContext as V_EmployeeSurveySubject;
                _osub.Remove(i);

                if (!i.IsAdd) //删除已经保存到服务器中的数据
                {
                    ObservableCollection<V_EmployeeSurveySubject> o = new ObservableCollection<V_EmployeeSurveySubject>();
                    o.Add(i);
                    _VM.DeleteEmployeeSurveySubjectViewAsync(o);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("REQUIRED", "SubjectContent"));
                return;
            }
        }
        //题目选中事件
        private void dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg.SelectedIndex > -1)
                GetAnswer(dg.SelectedItem as V_EmployeeSurveySubject);
        }
        //选中题目时，获取答案
        private void GetAnswer(V_EmployeeSurveySubject v)
        {
            if (v.AnswerList != null & v.AnswerList.Count > 0)
                foreach (T_OA_REQUIREDETAIL i in v.AnswerList)
                    switch (i.CODE)
                    {
                        case "A": txtA.Text = i.CONTENT; break;
                        case "B": txtB.Text = i.CONTENT; break;
                        case "C": txtC.Text = i.CONTENT; break;
                        case "D": txtD.Text = i.CONTENT; break;
                        case "E": txtE.Text = i.CONTENT; break;
                        case "F": txtF.Text = i.CONTENT; break;
                        case "G": txtG.Text = i.CONTENT; break;
                        case "H": txtH.Text = i.CONTENT; break;
                    }

        }
        //答案更改后保存在 集合中 
        private void txtAnswer_KeyUp(object sender, KeyEventArgs e)
        {
            //V_EmployeeSurveySubject sub = dg.SelectedItem as V_EmployeeSurveySubject;
            V_EmployeeSurveySubject sub = new V_EmployeeSurveySubject();
            TextBox txt = (TextBox)sender;
            switch (txt.Name)
            {
                case "txtA": sub.AnswerList[0].CONTENT = txtA.Text; break;
                case "txtB": sub.AnswerList[1].CONTENT = txtB.Text; break;
                case "txtC": sub.AnswerList[2].CONTENT = txtC.Text; break;
                case "txtD": sub.AnswerList[3].CONTENT = txtD.Text; break;
                case "txtE": sub.AnswerList[4].CONTENT = txtE.Text; break;
                case "txtF": sub.AnswerList[5].CONTENT = txtF.Text; break;
                case "txtG": sub.AnswerList[6].CONTENT = txtG.Text; break;
                case "txtH": sub.AnswerList[7].CONTENT = txtH.Text; break;
            }
        }

        #endregion 题目

        #region IEntityEditor

        public string GetStatus()
        {
            return "";
        }

        public string GetTitle()
        {
            return Utility.GetResourceStr("ADDTITLE", "EmployeeSurvey");//获取标题
        }
        //工具栏
        public List<ToolbarItem> GetToolBarItems()//配置保存和保存后关闭按钮
        {
            object[,] arr = new object[,]{             
             {ToolbarItemDisplayTypes.Image,"1","SAVEANDCLOSE", "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"},
             {ToolbarItemDisplayTypes.Image,"0","SAVE","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"}
            };
            return VehicleMgt.GetToolBarItems(ref arr);
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
        public void DoAction(string actionType)
        {
            if (!Check()) return;
            RefreshUI(RefreshedTypes.ShowProgressBar);

            switch (actionType)
            {
                case "0":
                    isFlow = false;
                    saveType = RefreshedTypes.HideProgressBar;
                    Save();
                    break;
                case "1":
                    isFlow = false;
                    saveType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;

            }
        }
        public event UIRefreshedHandler OnUIRefreshed;

        #endregion

        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        //检查
        private bool Check()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            foreach (V_EmployeeSurveySubject vsub in _osub)
            {
                if (vsub.SubjectInfo.CONTENT == null || vsub.SubjectInfo.CONTENT.Trim().Length == 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OAESURVEYSUBJECTNULL"));
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 设置 方案其它信息
        /// </summary>
        /// <param name="i"></param>
        private void SetSurvey()
        {
            _Survey.RequireMaster.CREATEDATE = System.DateTime.Now;
            _Survey.RequireMaster.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            _Survey.RequireMaster.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            _Survey.RequireMaster.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            _Survey.RequireMaster.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            _Survey.RequireMaster.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            _Survey.RequireMaster.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            _Survey.RequireMaster.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            _Survey.RequireMaster.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            _Survey.RequireMaster.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            _Survey.RequireMaster.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
        }
        /// <summary>
        /// 设置 题目其它信息
        /// </summary>
        /// <param name="i"></param>
        private static void SetSubject(ref V_EmployeeSurveySubject i)
        {
            i.SubjectInfo.CREATEDATE = System.DateTime.Now;
            i.SubjectInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            i.SubjectInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            i.SubjectInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            i.SubjectInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            i.SubjectInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            i.SubjectInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            i.SubjectInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            i.SubjectInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            i.SubjectInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            i.SubjectInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
        }
        /// <summary>
        /// 设置 答案其它信息
        /// </summary>
        /// <param name="i"></param>
        private static void SetAnswer(ref T_OA_REQUIREDETAIL i)
        {
            i.CREATEDATE = System.DateTime.Now;
            i.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            i.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            i.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            i.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            i.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            i.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            i.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            i.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            i.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            i.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
        }
        bool _isAdd = true;
        private void Save()
        {
            _Survey.RequireMaster.REQUIRETITLE = txtTitle.Text.Trim();
            _Survey.RequireMaster.CONTENT = txtContent.Text.Trim();
            _Survey.RequireMaster.CHECKSTATE = _survey.RequireMaster.CHECKSTATE;
            if (_isAdd)
            {
                SetSurvey();
                for (int i = 0; i < _osub.Count; i++)
                {
                    if (_osub[i].SubjectInfo != null && _osub[i].SubjectInfo.CONTENT != null && _osub[i].SubjectInfo.CONTENT != "无" && _osub[i].SubjectInfo.CONTENT.Trim().Length > 0)
                    {
                        V_EmployeeSurveySubject sub = _osub[i];
                        _Survey.SubjectViewList.Add(sub);   //题目id    
                    }
                }
                _VM.AddEmployeeSurveyViewAsync(_Survey);//新增
            }
            else
            {
                ObservableCollection<V_EmployeeSurveySubject> addLst = new ObservableCollection<V_EmployeeSurveySubject>();
                ObservableCollection<V_EmployeeSurveySubject> updLst = new ObservableCollection<V_EmployeeSurveySubject>();
                foreach (V_EmployeeSurveySubject i in _osub)
                {
                    if (i.IsAdd)
                        addLst.Add(i);
                    else
                        updLst.Add(i);
                }
                //先更新 方案，然后 添加 和修改题目、答案
                _VM.Upd_ESurveyAsync(_survey.RequireMaster, addLst, updLst);//更改数据
            }
        }
        //保存之后
        void employeeSurveyViewClient_AddEmployeeSurveyViewCompleted(object sender, AddEmployeeSurveyViewCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
                if (e.Result > 0)
                {
                    _isAdd = false;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                    Utility.ShowMessageBox("ADD", isFlow, true);
                    if (isFlow)
                        saveType = RefreshedTypes.CloseAndReloadData;
                    foreach (var v in _osub)  //新增的数据保存后再执行删除操作时 判断
                        v.IsAdd = false;
                    RefreshUI(saveType);
                }
                else
                {
                    Utility.ShowMessageBox("ADD", isFlow, false);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        // //先更新方案成功后， 添加 和修改题目、答案
        void Upd_ESurveyCompleted(object sender, Upd_ESurveyCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
            {
                Utility.ShowMessageBox("UPDATE", isFlow, true);
                if (isFlow)
                    saveType = RefreshedTypes.CloseAndReloadData;
                foreach (V_EmployeeSurveySubject v in _osub)  //新增的数据保存后再执行删除操作时 判断
                    v.IsAdd = false;
                RefreshUI(saveType);
            }
            else
            {
                Utility.ShowMessageBox("UPDATE", isFlow, false);
            }
        }
        // //先更新方案成功后， 添加 和修改题目、答案
        void DeleteEmployeeSurveySubjectViewCompleted(object sender, DeleteEmployeeSurveySubjectViewCompletedEventArgs e)
        {
            if (e.Result > 0)
                Utility.ShowMessageBox("DELETE", false, true);
            else
                Utility.ShowMessageBox("DELETE", false, false);
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_REQUIREMASTER>(_survey.RequireMaster, "OA");
            Utility.SetAuditEntity(entity, "T_OA_REQUIREMASTER", _survey.RequireMaster.REQUIREMASTERID, strXmlObjectSource);
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
            _survey.RequireMaster.CHECKSTATE = state;
            isFlow = true;
            Save();
        }

        public string GetAuditState()
        {
            string state = "0";
            if (_survey != null)
                state = _survey.RequireMaster.CHECKSTATE;
            if (types == FormTypes.Browse)
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
