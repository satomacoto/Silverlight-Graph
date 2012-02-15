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
    /// Represents a Particle in a physics simulation.
    /// </summary>
    /// <remarks>
    /// A particle is a 
    /// point-mass (or point-charge) subject to physical forces.    
    /// </remarks>
    public class Particle : DependencyObject
    {


        /// <summary>
        /// The x position of the Particle.
        /// </summary>        
        public double x
        {
            get { return (double)GetValue(xProperty); }
            set { SetValue(xProperty, value); }
        }

        // Using a DependencyProperty as the backing store for x.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty xProperty =
            DependencyProperty.Register("x", typeof(double), typeof(Particle), 
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnPositionChanged)));


        /// <summary>
        /// The y position of the Particle.
        /// </summary>
        public double y
        {
            get { return (double)GetValue(yProperty); }
            set { SetValue(yProperty, value); }
        }

        // Using a DependencyProperty as the backing store for y.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty yProperty =
            DependencyProperty.Register("y", typeof(double), typeof(Particle), 
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnPositionChanged)));


        public event DependencyPropertyChangedEventHandler PositionChanged;
        private static void OnPositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var p = (Particle)obj;
            if (p.PositionChanged != null)
                p.PositionChanged(obj, args);
        }



        /// <summary>
        /// The mass (or charge) of the Particle.
        /// </summary>
        public double mass;
        /// <summary>
        /// The number of springs (degree) attached to this Particle.
        /// </summary>
        public int degree;
        /// <summary>
        /// The x velocity of the Particle.
        /// </summary>
        public double vx;
        /// <summary>
        /// A temporary x velocity variable.
        /// </summary>
        public double _vx;
        /// <summary>
        /// The y velocity of the Particle.
        /// </summary>
        public double vy;
        /// <summary>
        /// A temporary y velocity variable.
        /// </summary>
        public double _vy;
        /// <summary>
        /// The x force exerted on the Particle.
        /// </summary>
        public double fx;
        /// <summary>
        /// The y force exerted on the Particle.
        /// </summary>
        public double fy;

        /// <summary>
        /// The age of the particle in simulation ticks.
        /// </summary>
        public int age;

        /// <summary>
        /// Flag indicating if the particule is active.
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// Flag indicating that the Particle is scheduled for removal.
        /// </summary>
        public bool IsDead { get; set; }

        /// <summary>
        /// Tag property for storing an arbitrary value.
        /// </summary>
        public object tag;

        /// <summary>
        /// Create a new Particle.
        /// </summary>
        public Particle()
        {
            this.Init(10.0, 0.0, 0.0, 0.0, 0.0, false);
        }

        /// <summary>
        /// Create a new Particle with given parameters.
        /// </summary>
        /// <param name="mass">the mass (or charge) of the particle</param>
        /// <param name="x">the x position of the particle</param>
        /// <param name="y">the y position of the particle</param>
        /// <param name="vx">the x velocity of the particle</param>
        /// <param name="vy">the y velocity of the particle</param>
        /// <param name="isActive">flag indicating if the particle is active</param>
        public Particle(double mass, double x, double y, double vx, double vy, bool isActive)
        {
            this.Init(mass, x, y, vx, vy, isActive);
        }

        /// <summary>
        /// Initializes an existing particle instance.
        /// </summary>
        /// <param name="mass">the mass (or charge) of the particle</param>
        /// <param name="x">the x position of the particle</param>
        /// <param name="y">the y position of the particle</param>
        /// <param name="vx">the x velocity of the particle</param>
        /// <param name="vy">the y velocity of the particle</param>
        /// <param name="isActive">flag indicating if the particle is active</param>
        public void Init(double mass, double x, double y, double vx, double vy, bool isActive)
        {
            this.mass = mass;
            this.x = x;
            this.y = y;
            this.degree = 0;
            this.vx = this._vx = vx;
            this.vy = this._vy = vy;
            this.fx = 0.0;
            this.fy = 0.0;
            this.IsActive = false;
            this.IsDead = false;
        }

        /// <summary>
        /// "Kills" this Particle, scheduling it for removal in the next
		/// simulation cycle.
        /// </summary>
        public void Kill()
        {
            this.IsDead = true;
        }

    }
}
