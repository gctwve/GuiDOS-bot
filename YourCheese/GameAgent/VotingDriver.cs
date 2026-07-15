using HamsterCheese.AmongUsMemory;
using System;
using System.Collections.Generic;

namespace YourCheese.GameAgent
{
    class VotingVector
    {
        public Vector2 initialButton;
        public Vector2 confirmButton;

        public VotingVector(Vector2 initialButton, Vector2 confirmButton)
        {
            this.initialButton = initialButton;
            this.confirmButton = confirmButton;
        }

        public VotingVector(Vector2 pos)
        {
            this.initialButton = pos;
            this.confirmButton = pos;
        }
    }

    class VotingDriver
    {
        private const byte SkipVote = 253;

        private readonly List<VotingVector> votingButtons = new List<VotingVector>() {
            new VotingVector(new Vector2(395, 939), new Vector2(570, 937)),
            new VotingVector(new Vector2(714, 265)), new VotingVector(new Vector2(1367, 265)),
            new VotingVector(new Vector2(714, 406)), new VotingVector(new Vector2(1367, 406)),
            new VotingVector(new Vector2(714, 544)), new VotingVector(new Vector2(1367, 544)),
            new VotingVector(new Vector2(714, 686)), new VotingVector(new Vector2(1367, 686)),
            new VotingVector(new Vector2(714, 819)), new VotingVector(new Vector2(1367, 819))
        };

        public void vote(GameDataContainer gameData, RoundMemory roundMemory)
        {
            var targetPlayerId = ChooseVote(gameData, roundMemory);
            WaitUntilVotingOpen();

            var buttonIndex = 0;
            if (targetPlayerId != SkipVote)
            {
                buttonIndex = Cheese.GetMeetingVoteButtonIndex(targetPlayerId);
                if (buttonIndex < 1 || buttonIndex >= votingButtons.Count)
                {
                    buttonIndex = 0;
                }
            }

            var choice = votingButtons[buttonIndex];
            var taskInput = new TaskInput();
            taskInput.mouseClick(choice.initialButton);
            System.Threading.Thread.Sleep(500);
            taskInput.mouseClick(choice.confirmButton);
        }

        private static byte ChooseVote(GameDataContainer gameData, RoundMemory roundMemory)
        {
            if (gameData == null || roundMemory == null || gameData.botPlayer.isImposter)
            {
                return SkipVote;
            }

            foreach (var myEvent in roundMemory.witnessedEvents)
            {
                if (myEvent is DeathEvent deathEvent && deathEvent.killer.colorId != gameData.botPlayer.colorId)
                {
                    return deathEvent.killer.colorId;
                }
            }

            foreach (var myEvent in roundMemory.witnessedEvents)
            {
                if (myEvent is VentEvent ventEvent && ventEvent.venter.colorId != gameData.botPlayer.colorId)
                {
                    return ventEvent.venter.colorId;
                }
            }

            return SkipVote;
        }

        private static void WaitUntilVotingOpen()
        {
            var waited = 0;
            while (waited < 45000)
            {
                var state = Cheese.GetMeetingVoteState();
                if (state == 2)
                {
                    System.Threading.Thread.Sleep(1500);
                    return;
                }

                if (state >= 3 || state < 0)
                {
                    return;
                }

                System.Threading.Thread.Sleep(500);
                waited += 500;
            }
        }
    }
}
