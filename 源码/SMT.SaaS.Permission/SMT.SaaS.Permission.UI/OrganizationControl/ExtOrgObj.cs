﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using PersonnelWS = SMT.Saas.Tools.PersonnelWS;

namespace SMT.SaaS.Permission.UI.OrganizationControl
{
    public class ExtOrgObj
    {
        public OrgTreeItemTypes ObjectType
        {
            get
            {
                if (ObjectInstance is OrganizationWS.T_HR_COMPANY)
                    return OrgTreeItemTypes.Company;
                else if (ObjectInstance is OrganizationWS.T_HR_DEPARTMENT)
                    return OrgTreeItemTypes.Department;
                else if (ObjectInstance is OrganizationWS.T_HR_POST)
                    return OrgTreeItemTypes.Post;
                else if (ObjectInstance is PersonnelWS.T_HR_EMPLOYEE)
                    return OrgTreeItemTypes.Personnel;
                else
                    return OrgTreeItemTypes.All;
            }
        }

        public string ObjectID
        {
            get
            {
                if (ObjectInstance is OrganizationWS.T_HR_COMPANY)
                    return ((OrganizationWS.T_HR_COMPANY)ObjectInstance).COMPANYID;
                else if (ObjectInstance is OrganizationWS.T_HR_DEPARTMENT)
                    return ((OrganizationWS.T_HR_DEPARTMENT)ObjectInstance).DEPARTMENTID;
                else if (ObjectInstance is OrganizationWS.T_HR_POST)
                    return ((OrganizationWS.T_HR_POST)ObjectInstance).POSTID;
                else if (ObjectInstance is PersonnelWS.T_HR_EMPLOYEE)
                    return ((PersonnelWS.T_HR_EMPLOYEE)ObjectInstance).EMPLOYEEID;
                else
                    return "";
            }
            
        }

        public string ObjectName
        {
            get
            {
                if (ObjectInstance is OrganizationWS.T_HR_COMPANY)
                    return ((OrganizationWS.T_HR_COMPANY)ObjectInstance).CNAME;
                else if (ObjectInstance is OrganizationWS.T_HR_DEPARTMENT)
                    return ((OrganizationWS.T_HR_DEPARTMENT)ObjectInstance).T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                else if (ObjectInstance is OrganizationWS.T_HR_POST)
                    return ((OrganizationWS.T_HR_POST)ObjectInstance).T_HR_POSTDICTIONARY.POSTNAME;
                else if (ObjectInstance is PersonnelWS.T_HR_EMPLOYEE)
                    return ((PersonnelWS.T_HR_EMPLOYEE)ObjectInstance).EMPLOYEECNAME;
                else
                    return "";
            }
        }



        public object ObjectInstance { get; set; }

        public object ParentObject { get; set; }
    }
}
