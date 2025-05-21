using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class Animation
    {
        public double Time { get; set; } = 0;

        public double GlobalYAngle { get; private set; } = 0;
        public double GlobalXAngle { get; private set; } = 0;
        public double GlobalZAngle { get; private set; } = 0;

        private double rotationSpeed { get; set; } = 0.5; 

        private double orbitRnage { get; set; } = 0.5;

        // change angles 
        internal void AdvanceTime(double deltaTime)
        {
            Time += deltaTime;

            GlobalXAngle = -Time;
            GlobalYAngle = -Time;
            GlobalZAngle = -Time;
        }
    }
}
