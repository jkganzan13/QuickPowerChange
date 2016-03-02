using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickPowerChange
{
    public partial class Form1 : Form
    {
        [DllImport("powrprof.dll")]
        static extern uint PowerGetActiveScheme(
            IntPtr UserRootPowerKey,
            ref IntPtr ActivePolicyGuid);

        [DllImport("powrprof.dll", CharSet = CharSet.Unicode)]
        static extern uint PowerReadFriendlyName(
            IntPtr RootPowerKey,
            IntPtr SchemeGuid,
            IntPtr SubGroupOfPowerSettingGuid,
            IntPtr PowerSettingGuid,
            StringBuilder Buffer,
            ref uint BufferSize
            );

        [DllImport("kernel32.dll")]
        static extern IntPtr LocalFree(
            IntPtr hMem
            );

        private const uint ERROR_MORE_DATA = 234;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = "cmd.exe";
            processInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            processInfo.CreateNoWindow = true;
                      
            string s = GetCurrentActiveScheme();
            string powerSetting = "";
            if (s == "High performance")
            {
                powerSetting = "Changed to Power Saver scheme";
                processInfo.Arguments = "/C powercfg -s a1841308-3541-4fab-bc81-f71556f20b4a";
            } else
            {
                powerSetting = "Changed to High Performance scheme";
                processInfo.Arguments = "/C powercfg -s 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";
            }

            Process process = new Process();
            process.StartInfo = processInfo;
            process.Start();

            label1.Text = powerSetting;
        }

        private void OKbutton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private string GetCurrentActiveScheme()
        {
            IntPtr activeGuidPtr = IntPtr.Zero;            
            
            uint res = PowerGetActiveScheme(IntPtr.Zero, ref activeGuidPtr); 
            if (res != 0)
                throw new Win32Exception();

            uint buffSize = 0;
            StringBuilder buffer = new StringBuilder();
            res = PowerReadFriendlyName(IntPtr.Zero, activeGuidPtr, IntPtr.Zero, IntPtr.Zero, buffer, ref buffSize); 
            
            if (res == ERROR_MORE_DATA)
            {
                buffer.Capacity = (int)buffSize;
                res = PowerReadFriendlyName(IntPtr.Zero, activeGuidPtr,
                    IntPtr.Zero, IntPtr.Zero, buffer, ref buffSize); 
            }

            if (res != 0)
                throw new Win32Exception();
            
            string s = buffer.ToString();
            return s;
        }
    
    }
}
