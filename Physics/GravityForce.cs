using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Physics
{
    /// <summary>
    /// Force simulating a global gravitational pull on Particle instances.
    /// </summary>
    public class GravityForce : IForce
    {
        double _gx, _gy;

        /// <summary>
        /// The gravitational acceleration in the horizontal dimension. The defalut value is 0.
        /// </summary>
        public double gx
        {
            get { return _gx; }
            set { _gx = value; }
        }
        /// <summary>
        /// The gravitational acceleration in the vertical dimension. The defalut value is 0.
        /// </summary>
        public double gy
        {
            get { return _gy; }
            set { _gy = value; }
        }

        /// <summary>
        /// Creates a new gravity force.
        /// </summary>
        public GravityForce()
        {
            _gx = _gy = 0;
        }

        /// <summary>
        /// Creates a new gravity force with given acceleration values.
        /// </summary>
        /// <param name="gx">the gravitational acceleration in the horizontal dimension</param>
        /// <param name="gy">the gravitational acceleration in the vertical dimension</param>
        public GravityForce(double gx, double gy)
        {
            _gx = _gy = 0;
        }


        /// <summary>
        /// Applies this force to a simulation.
        /// </summary>
        /// <param name="sim">the Simulation to apply the force to</param>
        public void Apply(Simulation sim)
        {
            if (_gx == 0 && _gy == 0) return;

            foreach (var n in sim.Particles)
            {
                n.fx += _gx * n.mass;
                n.fy += _gy * n.mass;
            }
        }
    }
}
