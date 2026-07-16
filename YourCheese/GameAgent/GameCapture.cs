using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace YourCheese
{

    class GameCapture
    {
        public static DirectBitmap getGameScreen()
        {
            using (var captured = CaptureScaledDesignScreen())
            {
                DirectBitmap bitmap = new DirectBitmap(captured.Width, captured.Height);
                using (Graphics g = Graphics.FromImage(bitmap.Bitmap))
                {
                    g.DrawImage(captured, Point.Empty);
                }

                SaveDebugImage(bitmap.Bitmap);
                return bitmap;
            }
        }

        public static DirectBitmap getGameScreen(Rectangle bounds)
        {
            DirectBitmap bitmap = new DirectBitmap(bounds.Width, bounds.Height);
            using (var captured = CaptureScaledDesignScreen())
            using (var cropped = captured.Clone(bounds, captured.PixelFormat))
            using (Graphics g = Graphics.FromImage(bitmap.Bitmap))
            {
                g.DrawImage(cropped, Point.Empty);
            }

            return bitmap;
        }

        public static Bitmap getGameScreenAsImage()
        {
            Bitmap bitmap = CaptureScaledDesignScreen();
            SaveDebugImage(bitmap);

            return bitmap;
            
        }

        public static Bitmap getGameScreenAsImage(Rectangle bounds)
        {
            Bitmap bitmap;
            using (var captured = CaptureScaledDesignScreen())
            {
                bitmap = captured.Clone(bounds, PixelFormat.Format32bppArgb);
            }
            SaveDebugImage(bitmap);

            return bitmap;

        }

        private static Bitmap CaptureScaledDesignScreen()
        {
            Rectangle bounds = GameWindow.GetContentBounds();
            Bitmap source = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(source))
            {
                g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            Bitmap scaled = new Bitmap(GameWindow.DesignWidth, GameWindow.DesignHeight, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(scaled))
            {
                g.DrawImage(source, new Rectangle(0, 0, scaled.Width, scaled.Height));
            }

            source.Dispose();
            return scaled;
        }

        private static void SaveDebugImage(Bitmap bitmap)
        {
            try
            {
                var path = System.IO.Path.Combine(Constants.FILE_LOCATION, "templates", "CURRENTLY_ORIGINAL.png");
                bitmap.Save(path, ImageFormat.Png);
            }
            catch
            {
            }
        }

    }
}
