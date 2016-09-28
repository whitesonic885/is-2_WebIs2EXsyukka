using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.Text;

namespace is2EXsyukka 
{
	/// <summary>
	/// Global �̊T�v�̐����ł��B
	/// </summary>
	public class Global : System.Web.HttpApplication
	{
		private string sLogPath = "D:\\IS2EX\\ServiceLog\\";
		private static Encoding enc = Encoding.GetEncoding("shift-jis");
		private static string gsAppSrc = System.Web.HttpRuntime.AppDomainAppVirtualPath.Replace('/','_');

		/// <summary>
		/// �K�v�ȃf�U�C�i�ϐ��ł��B
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		public Global()
		{
			InitializeComponent();

//			//���O�o�͗p����t���O�̎擾
//			System.Type type = System.Type.GetType("System.String");
//			System.Configuration.AppSettingsReader config = new System.Configuration.AppSettingsReader();
//			CommService.iLogMode = int.Parse(config.GetValue("log", type).ToString());
//			CommService.iLogMode = int.Parse(config.GetValue("log", type).ToString());
//			//���O�o�̓p�X�̎擾
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
			//���O�o�̓p�X�̎擾
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

		#region Web �t�H�[�� �f�U�C�i�Ő������ꂽ�R�[�h 
		/// <summary>
		/// �f�U�C�i �T�|�[�g�ɕK�v�ȃ��\�b�h�ł��B���̃��\�b�h�̓��e��
		/// �R�[�h �G�f�B�^�ŕύX���Ȃ��ł��������B
		/// </summary>
		private void InitializeComponent()
		{    
			this.components = new System.ComponentModel.Container();
		}
		#endregion

		/*********************************************************************
		 * ���O�o��
		 * �����F���O�o�͕�����
		 * �ߒl�F����-
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

