using HamsterCheese.AmongUsMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Speech.Synthesis;
using YourCheese.GameAgent;

namespace YourCheese.GameAgent.Conversation
{
    class MeetingChatDirector
    {
        private readonly GameChatMessenger chatMessenger = new GameChatMessenger();
        private readonly ChatMiniMemory memory = new ChatMiniMemory();
        private readonly Random random = new Random();
        private DateTime lastAmbientChat = DateTime.MinValue;
        private DateTime lastReply = DateTime.MinValue;
        private string lastSent = string.Empty;

        public void ResetMeetingState()
        {
            memory.ClearPrompts();
            lastAmbientChat = DateTime.MinValue;
            lastReply = DateTime.MinValue;
            lastSent = string.Empty;
        }

        public void ClearMemory()
        {
            memory.Clear();
            ResetMeetingState();
        }

        public void Update(GameDataContainer gameData, RoundMemory roundMemory, PlayerInformation reportedBody, SkeldMap map)
        {
            if (gameData == null || !gameData.isInMeeting || gameData.botPlayer.isDead)
            {
                return;
            }

            ReplyToNewChat(gameData, roundMemory, reportedBody, map);

            if ((DateTime.UtcNow - lastAmbientChat).TotalSeconds >= 5)
            {
                var line = BuildAmbientLine(gameData, roundMemory, reportedBody, map);
                if (!string.IsNullOrWhiteSpace(line))
                {
                    Say(line);
                    lastAmbientChat = DateTime.UtcNow;
                }
            }
        }

        private void ReplyToNewChat(GameDataContainer gameData, RoundMemory roundMemory, PlayerInformation reportedBody, SkeldMap map)
        {
            if ((DateTime.UtcNow - lastReply).TotalSeconds < 2)
            {
                return;
            }

            foreach (var message in Cheese.GetVisibleChatMessages())
            {
                var compact = Normalize(message);
                if (string.IsNullOrWhiteSpace(compact) || memory.SeenMessages.Contains(compact))
                {
                    continue;
                }

                memory.RememberMessage(compact);
                if (IsProbablyMine(compact, gameData.botPlayer))
                {
                    continue;
                }

                var reply = BuildReply(compact, gameData, roundMemory, reportedBody, map);
                if (!string.IsNullOrWhiteSpace(reply))
                {
                    Say(reply);
                    lastReply = DateTime.UtcNow;
                    lastAmbientChat = DateTime.UtcNow;
                    return;
                }
            }
        }

        private string BuildReply(string message, GameDataContainer gameData, RoundMemory roundMemory, PlayerInformation reportedBody, SkeldMap map)
        {
            var lower = message.ToLowerInvariant();
            if (lower.Contains("where") || lower.Contains("body"))
            {
                if (!reportedBody.position.IsGarbage())
                {
                    return $"Body was in {map.getLocationRegionName(map.gamePosToMeshPos(reportedBody.position))}.";
                }
                return $"I was in {map.getLocationRegionName(map.gamePosToMeshPos(gameData.botPlayer.position))}.";
            }

            if (lower.Contains("who") || lower.Contains("sus") || lower.Contains("imp"))
            {
                var witnessed = GetWitnessLine(roundMemory, map);
                if (!string.IsNullOrWhiteSpace(witnessed))
                {
                    return witnessed;
                }

                var visible = gameData.getVisiblePlayers().Where(x => !x.isDead).Take(3).Select(x => x.color);
                return visible.Any() ? $"I last saw {string.Join(", ", visible)} near me." : "No hard sus from me yet.";
            }

            if (lower.Contains(gameData.botPlayer.color.ToLowerInvariant()) || lower.Contains(gameData.botPlayer.name.ToLowerInvariant()))
            {
                return $"I was {map.getLocationRegionName(map.gamePosToMeshPos(gameData.botPlayer.position))}. Check my path.";
            }

            if (lower.Contains("vote"))
            {
                return "I can vote with the clearest evidence.";
            }

            return null;
        }

        private string BuildAmbientLine(GameDataContainer gameData, RoundMemory roundMemory, PlayerInformation reportedBody, SkeldMap map)
        {
            var witnessed = GetWitnessLine(roundMemory, map);
            if (!string.IsNullOrWhiteSpace(witnessed))
            {
                return witnessed;
            }

            if (!reportedBody.position.IsGarbage())
            {
                return $"Report info: body was around {map.getLocationRegionName(map.gamePosToMeshPos(reportedBody.position))}.";
            }

            var choices = new[]
            {
                $"I was around {map.getLocationRegionName(map.gamePosToMeshPos(gameData.botPlayer.position))}.",
                "Ask me where or who and I will answer.",
                "No hard evidence from me yet.",
                BuildTrustLine(roundMemory)
            };

            return choices.Where(x => !string.IsNullOrWhiteSpace(x)).OrderBy(_ => random.Next()).FirstOrDefault();
        }

        private string GetWitnessLine(RoundMemory roundMemory, SkeldMap map)
        {
            foreach (var myEvent in roundMemory.witnessedEvents)
            {
                if (myEvent is DeathEvent deathEvent)
                {
                    return $"I saw {deathEvent.killer.color} kill {deathEvent.victim.color} in {map.getLocationRegionName(map.gamePosToMeshPos(deathEvent.position))}.";
                }

                if (myEvent is VentEvent ventEvent)
                {
                    return $"{ventEvent.venter.color} vented in {map.getLocationRegionName(map.gamePosToMeshPos(ventEvent.position))}.";
                }
            }

            return null;
        }

        private string BuildTrustLine(RoundMemory roundMemory)
        {
            var trusted = roundMemory.getTrustedPlayers().Take(3).Select(x => x.color).ToList();
            return trusted.Count > 0 ? $"I trust {string.Join(", ", trusted)} based on sightings." : null;
        }

        private bool IsProbablyMine(string message, PlayerInformation bot)
        {
            return string.Equals(message, Normalize(lastSent), StringComparison.OrdinalIgnoreCase)
                || message.StartsWith(bot.name + ":", StringComparison.OrdinalIgnoreCase)
                || message.StartsWith(bot.color + ":", StringComparison.OrdinalIgnoreCase);
        }

        private void Say(string text)
        {
            var message = Normalize(text);
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            lastSent = message;
            Speak(message);
            chatMessenger.Send(message);
        }

        private static void Speak(string text)
        {
            using (var synth = new SpeechSynthesizer())
            using (var streamAudio = new System.IO.MemoryStream())
            {
                synth.SelectVoiceByHints(VoiceGender.Female);
                synth.SetOutputToWaveStream(streamAudio);
                synth.Speak(text);
                streamAudio.Position = 0;
                using (var soundPlayer = new SoundPlayer(streamAudio))
                {
                    soundPlayer.Play();
                }
                synth.SetOutputToNull();
            }
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var compact = string.Join(" ", text.Replace("\r", " ").Replace("\n", " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            return compact.Length <= 95 ? compact : compact.Substring(0, 95);
        }
    }
}
