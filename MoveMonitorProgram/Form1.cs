using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms;

namespace MoveMonitorProgram
{
    public partial class Form1 : Form
    {
        // RECT 구조체 정의
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        // Windows API 함수 선언
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool bRepaint);


        // 프로그램 창을 선택한 모니터로 이동하는 함수
        private void MoveWindowToMonitor(Process process, Screen screen)
        {
            IntPtr handle = process.MainWindowHandle;
            if (handle != IntPtr.Zero)
            {
                RECT rect;
                GetWindowRect(handle, out rect);
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                int newX = screen.Bounds.X + (screen.Bounds.Width - width) / 2;
                int newY = screen.Bounds.Y + (screen.Bounds.Height - height) / 2;
                MoveWindow(handle, newX, newY, width, height, true);
            }
        }
        /*
        // 프로그램 창을 선택한 모니터로 이동하는 함수
        private void MoveWindowToMonitor(Process process, Screen screen)
        {
            IntPtr handle = process.MainWindowHandle;
            if (handle != IntPtr.Zero)
            {
                NativeMethods.MoveWindow(handle, screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height, true);
            }
        }
        */

        // NativeMethods 클래스를 정의하여 MoveWindow 함수를 사용할 수 있도록 함
        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);
        }

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

        // RECT 구조체 대신 MyRect 구조체 사용
        public struct MyRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szDevice;
        }

        private List<Process> runningProcesses;

        public Form1()
        {
            InitializeComponent();
            runningProcesses = new List<Process>();
            LoadRunningProcesses();
            LoadMonitors();
        }

        private void LoadRunningProcesses()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                runningProcesses.Add(process);
                comboBox1.Items.Add(process.ProcessName);
            }
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
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            LoadRunningProcesses();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 선택된 프로세스 가져오기
            Process selectedProcess = runningProcesses[comboBox1.SelectedIndex];

            // 선택된 모니터 가져오기
            Screen selectedMonitor = Screen.AllScreens[comboBox2.SelectedIndex];

            // 선택된 프로세스 창을 선택된 모니터로 이동시키기
            MoveWindowToMonitor(selectedProcess, selectedMonitor);
        }
    }
}