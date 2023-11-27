/**********************************************************************/
/* Copyright (c) 2023 Carpe Diem Software Developing by Alex Versetty */
/* http://carpediem.0fees.us                                          */
/**********************************************************************/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrayTextPaste.Properties;
using WindowsInput;
using WindowsInput.Native;

namespace TrayTextPaste
{
    internal static class Program
    {
        static KeyboardHook hook;
        static InputSimulator inputSim = new InputSimulator();
        static string CSPHotkeySeq;
        static bool CSPHotkeyWorking;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                ConfigManager.load();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка чтения файла конфигурации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            RegisterHotkeys();
            Application.Run(new TTPApplicationContext());
        }

        private static void RegisterHotkeys()
        {
            hook = new KeyboardHook();
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            CSPHotkeySeq = ConfigManager.Current.CopySwitchPasteHotkey;

            try
            {
                switch (CSPHotkeySeq)
                {
                    case "F18": hook.RegisterHotKey(ModifierKeys.None, Keys.F18); break;
                    case "F19": hook.RegisterHotKey(ModifierKeys.None, Keys.F19); break;
                    case "Shift-Ctrl-Alt-Z": hook.RegisterHotKey(ModifierKeys.Shift | ModifierKeys.Control | ModifierKeys.Alt, Keys.Z); break;
                    case "Shift-Ctrl-Alt-V": hook.RegisterHotKey(ModifierKeys.Shift | ModifierKeys.Control | ModifierKeys.Alt, Keys.V); break;
                    case "Ctrl-Alt-Insert": hook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt, Keys.Insert); break;
                    case "Ctrl-Alt-Num+": hook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt, Keys.Add); break;
                    case "Ctrl-Alt-Num*": hook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt, Keys.Multiply); break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка регистрации глобальной комбинации клавиш для Copy+C, Alt+Tab, Ctrl+V (возможно эта комбинация клавиш уже занята)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            //атомарная операция, lock не требуется
            if (CSPHotkeyWorking) return;
            else CSPHotkeyWorking = true;
            
            Task.Factory.StartNew(() =>
            {
                while (inputSim.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.LSHIFT) ||
                    inputSim.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.RSHIFT) ||
                    inputSim.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.LCONTROL) ||
                    inputSim.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.RCONTROL) ||
                    inputSim.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.LMENU) ||
                    inputSim.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.RMENU) ||
                    inputSim.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.VK_Z) ||
                    inputSim.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.VK_V) ||
                    inputSim.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.INSERT) ||
                    inputSim.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.ADD) ||
                    inputSim.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.MULTIPLY) ||
                    inputSim.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.F18) ||
                    inputSim.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.F19)) 
                {
                    inputSim.Keyboard.Sleep(20);
                }
                
                inputSim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LCONTROL, VirtualKeyCode.VK_C);
                inputSim.Keyboard.Sleep(50);
                inputSim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LMENU, VirtualKeyCode.TAB);
                inputSim.Keyboard.Sleep(200); //ждем готовности буфера обмена
                inputSim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LCONTROL, VirtualKeyCode.VK_V);
                
                /* аналогичный код ниже с Word/Excel в основном не работает, почему - понятия не имею
                SendKeys.SendWait("^(c)%{tab}");
                Thread.Sleep(200);
                SendKeys.SendWait("^(v)");*/

                CSPHotkeyWorking = false; //атомарная операция, lock не требуется
            });
        }
    }

    public class TTPApplicationContext : ApplicationContext
    {
        const int maxItemLength = 50;
        NotifyIcon trayIcon;
        IntPtr lastAppWindowHandle;
        InputSimulator inputSim = new InputSimulator();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true)]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        public static string GetWindowTitle(IntPtr hWnd)
        {
            var length = GetWindowTextLength(hWnd) + 1;
            var title = new StringBuilder(length);
            GetWindowText(hWnd, title, length);
            return title.ToString();
        }

        public TTPApplicationContext()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = Resources.AppIcon,
                ContextMenu = new ContextMenu(),
                Visible = true
            };

            trayIcon.DoubleClick += TrayIcon_DClick;
            SetupMenuItems();
            RunLastActiveWindowChecker();
        }

        private void TrayIcon_DClick(object sender, EventArgs e)
        {
            OnEdit(null, null);
        }

        private void RunLastActiveWindowChecker()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    var h = GetForegroundWindow();
                    if (h == IntPtr.Zero) continue;
                    if (!IsWindowVisible(h)) continue;
                    var t = GetWindowTitle(h);
                    if (!string.IsNullOrEmpty(t)) lastAppWindowHandle = h;
                }
            });
        }

        void OnExit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        void OnEdit(object sender, EventArgs e)
        {
            var f = new FormEditor();
            f.ShowDialog();
            SetupMenuItems();
        }

        void OnConfig(object sender, EventArgs e)
        {
            var f = new FormConfig();
            f.ShowDialog();
            SetupMenuItems();
        }

        void OnSelectItem(object sender, EventArgs e)
        {
            var menuitem = sender as MenuItem;

            if (menuitem != null && lastAppWindowHandle != IntPtr.Zero && menuitem.Tag is string)
            {
                SetForegroundWindow(lastAppWindowHandle);

                string[] sep = { "{tab}", "{TAB}", "{Tab}" };
                var strArray = (menuitem.Tag as string).Split(sep, StringSplitOptions.None);

                for (int i = 0; i < strArray.Count(); i++)
                {
                    if (strArray[i] != "") inputSim.Keyboard.TextEntry(strArray[i]);
                    
                    if (i != (strArray.Count() - 1))
                    {
                        inputSim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        Thread.Sleep(20);
                    }
                }
            }
        }

        private void SetupMenuItems()
        {
            trayIcon.ContextMenu.MenuItems.Clear();
            MenuItem parent = null;

            string[] sep = { "\r\n" };
            var strArray = ConfigManager.Current.Strings.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            foreach (var str in strArray)
            {
                var item = new MenuItem(ShortenItemName(str), OnSelectItem);
                
                if (HaveStartSpaces(str))
                {
                    //sub menu
                    parent.MenuItems.Add(item);
                    item.Tag = str.TrimStart();
                }
                else
                {
                    //top menu
                    parent = item;
                    trayIcon.ContextMenu.MenuItems.Add(item);
                    item.Tag = str;
                }
            }

            trayIcon.ContextMenu.MenuItems.Add("------------------------------");
            trayIcon.ContextMenu.MenuItems.Add(new MenuItem("<Редактировать строки>", OnEdit));
            trayIcon.ContextMenu.MenuItems.Add(new MenuItem("<Настройки>", OnConfig));
            trayIcon.ContextMenu.MenuItems.Add(new MenuItem("<Выход>", OnExit));
        }

        private bool HaveStartSpaces(string str)
        {
            var strTrimmed = str.TrimStart(new char[] { ' ' });
            return str.Length - strTrimmed.Length > 1;
        }

        private string ShortenItemName(string x)
        {
            var str = x.Trim();
            return str.Length > maxItemLength ? str.Substring(0, maxItemLength) + "..." : str;
        }
    }
}
