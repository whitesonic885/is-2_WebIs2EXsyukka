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
	//  エコー金属殿向けＩＳ２サーバ上で動作する暗号済出荷データのDB登録
	//--------------------------------------------------------------------------
	// 修正履歴
	//--------------------------------------------------------------------------
	// 2011.02.01 KCL）小倉 IS2での更新内容を抽出する処理の追加　
	//--------------------------------------------------------------------------
	// 2015.03.26 BEVAS) 前田 特殊計の無意味な"0"をすべて" "に置換する処理を追加
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
			//CODEGEN: この呼び出しは、ASP.NET Web サービス デザイナで必要です。
			InitializeComponent();

			connectService();
		}

		#region コンポーネント デザイナで生成されたコード 
		
		//Web サービス デザイナで必要です。
		private IContainer components = null;
				
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{

			System.Type type = System.Type.GetType("System.String");
			System.Configuration.AppSettingsReader config = new System.Configuration.AppSettingsReader();

			// ＤＢ定義
			string sSUser = "";
			string sSPass = "";
			string sSTns  = "";
			// ＳＶＲ内ＤＢアクセス定義
			sSUser = config.GetValue("user", type).ToString();
			sSPass = config.GetValue("pass", type).ToString();
			sSTns  = config.GetValue("data", type).ToString();
			sSvUser = new string[]{sSUser,sSPass,sSTns};
			sConn = "User Id="  + sSUser
				+ ";Password=" + sSPass
				+ ";Data Source=" + sSTns;

		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
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
		 * 出荷データ登録
		 * 引数：会員ＣＤ、部門ＣＤ、出荷日...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Ins_EX_syukka(string[] sUser, string[] sData)
		{
			logWriter(sUser, INF, "エコー金属出荷登録開始");
			byte[] bText;
			string[] sList;
			int in_cnt;
			int upd_cnt = 0;
			int ins_cnt = 0;

			if (sUser[0] == "")
				sUser = sSvUser;

		//---- 暗号化ファイルの復号
			
			// 3DES 向けKEYの定義
			string key1 = "Da6JAU9Uc7JbLwSg";			//16文字(128ﾋﾞｯﾄ)
			string keyIV_w = "TBBXuA8V";				// 8文字(64ﾋﾞｯﾄ)
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
				// 暗号化された文字列を byte 配列に変換します
//				byte[] source = Encoding.UTF8.GetBytes(sData[in_cnt]);

				string  sByte = "";
				sText = sData[in_cnt];

				bText = new byte[sText.Length / 2];
				for(int iCnt = 0; iCnt < sText.Length; iCnt+=2)
				{
					sByte = sText.Substring(iCnt, 2);
					bText[iCnt/2] = Convert.ToByte(sByte,16);
				}

				// Trippe DES のサービス プロバイダを生成します
				TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();

				// 入出力用のストリームを生成します(復号)
				MemoryStream ms = new MemoryStream();
				CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor( DesKey, DesIV),
					CryptoStreamMode.Write);

				// ストリームに暗号化されたデータを書き込みます
				cs.Write(bText, 0, bText.Length);
				cs.Close();

				// 復号化されたデータを byte 配列で取得します
				byte[] destination = ms.ToArray();
				ms.Close();

				// byte 配列を文字列に変換してARRAYに保存する
				aData.Add(Encoding.UTF8.GetString(destination));
				// 復号化された1行を溜め込む　
			}
		//----　ＤＢ処理
		
//			decimal d件数;
//			string s特殊計 = " ";
			string s登録日;
			int i管理ＮＯ;
			string s日付;

			OracleConnection conn2 = null;
			string[] sRet = new string[1 + aData.Count * 2];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
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

					// 区切文字「|」にて分割　
					//string sStr = new string[aData.ToString()];
					string sStr = enumList.Current.ToString();
					sList = sStr.Split(new char[] { '|' });
					
					//※※※　登録済みチェック　※※※
					cmdQuery
						= "SELECT \"登録日\" , \"ジャーナルＮＯ\" \n"
						+ "  FROM \"ＳＴ０１出荷ジャーナル\" \n"
						+ " WHERE 会員ＣＤ = '" + sList[0] +"' \n"		
						+ "   AND 部門ＣＤ = '" + sList[1] +"' \n"
						+ "   AND 登録日   = '" + sList[2] +"' \n"
						+ "   AND \"ジャーナルＮＯ\" = '" + sList[3] +"' \n"
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					bool bRead = reader.Read();
					if(bRead)
					{
						disposeReader(reader);
						reader = null;
						//※※※　登録済み：更新　※※※ 
						cmdQuery 
							= "UPDATE \"ＳＴ０１出荷ジャーナル\" \n"
							  +    "SET 出荷日             = '" + sList[4]  +"', \n"
							  +        "お客様出荷番号     = '" + sList[5]  +"',"
							  +        "荷受人ＣＤ         = '" + sList[6]  +"',"
							  +        "電話番号１         = '" + sList[7]  +"', \n"
							  +        "電話番号２         = '" + sList[8]  +"',"
							  +        "電話番号３         = '" + sList[9]  +"',"
							  +        "ＦＡＸ番号１       = '" + sList[10] +"', \n"
							  +        "ＦＡＸ番号２       = '" + sList[11] +"',"
							  +        "ＦＡＸ番号３       = '" + sList[12] +"',"
							  +        "住所ＣＤ           = '" + sList[13] +"', \n"
							  +        "住所１             = '" + sList[14] +"',"
							  +        "住所２             = '" + sList[15] +"',"
							  +        "住所３             = '" + sList[16] +"', \n"
							  +        "名前１             = '" + sList[17] +"',"
							  +        "名前２             = '" + sList[18] +"',"
							  +        "名前３             = '" + sList[19] +"', \n"
							  +        "郵便番号           = '" + sList[20] +"',"
							  +        "着店ＣＤ           = '" + sList[21] +"',"
							  +        "着店名             = '" + sList[22] +"',"
// MOD 2015.03.26 BEVAS)前田 特殊計を空白固定にする START
//							+        "特殊計             = '" + sList[23] +"', \n"
							  +        "特殊計             = ' ', \n"
// MOD 2015.03.26 BEVAS)前田 特殊計を空白固定にする END
							  +        "荷送人ＣＤ         = '" + sList[24] +"',"
							  +        "荷送人部署名       = '" + sList[25] +"',"
							  +        "集約店ＣＤ         = '" + sList[26] +"', \n"
							  +        "発店ＣＤ           = '" + sList[27] +"',"
							  +        "発店名             = '" + sList[28] +"',"
							  +        "得意先ＣＤ         = '" + sList[29] +"', \n"
							  +        "部課ＣＤ           = '" + sList[30] +"',"
							  +        "部課名             = '" + sList[31] +"',"
							  +        "個数               =  " + sList[32] +", \n"
							  +        "才数               =  " + sList[33] +","
							  +        "重量               =  " + sList[34] +","
							  +        "ユニット           =  " + sList[35] +","
							  +        "指定日             = '" + sList[36] +"',"
							  +        "指定日区分         = '" + sList[37] +"',"
							  +        "輸送指示ＣＤ１     = '" + sList[38] +"',"
							  +        "輸送指示１         = '" + sList[39] +"', \n"
							  +        "輸送指示ＣＤ２     = '" + sList[40] +"',"
							  +        "輸送指示２         = '" + sList[41] +"',"
							  +        "品名記事１         = '" + sList[42] +"',"
							  +        "品名記事２         = '" + sList[43] +"', \n"
							  +        "品名記事３         = '" + sList[44] +"',"
							  +        "品名記事４         = '" + sList[45] +"',"
							  +        "品名記事５         = '" + sList[46] +"',"
							  +        "品名記事６         = '" + sList[47] +"',"
							  +        "品名記事７         = '" + sList[48] +"',"
							  +        "元着区分           = '" + sList[49] +"',"
//							  +        "保険金額           =  " + sList[50] +","
//							  +        "運賃               =  " + sList[51] +","
//							  +        "中継               =  " + sList[52] +","
//							  +        "諸料金             =  " + sList[53] +","
//							  +        "仕分ＣＤ           = '" + sList[54] + "', \n"
							  +        "送り状番号         = '" + sList[55] +"',"
							  +        "送り状区分         = '" + sList[56] +"',"
							  +        "送り状発行済ＦＧ   = '" + sList[57] +"',"
							  +        "出荷済ＦＧ         = '" + sList[58] +"',"
							  +        "送信済ＦＧ         = '0',"
							  +        "一括出荷ＦＧ       = '" + sList[60] +"',"
							  +        "状態               = '01',"
							  +        "詳細状態           = '  ', \n"
//							  +        "運賃エラー確認ＦＧ = '" + sList[63] +"',"
//							  +        "運賃個数           = '" + sList[64] +"',"
//   						  +        "運賃才数           = '" + sList[65] +"',"
//							  +        "運賃重量           = '" + sList[66] +"',"
//							  +        "処理０１           = '" + sList[67] +"',"　　//〜処理０６
							  +        "削除ＦＧ           = '" + sList[73] +"',"
							  +        "更新日時           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
							  +        "更新ＰＧ           = '" + sList[78] +"',"
							  +        "更新者             = '" + sList[79] +"', \n"
							  +        "削除日時           = '" + sList[80] +"', \n"
							  +        "削除ＰＧ           = '" + sList[81] +"',"
							  +        "削除者             = '" + sList[82] +"' \n"
							  + " WHERE 会員ＣＤ           = '" + sList[0]  +"' \n"
							  + "   AND 部門ＣＤ           = '" + sList[1]  +"' \n"
							  + "   AND 登録日             = '" + sList[2] +"' \n"
							  + "   AND \"ジャーナルＮＯ\" = "  + sList[3] +" \n"
							  ;
						int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
						upd_cnt++;
					}
					else
					{
						reader.Close();
						//※※※　新規登録　※※※ 
						//ジャーナルＮＯ取得
						cmdQuery
							= "SELECT \"ジャーナルＮＯ登録日\",\"ジャーナルＮＯ管理\", \n"
							+ "       TO_CHAR(SYSDATE,'YYYYMMDD') \n"
							+ "  FROM ＣＭ０２部門 \n"
							+ " WHERE 会員ＣＤ = '" + sList[0] +"' \n"		
							+ "   AND 部門ＣＤ = '" + sList[1] +"' \n"
							+ "   AND 削除ＦＧ = '0'"
							+ "   FOR UPDATE ";

						reader = CmdSelect(sUser, conn2, cmdQuery);
						reader.Read();
						s登録日   = reader.GetString(0).Trim();
						i管理ＮＯ = reader.GetInt32(1);
						s日付     = reader.GetString(2);
						reader.Close();
						if(s登録日 == s日付)
							i管理ＮＯ++;
						else
						{
							s登録日 = s日付;
							i管理ＮＯ = 1;
						}
						
						string s更新PG名 = "Ｅ自動出";
						string s更新者名 = "is2ex";
						cmdQuery 
							= "UPDATE ＣＭ０２部門 \n"
							+    "SET \"ジャーナルＮＯ登録日\"  = '" + s登録日 +"', \n"
							+        "\"ジャーナルＮＯ管理\"    = "  + i管理ＮＯ +", \n"
							+        "更新ＰＧ                  = '" + s更新PG名 +"', \n"
							+        "更新者                    = '" + s更新者名 +"', \n"
							+        "更新日時                  =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
							+ " WHERE 会員ＣＤ       = '" + sList[0] +"' \n"
							+ "   AND 部門ＣＤ       = '" + sList[1] +"' \n"
							+ "   AND 削除ＦＧ = '0'";

						int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
						disposeReader(reader);
						reader = null;
						//仕分ＣＤ取得
						string s発店ＣＤ = sList[21];
						string s着店ＣＤ = sList[27];
						string s仕分ＣＤ = " ";
						if(s発店ＣＤ.Trim().Length > 0 && s着店ＣＤ.Trim().Length > 0)
						{
							string[] sRetSiwake = Get_siwake(sUser, conn2, s発店ＣＤ, s着店ＣＤ);
							s仕分ＣＤ = sRetSiwake[1];
						}
						if(sList[5] != " " && sList[5] != null)
						{
							cmdQuery
								= "UPDATE ＳＭ０２荷受人 \n"
								+ " SET 登録ＰＧ = TO_CHAR(SYSDATE,'YYYYMMDD') \n"
								+ " WHERE 会員ＣＤ = '" + sList[0] +"' \n"
								+ " AND 部門ＣＤ   = '" + sList[1] +"' \n"
								+ " AND 荷受人ＣＤ = '" + sList[6] +"' \n"
								+ " AND 削除ＦＧ   = '0'";
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
							= "INSERT INTO \"ＳＴ０１出荷ジャーナル\" \n"
							+ "(会員ＣＤ, 部門ＣＤ, 登録日, \"ジャーナルＮＯ\", 出荷日 \n"
							+ ", お客様出荷番号, 荷受人ＣＤ \n"
							+ ", 電話番号１, 電話番号２, 電話番号３, ＦＡＸ番号１, ＦＡＸ番号２, ＦＡＸ番号３ \n"
							+ ", 住所ＣＤ, 住所１, 住所２, 住所３ \n"
							+ ", 名前１, 名前２, 名前３ \n"
							+ ", 郵便番号, 着店ＣＤ, 着店名, 特殊計 \n"
							+ ", 荷送人ＣＤ, 荷送人部署名 \n"
							+ ", 集約店ＣＤ, 発店ＣＤ, 発店名 \n"
							+ ", 得意先ＣＤ, 部課ＣＤ, 部課名 \n"
							+ ", 個数, 才数, 重量, ユニット \n"
							+ ", 指定日, 指定日区分 \n"
							+ ", 輸送指示ＣＤ１, 輸送指示１ \n"
							+ ", 輸送指示ＣＤ２, 輸送指示２ \n"
							+ ", 品名記事１, 品名記事２, 品名記事３ \n"
							+ ", 元着区分, 保険金額, 運賃, 中継, 諸料金 \n"
							+ ", 仕分ＣＤ, 送り状番号, 送り状区分 \n"
							+ ", 送り状発行済ＦＧ, 出荷済ＦＧ, 送信済ＦＧ, 一括出荷ＦＧ \n"
							+ ", 状態, 詳細状態 \n"
							+ ", 削除ＦＧ, 登録日時, 登録ＰＧ, 登録者 \n"
							+ ", 更新日時, 更新ＰＧ, 更新者 \n"
							+ ", 削除日時, 削除ＰＧ, 削除者 \n"
							+ ") \n"
							//
							+ "VALUES ('" + sList[0]  +"','" + sList[1]  +"','" + sList[2] +"'," + sList[3] +",'" + sList[4] +"', \n"
							+         "'" + sList[5]  +"','" + sList[6]  +"', \n"															//お客様出荷番号〜
							+         "'" + sList[7]  +"','" + sList[8]  +"','" + sList[9]  +"','" + sList[10] +"','" + sList[11] +"','" + sList[12] +"', \n"		//電話番号１〜
							+         "'" + sList[13] +"','" + sList[14] +"','" + sList[15] +"','" + sList[16] +"', \n"						//住所ＣＤ〜
							+         "'" + sList[17] +"','" + sList[18] +"','" + sList[19] +"', \n"										//名前１〜
// MOD 2015.03.26 BEVAS)前田 特殊計を半角スペース固定化 START
//							+         "'" + sList[20] +"','" + sList[21] +"','" + sList[22] +"','" + sList[23] +"', \n"						//郵便番号〜
							+         "'" + sList[20] +"','" + sList[21] +"','" + sList[22] +"',' ', \n"						            //郵便番号〜 特殊計は空白とする
// MOD 2015.03.26 BEVAS)前田 特殊計を半角スペース固定化 END
							+         "'" + sList[24] +"','" + sList[25] +"', \n"															//荷送人ＣＤ〜
							+         "'" + sList[26] +"','" + sList[27] +"','" + sList[28] +"', \n"										//集約店ＣＤ〜 
							+         "'" + sList[29] +"','" + sList[30] +"','" + sList[31] +"', \n"										//得意先ＣＤ〜
							+         "'" + sList[32] +"','" + sList[33] +"','" + sList[34] +"','" + sList[35] +"', \n"						//個数〜
							+         "'" + sList[36] +"','" + sList[37] +"', \n"															//指定日〜
							+         "'" + sList[38] +"','" + sList[39] +"', \n"															//輸送指示ＣＤ１〜					
							+         "'" + sList[40] +"','" + sList[41] +"', \n"															//輸送指示ＣＤ２〜
							+         "'" + sList[42] +"','" + sList[43] +"','" + sList[44] +"', \n"										//品名記事〜
							+         "'" + sList[49] +"','" + sList[50] +"','" + sList[51] +"','" + sList[52] +"','" + sList[53] +"', \n"	//元着区分〜
							+         "'" + s仕分ＣＤ +"','" + sList[55] +"','" + sList[56] +"', \n"										//仕分ＣＤ〜
							+         "'" + sList[57] +"','" + sList[58] +"','" + '0' +"','" + sList[60] +"', \n"							//送り状発行済ＦＧ〜
							+         "'" + sList[61] +"','" + sList[62] +"', \n"															//状態〜
							+         "'" + sList[73] +"',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sList[75] +"','" + sList[76] +"', \n"	//削除ＦＧ〜
							+         "'" + sList[77] +"','" + sList[78] +"','" + sList[79]  +"', \n"										//更新日時〜		
							+         "'" + sList[80] +"','" + sList[81] +"','" + sList[82]  +"')";											//削除日時〜		

						iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
						ins_cnt++;
					}
					
				}
				tran.Commit();
				string sLogInfo = "登録件数：" + ins_cnt + " 件、更新件数：" + upd_cnt + " 件が正常終了しました。";　
				logWriter(sUser, INF, sLogInfo);
				sRet[0] = "正常終了";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
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
		 * 仕分ＣＤ取得
		 * 引数：会員ＣＤ、部門ＣＤ、ＤＢ接続、発店、着店
		 * 戻値：ステータス、仕分ＣＤ
		 *
		 *********************************************************************/
		private static string GET_SIWAKE_SELECT
			= "SELECT 仕分ＣＤ \n"
			+ " FROM ＣＭ１７仕分 \n"
			;

		private String[] Get_siwake(string[] sUser, OracleConnection conn2, string sHatuCd, string sTyakuCd)
		{

			string[] sRet = new string[2];

			string cmdQuery = GET_SIWAKE_SELECT
				+ " WHERE 発店所ＣＤ = '" + sHatuCd + "' \n"
				+ " AND 着店所ＣＤ = '" + sTyakuCd + "' \n"
				+ " AND 削除ＦＧ = '0' \n"
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
				sRet[0] = "仕分ＣＤを決められませんでした";
				sRet[1] = " ";
			}
			disposeReader(reader);
			reader = null;

			return sRet;
		}

		/*********************************************************************
		 * 出荷ジャーナル状態獲得
		 * 引数：会員ＣＤ[]、部門ＣＤ、前回取得日時
		 * 戻値：ステータス、今回取得日時、送り状番号、運賃、状態、詳細状態
		 *
		 *********************************************************************/
		private static string GET_ST01_Status_SELECT
			= "	SELECT \n"
			+         " ST01.送り状番号 || '|' "
			+       "|| NVL(ST01.運賃,'0') || '|' "
			+       "|| ST01.状態 || '|' "
			+       "|| NVL(ST01.詳細状態,' ') || '|' " 
			+       "|| NVL(ST01.中継,'0')  || '|' "
			+       "|| NVL(ST01.諸料金,'0') "
			+ " FROM ＳＴ０１出荷ジャーナル ST01 \n"
			+ " LEFT JOIN ＧＴ０２配完 GT02 \n"
			+ " ON   ST01.送り状番号 = GT02.原票番号 \n"
			+ " LEFT JOIN ＧＴ０３原票運賃 GT03 \n" 
			+ " ON   ST01.送り状番号 = GT03.原票番号 \n"
			;

		private static string GET_DATETIME_SELECT
			= "	SELECT TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') FROM DUAL";


		[WebMethod]
		public String[] Get_St01Status(string[] sUser, string[] sKaiinCD, string sUpdDateTime)
		{
			logWriter(sUser, INF, "エコー金属出荷ジャーナル状態取得開始");

			string[] sRet = new string[2];

			if (sUser[0] == "")
				sUser = sSvUser;

			OracleConnection conn2 = null;

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
			
			//ＩＳ−２サーバより、状態の更新されたＳＴ０１出荷ジャーナル情報を取得	
			ArrayList arList = new ArrayList();

			// Array上の会員ＣＤから　WHERE文を生成
			IEnumerator iEnum = sKaiinCD.GetEnumerator();
			iEnum.MoveNext();								//先頭は完了情報
			int iCnt = 0;
			string sWhere = " WHERE (ST01.会員ＣＤ = ";		

			while (iEnum.MoveNext())
			{
				if (iCnt > 0)
					sWhere = sWhere + " OR \n ST01.会員ＣＤ = ";		// 2番目以降
				sWhere = sWhere + "'" + iEnum.Current + "'";
				iCnt++;
			}
			sWhere = sWhere +  ") \n";
			//更新日付範囲の追加(sUpdDateTime)
			sWhere = sWhere   
			+	 " AND (GT02.更新日時 BETWEEN " +  sUpdDateTime + " AND " + "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n" 
			+	 "  OR  GT03.更新日時 BETWEEN " +  sUpdDateTime + " AND " + "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n" + ")"; 

			StringBuilder sbQuery_T	= null;
			StringBuilder sbQuery	= null;
			OracleDataReader reader = null;
			string sDateTime = null;

			try
			{
				//ＩＳ−２サーバより現在のシステム日付時間を取得
				sbQuery_T = new StringBuilder(256);
				sbQuery_T.Append(GET_DATETIME_SELECT);

				reader = CmdSelect(sUser, conn2, sbQuery_T);
			
				if (reader.Read())
				{
					sDateTime = reader.GetString(0).Trim();
				}
				disposeReader(reader);
				reader = null;

				// ＳＴ０１の更新ＳＥＬＥＣＴ				
				sbQuery = new StringBuilder(1024);
				sbQuery.Append(GET_ST01_Status_SELECT);
				sbQuery.Append(sWhere);

//				logWriter(sUser, INF, "SQL分析 " + sbQuery);
				reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					arList.Add(reader.GetString(0).Trim());
				}
				disposeReader(reader);
				reader = null;

				if(arList.Count == 0) 
				{
					sRet[0] = "該当データがありません";
					arList.Add(sRet[0]);
				}
				else
				{
					sRet[0] = "正常終了";
					arList.Insert(0, sRet[0]);
				}
				//今回処理時間の挿入
				arList.Insert(1, sDateTime);
				sRet[1] = sDateTime;

				//ARRAY からSTRING[]へ　
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
				logWriter(sUser, ERR, "出荷ジャーナル状態取得 エラー" + sRet[0]);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, "出荷ジャーナル状態取得 " + sRet[0]);
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
