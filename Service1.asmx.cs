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
	//  ƒGƒR[‹à‘®“aŒü‚¯‚h‚r‚QƒT[ƒoã‚Å“®ì‚·‚éˆÃ†Ïo‰×ƒf[ƒ^‚ÌDB“o˜^
	//--------------------------------------------------------------------------
	// C³—š—ğ
	//--------------------------------------------------------------------------
	// 2011.02.01 KCLj¬‘q IS2‚Å‚ÌXV“à—e‚ğ’Šo‚·‚éˆ—‚Ì’Ç‰Á@
	//--------------------------------------------------------------------------
	// 2015.03.26 BEVAS) ‘O“c “ÁêŒv‚Ì–³ˆÓ–¡‚È"0"‚ğ‚·‚×‚Ä" "‚É’uŠ·‚·‚éˆ—‚ğ’Ç‰Á
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
			//CODEGEN: ‚±‚ÌŒÄ‚Ño‚µ‚ÍAASP.NET Web ƒT[ƒrƒX ƒfƒUƒCƒi‚Å•K—v‚Å‚·B
			InitializeComponent();

			connectService();
		}

		#region ƒRƒ“ƒ|[ƒlƒ“ƒg ƒfƒUƒCƒi‚Å¶¬‚³‚ê‚½ƒR[ƒh 
		
		//Web ƒT[ƒrƒX ƒfƒUƒCƒi‚Å•K—v‚Å‚·B
		private IContainer components = null;
				
		/// <summary>
		/// ƒfƒUƒCƒi ƒTƒ|[ƒg‚É•K—v‚Èƒƒ\ƒbƒh‚Å‚·B‚±‚Ìƒƒ\ƒbƒh‚Ì“à—e‚ğ
		/// ƒR[ƒh ƒGƒfƒBƒ^‚Å•ÏX‚µ‚È‚¢‚Å‚­‚¾‚³‚¢B
		/// </summary>
		private void InitializeComponent()
		{

			System.Type type = System.Type.GetType("System.String");
			System.Configuration.AppSettingsReader config = new System.Configuration.AppSettingsReader();

			// ‚c‚a’è‹`
			string sSUser = "";
			string sSPass = "";
			string sSTns  = "";
			// ‚r‚u‚q“à‚c‚aƒAƒNƒZƒX’è‹`
			sSUser = config.GetValue("user", type).ToString();
			sSPass = config.GetValue("pass", type).ToString();
			sSTns  = config.GetValue("data", type).ToString();
			sSvUser = new string[]{sSUser,sSPass,sSTns};
			sConn = "User Id="  + sSUser
				+ ";Password=" + sSPass
				+ ";Data Source=" + sSTns;

		}

		/// <summary>
		/// g—p‚³‚ê‚Ä‚¢‚éƒŠƒ\[ƒX‚ÉŒãˆ—‚ğÀs‚µ‚Ü‚·B
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
		 * o‰×ƒf[ƒ^“o˜^
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚cAo‰×“ú...
		 * –ß’lFƒXƒe[ƒ^ƒX
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Ins_EX_syukka(string[] sUser, string[] sData)
		{
			logWriter(sUser, INF, "ƒGƒR[‹à‘®o‰×“o˜^ŠJn");
			byte[] bText;
			string[] sList;
			int in_cnt;
			int upd_cnt = 0;
			int ins_cnt = 0;

			if (sUser[0] == "")
				sUser = sSvUser;

		//---- ˆÃ†‰»ƒtƒ@ƒCƒ‹‚Ì•œ†
			
			// 3DES Œü‚¯KEY‚Ì’è‹`
			string key1 = "Da6JAU9Uc7JbLwSg";			//16•¶š(128ËŞ¯Ä)
			string keyIV_w = "TBBXuA8V";				// 8•¶š(64ËŞ¯Ä)
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
				// ˆÃ†‰»‚³‚ê‚½•¶š—ñ‚ğ byte ”z—ñ‚É•ÏŠ·‚µ‚Ü‚·
//				byte[] source = Encoding.UTF8.GetBytes(sData[in_cnt]);

				string  sByte = "";
				sText = sData[in_cnt];

				bText = new byte[sText.Length / 2];
				for(int iCnt = 0; iCnt < sText.Length; iCnt+=2)
				{
					sByte = sText.Substring(iCnt, 2);
					bText[iCnt/2] = Convert.ToByte(sByte,16);
				}

				// Trippe DES ‚ÌƒT[ƒrƒX ƒvƒƒoƒCƒ_‚ğ¶¬‚µ‚Ü‚·
				TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();

				// “üo—Í—p‚ÌƒXƒgƒŠ[ƒ€‚ğ¶¬‚µ‚Ü‚·(•œ†)
				MemoryStream ms = new MemoryStream();
				CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor( DesKey, DesIV),
					CryptoStreamMode.Write);

				// ƒXƒgƒŠ[ƒ€‚ÉˆÃ†‰»‚³‚ê‚½ƒf[ƒ^‚ğ‘‚«‚İ‚Ü‚·
				cs.Write(bText, 0, bText.Length);
				cs.Close();

				// •œ†‰»‚³‚ê‚½ƒf[ƒ^‚ğ byte ”z—ñ‚Åæ“¾‚µ‚Ü‚·
				byte[] destination = ms.ToArray();
				ms.Close();

				// byte ”z—ñ‚ğ•¶š—ñ‚É•ÏŠ·‚µ‚ÄARRAY‚É•Û‘¶‚·‚é
				aData.Add(Encoding.UTF8.GetString(destination));
				// •œ†‰»‚³‚ê‚½1s‚ğ—­‚ß‚Ş@
			}
		//----@‚c‚aˆ—
		
//			decimal dŒ”;
//			string s“ÁêŒv = " ";
			string s“o˜^“ú;
			int iŠÇ—‚m‚n;
			string s“ú•t;

			OracleConnection conn2 = null;
			string[] sRet = new string[1 + aData.Count * 2];

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
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

					// ‹æØ•¶šu|v‚É‚Ä•ªŠ„@
					//string sStr = new string[aData.ToString()];
					string sStr = enumList.Current.ToString();
					sList = sStr.Split(new char[] { '|' });
					
					//¦¦¦@“o˜^Ï‚İƒ`ƒFƒbƒN@¦¦¦
					cmdQuery
						= "SELECT \"“o˜^“ú\" , \"ƒWƒƒ[ƒiƒ‹‚m‚n\" \n"
						+ "  FROM \"‚r‚s‚O‚Po‰×ƒWƒƒ[ƒiƒ‹\" \n"
						+ " WHERE ‰ïˆõ‚b‚c = '" + sList[0] +"' \n"		
						+ "   AND •”–å‚b‚c = '" + sList[1] +"' \n"
						+ "   AND “o˜^“ú   = '" + sList[2] +"' \n"
						+ "   AND \"ƒWƒƒ[ƒiƒ‹‚m‚n\" = '" + sList[3] +"' \n"
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					bool bRead = reader.Read();
					if(bRead)
					{
						disposeReader(reader);
						reader = null;
						//¦¦¦@“o˜^Ï‚İFXV@¦¦¦ 
						cmdQuery 
							= "UPDATE \"‚r‚s‚O‚Po‰×ƒWƒƒ[ƒiƒ‹\" \n"
							  +    "SET o‰×“ú             = '" + sList[4]  +"', \n"
							  +        "‚¨‹q—lo‰×”Ô†     = '" + sList[5]  +"',"
							  +        "‰×ól‚b‚c         = '" + sList[6]  +"',"
							  +        "“d˜b”Ô†‚P         = '" + sList[7]  +"', \n"
							  +        "“d˜b”Ô†‚Q         = '" + sList[8]  +"',"
							  +        "“d˜b”Ô†‚R         = '" + sList[9]  +"',"
							  +        "‚e‚`‚w”Ô†‚P       = '" + sList[10] +"', \n"
							  +        "‚e‚`‚w”Ô†‚Q       = '" + sList[11] +"',"
							  +        "‚e‚`‚w”Ô†‚R       = '" + sList[12] +"',"
							  +        "ZŠ‚b‚c           = '" + sList[13] +"', \n"
							  +        "ZŠ‚P             = '" + sList[14] +"',"
							  +        "ZŠ‚Q             = '" + sList[15] +"',"
							  +        "ZŠ‚R             = '" + sList[16] +"', \n"
							  +        "–¼‘O‚P             = '" + sList[17] +"',"
							  +        "–¼‘O‚Q             = '" + sList[18] +"',"
							  +        "–¼‘O‚R             = '" + sList[19] +"', \n"
							  +        "—X•Ö”Ô†           = '" + sList[20] +"',"
							  +        "’…“X‚b‚c           = '" + sList[21] +"',"
							  +        "’…“X–¼             = '" + sList[22] +"',"
// MOD 2015.03.26 BEVAS)‘O“c “ÁêŒv‚ğ‹ó”’ŒÅ’è‚É‚·‚é START
//							+        "“ÁêŒv             = '" + sList[23] +"', \n"
							  +        "“ÁêŒv             = ' ', \n"
// MOD 2015.03.26 BEVAS)‘O“c “ÁêŒv‚ğ‹ó”’ŒÅ’è‚É‚·‚é END
							  +        "‰×‘—l‚b‚c         = '" + sList[24] +"',"
							  +        "‰×‘—l•”–¼       = '" + sList[25] +"',"
							  +        "W–ñ“X‚b‚c         = '" + sList[26] +"', \n"
							  +        "”­“X‚b‚c           = '" + sList[27] +"',"
							  +        "”­“X–¼             = '" + sList[28] +"',"
							  +        "“¾ˆÓæ‚b‚c         = '" + sList[29] +"', \n"
							  +        "•”‰Û‚b‚c           = '" + sList[30] +"',"
							  +        "•”‰Û–¼             = '" + sList[31] +"',"
							  +        "ŒÂ”               =  " + sList[32] +", \n"
							  +        "Ë”               =  " + sList[33] +","
							  +        "d—Ê               =  " + sList[34] +","
							  +        "ƒ†ƒjƒbƒg           =  " + sList[35] +","
							  +        "w’è“ú             = '" + sList[36] +"',"
							  +        "w’è“ú‹æ•ª         = '" + sList[37] +"',"
							  +        "—A‘—w¦‚b‚c‚P     = '" + sList[38] +"',"
							  +        "—A‘—w¦‚P         = '" + sList[39] +"', \n"
							  +        "—A‘—w¦‚b‚c‚Q     = '" + sList[40] +"',"
							  +        "—A‘—w¦‚Q         = '" + sList[41] +"',"
							  +        "•i–¼‹L–‚P         = '" + sList[42] +"',"
							  +        "•i–¼‹L–‚Q         = '" + sList[43] +"', \n"
							  +        "•i–¼‹L–‚R         = '" + sList[44] +"',"
							  +        "•i–¼‹L–‚S         = '" + sList[45] +"',"
							  +        "•i–¼‹L–‚T         = '" + sList[46] +"',"
							  +        "•i–¼‹L–‚U         = '" + sList[47] +"',"
							  +        "•i–¼‹L–‚V         = '" + sList[48] +"',"
							  +        "Œ³’…‹æ•ª           = '" + sList[49] +"',"
//							  +        "•ÛŒ¯‹àŠz           =  " + sList[50] +","
//							  +        "‰^’À               =  " + sList[51] +","
//							  +        "’†Œp               =  " + sList[52] +","
//							  +        "”—¿‹à             =  " + sList[53] +","
//							  +        "d•ª‚b‚c           = '" + sList[54] + "', \n"
							  +        "‘—‚èó”Ô†         = '" + sList[55] +"',"
							  +        "‘—‚èó‹æ•ª         = '" + sList[56] +"',"
							  +        "‘—‚èó”­sÏ‚e‚f   = '" + sList[57] +"',"
							  +        "o‰×Ï‚e‚f         = '" + sList[58] +"',"
							  +        "‘—MÏ‚e‚f         = '0',"
							  +        "ˆêŠ‡o‰×‚e‚f       = '" + sList[60] +"',"
							  +        "ó‘Ô               = '01',"
							  +        "Ú×ó‘Ô           = '  ', \n"
//							  +        "‰^’ÀƒGƒ‰[Šm”F‚e‚f = '" + sList[63] +"',"
//							  +        "‰^’ÀŒÂ”           = '" + sList[64] +"',"
//   						  +        "‰^’ÀË”           = '" + sList[65] +"',"
//							  +        "‰^’Àd—Ê           = '" + sList[66] +"',"
//							  +        "ˆ—‚O‚P           = '" + sList[67] +"',"@@//`ˆ—‚O‚U
							  +        "íœ‚e‚f           = '" + sList[73] +"',"
							  +        "XV“ú           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
							  +        "XV‚o‚f           = '" + sList[78] +"',"
							  +        "XVÒ             = '" + sList[79] +"', \n"
							  +        "íœ“ú           = '" + sList[80] +"', \n"
							  +        "íœ‚o‚f           = '" + sList[81] +"',"
							  +        "íœÒ             = '" + sList[82] +"' \n"
							  + " WHERE ‰ïˆõ‚b‚c           = '" + sList[0]  +"' \n"
							  + "   AND •”–å‚b‚c           = '" + sList[1]  +"' \n"
							  + "   AND “o˜^“ú             = '" + sList[2] +"' \n"
							  + "   AND \"ƒWƒƒ[ƒiƒ‹‚m‚n\" = "  + sList[3] +" \n"
							  ;
						int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
						upd_cnt++;
					}
					else
					{
						reader.Close();
						//¦¦¦@V‹K“o˜^@¦¦¦ 
						//ƒWƒƒ[ƒiƒ‹‚m‚næ“¾
						cmdQuery
							= "SELECT \"ƒWƒƒ[ƒiƒ‹‚m‚n“o˜^“ú\",\"ƒWƒƒ[ƒiƒ‹‚m‚nŠÇ—\", \n"
							+ "       TO_CHAR(SYSDATE,'YYYYMMDD') \n"
							+ "  FROM ‚b‚l‚O‚Q•”–å \n"
							+ " WHERE ‰ïˆõ‚b‚c = '" + sList[0] +"' \n"		
							+ "   AND •”–å‚b‚c = '" + sList[1] +"' \n"
							+ "   AND íœ‚e‚f = '0'"
							+ "   FOR UPDATE ";

						reader = CmdSelect(sUser, conn2, cmdQuery);
						reader.Read();
						s“o˜^“ú   = reader.GetString(0).Trim();
						iŠÇ—‚m‚n = reader.GetInt32(1);
						s“ú•t     = reader.GetString(2);
						reader.Close();
						if(s“o˜^“ú == s“ú•t)
							iŠÇ—‚m‚n++;
						else
						{
							s“o˜^“ú = s“ú•t;
							iŠÇ—‚m‚n = 1;
						}
						
						string sXVPG–¼ = "‚d©“®o";
						string sXVÒ–¼ = "is2ex";
						cmdQuery 
							= "UPDATE ‚b‚l‚O‚Q•”–å \n"
							+    "SET \"ƒWƒƒ[ƒiƒ‹‚m‚n“o˜^“ú\"  = '" + s“o˜^“ú +"', \n"
							+        "\"ƒWƒƒ[ƒiƒ‹‚m‚nŠÇ—\"    = "  + iŠÇ—‚m‚n +", \n"
							+        "XV‚o‚f                  = '" + sXVPG–¼ +"', \n"
							+        "XVÒ                    = '" + sXVÒ–¼ +"', \n"
							+        "XV“ú                  =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
							+ " WHERE ‰ïˆõ‚b‚c       = '" + sList[0] +"' \n"
							+ "   AND •”–å‚b‚c       = '" + sList[1] +"' \n"
							+ "   AND íœ‚e‚f = '0'";

						int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
						disposeReader(reader);
						reader = null;
						//d•ª‚b‚cæ“¾
						string s”­“X‚b‚c = sList[21];
						string s’…“X‚b‚c = sList[27];
						string sd•ª‚b‚c = " ";
						if(s”­“X‚b‚c.Trim().Length > 0 && s’…“X‚b‚c.Trim().Length > 0)
						{
							string[] sRetSiwake = Get_siwake(sUser, conn2, s”­“X‚b‚c, s’…“X‚b‚c);
							sd•ª‚b‚c = sRetSiwake[1];
						}
						if(sList[5] != " " && sList[5] != null)
						{
							cmdQuery
								= "UPDATE ‚r‚l‚O‚Q‰×ól \n"
								+ " SET “o˜^‚o‚f = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
								+ " WHERE ‰ïˆõ‚b‚c = '" + sList[0] +"' \n"
								+ " AND •”–å‚b‚c   = '" + sList[1] +"' \n"
								+ " AND ‰×ól‚b‚c = '" + sList[6] +"' \n"
								+ " AND íœ‚e‚f   = '0'";
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
							= "INSERT INTO \"‚r‚s‚O‚Po‰×ƒWƒƒ[ƒiƒ‹\" \n"
							+ "(‰ïˆõ‚b‚c, •”–å‚b‚c, “o˜^“ú, \"ƒWƒƒ[ƒiƒ‹‚m‚n\", o‰×“ú \n"
							+ ", ‚¨‹q—lo‰×”Ô†, ‰×ól‚b‚c \n"
							+ ", “d˜b”Ô†‚P, “d˜b”Ô†‚Q, “d˜b”Ô†‚R, ‚e‚`‚w”Ô†‚P, ‚e‚`‚w”Ô†‚Q, ‚e‚`‚w”Ô†‚R \n"
							+ ", ZŠ‚b‚c, ZŠ‚P, ZŠ‚Q, ZŠ‚R \n"
							+ ", –¼‘O‚P, –¼‘O‚Q, –¼‘O‚R \n"
							+ ", —X•Ö”Ô†, ’…“X‚b‚c, ’…“X–¼, “ÁêŒv \n"
							+ ", ‰×‘—l‚b‚c, ‰×‘—l•”–¼ \n"
							+ ", W–ñ“X‚b‚c, ”­“X‚b‚c, ”­“X–¼ \n"
							+ ", “¾ˆÓæ‚b‚c, •”‰Û‚b‚c, •”‰Û–¼ \n"
							+ ", ŒÂ”, Ë”, d—Ê, ƒ†ƒjƒbƒg \n"
							+ ", w’è“ú, w’è“ú‹æ•ª \n"
							+ ", —A‘—w¦‚b‚c‚P, —A‘—w¦‚P \n"
							+ ", —A‘—w¦‚b‚c‚Q, —A‘—w¦‚Q \n"
							+ ", •i–¼‹L–‚P, •i–¼‹L–‚Q, •i–¼‹L–‚R \n"
							+ ", Œ³’…‹æ•ª, •ÛŒ¯‹àŠz, ‰^’À, ’†Œp, ”—¿‹à \n"
							+ ", d•ª‚b‚c, ‘—‚èó”Ô†, ‘—‚èó‹æ•ª \n"
							+ ", ‘—‚èó”­sÏ‚e‚f, o‰×Ï‚e‚f, ‘—MÏ‚e‚f, ˆêŠ‡o‰×‚e‚f \n"
							+ ", ó‘Ô, Ú×ó‘Ô \n"
							+ ", íœ‚e‚f, “o˜^“ú, “o˜^‚o‚f, “o˜^Ò \n"
							+ ", XV“ú, XV‚o‚f, XVÒ \n"
							+ ", íœ“ú, íœ‚o‚f, íœÒ \n"
							+ ") \n"
							//
							+ "VALUES ('" + sList[0]  +"','" + sList[1]  +"','" + sList[2] +"'," + sList[3] +",'" + sList[4] +"', \n"
							+         "'" + sList[5]  +"','" + sList[6]  +"', \n"															//‚¨‹q—lo‰×”Ô†`
							+         "'" + sList[7]  +"','" + sList[8]  +"','" + sList[9]  +"','" + sList[10] +"','" + sList[11] +"','" + sList[12] +"', \n"		//“d˜b”Ô†‚P`
							+         "'" + sList[13] +"','" + sList[14] +"','" + sList[15] +"','" + sList[16] +"', \n"						//ZŠ‚b‚c`
							+         "'" + sList[17] +"','" + sList[18] +"','" + sList[19] +"', \n"										//–¼‘O‚P`
// MOD 2015.03.26 BEVAS)‘O“c “ÁêŒv‚ğ”¼ŠpƒXƒy[ƒXŒÅ’è‰» START
//							+         "'" + sList[20] +"','" + sList[21] +"','" + sList[22] +"','" + sList[23] +"', \n"						//—X•Ö”Ô†`
							+         "'" + sList[20] +"','" + sList[21] +"','" + sList[22] +"',' ', \n"						            //—X•Ö”Ô†` “ÁêŒv‚Í‹ó”’‚Æ‚·‚é
// MOD 2015.03.26 BEVAS)‘O“c “ÁêŒv‚ğ”¼ŠpƒXƒy[ƒXŒÅ’è‰» END
							+         "'" + sList[24] +"','" + sList[25] +"', \n"															//‰×‘—l‚b‚c`
							+         "'" + sList[26] +"','" + sList[27] +"','" + sList[28] +"', \n"										//W–ñ“X‚b‚c` 
							+         "'" + sList[29] +"','" + sList[30] +"','" + sList[31] +"', \n"										//“¾ˆÓæ‚b‚c`
							+         "'" + sList[32] +"','" + sList[33] +"','" + sList[34] +"','" + sList[35] +"', \n"						//ŒÂ”`
							+         "'" + sList[36] +"','" + sList[37] +"', \n"															//w’è“ú`
							+         "'" + sList[38] +"','" + sList[39] +"', \n"															//—A‘—w¦‚b‚c‚P`					
							+         "'" + sList[40] +"','" + sList[41] +"', \n"															//—A‘—w¦‚b‚c‚Q`
							+         "'" + sList[42] +"','" + sList[43] +"','" + sList[44] +"', \n"										//•i–¼‹L–`
							+         "'" + sList[49] +"','" + sList[50] +"','" + sList[51] +"','" + sList[52] +"','" + sList[53] +"', \n"	//Œ³’…‹æ•ª`
							+         "'" + sd•ª‚b‚c +"','" + sList[55] +"','" + sList[56] +"', \n"										//d•ª‚b‚c`
							+         "'" + sList[57] +"','" + sList[58] +"','" + '0' +"','" + sList[60] +"', \n"							//‘—‚èó”­sÏ‚e‚f`
							+         "'" + sList[61] +"','" + sList[62] +"', \n"															//ó‘Ô`
							+         "'" + sList[73] +"',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sList[75] +"','" + sList[76] +"', \n"	//íœ‚e‚f`
							+         "'" + sList[77] +"','" + sList[78] +"','" + sList[79]  +"', \n"										//XV“ú`		
							+         "'" + sList[80] +"','" + sList[81] +"','" + sList[82]  +"')";											//íœ“ú`		

						iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
						ins_cnt++;
					}
					
				}
				tran.Commit();
				string sLogInfo = "“o˜^Œ”F" + ins_cnt + " ŒAXVŒ”F" + upd_cnt + " Œ‚ª³íI—¹‚µ‚Ü‚µ‚½B";@
				logWriter(sUser, INF, sLogInfo);
				sRet[0] = "³íI—¹";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
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
		 * d•ª‚b‚cæ“¾
		 * ˆø”F‰ïˆõ‚b‚cA•”–å‚b‚cA‚c‚aÚ‘±A”­“XA’…“X
		 * –ß’lFƒXƒe[ƒ^ƒXAd•ª‚b‚c
		 *
		 *********************************************************************/
		private static string GET_SIWAKE_SELECT
			= "SELECT d•ª‚b‚c \n"
			+ " FROM ‚b‚l‚P‚Vd•ª \n"
			;

		private String[] Get_siwake(string[] sUser, OracleConnection conn2, string sHatuCd, string sTyakuCd)
		{

			string[] sRet = new string[2];

			string cmdQuery = GET_SIWAKE_SELECT
				+ " WHERE ”­“XŠ‚b‚c = '" + sHatuCd + "' \n"
				+ " AND ’…“XŠ‚b‚c = '" + sTyakuCd + "' \n"
				+ " AND íœ‚e‚f = '0' \n"
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
				sRet[0] = "d•ª‚b‚c‚ğŒˆ‚ß‚ç‚ê‚Ü‚¹‚ñ‚Å‚µ‚½";
				sRet[1] = " ";
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * o‰×ƒWƒƒ[ƒiƒ‹ó‘ÔŠl“¾
		 * ˆø”F‰ïˆõ‚b‚c[]A•”–å‚b‚cA‘O‰ñæ“¾“ú
		 * –ß’lFƒXƒe[ƒ^ƒXA¡‰ñæ“¾“úA‘—‚èó”Ô†A‰^’ÀAó‘ÔAÚ×ó‘Ô
		 *
		 *********************************************************************/
		private static string GET_ST01_Status_SELECT
			= "	SELECT \n"
			+         " ST01.‘—‚èó”Ô† || '|' "
			+       "|| NVL(ST01.‰^’À,'0') || '|' "
			+       "|| ST01.ó‘Ô || '|' "
			+       "|| NVL(ST01.Ú×ó‘Ô,' ') || '|' " 
			+       "|| NVL(ST01.’†Œp,'0')  || '|' "
			+       "|| NVL(ST01.”—¿‹à,'0') "
			+ " FROM ‚r‚s‚O‚Po‰×ƒWƒƒ[ƒiƒ‹ ST01 \n"
			+ " LEFT JOIN ‚f‚s‚O‚Q”zŠ® GT02 \n"
			+ " ON   ST01.‘—‚èó”Ô† = GT02.Œ´•[”Ô† \n"
			+ " LEFT JOIN ‚f‚s‚O‚RŒ´•[‰^’À GT03 \n" 
			+ " ON   ST01.‘—‚èó”Ô† = GT03.Œ´•[”Ô† \n"
			;

		private static string GET_DATETIME_SELECT
			= "	SELECT TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') FROM DUAL";


		[WebMethod]
		public String[] Get_St01Status(string[] sUser, string[] sKaiinCD, string sUpdDateTime)
		{
			logWriter(sUser, INF, "ƒGƒR[‹à‘®o‰×ƒWƒƒ[ƒiƒ‹ó‘Ôæ“¾ŠJn");

			string[] sRet = new string[2];

			if (sUser[0] == "")
				sUser = sSvUser;

			OracleConnection conn2 = null;

			// ‚c‚aÚ‘±
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "‚c‚aÚ‘±ƒGƒ‰[";
				return sRet;
			}
			
			//‚h‚r|‚QƒT[ƒo‚æ‚èAó‘Ô‚ÌXV‚³‚ê‚½‚r‚s‚O‚Po‰×ƒWƒƒ[ƒiƒ‹î•ñ‚ğæ“¾	
			ArrayList arList = new ArrayList();

			// Arrayã‚Ì‰ïˆõ‚b‚c‚©‚ç@WHERE•¶‚ğ¶¬
			IEnumerator iEnum = sKaiinCD.GetEnumerator();
			iEnum.MoveNext();								//æ“ª‚ÍŠ®—¹î•ñ
			int iCnt = 0;
			string sWhere = " WHERE (ST01.‰ïˆõ‚b‚c = ";		

			while (iEnum.MoveNext())
			{
				if (iCnt > 0)
					sWhere = sWhere + " OR \n ST01.‰ïˆõ‚b‚c = ";		// 2”Ô–ÚˆÈ~
				sWhere = sWhere + "'" + iEnum.Current + "'";
				iCnt++;
			}
			sWhere = sWhere +  ") \n";
			//XV“ú•t”ÍˆÍ‚Ì’Ç‰Á(sUpdDateTime)
			sWhere = sWhere   
			+	 " AND (GT02.XV“ú BETWEEN " +  sUpdDateTime + " AND " + "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n" 
			+	 "  OR  GT03.XV“ú BETWEEN " +  sUpdDateTime + " AND " + "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n" + ")"; 

			StringBuilder sbQuery_T	= null;
			StringBuilder sbQuery	= null;
			OracleDataReader reader = null;
			string sDateTime = null;

			try
			{
				//‚h‚r|‚QƒT[ƒo‚æ‚èŒ»İ‚ÌƒVƒXƒeƒ€“ú•tŠÔ‚ğæ“¾
				sbQuery_T = new StringBuilder(256);
				sbQuery_T.Append(GET_DATETIME_SELECT);

				reader = CmdSelect(sUser, conn2, sbQuery_T);
			
				if (reader.Read())
				{
					sDateTime = reader.GetString(0).Trim();
				}
				disposeReader(reader);
				reader = null;

				// ‚r‚s‚O‚P‚ÌXV‚r‚d‚k‚d‚b‚s				
				sbQuery = new StringBuilder(1024);
				sbQuery.Append(GET_ST01_Status_SELECT);
				sbQuery.Append(sWhere);

//				logWriter(sUser, INF, "SQL•ªÍ " + sbQuery);
				reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					arList.Add(reader.GetString(0).Trim());
				}
				disposeReader(reader);
				reader = null;

				if(arList.Count == 0) 
				{
					sRet[0] = "ŠY“–ƒf[ƒ^‚ª‚ ‚è‚Ü‚¹‚ñ";
					arList.Add(sRet[0]);
				}
				else
				{
					sRet[0] = "³íI—¹";
					arList.Insert(0, sRet[0]);
				}
				//¡‰ñˆ—ŠÔ‚Ì‘}“ü
				arList.Insert(1, sDateTime);
				sRet[1] = sDateTime;

				//ARRAY ‚©‚çSTRING[]‚Ö@
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
				logWriter(sUser, ERR, "o‰×ƒWƒƒ[ƒiƒ‹ó‘Ôæ“¾ ƒGƒ‰[" + sRet[0]);
			}
			catch (Exception ex)
			{
				sRet[0] = "ƒT[ƒoƒGƒ‰[F" + ex.Message;
				logWriter(sUser, ERR, "o‰×ƒWƒƒ[ƒiƒ‹ó‘Ôæ“¾ " + sRet[0]);
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
