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
    /// Represents a Spring in a physics simulation. 
    /// </summary>
    /// <remarks>
    /// A spring connects two
    /// particles and is defined by the springs rest length, spring tension,
    /// and damping (friction) co-efficient.
    /// </remarks>
    public class Spring
    {
        /// <summary>
        /// The first particle attached to the spring.
        /// </summary>
        public Particle p1 { get; set; }
        /// <summary>
        /// The second particle attached to the spring.
        /// </summary>
        public Particle p2 { get; set; }

        /// <summary>
        /// The rest length of the spring. The default value is 10.
        /// </summary>
        public double restLength;
        /// <summary>
        /// The tensino of the spring. The default value is 0.1.
        /// </summary>
        public double tension;
        /// <summary>
        /// The damping (friction) co-efficient of the spring. The default value is 0.1.
        /// </summary>
        public double damping;
        /// <summary>
        /// Flag indicating that the spring is scheduled for removal.
        /// </summary>
        public bool isDead;

        /// <summary>
        /// Create a new spring with given parameter
        /// </summary>
        /// <param name="p1">the first particle attached to the spring</param>
        /// <param name="p2">the second particle attached to the spring</param>
        public Spring(Particle p1, Particle p2)
        {
            this.p1 = p1;
            this.p2 = p2;

            restLength = 10.0;
            tension = 0.1;
            damping = 0.1;
            isDead = false;
        }

        /// <summary>
        /// "Kills" this spring, scheduling it for removal in the next
		/// simulation cycle.
        /// </summary>
        public void Kill()
        {
            isDead = true;
        }

    }
}
