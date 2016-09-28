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
	// 修正履歴
	//--------------------------------------------------------------------------
	// ADD 2007.04.28 東都）高木 オブジェクトの破棄
	//	disposeReader(reader);
	//	reader = null;
	//--------------------------------------------------------------------------
	// DEL 2007.05.10 東都）高木 未使用関数のコメント化
	//	logFileOpen(sUser);
	//	userCheck2(conn2, sUser);
	//	logFileClose();
	//--------------------------------------------------------------------------
	// ADD 2007.10.19 東都）高木 端末バージョン管理
	//--------------------------------------------------------------------------
	// MOD 2009.12.24 東都）高木 wakeupDBの高速化 
	// MOD 2009.12.24 東都）高木 接続リトライ機能の追加 
	// MOD 2009.12.24 東都）高木 バージョンチェック設定値の追加 
	// MOD 2009.12.24 東都）高木 イベントログの廃止 
	//--------------------------------------------------------------------------
	// MOD 2010.01.25 東都）高木 接続リトライ機能の追加 
	// MOD 2010.02.19 東都）高木 接続リトライ機能の調整 
	// MOD 2010.06.17 東都）高木 接続リトライ機能の調整 
	//保留 MOD 2010.06.17 東都）高木 ＯＳ情報の取得機能の追加 
	// MOD 2010.08.16 東都）高木 ORA-12152 対応の追加 
	//==========================================================================
	// 2010.11.17 KCL）小倉 エコー金属殿向けとして取込＆コメント行の削除
	//==========================================================================
	//
	public class CommService : System.Web.Services.WebService
	{
		private const string sＤＢ通信エラー
						= "サーバが混雑しています。数秒後に再度実行して下さい。";
		private const string sＤＢ一意制約エラー
						= "同一のコードが既に他の端末より登録されています。\r\n"
						+ "再度、最新データを呼び出して更新してください。";

		protected static string sConn    = "";
		protected static int    iLogMode = 0;		//ログモード
		protected static string sLogPath = "";
		protected static int    iRetry   = 0; //リトライ回数の設定（初期値：０）
		protected static string sMinVer  = "2.7";
		protected static string[] sSvUser = new string[]{"","",""};

		protected const int ERR = 1;
		protected const int INF = 2;
		protected const int INF_SQL = 3;


		// Ｗｅｂサービス変数
		private static is2EXlogout.Service1 sv_logout = null;

		private static Encoding enc = Encoding.GetEncoding("shift-jis");

		public CommService()
		{
			//CODEGEN: この呼び出しは、ASP.NET Web サービス デザイナで必要です。
			InitializeComponent();
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
		 * スタンバイ状態回避用メソッド
		 * 引数：なし
		 * 戻値：なし
		 *
		 *********************************************************************/

		private static string WAKEUPDB_SELECT
			= "SELECT 1 FROM DUAL";
		[WebMethod]
		public string wakeupDB()
		{
			// ログ出力メソッドの起動
			if(sv_logout == null)
			{
				try
				{
					sv_logout = new is2EXlogout.Service1();
					// Timeoutのデフォルトは100000（１００秒）
					sv_logout.Timeout = 1000; // １秒
					int iRet = sv_logout.LogOut("");
				}
				catch (Exception)
				{
					//エラーは無視する
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
					return "wakeupDB：接続エラー１";
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
				sRet = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet);
			}
			finally
			{
				if(conn2 != null){
					if(conn2.State == ConnectionState.Open){
						try{
							conn2.Close();
						}catch (Exception ex){
							logWriter(sUser, ERR, "wakeupDB：切断エラー：" + ex.Message);
						}
					}else{
						logWriter(sUser, INF, "wakeupDB：State:" + conn2.State);
					}

					try{
						conn2.Dispose();
					}catch (Exception ex){
						logWriter(sUser, ERR, "wakeupDB：破棄エラー：" + ex.Message);
					}
					conn2 = null;
				}
			}

			return sRet;
		}

		[WebMethod]
		public string wakeupDB2(int iConCnt)
		{
			// ログ出力メソッドの起動
			if(sv_logout == null)
			{
				try
				{
					sv_logout = new is2EXlogout.Service1();
					// Timeoutのデフォルトは100000（１００秒）
					sv_logout.Timeout = 1000; // １秒
					int iRet = sv_logout.LogOut("");
				}
				catch (Exception)
				{
					//エラーは無視する
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
					return "wakeupDB2：接続エラー１";
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
							logWriter(sUser, INF, "wakeupDB2：（AUDSID:"+reader.GetDecimal(0)+"）");
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
							; // セッションは強制終了されました
						}else if(ex.Number == 01012){
							; // ログオンされていません
						}else if(ex.Number == 03113){
							; // 通信チャネルでファイルの終わりが検出されました
						}else if(ex.Number == 03114){
							; // Oracle に接続されていません
						}else if(ex.Number == 03135){
							; // 接続が失われました
						}else if(ex.Number == 12152){
							; // TNS: ブレーク・メッセージの送信に失敗しました。
						}else if(ex.Number == 12571){
							; // TNS: パケット・ライターに障害が発生しました
						}else{
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				sRet = "サーバエラー：" + ex.Message;
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
							logWriter(sUser, ERR, "wakeupDB2：切断エラー：" + ex.Message);
						}
					}else{
						logWriter(sUser, INF, "wakeupDB2：State:" + conWakeup[iCnt].State);
					}

					try{
						conWakeup[iCnt].Dispose();
					}catch (Exception ex){
						logWriter(sUser, ERR, "wakeupDB2：破棄エラー：" + ex.Message);
					}
					conWakeup[iCnt] = null;
				}
			}

			return sRet;
		}

		/*********************************************************************
		 * コネクトサービス
		 * 引数：なし
		 * 戻値：なし
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
		 * ＤＢ接続
		 * 引数：なし
		 * 戻値：OracleConnection
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
								//リトライ時のみログ出力
								if(iCon > 0){
									logWriter(sUser, INF, "connect2["+iCon+"]（AUDSID:"+reader.GetDecimal(0)+"）");
								}
							}
							cmd.Dispose();
							cmd    = null;
							disposeReader(reader);
							reader = null;
							//接続に成功した場合には、接続を保持してループからぬける
							conn2 = conRetry[iCon];
							//リトライ時のみログ出力
							if(iCon > 0){
								logWriter(sUser, INF, "connect2["+iCon+"]成功");
							}
							break;
						}catch (OracleException ex){
							logWriter(sUser, ERR, "connect2["+iCon+"]オープン：" + ex.Message);
							if(ex.Number == 00028){
								; // セッションは強制終了されました
							}else if(ex.Number == 01012){
								; // ログオンされていません
							}else if(ex.Number == 03113){
								; // 通信チャネルでファイルの終わりが検出されました
							}else if(ex.Number == 03114){
								; // Oracle に接続されていません
							}else if(ex.Number == 03135){
								; // 接続が失われました
							}else if(ex.Number == 12152){
								; // TNS: ブレーク・メッセージの送信に失敗しました。
							}else if(ex.Number == 12571){
								; // TNS: パケット・ライターに障害が発生しました
							}else{
								break;
							}
						}catch (Exception ex){
							logWriter(sUser, ERR, "connect2["+iCon+"]オープン：" + ex.Message);
						}
						System.Threading.Thread.Sleep(10000);	//１０秒待ち
					}
					//成功した接続以外は、切断し破棄する
					for(iCon = 0; iCon < iRetry; iCon++){
						if(conRetry[iCon] == null)  continue;
						if(conRetry[iCon] == conn2) continue;

						if(conRetry[iCon].State == ConnectionState.Open){
							try{
								logWriter(sUser, INF, "connect2["+iCon+"]クローズ");
								conRetry[iCon].Close();
							}catch (Exception ex){
								logWriter(sUser, ERR, "connect2["+iCon+"]クローズ：" + ex.Message);
							}
						}

						try{
							logWriter(sUser, INF, "connect2["+iCon+"]破棄");
							conRetry[iCon].Dispose();
						}catch (Exception ex){
							logWriter(sUser, ERR, "connect2["+iCon+"]破棄：" + ex.Message);
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
					// ＤＢオープンがされていない場合には待つ
					for(int iWait = 0; conn2.State != ConnectionState.Open && iWait < 20; iWait++)
					{
						logWriter(sUser, INF, "ＤＢオープン待ち");
						System.Threading.Thread.Sleep(3000);
						logWriter(sUser, INF, "conn2.State:" + conn2.State);
					}
				}
				catch (InvalidOperationException ex) 
				{
						logWriter(sUser, ERR, "接続エラー：" + ex.Message);

					// ＤＢ切断
					disconnect2(sUser, conn2);
					// ＤＢ接続
					if(conn2.State != ConnectionState.Closed)
						logWriter(sUser, INF, "conn2.State:" + conn2.State);
					if(conn2.State == ConnectionState.Closed){
						conn2.Open();
					}
					if(conn2.State != ConnectionState.Open)
						logWriter(sUser, INF, "conn2.State:" + conn2.State);
					// ＤＢオープンがされていない場合には待つ
					for(int iWait = 0; conn2.State != ConnectionState.Open && iWait < 20; iWait++)
					{
						logWriter(sUser, INF, "ＤＢオープン待ち");
						System.Threading.Thread.Sleep(3000);
						logWriter(sUser, INF, "conn2.State:" + conn2.State);
					}
				}
			}
			catch (Exception ex) 
			{
				{
					logWriter(sUser, ERR, "接続エラー：" + ex.Message);
				}
				return null;
			}

			return conn2;
		}

		/*********************************************************************
		 * ＤＢ切断
		 * 引数：OracleConnection conn
		 * 戻値：なし
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
					logWriter(sUser, ERR, "切断エラー：" + ex.Message);
				}
			}

			try
			{
				conn2.Dispose();
			}
			catch (Exception ex) 
			{
				logWriter(sUser, ERR, "破棄エラー：" + ex.Message);
			}
			conn2 = null;
		}

		/*********************************************************************
		 * オラクルのエラーメッセージの変換
		 * 引数：オラクルエクセプション
		 * 戻値：エラーメッセージ
		 *
		 *********************************************************************/
		protected string chgDBErrMsg(string[] sUser, OracleException ex)
		{
			string sRet = "";
			switch(ex.Number)
			{
				case    1:	// 一意制約（string.string）に反しています
					sRet = sＤＢ一意制約エラー;
					break;
				case 3113:	// 通信チャネルでファイルの終わりが検出されました。
					sRet = sＤＢ通信エラー;
					break;
				case 3114:	// Oracle に接続されていません。
					sRet = sＤＢ通信エラー;
					break;
				case 3135:	// 接続が失われました
					sRet = sＤＢ通信エラー;
					break;
				case 12571:	// TNS: パケット・ライターに障害が発生しました
					sRet = sＤＢ通信エラー;
					break;
				default:
					sRet = "ＤＢエラー：" + ex.Message;
					break;
			}
			logWriter(sUser, ERR, sRet);

			return sRet;
		}
		/*********************************************************************
		 * 会員チェック
		 * 引数：引数：会員ＣＤ、利用者ＣＤ、端末ＩＤ
		 * 戻値：エラーメッセージ
		 *
		 *********************************************************************/

		/*********************************************************************
		 * ＳＥＬＥＣＴの実行（String版）
		 * 引数：ＤＢコネクション、実行ＳＱＬ
		 * 戻値：オラクルＲｅａｄｅｒ
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
				logWriter(sUser, ERR, "エラー番号：" + ex.Number);
				logWriter(sUser, ERR, "StackTrace:\n" + ex.StackTrace);
				throw ex;
			}
		}

		/*********************************************************************
		 * ＵＰＤＡＴＥの実行（String版）
		 * 引数：ＤＢコネクション、実行ＳＱＬ
		 * 戻値：更新件数
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
				logWriter(sUser, ERR, "エラー番号：" + ex.Number);
				logWriter(sUser, ERR, "StackTrace:\n" + ex.StackTrace);
				throw ex;
			}
		}

		/*********************************************************************
		 * ＳＥＬＥＣＴの実行（StringBuilder版）
		 * 引数：ＤＢコネクション、実行ＳＱＬ
		 * 戻値：オラクルＲｅａｄｅｒ
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
				logWriter(sUser, ERR, "エラー番号：" + ex.Number);
				logWriter(sUser, ERR, "StackTrace:\n" + ex.StackTrace);
				throw ex;
			}

		}

		/*********************************************************************
		 * ＵＰＤＡＴＥの実行（StringBuilder版）
		 * 引数：ＤＢコネクション、実行ＳＱＬ
		 * 戻値：更新件数
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
				logWriter(sUser, ERR, "エラー番号：" + ex.Number);
				logWriter(sUser, ERR, "StackTrace:\n" + ex.StackTrace);
				throw ex;
			}
		}


		/*********************************************************************
		 * ログファイル書き込み
		 * 引数：ログ出力フラグ、ログ
		 * 戻値：なし
		 *
		 *********************************************************************/

		protected void logWriter2(string[] sUser, int iMode, string sLog)
		{
			// エラー以外の時は、未出力
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
						//端末ＩＤ
						//（存在しない場合、ＩＰアドレスを使用する）
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

					// 日時
					sw.Write("["+ System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") +"]");

					// ＩＰアドレス
					sw.Write("["+ Context.Request.UserHostName +"]");

					// 端末ＩＤ
					if (sUser != null && sUser[2] != null && sUser[2].Length > 0)
						sw.Write("["+ sUser[2] +"]");
					else
						sw.Write("[-]");

					// 会員ＣＤ
					if (sUser != null && sUser[0] != null && sUser[0].Length > 0)
						sw.Write("["+ sUser[0] +"]");
					else
						sw.Write("[-]");

					// 利用者ＣＤ
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

					// アプリケーションパス
					sw.Write("[" + Context.Request.ApplicationPath + "]");

					// ログ
					sw.WriteLine(sLog);
					sw.Flush();

				}
				catch(Exception )
				{
					;
				}
				finally
				{
					// ファイルクローズ
					if(sw != null) sw.Close();
					sw = null;
					if(fs != null) fs.Close();
					fs = null;
				}
			}
		}

		/*********************************************************************
		 * ログファイル書き込み
		 * 引数：ログ出力フラグ、ログ
		 * 戻値：なし
		 *
		 *********************************************************************/
		private static bool gbLogOutErr = false;
		protected void logWriter(string[] sUser, int iMode, string sLog)
		{
			if (iMode <= iLogMode)
			{
				if(sLog == null || sLog.Length == 0) return;
				//通常運用時には、
				//[正常終了]や[更新]など４文字以下の場合にはログを出力しない
				if(iMode < INF_SQL && sLog.Length <= 4) return;

				int iRet = 0;
				StringBuilder sbBuff = new StringBuilder(2048);
				try
				{

					// 日時
					sbBuff.Append("["+ System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") +"]");

					// ＩＰアドレス
					sbBuff.Append("["+ Context.Request.UserHostName +"]");

					// 端末ＩＤ
					if (sUser != null && sUser[2] != null && sUser[2].Length > 0)
						sbBuff.Append("["+ sUser[2] +"]");
					else
						sbBuff.Append("[-]");

					// 会員ＣＤ
					if (sUser != null && sUser[0] != null && sUser[0].Length > 0)
						sbBuff.Append("["+ sUser[0] +"]");
					else
						sbBuff.Append("[-]");

					// 利用者ＣＤ
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
					// アプリケーションパス
					sbBuff.Append("[" + Context.Request.ApplicationPath + "]");

					// ログ
					sbBuff.Append(sLog);

					if(sv_logout == null){
						sv_logout = new  is2EXlogout.Service1();
						// Timeoutのデフォルトは100000（１００秒）
						sv_logout.Timeout = 1000; // １秒
					}
					if(sv_logout == null){
						logWriter2(sUser, ERR, "サーバエラー：sv_logout == null");
						logWriter2(sUser, iMode, sbBuff.ToString());
						return;
					}
					iRet = sv_logout.LogOut(sbBuff.ToString());
					if(iRet == 0) return;
				}
				catch(System.Net.WebException ex)
				{
					//初回のみ出力する
					if(gbLogOutErr == false)
					{
						gbLogOutErr = true;
						logWriter2(sUser, ERR, "サーバエラー：\n"  + ex.ToString());
						logWriter2(sUser, iMode, sbBuff.ToString());
					}
					return;
				}
				catch(Exception ex)
				{
					logWriter2(sUser, ERR, "サーバエラー：\n"  + ex.ToString());
					logWriter2(sUser, iMode, sLog);
					return;
				}
				finally
				{
					sbBuff = null;
				}

				// エラー以外の時は、未出力
				if(iMode != ERR) return;

				// ログ出力用Ｗｅｂサービスが使用できない場合、
				// ファイルに直接出力
				if(iRet != 0) 
					logWriter2(sUser, ERR, "サーバエラー：sv_logout.LogOut：" + iRet);
				logWriter2(sUser, iMode, sLog);
			}
		}

		/*********************************************************************
		 * イベントログ出力
		 * 引数：ログ出力文字列
		 * 戻値：無し-
		 *
		 *********************************************************************/
		private static string gsAppSrc = System.Web.HttpRuntime.AppDomainAppVirtualPath.Replace('/','_');

		/*********************************************************************
		 * リーダオブジェクト破棄
		 * 引数：なし
		 * 戻値：なし
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
