
using HamsterCheese.AmongUsMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YourCheese.GameAgent.Forms;
using System.Windows.Forms;
using YourCheese.GameAgent.Conversation;
using YourCheese.GameAgent;

namespace YourCheese
{
    class Program
    {
        static int tableWidth = 170;
        public static int MEMORY_POLLING_PERIOD = 90;

        static SkeldMap skeld;
        static BehaviorDriver behaviorDriver;
        static BotStatus botStatusForm;
        static EventGenerator eventGenerator;
        static List<PlayerData> playerDatas = new List<PlayerData>(); 
        static Task behaviorLoopTask = null;
        static readonly object behaviorLoopLock = new object();
        static bool wasInActiveRound = false;

        static void StartBehaviorLoop()
        {
            lock (behaviorLoopLock)
            {
                if (behaviorLoopTask != null && !behaviorLoopTask.IsCompleted)
                {
                    return;
                }

                behaviorLoopTask = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        behaviorDriver.run();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Behavior loop stopped: " + e);
                    }
                });
            }
        }

        static void RefreshPlayerDatas()
        {
            foreach (var player in playerDatas)
                player.StopObserveState();

            playerDatas = HamsterCheese.AmongUsMemory.Cheese.GetAllPlayers();

            foreach (var player in playerDatas)
            {
                player.onDie += (pos, colorId) => {
                    Console.WriteLine("OnPlayerDied! Color ID :" + colorId);
                };
                player.StartObserveState();
            }
        }

        static void UpdateCheat()
        {
            /*while (true)
            {
                Console.Clear();
                AmongUsClient resultInst = HamsterCheese.AmongUsMemory.Cheese.getAmongUsClient();
                //Console.WriteLine(resultInst.timer + " | " + resultInst.IsGamePublic + " | " + resultInst.GameState + " | " + resultInst.SpawnRadius + " | " + resultInst.mode);
                System.Threading.Thread.Sleep(2000);
            }*/
            
            while (true)
            {
                CheckHotkeys();
                if (playerDatas.Count == 0)
                {
                    RefreshPlayerDatas();
                }

                Console.Clear();
                ShipStatus shipStatus = HamsterCheese.AmongUsMemory.Cheese.shipStatus;
                AmongUsClient client = HamsterCheese.AmongUsMemory.Cheese.getAmongUsClient();
                //PrintRow($"Timer: {shipStatus.Timer}", $"EmergencyCooldown: {shipStatus.EmergencyCooldown}", $"Type: {shipStatus.Type}");

                GameDataContainer gameData = new GameDataContainer();
                gameData.emergencyCooldown = shipStatus.EmergencyCooldown;
                gameData.gameState = (int)client.GameState;
                gameData.activeSabotage = ActiveSabotage.None;
                bool meetingHudPresent = HamsterCheese.AmongUsMemory.Cheese.IsInMeeting();
                bool localPlayerFound = false;
                List<PlayerInformation> players = new List<PlayerInformation>();
                
                Console.WriteLine("Test Read Player Datas..");
                PrintRow("Name", "Position", "Color", "isDead", "Emergencies", "inVent", "isImposter", "killTimer");
                PrintLine();

                foreach (var data in playerDatas)
                {
                    var playerInfo = data.PlayerInfo;
                    if (!playerInfo.HasValue)
                    {
                        continue;
                    }

                    var Name = HamsterCheese.AmongUsMemory.Utils.ReadString(playerInfo.Value.PlayerName);
                    if (data.IsLocalPlayer)
                    {
                        localPlayerFound = true;
                        gameData.botPlayer = new PlayerInformation(data.Position, Name, playerInfo.Value.ColorId, playerInfo.Value.IsDead, data.remainingEmergencies(), data.inVent(), playerInfo.Value.IsImpostor, data.getKillTimer(), playerInfo.Value.RoleType, playerInfo.Value.PlayerId, ReadRoleMemory(data, playerInfo.Value));
                        LightSource ls = data.LightSource;
                        gameData.lightRadius = ls.LightRadius;
                    }
                    else
                    {
                        PlayerInformation player = new PlayerInformation(data.Position, Name, playerInfo.Value.ColorId, playerInfo.Value.IsDead, data.remainingEmergencies(), data.inVent(), playerInfo.Value.IsImpostor, data.getKillTimer(), playerInfo.Value.RoleType, playerInfo.Value.PlayerId, ReadRoleMemory(data, playerInfo.Value));
                        players.Add(player);
                    }

                    //PrintRow($"{(data.IsLocalPlayer == true ? "Me->" : "")}{data.PlayerControllPTROffset}", $"{Name}", $"{data.Instance.OwnerId}", $"{data.Instance.PlayerId}", $"{data.Instance.SpawnId}", $"{data.Instance.SpawnFlags}");
                }

                gameData.players = players;
                gameData.isInGame = localPlayerFound;
                gameData.isInLobby = gameData.gameState == 1;
                gameData.isInMeeting = meetingHudPresent && localPlayerFound && gameData.gameState == 2;
                if (localPlayerFound && gameData.isInGame && !gameData.isInMeeting)
                {
                    if (gameData.lightRadius > 0 && shipStatus.MinLightRadius > 0 && gameData.lightRadius <= shipStatus.MinLightRadius + 0.25f)
                    {
                        gameData.activeSabotage = ActiveSabotage.Lights;
                    }
                    else if (shipStatus.Timer > 0 && shipStatus.Timer < 120)
                    {
                        gameData.activeSabotage = ActiveSabotage.Critical;
                    }
                }
                var isActiveRound = gameData.isInGame && gameData.gameState == 2;
                if (wasInActiveRound && !isActiveRound)
                {
                    behaviorDriver.ClearGameMemory();
                }
                wasInActiveRound = isActiveRound;

                Console.ForegroundColor = ConsoleColor.Green;
                PrintRow($"{gameData.botPlayer.name}", $"{gameData.botPlayer.position.x},{gameData.botPlayer.position.y}", $"{gameData.botPlayer.color}", $"{gameData.botPlayer.isDead.ToString()}", $"{gameData.botPlayer.remainingEmergencies.ToString()}", $"{gameData.botPlayer.inVent.ToString()}", $"{gameData.botPlayer.isImposter.ToString()}", $"{gameData.botPlayer.killTimer.ToString()}");
                Console.ForegroundColor = ConsoleColor.White;

                foreach (var player in players)
                {
                    PrintRow($"{player.name}", $"{player.position.x},{player.position.y}", $"{player.color}", $"{player.isDead.ToString()}", $"{player.remainingEmergencies.ToString()}", $"{player.inVent.ToString()}", $"{player.isImposter.ToString()}",$"{player.killTimer.ToString()}");
                    Console.ForegroundColor = ConsoleColor.White;

                    PrintLine();
                }
                PrintRow($"Light level: {gameData.lightRadius}");
                PrintRow($"StrategyMode: {behaviorDriver.StrategyMode}", "F6 cycles mode", "F7 mouse test");
                PrintRow($"GameState: {gameData.gameState}", $"Lobby: {gameData.isInLobby}", $"InGame: {gameData.isInGame}", $"Meeting: {gameData.isInMeeting}");
                PrintRow($"OriginalPos: {gameData.botPlayer.position.x},{gameData.botPlayer.position.y}");
                Vector2 meshPos = skeld.gamePosToMeshPos(gameData.botPlayer.position);
                PrintRow($"MeshPos: {meshPos.x},{meshPos.y}");
                Vector2 gaemPos = skeld.meshPosToGamePos(meshPos);
                PrintRow($"GaemPos: {gaemPos.x},{gaemPos.y}");
                PrintRow($"Region: {skeld.getLocationRegionName(meshPos)}");
                PrintRow($"Timer: {shipStatus.Timer}");

                GameUpdate gameUpdate = eventGenerator.getGameUpdate(gameData);
                if (gameData.isInLobby)
                {
                    behaviorDriver.UpdateLobbyChat(gameData);
                }

                if (gameUpdate.gameDataContainer != null && gameData.players.Count > 0)
                {
                    behaviorDriver.update(gameUpdate);
                    if (gameData.isInGame)
                    {
                        StartBehaviorLoop();
                    }
                }
                botStatusForm.update(behaviorDriver);

                System.Threading.Thread.Sleep(MEMORY_POLLING_PERIOD);
            }
        }

        static void CheckHotkeys()
        {
            try
            {
                while (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.F6)
                    {
                        behaviorDriver.CycleStrategyMode();
                    }
                    else if (key.Key == ConsoleKey.F7)
                    {
                        RunMouseTest();
                    }
                }
            }
            catch
            {
            }
        }

        static void RunMouseTest()
        {
            Console.WriteLine("Running real mouse movement test.");
            var input = new TaskInput();
            input.mouseClick(TaskScreenLayout.Center);
            System.Threading.Thread.Sleep(250);
            input.mouseClick(TaskScreenLayout.MouseTestCard);
            System.Threading.Thread.Sleep(250);
            input.mouseClick(TaskScreenLayout.MouseTestSwipeEnd);
        }

        static RoleMemorySnapshot ReadRoleMemory(PlayerData data, PlayerInfo playerInfo)
        {
            var snapshot = new RoleMemorySnapshot()
            {
                rolePtr = playerInfo.Role
            };

            try
            {
                snapshot.closestUsablePtr = HamsterCheese.AmongUsMemory.Cheese.ReadPlayerControlPointer(data.PlayerControllPTR, HamsterCheese.AmongUsMemory.Offset.PlayerControlClosestUsable);
                snapshot.itemsInRangePtr = HamsterCheese.AmongUsMemory.Cheese.ReadPlayerControlPointer(data.PlayerControllPTR, HamsterCheese.AmongUsMemory.Offset.PlayerControlItemsInRange);
                snapshot.newItemsInRangePtr = HamsterCheese.AmongUsMemory.Cheese.ReadPlayerControlPointer(data.PlayerControllPTR, HamsterCheese.AmongUsMemory.Offset.PlayerControlNewItemsInRange);

                if (snapshot.rolePtr == IntPtr.Zero)
                {
                    return snapshot;
                }

                switch ((BotRoleType)playerInfo.RoleType)
                {
                    case BotRoleType.Phantom:
                        snapshot.abilityCooldown = HamsterCheese.AmongUsMemory.Cheese.ReadRoleFloat(snapshot.rolePtr, HamsterCheese.AmongUsMemory.Offset.PhantomCooldownSecondsRemaining);
                        snapshot.abilityDuration = HamsterCheese.AmongUsMemory.Cheese.ReadRoleFloat(snapshot.rolePtr, HamsterCheese.AmongUsMemory.Offset.PhantomDurationSecondsRemaining);
                        snapshot.isInvisible = HamsterCheese.AmongUsMemory.Cheese.ReadRoleBool(snapshot.rolePtr, HamsterCheese.AmongUsMemory.Offset.PhantomIsInvisible);
                        snapshot.isFading = HamsterCheese.AmongUsMemory.Cheese.ReadRoleBool(snapshot.rolePtr, HamsterCheese.AmongUsMemory.Offset.PhantomIsFading);
                        snapshot.serverApproved = HamsterCheese.AmongUsMemory.Cheese.ReadRoleBool(snapshot.rolePtr, HamsterCheese.AmongUsMemory.Offset.PhantomServerApproved);
                        break;
                    case BotRoleType.Shapeshifter:
                        snapshot.abilityCooldown = HamsterCheese.AmongUsMemory.Cheese.ReadRoleFloat(snapshot.rolePtr, HamsterCheese.AmongUsMemory.Offset.ShapeshifterCooldownSecondsRemaining);
                        snapshot.abilityDuration = HamsterCheese.AmongUsMemory.Cheese.ReadRoleFloat(snapshot.rolePtr, HamsterCheese.AmongUsMemory.Offset.ShapeshifterDurationSecondsRemaining);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not read role memory: " + e.Message);
            }

            return snapshot;
        }

        [STAThread]
        static void Main(string[] args)
        {
            skeld = new SkeldMap();
            behaviorDriver = new BehaviorDriver(skeld);
            eventGenerator = new EventGenerator();
            Application.EnableVisualStyles();
            botStatusForm = new BotStatus();
            Task.Run(() => Application.Run(botStatusForm));
            System.Threading.Thread.Sleep(3500);
            // Memory Init
            if (HamsterCheese.AmongUsMemory.Cheese.Init())
            { 
                // Update Player Data When Every Game
                HamsterCheese.AmongUsMemory.Cheese.ObserveShipStatus((x) =>
                {
                    RefreshPlayerDatas();
                });

                RefreshPlayerDatas();

                // Cheat Logic
                CancellationTokenSource cts = new CancellationTokenSource();
                Task.Factory.StartNew(
                    UpdateCheat
                , cts.Token);
                
                //Task.Factory.StartNew(() => behaviorDriver.run());
            }

            System.Threading.Thread.Sleep(10000000);
        }

        static void PrintLine()
        {
            Console.WriteLine(new string('-', tableWidth));
        }

        static void PrintRow(params string[] columns)
        {
            int width = (tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (string column in columns)
            {
                row += AlignCentre(column, width) + "|";
            }

            Console.WriteLine(row);

            
        }

        static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
            }
        } 
    }
}
