using System;
using WindowsInput.Native;

namespace YourCheese
{
    class TaskInput
    {
        public static bool SuppressActionKeys { get; set; } = false;

        public void mouseClick(Vector2 position)
        {
            if (SuppressActionKeys)
            {
                return;
            }

            moveMouse(position);
            System.Threading.Thread.Sleep(80);
            NativeInput.MouseDown();
            System.Threading.Thread.Sleep(70);
            NativeInput.MouseUp();
            System.Threading.Thread.Sleep(80);
        }

        public void mouseDown(Vector2 position)
        {
            if (SuppressActionKeys)
            {
                return;
            }

            moveMouse(position);
            System.Threading.Thread.Sleep(80);
            NativeInput.MouseDown();
        }

        public void releaseMouse()
        {
            NativeInput.MouseUp();
        }

        public void pressE()
        {
            pressKey(VirtualKeyCode.VK_E);
        }

        public void pressR()
        {
            pressKey(VirtualKeyCode.VK_R);
        }

        public void pressQ()
        {
            pressKey(VirtualKeyCode.VK_Q);
            System.Threading.Thread.Sleep(50);
        }

        public void pressF()
        {
            pressKey(VirtualKeyCode.VK_F);
            System.Threading.Thread.Sleep(100);
        }

        public void pressEsc()
        {
            pressKey(VirtualKeyCode.ESCAPE);
        }

        public void closeTask()
        {
            mouseClick(TaskScreenLayout.CloseButton);
        }

        public void dragMouse(Vector2 position, Vector2 destination)
        {
            if (SuppressActionKeys)
            {
                return;
            }

            moveMouse(position);
            System.Threading.Thread.Sleep(80);
            NativeInput.MouseDown();
            System.Threading.Thread.Sleep(80);
            moveMouse(destination);
            System.Threading.Thread.Sleep(80);
            NativeInput.MouseUp();
            System.Threading.Thread.Sleep(80);
        }

        public void dragMouseNoRelease(Vector2 position, Vector2 destination)
        {
            if (SuppressActionKeys)
            {
                return;
            }

            moveMouse(position);
            System.Threading.Thread.Sleep(80);
            NativeInput.MouseDown();
            System.Threading.Thread.Sleep(80);
            moveMouse(destination);
            System.Threading.Thread.Sleep(80);
        }

        public void dragMouseLinear(Vector2 position, Vector2 destination, float milliseconds)
        {
            if (SuppressActionKeys)
            {
                return;
            }

            int steps = Math.Max(1, (int)Math.Round(milliseconds / 20));
            Vector2[] points = Vector2.pointsInBetween(position, destination, steps);

            moveMouse(position);
            System.Threading.Thread.Sleep(80);
            NativeInput.MouseDown();
            System.Threading.Thread.Sleep(80);

            foreach (var point in points)
            {
                moveMouse(point);
                System.Threading.Thread.Sleep(20);
            }

            moveMouse(destination);
            System.Threading.Thread.Sleep(80);
            NativeInput.MouseUp();
            System.Threading.Thread.Sleep(80);
        }

        private void pressKey(VirtualKeyCode key)
        {
            if (SuppressActionKeys)
            {
                return;
            }

            NativeInput.FocusAmongUs();
            NativeInput.KeyPress(key);
        }

        private void moveMouse(Vector2 destination)
        {
            NativeInput.FocusAmongUs();
            var screenDestination = GameWindow.DesignPointToScreen(destination);
            Console.WriteLine($"Moving real mouse to {screenDestination.x},{screenDestination.y} for task point {destination.x},{destination.y}");
            NativeInput.MoveMouse(screenDestination);
        }
    }
}
