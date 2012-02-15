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
    /// Force simulating a spring force between two particles. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// This force iterates over each Spring instance in a simulation and
    /// computes the spring force between the attached particles. Spring forces
    /// are computed using Hooke's Law plus a damping term modeling frictional
    /// forces in the spring.
    /// </para>
    /// <para>
    /// The actual equation is of the form: F = -k*(d - L) + a*d*(v1 - 
    /// v2), where k is the spring tension, d is the distance between
    /// particles, L is the rest length of the string, a is the damping
    /// co-efficient, and v1 and v2 are the velocities of the particles.
    /// </para>
    /// </remarks>
    public class SpringForce : IForce
    {

        double dx, dy, dn, dd, k, fx, fy;

        /// <summary>
        /// Applies this force to a simulation.
        /// </summary>
        /// <param name="sim">the Simulation to apply the force to</param>
        public void Apply(Simulation sim)
        {
            foreach (var edge in sim.Springs)
            {
                var s = edge.p1;
                var t = edge.p2;
                dx = s.x - t.x;
                dy = s.y - t.y;
                dn = Math.Sqrt(dx * dx + dy * dy);
                dd = dn < 1.0 ? 1.0 : dn;

                k = edge.tension * (dn - edge.restLength);
                k += edge.damping * (dx * (s.vx - t.vx) + dy * (s.vy - t.vy)) / dd;
                k /= dd;


                Random rnd = new Random();
                if (dn == 0)
                {
                    dx = 0.01 * (0.5 - rnd.NextDouble());
                    dy = 0.01 * (0.5 - rnd.NextDouble());
                }

                fx = -k * dx;
                fy = -k * dy;

                s.fx += fx; s.fy += fy;
                t.fx -= fx; t.fy -= fy;
            }
        }
    }
}
