using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YourCheese
{
    static class GameWindow
    {
        public const int DesignWidth = 1920;
        public const int DesignHeight = 1080;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        public static Rectangle GetContentBounds()
        {
            if (TryGetClientBounds(out var clientBounds))
            {
                return GetAspectFittedContentBounds(clientBounds);
            }

            var screen = SystemInformation.VirtualScreen;
            return GetAspectFittedContentBounds(screen);
        }

        public static Rectangle ScaleDesignRect(Rectangle designRect)
        {
            var content = GetContentBounds();
            var x = content.Left + (int)Math.Round(designRect.Left * content.Width / (double)DesignWidth);
            var y = content.Top + (int)Math.Round(designRect.Top * content.Height / (double)DesignHeight);
            var width = Math.Max(1, (int)Math.Round(designRect.Width * content.Width / (double)DesignWidth));
            var height = Math.Max(1, (int)Math.Round(designRect.Height * content.Height / (double)DesignHeight));
            return new Rectangle(x, y, width, height);
        }

        public static Vector2 DesignPointToScreen(Vector2 point)
        {
            var content = GetContentBounds();
            var x = content.Left + point.x * content.Width / DesignWidth;
            var y = content.Top + point.y * content.Height / DesignHeight;
            return new Vector2(x, y);
        }

        private static bool TryGetClientBounds(out Rectangle bounds)
        {
            bounds = Rectangle.Empty;
            try
            {
                foreach (var process in Process.GetProcessesByName("Among Us"))
                {
                    if (process.MainWindowHandle == IntPtr.Zero)
                    {
                        continue;
                    }

                    if (!GetClientRect(process.MainWindowHandle, out var rect))
                    {
                        continue;
                    }

                    var topLeft = new POINT { X = 0, Y = 0 };
                    if (!ClientToScreen(process.MainWindowHandle, ref topLeft))
                    {
                        continue;
                    }

                    var width = rect.Right - rect.Left;
                    var height = rect.Bottom - rect.Top;
                    if (width > 100 && height > 100)
                    {
                        bounds = new Rectangle(topLeft.X, topLeft.Y, width, height);
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        private static Rectangle GetAspectFittedContentBounds(Rectangle bounds)
        {
            var designAspect = DesignWidth / (double)DesignHeight;
            var boundsAspect = bounds.Width / (double)Math.Max(1, bounds.Height);

            if (boundsAspect > designAspect)
            {
                var width = (int)Math.Round(bounds.Height * designAspect);
                var x = bounds.Left + (bounds.Width - width) / 2;
                return new Rectangle(x, bounds.Top, width, bounds.Height);
            }

            var height = (int)Math.Round(bounds.Width / designAspect);
            var y = bounds.Top + (bounds.Height - height) / 2;
            return new Rectangle(bounds.Left, y, bounds.Width, height);
        }
    }
}
