using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using Oracle.DataAccess.Client;



namespace is2EXsyukka
{
	/// <summary>
	/// [CommService]
	/// </summary>
	//--------------------------------------------------------------------------
	// �C������
	//--------------------------------------------------------------------------
	// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j��
	//	disposeReader(reader);
	//	reader = null;
	//--------------------------------------------------------------------------
	// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
	//	logFileOpen(sUser);
	//	userCheck2(conn2, sUser);
	//	logFileClose();
	//--------------------------------------------------------------------------
	// ADD 2007.10.19 ���s�j���� �[���o�[�W�����Ǘ�
	//--------------------------------------------------------------------------
	// MOD 2009.12.24 ���s�j���� wakeupDB�̍����� 
	// MOD 2009.12.24 ���s�j���� �ڑ����g���C�@�\�̒ǉ� 
	// MOD 2009.12.24 ���s�j���� �o�[�W�����`�F�b�N�ݒ�l�̒ǉ� 
	// MOD 2009.12.24 ���s�j���� �C�x���g���O�̔p�~ 
	//--------------------------------------------------------------------------
	// MOD 2010.01.25 ���s�j���� �ڑ����g���C�@�\�̒ǉ� 
	// MOD 2010.02.19 ���s�j���� �ڑ����g���C�@�\�̒��� 
	// MOD 2010.06.17 ���s�j���� �ڑ����g���C�@�\�̒��� 
	//�ۗ� MOD 2010.06.17 ���s�j���� �n�r���̎擾�@�\�̒ǉ� 
	// MOD 2010.08.16 ���s�j���� ORA-12152 �Ή��̒ǉ� 
	//==========================================================================
	// 2010.11.17 KCL�j���q �G�R�[�����a�����Ƃ��Ď捞���R�����g�s�̍폜
	//==========================================================================
	//
	public class CommService : System.Web.Services.WebService
	{
		private const string s�c�a�ʐM�G���[
						= "�T�[�o�����G���Ă��܂��B���b��ɍēx���s���ĉ������B";
		private const string s�c�a��Ӑ���G���[
						= "����̃R�[�h�����ɑ��̒[�����o�^����Ă��܂��B\r\n"
						+ "�ēx�A�ŐV�f�[�^���Ăяo���čX�V���Ă��������B";

		protected static string sConn    = "";
		protected static int    iLogMode = 0;		//���O���[�h
		protected static string sLogPath = "";
		protected static int    iRetry   = 0; //���g���C�񐔂̐ݒ�i�����l�F�O�j
		protected static string sMinVer  = "2.7";
		protected static string[] sSvUser = new string[]{"","",""};

		protected const int ERR = 1;
		protected const int INF = 2;
		protected const int INF_SQL = 3;


		// �v�����T�[�r�X�ϐ�
		private static is2EXlogout.Service1 sv_logout = null;

		private static Encoding enc = Encoding.GetEncoding("shift-jis");

		public CommService()
		{
			//CODEGEN: ���̌Ăяo���́AASP.NET Web �T�[�r�X �f�U�C�i�ŕK�v�ł��B
			InitializeComponent();
		}

		#region �R���|�[�l���g �f�U�C�i�Ő������ꂽ�R�[�h 
		
		//Web �T�[�r�X �f�U�C�i�ŕK�v�ł��B
		private IContainer components = null;
				
		/// <summary>
		/// �f�U�C�i �T�|�[�g�ɕK�v�ȃ��\�b�h�ł��B���̃��\�b�h�̓��e��
		/// �R�[�h �G�f�B�^�ŕύX���Ȃ��ł��������B
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// �g�p����Ă��郊�\�[�X�Ɍ㏈�������s���܂��B
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		/*********************************************************************
		 * �X�^���o�C��ԉ��p���\�b�h
		 * �����F�Ȃ�
		 * �ߒl�F�Ȃ�
		 *
		 *********************************************************************/

		private static string WAKEUPDB_SELECT
			= "SELECT 1 FROM DUAL";
		[WebMethod]
		public string wakeupDB()
		{
			// ���O�o�̓��\�b�h�̋N��
			if(sv_logout == null)
			{
				try
				{
					sv_logout = new is2EXlogout.Service1();
					// Timeout�̃f�t�H���g��100000�i�P�O�O�b�j
					sv_logout.Timeout = 1000; // �P�b
					int iRet = sv_logout.LogOut("");
				}
				catch (Exception)
				{
					//�G���[�͖�������
				}
			}
			OracleConnection conn2 = null;
			string sRet = "";
			string[] sUser = {"wakeupDB","",""};

			if(sConn.Length == 0){
				Object obj = Context.Application.Get("sConn");
				if(obj != null){
					sConn = (string)obj;
				}else{
					return "wakeupDB�F�ڑ��G���[�P";
				}
			}

			try
			{
				conn2 = new OracleConnection(sConn);
				conn2.Open();

				OracleCommand cmd = new OracleCommand(WAKEUPDB_SELECT);
				cmd.Connection  = conn2;
				cmd.CommandType = CommandType.Text;

				cmd.Prepare();
				OracleDataReader reader = cmd.ExecuteReader();
				cmd.Dispose();
				cmd    = null;

				disposeReader(reader);
				
				reader = null;
			}
			catch (OracleException ex)
			{
				sRet = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet);
			}
			finally
			{
				if(conn2 != null){
					if(conn2.State == ConnectionState.Open){
						try{
							conn2.Close();
						}catch (Exception ex){
							logWriter(sUser, ERR, "wakeupDB�F�ؒf�G���[�F" + ex.Message);
						}
					}else{
						logWriter(sUser, INF, "wakeupDB�FState:" + conn2.State);
					}

					try{
						conn2.Dispose();
					}catch (Exception ex){
						logWriter(sUser, ERR, "wakeupDB�F�j���G���[�F" + ex.Message);
					}
					conn2 = null;
				}
			}

			return sRet;
		}

		[WebMethod]
		public string wakeupDB2(int iConCnt)
		{
			// ���O�o�̓��\�b�h�̋N��
			if(sv_logout == null)
			{
				try
				{
					sv_logout = new is2EXlogout.Service1();
					// Timeout�̃f�t�H���g��100000�i�P�O�O�b�j
					sv_logout.Timeout = 1000; // �P�b
					int iRet = sv_logout.LogOut("");
				}
				catch (Exception)
				{
					//�G���[�͖�������
				}
			}

			OracleConnection[] conWakeup = new OracleConnection[iConCnt];
			string sRet = "";
			string[] sUser = {"wakeupDB2","",""};

			if(sConn.Length == 0){
				Object obj = Context.Application.Get("sConn");
				if(obj != null){
					sConn = (string)obj;
				}else{
					return "wakeupDB2�F�ڑ��G���[�P";
				}
			}

			try
			{
				OracleCommand cmd;
				OracleDataReader reader;
				for(int iCnt = 0; iCnt < iConCnt; iCnt++){
					try
					{

						conWakeup[iCnt] = null;
						conWakeup[iCnt] = new OracleConnection(sConn);
						conWakeup[iCnt].Open();

						cmd = new OracleCommand(GET_AUDSID);
						cmd.Connection  = conWakeup[iCnt];
						cmd.CommandType = CommandType.Text;

						cmd.Prepare();
						reader = cmd.ExecuteReader();
						if(reader.Read()){
							logWriter(sUser, INF, "wakeupDB2�F�iAUDSID:"+reader.GetDecimal(0)+"�j");
						}

						cmd.Dispose();
						cmd    = null;

						disposeReader(reader);
						reader = null;

					}
					catch (OracleException ex)
					{
						sRet = chgDBErrMsg(sUser, ex);
						if(ex.Number == 00028){
							; // �Z�b�V�����͋����I������܂���
						}else if(ex.Number == 01012){
							; // ���O�I������Ă��܂���
						}else if(ex.Number == 03113){
							; // �ʐM�`���l���Ńt�@�C���̏I��肪���o����܂���
						}else if(ex.Number == 03114){
							; // Oracle �ɐڑ�����Ă��܂���
						}else if(ex.Number == 03135){
							; // �ڑ��������܂���
						}else if(ex.Number == 12152){
							; // TNS: �u���[�N�E���b�Z�[�W�̑��M�Ɏ��s���܂����B
						}else if(ex.Number == 12571){
							; // TNS: �p�P�b�g�E���C�^�[�ɏ�Q���������܂���
						}else{
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				sRet = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet);
			}
			finally
			{
				for(int iCnt = 0; iCnt < iConCnt; iCnt++){
					if(conWakeup[iCnt] == null) continue;
					if(conWakeup[iCnt].State == ConnectionState.Open){
						try{
							conWakeup[iCnt].Close();
						}catch (Exception ex){
							logWriter(sUser, ERR, "wakeupDB2�F�ؒf�G���[�F" + ex.Message);
						}
					}else{
						logWriter(sUser, INF, "wakeupDB2�FState:" + conWakeup[iCnt].State);
					}

					try{
						conWakeup[iCnt].Dispose();
					}catch (Exception ex){
						logWriter(sUser, ERR, "wakeupDB2�F�j���G���[�F" + ex.Message);
					}
					conWakeup[iCnt] = null;
				}
			}

			return sRet;
		}

		/*********************************************************************
		 * �R�l�N�g�T�[�r�X
		 * �����F�Ȃ�
		 * �ߒl�F�Ȃ�
		 *
		 *********************************************************************/
		protected void connectService()
		{
			System.Type type = System.Type.GetType("System.String");
			System.Configuration.AppSettingsReader config = new System.Configuration.AppSettingsReader();
//			Object obj = null;
			iLogMode = int.Parse(config.GetValue("iLogMode", type).ToString());
			sLogPath = config.GetValue("Path", type).ToString();
			iRetry   = int.Parse(config.GetValue("iretry", type).ToString());
			


//			obj = Context.Application.Get("iLogMode");
//			if(obj != null) iLogMode = (int)obj;
//			obj = Context.Application.Get("sLogPath");
//			if(obj != null) sLogPath = (string)obj;
//			obj = Context.Application.Get("sConn");
//			if(obj != null) sConn    = (string)obj;
//			obj = Context.Application.Get("iRetry");
//			if(obj != null) iRetry = (int)obj;
//			obj = Context.Application.Get("sMinVer");
//			if(obj != null) sMinVer = (string)obj;
		}


		/*********************************************************************
		 * �c�a�ڑ�
		 * �����F�Ȃ�
		 * �ߒl�FOracleConnection
		 *
		 *********************************************************************/
		private static string GET_AUDSID
			= "SELECT USERENV('SESSIONID') FROM DUAL";
		protected OracleConnection connect2(string[] sUser)
		{
			OracleConnection conn2 = null;

			if(sConn.Length == 0)
			{
				Object obj = Context.Application.Get("sConn");
				if(obj != null)
					sConn = (string)obj;
				else
					return conn2;
			}

			try
			{
				if(iRetry == 0){
					conn2 = new OracleConnection(sConn);
				}else{
					int iCon = 0;
					OracleConnection[] conRetry = new OracleConnection[iRetry];
					for(iCon = 0; iCon < iRetry; iCon++){
						conRetry[iCon] = null;
						try{
							conRetry[iCon] = new OracleConnection(sConn);
							conRetry[iCon].Open();
							OracleCommand cmd;
							OracleDataReader reader;
							cmd = new OracleCommand(GET_AUDSID);
							cmd.Connection  = conRetry[iCon];
							cmd.CommandType = CommandType.Text;

							cmd.Prepare();
							reader = cmd.ExecuteReader();
							if(reader.Read()){
								//���g���C���̂݃��O�o��
								if(iCon > 0){
									logWriter(sUser, INF, "connect2["+iCon+"]�iAUDSID:"+reader.GetDecimal(0)+"�j");
								}
							}
							cmd.Dispose();
							cmd    = null;
							disposeReader(reader);
							reader = null;
							//�ڑ��ɐ��������ꍇ�ɂ́A�ڑ���ێ����ă��[�v����ʂ���
							conn2 = conRetry[iCon];
							//���g���C���̂݃��O�o��
							if(iCon > 0){
								logWriter(sUser, INF, "connect2["+iCon+"]����");
							}
							break;
						}catch (OracleException ex){
							logWriter(sUser, ERR, "connect2["+iCon+"]�I�[�v���F" + ex.Message);
							if(ex.Number == 00028){
								; // �Z�b�V�����͋����I������܂���
							}else if(ex.Number == 01012){
								; // ���O�I������Ă��܂���
							}else if(ex.Number == 03113){
								; // �ʐM�`���l���Ńt�@�C���̏I��肪���o����܂���
							}else if(ex.Number == 03114){
								; // Oracle �ɐڑ�����Ă��܂���
							}else if(ex.Number == 03135){
								; // �ڑ��������܂���
							}else if(ex.Number == 12152){
								; // TNS: �u���[�N�E���b�Z�[�W�̑��M�Ɏ��s���܂����B
							}else if(ex.Number == 12571){
								; // TNS: �p�P�b�g�E���C�^�[�ɏ�Q���������܂���
							}else{
								break;
							}
						}catch (Exception ex){
							logWriter(sUser, ERR, "connect2["+iCon+"]�I�[�v���F" + ex.Message);
						}
						System.Threading.Thread.Sleep(10000);	//�P�O�b�҂�
					}
					//���������ڑ��ȊO�́A�ؒf���j������
					for(iCon = 0; iCon < iRetry; iCon++){
						if(conRetry[iCon] == null)  continue;
						if(conRetry[iCon] == conn2) continue;

						if(conRetry[iCon].State == ConnectionState.Open){
							try{
								logWriter(sUser, INF, "connect2["+iCon+"]�N���[�Y");
								conRetry[iCon].Close();
							}catch (Exception ex){
								logWriter(sUser, ERR, "connect2["+iCon+"]�N���[�Y�F" + ex.Message);
							}
						}

						try{
							logWriter(sUser, INF, "connect2["+iCon+"]�j��");
							conRetry[iCon].Dispose();
						}catch (Exception ex){
							logWriter(sUser, ERR, "connect2["+iCon+"]�j���F" + ex.Message);
						}
						conRetry[iCon] = null;
					}
				}
				try
				{
					if(conn2.State == ConnectionState.Closed){
						conn2.Open();
					}
					if(conn2.State != ConnectionState.Open)
						logWriter(sUser, INF, "conn2.State:" + conn2.State);
					// �c�a�I�[�v��������Ă��Ȃ��ꍇ�ɂ͑҂�
					for(int iWait = 0; conn2.State != ConnectionState.Open && iWait < 20; iWait++)
					{
						logWriter(sUser, INF, "�c�a�I�[�v���҂�");
						System.Threading.Thread.Sleep(3000);
						logWriter(sUser, INF, "conn2.State:" + conn2.State);
					}
				}
				catch (InvalidOperationException ex) 
				{
						logWriter(sUser, ERR, "�ڑ��G���[�F" + ex.Message);

					// �c�a�ؒf
					disconnect2(sUser, conn2);
					// �c�a�ڑ�
					if(conn2.State != ConnectionState.Closed)
						logWriter(sUser, INF, "conn2.State:" + conn2.State);
					if(conn2.State == ConnectionState.Closed){
						conn2.Open();
					}
					if(conn2.State != ConnectionState.Open)
						logWriter(sUser, INF, "conn2.State:" + conn2.State);
					// �c�a�I�[�v��������Ă��Ȃ��ꍇ�ɂ͑҂�
					for(int iWait = 0; conn2.State != ConnectionState.Open && iWait < 20; iWait++)
					{
						logWriter(sUser, INF, "�c�a�I�[�v���҂�");
						System.Threading.Thread.Sleep(3000);
						logWriter(sUser, INF, "conn2.State:" + conn2.State);
					}
				}
			}
			catch (Exception ex) 
			{
				{
					logWriter(sUser, ERR, "�ڑ��G���[�F" + ex.Message);
				}
				return null;
			}

			return conn2;
		}

		/*********************************************************************
		 * �c�a�ؒf
		 * �����FOracleConnection conn
		 * �ߒl�F�Ȃ�
		 *
		 *********************************************************************/
		protected void disconnect2(string[] sUser, OracleConnection conn2)
		{
			if (conn2 == null) return;

			try
			{
				if(conn2.State != ConnectionState.Open)
					logWriter(sUser, INF, "conn2.State:" + conn2.State);
				conn2.Close();
				if(conn2.State != ConnectionState.Closed)
					logWriter(sUser, INF, "conn2.State:" + conn2.State);
			}
			catch (Exception ex) 
			{
				{
					logWriter(sUser, ERR, "�ؒf�G���[�F" + ex.Message);
				}
			}

			try
			{
				conn2.Dispose();
			}
			catch (Exception ex) 
			{
				logWriter(sUser, ERR, "�j���G���[�F" + ex.Message);
			}
			conn2 = null;
		}

		/*********************************************************************
		 * �I���N���̃G���[���b�Z�[�W�̕ϊ�
		 * �����F�I���N���G�N�Z�v�V����
		 * �ߒl�F�G���[���b�Z�[�W
		 *
		 *********************************************************************/
		protected string chgDBErrMsg(string[] sUser, OracleException ex)
		{
			string sRet = "";
			switch(ex.Number)
			{
				case    1:	// ��Ӑ���istring.string�j�ɔ����Ă��܂�
					sRet = s�c�a��Ӑ���G���[;
					break;
				case 3113:	// �ʐM�`���l���Ńt�@�C���̏I��肪���o����܂����B
					sRet = s�c�a�ʐM�G���[;
					break;
				case 3114:	// Oracle �ɐڑ�����Ă��܂���B
					sRet = s�c�a�ʐM�G���[;
					break;
				case 3135:	// �ڑ��������܂���
					sRet = s�c�a�ʐM�G���[;
					break;
				case 12571:	// TNS: �p�P�b�g�E���C�^�[�ɏ�Q���������܂���
					sRet = s�c�a�ʐM�G���[;
					break;
				default:
					sRet = "�c�a�G���[�F" + ex.Message;
					break;
			}
			logWriter(sUser, ERR, sRet);

			return sRet;
		}
		/*********************************************************************
		 * ����`�F�b�N
		 * �����F�����F����b�c�A���p�҂b�c�A�[���h�c
		 * �ߒl�F�G���[���b�Z�[�W
		 *
		 *********************************************************************/

		/*********************************************************************
		 * �r�d�k�d�b�s�̎��s�iString�Łj
		 * �����F�c�a�R�l�N�V�����A���s�r�p�k
		 * �ߒl�F�I���N���q����������
		 *
		 *********************************************************************/
		protected OracleDataReader CmdSelect(string[] sUser, OracleConnection connSelect, string sSQL)
		{
			logWriter(sUser, INF_SQL, "\n" + sSQL);

			try
			{
				OracleCommand cmd = new OracleCommand(sSQL);
				cmd.Connection = connSelect;
				cmd.CommandType = CommandType.Text;

				cmd.Prepare();
				OracleDataReader reader = cmd.ExecuteReader();
				cmd.Dispose();

				return reader;
			}
			catch (OracleException ex)
			{
				logWriter(sUser, ERR, "\n" + sSQL);
				logWriter(sUser, ERR, "�G���[�ԍ��F" + ex.Number);
				logWriter(sUser, ERR, "StackTrace:\n" + ex.StackTrace);
				throw ex;
			}
		}

		/*********************************************************************
		 * �t�o�c�`�s�d�̎��s�iString�Łj
		 * �����F�c�a�R�l�N�V�����A���s�r�p�k
		 * �ߒl�F�X�V����
		 *
		 *********************************************************************/
		protected int CmdUpdate(string[] sUser, OracleConnection connUpdate, string sSQL)
		{
			logWriter(sUser, INF_SQL, "\n" + sSQL);

			try
			{
				OracleCommand cmd = new OracleCommand(sSQL);
				cmd.Connection = connUpdate;
				cmd.CommandType = CommandType.Text;

				cmd.Prepare();
				int iUpdRow = cmd.ExecuteNonQuery();
				cmd.Dispose();

				return iUpdRow;
			}
			catch (OracleException ex)
			{
				logWriter(sUser, ERR, "\n" + sSQL);
				logWriter(sUser, ERR, "�G���[�ԍ��F" + ex.Number);
				logWriter(sUser, ERR, "StackTrace:\n" + ex.StackTrace);
				throw ex;
			}
		}

		/*********************************************************************
		 * �r�d�k�d�b�s�̎��s�iStringBuilder�Łj
		 * �����F�c�a�R�l�N�V�����A���s�r�p�k
		 * �ߒl�F�I���N���q����������
		 *
		 *********************************************************************/
		protected OracleDataReader CmdSelect(string[] sUser, OracleConnection connSelect, StringBuilder sSQL)
		{
			logWriter(sUser, INF_SQL, "\n" + sSQL.ToString());

			try
			{
				OracleCommand cmd = new OracleCommand(sSQL.ToString());
				cmd.Connection = connSelect;
				cmd.CommandType = CommandType.Text;

				cmd.Prepare();
				OracleDataReader reader = cmd.ExecuteReader();
				cmd.Dispose();

				return reader;
			}
			catch (OracleException ex)
			{
				logWriter(sUser, ERR, "\n" + sSQL.ToString());
				logWriter(sUser, ERR, "�G���[�ԍ��F" + ex.Number);
				logWriter(sUser, ERR, "StackTrace:\n" + ex.StackTrace);
				throw ex;
			}

		}

		/*********************************************************************
		 * �t�o�c�`�s�d�̎��s�iStringBuilder�Łj
		 * �����F�c�a�R�l�N�V�����A���s�r�p�k
		 * �ߒl�F�X�V����
		 *
		 *********************************************************************/
		protected int CmdUpdate(string[] sUser, OracleConnection connUpdate, StringBuilder sSQL)
		{
			logWriter(sUser, INF_SQL, "\n" + sSQL.ToString());

			try
			{
				OracleCommand cmd = new OracleCommand(sSQL.ToString());
				cmd.Connection = connUpdate;
				cmd.CommandType = CommandType.Text;

				cmd.Prepare();
				int iUpdRow = cmd.ExecuteNonQuery();
				cmd.Dispose();

				return iUpdRow;
			}
			catch (OracleException ex)
			{
				logWriter(sUser, ERR, "\n" + sSQL.ToString());
				logWriter(sUser, ERR, "�G���[�ԍ��F" + ex.Number);
				logWriter(sUser, ERR, "StackTrace:\n" + ex.StackTrace);
				throw ex;
			}
		}


		/*********************************************************************
		 * ���O�t�@�C����������
		 * �����F���O�o�̓t���O�A���O
		 * �ߒl�F�Ȃ�
		 *
		 *********************************************************************/

		protected void logWriter2(string[] sUser, int iMode, string sLog)
		{
			// �G���[�ȊO�̎��́A���o��
			if (iMode != ERR) return;

			if (iMode <= iLogMode)
			{
				System.IO.FileStream   fs = null;
				System.IO.StreamWriter sw = null;
				string sFileName = sLogPath
								+ System.DateTime.Now.ToString("MMdd")
								+ "_is2LogOut"
								+ gsAppSrc
								+ '_'
								;
						//�[���h�c
						//�i���݂��Ȃ��ꍇ�A�h�o�A�h���X���g�p����j
						if (sUser != null && sUser[2] != null && sUser[2].Length > 0)
						{
							sFileName += sUser[2];
						}
						else
						{
							sFileName += Context.Request.UserHostName.Replace('.','_');
						}
						sFileName += ".log";

				try
				{
					fs = new System.IO.FileStream(sFileName, 
													System.IO.FileMode.Append, 
													System.IO.FileAccess.Write, 
													System.IO.FileShare.Write);
					sw = new System.IO.StreamWriter(fs, enc);

					// ����
					sw.Write("["+ System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") +"]");

					// �h�o�A�h���X
					sw.Write("["+ Context.Request.UserHostName +"]");

					// �[���h�c
					if (sUser != null && sUser[2] != null && sUser[2].Length > 0)
						sw.Write("["+ sUser[2] +"]");
					else
						sw.Write("[-]");

					// ����b�c
					if (sUser != null && sUser[0] != null && sUser[0].Length > 0)
						sw.Write("["+ sUser[0] +"]");
					else
						sw.Write("[-]");

					// ���p�҂b�c
					if (sUser != null && sUser[1] != null && sUser[1].Length > 0)
						sw.Write("["+ sUser[1] +"]");
					else
						sw.Write("[-]");

					if(sUser.Length >= 4)
					{
						if (sUser != null && sUser[3] != null && sUser[3].Length > 0)
							sw.Write("["+ sUser[3] +"]");
						else
							sw.Write("[-]");
					}

					// �A�v���P�[�V�����p�X
					sw.Write("[" + Context.Request.ApplicationPath + "]");

					// ���O
					sw.WriteLine(sLog);
					sw.Flush();

				}
				catch(Exception )
				{
					;
				}
				finally
				{
					// �t�@�C���N���[�Y
					if(sw != null) sw.Close();
					sw = null;
					if(fs != null) fs.Close();
					fs = null;
				}
			}
		}

		/*********************************************************************
		 * ���O�t�@�C����������
		 * �����F���O�o�̓t���O�A���O
		 * �ߒl�F�Ȃ�
		 *
		 *********************************************************************/
		private static bool gbLogOutErr = false;
		protected void logWriter(string[] sUser, int iMode, string sLog)
		{
			if (iMode <= iLogMode)
			{
				if(sLog == null || sLog.Length == 0) return;
				//�ʏ�^�p���ɂ́A
				//[����I��]��[�X�V]�ȂǂS�����ȉ��̏ꍇ�ɂ̓��O���o�͂��Ȃ�
				if(iMode < INF_SQL && sLog.Length <= 4) return;

				int iRet = 0;
				StringBuilder sbBuff = new StringBuilder(2048);
				try
				{

					// ����
					sbBuff.Append("["+ System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") +"]");

					// �h�o�A�h���X
					sbBuff.Append("["+ Context.Request.UserHostName +"]");

					// �[���h�c
					if (sUser != null && sUser[2] != null && sUser[2].Length > 0)
						sbBuff.Append("["+ sUser[2] +"]");
					else
						sbBuff.Append("[-]");

					// ����b�c
					if (sUser != null && sUser[0] != null && sUser[0].Length > 0)
						sbBuff.Append("["+ sUser[0] +"]");
					else
						sbBuff.Append("[-]");

					// ���p�҂b�c
					if (sUser != null && sUser[1] != null && sUser[1].Length > 0)
						sbBuff.Append("["+ sUser[1] +"]");
					else
						sbBuff.Append("[-]");

					if(sUser.Length >= 4)
					{
						if (sUser != null && sUser[3] != null && sUser[3].Length > 0)
							sbBuff.Append("["+ sUser[3] +"]");
						else
							sbBuff.Append("[-]");
					}
					// �A�v���P�[�V�����p�X
					sbBuff.Append("[" + Context.Request.ApplicationPath + "]");

					// ���O
					sbBuff.Append(sLog);

					if(sv_logout == null){
						sv_logout = new  is2EXlogout.Service1();
						// Timeout�̃f�t�H���g��100000�i�P�O�O�b�j
						sv_logout.Timeout = 1000; // �P�b
					}
					if(sv_logout == null){
						logWriter2(sUser, ERR, "�T�[�o�G���[�Fsv_logout == null");
						logWriter2(sUser, iMode, sbBuff.ToString());
						return;
					}
					iRet = sv_logout.LogOut(sbBuff.ToString());
					if(iRet == 0) return;
				}
				catch(System.Net.WebException ex)
				{
					//����̂ݏo�͂���
					if(gbLogOutErr == false)
					{
						gbLogOutErr = true;
						logWriter2(sUser, ERR, "�T�[�o�G���[�F\n"  + ex.ToString());
						logWriter2(sUser, iMode, sbBuff.ToString());
					}
					return;
				}
				catch(Exception ex)
				{
					logWriter2(sUser, ERR, "�T�[�o�G���[�F\n"  + ex.ToString());
					logWriter2(sUser, iMode, sLog);
					return;
				}
				finally
				{
					sbBuff = null;
				}

				// �G���[�ȊO�̎��́A���o��
				if(iMode != ERR) return;

				// ���O�o�͗p�v�����T�[�r�X���g�p�ł��Ȃ��ꍇ�A
				// �t�@�C���ɒ��ڏo��
				if(iRet != 0) 
					logWriter2(sUser, ERR, "�T�[�o�G���[�Fsv_logout.LogOut�F" + iRet);
				logWriter2(sUser, iMode, sLog);
			}
		}

		/*********************************************************************
		 * �C�x���g���O�o��
		 * �����F���O�o�͕�����
		 * �ߒl�F����-
		 *
		 *********************************************************************/
		private static string gsAppSrc = System.Web.HttpRuntime.AppDomainAppVirtualPath.Replace('/','_');

		/*********************************************************************
		 * ���[�_�I�u�W�F�N�g�j��
		 * �����F�Ȃ�
		 * �ߒl�F�Ȃ�
		 *
		 *********************************************************************/
		protected void disposeReader(OracleDataReader reader)
		{
			if(reader == null) return;

			try{ reader.Close(); } catch (Exception){};
			try{ reader.Dispose(); } catch (Exception){};

			reader = null;

			return;
		}
	}
}
