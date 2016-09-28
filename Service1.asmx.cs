using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.Services;
using Oracle.DataAccess.Client;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;

namespace is2EXsyukka
{
	/// <summary>
	/// [is2EXsyukka]
	/// </summary>
	//--------------------------------------------------------------------------
	//  �G�R�[�����a�����h�r�Q�T�[�o��œ��삷��Í��Ϗo�׃f�[�^��DB�o�^
	//--------------------------------------------------------------------------
	// �C������
	//--------------------------------------------------------------------------
	// 2011.02.01 KCL�j���q IS2�ł̍X�V���e�𒊏o���鏈���̒ǉ��@
	//--------------------------------------------------------------------------
	// 2015.03.26 BEVAS) �O�c ����v�̖��Ӗ���"0"�����ׂ�" "�ɒu�����鏈����ǉ�
	//--------------------------------------------------------------------------
	[System.Web.Services.WebService(
		 Namespace="http://Walkthrough/XmlWebServices/",
		 Description="is2EXsyukka")]

	public class Service1 : is2EXsyukka.CommService
	{
//		private static string sCRLF = "\\r\\n";
//		private static string sSepa = "|";
//		private static string sKanma = ",";
//		private static string sDbl = "\"";
//		private static string sSng = "'";
			
		public Service1()
		{
			//CODEGEN: ���̌Ăяo���́AASP.NET Web �T�[�r�X �f�U�C�i�ŕK�v�ł��B
			InitializeComponent();

			connectService();
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

			System.Type type = System.Type.GetType("System.String");
			System.Configuration.AppSettingsReader config = new System.Configuration.AppSettingsReader();

			// �c�a��`
			string sSUser = "";
			string sSPass = "";
			string sSTns  = "";
			// �r�u�q���c�a�A�N�Z�X��`
			sSUser = config.GetValue("user", type).ToString();
			sSPass = config.GetValue("pass", type).ToString();
			sSTns  = config.GetValue("data", type).ToString();
			sSvUser = new string[]{sSUser,sSPass,sSTns};
			sConn = "User Id="  + sSUser
				+ ";Password=" + sSPass
				+ ";Data Source=" + sSTns;

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
		 * �o�׃f�[�^�o�^
		 * �����F����b�c�A����b�c�A�o�ד�...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Ins_EX_syukka(string[] sUser, string[] sData)
		{
			logWriter(sUser, INF, "�G�R�[�����o�דo�^�J�n");
			byte[] bText;
			string[] sList;
			int in_cnt;
			int upd_cnt = 0;
			int ins_cnt = 0;

			if (sUser[0] == "")
				sUser = sSvUser;

		//---- �Í����t�@�C���̕���
			
			// 3DES ����KEY�̒�`
			string key1 = "Da6JAU9Uc7JbLwSg";			//16����(128�ޯ�)
			string keyIV_w = "TBBXuA8V";				// 8����(64�ޯ�)
			byte[] DesIV = Encoding.UTF8.GetBytes(keyIV_w);
			string key2 = System.DateTime.Today.ToString("yyyyMMdd");	
//			string DesKey3 = key1 + key2;
			string DesKey3 = key1;
			byte[] DesKey = Encoding.UTF8.GetBytes(DesKey3);
			
			string sText;

			ArrayList aData = new ArrayList();

			//		  	
			for (in_cnt = 0; in_cnt < sData.Length; in_cnt++)
			{
				// �Í������ꂽ������� byte �z��ɕϊ����܂�
//				byte[] source = Encoding.UTF8.GetBytes(sData[in_cnt]);

				string  sByte = "";
				sText = sData[in_cnt];

				bText = new byte[sText.Length / 2];
				for(int iCnt = 0; iCnt < sText.Length; iCnt+=2)
				{
					sByte = sText.Substring(iCnt, 2);
					bText[iCnt/2] = Convert.ToByte(sByte,16);
				}

				// Trippe DES �̃T�[�r�X �v���o�C�_�𐶐����܂�
				TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();

				// ���o�͗p�̃X�g���[���𐶐����܂�(����)
				MemoryStream ms = new MemoryStream();
				CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor( DesKey, DesIV),
					CryptoStreamMode.Write);

				// �X�g���[���ɈÍ������ꂽ�f�[�^���������݂܂�
				cs.Write(bText, 0, bText.Length);
				cs.Close();

				// ���������ꂽ�f�[�^�� byte �z��Ŏ擾���܂�
				byte[] destination = ms.ToArray();
				ms.Close();

				// byte �z��𕶎���ɕϊ�����ARRAY�ɕۑ�����
				aData.Add(Encoding.UTF8.GetString(destination));
				// ���������ꂽ1�s�𗭂ߍ��ށ@
			}
		//----�@�c�a����
		
//			decimal d����;
//			string s����v = " ";
			string s�o�^��;
			int i�Ǘ��m�n;
			string s���t;

			OracleConnection conn2 = null;
			string[] sRet = new string[1 + aData.Count * 2];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();
			
			sRet[0] = "";
			try
			{
				IEnumerator enumList = aData.GetEnumerator();
				while(enumList.MoveNext())
				{
					string cmdQuery = "";
					OracleDataReader reader;

					// ��ؕ����u|�v�ɂĕ����@
					//string sStr = new string[aData.ToString()];
					string sStr = enumList.Current.ToString();
					sList = sStr.Split(new char[] { '|' });
					
					//�������@�o�^�ς݃`�F�b�N�@������
					cmdQuery
						= "SELECT \"�o�^��\" , \"�W���[�i���m�n\" \n"
						+ "  FROM \"�r�s�O�P�o�׃W���[�i��\" \n"
						+ " WHERE ����b�c = '" + sList[0] +"' \n"		
						+ "   AND ����b�c = '" + sList[1] +"' \n"
						+ "   AND �o�^��   = '" + sList[2] +"' \n"
						+ "   AND \"�W���[�i���m�n\" = '" + sList[3] +"' \n"
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					bool bRead = reader.Read();
					if(bRead)
					{
						disposeReader(reader);
						reader = null;
						//�������@�o�^�ς݁F�X�V�@������ 
						cmdQuery 
							= "UPDATE \"�r�s�O�P�o�׃W���[�i��\" \n"
							  +    "SET �o�ד�             = '" + sList[4]  +"', \n"
							  +        "���q�l�o�הԍ�     = '" + sList[5]  +"',"
							  +        "�׎�l�b�c         = '" + sList[6]  +"',"
							  +        "�d�b�ԍ��P         = '" + sList[7]  +"', \n"
							  +        "�d�b�ԍ��Q         = '" + sList[8]  +"',"
							  +        "�d�b�ԍ��R         = '" + sList[9]  +"',"
							  +        "�e�`�w�ԍ��P       = '" + sList[10] +"', \n"
							  +        "�e�`�w�ԍ��Q       = '" + sList[11] +"',"
							  +        "�e�`�w�ԍ��R       = '" + sList[12] +"',"
							  +        "�Z���b�c           = '" + sList[13] +"', \n"
							  +        "�Z���P             = '" + sList[14] +"',"
							  +        "�Z���Q             = '" + sList[15] +"',"
							  +        "�Z���R             = '" + sList[16] +"', \n"
							  +        "���O�P             = '" + sList[17] +"',"
							  +        "���O�Q             = '" + sList[18] +"',"
							  +        "���O�R             = '" + sList[19] +"', \n"
							  +        "�X�֔ԍ�           = '" + sList[20] +"',"
							  +        "���X�b�c           = '" + sList[21] +"',"
							  +        "���X��             = '" + sList[22] +"',"
// MOD 2015.03.26 BEVAS)�O�c ����v���󔒌Œ�ɂ��� START
//							+        "����v             = '" + sList[23] +"', \n"
							  +        "����v             = ' ', \n"
// MOD 2015.03.26 BEVAS)�O�c ����v���󔒌Œ�ɂ��� END
							  +        "�ב��l�b�c         = '" + sList[24] +"',"
							  +        "�ב��l������       = '" + sList[25] +"',"
							  +        "�W��X�b�c         = '" + sList[26] +"', \n"
							  +        "���X�b�c           = '" + sList[27] +"',"
							  +        "���X��             = '" + sList[28] +"',"
							  +        "���Ӑ�b�c         = '" + sList[29] +"', \n"
							  +        "���ۂb�c           = '" + sList[30] +"',"
							  +        "���ۖ�             = '" + sList[31] +"',"
							  +        "��               =  " + sList[32] +", \n"
							  +        "�ː�               =  " + sList[33] +","
							  +        "�d��               =  " + sList[34] +","
							  +        "���j�b�g           =  " + sList[35] +","
							  +        "�w���             = '" + sList[36] +"',"
							  +        "�w����敪         = '" + sList[37] +"',"
							  +        "�A���w���b�c�P     = '" + sList[38] +"',"
							  +        "�A���w���P         = '" + sList[39] +"', \n"
							  +        "�A���w���b�c�Q     = '" + sList[40] +"',"
							  +        "�A���w���Q         = '" + sList[41] +"',"
							  +        "�i���L���P         = '" + sList[42] +"',"
							  +        "�i���L���Q         = '" + sList[43] +"', \n"
							  +        "�i���L���R         = '" + sList[44] +"',"
							  +        "�i���L���S         = '" + sList[45] +"',"
							  +        "�i���L���T         = '" + sList[46] +"',"
							  +        "�i���L���U         = '" + sList[47] +"',"
							  +        "�i���L���V         = '" + sList[48] +"',"
							  +        "�����敪           = '" + sList[49] +"',"
//							  +        "�ی����z           =  " + sList[50] +","
//							  +        "�^��               =  " + sList[51] +","
//							  +        "���p               =  " + sList[52] +","
//							  +        "������             =  " + sList[53] +","
//							  +        "�d���b�c           = '" + sList[54] + "', \n"
							  +        "�����ԍ�         = '" + sList[55] +"',"
							  +        "�����敪         = '" + sList[56] +"',"
							  +        "����󔭍s�ςe�f   = '" + sList[57] +"',"
							  +        "�o�׍ςe�f         = '" + sList[58] +"',"
							  +        "���M�ςe�f         = '0',"
							  +        "�ꊇ�o�ׂe�f       = '" + sList[60] +"',"
							  +        "���               = '01',"
							  +        "�ڍ׏��           = '  ', \n"
//							  +        "�^���G���[�m�F�e�f = '" + sList[63] +"',"
//							  +        "�^����           = '" + sList[64] +"',"
//   						  +        "�^���ː�           = '" + sList[65] +"',"
//							  +        "�^���d��           = '" + sList[66] +"',"
//							  +        "�����O�P           = '" + sList[67] +"',"�@�@//�`�����O�U
							  +        "�폜�e�f           = '" + sList[73] +"',"
							  +        "�X�V����           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
							  +        "�X�V�o�f           = '" + sList[78] +"',"
							  +        "�X�V��             = '" + sList[79] +"', \n"
							  +        "�폜����           = '" + sList[80] +"', \n"
							  +        "�폜�o�f           = '" + sList[81] +"',"
							  +        "�폜��             = '" + sList[82] +"' \n"
							  + " WHERE ����b�c           = '" + sList[0]  +"' \n"
							  + "   AND ����b�c           = '" + sList[1]  +"' \n"
							  + "   AND �o�^��             = '" + sList[2] +"' \n"
							  + "   AND \"�W���[�i���m�n\" = "  + sList[3] +" \n"
							  ;
						int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
						upd_cnt++;
					}
					else
					{
						reader.Close();
						//�������@�V�K�o�^�@������ 
						//�W���[�i���m�n�擾
						cmdQuery
							= "SELECT \"�W���[�i���m�n�o�^��\",\"�W���[�i���m�n�Ǘ�\", \n"
							+ "       TO_CHAR(SYSDATE,'YYYYMMDD') \n"
							+ "  FROM �b�l�O�Q���� \n"
							+ " WHERE ����b�c = '" + sList[0] +"' \n"		
							+ "   AND ����b�c = '" + sList[1] +"' \n"
							+ "   AND �폜�e�f = '0'"
							+ "   FOR UPDATE ";

						reader = CmdSelect(sUser, conn2, cmdQuery);
						reader.Read();
						s�o�^��   = reader.GetString(0).Trim();
						i�Ǘ��m�n = reader.GetInt32(1);
						s���t     = reader.GetString(2);
						reader.Close();
						if(s�o�^�� == s���t)
							i�Ǘ��m�n++;
						else
						{
							s�o�^�� = s���t;
							i�Ǘ��m�n = 1;
						}
						
						string s�X�VPG�� = "�d�����o";
						string s�X�V�Җ� = "is2ex";
						cmdQuery 
							= "UPDATE �b�l�O�Q���� \n"
							+    "SET \"�W���[�i���m�n�o�^��\"  = '" + s�o�^�� +"', \n"
							+        "\"�W���[�i���m�n�Ǘ�\"    = "  + i�Ǘ��m�n +", \n"
							+        "�X�V�o�f                  = '" + s�X�VPG�� +"', \n"
							+        "�X�V��                    = '" + s�X�V�Җ� +"', \n"
							+        "�X�V����                  =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
							+ " WHERE ����b�c       = '" + sList[0] +"' \n"
							+ "   AND ����b�c       = '" + sList[1] +"' \n"
							+ "   AND �폜�e�f = '0'";

						int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
						disposeReader(reader);
						reader = null;
						//�d���b�c�擾
						string s���X�b�c = sList[21];
						string s���X�b�c = sList[27];
						string s�d���b�c = " ";
						if(s���X�b�c.Trim().Length > 0 && s���X�b�c.Trim().Length > 0)
						{
							string[] sRetSiwake = Get_siwake(sUser, conn2, s���X�b�c, s���X�b�c);
							s�d���b�c = sRetSiwake[1];
						}
						if(sList[5] != " " && sList[5] != null)
						{
							cmdQuery
								= "UPDATE �r�l�O�Q�׎�l \n"
								+ " SET �o�^�o�f = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
								+ " WHERE ����b�c = '" + sList[0] +"' \n"
								+ " AND ����b�c   = '" + sList[1] +"' \n"
								+ " AND �׎�l�b�c = '" + sList[6] +"' \n"
								+ " AND �폜�e�f   = '0'";
							try
							{
								int iUpdRowSM02 = CmdUpdate(sUser, conn2, cmdQuery);
							}
							catch(Exception)
							{
								;
							}
						}

						cmdQuery 
							= "INSERT INTO \"�r�s�O�P�o�׃W���[�i��\" \n"
							+ "(����b�c, ����b�c, �o�^��, \"�W���[�i���m�n\", �o�ד� \n"
							+ ", ���q�l�o�הԍ�, �׎�l�b�c \n"
							+ ", �d�b�ԍ��P, �d�b�ԍ��Q, �d�b�ԍ��R, �e�`�w�ԍ��P, �e�`�w�ԍ��Q, �e�`�w�ԍ��R \n"
							+ ", �Z���b�c, �Z���P, �Z���Q, �Z���R \n"
							+ ", ���O�P, ���O�Q, ���O�R \n"
							+ ", �X�֔ԍ�, ���X�b�c, ���X��, ����v \n"
							+ ", �ב��l�b�c, �ב��l������ \n"
							+ ", �W��X�b�c, ���X�b�c, ���X�� \n"
							+ ", ���Ӑ�b�c, ���ۂb�c, ���ۖ� \n"
							+ ", ��, �ː�, �d��, ���j�b�g \n"
							+ ", �w���, �w����敪 \n"
							+ ", �A���w���b�c�P, �A���w���P \n"
							+ ", �A���w���b�c�Q, �A���w���Q \n"
							+ ", �i���L���P, �i���L���Q, �i���L���R \n"
							+ ", �����敪, �ی����z, �^��, ���p, ������ \n"
							+ ", �d���b�c, �����ԍ�, �����敪 \n"
							+ ", ����󔭍s�ςe�f, �o�׍ςe�f, ���M�ςe�f, �ꊇ�o�ׂe�f \n"
							+ ", ���, �ڍ׏�� \n"
							+ ", �폜�e�f, �o�^����, �o�^�o�f, �o�^�� \n"
							+ ", �X�V����, �X�V�o�f, �X�V�� \n"
							+ ", �폜����, �폜�o�f, �폜�� \n"
							+ ") \n"
							//
							+ "VALUES ('" + sList[0]  +"','" + sList[1]  +"','" + sList[2] +"'," + sList[3] +",'" + sList[4] +"', \n"
							+         "'" + sList[5]  +"','" + sList[6]  +"', \n"															//���q�l�o�הԍ��`
							+         "'" + sList[7]  +"','" + sList[8]  +"','" + sList[9]  +"','" + sList[10] +"','" + sList[11] +"','" + sList[12] +"', \n"		//�d�b�ԍ��P�`
							+         "'" + sList[13] +"','" + sList[14] +"','" + sList[15] +"','" + sList[16] +"', \n"						//�Z���b�c�`
							+         "'" + sList[17] +"','" + sList[18] +"','" + sList[19] +"', \n"										//���O�P�`
// MOD 2015.03.26 BEVAS)�O�c ����v�𔼊p�X�y�[�X�Œ艻 START
//							+         "'" + sList[20] +"','" + sList[21] +"','" + sList[22] +"','" + sList[23] +"', \n"						//�X�֔ԍ��`
							+         "'" + sList[20] +"','" + sList[21] +"','" + sList[22] +"',' ', \n"						            //�X�֔ԍ��` ����v�͋󔒂Ƃ���
// MOD 2015.03.26 BEVAS)�O�c ����v�𔼊p�X�y�[�X�Œ艻 END
							+         "'" + sList[24] +"','" + sList[25] +"', \n"															//�ב��l�b�c�`
							+         "'" + sList[26] +"','" + sList[27] +"','" + sList[28] +"', \n"										//�W��X�b�c�` 
							+         "'" + sList[29] +"','" + sList[30] +"','" + sList[31] +"', \n"										//���Ӑ�b�c�`
							+         "'" + sList[32] +"','" + sList[33] +"','" + sList[34] +"','" + sList[35] +"', \n"						//���`
							+         "'" + sList[36] +"','" + sList[37] +"', \n"															//�w����`
							+         "'" + sList[38] +"','" + sList[39] +"', \n"															//�A���w���b�c�P�`					
							+         "'" + sList[40] +"','" + sList[41] +"', \n"															//�A���w���b�c�Q�`
							+         "'" + sList[42] +"','" + sList[43] +"','" + sList[44] +"', \n"										//�i���L���`
							+         "'" + sList[49] +"','" + sList[50] +"','" + sList[51] +"','" + sList[52] +"','" + sList[53] +"', \n"	//�����敪�`
							+         "'" + s�d���b�c +"','" + sList[55] +"','" + sList[56] +"', \n"										//�d���b�c�`
							+         "'" + sList[57] +"','" + sList[58] +"','" + '0' +"','" + sList[60] +"', \n"							//����󔭍s�ςe�f�`
							+         "'" + sList[61] +"','" + sList[62] +"', \n"															//��ԁ`
							+         "'" + sList[73] +"',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sList[75] +"','" + sList[76] +"', \n"	//�폜�e�f�`
							+         "'" + sList[77] +"','" + sList[78] +"','" + sList[79]  +"', \n"										//�X�V�����`		
							+         "'" + sList[80] +"','" + sList[81] +"','" + sList[82]  +"')";											//�폜�����`		

						iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
						ins_cnt++;
					}
					
				}
				tran.Commit();
				string sLogInfo = "�o�^�����F" + ins_cnt + " ���A�X�V�����F" + upd_cnt + " ��������I�����܂����B";�@
				logWriter(sUser, INF, sLogInfo);
				sRet[0] = "����I��";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}	

		/*********************************************************************
		 * �d���b�c�擾
		 * �����F����b�c�A����b�c�A�c�a�ڑ��A���X�A���X
		 * �ߒl�F�X�e�[�^�X�A�d���b�c
		 *
		 *********************************************************************/
		private static string GET_SIWAKE_SELECT
			= "SELECT �d���b�c \n"
			+ " FROM �b�l�P�V�d�� \n"
			;

		private String[] Get_siwake(string[] sUser, OracleConnection conn2, string sHatuCd, string sTyakuCd)
		{

			string[] sRet = new string[2];

			string cmdQuery = GET_SIWAKE_SELECT
				+ " WHERE ���X���b�c = '" + sHatuCd + "' \n"
				+ " AND ���X���b�c = '" + sTyakuCd + "' \n"
				+ " AND �폜�e�f = '0' \n"
				;

			OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

			if(reader.Read())
			{
				sRet[0] = " ";
				//				sRet[1] = reader.GetString(0).Trim();
				sRet[1] = reader.GetString(0);
			}
			else
			{
				sRet[0] = "�d���b�c�����߂��܂���ł���";
				sRet[1] = " ";
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * �o�׃W���[�i����Ԋl��
		 * �����F����b�c[]�A����b�c�A�O��擾����
		 * �ߒl�F�X�e�[�^�X�A����擾�����A�����ԍ��A�^���A��ԁA�ڍ׏��
		 *
		 *********************************************************************/
		private static string GET_ST01_Status_SELECT
			= "	SELECT \n"
			+         " ST01.�����ԍ� || '|' "
			+       "|| NVL(ST01.�^��,'0') || '|' "
			+       "|| ST01.��� || '|' "
			+       "|| NVL(ST01.�ڍ׏��,' ') || '|' " 
			+       "|| NVL(ST01.���p,'0')  || '|' "
			+       "|| NVL(ST01.������,'0') "
			+ " FROM �r�s�O�P�o�׃W���[�i�� ST01 \n"
			+ " LEFT JOIN �f�s�O�Q�z�� GT02 \n"
			+ " ON   ST01.�����ԍ� = GT02.���[�ԍ� \n"
			+ " LEFT JOIN �f�s�O�R���[�^�� GT03 \n" 
			+ " ON   ST01.�����ԍ� = GT03.���[�ԍ� \n"
			;

		private static string GET_DATETIME_SELECT
			= "	SELECT TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') FROM DUAL";


		[WebMethod]
		public String[] Get_St01Status(string[] sUser, string[] sKaiinCD, string sUpdDateTime)
		{
			logWriter(sUser, INF, "�G�R�[�����o�׃W���[�i����Ԏ擾�J�n");

			string[] sRet = new string[2];

			if (sUser[0] == "")
				sUser = sSvUser;

			OracleConnection conn2 = null;

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
			
			//�h�r�|�Q�T�[�o���A��Ԃ̍X�V���ꂽ�r�s�O�P�o�׃W���[�i�������擾	
			ArrayList arList = new ArrayList();

			// Array��̉���b�c����@WHERE���𐶐�
			IEnumerator iEnum = sKaiinCD.GetEnumerator();
			iEnum.MoveNext();								//�擪�͊������
			int iCnt = 0;
			string sWhere = " WHERE (ST01.����b�c = ";		

			while (iEnum.MoveNext())
			{
				if (iCnt > 0)
					sWhere = sWhere + " OR \n ST01.����b�c = ";		// 2�Ԗڈȍ~
				sWhere = sWhere + "'" + iEnum.Current + "'";
				iCnt++;
			}
			sWhere = sWhere +  ") \n";
			//�X�V���t�͈͂̒ǉ�(sUpdDateTime)
			sWhere = sWhere   
			+	 " AND (GT02.�X�V���� BETWEEN " +  sUpdDateTime + " AND " + "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n" 
			+	 "  OR  GT03.�X�V���� BETWEEN " +  sUpdDateTime + " AND " + "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n" + ")"; 

			StringBuilder sbQuery_T	= null;
			StringBuilder sbQuery	= null;
			OracleDataReader reader = null;
			string sDateTime = null;

			try
			{
				//�h�r�|�Q�T�[�o��茻�݂̃V�X�e�����t���Ԃ��擾
				sbQuery_T = new StringBuilder(256);
				sbQuery_T.Append(GET_DATETIME_SELECT);

				reader = CmdSelect(sUser, conn2, sbQuery_T);
			
				if (reader.Read())
				{
					sDateTime = reader.GetString(0).Trim();
				}
				disposeReader(reader);
				reader = null;

				// �r�s�O�P�̍X�V�r�d�k�d�b�s				
				sbQuery = new StringBuilder(1024);
				sbQuery.Append(GET_ST01_Status_SELECT);
				sbQuery.Append(sWhere);

//				logWriter(sUser, INF, "SQL���� " + sbQuery);
				reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					arList.Add(reader.GetString(0).Trim());
				}
				disposeReader(reader);
				reader = null;

				if(arList.Count == 0) 
				{
					sRet[0] = "�Y���f�[�^������܂���";
					arList.Add(sRet[0]);
				}
				else
				{
					sRet[0] = "����I��";
					arList.Insert(0, sRet[0]);
				}
				//���񏈗����Ԃ̑}��
				arList.Insert(1, sDateTime);
				sRet[1] = sDateTime;

				//ARRAY ����STRING[]�ց@
				sRet = new string[arList.Count + 0];

				iCnt = 0;
				IEnumerator enumList = arList.GetEnumerator();
				while(enumList.MoveNext())
				{
					sRet[iCnt] = enumList.Current.ToString();
					iCnt++;
//					sRet.CopyTo(sRet = new string[sRet.Length+1],0);
//					sRet[sRet.Length-1] = enumList.Current.ToString();
				}
				//				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
				logWriter(sUser, ERR, "�o�׃W���[�i����Ԏ擾 �G���[" + sRet[0]);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, "�o�׃W���[�i����Ԏ擾 " + sRet[0]);
			}
			finally
			{
				sbQuery = null;
				disconnect2(sUser, conn2);
				conn2 = null;
			}

			return sRet;
		}

	//Class END
	}
}
