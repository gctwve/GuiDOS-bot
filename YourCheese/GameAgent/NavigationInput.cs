using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput.Native;

namespace YourCheese
{
    public class NavigationError : Exception
    {
    }

    public class NavigationInput
    {
        public List<Polygon> polygons;
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 previousVelocity;
        public int iterationsLost = 0;
        private float previousDistanceToWaypoint = float.MaxValue;
        private DateTime lastProgressTime = DateTime.UtcNow;
        private readonly HashSet<VirtualKeyCode> heldKeys = new HashSet<VirtualKeyCode>();

        public bool abort = false;

        public void getToPos(Waypoint waypoint, Waypoint nextWaypoint)
        {
            NativeInput.FocusAmongUs();
            lastProgressTime = DateTime.UtcNow;
            previousDistanceToWaypoint = float.MaxValue;
            var startedAt = DateTime.UtcNow;

            while (!waypoint.isReached(position, nextWaypoint == null) && !abort)
            {
                var currentDistanceToWaypoint = Vector2.Distance(position, new Vector2(waypoint.x, waypoint.y));
                if (nextWaypoint != null && PolyPathfinder.InLineOfSight(polygons, new Vertex(position), new Vertex(nextWaypoint)))
                {
                    break;
                }

                Console.WriteLine($"Current pos: {position.x}, {position.y}");
                Console.WriteLine($"Current destination: {waypoint.x}, {waypoint.y}");

                float distance = nextWaypoint == null ? 2 : 8;
                if (PolyPathfinder.InLineOfSight(polygons, new Vertex(position), new Vertex(waypoint)))
                {
                    if (!(Math.Abs(position.y - waypoint.y) > Math.Abs(position.x - waypoint.x) + 70))
                    {
                        getToX(waypoint.x, distance);
                    }

                    if (!(Math.Abs(position.x - waypoint.x) > Math.Abs(position.y - waypoint.y) + 70))
                    {
                        getToY(waypoint.y, distance);
                    }
                }
                else
                {
                    iterationsLost += 1;
                    getToX(waypoint.x, 2);
                    getToY(waypoint.y, 2);
                }

                if (previousDistanceToWaypoint - currentDistanceToWaypoint > 0.2f)
                {
                    iterationsLost = 0;
                    lastProgressTime = DateTime.UtcNow;
                }
                else
                {
                    iterationsLost += 1;
                }
                previousDistanceToWaypoint = currentDistanceToWaypoint;

                if (iterationsLost >= 80
                    || (DateTime.UtcNow - lastProgressTime).TotalMilliseconds > 2500
                    || (DateTime.UtcNow - startedAt).TotalMilliseconds > 14000)
                {
                    releaseInput();
                    throw new NavigationError();
                }

                System.Threading.Thread.Sleep(35);
            }

            iterationsLost = 0;
            previousDistanceToWaypoint = float.MaxValue;
            releaseInput();
        }

        public void releaseInput()
        {
            foreach (var key in heldKeys.ToList())
            {
                releaseKey(key);
            }
        }

        public void getToX(float x, float margin)
        {
            if (Math.Abs(position.x - x) > margin)
            {
                if (position.x > x)
                {
                    releaseKey(VirtualKeyCode.VK_D);
                    holdKey(VirtualKeyCode.VK_A);
                }
                else
                {
                    releaseKey(VirtualKeyCode.VK_A);
                    holdKey(VirtualKeyCode.VK_D);
                }
            }
            else
            {
                releaseKey(VirtualKeyCode.VK_A);
                releaseKey(VirtualKeyCode.VK_D);
            }
        }

        public void getToY(float y, float margin)
        {
            if (Math.Abs(position.y - y) > margin)
            {
                if (position.y > y)
                {
                    releaseKey(VirtualKeyCode.VK_S);
                    holdKey(VirtualKeyCode.VK_W);
                }
                else
                {
                    releaseKey(VirtualKeyCode.VK_W);
                    holdKey(VirtualKeyCode.VK_S);
                }
            }
            else
            {
                releaseKey(VirtualKeyCode.VK_W);
                releaseKey(VirtualKeyCode.VK_S);
            }
        }

        private void holdKey(VirtualKeyCode key)
        {
            if (heldKeys.Contains(key))
            {
                return;
            }

            NativeInput.KeyDown(key);
            heldKeys.Add(key);
        }

        private void releaseKey(VirtualKeyCode key)
        {
            if (!heldKeys.Contains(key))
            {
                return;
            }

            NativeInput.KeyUp(key);
            heldKeys.Remove(key);
        }

        public void updatePosition(Vector2 position)
        {
            previousVelocity = velocity;
            velocity = new Vector2((position.x - this.position.x) / Program.MEMORY_POLLING_PERIOD, (position.y - this.position.y) / Program.MEMORY_POLLING_PERIOD);
            this.position = position;
        }
    }
}
