using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static MoveMonitorProgram.Form1;

namespace MoveMonitorProgram
{
    public partial class Form1 : Form
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // MONITORINFOEX 구조체 정의
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szDevice;
        }

        // MonitorEnumDelegate 델리게이트 선언
        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        private List<Process> runningProcesses;

        public Form1()
        {
            InitializeComponent();
            runningProcesses = new List<Process>();
            // 폼이 로드될 때 라디오 버튼 1을 선택한 상태로 초기화
            radioButton1.Checked = true;
            LoadRunningProcesses();
            LoadMonitors();
        }

        private void LoadMonitors()
        {
            comboBox2.Items.Clear();
            int monitorCount = 0;
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
            {
                MONITORINFOEX info = new MONITORINFOEX();
                info.cbSize = Marshal.SizeOf(info);
                GetMonitorInfo(hMonitor, ref info);
                monitorCount++;
                comboBox2.Items.Add("Monitor " + monitorCount);
                return true;
            }, IntPtr.Zero);

            if (comboBox2.Items.Count > 0)
            {
                comboBox2.SelectedIndex = 0;
            }
        }

        private void LoadRunningProcesses()
        {
            comboBox1.Items.Clear();
            string[] processNames;

            if (radioButton1.Checked)
            {
                processNames = new string[] { "dnplayer" };
            }
            else if (radioButton2.Checked)
            {
                processNames = new string[] { "Nox" };
            }
            else if (radioButton3.Checked)
            {
                string customProcessName = textBox1.Text.Trim();
                processNames = new string[] { customProcessName };
            }
            else
            {
                processNames = new string[] { "" };
            }

            //{ "chrome", "dnplayer", "iMax", "KakaoTalk","ProjectLM_Kr", "Seven Knights2", "xxd-0.xem"};

            runningProcesses.Clear();

            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                foreach (string processName in processNames)
                {
                    if (string.IsNullOrEmpty(processName) || process.ProcessName.ToLower().Contains(processName.ToLower()))
                    {
                        runningProcesses.Add(process);
                        string windowTitle = !string.IsNullOrEmpty(process.MainWindowTitle) ? " - " + process.MainWindowTitle : "";
                        string displayName = process.ProcessName + windowTitle;

                        RECT rect;
                        GetWindowRect(process.MainWindowHandle, out rect);
                        string processInfo = $"({rect.Left}, {rect.Top}) {rect.Right - rect.Left} x {rect.Bottom - rect.Top}";
                        textBox2.Text = $"{rect.Left}";
                        textBox3.Text = $"{rect.Top}";
                        textBox4.Text = $"{rect.Right - rect.Left}";
                        textBox5.Text = $"{rect.Bottom - rect.Top}";
                        displayName += $" {processInfo}";
                        comboBox1.Items.Add(displayName);
                        break;
                    }
                }
            }

            // 프로세스를 로드한 후 첫 번째 아이템을 선택하도록 설정
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void MoveWindowToMonitor(Process process, Screen screen)
        {
            IntPtr handle = process.MainWindowHandle;
            if (handle != IntPtr.Zero)
            {
                RECT rect;
                GetWindowRect(handle, out rect);
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                int newX = screen.WorkingArea.X + (screen.WorkingArea.Width - width) / 2;
                int newY = screen.WorkingArea.Y + (screen.WorkingArea.Height - height) / 2;
                MoveWindow(handle, newX, newY, width, height, true);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadRunningProcesses();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked || radioButton2.Checked)
            {
                if (comboBox1.SelectedIndex >= 0 && comboBox1.SelectedIndex < runningProcesses.Count)
                {
                    Process selectedProcess = runningProcesses[comboBox1.SelectedIndex];
                    Screen selectedMonitor = Screen.AllScreens[comboBox2.SelectedIndex];
                    MoveWindowToMonitor(selectedProcess, selectedMonitor);
                }
            }
            else if (radioButton3.Checked)
            {
                string customProcessName = textBox1.Text.Trim();
                Process selectedProcess = GetProcessByNameString(customProcessName);
                if (selectedProcess != null)
                {
                    Screen selectedMonitor = Screen.AllScreens[comboBox2.SelectedIndex];
                    MoveWindowToMonitor(selectedProcess, selectedMonitor);
                }
                else
                {
                    MessageBox.Show("입력한 프로세스를 찾을 수 없습니다.");
                }
            }
        }

        private Process GetProcessByNameString(string processName)
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName.ToLower().Contains(processName.ToLower()))
                {
                    return process;
                }
            }
            return null;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Visible = radioButton3.Checked;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected process.
            Process selectedProcess = runningProcesses[comboBox1.SelectedIndex];

            // Get the window size of the selected process.
            RECT rect;
            GetWindowRect(selectedProcess.MainWindowHandle, out rect);

            // Set the text of the four text boxes to the window size.
            textBox2.Text = $"{rect.Left}";
            textBox3.Text = $"{rect.Top}";
            textBox4.Text = $"{rect.Right - rect.Left}";
            textBox5.Text = $"{rect.Bottom - rect.Top}";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Get the values of the four text boxes.
            int left = int.Parse(textBox2.Text);
            int top = int.Parse(textBox3.Text);
            int width = int.Parse(textBox4.Text);
            int height = int.Parse(textBox5.Text);

            // Get the selected process.
            Process selectedProcess = runningProcesses[comboBox1.SelectedIndex];

            // Move the window to the new location and size.
            MoveWindow(selectedProcess.MainWindowHandle, left, top, width, height, true);
        }
    }
}
