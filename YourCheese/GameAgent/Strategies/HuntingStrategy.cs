using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourCheese.GameAgent.Strategies
{
    class HuntingStrategy : Strategy
    {
        PlayerInformation target;
        SkeldMap map;
        Navigator navigator;
        GameDataContainer gameState;
        BehaviorDriver behaviorDriver;
        double confidence = 1;
        String mode = "Hunting";
        bool murdered = false;
        bool usedRoleAbility = false;

        public HuntingStrategy(Navigator navigator, SkeldMap map, GameDataContainer gameState, BehaviorDriver behaviorDriver)
        {
            this.navigator = navigator;
            this.map = map;
            this.gameState = gameState;
            this.behaviorDriver = behaviorDriver;
        }

        public double getConfidence() 
        {
            return confidence;
        }

        public void setConfidence(double t) 
        {
            confidence = t;
        }

        public void run() 
        {
            while (confidence > 0 && gameState.botPlayer.killTimer == 0)
            {
                acquireTarget();
                if (confidence <= 0)
                {
                    break;
                }

                while (targetStillValid())
                {
                    navigator.followPlayer(map.gamePosToMeshPos(target.position));
                    if (Vector2.Distance(navigator.botPos, map.gamePosToMeshPos(target.position)) < 20 && MemorySaysKillIsAvailable())
                    {
                        var input = new TaskInput();
                        if (!PrepareRoleKill(input))
                        {
                            confidence = 0;
                            break;
                        }
                        input.pressQ();
                        murdered = true;
                    }
                }
            }
            if (murdered)
            {
                Vector2 escapePoint = Vector2.Zero;
                foreach (var point in map.places)
                {
                    Vector2 pointVector = new Vector2(point.x, point.y);
                    List<Waypoint> escapeRoute = navigator.getWaypoints(pointVector);
                    int i = 0;
                    bool safeRoute = true;
                    foreach (var waypoint in escapeRoute)
                    {
                        double closestCrewmateDistance = gameState.getClosestCrewmateToPoint(new Vector2(waypoint.x, waypoint.y));
                        if (closestCrewmateDistance < 150 - (i * 10))
                        {
                            safeRoute = false;
                            break;
                        }
                    }
                    if (safeRoute)
                    {
                        escapePoint = pointVector;
                        break;
                    }
                }
                if (escapePoint.IsGarbage() && Vector2.Distance(map.gamePosToMeshPos(gameState.getTheOtherImposter().position), navigator.botPos) > 50)
                {
                    // self report
                    behaviorDriver.reportedBody = target;
                    new TaskInput().pressR();
                }
                else
                {
                    // run for your life
                    mode = "Escaping murder scene";
                    navigator.setDestination(escapePoint);
                }
            }
        }

        public void update(GameDataContainer gameState)
        {
            this.gameState = gameState;
            this.target = gameState.getPlayerByColor(target.colorId);
        }

        void acquireTarget()
        {
            foreach (var player in gameState.getLivingCrewmatesThatArentBot())
            {
                PlayerInformation closestCrewmate = gameState.getClosestPlayer(player.colorId);
                float distance = Vector2.Distance(map.gamePosToMeshPos(closestCrewmate.position), map.gamePosToMeshPos(player.position));
                if (distance > 80 && !player.isDead && !HasWitnesses(player))
                {
                    target = player;
                    return;
                }
            }
            setConfidence(0);
        }

        bool targetStillValid()
        {
            PlayerInformation closestCrewmate = gameState.getClosestPlayer(target.colorId);
            float distance = Vector2.Distance(map.gamePosToMeshPos(closestCrewmate.position), map.gamePosToMeshPos(target.position));
            return (distance > 60 && !target.isDead && !HasWitnesses(target));
        }

        private bool HasWitnesses(PlayerInformation victim)
        {
            foreach (var player in gameState.getLivingCrewmatesThatArentBot())
            {
                if (player.colorId == victim.colorId)
                {
                    continue;
                }

                var distance = Vector2.Distance(map.gamePosToMeshPos(player.position), map.gamePosToMeshPos(victim.position));
                if (distance < 90)
                {
                    return true;
                }
            }

            return gameState.getVisiblePlayers()
                .Any(player => player.colorId != victim.colorId && !player.isDead && !player.isImposter);
        }

        private bool PrepareRoleKill(TaskInput input)
        {
            if (!gameState.botPlayer.roleMemory.HasRoleMemory)
            {
                return gameState.botPlayer.roleType == BotRoleType.Impostor || gameState.botPlayer.roleType == BotRoleType.Viper;
            }

            if (usedRoleAbility)
            {
                return true;
            }

            if (gameState.botPlayer.roleType == BotRoleType.Phantom)
            {
                if (gameState.botPlayer.roleMemory.isInvisible || gameState.botPlayer.roleMemory.isFading)
                {
                    usedRoleAbility = true;
                    return true;
                }

                if (!gameState.botPlayer.roleMemory.AbilityReady)
                {
                    return false;
                }

                mode = "Phantom vanish from memory-ready state";
                input.pressF();
                usedRoleAbility = true;
                System.Threading.Thread.Sleep(500);
                return true;
            }

            if (gameState.botPlayer.roleType == BotRoleType.Shapeshifter)
            {
                if (gameState.botPlayer.roleMemory.abilityDuration > 1)
                {
                    usedRoleAbility = true;
                    return true;
                }

                if (!gameState.botPlayer.roleMemory.AbilityReady || gameState.getVisiblePlayers().Count > 0)
                {
                    return false;
                }

                mode = "Shapeshifter shift from memory-ready state";
                input.pressF();
                System.Threading.Thread.Sleep(300);
                SelectMemoryBackedShapeshiftTarget(input);
                usedRoleAbility = true;
                System.Threading.Thread.Sleep(600);
                return true;
            }

            return true;
        }

        private bool MemorySaysKillIsAvailable()
        {
            return gameState.botPlayer.killTimer <= 0.05f
                && !gameState.botPlayer.inVent
                && gameState.botPlayer.roleMemory.closestUsablePtr != IntPtr.Zero;
        }

        private void SelectMemoryBackedShapeshiftTarget(TaskInput input)
        {
            var safestTarget = gameState.getLivingPlayersThatArentBot()
                .Where(player => !player.isImposter && player.colorId != target.colorId)
                .OrderByDescending(player => Vector2.Distance(map.gamePosToMeshPos(player.position), navigator.botPos))
                .FirstOrDefault();

            if (!safestTarget.position.IsGarbage())
            {
                var index = Math.Max(0, gameState.getLivingPlayersThatArentBot().FindIndex(player => player.colorId == safestTarget.colorId));
                input.mouseClick(new Vector2(530 + (index % 3) * 320, 320 + (index / 3) * 220));
                return;
            }

            input.mouseClick(new Vector2(960, 540));
        }

        public String getAsString() 
        {
            return gameState.botPlayer.isImposter ? "Playing " + gameState.botPlayer.RoleName + " carefully" : "Looking for bodies";
        }


        public void abort()
        {
            confidence = 0;
            navigator.abort();
        }

        public String getMode()
        {
            return mode;
        }

        public void setNavigator(Navigator navigator)
        {
            this.navigator = navigator;
        }
    }
}
