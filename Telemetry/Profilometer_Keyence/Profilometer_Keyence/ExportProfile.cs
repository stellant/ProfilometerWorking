using System;
using System.IO;
using System.Text;
using Profilometer_Keyence.Datas;
using Profilometer_Keyence;

namespace LJV7_IF_Test
{
    /// <summary>
    /// プロファイルエキスポートクラス
    /// </summary>
    public static class ExportProfile
    {
        /// <summary>
        /// ファイル出力
        /// </summary>
		static public bool ExportOne(ProfileData[] datas, int profileNo, string fileName)
        {
            try
            {
                Encoding unicode = System.Text.Encoding.GetEncoding("utf-16");
				using (StreamWriter sw = new StreamWriter(fileName, false, unicode))
                {
                    try
                    {
						if (datas[0] == null) return false;
						sw.WriteLine(GetProfileFileString(datas[profileNo]).ToString());
                    }
                    finally
                    {
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // ファイルの保存失敗
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.Assert(false);
                return false;
            }

            return true;
        }

        /// <summary>
        /// ファイル出力
        /// </summary>
		static public bool ExportAll(ProfileData[] datas, int profileNo, int outProfileCount, string fileName)
        {
            try
            {
                Encoding unicode = System.Text.Encoding.GetEncoding("utf-16");

                // 複数プロファイル受信した場合は全部に対してファイル出力？
				for (int i = profileNo; i < profileNo + outProfileCount; i++)
                {
					using (StreamWriter sw = new StreamWriter(fileName + i, false, unicode))
                    {
                        try
                        {
                            sw.WriteLine(datas[i].ToString());
                        }
                        finally
                        {
                            sw.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ファイルの保存失敗
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.Assert(false);
                return false;
            }

            return true;
        }


        //@@@エンベロープ数等の制御のメソッドも追加必要
        /// <summary>
        /// １プロファイルデータの出力
        /// </summary>
        /// <param name="info"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        static private StringBuilder GetProfileFileString(ProfileData data)
        {
            StringBuilder sb = new StringBuilder();

			double ratio = Define.PROFILE_UNIT_MM;
            // データ位置の算出
			double posX = data.ProfInfo.lXStart * ratio;
			double deltaX = data.ProfInfo.lXPitch * ratio;

			int singleProfileCount = data.ProfInfo.wProfDataCnt;
			bool isEnvelope = (data.ProfInfo.byEnvelope == 1);
			bool isTwoHead = (data.ProfInfo.byProfileCnt == 2);

			double headAMax = 0.0;
			double headAMin = 0.0;
			double headBMax = 0.0;
			double headBMin = 0.0;
            for (int i = 0; i < singleProfileCount; i++)
            {
				headAMax = (data.ProfDatas[i] * ratio);

				if (isEnvelope)
				{
					if (isTwoHead)
					{
						headAMin = data.ProfDatas[i + singleProfileCount] * ratio;
						headBMax = data.ProfDatas[i + singleProfileCount * 2] * ratio;
						headBMin = data.ProfDatas[i + singleProfileCount * 3] * ratio;
						sb.AppendFormat("{0,0:f3}\t{1,0:f3}\t{2,0:f3}\t{3,0:f3}\t{4,0:f3}", (posX + deltaX * i), headAMax, headAMin, headBMax, headBMin).AppendLine();
					}
					else
					{
						headAMin = data.ProfDatas[i + singleProfileCount] * ratio;
						sb.AppendFormat("{0,0:f3}\t{1,0:f3}\t{2,0:f3}", (posX + deltaX * i), headAMax, headAMin).AppendLine();
					}
				}
				else
				{
					if (isTwoHead)
					{
						headBMax = data.ProfDatas[i + singleProfileCount] * ratio;
						sb.AppendFormat("{0,0:f3}\t{1,0:f3}\t{2,0:f3}", (posX + deltaX * i), headAMax, headBMax).AppendLine();
					}
					else
					{
						headAMin = double.NaN;
						headBMax = double.NaN;
						headBMin = double.NaN;
						sb.AppendFormat("{0,0:f3}\t{1,0:f3}", (posX + deltaX * i), headAMax).AppendLine();
					}
				}
            }

            return sb;
        }
    }
}
