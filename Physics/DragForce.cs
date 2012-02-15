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
    /// Force simulating frictional drag forces (e.g., air resistance). 
    /// </summary>
    /// <remarks>
    /// For each Particle, this force applies a drag based on the particles
    /// velocity (F = a * v, where a is a drag co-efficient and
    /// v is the velocity of the Particle).
    /// </remarks>
	public class DragForce : IForce
	{
		double _dc;
		
        /// <summary>
        /// The drag co-efficient.
        /// </summary>
        public double drag
        {
            get { return _dc; }
            set { _dc = value; }
        }
		
        /// <summary>
        /// Creates a new DragForce with given drag co-efficient.
        /// @param drag the drag co-efficient.
        /// </summary>
		public DragForce() {
			_dc = 0.1;
		}

        /// <summary>
        /// Applies this force to a simulation.
        /// </summary>
        /// <param name="sim">the Simulation to apply the force to</param>
        public void Apply(Simulation sim)
        {
            if (_dc == 0) return;
            foreach (var particle in sim.Particles)
            {
                var p = particle;
                p.fx -= _dc * p.vx;
                p.fy -= _dc * p.vy;
            }
        }
		
	} // end of class DragForce
}
