using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourCheese.GameAgent.Strategies
{
    class TaskDoingStrategy : Strategy
    {
        SkeldMap map;
        Navigator navigator;
        double confidence = 1;
        bool finishAllTasks;
        static readonly Random random = new Random();
        public List<GameTask> taskPositions;
        public List<GameTask> doneTasks = new List<GameTask>();
        private readonly List<GameTask> rememberedDoneTasks;
        private readonly List<GameTask> initialTaskPositions;
        TaskIdentifier currentTaskIdentifier;

        public TaskDoingStrategy(Navigator navigator, SkeldMap map, bool finishAllTasks = false, List<GameTask> rememberedDoneTasks = null, List<GameTask> initialTaskPositions = null)
        {
            this.navigator = navigator;
            this.map = map;
            this.finishAllTasks = finishAllTasks;
            this.rememberedDoneTasks = rememberedDoneTasks ?? new List<GameTask>();
            this.initialTaskPositions = initialTaskPositions;
        }

        public void run()
        {
            taskPositions = initialTaskPositions ?? new TaskManager().getTaskPositions(false);
            taskPositions.RemoveAll(task => rememberedDoneTasks.Contains(task) || doneTasks.Contains(task));

            while (taskPositions.Count > 0 && (finishAllTasks || random.NextDouble() < confidence))
            {
                doTask();
                taskPositions.RemoveAll(task => rememberedDoneTasks.Contains(task) || doneTasks.Contains(task));
            }
        }

        private void doTask()
        {
            System.Threading.Thread.Sleep(500);
            var task = getClosestTask(taskPositions);
            if (task != null)
            {
                //Console.WriteLine($"{task.position.x},{task.position.y}");
                if (!navigator.setDestination(task.position))
                {
                    taskPositions.Remove(task);
                    confidence -= 0.15;
                    return;
                }

                currentTaskIdentifier = new TaskIdentifier();
                var attempted = currentTaskIdentifier.doTask(task);
                System.Threading.Thread.Sleep(500);
                if (attempted && !doneTasks.Contains(task))
                {
                    doneTasks.Add(task);
                }
                taskPositions.Remove(task);
            }
            confidence -= 0.1;
        }

        private GameTask getClosestTask(List<GameTask> taskPositions)
        {
            float distance = 99999;
            GameTask closestTask = null;
            foreach (var task in taskPositions)
            {
                float temp = Vector2.Distance(navigator.botPos, task.position);
                if (temp < distance)
                {
                    distance = temp;
                    closestTask = task;
                }
            }
            return closestTask;
        }

        public double getConfidence()
        {
            return confidence;
        }

        public void setConfidence(double t)
        {
            this.confidence = t;
        }

        public String getAsString()
        {
            return $"Doing my tasks";
        }

        public String getMode()
        {
            return finishAllTasks ? $"Finishing tasks" : $"Doing tasks";
        }

        public void abort()
        {
            confidence = 0;
            currentTaskIdentifier?.abort();
            navigator.abort();

        }

        public void update(GameDataContainer gameDataContainer)
        {

        }

        public void setNavigator(Navigator navigator)
        {
            this.navigator = navigator;
        }
    }
}
