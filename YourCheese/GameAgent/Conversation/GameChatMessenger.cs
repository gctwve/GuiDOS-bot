using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using WindowsInput;
using WindowsInput.Native;

namespace YourCheese.GameAgent.Conversation
{
    class GameChatMessenger
    {
        private const int MaxChatLength = 120;
        private static readonly object SendLock = new object();
        private static DateTime lastSend = DateTime.MinValue;
        private readonly InputSimulator inputSimulator = new InputSimulator();

        public bool Enabled { get; set; } = true;

        public void Send(string text)
        {
            if (!Enabled)
            {
                return;
            }

            var message = Normalize(text);
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            lock (SendLock)
            {
                var wait = 900 - (int)(DateTime.UtcNow - lastSend).TotalMilliseconds;
                if (wait > 0)
                {
                    Thread.Sleep(wait);
                }

                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.ESCAPE);
                Thread.Sleep(80);
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                Thread.Sleep(180);

                foreach (var character in message)
                {
                    TypeCharacter(character);
                    Thread.Sleep(18);
                }
                Thread.Sleep(50);
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                lastSend = DateTime.UtcNow;
            }
        }

        private void TypeCharacter(char character)
        {
            if (character >= 'a' && character <= 'z')
            {
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode)('A' + character - 'a'));
                return;
            }

            if (character >= 'A' && character <= 'Z')
            {
                inputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, (VirtualKeyCode)character);
                return;
            }

            if (character >= '0' && character <= '9')
            {
                inputSimulator.Keyboard.KeyPress((VirtualKeyCode)('0' + character - '0'));
                return;
            }

            if (character == ' ')
            {
                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                return;
            }

            var key = GetPunctuationKey(character);
            if (key.Key == 0)
            {
                return;
            }

            if (key.Shift)
            {
                inputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, key.Key);
            }
            else
            {
                inputSimulator.Keyboard.KeyPress(key.Key);
            }
        }

        private static KeyStroke GetPunctuationKey(char character)
        {
            switch (character)
            {
                case '.': return new KeyStroke(VirtualKeyCode.OEM_PERIOD);
                case ',': return new KeyStroke(VirtualKeyCode.OEM_COMMA);
                case '?': return new KeyStroke(VirtualKeyCode.OEM_2, true);
                case '!': return new KeyStroke(VirtualKeyCode.VK_1, true);
                case '\'': return new KeyStroke(VirtualKeyCode.OEM_7);
                case ':': return new KeyStroke(VirtualKeyCode.OEM_1, true);
                case ';': return new KeyStroke(VirtualKeyCode.OEM_1);
                case '-': return new KeyStroke(VirtualKeyCode.OEM_MINUS);
                case '/': return new KeyStroke(VirtualKeyCode.OEM_2);
                default: return new KeyStroke(0);
            }
        }

        private struct KeyStroke
        {
            public VirtualKeyCode Key;
            public bool Shift;

            public KeyStroke(VirtualKeyCode key, bool shift = false)
            {
                Key = key;
                Shift = shift;
            }
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var compact = string.Join(" ", text
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            return compact.Length <= MaxChatLength ? compact : compact.Substring(0, MaxChatLength);
        }
    }
}
