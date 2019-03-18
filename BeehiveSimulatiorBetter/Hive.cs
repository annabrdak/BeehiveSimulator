using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeehiveSimulatorBetter
{
    [Serializable]
    public class Hive
    {
        [NonSerialized]
        public BeeMessage MessageSender;

        private const int InitialBees = 6;
        private const double InitialHoney = 3.2;
        private const double MaximumHoney = 15.0;
        private const double NectarHoneyRatio = 0.25;
        private const int MaximumBees = 8;
        private const double MinHoneyForCreatingBees = 4.0;

        public double Honey { get; private set; }
        private Dictionary<string, Point> locations;
        private int beeCount = 0;
        private World world;

        public Hive(World world, BeeMessage MessageSender)
        {
            this.world = world;
            this.MessageSender = MessageSender;
            Honey = InitialHoney;
            InitializeLocations();

            Random random = new Random();
            for (int i = 0; i < InitialBees; i++)
                AddBee(random);

            //while (beeCount < InitialBees)
            //{
            //    AddBee(random);
            //    beeCount++;
            //}                                
        }

        public Point GetLocation(string locationKey)
        {
            if (locations.Keys.Contains(locationKey))
            {
                return locations[locationKey];
            }
            else
            {
                throw new ArgumentException("Unknown location: " + locationKey);
            }
        }
        
        private void InitializeLocations()
        {
            locations = new Dictionary<string, Point>();
            locations.Add("Entrance", new Point(614,103));
            locations.Add("Nursery", new Point(99, 176));
            locations.Add("HoneyFactory", new Point(193, 88));
            locations.Add("Exit", new Point(213, 219));
        }

        public bool AddHoney(double nectar)
        {
            double honeyToAdd = nectar*NectarHoneyRatio;
            if (honeyToAdd + Honey > MaximumHoney)
                return false;
            Honey += honeyToAdd;
            return true;
        }

        public bool ConsumeHoney(double amount)
        {
            if (Honey < amount)
                return false;
            else
            {
                Honey -= amount;
                return true;
            }            
        }

        private void AddBee(Random random)
        {
            beeCount++;
            // Calculate the starting point
            int r1 = random.Next(100) - 50;
            int r2 = random.Next(100) - 50;

            // start the near the nursery
            Point startPoint = new Point(locations["Nursery"].X + r1,
            locations["Nursery"].Y + r2);
            Bee newBee = new Bee(beeCount, startPoint, world , this);

            newBee.MessageSender += this.MessageSender;

            // Once we have a system, we need to add this bee to the system 
            world.Bees.Add(newBee);
        }

        public void Go(Random random)
        {
            //let’s assume that when there’s enough honey to create bees, a new bee actually gets created 10 % of the time.
            if (Honey > MinHoneyForCreatingBees 
                && random.Next(10) == 1 
                && world.Bees.Count < MaximumBees)
                AddBee(random);                
        }
                
    }



    
}
