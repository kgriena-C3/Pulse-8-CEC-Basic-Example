using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CecSharp;

namespace C3CECControl
{
    public partial class Form1 : Form
    {
        CECClient client;

        List<Control> buttons = new List<Control>();

        Timer retryConnectionTimer = new Timer();
        public Form1()
        {
            InitializeComponent();
            client = new CECClient();
            buttons.Add(On);
            buttons.Add(Off);
            //buttons.Add(InputComboBox);
            foreach (Control button in buttons)
            {
                button.Enabled = false;
            }
            ConnectionStatusLabel.Enabled = true;
            clientConnect();
            retryConnectionTimer.Tick += RetryConnectionTimer_Tick;
        }

        private void RetryConnectionTimer_Tick(object sender, EventArgs e)
        {
            ConnectionStatusLabel.Text = "Retrying Connection";
            clientConnect();
        }

        public void clientConnect()
        {
            if (client.Connect(10000))
            {
                ConnectionStatusLabel.Text = "Connected";
                foreach (Control button in buttons)
                {
                    button.Enabled = true;
                }
            }
            else
            {
                ConnectionStatusLabel.Text = "Could not open a connection to the CEC adapter";
                retryConnectionTimer.Interval = 10000;
                retryConnectionTimer.Start();
            }
        }

        private void On_Click(object sender, EventArgs e)
        {
            client.Lib.PowerOnDevices(CecLogicalAddress.Broadcast);
        }

        private void Off_Click(object sender, EventArgs e)
        {
            client.Lib.StandbyDevices(CecLogicalAddress.Broadcast);
        }

        private void InputComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string selectedInput = (string)comboBox.SelectedItem;
            if (selectedInput.ToLower().Contains("hdmi"))
            {
                var inputNumber = Convert.ToByte(Convert.ToInt32(selectedInput.Substring(selectedInput.IndexOf(" ") + 1, 1))+0x80);
                client.Lib.SetHDMIPort(CecLogicalAddress.Broadcast,inputNumber);
            }

        }        

    }

    class CECClient : CecCallbackMethods
    {
        private int LogLevel;
        public LibCecSharp Lib;
        private LibCECConfiguration Config;

        public CECClient()
        {
            Config = new LibCECConfiguration();
            Config.DeviceTypes.Types[0] = CecDeviceType.RecordingDevice;
            Config.DeviceName = "Cenero Conferencing System";
            Config.ClientVersion = LibCECConfiguration.CurrentVersion;
            Config.SetCallbacks(this);
            LogLevel = (int)CecLogLevel.All;

            Lib = new LibCecSharp(Config);
            Lib.InitVideoStandalone();
        }
        public bool Connect(int timeout)
        {
            CecAdapter[] adapters = Lib.FindAdapters(string.Empty);
            if (adapters.Length > 0)
                return Connect(adapters[0].ComPort, timeout);
            else
            {
                return false;
            }
        }

        public bool Connect(string port, int timeout)
        {
            return Lib.Open(port, timeout);
        }

        public void Close()
        {
            Lib.Close();
        }
    }
}
