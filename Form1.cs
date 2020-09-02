using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace bieg4._6
{
    public partial class Form1 : Form
    {

        public static Process GameProcess = Process.GetProcessesByName("WowClassic").FirstOrDefault();
        public static IntPtr window;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        const UInt32 WM_KEYDOWN = 0x0100;
        const UInt32 WM_KEYUP = 0x0101;

        private static System.Windows.Forms.Timer timer1Reference;
        public List<ComboBox> comboBoxes = new List<ComboBox>();
        public List<NumericUpDown> numericUpDowns = new List<NumericUpDown>();

        public static byte lastReferencedPair = 0;
        public static Random rnd = new Random();

        public Form1()
        {
            InitializeComponent();
            window = GameProcess.MainWindowHandle;
            timer1Reference = timer1;
            timer1Reference.Interval = 1;
            initLists();
            this.Text = "WoW Caster - Cast with LCTRL";
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {

            Debug.WriteLine(GameProcess);

            if (checkBox1.Checked)
            {
                _hookID = SetHooks(_proc);
            }
            else
            {
                UnhookWindowsHookEx(_hookID);
                disableTimer();
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (  !string.IsNullOrWhiteSpace(comboBoxes[lastReferencedPair].Text)  )
            {
                PostMessage(window, WM_KEYDOWN, getKeyValue(comboBoxes[lastReferencedPair].Text), 0);
                Thread.Sleep(rnd.Next(15, 40));
                PostMessage(window, WM_KEYUP, getKeyValue(comboBoxes[lastReferencedPair].Text), 0);
                timer1Reference.Interval = Convert.ToInt32(numericUpDowns[lastReferencedPair].Value * 1000) + rnd.Next(100, 250);

                lastReferencedPair++;
                if(lastReferencedPair == comboBoxes.Count){
                    Debug.WriteLine("Just Finished...");
                    disableTimer();
                }
            }
            else
            {
                disableTimer();
            }
         }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            disableTimer();
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
            }
        }

        private static void disableTimer()
        {
            lastReferencedPair = 0;
            if (timer1Reference.Enabled) timer1Reference.Enabled = false;
        }

        private static void toggleTimer()
        {
            if (!timer1Reference.Enabled)
            {
                timer1Reference.Interval = 50;
                timer1Reference.Enabled = true;
                Debug.WriteLine("Casting...");
            }
            else
            {
                timer1Reference.Enabled = false;
                Debug.WriteLine("Stop Casting!");
                lastReferencedPair = 0;
            }
        }

        private void initLists()
        {
            comboBoxes.Add(comboBox1); comboBoxes.Add(comboBox2); comboBoxes.Add(comboBox3); comboBoxes.Add(comboBox4);
            numericUpDowns.Add(numericUpDown1); numericUpDowns.Add(numericUpDown2); numericUpDowns.Add(numericUpDown3); numericUpDowns.Add(numericUpDown4);
        }

        private int getKeyValue(string x)
        {
            switch (x)
            {
                case "1": return (int)Keys.D1;
                case "2": return (int)Keys.D2;
                case "3": return (int)Keys.D3;
                case "4": return (int)Keys.D4;
                case "5": return (int)Keys.D5;
                case "6": return (int)Keys.D6;
                case "7": return (int)Keys.D7;
                case "8": return (int)Keys.D8;
                case "9": return (int)Keys.D9;
                default: return (int)Keys.F1;
            }
        }






        //hooks sh#t

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Console.WriteLine((Keys)vkCode);
                if ((Keys)vkCode == Keys.LControlKey)
                {
                    toggleTimer();
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


        private const int WH_KEYBOARD_LL = 13;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHooks(LowLevelKeyboardProc proc)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
            GetModuleHandle(GameProcess.MainModule.ModuleName), 0);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

       
    }
}
