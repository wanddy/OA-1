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

using SMT.Saas.Tools.PermissionWS;
using System.Linq;

using System.Collections.Generic;
using SMT.SAAS.ClientUtility;

namespace SMT.SaaS.OA.UI.Class
{
    public class TravelDictionaryComboBox : ComboBox
    {
        public DependencyProperty SelectedValueProperty;
        public DependencyProperty CategoryProperty;
        public DependencyProperty IsShowNullProperty;

        public string SelectedValue
        {
            get
            {
                return GetValue(SelectedValueProperty) as string;

            }
            set
            {
                SetValue(SelectedValueProperty, value);

            }
        }
        public bool IsShowNull
        {
            get
            {
                return (bool)GetValue(IsShowNullProperty);

            }
            set
            {
                SetValue(IsShowNullProperty, value);

            }
        }
        public string Category
        {
            get
            {
                return GetValue(CategoryProperty) as string;
            }
            set
            {
                SetValue(CategoryProperty, value);
            }
        }
        public TravelDictionaryComboBox()
        {
            IsShowNullProperty = DependencyProperty.Register("IsShowNull", typeof(bool), typeof(TravelDictionaryComboBox)
                , new PropertyMetadata(true, new PropertyChangedCallback(TravelDictionaryComboBox.OnIsShowNullChanged)));

            SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(string), typeof(TravelDictionaryComboBox)
              , new PropertyMetadata("", new PropertyChangedCallback(TravelDictionaryComboBox.OnSelectedValuePropertyChanged)));

            CategoryProperty = DependencyProperty.Register("Category", typeof(string), typeof(TravelDictionaryComboBox)
   , new PropertyMetadata("", new PropertyChangedCallback(TravelDictionaryComboBox.OnCategoryPropertyChanged)));
        }
        public static void OnSelectedValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TravelDictionaryComboBox obj = sender as TravelDictionaryComboBox;
            if (obj != null)
            {
                obj.OnSelectedValuePropertyChanged(e);
            }
        }
        public static void OnCategoryPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TravelDictionaryComboBox obj = sender as TravelDictionaryComboBox;
            if (obj != null)
            {
                obj.OnCategoryPropertyChanged(e);
            }
        }
        public static void OnIsShowNullChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TravelDictionaryComboBox obj = sender as TravelDictionaryComboBox;
            if (obj != null)
            {
                obj.OnIsShowNullChanged(e);
            }
        }
        private void OnCategoryPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            BindItems(e.NewValue == null ? "" : e.NewValue.ToString());
        }
        private void OnIsShowNullChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                IsShowNull = (bool)e.NewValue;
            }
        }
        private void BindItems(string cate)
        {
            try
            {
                DictionaryManager dm = new DictionaryManager();
                dm.OnDictionaryLoadCompleted += (o, e) =>
                {
                    List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
                    BindComboBox(dicts, cate, SelectedValue);
                };
                dm.LoadDictionary(cate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 可用于动态加载字典
        /// </summary>
        /// <param name="dicts"></param>
        /// <param name="category"></param>
        /// <param name="defaultValue"></param>
        public void BindComboBox(List<T_SYS_DICTIONARY> dicts, string category, string defaultValue)
        {
            var objs = from d in dicts
                       where d.DICTIONCATEGORY == category
                       orderby d.DICTIONARYVALUE
                       select d;
            List<T_SYS_DICTIONARY> tmpDicts = objs.ToList();

            if (IsShowNull)
            {
                T_SYS_DICTIONARY nuldict = new T_SYS_DICTIONARY();
                string dictname = Utility.GetResourceStr("PLEASESELECTED");

                nuldict.DICTIONARYNAME = dictname;
                nuldict.DICTIONARYVALUE = null;
                tmpDicts.Insert(0, nuldict);
            }
           
            ItemsSource = tmpDicts;
            DisplayMemberPath = "DICTIONARYNAME";
            SelectedIndex = 0; // zwp 默认选中“请选择"

            SetValue(SelectedValueProperty, defaultValue);

        }
        private void OnSelectedValuePropertyChanged(DependencyPropertyChangedEventArgs e)
        {

            SetSelectedItem(e.NewValue == null ? "" : e.NewValue.ToString());
        }
        private void SetSelectedItem(string value)
        {
            foreach (var item in Items)
            {
                T_SYS_DICTIONARY obj = item as T_SYS_DICTIONARY;
                if (obj != null)
                {
                    if (obj.DICTIONARYVALUE.ToString() == value && obj.DICTIONARYNAME != "空")
                    {
                        SelectedItem = item;
                        break;
                    }
                }
            }
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);

            T_SYS_DICTIONARY dict = this.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SelectedValue = dict.DICTIONARYVALUE.ToString();
            }
        }
    }
}
