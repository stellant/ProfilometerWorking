using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Net;
using System.Runtime.InteropServices;

namespace Profilometer_Keyence_WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Profilometer" in code, svc and config file together.
    public class Profilometer : IProfilometer
    {
        private LJV7IF_ETHERNET_CONFIG _ethernetConfig;
        private List<MeasureData> _measureDatas;
        private int _currentDeviceId;
        private LJV7IF_PROFILE_INFO[] _profileInfo;
        private XmlElement _result = null;
        private HighSpeedDataCallBack _callbackOnlyCount = new HighSpeedDataCallBack(CountProfileReceive);
        public XmlElement GetMeasurementValue(string ipAddress, string port, string type)
        {
            try
            {
                if (type.Trim().Equals("measurement"))
                {
                    InitializeCommunication(ipAddress, port, type);
                    _result = GetMeasurementValues();
                    FinalizeCommunication(ipAddress, port, type);
                }
                /*else if (type.Trim().Equals("highbatchoff"))
                {
                    HighSpeedDataEthernetCommunicationInitalize(ipAddress,port);
                    PreStartHighSpeedDataCommunication();
                    StartHighSpeedDataCommunication();
                    _result = GetHighSpeedProfileValues();
                    StopHighSpeedDataCommunication();
                    HighSpeedDataCommunicationFinalize();
                }
                else if (type.Trim().Equals("highbatchon"))
                {
                    HighSpeedDataEthernetCommunicationInitalize(ipAddress, port);
                    PreStartHighSpeedDataCommunication();
                    StartHighSpeedDataCommunication();
                    _result = GetBatchHighSpeedProfileValues();
                    StopHighSpeedDataCommunication();
                    HighSpeedDataCommunicationFinalize();
                }*/
                else if (type.Trim().Equals("advancedbatchoff"))
                {
                    InitializeCommunication(ipAddress, port, type);
                    _result = GetProfileAdvanceValues();
                    FinalizeCommunication(ipAddress, port, type);
                }
                else if (type.Trim().Equals("advancedbatchon"))
                {
                    InitializeCommunication(ipAddress, port, type);
                    _result = GetBatchProfileAdvanceValues();
                    FinalizeCommunication(ipAddress, port, type);
                }
                else
                {
                    throw new Exception("No Service Written to Fetch Data for the Specified Type \"" + type.ToString() + "\"");
                }
            }
            catch (Exception ex)
            {
                _result = GetExceptionXML(ex.ToString());
            }
            finally
            {
                FinalizeCommunication(ipAddress, port, type);
            }
            return _result;
        }
        private XmlElement GetMeasurementValueXML(MeasureData s)
        {
            XmlDocument document = new XmlDocument();
            XmlNode root = document.CreateElement("Profilometer");
            document.AppendChild(root);
            XmlNode child = document.CreateElement("MeasurementValues");
            root.AppendChild(child);
            XmlNode dataCount = document.CreateElement("MeasurementCount");
            dataCount.InnerText = NativeMethods.MeasurementDataCount.ToString();
            child.AppendChild(dataCount);
            for (int i = 0; i < NativeMethods.MeasurementDataCount; i++)
            {
                XmlNode dataValue = document.CreateElement("MeasurementValue");
                dataValue.InnerText = string.Format("OUT {0:d2}:\t{1,0:f4}\r\n", (i + 1), s.Data[i].fValue);
                child.AppendChild(dataValue);
            }
            XmlNode date = document.CreateElement("DateTime");
            date.InnerText = convertTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString());
            child.AppendChild(date);
            return document.DocumentElement;
        }
        private XmlElement GetProfileAdvanceValueXML(MeasureData s, List<ProfileData> p)
        {
            XmlDocument document = new XmlDocument();
            XmlNode root = document.CreateElement("Profilometer");
            document.AppendChild(root);
            XmlNode dataCount = document.CreateElement("MeasurementCount");
            dataCount.InnerText = NativeMethods.MeasurementDataCount.ToString();
            root.AppendChild(dataCount);
            XmlNode measurementChild = document.CreateElement("MeasurementValues");
            for (int i = 0; i < NativeMethods.MeasurementDataCount; i++)
            {
                XmlNode dataValue = document.CreateElement("MeasurementValue");
                dataValue.InnerText = string.Format("OUT {0:d2}:\t{1,0:f4}\r\n", (i + 1), s.Data[i].fValue);
                measurementChild.AppendChild(dataValue);
            }
            root.AppendChild(measurementChild);
            XmlNode profileCount = document.CreateElement("ProfileCount");
            profileCount.InnerText = p.Count.ToString();
            root.AppendChild(profileCount);
            XmlNode profileChild = document.CreateElement("ProfileAdvanceValues");
            root.AppendChild(profileChild);
            XmlNode profileInfo = document.CreateElement("ProfileInfo");
            profileInfo.InnerText = p[0].ProfInfo.ToString();
            profileChild.AppendChild(profileInfo);
            foreach (ProfileData profile in p)
            {
                XmlNode profileData = document.CreateElement("ProfileData");
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < profile.ProfDatas.Length; i++)
                {
                    stringBuilder.AppendFormat("{0}\t", profile.ProfDatas[i]);
                }
                profileData.InnerText = stringBuilder.ToString();
                profileChild.AppendChild(profileData);
            }
            XmlNode date = document.CreateElement("DateTime");
            date.InnerText = convertTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString());
            root.AppendChild(date);
            return document.DocumentElement;
        }
        private XmlElement GetBatchProfileAdvanceValueXML(MeasureData b, MeasureData s, List<ProfileData> p)
        {
            XmlDocument document = new XmlDocument();
            XmlNode root = document.CreateElement("Profilometer");
            document.AppendChild(root);
            XmlNode batchDataCount = document.CreateElement("BatchMeasurementCount");
            batchDataCount.InnerText = NativeMethods.MeasurementDataCount.ToString();
            root.AppendChild(batchDataCount);
            XmlNode batchMeasurementChild = document.CreateElement("BatchMeasurementValues");
            for (int i = 0; i < NativeMethods.MeasurementDataCount; i++)
            {
                XmlNode batchDataValue = document.CreateElement("BatchMeasurementValue");
                batchDataValue.InnerText = string.Format("OUT {0:d2}:\t{1,0:f4}\r\n", (i + 1), s.Data[i].fValue);
                batchMeasurementChild.AppendChild(batchDataValue);
            }
            root.AppendChild(batchMeasurementChild);
            XmlNode dataCount = document.CreateElement("MeasurementCount");
            dataCount.InnerText = NativeMethods.MeasurementDataCount.ToString();
            root.AppendChild(dataCount);
            XmlNode measurementChild = document.CreateElement("MeasurementValues");
            for (int i = 0; i < NativeMethods.MeasurementDataCount; i++)
            {
                XmlNode dataValue = document.CreateElement("MeasurementValue");
                dataValue.InnerText = string.Format("OUT {0:d2}:\t{1,0:f4}\r\n", (i + 1), s.Data[i].fValue);
                measurementChild.AppendChild(dataValue);
            }
            root.AppendChild(measurementChild);
            XmlNode profileCount = document.CreateElement("ProfileCount");
            profileCount.InnerText = p.Count.ToString();
            root.AppendChild(profileCount);
            XmlNode profileChild = document.CreateElement("ProfileBatchAdvanceValues");
            root.AppendChild(profileChild);
            XmlNode profileInfo = document.CreateElement("ProfileInfo");
            profileInfo.InnerText = p[0].ProfInfo.ToString();
            profileChild.AppendChild(profileInfo);
            foreach (ProfileData profile in p)
            {
                XmlNode profileData = document.CreateElement("ProfileData");
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < profile.ProfDatas.Length; i++)
                {
                    stringBuilder.AppendFormat("{0}\t", profile.ProfDatas[i]);
                }
                profileData.InnerText = stringBuilder.ToString();
                profileChild.AppendChild(profileData);
            }
            XmlNode date = document.CreateElement("DateTime");
            date.InnerText = convertTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString());
            root.AppendChild(date);
            return document.DocumentElement;
        }
        private XmlElement GetHighSpeedProfileValueXML(List<ProfileData> p) 
        {
            XmlDocument document = new XmlDocument();
            XmlNode root = document.CreateElement("Profilometer");
            document.AppendChild(root);
            XmlNode profileCount = document.CreateElement("ProfileCount");
            profileCount.InnerText = p.Count.ToString();
            root.AppendChild(profileCount);
            XmlNode profileChild = document.CreateElement("ProfileHighSpeedValues");
            root.AppendChild(profileChild);
            XmlNode profileInfo = document.CreateElement("ProfileInfo");
            profileInfo.InnerText = p[0].ProfInfo.ToString();
            profileChild.AppendChild(profileInfo);
            foreach (ProfileData profile in p)
            {
                XmlNode profileData = document.CreateElement("ProfileData");
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < profile.ProfDatas.Length; i++)
                {
                    stringBuilder.AppendFormat("{0}\t", profile.ProfDatas[i]);
                }
                profileData.InnerText = stringBuilder.ToString();
                profileChild.AppendChild(profileData);
            }
            XmlNode date = document.CreateElement("DateTime");
            date.InnerText = convertTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString());
            root.AppendChild(date);
            return document.DocumentElement;
        }
        private XmlElement GetBatchHighSpeedProfileValueXML(List<ProfileData> p) 
        {
             XmlDocument document = new XmlDocument();
            XmlNode root = document.CreateElement("Profilometer");
            document.AppendChild(root);
            XmlNode profileCount = document.CreateElement("ProfileCount");
            profileCount.InnerText = p.Count.ToString();
            root.AppendChild(profileCount);
            XmlNode profileChild = document.CreateElement("ProfileBatchHighSpeedValues");
            root.AppendChild(profileChild);
            XmlNode profileInfo = document.CreateElement("ProfileInfo");
            profileInfo.InnerText = p[0].ProfInfo.ToString();
            profileChild.AppendChild(profileInfo);
            foreach (ProfileData profile in p)
            {
                XmlNode profileData = document.CreateElement("ProfileData");
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < profile.ProfDatas.Length; i++)
                {
                    stringBuilder.AppendFormat("{0}\t", profile.ProfDatas[i]);
                }
                profileData.InnerText = stringBuilder.ToString();
                profileChild.AppendChild(profileData);
            }
            XmlNode date = document.CreateElement("DateTime");
            date.InnerText = convertTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString());
            root.AppendChild(date);
            return document.DocumentElement;
        }
        private XmlElement GetExceptionXML(string ex)
        {
            XmlDocument document = new XmlDocument();
            XmlNode root = document.CreateElement("Profilometer");
            document.AppendChild(root);
            XmlNode dataNode = document.CreateElement("ExceptionData");
            dataNode.InnerText = ex;
            root.AppendChild(dataNode);
            XmlNode dateNode = document.CreateElement("ExceptionDateTime");
            dateNode.InnerText = convertTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString());
            root.AppendChild(dateNode);
            return document.DocumentElement;
        }
        private void InitializeCommunication(string ipAddress, string port, string type)
        {
            Rc rc = Rc.Ok;
            //Initialising the DLL 
            rc = (Rc)NativeMethods.LJV7IF_Initialize();
            if (rc != Rc.Ok)
            {
                throw new Exception("Cannot Initialize DLL");
            }
            //Initialising Ethernet IPAddress and Port
            _ethernetConfig.abyIpAddress = IPAddress.Parse(ipAddress).GetAddressBytes();
            _ethernetConfig.wPortNo = Convert.ToUInt16(port);
            //Initialilizing the Ethernet Communication
            rc = (Rc)NativeMethods.LJV7IF_EthernetOpen(Define.DEVICE_ID, ref _ethernetConfig);
            if (rc != Rc.Ok)
            {
                throw new Exception("Cannot Establish Ethernet Communication");
            }
        }
        private void FinalizeCommunication(string ipAddress, string port, string type)
        {
            Rc rc = Rc.Ok;
            // Close the Ethernet communication
            rc = (Rc)NativeMethods.LJV7IF_CommClose(Define.DEVICE_ID);
            if (rc != Rc.Ok)
            {
                throw new Exception("Cannot Finalize Ethernet Communication");
            }
            // Finalize the DLL
            rc = (Rc)NativeMethods.LJV7IF_Finalize();
            if (rc != Rc.Ok)
            {
                throw new Exception("Cannot Finalize DLL");
            }
        }
        private XmlElement GetMeasurementValues()
        {
            LJV7IF_MEASURE_DATA[] measureData = new LJV7IF_MEASURE_DATA[NativeMethods.MeasurementDataCount];
            Rc rc = (Rc)NativeMethods.LJV7IF_GetMeasurementValue(Define.DEVICE_ID, measureData);
            if (rc != Rc.Ok) throw new Exception("Cannot Get Measurement Value");
            MeasureData data = new MeasureData(measureData);
            return GetMeasurementValueXML(data);
        }
        private XmlElement GetProfileAdvanceValues()
        {
            LJV7IF_PROFILE_INFO profileInfo = new LJV7IF_PROFILE_INFO();
            LJV7IF_MEASURE_DATA[] measureData = new LJV7IF_MEASURE_DATA[NativeMethods.MeasurementDataCount];

            int profileDataSize = Define.MAX_PROFILE_COUNT +
                (Marshal.SizeOf(typeof(LJV7IF_PROFILE_HEADER)) + Marshal.SizeOf(typeof(LJV7IF_PROFILE_FOOTER))) / Marshal.SizeOf(typeof(int));
            int[] receiveBuffer = new int[profileDataSize];	 // 3,207 (total of the header, the footer, and the 3,200 data entries)
            using (PinnedObject pin = new PinnedObject(receiveBuffer))
            {
                Rc rc = (Rc)NativeMethods.LJV7IF_GetProfileAdvance(Define.DEVICE_ID, ref profileInfo, pin.Pointer,
                (uint)(receiveBuffer.Length * Marshal.SizeOf(typeof(int))), measureData);
                if (rc != Rc.Ok) throw new Exception("Cannot Read Profile Advance Data");
            }
            List<ProfileData> profileDatas = new List<ProfileData>();
            // Output the data of each profile
            profileDatas.Add(new ProfileData(receiveBuffer, 0, profileInfo));
            return GetProfileAdvanceValueXML(new MeasureData(measureData), profileDatas);
        }
        private XmlElement GetBatchProfileAdvanceValues()
        {
            LJV7IF_GET_BATCH_PROFILE_ADVANCE_REQ req = new LJV7IF_GET_BATCH_PROFILE_ADVANCE_REQ();
            req.byPosMode = (byte)BatchPos.Commited;
            req.dwGetBatchNo = 0;
            req.dwGetProfNo = 0;
            req.byGetProfCnt = byte.MaxValue;

            LJV7IF_GET_BATCH_PROFILE_ADVANCE_RSP rsp = new LJV7IF_GET_BATCH_PROFILE_ADVANCE_RSP();
            LJV7IF_PROFILE_INFO profileInfo = new LJV7IF_PROFILE_INFO();
            LJV7IF_MEASURE_DATA[] batchMeasureData = new LJV7IF_MEASURE_DATA[NativeMethods.MeasurementDataCount];
            LJV7IF_MEASURE_DATA[] measureData = new LJV7IF_MEASURE_DATA[NativeMethods.MeasurementDataCount];

            int profileDataSize = Define.MAX_PROFILE_COUNT +
                (Marshal.SizeOf(typeof(LJV7IF_PROFILE_HEADER)) + Marshal.SizeOf(typeof(LJV7IF_PROFILE_FOOTER))) / Marshal.SizeOf(typeof(int));
            int measureDataSize = Marshal.SizeOf(typeof(LJV7IF_MEASURE_DATA)) * NativeMethods.MeasurementDataCount / Marshal.SizeOf(typeof(int));
            int[] receiveBuffer = new int[(profileDataSize + measureDataSize) * req.byGetProfCnt];

            List<ProfileData> profileDatas = new List<ProfileData>();
            // Get profiles.
            using (PinnedObject pin = new PinnedObject(receiveBuffer))
            {
                Rc rc = (Rc)NativeMethods.LJV7IF_GetBatchProfileAdvance(Define.DEVICE_ID, ref req, ref rsp, ref profileInfo, pin.Pointer,
                    (uint)(receiveBuffer.Length * Marshal.SizeOf(typeof(int))), batchMeasureData, measureData);
                if (rc != Rc.Ok) throw new Exception("Cannot Read Batch Profile Advance Data");

                // Output the data of each profile
                int unitSize = ProfileData.CalculateDataSize(profileInfo) + measureDataSize;
                for (int i = 0; i < rsp.byGetProfCnt; i++)
                {
                    profileDatas.Add(new ProfileData(receiveBuffer, unitSize * i, profileInfo));
                }

                // Get all profiles within the batch.
                req.byPosMode = (byte)BatchPos.Spec;
                req.dwGetBatchNo = rsp.dwGetBatchNo;
                do
                {
                    // Update the get profile position
                    req.dwGetProfNo = rsp.dwGetBatchTopProfNo + rsp.byGetProfCnt;
                    req.byGetProfCnt = (byte)Math.Min((uint)(byte.MaxValue), (rsp.dwGetBatchProfCnt - req.dwGetProfNo));

                    rc = (Rc)NativeMethods.LJV7IF_GetBatchProfileAdvance(Define.DEVICE_ID, ref req, ref rsp, ref profileInfo, pin.Pointer,
                        (uint)(receiveBuffer.Length * Marshal.SizeOf(typeof(int))), batchMeasureData, measureData);
                    if (rc != Rc.Ok) throw new Exception("Cannot Read Batch Profile Advance Data");
                    for (int i = 0; i < rsp.byGetProfCnt; i++)
                    {
                        profileDatas.Add(new ProfileData(receiveBuffer, unitSize * i, profileInfo));
                    }
                } while (rsp.dwGetBatchProfCnt != (rsp.dwGetBatchTopProfNo + rsp.byGetProfCnt));
            }
            return GetBatchProfileAdvanceValueXML(new MeasureData(batchMeasureData),new MeasureData(measureData),profileDatas);
        }
        private XmlElement GetHighSpeedProfileValues()
        {
            LJV7IF_GET_PROFILE_REQ req = new LJV7IF_GET_PROFILE_REQ();
            req.byTargetBank = (byte)ProfileBank.Active;
            req.byPosMode = (byte)ProfilePos.Current;
            req.dwGetProfNo = 0;
            req.byGetProfCnt = 10;
            req.byErase = 0;

            LJV7IF_GET_PROFILE_RSP rsp = new LJV7IF_GET_PROFILE_RSP();
            LJV7IF_PROFILE_INFO profileInfo = new LJV7IF_PROFILE_INFO();

            int profileDataSize = Define.MAX_PROFILE_COUNT +
                (Marshal.SizeOf(typeof(LJV7IF_PROFILE_HEADER)) + Marshal.SizeOf(typeof(LJV7IF_PROFILE_FOOTER))) / Marshal.SizeOf(typeof(int));
            int[] receiveBuffer = new int[profileDataSize * req.byGetProfCnt];

            
                using (PinnedObject pin = new PinnedObject(receiveBuffer))
                {
                    Rc rc = (Rc)NativeMethods.LJV7IF_GetProfile(Define.DEVICE_ID, ref req, ref rsp, ref profileInfo, pin.Pointer,
                        (uint)(receiveBuffer.Length * Marshal.SizeOf(typeof(int))));
                    if (rc!=Rc.Ok) throw new Exception("Cannot Read High Speed Profile Data");
                }

                // Output the data of each profile
                List<ProfileData> profileDatas = new List<ProfileData>();
                int unitSize = ProfileData.CalculateDataSize(profileInfo);
                for (int i = 0; i < rsp.byGetProfCnt; i++)
                {
                    profileDatas.Add(new ProfileData(receiveBuffer, unitSize * i, profileInfo));
                }
                return GetHighSpeedProfileValueXML(profileDatas);
        }
        private XmlElement GetBatchHighSpeedProfileValues() 
        {
            // Specify the target batch to get.
            LJV7IF_GET_BATCH_PROFILE_REQ req = new LJV7IF_GET_BATCH_PROFILE_REQ();
            req.byTargetBank = (byte)ProfileBank.Active;
            req.byPosMode = (byte)BatchPos.Commited;
            req.dwGetBatchNo = 0;
            req.dwGetProfNo = 0;
            req.byGetProfCnt = byte.MaxValue;
            req.byErase = 0;

            LJV7IF_GET_BATCH_PROFILE_RSP rsp = new LJV7IF_GET_BATCH_PROFILE_RSP();
            LJV7IF_PROFILE_INFO profileInfo = new LJV7IF_PROFILE_INFO();

            int profileDataSize = Define.MAX_PROFILE_COUNT +
                (Marshal.SizeOf(typeof(LJV7IF_PROFILE_HEADER)) + Marshal.SizeOf(typeof(LJV7IF_PROFILE_FOOTER))) / Marshal.SizeOf(typeof(int));
            int[] receiveBuffer = new int[profileDataSize * req.byGetProfCnt];
            List<ProfileData> profileDatas = new List<ProfileData>();
            // Get profiles
            using (PinnedObject pin = new PinnedObject(receiveBuffer))
            {
                Rc rc = (Rc)NativeMethods.LJV7IF_GetBatchProfile(Define.DEVICE_ID, ref req, ref rsp, ref profileInfo, pin.Pointer,
                    (uint)(receiveBuffer.Length * Marshal.SizeOf(typeof(int))));
                if (rc != Rc.Ok) throw new Exception("Cannot Read Batch High Speed Profile");

                // Output the data of each profile
                int unitSize = ProfileData.CalculateDataSize(profileInfo);
                for (int i = 0; i < rsp.byGetProfCnt; i++)
                {
                    profileDatas.Add(new ProfileData(receiveBuffer, unitSize * i, profileInfo));
                }

                // Get all profiles within the batch.
                req.byPosMode = (byte)BatchPos.Spec;
                req.dwGetBatchNo = rsp.dwGetBatchNo;
                do
                {
                    // Update the get profile position
                    req.dwGetProfNo = rsp.dwGetBatchTopProfNo + rsp.byGetProfCnt;
                    req.byGetProfCnt = (byte)Math.Min((uint)(byte.MaxValue), (rsp.dwCurrentBatchProfCnt - req.dwGetProfNo));

                    rc = (Rc)NativeMethods.LJV7IF_GetBatchProfile(Define.DEVICE_ID, ref req, ref rsp, ref profileInfo, pin.Pointer,
                        (uint)(receiveBuffer.Length * Marshal.SizeOf(typeof(int))));
                    if (rc != Rc.Ok) throw new Exception("Cannot Read Batch High Speed Profile");
                    for (int i = 0; i < rsp.byGetProfCnt; i++)
                    {
                        profileDatas.Add(new ProfileData(receiveBuffer, unitSize * i, profileInfo));
                    }
                } while (rsp.dwGetBatchProfCnt != (rsp.dwGetBatchTopProfNo + rsp.byGetProfCnt));
            }
            return GetHighSpeedProfileValueXML(profileDatas);
        }
        public static void CountProfileReceive(IntPtr buffer, uint size, uint count, uint notify, uint user)
		{
			ThreadSafeBuffer.AddCount((int)user, count, notify);
		}
        private void HighSpeedDataEthernetCommunicationInitalize(string ipAddress, string highSpeedport)
        {
            _ethernetConfig.abyIpAddress = IPAddress.Parse(ipAddress).GetAddressBytes();
            _ethernetConfig.wPortNo = Convert.ToUInt16(highSpeedport);
            int rc = NativeMethods.LJV7IF_HighSpeedDataEthernetCommunicationInitalize(Define.DEVICE_ID, ref _ethernetConfig,
                Convert.ToUInt16(highSpeedport), _callbackOnlyCount,
                Define.MAX_PROFILE_COUNT, (uint)Define.DEVICE_ID);
            if (rc != (int)Rc.Ok)
            {
                throw new Exception("Cannot Initialize High Speed Communication Initialization");
            }
        }
        private void PreStartHighSpeedDataCommunication()
        {
                    LJV7IF_HIGH_SPEED_PRE_START_REQ req = new LJV7IF_HIGH_SPEED_PRE_START_REQ();
                    LJV7IF_PROFILE_INFO profileInfo = new LJV7IF_PROFILE_INFO();
                    int rc = NativeMethods.LJV7IF_PreStartHighSpeedDataCommunication(_currentDeviceId, ref req, ref profileInfo);
                    if (rc != (int)Rc.Ok)
                    {
                        throw new Exception("Cannot Initialize High Speed Communication Initialization");
                    }
        }
        private void StartHighSpeedDataCommunication()
        {
            ThreadSafeBuffer.ClearBuffer(Define.DEVICE_ID);
            int rc = NativeMethods.LJV7IF_StartHighSpeedDataCommunication(Define.DEVICE_ID);
            if (rc != (int)Rc.Ok)
            {
                throw new Exception("Cannot Start High Speed Communication Initialization");
            }
        }
        private void StopHighSpeedDataCommunication()
        {
            int rc = NativeMethods.LJV7IF_StopHighSpeedDataCommunication(Define.DEVICE_ID);
            if (rc != (int)Rc.Ok)
            {
                throw new Exception("Cannot Stop High Speed Communication Initialization");
            }
        }
        private void HighSpeedDataCommunicationFinalize()
        {
            int rc = NativeMethods.LJV7IF_HighSpeedDataCommunicationFinalize(Define.DEVICE_ID);
            if (rc != (int)Rc.Ok)
            {
                throw new Exception("Cannot Finalize High Speed Communication Initialization");
            }
        }
        private string convertTimeZone(string timeZone)
        {
            string timeZoneParsed = string.Empty;
            if (!timeZone.Trim().Equals(string.Empty))
            {
                string[] timeZones = timeZone.Split(':');
                timeZoneParsed += Convert.ToInt64(timeZones[0]);
                if (Convert.ToInt64(timeZones[1]) > 0)
                {
                    timeZoneParsed += Convert.ToInt64(timeZones[1]);
                }
            }
            return DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "Tz" + timeZoneParsed.Trim();
        }
    }
}
