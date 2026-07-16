using System;
using System.Collections.Generic;

namespace YourCheese.GameAgent.Strategies
{
    class WanderStrategy : Strategy
    {
        private double confidence = 0.8;
        private static readonly Random random = new Random();
        private SkeldMap map;
        private Navigator navigator;
        private readonly List<Vertex> points = new List<Vertex>();

        public WanderStrategy(Navigator navigator, SkeldMap map)
        {
            this.navigator = navigator;
            this.map = map;
        }

        public void run()
        {
            points.AddRange(map.places);
            while (points.Count > 0 && random.NextDouble() < confidence)
            {
                var nextPoint = points[random.Next(points.Count)];
                navigator.setDestination(new Vector2(nextPoint.x, nextPoint.y));
                points.Remove(nextPoint);
                confidence -= 0.08;
            }
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

        public string getAsString()
        {
            return "Wandering because my tasks are done";
        }

        public string getMode()
        {
            return "Wandering";
        }

        public void setNavigator(Navigator navigator)
        {
            this.navigator = navigator;
        }

        public void update(GameDataContainer gameDataContainer)
        {
        }
    }
}
