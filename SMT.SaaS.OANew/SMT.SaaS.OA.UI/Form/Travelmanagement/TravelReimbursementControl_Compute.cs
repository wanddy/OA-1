﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.SelectCityControl;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.TravelExpApplyMaster;
using SMT.Saas.Tools.FBServiceWS;
using SMT.SAAS.Platform.Logging;
using SMT.SaaS.MobileXml;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class TravelReimbursementControl
    {
        #region 出差时间计算
        /// <summary>
        /// 计算出差天数
        /// </summary>
        public void TravelTime()
        {
            if (TravelDetailList_Golbal == null)
            {
                return;
            }
            #region 时间计算
            TextBox myDaysTime = new TextBox();
            bool OneDayTrave = false;
            DateTime dtTempStart=new DateTime(2014,1,1);
            int tempStart = 0;
            for (int i = 0; i < TravelDetailList_Golbal.Count; i++)
            {
                tempStart = i;
                //记录本条记录以便处理
                DateTime StartTime = Convert.ToDateTime(TravelDetailList_Golbal[i].STARTDATE);
                DateTime EndTime = Convert.ToDateTime(TravelDetailList_Golbal[i].ENDDATE);
                string StartCity = TravelDetailList_Golbal[i].DEPCITY;
                string EndCity = TravelDetailList_Golbal[i].DESTCITY;
                GetTraveDayTextBox(myDaysTime, i).Text = string.Empty;

                #region 判断是否当他往返
                OneDayTrave = false;
                //遍历剩余的记录
                for (int j = i + 1; j < TravelDetailList_Golbal.Count; j++)
                {
                    DateTime NextStartTime = Convert.ToDateTime(TravelDetailList_Golbal[j].STARTDATE);
                    DateTime NextEndTime = Convert.ToDateTime(TravelDetailList_Golbal[j].ENDDATE);
                    string NextTraveFrom = TravelDetailList_Golbal[j].DEPCITY;
                    string NextTraveTo = TravelDetailList_Golbal[j].DESTCITY;
                   

                    GetTraveDayTextBox(myDaysTime, j).Text = string.Empty;
                    if (NextEndTime.Date == StartTime.Date)
                    {
                        if (NextTraveTo == StartCity && (TravelDetailList_Golbal.Count == 2 || TravelDetailList_Golbal.Count == 1))
                        {
                            myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                            myDaysTime.Text = "1";
                            TravelDetailList_Golbal[i].BUSINESSDAYS = "1";
                            //i = j - 1;
                            OneDayTrave = true;
                            break;
                        }
                        else continue;
                    }
                    else
                    { 
                        break;
                    }
                }
                if (OneDayTrave == true) continue;
                #endregion

                //非当天往返
                decimal TotalDays = 0;
                switch (TravelDetailList_Golbal.Count())
                {
                    case 1:
                        TotalDays = CaculateTravDays(StartTime, EndTime);
                        break;
                    case 2:
                        if (i == 1) break;
                        DateTime NextEndTime = Convert.ToDateTime(TravelDetailList_Golbal[i + 1].ENDDATE);
                        TotalDays = CaculateTravDays(StartTime, NextEndTime);
                        break;
                    default:
                         if (i == TravelDetailList_Golbal.Count() - 1) break;//最后一条记录不处理
                        
                            for (int j =i+1; j < TravelDetailList_Golbal.Count() ;j++ )
                            {
                                //如果下一条记录的到达日期==上条记录的开始日期,不计算出差天数
                                if (TravelDetailList_Golbal[j].ENDDATE.Value.Date
                                    == TravelDetailList_Golbal[i].STARTDATE.Value.Date)
                                {
                                    TravelDetailList_Golbal[i].BUSINESSDAYS = "0";
                                    if(dtTempStart==new DateTime(2014,1,1))
                                    { 
                                        dtTempStart = TravelDetailList_Golbal[i].STARTDATE.Value;
                                    }
                                    if (j == TravelDetailList_Golbal.Count()-1)//轮询到最后一条记录了
                                    {
                                        DateTime dtEndTime = Convert.ToDateTime(TravelDetailList_Golbal[j].ENDDATE);
                                        TotalDays = CaculateTravDays(dtTempStart, dtEndTime);
                                        i = j;
                                    }
                                    continue;
                                }
                                else
                                {
                                    if (dtTempStart == new DateTime(2014, 1, 1))
                                    {
                                        dtTempStart = dtTempStart = TravelDetailList_Golbal[i].STARTDATE.Value;
                                    }
                                    DateTime dtEndTime = Convert.ToDateTime(TravelDetailList_Golbal[j].STARTDATE);
                                    if (i == TravelDetailList_Golbal.Count() - 2)//倒数第二条记录=最后一条结束时间-上一条开始时间
                                    {
                                        dtEndTime = Convert.ToDateTime(TravelDetailList_Golbal[j].ENDDATE);
                                    }
                                    TotalDays = CaculateTravDays(dtTempStart, dtEndTime);
                                    i = j;
                                    break;
                                }
                            }
                        
                        //否则出差时间=下一条开始时间-上一条开始时间
                        //DateTime NextStartTime = Convert.ToDateTime(TravelDetailList_Golbal[i + 1].STARTDATE);
                        //TotalDays = CaculateTravDays(StartTime, NextStartTime);
                        break;
                }
                //保存计算的出差天数
                TravelDetailList_Golbal[tempStart].BUSINESSDAYS = TotalDays.ToString();
                //设置显示值
                try
                {
                    myDaysTime = GetTraveDayTextBox(myDaysTime, tempStart);
                    myDaysTime.Text = TotalDays.ToString();
                }
                catch (Exception ex)
                {

                }
            }
            #endregion
        }

        private TextBox GetTraveDayTextBox(TextBox myDaysTime, int i)
        {
            try
            {
                if (DaGrEdit.Columns[4].GetCellContent(TravelDetailList_Golbal[i]) != null)
                {
                    myDaysTime = DaGrEdit.Columns[4].GetCellContent(TravelDetailList_Golbal[i]).FindName("txtTOTALDAYS") as TextBox;
                }
            }
            catch (Exception ex)
            {
            }
            return myDaysTime;
        }
        /// <summary>
        /// 计算出差时长结算-开始时间NextStartTime-FirstStartTime
        /// </summary>
        /// <param name="StartTime">开始时间</param>
        /// <param name="EndTime">结束时间</param>
        /// <returns></returns>
        private decimal CaculateTravDays(DateTime StartTime, DateTime EndTime)
        {
            //计算出差时间（天数）
            TimeSpan TraveDays = EndTime.Subtract(StartTime);
            decimal TotalDays = 0;//出差天数
            decimal TotalHours = 0;//出差小时
            TotalDays = TraveDays.Days;
            TotalHours = TraveDays.Hours;
            int customhalfday = travelsolutions.CUSTOMHALFDAY.ToInt32();
            if (TotalHours >= customhalfday)//如果出差时间大于等于方案设置的时间，按方案标准时间计算
            {
                TotalDays += 1;
            }
            else
            {
                if (TotalHours > 0)
                    TotalDays += Convert.ToDecimal("0.5");//TotalDays += decimal.Round(TotalHours / 24,1);
            }
            return TotalDays;
        }
        #endregion

        #region 住宿时间计算

        public void TravelTimeCalculation()
        {
            if (TravelDetailList_Golbal == null || DaGrEdit.ItemsSource == null)
            {
                return;
            }
            #region 存在多条的处理
            TextBox myDaysTime = new TextBox();
            bool OneDayTrave = false;
            for (int i = 0; i < TravelDetailList_Golbal.Count; i++)
            {
                GetTraveTimeCalculationTextBox(myDaysTime, i).Text = string.Empty;
                OneDayTrave = false;
                //记录本条记录以便处理
                DateTime FirstStartTime = Convert.ToDateTime(TravelDetailList_Golbal[i].STARTDATE);
                DateTime FirstEndTime = Convert.ToDateTime(TravelDetailList_Golbal[i].ENDDATE);
                string FirstTraveFrom = TravelDetailList_Golbal[i].DEPCITY;
                string FirstTraveTo = TravelDetailList_Golbal[i].DESTCITY;
                //遍历剩余的记录
                for (int j = i + 1; j < TravelDetailList_Golbal.Count; j++)
                {
                    DateTime NextStartTime = Convert.ToDateTime(TravelDetailList_Golbal[j].STARTDATE);
                    DateTime NextEndTime = Convert.ToDateTime(TravelDetailList_Golbal[j].ENDDATE);
                    string NextTraveFrom = TravelDetailList_Golbal[j].DEPCITY;
                    string NextTraveTo = TravelDetailList_Golbal[j].DESTCITY;
                    GetTraveTimeCalculationTextBox(myDaysTime, j).Text = string.Empty;
                    if (NextEndTime.Date == FirstStartTime.Date)
                    {
                        if (NextTraveTo == FirstTraveFrom)
                        {
                            myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                            myDaysTime.Text = "1";
                            i = j - 1;
                            OneDayTrave = true;
                            break;
                        }
                        else continue;
                    }
                    else
                    {
                        break;
                    }
                }
                if (OneDayTrave == true) continue;
                //非当天往返
                decimal TotalDays = 0;
                switch (TravelDetailList_Golbal.Count())
                {
                    case 1:
                        TotalDays = CaculateTravCalculationDays(FirstStartTime, FirstEndTime);
                        myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                    case 2:
                        if (i == 1) break;
                        DateTime NextEndTime = Convert.ToDateTime(TravelDetailList_Golbal[i + 1].ENDDATE);
                        TotalDays = CaculateTravCalculationDays(FirstStartTime, NextEndTime);
                        myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                    default:
                        if (i == TravelDetailList_Golbal.Count() - 1) break;//最后一条记录不处理
                        if (i == TravelDetailList_Golbal.Count() - 2)//倒数第二条记录=最后一条结束时间-上一条开始时间
                        {
                            DateTime NextENDDATETime = Convert.ToDateTime(TravelDetailList_Golbal[i + 1].ENDDATE);
                            TotalDays = CaculateTravCalculationDays(FirstStartTime, NextENDDATETime);
                            myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                            myDaysTime.Text = TotalDays.ToString();
                            break;
                        }
                        //否则出差时间=下一条开始时间-上一条开始时间
                        DateTime NextStartTime = Convert.ToDateTime(TravelDetailList_Golbal[i + 1].STARTDATE);
                        TotalDays = CaculateTravCalculationDays(FirstStartTime, NextStartTime);
                        myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                }
            }
            #endregion
        }

        private TextBox GetTraveTimeCalculationTextBox(TextBox myDaysTime, int i)
        {
            if (DaGrEdit.Columns[5].GetCellContent(TravelDetailList_Golbal[i]) != null)
            {
                myDaysTime = DaGrEdit.Columns[5].GetCellContent(TravelDetailList_Golbal[i]).FindName("txtTHENUMBEROFNIGHTS") as TextBox;
            }
            return myDaysTime;
        }

        /// <summary>
        /// 计算出差时长结算-开始时间NextStartTime-FirstStartTime
        /// </summary>
        /// <param name="FirstStartTime">开始时间</param>
        /// <param name="NextStartTime">结束时间</param>
        /// <returns></returns>
        private decimal CaculateTravCalculationDays(DateTime FirstStartTime, DateTime NextStartTime)
        {
            //计算出差时间（天数）
            TimeSpan TraveDays = NextStartTime.Subtract(FirstStartTime.Date);
            decimal TotalDays = 0;//出差天数
            decimal TotalHours = 0;//出差小时
            TotalDays = TraveDays.Days;
            TotalHours = TraveDays.Hours;

            return TotalDays;
        }
        #endregion

        #region 计算出差补贴补贴
        /// <summary>
        /// 计算补贴,在出差表格还没有loadingrow之前获取控件会报错。
        /// </summary>
        /// <param name="FromReadOnlyDataGrid">是否显示只读的查看Grid</param>
        private void TravelAllowance(bool FromReadOnlyDataGrid)
        {
            try
            {
                DataGrid dataGrid = new DataGrid();
                if (FromReadOnlyDataGrid)//查看模式下
                {
                    dataGrid = this.DaGrReadOnly;
                }
                else
                {
                    dataGrid = this.DaGrEdit;
                }

                if (TravelDetailList_Golbal != null)
                {
                    T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();

                    int i = 0;
                    foreach (var detail in TravelDetailList_Golbal)
                    {
                        double toodays = 0;
                        List<string> list = new List<string> { detail.BUSINESSDAYS };

                        if (detail.BUSINESSDAYS != null && !string.IsNullOrEmpty(detail.BUSINESSDAYS))
                        {
                            double totalHours = System.Convert.ToDouble(list[0]);
                            toodays = totalHours;
                        }
                        double totolDay = toodays;//计算本次出差的总天数

                        string cityValue = detail.DESTCITY.Replace(",", "");//目标城市值
                        entareaallowance = this.GetAllowanceByCityValue(cityValue);
                        if (travelsolutions != null && employeepost != null)
                        {

                            if (EmployeePostLevel.ToInt32() <= travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                            {
                                //textStandards.Text = textStandards.Text +"报销人的岗位级别小于等于I级，无交通补贴及住宿补贴";
                                detail.TRANSPORTATIONSUBSIDIES = 0;
                                detail.MEALSUBSIDIES = 0;
                            }
                            else
                            {
                                #region 根据本次出差的总天数,根据天数获取相应的补贴
                                if (totolDay <= travelsolutions.MINIMUMINTERVALDAYS.ToInt32())//本次出差总时间小于等于设定天数的报销标准
                                {
                                    if (entareaallowance != null)
                                    {
                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                detail.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                            {
                                                detail.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    detail.TRANSPORTATIONSUBSIDIES = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES.ToDouble() * toodays);
                                                }
                                                else
                                                {
                                                    detail.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                        else//如果天数为null的禁用住宿费控件
                                        {
                                            detail.TRANSPORTATIONSUBSIDIES = 0;
                                        }
                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                detail.MEALSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1")//如果是开会
                                            {
                                                detail.MEALSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    detail.MEALSUBSIDIES = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES.ToDouble() * toodays);
                                                }
                                                else
                                                {
                                                    detail.MEALSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                        else//如果天数为null的禁用住宿费控件
                                        {
                                            //txtASubsidies.IsReadOnly = true;
                                        }
                                    }
                                }

                                #endregion

                                #region 如果出差天数大于设定的最大天数,按驻外标准获取补贴

                                else if (totolDay > travelsolutions.MAXIMUMRANGEDAYS.ToInt32())
                                {
                                    if (entareaallowance != null)
                                    {
                                        double DbTranceport = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble();
                                        double DbMeal = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble();
                                        //区间补贴标准 区间报销比例（50）/100
                                        double tfSubsidies = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
                                        double mealSubsidies = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
                                        if (entareaallowance != null)
                                        {
                                            if (detail.BUSINESSDAYS != null)
                                            {
                                                if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                                {
                                                    detail.TRANSPORTATIONSUBSIDIES = 0;
                                                    detail.TRANSPORTCOSTS = 0;
                                                }
                                                else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                                {
                                                    detail.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                                else
                                                {
                                                    if (EmployeePostLevel.ToInt32() > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                    {
                                                        //可全额报销天数*每天的补贴
                                                        double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbTranceport;
                                                        //区间可以报销天数*报销比例50%
                                                        double middlemoney = (travelsolutions.MAXIMUMRANGEDAYS.ToDouble() - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * tfSubsidies;
                                                        //除以2是因为驻外标准不分餐费和交通补贴，2者合2为一，否则会多加 （餐补及交通补贴都按驻外标准计算）
                                                        double lastmoney = (totolDay - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble() / 2;
                                                        detail.TRANSPORTATIONSUBSIDIES = Convert.ToDecimal(minmoney + middlemoney + lastmoney);
                                                    }
                                                    else
                                                    {
                                                        detail.TRANSPORTATIONSUBSIDIES = 0;
                                                    }
                                                }
                                            }
                                            else//如果天数为null的禁用住宿费控件
                                            {
                                            }
                                            if (detail.BUSINESSDAYS != null)
                                            {
                                                if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                                {
                                                    detail.MEALSUBSIDIES = 0;
                                                }
                                                else if (detail.GOOUTTOMEET == "1")//如果是开会
                                                {
                                                    detail.MEALSUBSIDIES = 0;
                                                }
                                                else
                                                {
                                                    if (EmployeePostLevel.ToInt32() > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                    {
                                                        double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbMeal;
                                                        //double middlemoney = (travelsolutions.MAXIMUMRANGEDAYS.ToDouble() - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * mealSubsidies;
                                                        double IntMaxDays = travelsolutions.MAXIMUMRANGEDAYS.ToDouble();
                                                        double IntMinDAys = travelsolutions.MINIMUMINTERVALDAYS.ToDouble();
                                                        double middlemoney = (IntMaxDays - IntMinDAys) * mealSubsidies;
                                                        //double lastmoney = (tresult - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble();
                                                        //驻外标准：交通费和餐费补贴为一起的，所以除以2
                                                        double lastmoney = (totolDay - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble() / 2;
                                                        detail.MEALSUBSIDIES = Convert.ToDecimal(minmoney + middlemoney + lastmoney);
                                                    }
                                                    else
                                                    {
                                                        detail.MEALSUBSIDIES = 0;
                                                    }
                                                }
                                            }
                                            else//如果天数为null的禁用住宿费控件
                                            {
                                            }
                                        }
                                    }
                                }

                                #endregion

                                #region 如果出差时间大于设定的最小天数并且小于设定的最大天数的报销标准

                                else if (totolDay >= travelsolutions.MINIMUMINTERVALDAYS.ToInt32() && totolDay <= travelsolutions.MAXIMUMRANGEDAYS.ToInt32())
                                {
                                    if (entareaallowance != null)
                                    {
                                        double DbTranceport = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble();
                                        double DbMeal = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble();
                                        double tfSubsidies = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
                                        double mealSubsidies = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                detail.TRANSPORTATIONSUBSIDIES = 0;
                                                detail.TRANSPORTCOSTS = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                            {
                                                detail.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbTranceport;
                                                    double middlemoney = (totolDay - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * tfSubsidies;
                                                    detail.TRANSPORTATIONSUBSIDIES = Convert.ToDecimal(minmoney + middlemoney);
                                                }
                                                else
                                                {
                                                    detail.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                        else//如果天数为null的禁用住宿费控件
                                        {
                                            //txtASubsidies.IsReadOnly = true;
                                        }
                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                detail.MEALSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1")//如果是开会
                                            {
                                                detail.MEALSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    //最小区间段金额
                                                    double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbMeal;
                                                    //中间区间段金额
                                                    double middlemoney = (totolDay - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * mealSubsidies;
                                                    detail.MEALSUBSIDIES = Convert.ToDecimal(minmoney + middlemoney);
                                                }
                                                else
                                                {
                                                    detail.MEALSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                        else//如果天数为null的禁用住宿费控件
                                        {
                                            //txtASubsidies.IsReadOnly = true;
                                        }
                                    }
                                }

                                #endregion
                            }
                        }

                        //交通补贴，
                        if (detail.TRANSPORTATIONSUBSIDIES != null)
                        {
                            try
                            {
                                //交通补贴,在grid没有绑定之前获取不到。
                                TextBox txtTFSubsidies = DaGrEdit.Columns[10].GetCellContent(detail).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;
                                if (txtTFSubsidies != null)
                                {
                                    txtTFSubsidies.Text = detail.TRANSPORTATIONSUBSIDIES.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                Utility.SetLog(ex.ToString());
                            }
                        }
                        //餐费补贴
                        if (detail.MEALSUBSIDIES != null)
                        {
                            try
                            {
                                //餐费补贴，在grid没有绑定之前获取不到。
                                TextBox txtMealSubsidies = DaGrEdit.Columns[11].GetCellContent(detail).FindName("txtMEALSUBSIDIES") as TextBox;
                                if (txtMealSubsidies != null)
                                {
                                    txtMealSubsidies.Text = detail.MEALSUBSIDIES.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                Utility.SetLog(ex.ToString());
                            }
                        }
                      
                        //Fees = total;
                        i++;
                    }
                    CountMoney();
                }
            }
            catch (Exception ex)
            {
                Utility.SetLog(ex.ToString());
            }
        }
        ///// <summary>
        ///// 计算补贴
        ///// </summary>
        ///// <param name="FromReadOnlyDataGrid">是否显示只读的查看Grid</param>
        //private void TravelAllowance(bool FromReadOnlyDataGrid)
        //{
        //    TextBox txtTFSubsidies = new TextBox();//初始化交通补贴控件
        //    TextBox txtMealSubsidies = new TextBox();//初始化餐费补贴控件
        //    TextBox txtASubsidies = new TextBox();//初始化住宿费控件
        //    TextBox txtTranSportcosts = new TextBox();//初始化交通费控件
        //    TextBox txtOtherCosts = new TextBox();//初始化其他费用控件

        //    DataGrid dataGrid = new DataGrid();
        //    if (FromReadOnlyDataGrid)//查看模式下
        //    {
        //        dataGrid = this.DaGrReadOnly;
        //    }
        //    else
        //    {
        //        dataGrid = this.DaGrEdit;
        //    }

        //    if (dataGrid.ItemsSource != null)
        //    {
        //        T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();

        //        ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = dataGrid.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
        //        double total = 0;
        //        int i = 0;
        //        foreach (var obje in objs)
        //        {
        //            i++;
        //            double toodays = 0;
        //            //if (FromReadOnlyDataGrid)
        //            //{
        //            //    if (i > 0) txtTFSubsidies = dataGrid.Columns[10].GetCellContent(obje).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;//交通补贴
        //            //    if (i > 0) txtMealSubsidies = dataGrid.Columns[11].GetCellContent(obje).FindName("txtMEALSUBSIDIES") as TextBox;//餐费补贴
        //            //}
        //            //else
        //            //{
        //            //    if (i >0) txtTFSubsidies = dataGrid.Columns[10].GetCellContent(obje).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;//交通补贴
        //            //    if (i >0) txtMealSubsidies = dataGrid.Columns[11].GetCellContent(obje).FindName("txtMEALSUBSIDIES") as TextBox;//餐费补贴
        //            //}

        //            List<string> list = new List<string>
        //                        {
        //                             obje.BUSINESSDAYS
        //                        };

        //            if (obje.BUSINESSDAYS != null && !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //            {
        //                double totalHours = System.Convert.ToDouble(list[0]);
        //                toodays = totalHours;
        //            }
        //            double totolDay = toodays;//计算本次出差的总天数

        //            string cityValue = citysEndList_Golbal[i - 1].Replace(",", "");//目标城市值
        //            entareaallowance = this.GetAllowanceByCityValue(cityValue);

        //            #region 根据本次出差的总天数,根据天数获取相应的补贴
        //            if (travelsolutions != null && employeepost != null)
        //            {

        //                txtTFSubsidies = GetTFSubsidiesTextBox(txtTFSubsidies, i, FromReadOnlyDataGrid);//交通补贴控件赋值
        //                txtTranSportcosts = GetTranSportcostsTextBox(txtTranSportcosts, i,FromReadOnlyDataGrid);//交通费控件赋值
        //                txtASubsidies = GetASubsidiesTextBox(txtASubsidies, i, FromReadOnlyDataGrid);//住宿费控件赋值
        //                txtOtherCosts = GetOtherCostsTextBox(txtOtherCosts, i, FromReadOnlyDataGrid);//其他费用控件赋值
        //                txtMealSubsidies = GetMealSubsidiesTextBox(txtMealSubsidies, i, FromReadOnlyDataGrid);//餐费补贴控件赋值

        //                if (totolDay <= travelsolutions.MINIMUMINTERVALDAYS.ToInt32())//本次出差总时间小于等于设定天数的报销标准
        //                {
        //                    if (entareaallowance != null)
        //                    {
        //                        if (txtTFSubsidies != null)//交通补贴
        //                        {
        //                            if (obje.BUSINESSDAYS != null)
        //                            {
        //                                if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                                {
        //                                    txtTFSubsidies.Text = "0";
        //                                    txtTFSubsidies.IsReadOnly = true;
        //                                    txtTranSportcosts.IsReadOnly = true;//交通费
        //                                    txtASubsidies.IsReadOnly = true;//住宿标准
        //                                    txtOtherCosts.IsReadOnly = true;//其他费用
        //                                }
        //                                else if (obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
        //                                {
        //                                    txtTFSubsidies.Text = "0";
        //                                }
        //                                else
        //                                {
        //                                    if (EmployeePostLevel.ToInt32() > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                    {
        //                                        txtTFSubsidies.Text = (entareaallowance.TRANSPORTATIONSUBSIDIES.ToDouble() * toodays).ToString();
        //                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
        //                                        if (string.IsNullOrWhiteSpace(txtTFSubsidies.Text))
        //                                        {
        //                                            ComfirmWindow com = new ComfirmWindow();
        //                                            com.OnSelectionBoxClosed += (obj, result) =>
        //                                            {
        //                                                txtTranSportcosts.IsReadOnly = true;//交通费
        //                                                txtASubsidies.IsReadOnly = true;//住宿标准
        //                                                txtOtherCosts.IsReadOnly = true;//其他费用
        //                                            };
        //                                            if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //                                            {
        //                                                if (formType == FormTypes.Audit) return;
        //                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        txtTFSubsidies.Text = "0";
        //                                        txtTFSubsidies.IsReadOnly = false;
        //                                    }
        //                                }
        //                            }
        //                            else//如果天数为null的禁用住宿费控件
        //                            {
        //                                txtASubsidies.IsReadOnly = true;
        //                            }
        //                        }
        //                        if (txtMealSubsidies != null)//餐费补贴
        //                        {
        //                            if (obje.BUSINESSDAYS != null)
        //                            {
        //                                if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                                {
        //                                    txtMealSubsidies.Text = "0";
        //                                    txtMealSubsidies.IsReadOnly = true;
        //                                    txtTranSportcosts.IsReadOnly = true;//交通费
        //                                    txtASubsidies.IsReadOnly = true;//住宿标准
        //                                    txtOtherCosts.IsReadOnly = true;//其他费用
        //                                }
        //                                else if (obje.GOOUTTOMEET == "1")//如果是开会
        //                                {
        //                                    txtMealSubsidies.Text = "0";
        //                                }
        //                                else
        //                                {
        //                                    if (EmployeePostLevel.ToInt32() > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                    {
        //                                        txtMealSubsidies.Text = (entareaallowance.MEALSUBSIDIES.ToDouble() * toodays).ToString();
        //                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
        //                                        if (string.IsNullOrWhiteSpace(txtMealSubsidies.Text))
        //                                        {
        //                                            ComfirmWindow com = new ComfirmWindow();
        //                                            com.OnSelectionBoxClosed += (obj, result) =>
        //                                            {
        //                                                txtTranSportcosts.IsReadOnly = true;//交通费
        //                                                txtASubsidies.IsReadOnly = true;//住宿标准
        //                                                txtOtherCosts.IsReadOnly = true;//其他费用
        //                                            };
        //                                            if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //                                            {
        //                                                if (formType == FormTypes.Audit) return;
        //                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        txtMealSubsidies.Text = "0";
        //                                        txtMealSubsidies.IsReadOnly = false;
        //                                    }
        //                                }
        //                            }
        //                            else//如果天数为null的禁用住宿费控件
        //                            {
        //                                txtASubsidies.IsReadOnly = true;
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (EmployeePostLevel.ToInt32() <= travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                    {
        //                        txtTFSubsidies.Text = "0";
        //                        txtMealSubsidies.Text = "0";
        //                    }
        //                }
        //            }
        //            #endregion

        //            #region 如果出差天数大于设定的最大天数,按驻外标准获取补贴
        //            if (travelsolutions != null && employeepost != null)
        //            {
        //                TextBox txtTranSportcosts = new TextBox();//初始化交通费控件
        //                TextBox txtOtherCosts = new TextBox();//初始化其他费用控件

        //                txtTFSubsidies = GetTFSubsidiesTextBox(txtTFSubsidies, i, FromReadOnlyDataGrid);//交通补贴控件赋值
        //                txtTranSportcosts = GetTranSportcostsTextBox(txtTranSportcosts, i, FromReadOnlyDataGrid);//交通费控件赋值
        //                txtASubsidies = GetASubsidiesTextBox(txtASubsidies, i, FromReadOnlyDataGrid);//住宿费控件赋值
        //                txtOtherCosts = GetOtherCostsTextBox(txtOtherCosts, i, FromReadOnlyDataGrid);//其他费用控件赋值
        //                txtMealSubsidies = GetMealSubsidiesTextBox(txtMealSubsidies, i, FromReadOnlyDataGrid);//餐费补贴控件赋值

        //                if (totolDay > travelsolutions.MAXIMUMRANGEDAYS.ToInt32())
        //                {
        //                    if (entareaallowance != null)
        //                    {
        //                        double DbTranceport = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble();
        //                        double DbMeal = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble();
        //                        double tfSubsidies = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
        //                        double mealSubsidies = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
        //                        if (entareaallowance != null)
        //                        {
        //                            if (txtTFSubsidies != null)//交通补贴
        //                            {
        //                                if (obje.BUSINESSDAYS != null)
        //                                {
        //                                    if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                                    {
        //                                        txtTFSubsidies.Text = "0";
        //                                        txtTFSubsidies.IsReadOnly = true;
        //                                        txtTranSportcosts.IsReadOnly = true;//交通费
        //                                        txtASubsidies.IsReadOnly = true;//住宿标准
        //                                        txtOtherCosts.IsReadOnly = true;//其他费用
        //                                    }
        //                                    else if (obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
        //                                    {
        //                                        txtTFSubsidies.Text = "0";
        //                                    }
        //                                    else
        //                                    {
        //                                        if (EmployeePostLevel.ToInt32() > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                        {
        //                                            double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbTranceport;
        //                                            double middlemoney = (travelsolutions.MAXIMUMRANGEDAYS.ToDouble() - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * tfSubsidies;
        //                                            //double lastmoney = (tresult - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble() / 2;
        //                                            //除以2是因为驻外标准不分餐费和交通补贴，2者合2为一，否则会多加
        //                                            double lastmoney = (totolDay - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble() / 2;
        //                                            txtTFSubsidies.Text = (minmoney + middlemoney + lastmoney).ToString();

        //                                            //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
        //                                            if (string.IsNullOrWhiteSpace(txtTFSubsidies.Text))
        //                                            {
        //                                                ComfirmWindow com = new ComfirmWindow();
        //                                                com.OnSelectionBoxClosed += (obj, result) =>
        //                                                {
        //                                                    txtTranSportcosts.IsReadOnly = true;//交通费
        //                                                    txtASubsidies.IsReadOnly = true;//住宿标准
        //                                                    txtOtherCosts.IsReadOnly = true;//其他费用
        //                                                };
        //                                                if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //                                                {
        //                                                    if (formType == FormTypes.Audit) return;
        //                                                    com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            txtTFSubsidies.Text = "0";
        //                                            txtTFSubsidies.IsReadOnly = false;
        //                                        }
        //                                    }
        //                                }
        //                                else//如果天数为null的禁用住宿费控件
        //                                {
        //                                    txtASubsidies.IsReadOnly = true;
        //                                }
        //                            }
        //                            if (txtMealSubsidies != null)//餐费补贴
        //                            {
        //                                if (obje.BUSINESSDAYS != null)
        //                                {
        //                                    if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                                    {
        //                                        txtMealSubsidies.Text = "0";
        //                                        txtMealSubsidies.IsReadOnly = true;
        //                                        txtTranSportcosts.IsReadOnly = true;//交通费
        //                                        txtASubsidies.IsReadOnly = true;//住宿标准
        //                                        txtOtherCosts.IsReadOnly = true;//其他费用
        //                                    }
        //                                    else if (obje.GOOUTTOMEET == "1")//如果是开会
        //                                    {
        //                                        txtMealSubsidies.Text = "0";
        //                                    }
        //                                    else
        //                                    {
        //                                        if (EmployeePostLevel.ToInt32() > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                        {
        //                                            double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbMeal;
        //                                            //double middlemoney = (travelsolutions.MAXIMUMRANGEDAYS.ToDouble() - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * mealSubsidies;
        //                                            double IntMaxDays = travelsolutions.MAXIMUMRANGEDAYS.ToDouble();
        //                                            double IntMinDAys = travelsolutions.MINIMUMINTERVALDAYS.ToDouble();
        //                                            double middlemoney = (IntMaxDays - IntMinDAys) * mealSubsidies;
        //                                            //double lastmoney = (tresult - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble();
        //                                            //驻外标准：交通费和餐费补贴为一起的，所以除以2
        //                                            double lastmoney = (totolDay - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble() / 2;
        //                                            txtMealSubsidies.Text = (minmoney + middlemoney + lastmoney).ToString();

        //                                            //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
        //                                            if (string.IsNullOrWhiteSpace(txtMealSubsidies.Text))
        //                                            {
        //                                                ComfirmWindow com = new ComfirmWindow();
        //                                                com.OnSelectionBoxClosed += (obj, result) =>
        //                                                {
        //                                                    txtTranSportcosts.IsReadOnly = true;//交通费
        //                                                    txtASubsidies.IsReadOnly = true;//住宿标准
        //                                                    txtOtherCosts.IsReadOnly = true;//其他费用
        //                                                };
        //                                                if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //                                                {
        //                                                    if (formType == FormTypes.Audit) return;
        //                                                    com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            txtMealSubsidies.Text = "0";
        //                                            txtMealSubsidies.IsReadOnly = false;
        //                                        }
        //                                    }
        //                                }
        //                                else//如果天数为null的禁用住宿费控件
        //                                {
        //                                    txtASubsidies.IsReadOnly = true;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (EmployeePostLevel.ToInt32() <= travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                    {
        //                        txtTFSubsidies.Text = "0";
        //                        txtMealSubsidies.Text = "0";
        //                    }
        //                }
        //            }
        //            #endregion

        //            #region 如果出差时间大于设定的最小天数并且小于设定的最大天数的报销标准
        //            if (travelsolutions != null && employeepost != null)
        //            {
        //                TextBox txtTranSportcosts = new TextBox();//初始化交通费控件
        //                TextBox txtOtherCosts = new TextBox();//初始化其他费用控件

        //                txtTFSubsidies = GetTFSubsidiesTextBox(txtTFSubsidies, i, FromReadOnlyDataGrid);//交通补贴控件赋值
        //                txtTranSportcosts = GetTranSportcostsTextBox(txtTranSportcosts, i, FromReadOnlyDataGrid);//交通费控件赋值
        //                txtASubsidies = GetASubsidiesTextBox(txtASubsidies, i, FromReadOnlyDataGrid);//住宿费控件赋值
        //                txtOtherCosts = GetOtherCostsTextBox(txtOtherCosts, i, FromReadOnlyDataGrid);//其他费用控件赋值
        //                txtMealSubsidies = GetMealSubsidiesTextBox(txtMealSubsidies, i, FromReadOnlyDataGrid);//餐费补贴控件赋值

        //                if (totolDay >= travelsolutions.MINIMUMINTERVALDAYS.ToInt32() && totolDay <= travelsolutions.MAXIMUMRANGEDAYS.ToInt32())
        //                {
        //                    if (entareaallowance != null)
        //                    {
        //                        double DbTranceport = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble();
        //                        double DbMeal = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble();
        //                        double tfSubsidies = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
        //                        double mealSubsidies = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
        //                        if (txtTFSubsidies != null)//交通补贴
        //                        {
        //                            if (obje.BUSINESSDAYS != null)
        //                            {
        //                                if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                                {
        //                                    txtTFSubsidies.Text = "0";
        //                                    txtTFSubsidies.IsReadOnly = true;
        //                                    txtTranSportcosts.IsReadOnly = true;//交通费
        //                                    txtASubsidies.IsReadOnly = true;//住宿标准
        //                                    txtOtherCosts.IsReadOnly = true;//其他费用
        //                                }
        //                                else if (obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
        //                                {
        //                                    txtTFSubsidies.Text = "0";
        //                                }
        //                                else
        //                                {
        //                                    if (EmployeePostLevel.ToInt32() > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                    {
        //                                        double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbTranceport;
        //                                        double middlemoney = (totolDay - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * tfSubsidies;
        //                                        txtTFSubsidies.Text = (minmoney + middlemoney).ToString();

        //                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
        //                                        if (string.IsNullOrWhiteSpace(txtTFSubsidies.Text))
        //                                        {
        //                                            ComfirmWindow com = new ComfirmWindow();
        //                                            com.OnSelectionBoxClosed += (obj, result) =>
        //                                            {
        //                                                txtTranSportcosts.IsReadOnly = true;//交通费
        //                                                txtASubsidies.IsReadOnly = true;//住宿标准
        //                                                txtOtherCosts.IsReadOnly = true;//其他费用
        //                                            };
        //                                            if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //                                            {
        //                                                if (formType == FormTypes.Audit) return;
        //                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        txtTFSubsidies.Text = "0";
        //                                        txtTFSubsidies.IsReadOnly = false;
        //                                    }
        //                                }
        //                            }
        //                            else//如果天数为null的禁用住宿费控件
        //                            {
        //                                txtASubsidies.IsReadOnly = true;
        //                            }
        //                        }
        //                        if (txtMealSubsidies != null)//餐费补贴
        //                        {
        //                            if (obje.BUSINESSDAYS != null)
        //                            {
        //                                if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                                {
        //                                    txtMealSubsidies.Text = "0";
        //                                    txtMealSubsidies.IsReadOnly = true;
        //                                    txtTranSportcosts.IsReadOnly = true;//交通费
        //                                    txtASubsidies.IsReadOnly = true;//住宿标准
        //                                    txtOtherCosts.IsReadOnly = true;//其他费用
        //                                }
        //                                else if (obje.GOOUTTOMEET == "1")//如果是开会
        //                                {
        //                                    txtMealSubsidies.Text = "0";
        //                                }
        //                                else
        //                                {
        //                                    if (EmployeePostLevel.ToInt32() > travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                    {
        //                                        //最小区间段金额
        //                                        double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbMeal;
        //                                        //中间区间段金额
        //                                        double middlemoney = (totolDay - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * mealSubsidies;
        //                                        txtMealSubsidies.Text = (minmoney + middlemoney).ToString();

        //                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
        //                                        if (string.IsNullOrWhiteSpace(txtMealSubsidies.Text))
        //                                        {
        //                                            ComfirmWindow com = new ComfirmWindow();
        //                                            com.OnSelectionBoxClosed += (obj, result) =>
        //                                            {
        //                                                txtTranSportcosts.IsReadOnly = true;//交通费
        //                                                txtASubsidies.IsReadOnly = true;//住宿标准
        //                                                txtOtherCosts.IsReadOnly = true;//其他费用
        //                                            };
        //                                            if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
        //                                            {
        //                                                if (formType == FormTypes.Audit) return;
        //                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        txtMealSubsidies.Text = "0";
        //                                        txtMealSubsidies.IsReadOnly = false;
        //                                    }
        //                                }
        //                            }
        //                            else//如果天数为null的禁用住宿费控件
        //                            {
        //                                txtASubsidies.IsReadOnly = true;
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (EmployeePostLevel.ToInt32() <= travelsolutions.NOALLOWANCEPOSTLEVEL.ToInt32())//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                    {
        //                        txtTFSubsidies.Text = "0";
        //                        txtMealSubsidies.Text = "0";
        //                    }
        //                }
        //            }
        //            #endregion

        //            total += txtTFSubsidies.Text.ToDouble() + txtMealSubsidies.Text.ToDouble();
        //            this.txtSubTotal.Text = total.ToString();//总费用
        //            this.txtChargeApplyTotal.Text = total.ToString();

        //            Fees = total;
        //        }

        //        CountMoney();
        //    }
        //}
        #endregion

        #region 住宿费，交通费，其他费用
        /// <summary>
        /// 计算 住宿费，交通费，其他费用
        /// </summary>
        private void CountMoney()
        {
            try
            {
                TravelTimeCalculation();

                double totall = 0;
                //int i = 0;
                if (TravelDetailList_Golbal == null)
                {
                    return;
                }
                //住宿费，交通费，其他费用
                bool IsPassEd = false;//住宿费是否超标
                string str = string.Empty;
                foreach (var obj in TravelDetailList_Golbal)
                {

                    if (DaGrEdit.Columns[8].GetCellContent(obj) == null)
                    {
                        return;
                    }
                    if (DaGrEdit.Columns[9].GetCellContent(obj) == null)
                    {
                        return;
                    }
                    if (DaGrEdit.Columns[12].GetCellContent(obj) == null)
                    {
                        return;
                    }
                    //住宿天数
                    TextBox myDaysTime = DaGrEdit.Columns[5].GetCellContent(obj).FindName("txtTHENUMBEROFNIGHTS") as TextBox;


                    ////交通费txtTRANSPORTCOSTS
                    //TextBox txtToolubsidies = DaGrEdit.Columns[8].GetCellContent(obj).FindName("txtTRANSPORTCOSTS") as TextBox;
                    ////住宿费txtACCOMMODATION
                    //TextBox txtASubsidies = DaGrEdit.Columns[9].GetCellContent(obj).FindName("txtACCOMMODATION") as TextBox;                   
                    ////交通补贴
                    //TextBox txtTFSubsidies = DaGrEdit.Columns[10].GetCellContent(obj).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;
                    ////餐费补贴
                    //TextBox txtMealSubsidies = DaGrEdit.Columns[11].GetCellContent(obj).FindName("txtMEALSUBSIDIES") as TextBox;
                    ////其他费用                   
                    //TextBox txtOthercosts = DaGrEdit.Columns[12].GetCellContent(obj).FindName("txtOtherCosts") as TextBox;

                    //交通费第8列
                    TextBox txtToolubsidies = ((TextBox)((StackPanel)DaGrEdit.Columns[8].GetCellContent(obj)).Children.FirstOrDefault()) as TextBox;
                    //住宿费第9列
                    TextBox txtASubsidies = ((TextBox)((StackPanel)DaGrEdit.Columns[9].GetCellContent(obj)).Children.FirstOrDefault()) as TextBox;
                    //交通补贴第10列
                    TextBox txtTFSubsidies = ((TextBox)((StackPanel)DaGrEdit.Columns[10].GetCellContent(obj)).Children.FirstOrDefault()) as TextBox;
                    //餐费补贴第11列
                    TextBox txtMealSubsidies = ((TextBox)((StackPanel)DaGrEdit.Columns[11].GetCellContent(obj)).Children.FirstOrDefault()) as TextBox;
                    //其他费用第12列
                    TextBox txtOthercosts = ((TextBox)((StackPanel)DaGrEdit.Columns[12].GetCellContent(obj)).Children.FirstOrDefault()) as TextBox;
                   

                    if (txtToolubsidies != null)
                    {
                        if (!string.IsNullOrEmpty(txtToolubsidies.Text))
                        {
                            totall = totall + txtToolubsidies.Text.ToDouble();//交通费
                        }
                    }
                    if (txtASubsidies != null)
                    {
                        if (!string.IsNullOrEmpty(txtASubsidies.Text))
                        {
                            totall = totall + txtASubsidies.Text.ToDouble();//住宿费
                        }
                    }
                   
                    if (txtTFSubsidies != null)
                    {
                        if (!string.IsNullOrEmpty(txtTFSubsidies.Text))
                        {
                            totall = totall + txtTFSubsidies.Text.ToDouble();//交通补贴
                        }
                    }
                    if (txtMealSubsidies != null)
                    {
                        if (!string.IsNullOrEmpty(txtMealSubsidies.Text))
                        {
                            totall = totall + txtMealSubsidies.Text.ToDouble();// 餐费补贴
                        }
                    }
                    if (txtOthercosts != null)
                    {
                        if (!string.IsNullOrEmpty(txtOthercosts.Text))
                        {
                            totall = totall + txtOthercosts.Text.ToDouble(); //其他费用        
                        }
                    }
                   
                    //获取出差补贴
                    T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
                    string cityValue = obj.DESTCITY;//目标城市值
                    //根据城市查出差标准补贴（已根据岗位级别过滤）
                    entareaallowance = this.GetAllowanceByCityValue(cityValue);
                    if (!string.IsNullOrEmpty(obj.BUSINESSDAYS) && obj.BUSINESSDAYS != "0")
                    {
                        //住宿天数
                        double totaldays = System.Convert.ToDouble(obj.BUSINESSDAYS);
                        obj.THENUMBEROFNIGHTS = totaldays.ToString();
                        //住宿天数舍弃小数位
                        decimal days = System.Convert.ToDecimal(obj.BUSINESSDAYS);
                        if (days.ToDouble() == 0.5)
                        {
                            days = 1;
                        }
                        else
                        {
                            days = decimal.Truncate(days);
                        }
                        if (entareaallowance != null)
                        {
                            if (txtASubsidies.Text.ToDouble() > entareaallowance.ACCOMMODATION.ToDouble() * days.ToDouble())//判断住宿费超标
                            {
                                //文本框标红
                                txtASubsidies.BorderBrush = new SolidColorBrush(Colors.Red);
                                txtASubsidies.Foreground = new SolidColorBrush(Colors.Red);
                                this.txtAccommodation.Visibility = Visibility.Visible;
                                IsPassEd = true;
                                //this.txtAccommodation.Text = "住宿费超标";
                            }

                            if (txtASubsidies.Text.ToDouble() <= entareaallowance.ACCOMMODATION.ToDouble() * days.ToDouble())
                            {
                                if (txtASubsidiesForeBrush != null)
                                {
                                    txtASubsidies.Foreground = txtASubsidiesForeBrush;
                                }
                                if (txtASubsidiesBorderBrush != null)
                                {
                                    txtASubsidies.BorderBrush = txtASubsidiesBorderBrush;
                                }
                                string StrMessage = "";
                                StrMessage = this.txtAccommodation.Text;
                                if (string.IsNullOrEmpty(StrMessage))
                                {
                                    this.txtAccommodation.Visibility = Visibility.Collapsed;
                                }
                            }

                        }

                    }
                   

                    
                    //if (obj.ACCOMMODATION != null) totall = totall + Convert.ToDouble(obj.ACCOMMODATION.Value); //住宿补贴   
                    //if (obj.TRANSPORTCOSTS != null) totall = totall + Convert.ToDouble(obj.TRANSPORTCOSTS.Value);//交通费
                    //if (obj.TRANSPORTATIONSUBSIDIES != null) totall = totall + Convert.ToDouble(obj.TRANSPORTATIONSUBSIDIES.Value);//交通补贴
                    //if (obj.OTHERCOSTS != null) totall = totall + Convert.ToDouble(obj.OTHERCOSTS.Value);//其他费用      

                     //str =str+System.Environment.NewLine+ "交通费:" + obj.TRANSPORTCOSTS.Value
                     //   + " 住宿费:" + Accommodation
                     //   + " 其他费用" + obj.OTHERCOSTS.Value
                     //   + " 交通补贴" + obj.TRANSPORTATIONSUBSIDIES.Value
                     //   + " 住宿补贴" + obj.ACCOMMODATION.Value;

                    //totall = totall
                    //    +Convert.ToDouble(obj.TRANSPORTCOSTS.Value)// textTransportcosts.Text.ToDouble() //交通费
                    //    + Accommodation //住宿费
                    //    + Convert.ToDouble(obj.OTHERCOSTS.Value)//textOthercosts.Text.ToDouble()     //其他费用       
                    //    +  Convert.ToDouble(obj.TRANSPORTATIONSUBSIDIES.Value)//txtTFSubsidies.Text.ToDouble()     //交通补贴
                    //    + Convert.ToDouble(obj.ACCOMMODATION.Value);//txtMealSubsidies.Text.ToDouble();  //住宿补贴                   
                }
                if (IsPassEd)
                {
                    this.txtAccommodation.Text = "住宿费超标";
                }
                else
                {
                    this.txtAccommodation.Text = string.Empty;
                    this.txtAccommodation.Visibility = Visibility.Collapsed;
                }
                txtSubTotal.Text = totall.ToString();//差旅费小计
                if (OpenFrom != "FromMVC")
                {
                    fbCtr.TravelSubject.ApplyMoney = Convert.ToDecimal(totall);
                }
                if (fbCtr.totalMoney > 0)
                {
                    totall = totall + Convert.ToDouble(fbCtr.totalMoney);
                }
                txtChargeApplyTotal.Text = totall.ToString(); //出差报销费用合计，包括费用报销控件中的费用               
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        #endregion
    }
}
