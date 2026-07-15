using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HamsterCheese.AmongUsMemory;
using WindowsInput;
using WindowsInput.Native;

namespace YourCheese
{

    public class GameTask
    {
        public Vector2 position;
        public string name;
        public bool isDone = false;
        public int fakeTime;
        public int taskType;
        public int systemType;

        public GameTask(Vector2 position, string name, int fakeTime, int taskType, int systemType)
        {
            this.position = position;
            this.name = name;
            this.fakeTime = fakeTime;
            this.taskType = taskType;
            this.systemType = systemType;
        }
    }

    class TaskLocationResolver
    {
        public static Dictionary<Vector2, GameTask> taskLocations = new Dictionary<Vector2, GameTask>()
        {
            {new Vector2(151, 412), new GameTask(new Vector2(44, 290), "Manifolds", 1100, 15, 3)}, // manifolds
            {new Vector2(187, 541), new GameTask(new Vector2(77,377), "Simon Says", 6000, 4, 3)}, // reactor
            {new Vector2(251, 845), new GameTask(new Vector2(143, 596), "Lower Engine Calibration", 1100, 11, 13)}, // lower engine
            {new Vector2(342, 831), new GameTask(new Vector2(171, 582), "Lower Gas", 3300, 2, 13)}, // lower gas
            {new Vector2(256, 350), new GameTask(new Vector2(144, 237), "Upper Engine Calibration", 1100, 11, 4)}, // upper engine
            {new Vector2(334, 334), new GameTask(new Vector2(172, 227), "Upper Gas", 3300, 2, 4)}, // upper gas
            {new Vector2(367, 185), new GameTask(new Vector2(198, 130), "Power Switch in Upper Reactor", 800, 14, 3)}, // power switch upper reactor
            {new Vector2(543, 431), new GameTask(new Vector2(336, 313), "Power Switch in Security", 800, 14, 11)}, // power switch security
            {new Vector2(307, 688), new GameTask(new Vector2(175, 497), "Power Switch in Lower Engine", 800, 14, 13)}, // power switch lower engine
            {new Vector2(755, 531), new GameTask(new Vector2(483, 356), "Medscan", 10, 0, 10)}, // medbay
            {new Vector2(809, 483), new GameTask(new Vector2(503, 346), "Inspect sample in medbay", 1000, 8, 10)}, // med analyzer
            {new Vector2(638, 614), new GameTask(new Vector2(411, 443), "Download in electrical", 9500, 7, 7)}, //download electrical
            {new Vector2(688, 602), new GameTask(new Vector2(422, 441), "Power switch in electrical", 800, 14, 7)}, // electrical power switch
            {new Vector2(821, 613), new GameTask(new Vector2(523, 450), "Wires in electrical", 900, 12, 7)}, // wires
            {new Vector2(759, 613), new GameTask(new Vector2(468, 440), "Electrical distributor", 2500, 13, 7)}, // electrical distributor
            {new Vector2(943, 880), new GameTask(new Vector2(600, 624), "Gas", 5000, 2, 1)}, // gas
            {new Vector2(1076, 1006), new GameTask(new Vector2(712, 706), "Bottom Trash", 3000, 9, 1)}, // bottom trash
            {new Vector2(998, 675), new GameTask(new Vector2(644, 480), "Wires in storage", 900, 12, 1)}, // wires storage
            {new Vector2(1291, 664), new GameTask(new Vector2(875, 443), "Card swipe", 1600, 5, 6)}, // card swipe
            // Cafe targets sit too close to the lunch table with this map calibration, so skip them.
            {new Vector2(1097, 571), new GameTask(new Vector2(736, 414), "Wires in admin", 900, 12, 6)}, // admin wire
            {new Vector2(1158, 572), new GameTask(new Vector2(770, 406), "Upload", 10000, 7, 6)}, // upload admin
            {new Vector2(1199, 887), new GameTask(new Vector2(823, 642), "Download in comms", 10000, 7, 14)}, // comms download
            {new Vector2(1299, 889), new GameTask(new Vector2(873, 640), "Power switch in comms", 900, 14, 14)}, // switch in comms
            {new Vector2(1383, 870), new GameTask(new Vector2(930, 630), "Shields", 900, 1, 9)}, // shields
            {new Vector2(1491, 727), new GameTask(new Vector2(1004, 519), "Power switch in shields", 900, 14, 9)}, // shields switch
            {new Vector2(1631, 471), new GameTask(new Vector2(1124, 341), "Navigation wires", 900, 12, 5)}, // nav wires
            {new Vector2(1735, 411), new GameTask(new Vector2(1195, 295), "Navigation download", 10000, 7, 5)}, // nav download
            {new Vector2(1780, 456), new GameTask(new Vector2(1227, 328), "Rocket in nav", 3100, 3, 5)}, // chart course
            {new Vector2(1803, 516), new GameTask(new Vector2(1227, 328), "Cross in nav", 800, 21, 5)}, // steering
            {new Vector2(1313, 437), new GameTask(new Vector2(874, 302), "leaves", 3500, 18, 8)}, // leaves in o2
            {new Vector2(1240, 462), new GameTask(new Vector2(854, 315), "Trash in o2", 3500, 9, 8)}, // trash in o2
            {new Vector2(1391, 416), new GameTask(new Vector2(946, 299), "Power switch in o2", 900, 14, 8)}, // switch in o2
            {new Vector2(1415, 228), new GameTask(new Vector2(969, 173), "Asteroids", 15000, 6, 12)}, // weapons
            {new Vector2(1395, 152), new GameTask(new Vector2(955, 105), "Download in weapons", 10000, 7, 12)}, // download in weapons
            {new Vector2(1513, 230), new GameTask(new Vector2(1025, 152), "Power switch in weapons", 900, 14, 12)} // switch in weapons
        };
    }

    public class TaskManager
    {
        public static InputSimulator inputSimulator = new InputSimulator();

        private struct LiveTaskInfo
        {
            public int TaskType;
            public int StartAt;
            public int TargetSystem;
        }

        public List<GameTask> getTaskPositions()
        {
            var liveTasks = ReadLiveTasks();
            if (liveTasks.Count == 0)
            {
                Console.WriteLine("No live task list found; using safe non-cafe fallback targets.");
                return GetSafeFallbackTargets();
            }

            var filtered = TaskLocationResolver.taskLocations.Values
                .Where(task => MatchesLiveTask(task, liveTasks))
                .Select(CloneTask)
                .ToList();

            Console.WriteLine("Live task targets: " + string.Join(", ", filtered.Select(x => x.name).Distinct()));
            return filtered.Count > 0 ? filtered : GetSafeFallbackTargets();
        }

        private static bool MatchesLiveTask(GameTask task, List<LiveTaskInfo> liveTasks)
        {
            foreach (var liveTask in liveTasks)
            {
                if (liveTask.TaskType != task.taskType)
                {
                    continue;
                }

                if (task.taskType == 2)
                {
                    return true;
                }

                if (task.taskType == 12)
                {
                    return liveTask.StartAt == 0 || task.systemType == liveTask.StartAt;
                }

                if (task.taskType == 7)
                {
                    return task.systemType == liveTask.StartAt || task.systemType == 6;
                }

                if (task.taskType == 14)
                {
                    return task.systemType == 7 || task.systemType == liveTask.TargetSystem;
                }

                return liveTask.StartAt == 0 || task.systemType == liveTask.StartAt;
            }

            return false;
        }

        private static List<LiveTaskInfo> ReadLiveTasks()
        {
            var result = new List<LiveTaskInfo>();
            try
            {
                var localPlayer = Cheese.GetLocalPlayer();
                if (localPlayer == IntPtr.Zero)
                {
                    return result;
                }

                var taskList = Cheese.ReadPointer(localPlayer.Sum(0xAC));
                var items = taskList == IntPtr.Zero ? IntPtr.Zero : Cheese.ReadPointer(taskList.Sum(0x8));
                var count = taskList == IntPtr.Zero ? 0 : Cheese.mem.ReadInt(taskList.Sum(0xC).GetAddress());
                if (items == IntPtr.Zero || count <= 0 || count > 32)
                {
                    return result;
                }

                for (var i = 0; i < count; i++)
                {
                    var taskPtr = Cheese.ReadPointer(items.Sum(0x10 + (i * IntPtr.Size)));
                    if (taskPtr == IntPtr.Zero)
                    {
                        continue;
                    }

                    var taskType = Cheese.mem.ReadInt(taskPtr.Sum(0x20).GetAddress());
                    if (taskType >= 0 && taskType < 82)
                    {
                        result.Add(new LiveTaskInfo()
                        {
                            TaskType = taskType,
                            StartAt = Cheese.mem.ReadInt(taskPtr.Sum(0x1C).GetAddress()),
                            TargetSystem = taskType == 14 ? Cheese.mem.ReadInt(taskPtr.Sum(0x60).GetAddress()) : -1
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not read live task list: " + e.Message);
            }

            return result;
        }

        private static GameTask CloneTask(GameTask task)
        {
            return new GameTask(task.position, task.name, task.fakeTime, task.taskType, task.systemType)
            {
                isDone = task.isDone
            };
        }

        private static List<GameTask> GetSafeFallbackTargets()
        {
            return TaskLocationResolver.taskLocations.Values
                .Where(task => task.systemType != 2)
                .Where(task => task.name == "Card swipe" || task.name == "Upload" || task.name == "Electrical distributor")
                .Select(CloneTask)
                .ToList();
        }
    }
}
