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
        private static readonly Queue<string> RecentMessages = new Queue<string>();
        private static readonly HashSet<string> RecentMessageSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly InputSimulator inputSimulator = new InputSimulator();

        public bool Enabled { get; set; } = true;

        public bool Send(string text)
        {
            if (!Enabled)
            {
                return false;
            }

            var message = Normalize(text);
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            lock (SendLock)
            {
                if (RecentMessageSet.Contains(message))
                {
                    return false;
                }

                var wait = 900 - (int)(DateTime.UtcNow - lastSend).TotalMilliseconds;
                if (wait > 0)
                {
                    Thread.Sleep(wait);
                }

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
                RememberSent(message);
                return true;
            }
        }

        public static void ClearRecentMessages()
        {
            lock (SendLock)
            {
                RecentMessages.Clear();
                RecentMessageSet.Clear();
            }
        }

        private static void RememberSent(string message)
        {
            RecentMessages.Enqueue(message);
            RecentMessageSet.Add(message);
            while (RecentMessages.Count > 16)
            {
                RecentMessageSet.Remove(RecentMessages.Dequeue());
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
