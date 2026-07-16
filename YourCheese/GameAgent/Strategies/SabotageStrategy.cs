using System;
using System.Collections.Generic;
using System.Linq;

namespace YourCheese.GameAgent.Strategies
{
    class SabotageStrategy : Strategy
    {
        private readonly SkeldMap map;
        private Navigator navigator;
        private readonly ActiveSabotage sabotage;
        private double confidence = 1;

        private readonly List<Vector2> criticalFixPoints = new List<Vector2>()
        {
            new Vector2(77, 377),   // Reactor
            new Vector2(854, 315),  // O2
            new Vector2(770, 406)   // Admin
        };

        public SabotageStrategy(Navigator navigator, SkeldMap map, ActiveSabotage sabotage)
        {
            this.navigator = navigator;
            this.map = map;
            this.sabotage = sabotage;
        }

        public void run()
        {
            var target = sabotage == ActiveSabotage.Lights
                ? new Vector2(468, 440)
                : criticalFixPoints.OrderBy(point => Vector2.Distance(navigator.botPos, point)).First();

            Console.WriteLine("Prioritizing sabotage: " + sabotage);
            navigator.setDestination(target);

            var input = new TaskInput();
            input.pressE();
            System.Threading.Thread.Sleep(700);

            if (sabotage == ActiveSabotage.Lights)
            {
                FixLights(input);
            }
            else
            {
                TryCriticalFix(input);
            }

            confidence = 0;
        }

        private void FixLights(TaskInput input)
        {
            var switches = new[]
            {
                new Vector2(685, 615),
                new Vector2(825, 615),
                new Vector2(960, 615),
                new Vector2(1095, 615),
                new Vector2(1235, 615)
            };

            foreach (var sw in switches)
            {
                input.mouseClick(sw);
                System.Threading.Thread.Sleep(120);
            }
        }

        private void TryCriticalFix(TaskInput input)
        {
            input.mouseClick(new Vector2(960, 540));
            System.Threading.Thread.Sleep(200);
            input.mouseClick(new Vector2(960, 640));
            System.Threading.Thread.Sleep(200);
            input.closeTask();
        }

        public double getConfidence()
        {
            return confidence;
        }

        public void setConfidence(double t)
        {
            confidence = t;
        }

        public void abort()
        {
            confidence = 0;
            navigator.abort();
        }

        public void update(GameDataContainer gameDataContainer)
        {
        }

        public void setNavigator(Navigator navigator)
        {
            this.navigator = navigator;
        }

        public string getAsString()
        {
            return "I went to fix " + sabotage;
        }

        public string getMode()
        {
            return "Fixing " + sabotage;
        }
    }
}
