using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using WindowsInput.Native;

namespace YourCheese
{
    static class NativeInput
    {
        private const int InputMouse = 0;
        private const int InputKeyboard = 1;
        private const uint KeyEventFKeyUp = 0x0002;
        private const uint KeyEventFScanCode = 0x0008;
        private const uint MouseEventFMove = 0x0001;
        private const uint MouseEventFLeftDown = 0x0002;
        private const uint MouseEventFLeftUp = 0x0004;
        private const uint MouseEventFAbsolute = 0x8000;
        private const uint MouseEventFVirtualDesk = 0x4000;
        private const uint MapVkToVsc = 0;

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool SetPhysicalCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT point);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void FocusAmongUs()
        {
            try
            {
                var processes = Process.GetProcessesByName("Among Us");
                if (processes.Length > 0 && processes[0].MainWindowHandle != IntPtr.Zero)
                {
                    SetForegroundWindow(processes[0].MainWindowHandle);
                    Thread.Sleep(35);
                }
            }
            catch
            {
            }
        }

        public static void MoveMouse(Vector2 destination)
        {
            var screen = SystemInformation.VirtualScreen;
            var targetX = (int)Math.Round(Math.Max(screen.Left, Math.Min(screen.Right - 1, destination.x)));
            var targetY = (int)Math.Round(Math.Max(screen.Top, Math.Min(screen.Bottom - 1, destination.y)));

            var normalizedX = (int)Math.Round((targetX - screen.Left) * 65535.0 / Math.Max(1, screen.Width - 1));
            var normalizedY = (int)Math.Round((targetY - screen.Top) * 65535.0 / Math.Max(1, screen.Height - 1));
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var before = GetCursorPoint();
                SetCursorPos(targetX, targetY);
                SetPhysicalCursorPos(targetX, targetY);
                Cursor.Position = new Point(targetX, targetY);
                SendMouse(normalizedX, normalizedY, MouseEventFMove | MouseEventFAbsolute | MouseEventFVirtualDesk);
                mouse_event(MouseEventFMove | MouseEventFAbsolute | MouseEventFVirtualDesk, (uint)normalizedX, (uint)normalizedY, 0, UIntPtr.Zero);
                Thread.Sleep(25);

                var current = GetCursorPoint();
                if (IsClose(current, targetX, targetY))
                {
                    return;
                }

                var dx = targetX - current.X;
                var dy = targetY - current.Y;
                SendMouse(dx, dy, MouseEventFMove);
                mouse_event(MouseEventFMove, (uint)dx, (uint)dy, 0, UIntPtr.Zero);
                Thread.Sleep(25);

                current = GetCursorPoint();
                if (IsClose(current, targetX, targetY))
                {
                    return;
                }

                Console.WriteLine($"Mouse move attempt {attempt + 1}: before {before.X},{before.Y}, wanted {targetX},{targetY}, now {current.X},{current.Y}");
            }

            var final = GetCursorPoint();
            Console.WriteLine($"Cursor did not reach requested position. Wanted {targetX},{targetY}; got {final.X},{final.Y}");
        }

        public static void MouseDown()
        {
            SendMouse(0, 0, MouseEventFLeftDown);
            mouse_event(MouseEventFLeftDown, 0, 0, 0, UIntPtr.Zero);
        }

        public static void MouseUp()
        {
            SendMouse(0, 0, MouseEventFLeftUp);
            mouse_event(MouseEventFLeftUp, 0, 0, 0, UIntPtr.Zero);
        }

        public static void KeyDown(VirtualKeyCode key)
        {
            SendKey(key, false);
        }

        public static void KeyUp(VirtualKeyCode key)
        {
            SendKey(key, true);
        }

        public static void KeyPress(VirtualKeyCode key, int holdMilliseconds = 45)
        {
            KeyDown(key);
            Thread.Sleep(holdMilliseconds);
            KeyUp(key);
        }

        private static void SendMouse(int x, int y, uint flags)
        {
            var input = new INPUT
            {
                type = InputMouse,
                u = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = x,
                        dy = y,
                        dwFlags = flags
                    }
                }
            };
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendKey(VirtualKeyCode key, bool keyUp)
        {
            var input = new INPUT
            {
                type = InputKeyboard,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wScan = (ushort)MapVirtualKey((uint)key, MapVkToVsc),
                        dwFlags = KeyEventFScanCode | (keyUp ? KeyEventFKeyUp : 0)
                    }
                }
            };
            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private static POINT GetCursorPoint()
        {
            if (GetCursorPos(out var point))
            {
                return point;
            }

            var position = Cursor.Position;
            return new POINT { X = position.X, Y = position.Y };
        }

        private static bool IsClose(POINT point, int x, int y)
        {
            return Math.Abs(point.X - x) <= 2 && Math.Abs(point.Y - y) <= 2;
        }
    }
}
