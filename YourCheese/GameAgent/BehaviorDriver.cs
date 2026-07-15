using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourCheese.GameAgent;
using YourCheese.GameAgent.Conversation;
using YourCheese.GameAgent.Strategies;

namespace YourCheese
{
    public enum BotStrategyMode
    {
        FinishTasks,
        Balanced,
        Follow,
        SearchBodies
    }

    public class BehaviorDriver
    {
        SkeldMap map;
        public PlayerInformation botInfo;
        Navigator navigator;
        public Strategy currentStrategy;
        Strategy queuedStrategy;
        RoundMemory roundMemory;
        GameDataContainer gameDataContainer;
        List<PlayerInformation> visiblePlayers;
        MeetingChatDirector meetingChatDirector = new MeetingChatDirector();
        LobbyChatDirector lobbyChatDirector = new LobbyChatDirector();
        public bool inEmergencyMeeting = false;
        bool talked = false;
        static readonly Random random = new Random();
        public BotStrategyMode StrategyMode { get; private set; } = BotStrategyMode.FinishTasks;

        public PlayerInformation reportedBody = PlayerInformation.Zero;
        PlayerInformation imposterPartner;
        int remainingTasks = 10;

        public BehaviorDriver(SkeldMap map)
        {
            this.map = map;
            this.navigator = new Navigator(map);
            this.roundMemory = new RoundMemory();
        }

        public void run()
        {
            while (gameDataContainer == null || !gameDataContainer.isInGame)
            {
                System.Threading.Thread.Sleep(100);
            }
            while (gameDataContainer.players.Count == 0)
            {
                System.Threading.Thread.Sleep(100);
            }
            System.Threading.Thread.Sleep(500);
            inEmergencyMeeting = false;
            talked = false;
            roundMemory.generatePlayerSightings(gameDataContainer.players);
            while (true)
            {
                while (gameDataContainer == null || !gameDataContainer.isInGame)
                {
                    navigator.stop();
                    System.Threading.Thread.Sleep(100);
                }

                while (!inEmergencyMeeting && !botInfo.isDead)
                {
                    talked = false;
                    currentStrategy = selectStrategy();
                    if (currentStrategy == null)
                    {
                        System.Threading.Thread.Sleep(100);
                        continue;
                    }

                    currentStrategy.run();
                    if (currentStrategy == null)
                    {
                        continue;
                    }

                    if (!inEmergencyMeeting && gameDataContainer != null && gameDataContainer.isInGame)
                    {
                        roundMemory.addNewStrategy(currentStrategy);
                    }

                    if (currentStrategy is TaskDoingStrategy)
                    {
                        TaskDoingStrategy taskDoingStrategy = (TaskDoingStrategy)currentStrategy;
                        roundMemory.addTasks(taskDoingStrategy.doneTasks);
                        remainingTasks = taskDoingStrategy.taskPositions.Count;
                    }
                    else if (currentStrategy is TaskFakingStrategy)
                    {
                        TaskFakingStrategy taskDoingStrategy = (TaskFakingStrategy)currentStrategy;
                        roundMemory.addTasks(taskDoingStrategy.doneTasks);
                        remainingTasks = taskDoingStrategy.taskPositions.Count;
                    }
                }
                while (!inEmergencyMeeting && botInfo.isDead && !botInfo.isImposter && remainingTasks > 0)
                {
                    var taskDoingStrategy = new TaskDoingStrategy(navigator, map);
                    taskDoingStrategy.run();
                    remainingTasks = taskDoingStrategy.taskPositions.Count;
                }
                while (inEmergencyMeeting && !botInfo.isDead)
                {
                    if (!talked)
                    {
                        new MeetingTalker(botInfo, gameDataContainer).tellTheMemory(roundMemory, reportedBody, map);
                        talked = true;
                        this.navigator = new Navigator(map);
                        Task.Factory.StartNew(() => new VotingDriver().vote(this.gameDataContainer, roundMemory));
                    }
                    meetingChatDirector.Update(gameDataContainer, roundMemory, reportedBody, map);
                    System.Threading.Thread.Sleep(500);
                }
                while (inEmergencyMeeting && botInfo.isDead)
                {
                    System.Threading.Thread.Sleep(500);
                    roundMemory.refresh();
                }
                while (botInfo.isDead && botInfo.isImposter)
                {
                    System.Threading.Thread.Sleep(500);
                }
                while (botInfo.isDead && remainingTasks == 0)
                {
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        public Strategy selectStrategy()
        {
            refreshNav();
            if (queuedStrategy != null)
            {
                var strat = queuedStrategy;
                queuedStrategy.setNavigator(navigator);
                queuedStrategy = null;
                return strat;
            }

            if (currentStrategy is PanicStrategy)
            {
                return currentStrategy;
            }

            if (!botInfo.isImposter)
            {
                return selectCrewmateStrategy();
            }
            else
            {
                return selectImposterStrategy();
            }
            throw new Exception();
        }

        private Strategy selectCrewmateStrategy()
        {
            switch (StrategyMode)
            {
                case BotStrategyMode.Balanced:
                    return random.Next(5) == 0 ? getFollowStrategyOrTasks() : new TaskDoingStrategy(navigator, map);
                case BotStrategyMode.Follow:
                    return getFollowStrategyOrTasks();
                case BotStrategyMode.SearchBodies:
                    return new BodySearchingStrategy(navigator, map);
                case BotStrategyMode.FinishTasks:
                default:
                    return new TaskDoingStrategy(navigator, map, true);
            }
        }

        private Strategy selectImposterStrategy()
        {
            switch (StrategyMode)
            {
                case BotStrategyMode.Follow:
                    return getFollowStrategyOrFakeTasks();
                case BotStrategyMode.Balanced:
                    return random.Next(4) == 0 ? getFollowStrategyOrFakeTasks() : new TaskFakingStrategy(navigator, map, roundMemory.completedTasks);
                case BotStrategyMode.SearchBodies:
                case BotStrategyMode.FinishTasks:
                default:
                    return new TaskFakingStrategy(navigator, map, roundMemory.completedTasks);
            }
        }

        private Strategy getFollowStrategyOrTasks()
        {
            var livingTargets = gameDataContainer.getLivingPlayersThatArentBot();
            if (livingTargets.Count == 0)
            {
                return new TaskDoingStrategy(navigator, map, true);
            }

            return new FollowingStrategy(navigator, map, livingTargets[random.Next(livingTargets.Count)]);
        }

        private Strategy getFollowStrategyOrFakeTasks()
        {
            var livingTargets = gameDataContainer.getLivingPlayersThatArentBot();
            if (livingTargets.Count == 0)
            {
                return new TaskFakingStrategy(navigator, map, roundMemory.completedTasks);
            }

            return new FollowingStrategy(navigator, map, livingTargets[random.Next(livingTargets.Count)]);
        }

        public void CycleStrategyMode()
        {
            var modes = (BotStrategyMode[])Enum.GetValues(typeof(BotStrategyMode));
            var nextIndex = (Array.IndexOf(modes, StrategyMode) + 1) % modes.Length;
            StrategyMode = modes[nextIndex];
            overrideStrategy(null);
            refreshNav();
            Console.WriteLine("Strategy mode changed to " + StrategyMode);
        }

        public void ClearGameMemory()
        {
            roundMemory.refresh();
            meetingChatDirector.ClearMemory();
            lobbyChatDirector.Reset();
            talked = false;
            reportedBody = PlayerInformation.Zero;
        }

        public void UpdateLobbyChat(GameDataContainer gameData)
        {
            lobbyChatDirector.Update(gameData);
        }

        public void updateBotInfo(PlayerInformation botInfo)
        {
            if (!botInfo.position.IsGarbage())
            {
                this.botInfo = botInfo;
                this.navigator.updateBotPos(this.map.gamePosToMeshPos(botInfo.position));
            }
        }

        private void processEvents(List<Event> events, List<PlayerInformation> nearbyPlayers)
        {
            if (!botInfo.isImposter)
            {
                foreach (var player in visiblePlayers)
                {
                    if (player.isDead)
                    {
                        reportedBody = player;
                        new TaskInput().pressR();
                    }
                }
                foreach (var myEvent in events)
                {
                    if (myEvent is VentEvent)
                    {
                        overrideStrategy(new PanicStrategy(navigator, myEvent));
                    }
                }
            }
            else
            {
                
                if (visiblePlayers.Count == 3 && visiblePlayers.Contains(gameDataContainer.getTheOtherImposter()) && gameDataContainer.getTheOtherImposter().killTimer < 3 && botInfo.killTimer < 2)
                {
                    overrideStrategy(new DoubleKillSetup(navigator, map, gameDataContainer));
                }
                if (botInfo.killTimer < 1 && !(currentStrategy is HuntingStrategy))
                {
                    overrideStrategy(new HuntingStrategy(navigator, map, gameDataContainer, this));
                }
                foreach (var myEvent in events)
                {
                    if (myEvent is DeathEvent)
                    {
                        if (currentStrategy is DoubleKillSetup)
                        {
                            new TaskInput().pressQ();
                        }
                    }
                }
            }
        }

        private void overrideStrategy(Strategy newStrategy)
        {
            if (currentStrategy != null)
            currentStrategy.abort();
            queuedStrategy = newStrategy;
        }

        private bool isInEmergencyMeeting(List<Event> events)
        {
            //if (gameDataContainer.emergencyCooldown > -15)
            foreach (var myEvent in events)
            {
                if (myEvent is EmergencyMeeting)
                {
                    return true;
                }
            }
            return false;
        }

        private void refreshNav()
        {
            this.navigator.stop();
            this.navigator = new Navigator(map);
        }

        public void update(GameUpdate gameUpdate)
        {
            gameDataContainer = gameUpdate.gameDataContainer;
            if (!gameDataContainer.isInGame)
            {
                if (currentStrategy != null)
                {
                    currentStrategy.abort();
                }

                queuedStrategy = null;
                inEmergencyMeeting = false;
                navigator.stop();
                return;
            }

            if (gameDataContainer.isInMeeting)
            {
                if (!inEmergencyMeeting)
                {
                    overrideStrategy(null);
                }

                inEmergencyMeeting = true;
                navigator.stop();
                return;
            }

            if (inEmergencyMeeting)
            {
                inEmergencyMeeting = false;
                talked = false;
                meetingChatDirector.ResetMeetingState();
                refreshNav();
            }

            updateBotInfo(gameUpdate.gameDataContainer.botPlayer);
            visiblePlayers = gameUpdate.visiblePlayers;
            roundMemory.update(gameUpdate.events, gameUpdate.visiblePlayers);
            inEmergencyMeeting = false;
            //inEmergencyMeeting = (gameDataContainer.emergencyCooldown < gameUpdate.gameDataContainer.emergencyCooldown);
            if (currentStrategy != null)
            currentStrategy.update(gameDataContainer);
            processEvents(gameUpdate.events, gameUpdate.gameDataContainer.getNearbyPlayers(botInfo.colorId));
            if (inEmergencyMeeting)
            {
                overrideStrategy(null);
            }
        }


        
    }
}
