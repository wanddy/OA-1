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
using System.Windows.Navigation;
using SMT.FB.UI.Common;
using System.ComponentModel;
using SMT.FB.UI.FBCommonWS;
using System.Windows.Data;
using SMT.FB.UI.Common.Controls;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.FB.UI.Views.SubjectManagement
{
    public partial class SubjectCompany : FBBasePage
    {
        public SubjectCompany()
        {
            InitializeComponent();
            this.FBBasePageLoaded += new EventHandler(SubjectCompany_FBBasePageLoaded);            
        }

        void SubjectCompany_FBBasePageLoaded(object sender, EventArgs e)
        {
            InitForm();
        }

        #region Fields
        private List<OrderEntity> EntityList;
        private Dictionary<EntityObject, OrderEntity> dictTreeViewItem;

        private OrderEntity currentOrderEntity = null;
        protected OrderEntity CurrentOrderEntity
        {
            get
            {
                return currentOrderEntity;
            }
            set
            {
                if (!object.Equals(currentOrderEntity, value))
                {
                    currentOrderEntity = value;
                    GetEditForm(currentOrderEntity);
                }
            }
        }
        private TreeViewItem currentTreeViewItem = null;

        public EditForm CurrentEditForm { get; set; }
        private ObjectList<EditForm> forms { get; set; }
        private OrderEntityService orderEntityService = null;

        #endregion

        #region 属性

        public List<IControlAction> Controls { get; set; }
        [DefaultValue(true)]
        public bool RefreshEntityWhenLoad { get; set; }
        #endregion

        #region 全局变量
        private string companyId = string.Empty;//保存公司ID，在查询是使用
        bool isLoadTree = true;//只加载左边树
        #endregion

        #region 获取数据
        protected void InitData()
        {
            isLoadTree = true;
            EntityList.Clear();

            dictTreeViewItem.Clear();

            QueryExpression qe = new QueryExpression();
            qe.QueryType = typeof(T_FB_SUBJECTCOMPANY).Name + "_COMPANY";

            orderEntityService.QueryFBEntities(qe);

            this.TreeView.Items.AddTempLoadingItem();
        }


        void orderEntityService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
             //this.TreeView.Items.Clear();

            List<OrderEntity> listCompany = e.Result.ToEntityAdapterList<OrderEntity>().ToList();
            if (isLoadTree)
            {
                this.TreeView.Items.Clear();
                List<TreeViewItem> items = TreeView.Items.AddObjectList(listCompany, "Entity.Name");
                isLoadTree = false;
            }
            else
            {
                this.CurrentOrderEntity = listCompany.FirstOrDefault();
            }
          
            //if (listCompany!=null && listCompany.Count==1)
            //{
            //    this.CurrentOrderEntity = listCompany.FirstOrDefault();
            //}
            //else
            //{
            //    this.TreeView.Items.Clear();
            //    List<TreeViewItem> items = TreeView.Items.AddObjectList(listCompany, "Entity.Name");
            //}
            
            EntityList = listCompany;
            this.CloseProcess();
            //TreeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(TreeView_SelectedItemChanged);

            //TreeViewItem tvi = this.TreeView.Items.FirstOrDefault() as TreeViewItem;
            //if (tvi != null)
            //{
            //    tvi.IsSelected = true;

            //    RoutedPropertyChangedEventArgs<object> ea = new RoutedPropertyChangedEventArgs<object>(null, tvi);
            //    TreeView_SelectedItemChanged(tvi, ea);
            //}

        }

       

        #endregion


        #region 初始化
        private void InitForm()
        {
            tooBarTop.InitToolBarItem(new List<ToolbarItem>() { ToolBarItems.Save });
            tooBarTop.ItemClicked += new EventHandler<ToolBar.ToolBarItemClickArgs>(tooBarTop_ItemClicked);
            editForm.RefreshEntityWhenLoad = false;
            editForm.LoadControlComplete += new EventHandler(editForm_LoadControlComplete);
            orderEntityService = new OrderEntityService();
            orderEntityService.ModelCode = typeof(T_FB_SUBJECTCOMPANY).Name;

            this.FormTitleName.TextTitle.Text = "公司科目设置";

            orderEntityService.SaveListCompleted += new EventHandler<ActionCompletedEventArgs<bool>>(orderEntityService_SaveListCompleted);
            orderEntityService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(orderEntityService_QueryFBEntitiesCompleted);
            EntityList = new List<OrderEntity>();
            forms = new ObjectList<EditForm>();
            dictTreeViewItem = new Dictionary<EntityObject, OrderEntity>();

            this.FormTitleName.Visibility = FBBasePage.ShowTitleBar ? Visibility.Visible : Visibility.Collapsed;
            InitData();
        }
       
        #endregion
        protected void GetEditForm(OrderEntity orderEntity)
        {
            editForm.OrderEntity = orderEntity;
            
            if (orderEntity != null)
            {
                Title = orderEntity.OrderTypeName;
            }
            editForm.InitForm();
        }

        void editForm_LoadControlComplete(object sender, EventArgs e)
        {
            DetailGrid grid = editForm.FindChildControl<DetailGrid>("OrderGrid");
            
            if (grid != null)
            {
                grid.SetGridFix();
                grid.ADGrid.Columns.ForEach(item =>
                    {
                        item.CanUserSort = false;
                    });
                // var aa = grid.ADGrid.Columns[3].Header;
                grid.ADGrid.Columns[3].HeaderStyle = null;
                grid.ADGrid.Columns[3].HeaderStyle = Resources["activeStyle"] as Style;  
                List<string> groups = new List<string> { "Entity.T_FB_SUBJECT.T_FB_SUBJECTTYPE.SUBJECTTYPENAME" };
                grid.Groups = groups;

                grid.ADGrid.Columns[2].Visibility = Visibility.Collapsed;
                
                // 是否可以设置滚动
                if (DataCore.GetSetting("CanSetSubjectControl") == "1")
                {
                    grid.ADGrid.Columns[7].IsReadOnly = false;
                }
                grid.ADGrid.Columns[5].IsReadOnly = false;
                //活动经费处理
                OrderEntity oe = grid.DataContext as OrderEntity;
                oe.FBEntity.CollectionEntity.ForEach(p=>
                    {
                        p.FBEntities.ForEach(item =>
                            {
                                var v = item.Entity as T_FB_SUBJECTCOMPANY;
                                if (v != null && v.T_FB_SUBJECT.SUBJECTID == DataCore.SystemSetting.MONEYASSIGNSUBJECTID)
                                {
                                    item.Entity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(DeatilEntity_PropertyChanged);
                                    DeatilEntity_PropertyChanged(item.Entity, null);
                                 }
                            });
                    });
            }
        }

        

        void DeatilEntity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {            
           T_FB_SUBJECTCOMPANY aa= sender as T_FB_SUBJECTCOMPANY;
           if (aa.ACTIVED != 1 || aa.ISYEARBUDGET != 1 || aa.ISMONTHLIMIT != 1 || aa.ISPERSON != 1)
           {
               ComfirmWindow.ConfirmationBoxs("提示", "活动经费只能是启用、年度受控、月度受控、启用分配个人，并且修改无效。", Utility.GetResourceStr("CONFIRM"), MessageIcon.Question);
        
              // CommonFunction.ShowErrorMessage("活动经费只能是启用、年度受控、月度受控、启用分配个人，并且修改无效。");
           }
            sender.SetObjValue("ACTIVED", 1);
            sender.SetObjValue("ISYEARBUDGET", 1);
            sender.SetObjValue("ISMONTHLIMIT", 1);
            sender.SetObjValue("ISPERSON", 1);
            sender.SetObjValue("CONTROLTYPE", 3);
        }

        void ADGrid_LoadingRowGroup(object sender, DataGridRowGroupHeaderEventArgs e)
        {
            
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.ShowProcess();
            TreeViewItem treeViewItem = e.NewValue as TreeViewItem;
            if (treeViewItem != null)
            {
                QuerySubjectCompanyData(treeViewItem);
                this.currentTreeViewItem = treeViewItem;
                OrderEntity entity = currentTreeViewItem.DataContext as OrderEntity;
                this.CurrentOrderEntity = entity;// new OrderEntity(typeof(VirtualCompany));               
            }
            //PermissionHelper.GetPermissionValue(orderEntityService.ModelCode, Permissions.Edit);
            //tooBarTop.ShowItem("Save", 
        }

        /// <summary>
        /// 点击公司右边显示科目
        /// </summary>
        /// <param name="item"></param>
        void QuerySubjectCompanyData(TreeViewItem item)
        {
            try
            {
                var com = (item.DataContext as EntityAdapter).Entity as VirtualEntityObject;
                if (com != null)
                {
                    QueryExpression qe = new QueryExpression();
                    qe.QueryType = typeof(T_FB_SUBJECTCOMPANY).Name + "_COMPANY";
                    QueryExpression company = new QueryExpression();
                    company.PropertyName = "COMPANYID";
                    company.PropertyValue = com.ID;
                    companyId = com.ID;
                    qe.RelatedExpression = company;
                    orderEntityService.QueryFBEntities(qe);
                }
            }
            catch (Exception ex)
            {
                CommonFunction.ShowErrorMessage(ex.Message);
                CloseProcess();
            }
        }

        #region 保存

        private void tooBarTop_ItemClicked(object sender, ToolBar.ToolBarItemClickArgs e)
        {
            
            try
            {
                ShowProcess();
              //  ComfirmWindow.ConfirmationBoxs("提示", "测试页面", Utility.GetResourceStr("CONFIRM"), MessageIcon.Question);
                Save();
                
            }
            catch ( Exception ex)
            {
                CommonFunction.ShowErrorMessage(ex.Message);
                CloseProcess();
            }
        }

        private void Save()
        {
            ObservableCollection<FBEntity> listOfFBEntity = new ObservableCollection<FBEntity>();
            for (int i = 0; i < EntityList.Count; i++)
            {
                OrderEntity item = EntityList[i];
                FBEntity fbEntitySave = item.GetModifiedFBEntity();

                
                fbEntitySave.CollectionEntity[0].FBEntities.ToList().ForEach(fbEntity =>
                {
                    T_FB_SUBJECTCOMPANY sc = fbEntity.Entity as T_FB_SUBJECTCOMPANY;
                    if (fbEntity.Entity.EntityKey.EntityKeyValues == null)
                    {
                        
                        fbEntity.FBEntityState = FBEntityState.Added;
                        fbEntity.Entity.EntityKey = null;
                        fbEntity.SetObjValue("Entity.SUBJECTCOMPANYID", Guid.NewGuid().ToString());
                        fbEntity.SetObjValue("Entity.CREATECOMPANYID", item.LoginUser.Company.Value);
                        fbEntity.SetObjValue("Entity.CREATECOMPANYNAME", item.LoginUser.Company.Text);
                        fbEntity.SetObjValue("Entity.CREATEDEPARTMENTID", item.LoginUser.Department.Value);
                        fbEntity.SetObjValue("Entity.CREATEDEPARTMENTNAME", item.LoginUser.Department.Text);
                        fbEntity.SetObjValue("Entity.CREATEPOSTID", item.LoginUser.Post.Value);
                        fbEntity.SetObjValue("Entity.CREATEPOSTNAME", item.LoginUser.Post.Text);
                        fbEntity.SetObjValue("Entity.CREATEUSERID", item.LoginUser.Value);
                        fbEntity.SetObjValue("Entity.CREATEUSERNAME", item.LoginUser.Text);

                        fbEntity.SetObjValue("Entity.OWNERCOMPANYID", item.GetObjValue("Entity.OWNERCOMPANYID"));
                        fbEntity.SetObjValue("Entity.OWNERCOMPANYNAME", item.GetObjValue("Entity.OWNERCOMPANYNAME"));
                        fbEntity.SetObjValue("Entity.OWNERDEPARTMENTID", item.LoginUser.Department.Value);
                        fbEntity.SetObjValue("Entity.OWNERDEPARTMENTNAME", item.LoginUser.Department.Text);
                        fbEntity.SetObjValue("Entity.OWNERPOSTID", item.LoginUser.Post.Value);
                        fbEntity.SetObjValue("Entity.OWNERPOSTNAME", item.LoginUser.Post.Text);
                        fbEntity.SetObjValue("Entity.OWNERID", item.LoginUser.Value);
                        fbEntity.SetObjValue("Entity.OWNERNAME", item.LoginUser.Text);
                        fbEntity.SetObjValue("Entity.EDITSTATES", decimal.Parse("1"));
                    }
                    else
                    {
                        //if (sc.ACTIVED != 1)
                        //{
                        //    fbEntity.FBEntityState = FBEntityState.Detached;
                        //}
                    }
                    fbEntity.SetObjValue("Entity.UPDATEUSERID", item.LoginUser.Value);
                    fbEntity.SetObjValue("Entity.UPDATEUSERNAME", item.LoginUser.Text);

                    sc.T_FB_SUBJECTReference.EntityKey = sc.T_FB_SUBJECT.EntityKey;
                    sc.T_FB_SUBJECT = null;

                    sc.T_FB_SUBJECTDEPTMENT = new ObservableCollection<T_FB_SUBJECTDEPTMENT>();
                    listOfFBEntity.Add(fbEntity);

                });
            }

            if (listOfFBEntity.Count > 0)
            {
                this.CurrentOrderEntity = new OrderEntity(typeof(VirtualCompany));
                this.orderEntityService.SaveList(listOfFBEntity);
            }
            else
            {
                CloseProcess();
            }
        }

        void orderEntityService_SaveListCompleted(object sender, ActionCompletedEventArgs<bool> e)
        {
            if (e.Result)
            {
                CommonFunction.NotifySelection("保存成功");
                InitData();
            }
            CloseProcess();
        }

        // 不要删除
        private void SetFouceBeforeAction(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }
        #endregion

        private void SelectTreeViewItem(TreeViewItem item)
        {
            if (item != null)
            {
                item.IsSelected = true;
                TreeView_SelectedItemChanged(this.TreeView, new RoutedPropertyChangedEventArgs<object>(null, item));
            }
        }

        public override bool CheckPermisstion()
        {
            int perm = PermissionHelper.GetPermissionValue(typeof(T_FB_SUBJECTCOMPANY).Name, Permissions.Browse);

            // 需要大于等公司的范围权限
            return !(perm > (int)PermissionRange.Company || perm < 0);
       
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(companyId))
                {
                    CommonFunction.ShowMessage("请选择公司");
                    return;
                }
                string tbText = this.tbSubjectName.Text.Trim();
                if (string.IsNullOrWhiteSpace(tbText))
                {
                    CommonFunction.ShowMessage("请输入查询条件");
                    return;
                }
                QueryExpression qe = new QueryExpression();
                qe.QueryType = typeof(T_FB_SUBJECTCOMPANY).Name + "_COMPANY";

                QueryExpression qeCompany = new QueryExpression();
                qeCompany.PropertyName = "COMPANYID";
                qeCompany.PropertyValue = companyId;

                QueryExpression qeFilterString = new QueryExpression();
                qeFilterString.PropertyName = "filterString";
                qeFilterString.PropertyValue = tbText;

                qeCompany.RelatedExpression = qeFilterString;
                qe.RelatedExpression = qeCompany;
                this.ShowProcess();
                orderEntityService.QueryFBEntities(qe);
            }
            catch (Exception ex)
            {
                CommonFunction.ShowErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// 重置刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            this.tbSubjectName.Text = "";
            try
            {
                if (string.IsNullOrWhiteSpace(companyId))
                {
                    return;
                }
                QueryExpression qe = new QueryExpression();
                qe.QueryType = typeof(T_FB_SUBJECTCOMPANY).Name + "_COMPANY";

                QueryExpression qeCompany = new QueryExpression();
                qeCompany.PropertyName = "COMPANYID";
                qeCompany.PropertyValue = companyId;
                qe.RelatedExpression = qeCompany;
                this.ShowProcess();
                orderEntityService.QueryFBEntities(qe);
            }
            catch (Exception ex)
            {
                CommonFunction.ShowErrorMessage(ex.Message);
                CloseProcess();
            }
        }

        private void cbActiveS_Click(object sender, RoutedEventArgs e)
        {
            var isCheck = ((CheckBox)sender).IsChecked.Value;
            
            var msg = isCheck ? "是否确认全部启用？" : "是否确认全部不启用？";
            var active = isCheck ? 1 : 0;
            
            Action action = () =>
            {
                ((CheckBox)sender).IsChecked = isCheck;
                var oEntity = editForm.OrderEntity;

                var list = oEntity.GetRelationFBEntities(typeof(T_FB_SUBJECTCOMPANY).Name).ToEntityList<T_FB_SUBJECTCOMPANY>();
                list.ForEach(item =>
                    {
                        if (item.T_FB_SUBJECT.SUBJECTID != DataCore.SystemSetting.MONEYASSIGNSUBJECTID)
                        {
                            item.ACTIVED = active;
                        }

                    });
            };
            ((CheckBox)sender).IsChecked = !isCheck;
            CommonFunction.DialogOKCanel("确认", msg, action,null);
        }

    }
}
