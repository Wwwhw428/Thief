using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using NHotkey;
using NHotkey.WindowsForms;
using Gma.System.MouseKeyHook;
using System.Windows.Input;
using System.Text;
using System.Collections.Generic;

namespace Thief
{
    public partial class Form1 : Form
    {
        #region windows api 函数声明
        // 当前位于前台的窗口句柄
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // 检查指定窗口句柄（hWnd）的窗口是否可见
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        // 更改指定窗口的属性。函数也设置一个新的窗口过程（如果必要）。
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        // 检索有关指定窗口的信息。
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        // 设置分层窗口的属性，例如透明度和颜色键。
        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

        // 获取分层窗口的属性
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetLayeredWindowAttributes(IntPtr hwnd, out uint crKey, out byte bAlpha, out uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        #endregion

        private const int HOTKEY_ID_1 = 1;
        private const int HOTKEY_ID_2 = 2;
        private const int HOTKEY_ID_3 = 3;

        private const int HWND_TOPMOST = -1;
        private const int HWND_NOTOPMOST = -2;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const int WS_EX_TOPMOST = 0x00000008;

        private const uint MOD_ALT = 0x0001;
        private const uint LWA_ALPHA = 0x00000002;

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private Button button1, button2, button3, button4, button5, button6;
        private TextBox textBox1, textBox2, textBox3;
        private Label label1;
        private ListBox listBox1;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        private IntPtr selectedWindowHandle = IntPtr.Zero;

        public Form1()
        {
            // 初始化窗体组件
            InitializeComponent();

            FillWindowList();
            listBox1.SelectedIndexChanged += new EventHandler(this.listBox1_SelectedIndexChanged);
            this.KeyPreview = true;
            this.KeyUp += new KeyEventHandler(this.Form1_KeyUp);
            textBox1.Text = "C";
            textBox2.Text = "Z";
            textBox3.Text = "X";

            RegisterHotKey(this.Handle, HOTKEY_ID_1, MOD_ALT, (uint)Keys.C);
            RegisterHotKey(this.Handle, HOTKEY_ID_2, MOD_ALT, (uint)Keys.Z);
            RegisterHotKey(this.Handle, HOTKEY_ID_3, MOD_ALT, (uint)Keys.X);

            trayIcon.MouseDoubleClick += TrayIcon_MouseDoubleClick;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (listeningToHotkeys)
            {
                if (textBox1.Enabled == false)
                {
                    textBox1.Text = e.KeyCode.ToString();
                    button2_Click(null, null);
                }
                else if (textBox2.Enabled == false)
                {
                    textBox2.Text = e.KeyCode.ToString();
                    button3_Click(null, null);
                }
                else if (textBox3.Enabled == false)
                {
                    textBox3.Text = e.KeyCode.ToString();
                    button4_Click(null, null);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ToggleWindowVisibility();
        }

        private void ToggleWindowVisibility()
        {
            if (selectedWindowHandle != IntPtr.Zero)
            {
                if (selectedWindowHandle != this.Handle)
                {
                    if (IsWindowVisible(selectedWindowHandle))
                    {
                        ShowWindow(selectedWindowHandle, SW_HIDE);
                    }
                    else
                    {
                        ShowWindow(selectedWindowHandle, SW_SHOW);
                    }
                }
            }
        }

        private static bool IsApplicationWindow(IntPtr hWnd)
        {
            if (hWnd == GetShellWindow()) return false;
            if (!IsWindowVisible(hWnd)) return false;

            int length = GetWindowTextLength(hWnd);
            if (length == 0) return false;

            return true;
        }

        private void FillWindowList()
        {
            listBox1.Items.Clear();

            EnumWindowsProc enumWindowsProc = (hWnd, lParam) =>
            {
                if (!IsApplicationWindow(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder stringBuilder = new StringBuilder(length + 1);
                GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity);
                string windowTitle = stringBuilder.ToString();

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    listBox1.Items.Add(new KeyValuePair<IntPtr, string>(hWnd, windowTitle));
                    listBox1.DisplayMember = "Value";
                    listBox1.ValueMember = "Key";
                }

                return true;
            };

            EnumWindows(enumWindowsProc, IntPtr.Zero);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                KeyValuePair<IntPtr, string> selectedItem = (KeyValuePair<IntPtr, string>)listBox1.SelectedItem;
                label1.Text = selectedItem.Value;
                selectedWindowHandle = selectedItem.Key;
            }
        }

        private bool listeningToHotkeys = false;
        private void button2_Click(object sender, EventArgs e)
        {
            if (!listeningToHotkeys)
            {
                listeningToHotkeys = true;
                textBox1.Enabled = false;
            }
            else
            {
                listeningToHotkeys = false;
                textBox1.Enabled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!listeningToHotkeys)
            {
                listeningToHotkeys = true;
                textBox2.Enabled = false;
            }
            else
            {
                listeningToHotkeys = false;
                textBox2.Enabled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!listeningToHotkeys)
            {
                listeningToHotkeys = true;
                textBox3.Enabled = false;
            }
            else
            {
                listeningToHotkeys = false;
                textBox3.Enabled = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FillWindowList();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (selectedWindowHandle != IntPtr.Zero)
            {
                bool isTopMost = IsTopMost(selectedWindowHandle);
                IntPtr hWndInsertAfter = isTopMost ? new IntPtr(HWND_NOTOPMOST) : new IntPtr(HWND_TOPMOST);
                SetWindowPos(selectedWindowHandle, hWndInsertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            }
        }

        private bool IsTopMost(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            return (exStyle & WS_EX_TOPMOST) == WS_EX_TOPMOST;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;

            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                switch (id)
                {
                    case HOTKEY_ID_1:
                        ToggleWindowVisibility();
                        break;
                    case HOTKEY_ID_2:
                        // 提高透明度
                        AdjustWindowTransparency(10);
                        break;
                    case HOTKEY_ID_3:
                        // 降低透明度
                        AdjustWindowTransparency(-10);
                        break;
                }
            }

            base.WndProc(ref m);
        }

        private void AdjustWindowTransparency(int change)
        {
            if (selectedWindowHandle != IntPtr.Zero)
            {
                int exStyle = GetWindowLong(selectedWindowHandle, GWL_EXSTYLE);
                SetWindowLong(selectedWindowHandle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);

                // 首先检查窗口是否已设置为分层。
                // 如果窗口尚未设置为分层（可能导致窗口突然变得全透明），将设置窗口为分层并将其透明度设置为100%。
                // 然后再进行透明度调整。
                if ((exStyle & WS_EX_LAYERED) != WS_EX_LAYERED)
                {
                    SetWindowLong(selectedWindowHandle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
                    SetLayeredWindowAttributes(selectedWindowHandle, 0, 255, LWA_ALPHA);
                }

                byte currentAlpha;
                GetLayeredWindowAttributes(selectedWindowHandle, out uint colorKey, out currentAlpha, out uint flags);

                int newAlpha = currentAlpha + change;
                newAlpha = Math.Max(0, Math.Min(255, newAlpha));

                SetLayeredWindowAttributes(selectedWindowHandle, 0, (byte)newAlpha, LWA_ALPHA);
            }
        }

        private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID_1);
            UnregisterHotKey(this.Handle, HOTKEY_ID_2);
            UnregisterHotKey(this.Handle, HOTKEY_ID_3);

            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
