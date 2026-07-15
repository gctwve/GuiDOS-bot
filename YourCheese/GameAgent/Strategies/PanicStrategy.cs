using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourCheese.GameAgent.Strategies
{
    class PanicStrategy : Strategy
    {
        Navigator navigator;
        Event cause;

        public PanicStrategy(Navigator navigator, Event cause)
        {
            this.navigator = navigator;
            this.cause = cause;
        }

        public void run()
        {
            var safePoint = new Vector2(770, 406);
            while (Vector2.Distance(navigator.botPos, safePoint) > 10)
            {
                navigator.setDestination(safePoint);
                System.Threading.Thread.Sleep(100);
            }
        }

        public double getConfidence()
        {
            return 1;
        }

        public void setConfidence(double t)
        {
            return;
        }

        public String getAsString()
        {
            return "I entered panic mode";
        }

        public String getMode()
        {
            return "PANIK";
        }

        public void abort()
        {
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
