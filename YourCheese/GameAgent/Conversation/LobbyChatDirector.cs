using System;
using System.Linq;

namespace YourCheese.GameAgent.Conversation
{
    class LobbyChatDirector
    {
        private readonly GameChatMessenger chatMessenger = new GameChatMessenger();
        private readonly Random random = new Random();
        private DateTime lastLobbyChat = DateTime.MinValue;
        private string lastLobbyMessage = string.Empty;

        public void Reset()
        {
            lastLobbyChat = DateTime.MinValue;
            lastLobbyMessage = string.Empty;
        }

        public void Update(GameDataContainer gameData)
        {
            if (gameData == null || !gameData.isInLobby)
            {
                return;
            }

            if ((DateTime.UtcNow - lastLobbyChat).TotalSeconds < 8)
            {
                return;
            }

            var message = BuildLobbyMessage(gameData);
            if (!string.IsNullOrWhiteSpace(message))
            {
                if (chatMessenger.Send(message))
                {
                    lastLobbyMessage = message;
                    lastLobbyChat = DateTime.UtcNow;
                }
            }
        }

        private string BuildLobbyMessage(GameDataContainer gameData)
        {
            var playerCount = (gameData.players == null ? 0 : gameData.players.Count) + (string.IsNullOrWhiteSpace(gameData.botPlayer.name) ? 0 : 1);
            var messages = new[]
            {
                "hi",
                "gl hf",
                playerCount > 1 ? $"we have {playerCount} players" : "waiting for players",
                "ready when everyone is"
            };

            return messages
                .Where(message => !string.Equals(message, lastLobbyMessage, StringComparison.OrdinalIgnoreCase))
                .OrderBy(_ => random.Next())
                .FirstOrDefault();
        }
    }
}
