using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.Text;

namespace is2EXsyukka 
{
	/// <summary>
	/// Global の概要の説明です。
	/// </summary>
	public class Global : System.Web.HttpApplication
	{
		private string sLogPath = "D:\\IS2EX\\ServiceLog\\";
		private static Encoding enc = Encoding.GetEncoding("shift-jis");
		private static string gsAppSrc = System.Web.HttpRuntime.AppDomainAppVirtualPath.Replace('/','_');

		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		public Global()
		{
			InitializeComponent();

//			//ログ出力用判定フラグの取得
//			System.Type type = System.Type.GetType("System.String");
//			System.Configuration.AppSettingsReader config = new System.Configuration.AppSettingsReader();
//			CommService.iLogMode = int.Parse(config.GetValue("log", type).ToString());
//			CommService.iLogMode = int.Parse(config.GetValue("log", type).ToString());
//			//ログ出力パスの取得
//			CommService.sLogPath = config.GetValue("sLogPath", type).ToString();
//			Context.Application.Add("iLogMode", CommService.iLogMode);
//			Context.Application.Add("sLogPath", CommService.sLogPath);
//
		}	
		
		protected void Application_Start(Object sender, EventArgs e)
		{
			LogOut("Application_Start");

			System.Type type = System.Type.GetType("System.String");
			System.Configuration.AppSettingsReader config = new System.Configuration.AppSettingsReader();
			//ログ出力パスの取得
			sLogPath = config.GetValue("path", type).ToString();
			Context.Application.Add("sLogPath", sLogPath);
		}

		protected void Session_Start(Object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(Object sender, EventArgs e)
		{

		}

		protected void Application_EndRequest(Object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{

		}

		protected void Application_Error(Object sender, EventArgs e)
		{
			
		}

		protected void Session_End(Object sender, EventArgs e)
		{

		}

		protected void Application_End(Object sender, EventArgs e)
		{
			LogOut("Application_End");
		}

		#region Web フォーム デザイナで生成されたコード 
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{    
			this.components = new System.ComponentModel.Container();
		}
		#endregion

		/*********************************************************************
		 * ログ出力
		 * 引数：ログ出力文字列
		 * 戻値：無し-
		 *
		 *********************************************************************/
		private void LogOut(string sLog)
		{
			System.IO.FileStream   cfs = null;
			System.IO.StreamWriter csw = null;
			try
			{
				string fileName = sLogPath 
								+ System.DateTime.Now.ToString("MMdd") 
								+ "_Global"
								+ gsAppSrc
								+ ".log"
								;

				cfs = new System.IO.FileStream(fileName, 
												System.IO.FileMode.Append, 
												System.IO.FileAccess.Write, 
												System.IO.FileShare.Write);
				csw = new System.IO.StreamWriter(cfs, enc);
				csw.Write("["+ System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") +"]");
				csw.Write("["+ System.Web.HttpRuntime.AppDomainAppVirtualPath +"]");
				csw.WriteLine(sLog);
				csw.Flush();
			}
			catch(Exception ex)
			{
				if(csw != null)
				{
					csw.WriteLine("["+ System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") +"]");
					csw.WriteLine(ex.ToString());
					csw.WriteLine(ex.Message);
					csw.WriteLine(ex.StackTrace);
					csw.Flush();
				}
			}
			finally
			{
				if(csw != null) csw.Close();
				if(cfs != null) cfs.Close();
			}
		}
	}
}

