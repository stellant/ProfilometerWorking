using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Profilometer_Keyence.Datas;
using System.Runtime.InteropServices;
using System.Threading;

namespace Profilometer_Keyence
{
    public partial class Form1 : Form
    {

        #region Enum

        /// <summary>
        /// Send command definition
        /// </summary>
        /// <remark>Defined for separate return code distinction</remark>
        public enum SendCommand
        {
            /// <summary>None</summary>
            None,
            /// <summary>Restart</summary>
            RebootController,
            /// <summary>Trigger</summary>
            Trigger,
            /// <summary>Start measurement</summary>
            StartMeasure,
            /// <summary>Stop measurement</summary>
            StopMeasure,
            /// <summary>Auto zero</summary>
            AutoZero,
            /// <summary>Timing</summary>
            Timing,
            /// <summary>Reset</summary>
            Reset,
            /// <summary>Program switch</summary>
            ChangeActiveProgram,
            /// <summary>Get measurement results</summary>
            GetMeasurementValue,
            /// <summary>Get profiles</summary>
            GetProfile,
            /// <summary>Get batch profiles (operation mode "high-speed (profile only)")</summary>
            GetBatchProfile,
            /// <summary>Get profiles (operation mode "advanced (with OUT measurement)")</summary>
            GetProfileAdvance,
            /// <summary>Get batch profiles (operation mode "advanced (with OUT measurement)").</summary>
            GetBatchProfileAdvance,
            /// <summary>Start storage</summary>
            StartStorage,
            /// <summary>Stop storage</summary>
            StopStorage,
            /// <summary>Get storage status</summary>
            GetStorageStatus,
            /// <summary>Manual storage request</summary>
            RequestStorage,
            /// <summary>Get storage data</summary>
            GetStorageData,
            /// <summary>Get profile storage data</summary>
            GetStorageProfile,
            /// <summary>Get batch profile storage data.</summary>
            GetStorageBatchProfile,
            /// <summary>Initialize USB high-speed data communication</summary>
            HighSpeedDataUsbCommunicationInitalize,
            /// <summary>Initialize Ethernet high-speed data communication</summary>
            HighSpeedDataEthernetCommunicationInitalize,
            /// <summary>Request preparation before starting high-speed data communication</summary>
            PreStartHighSpeedDataCommunication,
            /// <summary>Start high-speed data communication</summary>
            StartHighSpeedDataCommunication,
        }

        #endregion

        #region Field

        private LJV7IF_ETHERNET_CONFIG _ethernetConfig;
        private List<MeasureData> _measureDatas;
        private int _currentDeviceId;
        private SendCommand _sendCommand;
        private HighSpeedDataCallBack _callback;
        private HighSpeedDataCallBack _callbackOnlyCount;

        private LJV7IF_PROFILE_INFO[] _profileInfo;
        private DeviceData[] _deviceData;
        private Label[] _deviceStatusLabels;
        private Label[] _receivedProfileCountLabels;

        private ThreadStart threadStart=null;
        private Thread thread = null;

        private DialogResult dialogResult = DialogResult.None;
        private string fileName = string.Empty;
        private string fileNameNew = string.Empty;
        private string filePath = string.Empty;
        private bool fileStatus = false;
        private StreamWriter streamWriter = null;
        private StreamWriter streamLogWriter = null;
        #endregion

        #region Methods

        private void OnFocusIPAddress1(object Sender, EventArgs e)
        {
            if (Sender.Equals(textbox_ipaddress1))
            {
                textbox_ipaddress1.Text = "";
            }
            if (Sender.Equals(textbox_ipaddress2))
            {
                textbox_ipaddress2.Text = "";
            }
            if (Sender.Equals(textbox_ipaddress3))
            {
                textbox_ipaddress3.Text = "";
            }
            if (Sender.Equals(textbox_ipaddress4))
            {
                textbox_ipaddress4.Text = "";
            }
            if (Sender.Equals(textbox_commandport))
            {
                textbox_commandport.Text = "";
            }
            if (Sender.Equals(textbox_hsport))
            {
                textbox_hsport.Text = "";
            }
            if (Sender.Equals(textbox_frequency))
            {
                textbox_frequency.Text = "";
            }
            if (Sender.Equals(textbox_stprofile))
            {
                textbox_stprofile.Text = "";
            }

        }
        private void OnLostFocusIPAddress1(object Sender, EventArgs e)
        {
            if (Sender.Equals(textbox_ipaddress1))
            {
                if (textbox_ipaddress1.Text.Equals(""))
                {
                    textbox_ipaddress1.Text = "10";
                }
                else if (Convert.ToInt32(textbox_ipaddress1.Text) > 255)
                {
                    textbox_ipaddress1.Text = "10";
                }
            }
            if (Sender.Equals(textbox_ipaddress2))
            {
                if (textbox_ipaddress2.Text.Equals(""))
                {
                    textbox_ipaddress2.Text = "134";
                }
                else if (Convert.ToInt32(textbox_ipaddress2.Text) > 255)
                {
                    textbox_ipaddress2.Text = "134";
                }
            }
            if (Sender.Equals(textbox_ipaddress3))
            {
                if (textbox_ipaddress3.Text.Equals(""))
                {
                    textbox_ipaddress3.Text = "47";
                }
                else if (Convert.ToInt32(textbox_ipaddress3.Text) > 255)
                {
                    textbox_ipaddress3.Text = "47";
                }
            }
            if (Sender.Equals(textbox_ipaddress4))
            {
                if (textbox_ipaddress4.Text.Equals(""))
                {
                    textbox_ipaddress4.Text = "46";
                }
                else if (Convert.ToInt32(textbox_ipaddress4.Text) > 255)
                {
                    textbox_ipaddress4.Text = "46";
                }
            }
            if (Sender.Equals(textbox_commandport))
            {
                if (textbox_commandport.Text.Equals(""))
                {
                    textbox_commandport.Text = "24691";
                }
                else if (Convert.ToInt32(textbox_commandport.Text) > 65535)
                {
                    textbox_commandport.Text = "24691";
                }
            }
            if (Sender.Equals(textbox_hsport))
            {
                if (textbox_hsport.Text.Equals(""))
                {
                    textbox_hsport.Text = "24692";
                }
                else if (Convert.ToInt32(textbox_hsport.Text) > 65535)
                {
                    textbox_hsport.Text = "24692";
                }
            }
            if (Sender.Equals(textbox_frequency))
            {
                if (textbox_frequency.Text.Equals(""))
                {
                    textbox_frequency.Text = "10";
                }
                else if (Convert.ToInt32(textbox_frequency.Text) > 1000)
                {
                    textbox_frequency.Text = "10";
                }
            }
            if (Sender.Equals(textbox_stprofile))
            {
                if (textbox_stprofile.Text.Equals(""))
                {
                    textbox_stprofile.Text = "2";
                }
                else if (Convert.ToInt32(textbox_stprofile.Text) > 1000)
                {
                    textbox_stprofile.Text = "2";
                }
            }
        }
        public Form1()
        {
            InitializeComponent();
            textbox_frequency.Text = "10";
            textbox_commandport.Text = "24691";
            textbox_hsport.Text = "24692";
            textbox_ipaddress1.Text = "10";
            textbox_ipaddress2.Text = "134";
            textbox_ipaddress3.Text = "47";
            textbox_ipaddress4.Text = "46";
            textbox_stprofile.Text = "2";
            textbox_ipaddress1.GotFocus += OnFocusIPAddress1;
            textbox_ipaddress1.LostFocus += OnLostFocusIPAddress1;
            textbox_ipaddress2.GotFocus += OnFocusIPAddress1;
            textbox_ipaddress2.LostFocus += OnLostFocusIPAddress1;
            textbox_ipaddress3.GotFocus += OnFocusIPAddress1;
            textbox_ipaddress3.LostFocus += OnLostFocusIPAddress1;
            textbox_ipaddress4.GotFocus += OnFocusIPAddress1;
            textbox_ipaddress4.LostFocus += OnLostFocusIPAddress1;
            textbox_hsport.GotFocus += OnFocusIPAddress1;
            textbox_hsport.LostFocus += OnLostFocusIPAddress1;
            textbox_commandport.GotFocus += OnFocusIPAddress1;
            textbox_commandport.LostFocus += OnLostFocusIPAddress1;
            textbox_frequency.GotFocus += OnFocusIPAddress1;
            textbox_frequency.LostFocus += OnLostFocusIPAddress1;
            textbox_stprofile.GotFocus += OnFocusIPAddress1;
            textbox_stprofile.LostFocus += OnLostFocusIPAddress1;
            _callback = new HighSpeedDataCallBack(ReceiveHighSpeedData);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            textbox_filename.Enabled = false;
            button_connect.Enabled = false;
            button_disconnect.Enabled = false;
        }
        /// <summary>
        /// Method to parse time zone into Tz*** format
        /// </summary>
        /// <param name="timeZone">Current Time Zone</param>
        /// <returns>Parsed Time Zone</returns>
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
            return DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "Tz" +timeZoneParsed.Trim();
        }
        private void EnableButton()
        {
            if (fileStatus)
            {
                button_connect.Enabled = true;
            }
            else
            {
                button_connect.Enabled = false;
            }
        }
        private void EnableConnect()
        {
            button_connect.Enabled = true;
            button_disconnect.Enabled = false;
        }
        private void EnableDisconnect()
        {
            button_connect.Enabled =false;
            button_disconnect.Enabled = true;
        }
        private void button_browse_Click(object sender, EventArgs e)
        {
            filePath = string.Empty;
            fileName = string.Empty;
            fileNameNew = string.Empty;
            textbox_filename.Text = string.Empty;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "Enter File Name...";
            saveFileDialog1.Filter = "Comma Separated Values (*.csv)|*.csv";
            if (saveFileDialog1.ShowDialog().Equals(DialogResult.OK))
            {
                filePath = saveFileDialog1.FileName;
                fileName = Path.GetFileNameWithoutExtension(filePath);
                fileNameNew = fileName + "_" + convertTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString()) + ".csv";
                filePath = Path.Combine(Path.GetDirectoryName(filePath), fileNameNew);
                textBox_status.AppendText("Data will be written to " + filePath + "\n");
                textbox_filename.Text = filePath.ToString();
                fileStatus = true;
                EnableButton();
            }
            else
            {
                fileStatus = false;
                EnableButton();
                return;
            }
        }
        
        private void button_connect_Click(object sender, EventArgs e)
        {
                Rc rc2;
                LJV7IF_PROFILE_INFO profileInfo = new LJV7IF_PROFILE_INFO();
                ThreadSafeBuffer.Clear(Define.DEVICE_ID);
                LJV7IF_HIGH_SPEED_PRE_START_REQ req = new LJV7IF_HIGH_SPEED_PRE_START_REQ();
                try
                {
                    rc2 = (Rc)NativeMethods.LJV7IF_Initialize();
                    if (rc2 != Rc.Ok)
                    {
                        textBox_status.AppendText("Initialization Failed...\n");
                        EnableConnect();
                        return;
                    }
                    _ethernetConfig.abyIpAddress = new byte[] {
						Convert.ToByte(textbox_ipaddress1.Text),
						Convert.ToByte(textbox_ipaddress2.Text),
						Convert.ToByte(textbox_ipaddress3.Text),
						Convert.ToByte(textbox_ipaddress4.Text)
					};
                    _ethernetConfig.wPortNo = Convert.ToUInt16(textbox_commandport.Text);
                    rc2 = (Rc)NativeMethods.LJV7IF_EthernetOpen(Define.DEVICE_ID, ref _ethernetConfig);
                    if (rc2 != Rc.Ok)
                    {
                        textBox_status.AppendText("Ethernet Initialization Failed...\n");
                        EnableConnect();
                        return;
                    }
                    _ethernetConfig.abyIpAddress = new byte[] {
						Convert.ToByte(textbox_ipaddress1.Text),
						Convert.ToByte(textbox_ipaddress2.Text),
						Convert.ToByte(textbox_ipaddress3.Text),
						Convert.ToByte(textbox_ipaddress4.Text)
					};
                    NativeMethods.LJV7IF_StopHighSpeedDataCommunication(Define.DEVICE_ID);
                    NativeMethods.LJV7IF_HighSpeedDataCommunicationFinalize(Define.DEVICE_ID);
                    rc2 = (Rc)NativeMethods.LJV7IF_HighSpeedDataEthernetCommunicationInitalize(Define.DEVICE_ID, ref _ethernetConfig,
                        Convert.ToUInt16(textbox_hsport.Text), _callback, Convert.ToUInt32(textbox_frequency.Text), (uint)Define.DEVICE_ID);
                    if (rc2 != Rc.Ok)
                    {
                        textBox_status.AppendText("High Speed Data Communication Initialize Failed...\n");
                        EnableConnect();
                        return;
                    }
                    req.bySendPos = Convert.ToByte(textbox_stprofile.Text);
                    rc2 = (Rc)NativeMethods.LJV7IF_PreStartHighSpeedDataCommunication(Define.DEVICE_ID, ref req, ref profileInfo);
                    if (rc2 != Rc.Ok)
                    {
                        textBox_status.AppendText("Pre Start High Speed Data Communication Initialize Failed...\n");
                        EnableConnect();
                        return;
                    }
                    rc2 = (Rc)NativeMethods.LJV7IF_StartHighSpeedDataCommunication(Define.DEVICE_ID);
                    if (rc2 != Rc.Ok)
                    {
                        textBox_status.AppendText("Start High Speed Data Communication Initialize Failed...\n");
                        EnableConnect();
                        return;
                    }
                    EnableDisconnect();
                    highSpeedTimer.Start();
                }
                catch (FormatException ex)
                {
                    WriteLog(ex.ToString() + Environment.NewLine + "   at " + convertTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString()) + "\n");
                    textBox_status.AppendText("Exception Occured. Please refer log file." + "\n");
                    EnableConnect();
                    return;
                }
                catch (OverflowException ex)
                {
                    WriteLog(ex.ToString() + Environment.NewLine + "   at " + convertTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString()) + "\n");
                    textBox_status.AppendText("Exception Occured. Please refer log file." + "\n");
                    EnableConnect();
                    return;
                }
       }
        private void highSpeedTimer_Tick(object sender, EventArgs e)
        {
            uint notify = 0;
			int batcnNo = 0;
			List<int[]> dataList = ThreadSafeBuffer.Get(Define.DEVICE_ID, out notify, out batcnNo);
            StringBuilder builder = new StringBuilder();
			foreach (int[] profile in dataList)
			{
                for (int i = 0; i < profile.Length; i++)
                {
                    builder.Append(profile[i]+",");
                }
            }
            string data = builder.ToString();
            string date = convertTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString())+"\n";
            WriteData(data,date);
			if ((notify & 0xFFFF) != 0)
			{
				highSpeedTimer.Stop();
			}
			if ((notify & 0x10000) != 0)
			{
                //do Nothing
			}
        }
        public void ReceiveHighSpeedData(IntPtr buffer, uint size, uint count, uint notify, uint user)
		{
            try
            {
                uint profileSize = (uint)(size / Marshal.SizeOf(typeof(int)));
                List<int[]> receiveBuffer = new List<int[]>();
                int[] bufferArray = new int[profileSize * count];
                Marshal.Copy(buffer, bufferArray, 0, (int)(profileSize * count));
                for (int i = 0; i < count; i++)
                {
                    int[] oneProfile = new int[profileSize];
                    Array.Copy(bufferArray, i * profileSize, oneProfile, 0, profileSize);
                    receiveBuffer.Add(oneProfile);
                }

                ThreadSafeBuffer.Add((int)user, receiveBuffer, notify);
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString() + Environment.NewLine + "   at " + convertTimeZone(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString()) + "\n");
                textBox_status.AppendText("Exception Occured. Please refer log file." + "\n");
            }
		}
        private void button_disconnect_Click(object sender, EventArgs e)
        {
                highSpeedTimer.Stop();
                Rc rc = (Rc)NativeMethods.LJV7IF_StopHighSpeedDataCommunication(Define.DEVICE_ID);
                if (rc != Rc.Ok)
                {
                    textBox_status.Text = "Cannot Stop High Speed Communication...";
                    EnableDisconnect();
                    return;
                }
                rc = (Rc)NativeMethods.LJV7IF_HighSpeedDataCommunicationFinalize(Define.DEVICE_ID);
                if (rc != Rc.Ok)
                {
                    textBox_status.Text = "Cannot Finalize High Speed Data Communication...";
                    EnableDisconnect();
                    return;
                }
                rc = (Rc)NativeMethods.LJV7IF_CommClose(Define.DEVICE_ID);
                if (rc != Rc.Ok)
                {
                    textBox_status.Text = "Cannot Stop High Speed Data Communication...";
                    EnableDisconnect();
                    return;
                }
                rc = (Rc)NativeMethods.LJV7IF_Finalize();
                if (rc != Rc.Ok)
                {
                    textBox_status.Text = "Cannot Finalize High Speed Data Communication...";
                    EnableDisconnect();
                    return;
                }
                EnableConnect();
                
        }
        private StreamWriter getWriter(string filePath)
        {
            //Checks whether object is null
            if (streamWriter == null)
            {
                streamWriter = new StreamWriter(filePath, true);
            }
            //Checks whether writer is closed
            if (streamWriter.BaseStream == null)
            {
                streamWriter = null;
                streamWriter = new StreamWriter(filePath, true);
            }
            return streamWriter;
        }
        /// <summary>
        /// Method to write data into data file
        /// </summary>
        /// <param name="data">Barcode Data</param>
        /// <param name="date">Date Time with TimeZone</param>
        private void WriteData(string data, string date)
        {
            getWriter(filePath).WriteLine(string.Format("{0},{1}", data, date));
            getWriter(filePath).Flush();
            CloseData();
        }
        /// <summary>
        /// Method to close file writer
        /// </summary>
        private void CloseData()
        {
            getWriter(filePath).Close();
        }
        /// <summary>
        /// Method to create a StreamWriter object to write log data
        /// </summary>
        /// <param name="filePath">Location of the file</param>
        /// <returns></returns>
        private StreamWriter getLogWriter(string filePath)
        {
            //Checks whether object is null
            if (streamLogWriter == null)
            {
                streamLogWriter = new StreamWriter(filePath, true);
            }
            //Checks whether writer is closed
            if (streamLogWriter.BaseStream == null)
            {
                streamLogWriter = null;
                streamLogWriter = new StreamWriter(filePath, true);
            }
            return streamLogWriter;
        }
        /// <summary>
        /// Method to write logs into log file
        /// </summary>
        /// <param name="status">Log Details</param>
        private void WriteLog(string status)
        {
            getLogWriter("log.log").WriteLine(status);
            getLogWriter("log.log").Flush();
        }
        private void CloseLog()
        {
            getLogWriter("log.log").Close();
        }
    #endregion 
    }
}
