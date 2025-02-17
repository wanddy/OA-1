﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;
namespace SMT.SaaS.Permission.DAL
{
    /// <summary>
    /// 菜单视图
    /// </summary>
    public class V_EntityMenu
    {
        //菜单ID
        public string ENTITYMENUID { get; set; }
        //菜单名称
        public string MENUNAME { get; set; }
        //菜单编号
        public string MENUCODE { get; set; }
        //菜单父ID
        public string EntityMenuFatherID { get; set; }
        //菜单图标
        public string MENUICONPATH { get; set; }
        //菜单地址
        public string URLADDRESS { get; set; }
        //菜单顺序
        public decimal ORDERNUMBER { get; set; }
        //系统类型
        public string SYSTEMTYPE { get; set; }
        //子系统名称
        public string CHILDSYSTEMNAME { get; set; }
        //菜单是否有权限
        public string CanRead { get; set; }
    }
}
