﻿/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：LogHelper.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/7/6 14:33:55   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.SaaS.SMS.EFModel 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace SMT.Workflow.Common.DataAccess
{
    /// <summary>
    ///   日志辅助类
    /// </summary>
    public class LogHelper
    {      
        #region 日志（龙康才新增）
        private static string currentDir = Environment.CurrentDirectory;
        //string str = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        private static string hostDir = System.AppDomain.CurrentDomain.BaseDirectory + "Logs\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\";

        /// <summary>
        /// 创建目录(如果不存在就创建)
        /// </summary>
        /// <param name="dirPath">目录路径</param>
        public static void CreateDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        /// <summary>
        /// 创建文件(如果不存在就创建)
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void CreateFile(string filePath)
        {
            int index = filePath.LastIndexOf("\\");
            string dir = filePath.Substring(0, index);
            string dirPath = dir;
            if (!File.Exists(filePath))
            {
                CreateDirectory(dirPath);
                File.Create(filePath);
            }
        }
        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="code">编码</param>
        /// <returns>是否成功</returns>
        public static bool WriteFile(string content, Encoding code)
        {
            string dirPath = hostDir;
            if (!Directory.Exists(dirPath))
            {
                CreateDirectory(dirPath);

            }
            try
            {
                string fileSavePath = dirPath + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                using (FileStream aFile = new FileStream(fileSavePath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(aFile, code))
                    {
                        sw.Write(content);
                        return true;
                    }
                }
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="fileSavePath">路径</param>
        /// <param name="code">编码</param>
        /// <returns>是否成功</returns>
        public static bool WriteFile(string content, string fileSavePath, Encoding code)
        {
            int index = fileSavePath.LastIndexOf("\\");
            string dir = fileSavePath.Substring(0, index);
            string dirPath = dir;
            if (!Directory.Exists(dirPath))
            {
                CreateDirectory(dirPath);

            }
            try
            {
                using (FileStream aFile = new FileStream(fileSavePath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(aFile, code))
                    {
                        sw.Write(content);
                        return true;
                    }
                }
            }
            catch (System.Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="fileSavePath">路径</param>
        /// <param name="code">编码</param>
        /// <returns>是否成功</returns>
        public static bool WriteFile(string content, string fileSavePath)
        {
            int index = fileSavePath.LastIndexOf("\\");
            string dir = fileSavePath.Substring(0, index);
            string dirPath = dir;
            if (!Directory.Exists(dirPath))
            {
                CreateDirectory(dirPath);

            }
            try
            {

                using (FileStream aFile = new FileStream(fileSavePath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(aFile, System.Text.Encoding.Default))
                    {
                        sw.Write(content);
                        return true;
                    }
                }
            }
            catch (System.Exception)
            {
                return false;
            }

        }
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return "";
            }
            if (File.Exists(filePath))
            {
                try
                {
                    using (FileStream aFile = new FileStream(filePath, FileMode.Open))
                    {
                        using (StreamReader sw = new StreamReader(aFile, System.Text.Encoding.Default))
                        {
                            return sw.ReadToEnd();
                        }
                    }
                }
               catch (System.Exception e)
                {
                    //MessageBox.Show(e.Message);
                    return "";
                }
            }
            else
            {
                return "找不到文件路径:" + filePath;
            }
            //System.IO.StreamReader rd = System.IO.File.OpenText(FilePath);
            //string StrRead = rd.ReadToEnd().ToString();
            //rd.Close();
            //return StrRead;
        }
        public static string ReadFile(string filePath, Encoding code)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return "";
            }
            if (File.Exists(filePath))
            {
                try
                {
                    using (FileStream aFile = new FileStream(filePath, FileMode.Open))
                    {
                        using (StreamReader sw = new StreamReader(aFile, code))
                        {
                            return sw.ReadToEnd();
                        }
                    }
                }
               catch (System.Exception e)
                {
                    //MessageBox.Show(e.Message);
                    return "";
                }
            }
            else
            {
                return "找不到文件路径:" + filePath;
            }
            //System.IO.StreamReader rd = System.IO.File.OpenText(FilePath);
            //string StrRead = rd.ReadToEnd().ToString();
            //rd.Close();
            //return StrRead;
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool DeleteFile(string filePath)
        {
            bool bol = false;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                bol = true;
            }
            return bol;
        }

        #region 写日志
        /// <summary>
        /// 写日志,最新的内容写在最前面
        /// </summary>
        /// <param name="obj">当前发生的对象（如：类对象this）</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="content">内容</param>
        /// <param name="e">Exception</param>
        /// <returns></returns>
        public static bool WriteLogFrist(object obj, string methodName, string content, System.Exception e)
        {
            string dirPath = hostDir;
            if (!Directory.Exists(dirPath))
            {
                CreateDirectory(dirPath);

            }
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("------------------------------------------------------------------------------------------------------------------------------");
                sb.AppendLine("时间:" + DateTime.Now.ToString());
                sb.AppendLine("DLL:" + obj.GetType().Module.Name);
                sb.AppendLine("类名:" + obj.GetType().FullName);
                sb.AppendLine("方法:" + methodName);
                sb.AppendLine("内容:" + content);
                if (e != null)
                {
                    sb.AppendLine("异常:" + (e.InnerException != null ? e.InnerException.Message : e.Message));
                }
                string fileSavePath = dirPath + "/" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt";
                string oldConent = "";
                using (FileStream aFile = new FileStream(fileSavePath, FileMode.OpenOrCreate))
                {
                    using (StreamReader sr = new StreamReader(aFile, Encoding.UTF8))
                    {
                        oldConent = sr.ReadToEnd();
                    }
                }
                using (FileStream aFile = new FileStream(fileSavePath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(aFile, Encoding.UTF8))
                    {
                        sw.Write(sb.ToString() + oldConent);
                        return true;
                    }
                }
            }
            catch (System.Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="content">内容</param>
        /// <returns></returns>
        public static bool WriteLog(string content)
        {

            string dirPath = hostDir;
            if (!Directory.Exists(dirPath))
            {
                CreateDirectory(dirPath);

            }
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("------------------------------------------------------------------------------------------------------------------------------");
                sb.AppendLine("发生时间：" + DateTime.Now.ToString() );
                sb.AppendLine("日志内容:" + content);
                string fileSavePath = dirPath + "/" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt";
                using (FileStream aFile = new FileStream(fileSavePath, FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(aFile, Encoding.UTF8))
                    {
                        sw.Write(sb.ToString());
                        return true;
                    }
                }
            }
            catch (System.Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="fileName">文件名如:myname</param>
        /// <param name="content">日志内容</param>
        /// <returns></returns>
        public static bool WriteLog(string fileName,string content)
        {

            string dirPath = hostDir;
            if (!Directory.Exists(dirPath))
            {
                CreateDirectory(dirPath);

            }
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("------------------------------------------------------------------------------------------------------------------------------");
                sb.AppendLine("发生时间：" + DateTime.Now.ToString());
                sb.AppendLine("日志内容:" + content);
                string fileSavePath = dirPath + "/"+fileName+".txt";
                using (FileStream aFile = new FileStream(fileSavePath, FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(aFile, Encoding.UTF8))
                    {
                        sw.Write(sb.ToString());
                        return true;
                    }
                }
            }
            catch (System.Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="obj">当前发生的对象（如：类对象this）</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="content">内容</param>
        /// <param name="e">Exception</param>
        /// <returns></returns>
        public static bool WriteLog(string methodName, string content, System.Exception e)
        {
         
           string dirPath = hostDir;
            if (!Directory.Exists(dirPath))
            {
                CreateDirectory(dirPath);

            }
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("------------------------------------------------------------------------------------------------------------------------------");
                sb.AppendLine("时间:" + DateTime.Now.ToString());  
                sb.AppendLine("方法:" + methodName);
                if (!string.IsNullOrEmpty(content))
                {
                    sb.AppendLine("内容:" + content);
                }
                if (e != null)
                {
                    sb.AppendLine("异常:" + (e.InnerException != null ? e.InnerException.Message : e.Message));
                }
                string fileSavePath = dirPath + "/" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt";              
                using (FileStream aFile = new FileStream(fileSavePath, FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(aFile, Encoding.UTF8))
                    {
                        sw.Write(sb.ToString() );
                        return true;
                    }
                }
            }
            catch (System.Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="obj">当前发生的对象（如：类对象this）</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="content">内容</param>
        /// <param name="e">Exception</param>
        /// <returns></returns>
        public static bool WriteLog(object obj, string methodName, string content, System.Exception e)
        {

            string dirPath = hostDir;
            if (!Directory.Exists(dirPath))
            {
                CreateDirectory(dirPath);

            }
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("------------------------------------------------------------------------------------------------------------------------------");
                sb.AppendLine("时间:" + DateTime.Now.ToString());
                sb.AppendLine("DLL:" + obj.GetType().Module.Name);
                sb.AppendLine("类名:" + obj.GetType().FullName);
                sb.AppendLine("方法:" + methodName);
                if (!string.IsNullOrEmpty(content))
                {
                    sb.AppendLine("内容:" + content);
                }
                if (e != null)
                {
                    sb.AppendLine("异常:" + (e.InnerException != null ? e.InnerException.Message : e.Message));
                }
                string fileSavePath = dirPath + "/" + DateTime.Now.ToString("yyyy-MM-dd HH") + ".txt";
                using (FileStream aFile = new FileStream(fileSavePath, FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(aFile, Encoding.UTF8))
                    {
                        sw.Write(sb.ToString());
                        return true;
                    }
                }
            }
            catch (System.Exception)
            {
                return false;
            }
        }
        #endregion
        #endregion
    }
}
