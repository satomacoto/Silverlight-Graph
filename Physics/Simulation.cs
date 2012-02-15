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
using System.Collections.Generic;

namespace Physics
{
    /// <summary>
    /// A physical simulation involving particles, springs, and forces.
    /// Useful for simulating a range of physical effects or layouts.
    /// </summary>
    public class Simulation
    {
        /// <summary>
        /// The default gravity force for this simulation.
        /// </summary>
        public GravityForce gravityForce
        {
            get { return _forces[0] as GravityForce; }
        }
        /// <summary>
        /// The default n-body force for this simulation.
        /// </summary>
        public NBodyForce nbodyForce
        {
            get { return _forces[1] as NBodyForce; }
        }
        /// <summary>
        /// The default drag force for this simulation.
        /// </summary>
        public DragForce dragForce
        {
            get { return _forces[2] as DragForce; }
        }
        /// <summary>
        /// The default spring force for this simulation.
        /// </summary>
        public SpringForce springForce
        {
            get { return _forces[3] as SpringForce; }
        }

        List<Particle> _particles;
        /// <summary>
        /// The list of particles
        /// </summary>
        public List<Particle> Particles
        {
            get
            {
                if (_particles == null)
                    _particles = new List<Particle>();
                return _particles;
            }
        }
        List<Spring> _springs;
        /// <summary>
        /// The list of springs
        /// </summary>
        public List<Spring> Springs
        {
            get
            {
                if (_springs == null)
                    _springs = new List<Spring>();
                return _springs;
            }
        }

        Rect _bounds;
        /// <summary>
        /// Sets a bounding box for particles in this simulation. Rect.Empty (the default) indicates no boundaries.
        /// </summary>
        public Rect bounds
        {
            get { return _bounds; }
            set 
            {
                if (_bounds == value) return;
                if (value == Rect.Empty) { _bounds = Rect.Empty; return; }
                if (_bounds == Rect.Empty) { _bounds = new Rect(); }
                // ensure x is left-most and y is top-most
                _bounds.X = value.X + (value.Width < 0 ? value.Width : 0);
                _bounds.Width = (value.Width < 0 ? -1 : 1) * value.Width;
                _bounds.Y = value.Y + (value.Width < 0 ? value.Height : 0);
                _bounds.Height = (value.Height < 0 ? -1 : 1) * value.Height;
            }
        }

        /// <summary>
        /// Creates a new physics simulation.
        /// </summary>
        public Simulation()
        {
            _bounds = Rect.Empty;
            _forces.Add(new GravityForce() { gx = 0, gy = 0 });
            _forces.Add(new NBodyForce() { gravitation = -5 });
            _forces.Add(new DragForce() { drag = 0.1 });
            _forces.Add(new SpringForce());
        }

        /// <summary>
        /// Start animation.
        /// </summary>
        public void Start()
        {
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }
        /// <summary>
        /// Stop animation.
        /// </summary>
        public void Stop()
        {
            CompositionTarget.Rendering -= new EventHandler(CompositionTarget_Rendering);
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Tick(1);
        }


        #region Init Simulation
        /// <summary>
        /// Adds a custom force to the force simulation.
        /// </summary>
        /// <param name="force">the force to add</param>
        public void AddForce(IForce force)
        {
            _forces.Add(force);
        }

        /// <summary>
        /// Adds a new Particle to the simulation.
        /// </summary>
        /// <param name="mass">the mass (charge) of the Particle</param>
        /// <param name="x">the Particle's starting x position</param>
        /// <param name="y">the Particle's starting y position</param>
        /// <returns></returns>
        public Particle AddParticle(double mass, double x, double y)
        {
            var particle = getParticle(mass, x, y);
            Particles.Add(particle);
            return particle;
        }

        /// <summary>
        /// Removes a Particle from the simulation. Any springs attached to
        /// the Particle will also be removed.
        /// </summary>
        /// <returns>true if removed, false otherwise.</returns>
        public bool RemoveParticle()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a spring to the simulation
        /// </summary>
        /// <param name="p1">the first Particle attached to the spring</param>
        /// <param name="p2">the second Particle attached to the spring</param>
        /// <param name="restLength">the rest length of the spring</param>
        /// <param name="tenstion">the tension of the spring</param>
        /// <param name="damping">the damping (friction) co-efficient of the spring</param>
        /// <returns>the added spring</returns>
        public Spring AddSpring(Particle p1, Particle p2, double restLength,
            double tension, double damping)
        {
            var edge = new Spring(p1, p2)
            {
                restLength = restLength,
                tension = tension,
                damping = damping
            };
            p1.degree++;
            p2.degree++;
            Springs.Add(edge);
            return edge;
        }

        /// <summary>
        /// Removes a spring from the simulation.
        /// </summary>
        /// <returns>true if removed, false otherwise.</returns>
        public bool RemoveSpring()
        {
            throw new NotImplementedException();
        } 
        #endregion

        #region Run Simulation

        List<IForce> _forces = new List<IForce>();

        /// <summary>
        /// Advance the simulation for the specified time interval.
        /// </summary>
        /// <param name="dt">the time interval to step the simulation (default 1)</param>
        public void Tick(int dt)
        {
            var dt1 = dt / 2.0;
            var dt2 = dt * dt / 2.0;

            // remove springs connected to dead particles
            foreach (var s in Springs)
            {
                if (s.isDead || s.p1.IsDead || s.p2.IsDead)
                {
                    s.p1.degree--;
                    s.p2.degree--;
                    reclaimSpring(s);
                    _springs.Remove(s);
                }
            }

            // update nodes using Verlet integration
            foreach (var p in Particles)
            {
                p.age += dt;
                if (p.IsDead)
                {
                    reclaimParticle(p);
                    _particles.Remove(p);
                }
                else if (!p.IsActive)
                {
                    p.vx = p.vy = 0;
                }
                else
                {
                    var ax = p.fx / p.mass;
                    var ay = p.fy / p.mass;
                    var dx = p.vx * dt + ax * dt2;
                    var dy = p.vy * dt + ay * dt2;
                    // don't update nodes if dx and dy is  small
                    //if (dx * dx + dy * dy < 0.01)
                    //{
                    //    p._vx = 0;
                    //    p._vy = 0;
                    //    continue;
                    //}
                    p.x += dx;
                    p.y += dy;
                    p._vx = p.vx + ax * dt1;
                    p._vy = p.vy + ay * dt1;
                }
            }

            // evaluate force
            Eval();

            // update nodes velocities
            foreach (var p in Particles)
            {
                if (p.IsActive)
                {
                    var ax = dt1 / p.mass;
                    p.vx = p._vx + p.fx * ax;
                    p.vy = p._vy + p.fy * ax;
                }
            }

            // enfore bounds
            if (_bounds != Rect.Empty) enforceBounds();
        }

        void enforceBounds()
        {
            var minX = _bounds.X;
            var maxX = _bounds.X + _bounds.Width;
            var minY = _bounds.Y;
            var maxY = _bounds.Y + _bounds.Height;

            foreach (var p in Particles)
            {
                if (p.x < minX)
                {
                    p.x = minX; p.vx = 0;
                }
                else if (p.x > maxX)
                {
                    p.x = maxX; p.vx = 0;
                }
                if (p.y < minY)
                {
                    p.y = minY; p.vy = 0;
                }
                else if (p.y > maxY)
                {
                    p.y = maxY; p.vy = 0;
                }
            }
        }

        /// <summary>
        /// Evaluates the set of forces in the simulation.
        /// </summary>
        public void Eval()
        {
            foreach (var p in Particles)
            {
                p.fx = p.fy = 0;
            }
            foreach (var force in _forces)
            {
                force.Apply(this);
            }
        } 
        #endregion

        #region Particle Pool

        /// <summary>
        /// The maximum number of items stored in a simulation object pool.
        /// </summary>
        public static int objectPoolLimit = 5000;
        /// <summary>
        /// The pool of particles
        /// </summary>
        protected static Stack<Particle> _ppool = new Stack<Particle>();
        /// <summary>
        /// The pool of springs
        /// </summary>
        protected static Stack<Spring> _spool = new Stack<Spring>();

        /// <summary>
        /// Returns a Particle instance, pulling a recycled Particle from the
        /// object pool if available.
        /// </summary>
        /// <param name="mass">the mass (charge) of the Particle</param>
        /// <param name="x">the Particle's starting x position</param>
        /// <param name="y">the Particle's starting y position</param>
        /// <returns>a Particle instance</returns>
        protected static Particle getParticle(double mass, double x, double y)
        {
            if (_ppool.Count > 0)
            {
                var p = _ppool.Pop();
                p.Init(mass, x, y, 0.0, 0.0, false);
                return p;
            }
            else
            {
                return new Particle(mass, x, y, 0.0, 0.0, false);
            }
        }


        /// <summary>
        /// Reclaims a Particle, adding it to the object pool for recycling
        /// <param name="p">the Particle to Reclaim</param>
        /// </summary>
        protected static void reclaimParticle(Particle p)
        {
            if (_ppool.Count < objectPoolLimit)
            {
                _ppool.Push(p);
            }
        }

        /// <summary>
        /// Reclaims a spring, adding it to the object pool for recycling
        /// </summary>
        /// <param name="s">the spring to Reclaim</param>
        protected static void reclaimSpring(Spring s)
        {
            if (_spool.Count < objectPoolLimit)
            {
                _spool.Push(s);
            }
        } 
        #endregion
    }
}
