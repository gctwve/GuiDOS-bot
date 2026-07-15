using System;
using System.Collections.Generic;

namespace YourCheese.GameAgent.Conversation
{
    class ChatMiniMemory
    {
        public readonly HashSet<string> SeenMessages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly List<string> RecentTopics = new List<string>();
        public bool GreetedOpenChat;
        public bool AskedWhere;
        public bool AskedSus;

        public void RememberMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            SeenMessages.Add(message);
            var lower = message.ToLowerInvariant();
            if (lower.Contains("where"))
            {
                RememberTopic("where");
            }
            if (lower.Contains("who") || lower.Contains("sus") || lower.Contains("imp"))
            {
                RememberTopic("sus");
            }
            if (lower.Contains("vote"))
            {
                RememberTopic("vote");
            }
        }

        public void RememberTopic(string topic)
        {
            if (RecentTopics.Contains(topic))
            {
                return;
            }

            RecentTopics.Add(topic);
            if (RecentTopics.Count > 8)
            {
                RecentTopics.RemoveAt(0);
            }
        }

        public void Clear()
        {
            SeenMessages.Clear();
            RecentTopics.Clear();
            ClearPrompts();
        }

        public void ClearPrompts()
        {
            GreetedOpenChat = false;
            AskedWhere = false;
            AskedSus = false;
        }
    }
}
