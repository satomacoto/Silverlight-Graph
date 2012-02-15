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
    /// Interface representing a force within a physics simulation.
    /// </summary>
    public interface IForce
    {
        /// <summary>
        /// Applies this force to a simulation.
        /// </summary>
        /// <param name="sim">the Simulation to apply the force to</param>
        void Apply(Simulation sim);
    } // end of interface IForce
}
