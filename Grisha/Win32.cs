using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace VCC
{
    public static class Win32
    {
        public const int WM_SENDTEXT = 0x00C;
        public const int WM_SETCURSOR = 0x020;
        public const int WM_COMMAND = 0x111;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_CHAR = 0x102;
        public const int WM_MOUSEMOVE = 0x200;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_LBUTTONDBLCLK = 0x203;

        public static IntPtr VK_ALT = (IntPtr)0x12;
        public static IntPtr VK_CTRL = (IntPtr)0x11;
        public static IntPtr VK_ENTER = (IntPtr)0x0D;

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_APPWINDOW = 0x00040000;

        [DllImport("User32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, IntPtr strWindowName);


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);


        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr PostMessageA(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);


        [DllImport("User32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr window, int index, int
        value);



        [DllImport("user32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int RegisterWindowMessage(string lpString);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr window, int index);

        [DllImport("user32.dll")]
        public static extern byte VkKeyScan(char ch);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);

        public enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = System.Runtime.InteropServices.CharSet.Auto)] //
        public static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);


        [DllImport("user32.dll", EntryPoint = "SendMessage",
  CharSet = CharSet.Auto)]
        static extern int SendMessage3(IntPtr hwndControl, uint Msg,
          int wParam, StringBuilder strBuffer); // get text

        [DllImport("user32.dll", EntryPoint = "SendMessage",
          CharSet = CharSet.Auto)]
        static extern int SendMessage4(IntPtr hwndControl, uint Msg,
          int wParam, int lParam);  // text length

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        public static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size++ > 0)
            {
                var builder = new StringBuilder(size);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return String.Empty;
        }

        public static int FindWindowsWithText(string titleText)
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate(IntPtr wnd, IntPtr param)
            {
                if (GetWindowText(wnd).Contains(titleText))
                {
                    windows.Add(wnd);
                }
                return true;
            },
                        IntPtr.Zero);

            return windows.Count;
        } // closing bracket

        static int GetTextBoxTextLength(IntPtr hTextBox)
        {
            // helper for GetTextBoxText
            uint WM_GETTEXTLENGTH = 0x000E;
            int result = SendMessage4(hTextBox, WM_GETTEXTLENGTH,
              0, 0);
            return result;
        }

        public static string GetTextBoxText(IntPtr hTextBox)
        {
            uint WM_GETTEXT = 0x000D;
            int len = GetTextBoxTextLength(hTextBox);
            if (len <= 0) return null;  // no text
            StringBuilder sb = new StringBuilder(len + 1);
            SendMessage3(hTextBox, WM_GETTEXT, len + 1, sb);
            return sb.ToString();
        }

        public static IntPtr getChild(IntPtr handle, int depth)
        {
            int i=1;
            IntPtr child = GetWindow(handle, GetWindow_Cmd.GW_CHILD);
            while (i < depth) { 
                child = GetWindow(child, GetWindow_Cmd.GW_CHILD);
                i++;
            }
            return child;
            
        }


        const int  WM_GETTEXT = 0x000D;
        const int WM_GETTEXTLENGTH = 0x000E;

        public static void RegisterControlforMessages()
        {
          RegisterWindowMessage("WM_GETTEXT");
        }

        public static string GetControlText(IntPtr hWnd)
        {

            StringBuilder title = new StringBuilder();

            // Get the size of the string required to hold the window title. 
            Int32 size = SendMessage(hWnd, (uint)WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();

            // If the return is 0, there is no title. 
            if (size > 0)
            {
                title = new StringBuilder(size + 1);

                SendMessage(hWnd, (int)WM_GETTEXT, title.Capacity, title);


            }
            return title.ToString();
        }

        public static IntPtr getMainHandle(string name)
        {
            return Process.GetProcessesByName(name)[0].MainWindowHandle;
        }

        public static IntPtr getChildHandle(string name,string className, string title=""){
           IntPtr mainHandle = Process.GetProcessesByName(name)[0].MainWindowHandle;
           return FindWindowEx(mainHandle, IntPtr.Zero, className, title);
        }

        public static void ALT(IntPtr hWnd, IntPtr key)
        {
            StringBuilder ClassName = new StringBuilder(256);
            //Get the window class name
            int nRet = GetClassName(hWnd, ClassName, ClassName.Capacity);
            if (nRet != 0)
            {
                Logger.add("WIN32", "class name " + ClassName.ToString());
            }
            PostMessageA(hWnd, WM_KEYDOWN, VK_ALT, lParam(VK_ALT));
            Thread.Sleep(200);
            PostMessageA(hWnd, WM_KEYDOWN, VK_ALT, lParam(key));
            Thread.Sleep(200);
            PostMessageA(hWnd, WM_CHAR, key, (IntPtr)0x20210001);
            Logger.add("WIN32", "syschar" + lParam(key));;
        }

        public static void CTRL(IntPtr hWnd, IntPtr key)
        {
            Logger.add("WIN", "lParam " + lParam(VK_CTRL));
            PostMessage(hWnd, WM_KEYDOWN, VK_CTRL, lParam(VK_CTRL));
            Thread.Sleep(500);
            Logger.add("WIN", "lParam " + lParam(key));
            PostMessage(hWnd, WM_KEYDOWN, key, lParam(key));
        }

        public static void ENTER(IntPtr hWnd)
        {
            PostMessageA(hWnd, WM_KEYDOWN, VK_ENTER, lParam(VK_ENTER));
            PostMessageA(hWnd, WM_KEYUP, VK_ENTER, lParam(VK_ENTER));
        }

        public static void sendkey(IntPtr hWnd, IntPtr key)
        {
            PostMessage(hWnd, WM_KEYDOWN, key, lParam(key));
            PostMessage(hWnd, WM_CHAR, key, lParam(key));
        }

        public static void sendString(IntPtr hWnd, string str)
        {
            PostMessage(hWnd, WM_SENDTEXT, IntPtr.Zero, Marshal.StringToHGlobalAuto(str));
        }

        public static IntPtr lParam(IntPtr key, bool ext = false)
        {
            uint scanCode = MapVirtualKey((uint)key, 0);
            if (ext)
            {
                return (IntPtr)((0x00000001 | (scanCode << 16)) | 0x01000000);
            }
            return (IntPtr)(0x00000001 | (scanCode << 16)); 
        }
    }
}
