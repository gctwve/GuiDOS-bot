namespace YourCheese.GameAgent.Strategies
{
    class NoisemakerBodyStrategy : Strategy
    {
        private double confidence = 1;
        private SkeldMap map;
        private Navigator navigator;
        private readonly NoisemakerDeathEvent noisemakerDeath;

        public NoisemakerBodyStrategy(Navigator navigator, SkeldMap map, NoisemakerDeathEvent noisemakerDeath)
        {
            this.navigator = navigator;
            this.map = map;
            this.noisemakerDeath = noisemakerDeath;
        }

        public void run()
        {
            if (navigator.setDestination(map.gamePosToMeshPos(noisemakerDeath.position)))
            {
                new TaskInput().pressR();
            }
            confidence = 0;
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
            return "Going to " + noisemakerDeath.victim.color + "'s noisemaker alert";
        }

        public string getMode()
        {
            return "Noisemaker body";
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
