using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.AudioFormat;

namespace YourCheese.GameAgent.Conversation
{
    class MeetingTalker
    {

        PlayerInformation botInfo;
        GameDataContainer gameData;
        private readonly GameChatMessenger chatMessenger = new GameChatMessenger();

        public MeetingTalker(PlayerInformation botInfo, GameDataContainer gameData)
        {
            this.botInfo = botInfo;
            this.gameData = gameData;
        }

        public void tellTheMemory(RoundMemory roundMemory, PlayerInformation reportedBody, SkeldMap map)
        {
            var lines = new List<string>();
            if (!reportedBody.position.IsGarbage())
            {
                lines.Add("I found " + reportedBody.color + "'s body in " + map.getLocationRegionName(map.gamePosToMeshPos(reportedBody.position)) + ".");
            }

            foreach (var myEvent in roundMemory.witnessedEvents)
            {
                if (myEvent is DeathEvent)
                {
                    DeathEvent deathEvent = (DeathEvent)myEvent;
                    if (!botInfo.isImposter)
                    {
                        lines.Add("I watched " + deathEvent.killer.color + " murder " + deathEvent.victim.color + " in " + map.getLocationRegionName(map.gamePosToMeshPos(deathEvent.position)) + ".");
                    }
                }
                else if (myEvent is VentEvent)
                {
                    if (!botInfo.isImposter)
                    {
                        VentEvent ventEvent = (VentEvent)myEvent;
                        lines.Add(ventEvent.venter.color + " vented right in front of me in " + map.getLocationRegionName(map.gamePosToMeshPos(ventEvent.position)) + ".");
                    }
                }
            }

            var activity = BuildActivitySummary(roundMemory);
            if (!string.IsNullOrWhiteSpace(activity))
            {
                lines.Add(activity);
            }

            var roleEvidence = BuildImposterRoleEvidence();
            if (!string.IsNullOrWhiteSpace(roleEvidence))
            {
                lines.Add(roleEvidence);
            }

            if (roundMemory.getTrustedPlayers().Count > 0)
            {
                var trusted = roundMemory.getTrustedPlayers().Take(3).Select(player => player.color);
                lines.Add("I kept seeing " + string.Join(", ", trusted) + ", so they looked safe to me.");
            }

            if (lines.Count == 0)
            {
                lines.Add("I do not have hard evidence, but I can say what I was doing before the meeting.");
            }

            Say(string.Join(" ", lines));
        }

        private string BuildActivitySummary(RoundMemory roundMemory)
        {
            var completedTasks = roundMemory.completedTasks
                .Select(task => task.name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .Take(4)
                .ToList();

            var modes = roundMemory.strategies
                .Select(strategy => strategy.getMode())
                .Where(mode => !string.IsNullOrWhiteSpace(mode))
                .Distinct()
                .Take(3)
                .ToList();

            if (completedTasks.Count > 0)
            {
                return "Before the meeting I was doing " + string.Join(", ", completedTasks) + ".";
            }

            if (modes.Count > 0)
            {
                return "Before the meeting I was " + string.Join(", then ", modes).ToLowerInvariant() + ".";
            }

            return null;
        }

        private string BuildImposterRoleEvidence()
        {
            if (!botInfo.isImposter)
            {
                return null;
            }

            switch (botInfo.roleType)
            {
                case BotRoleType.Shapeshifter:
                    return "Role read: colors are weak evidence this round because a shapeshifter can frame someone.";
                case BotRoleType.Phantom:
                    return "Role read: if anyone vanished near a kill, that matters more than normal pathing.";
                case BotRoleType.Viper:
                    return "Role read: this looks like normal impostor pathing, so check who had isolated access.";
                case BotRoleType.Impostor:
                    return "Role read: no special role clue from me, just pathing and isolation.";
                default:
                    return null;
            }
        }

        public void Say(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            SpeakTheText(text);
            chatMessenger.Send(text);
        }

        public void SpeakTheText(string text)
        {
            // Initialize a new instance of the speech synthesizer.  
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            using (MemoryStream streamAudio = new MemoryStream())
            {
                // Create a SoundPlayer instance to play the output audio file.  
                System.Media.SoundPlayer m_SoundPlayer = new System.Media.SoundPlayer();
                // Set voice to male
                synth.SelectVoiceByHints(VoiceGender.Female);
                // Configure the synthesizer to output to an audio stream.  
                synth.SetOutputToWaveStream(streamAudio);

                // Speak a phrase.  
                synth.Speak(text);
                streamAudio.Position = 0;
                m_SoundPlayer.Stream = streamAudio;
                m_SoundPlayer.Play();

                // Set the synthesizer output to null to release the stream.   
                synth.SetOutputToNull();

                // Insert code to persist or process the stream contents here.  
            }
        }

        public void VoiceIntoFile(String text, String file)
        {
            // Initialize a new instance of the SpeechSynthesizer.  
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                synth.SelectVoiceByHints(VoiceGender.Female);
                // Configure the audio output.   
                synth.SetOutputToWaveFile($@"C:\Studio\{file}",
                  new SpeechAudioFormatInfo(32000, AudioBitsPerSample.Sixteen, AudioChannel.Mono));

                // Create a SoundPlayer instance to play output audio file.  
                System.Media.SoundPlayer m_SoundPlayer =
                  new System.Media.SoundPlayer($@"C:\Studio\{file}");

                // Build a prompt.  
                PromptBuilder builder = new PromptBuilder();
                builder.AppendText(text);

                // Speak the prompt.  
                synth.Speak(builder);
                m_SoundPlayer.Play();
            }
        }
    
    }
}
