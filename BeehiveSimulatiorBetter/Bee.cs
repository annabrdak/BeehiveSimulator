﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeehiveSimulatorBetter
{
    [Serializable]
    public class Bee
    {
        //callback to hook each
        //individual bee object up to the
        //form’s SendMessage() method.
        //It uses a delegate called
        //BeeMessage that takes a bee ID
        //and a message. The bee uses it to
        //send messages back to the form.

        [NonSerialized]
        public BeeMessage MessageSender;

        private const double HoneyConsumed = 0.5;
        private const int MoveRate = 3;
        private const double MinimumFlowerNectar = 1.5;
        private const int CareerSpan = 1000;

        public int Age { get; private set; }
        public bool InsideHive { get; private set; }
        public double NectarCollected { get; private set; }
        public BeeState CurrentState { get; private set; }

        private Point location;
        public Point Location { get { return location; } }

        private int ID;
        private Flower destinationFlower;
        private World world;
        private Hive hive;

        public Bee(int id, Point location, World world, Hive hive)
        {
            this.ID = id;
            Age = 0;
            this.location = location;
            this.world = world;
            this.hive = hive;
            InsideHive = true;
            destinationFlower = null;
            NectarCollected = 0;
            CurrentState = BeeState.Idle;
        }

        public void Go(Random random)
        {
            Age++;
            BeeState oldState = CurrentState;
            switch (CurrentState)
            {
                case BeeState.Idle:
                    if (Age > CareerSpan)
                        CurrentState = BeeState.Retired;
                    else if (world.Flowers.Count > 0 && hive.ConsumeHoney(HoneyConsumed))
                    {
                        Flower flower = world.Flowers[random.Next(world.Flowers.Count)];
                        if (flower.Nectar >= MinimumFlowerNectar && flower.Alive)
                        {
                            destinationFlower = flower;
                            CurrentState = BeeState.FlyingToFlower;
                        }
                    }
                    break;
                case BeeState.FlyingToFlower:
                    if (!world.Flowers.Contains(destinationFlower))
                        CurrentState = BeeState.ReturningToHive;
                    else if (InsideHive)
                    {
                        if (MoveTowardsLocation(hive.GetLocation("Exit")))
                        {
                            InsideHive = false;
                            location = hive.GetLocation("Entrance");
                        }
                    }
                    else
                    {
                        if (MoveTowardsLocation(destinationFlower.Location))
                            CurrentState = BeeState.GatheringNectar;
                    }
                    break;
                case BeeState.GatheringNectar:
                    double nectar = destinationFlower.HarvestNectar();
                    if (nectar > 0)
                        NectarCollected += nectar;
                    else
                        CurrentState = BeeState.ReturningToHive;
                    break;
                case BeeState.ReturningToHive:
                    if (!InsideHive)
                    {
                        if (MoveTowardsLocation(hive.GetLocation("Entrance")))
                        {
                            InsideHive = true;
                            location = hive.GetLocation("Exit");
                        }
                    }
                    else
                    {
                        if (MoveTowardsLocation(hive.GetLocation("HoneyFactory")))
                        {
                            CurrentState = BeeState.MakingHoney;
                        }                        
                    }
                    break;
                case BeeState.MakingHoney:
                    if (NectarCollected < 0.5)
                    {
                        NectarCollected = 0;
                        CurrentState = BeeState.Idle;
                    }
                    else
                    {
                        // once we have a Hive, we’ll turn the nectar into honey
                        if (hive.AddHoney(0.5))
                            NectarCollected -= 0.5;
                        else
                        {
                            NectarCollected = 0;
                        }
                    }
                    break;
                case BeeState.Retired:
                    break;                
            }

            //If the status of the Bee changed, we
            //call back the method our BeeMessage
            //delegate points to, and let that method
            //know about the status change.
            if (oldState!=CurrentState && MessageSender != null)
            {
                MessageSender(ID, CurrentState.ToString());
            }
        }

        private bool MoveTowardsLocation(Point destination)
        {
            if (Math.Abs(destination.X - location.X) <= MoveRate &&
            Math.Abs(destination.Y - location.Y) <= MoveRate)
                return true;
            if (destination.X > location.X)
                location.X += MoveRate;
            else if (destination.X < location.X)
                location.X -= MoveRate;
            if (destination.Y > location.Y)
                location.Y += MoveRate;
            else if (destination.Y < location.Y)
                location.Y -= MoveRate;
            return false;
        }
    }
}
