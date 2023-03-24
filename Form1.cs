using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using NHotkey;
using NHotkey.WindowsForms;
using Gma.System.MouseKeyHook;
using System.Windows.Input;

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
        #endregion 

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int LWA_ALPHA = 0x2;

        private IKeyboardMouseEvents _mouseHook;

        // 存储用户选择的按键和修饰符
        public Keys ShortcutKey { get; private set; }
        public ModifierKeys ShortcutModifiers { get; private set; }

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        public Form1()
        {
            // 初始化窗体组件
            InitializeComponent();

            _mouseHook = Hook.GlobalEvents();
            _mouseHook.MouseWheel += MouseHook_MouseWheel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ToggleWindowVisibility();
        }

        private void button1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                ToggleWindowVisibility();
            }
        }

        private void ToggleWindowVisibility()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd != this.Handle)
            {
                if (IsWindowVisible(hWnd))
                {
                    ShowWindow(hWnd, SW_HIDE);
                }
                else
                {
                    ShowWindow(hWnd, SW_SHOW);
                }
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void HideWindow(object sender, HotkeyEventArgs e)
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd != IntPtr.Zero)
            {
                if (IsWindowVisible(hWnd))
                {
                    this.WindowState = FormWindowState.Minimized;
                }
            }
        }

        private void MouseHook_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Form.ModifierKeys == Keys.Alt)
            {
                IntPtr hWnd = GetForegroundWindow();
                if (hWnd != IntPtr.Zero)
                {
                    int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
                    if ((exStyle & WS_EX_LAYERED) == 0) // 检查窗口是否已具有 WS_EX_LAYERED 样式
                    {
                        SetWindowLong(hWnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
                        SetLayeredWindowAttributes(hWnd, 0, 255, LWA_ALPHA); // 设置窗口为不透明，避免初始全透明
                    }

                    int currentTransparency;
                    GetLayeredWindowAttributes(hWnd, out uint _, out byte bAlpha, out uint _);
                    currentTransparency = bAlpha;

                    int delta = e.Delta > 0 ? 10 : -10; // 根据滚轮方向调整透明度
                    int newTransparency = Math.Max(0, Math.Min(255, currentTransparency + delta));
                    SetLayeredWindowAttributes(hWnd, 0, (byte)newTransparency, LWA_ALPHA);
                }
            }
        }

        // 在窗口关闭时注销全局热键
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            _mouseHook.MouseWheel -= MouseHook_MouseWheel;
            _mouseHook.Dispose();
        }
    }
}
