using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GlobalDataContracts;

namespace MsmqSensorTask
{
    public partial class MainForm : Form
    {
        private const int MaxOutputTextLength = 60000;

        private PerformanceCounterSensor _performanceCounterSensor = new PerformanceCounterSensor();
        public MainForm()
        {
            InitializeComponent();
        }


        private void InitSensors()
        {
            //Init sensors
            try
            {
                _performanceCounterSensor.LogError = LogError;
                _performanceCounterSensor.LogInfo = LogInfo;
                _performanceCounterSensor.SensorDataRetrieved = SensorDataRetrieved;
                _performanceCounterSensor.InitSensors(ConfigurationManager.AppSettings);
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, exc.ToString(), "Failed initializing sensors", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            InitSensors();

            //Start sensors
            try
            {
                _performanceCounterSensor.StartSendingSensors();
                buttonStart.Enabled = false;
                buttonStop.Enabled = true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, exc.Message, "Failed starting sensors", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            StopSendingSensors();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void StopSendingSensors()
        {
            //Stop sensors
            try
            {
                _performanceCounterSensor.StopSendingSensors();
                buttonStart.Enabled = true;
                buttonStop.Enabled = false;
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, exc.Message, "Failed stopping sending sensors", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopReceivingSensors()
        {
            //Stop sensors
            try
            {
                _performanceCounterSensor.StopReceivingSensors();
                buttonStartReceive.Enabled = true;
                buttonStopReceive.Enabled = false;
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, exc.Message, "Failed stopping receiving sensors", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LogInfo(string message)
        {
            textBoxOutput.Invoke(new Action(() =>
                {
                    var output = new StringBuilder(textBoxOutput.Text).AppendFormat("[{0}] {1}", DateTime.Now, message).AppendLine().ToString();
                    output = TrimOutput(output);
                    textBoxOutput.Text = output;

                    if (checkBoxJumpToCurrentOutput.Checked)
                    {
                        textBoxOutput.SelectionStart = textBoxOutput.Text.Length;
                        textBoxOutput.ScrollToCaret();
                    }
                }));
            Application.DoEvents();
        }

        private void LogError(string message)
        {
            textBoxOutput.Invoke(new Action(() =>
                {
                    var output = new StringBuilder(textBoxOutput.Text).AppendFormat("ERROR: {0}", message).AppendLine().ToString();
                    output = TrimOutput(output);
                    textBoxOutput.Text = output;

                    if (checkBoxJumpToCurrentOutput.Checked)
                    {
                        textBoxOutput.SelectionStart = textBoxOutput.Text.Length;
                        textBoxOutput.ScrollToCaret();
                    }
                }));
            Application.DoEvents();
        }

        private string TrimOutput(string output)
        {
            if (output.Length > MaxOutputTextLength)
            {
                output = output.Substring(output.Length - MaxOutputTextLength);
                var lineBreakIndex = output.IndexOf('\n');
                if (lineBreakIndex >= 0)
                    output = output.Substring(lineBreakIndex + 1);
            }
            return output;
        }

        private List<RetrievedSensorValue> _list = new List<RetrievedSensorValue>();

        private void SensorDataRetrieved(RetrievedSensorValue sensorData)
        {
            _list = new List<RetrievedSensorValue>(_list);
            RetrievedSensorValue entry = _list.FirstOrDefault(x => x.SensorId == sensorData.SensorId);
            if (entry != null)
            {
                entry.SensorValue = sensorData.SensorValue;
                entry.Timestamp = sensorData.Timestamp;
            }
            else
            {
                _list.Add(sensorData);
            }

            _list.Sort((value1, value2) => { return value1.SensorId.CompareTo(value2.SensorId); });

            dataGridViewValues.Invoke(new Action(() =>
            {
                dataGridViewValues.DataSource = _list;
            }));
            Application.DoEvents();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopReceivingSensors();
            StopSendingSensors();

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void buttonStartReceive_Click(object sender, EventArgs e)
        {
            try
            {
                InitSensors();

                //Start sensors
                try
                {
                    _performanceCounterSensor.StartReceivingSensors();
                    buttonStartReceive.Enabled = false;
                    buttonStopReceive.Enabled = true;
                }
                catch (Exception exc)
                {
                    MessageBox.Show(this, exc.Message, "Failed starting receiving sensors", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }
            catch (Exception)
            {
                ;
            }

        }

        private void buttonStopReceive_Click(object sender, EventArgs e)
        {
            StopReceivingSensors();
        }

    }

}
