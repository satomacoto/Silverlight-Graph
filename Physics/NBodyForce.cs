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
    /// Force simulating an N-Body force of charged particles with pairwise
    /// interaction, such as gravity or electrical charge. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class uses a
    /// quad-tree structure to aggregate charge values and optimize computation.
    /// The force function is a standard inverse-square law (though in this case
    /// approximated due to optimization): F = G * m1 * m2 / d^2,
    /// where G is a constant (e.g., gravitational constant), m1 and m2 are the
    /// masses (charge) of the particles, and d is the distance between them.
    /// </para>
    /// <para>
    /// The algorithm used is that of J. Barnes and P. Hut, in their research
    /// paper A Hierarchical  O(n log n) force calculation algorithm, Nature, 
    /// v.324, December 1986. For more details on the algorithm, see one of
    /// the following links:
    /// <list type="bullet">
    /// <item>
    /// <see href="http://www.cs.berkeley.edu/~demmel/cs267/lecture26/lecture26.html">
    /// James Demmel's UC Berkeley lecture notes</see>
    /// </item>
    /// <item>
    /// <see href="http://www.physics.gmu.edu/~large/lr_forces/desc/bh/bhdesc.html">
    /// Description of the Barnes-Hut algorithm</see>
    /// </item>
    /// <item>
    /// <see href="http://www.ifa.hawaii.edu/~barnes/treecode/treeguide.html">
    /// Joshua Barnes' implementation</see>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class NBodyForce : IForce
    {
		private double _g;     // gravitational constant
		private double _t;     // barnes-hut theta
		private double _max;   // max effective distance
		private double _min;   // min effective distance
		private double _eps;   // epsilon for determining 'same' location
		
		private double _x1, _y1, _x2, _y2;
		private QuadTreeNode _root;
		
		/// <summary> 
        /// The gravitational constant to use. 
		/// Negative values produce a repulsive force. 
        /// </summary>
        public double gravitation
        {
            get { return _g; }
            set { _g = value; }
        }
		
		/// <summary> 
        /// The maximum distance over which forces are exerted. 
		/// Any greater distances will be ignored. 
        /// </summary>
        public double maxDistance
        {
            get { return _max; }
            set { _max = value; }
        }
		
		/// <summary> 
        /// The minumum effective distance over which forces are exerted.
		/// Any lesser distafnces will be treated as the minimum. 
        /// </summary>
        public double minDistance
        {
            get { return _min; }
            set { _min = value; }
        }

		
		// --------------------------------------------------------------------
		
		/// <summary>
		/// Creates a new NBodyForce.
		/// </summary>
        public NBodyForce()
        {
            _g = -1;
            _max = 200;
            _min = 2;
            _eps = 0.01;
            _t = 0.9;
            _root = QuadTreeNode.Particle();
        }

        /// <summary>
        /// Creates a new NBodyForce with given parameters.
        /// </summary>
        /// <param name="g">the gravitational constant to use.
        ///  Negative values produce a repulsive force.</param>
        /// <param name="maxd">a maximum distance over which the force should operate.
        ///  Particles separated by more than this distance will not interact.</param>
        /// <param name="mind">the minimum distance over which the force should operate.
        ///  Particles closer than this distance will interact as if they were
        ///  the minimum distance apart. This helps avoid extreme forces.
        ///  Helpful when particles are very close together.</param>
        /// <param name="eps">an epsilon values for determining a minimum distance
        ///  between particles</param>
        /// <param name="t">t the theta parameter for the Barnes-Hut approximation.
        ///  Determines the level of approximation.</param>
        public NBodyForce(double g, double maxd, double mind, double eps, double t)
        {
            _g = g;
            _max = maxd;
            _min = mind;
            _eps = eps;
            _t = t;
            _root = QuadTreeNode.Particle();
        }


        /// <summary>
        /// Applies this force to a simulation.
        /// </summary>
        /// <param name="sim">the Simulation to apply the force to</param>
		public void Apply(Simulation sim)
		{
			if (_g == 0) return;
			
			// clear the quadtree
			clear(_root); _root = QuadTreeNode.Particle();
			
			// get the tree bounds
			bounds(sim);
        
        	// populate the tree
        	foreach (var particle in sim.Particles) {
        		insert(particle, _root, _x1, _y1, _x2, _y2);
        	}	
        	
        	// traverse tree to compute mass
        	accumulate(_root);
        	
        	// calculate forces on each Particle
        	foreach (var particle in sim.Particles) {
        		forces(particle, _root, _x1, _y1, _x2, _y2);
        	}
		}
		
		private void accumulate(QuadTreeNode n) {
			double xc = 0, yc = 0;
			n.mass = 0;
			
			// accumulate childrens' mass
            Action<QuadTreeNode> recurse = (c) =>
            {
                if (c == null) return;
                accumulate(c);
                n.mass += c.mass;
                xc += c.mass * c.cx;
                yc += c.mass * c.cy;
            };
			if (n.hasChildren) {
				recurse(n.c1); recurse(n.c2); recurse(n.c3); recurse(n.c4);
			}
			
			// accumulate own mass
			if (n.p != null) {
				n.mass += n.p.mass;
				xc += n.p.mass * n.p.x;
				yc += n.p.mass * n.p.y;
			}
			n.cx = xc / n.mass;
			n.cy = yc / n.mass;
		}
		
		private void forces(Particle p, QuadTreeNode n,
			double x1, double y1, double x2, double y2)
		{
            Random rnd = new Random();
			double f = 0;
			double dx = n.cx - p.x;
			double dy = n.cy - p.y;
			double dd = Math.Sqrt(dx*dx + dy*dy);
			bool max = _max > 0 && dd > _max;
            if (dd == 0)
            { // add direction when needed
                dx = _eps * (0.5 - rnd.NextDouble());
                dy = _eps * (0.5 - rnd.NextDouble());
            }
			
			// the Barnes-Hut approximation criteria is if the ratio of the
        	// size of the quadtree box to the distance between the point and
        	// the box's center of mass is beneath some threshold theta.
        	if ( (!n.hasChildren && n.p != p) || ((x2-x1)/dd < _t) )
        	{
            	if ( max ) return;
            	// either only 1 Particle or we meet criteria
            	// for Barnes-Hut approximation, so calc force
            	dd = dd<_min ? _min : dd;
                f = _g * p.mass * n.mass / (dd * dd * dd);
            	p.fx += f*dx; p.fy += f*dy;
        	}
        	else if ( n.hasChildren )
        	{
            	// recurse for more accurate calculation
            	double sx = (x1+x2)/2;
            	double sy = (y1+y2)/2;
            	
            	if (n.c1 != null) forces(p, n.c1, x1, y1, sx, sy);
				if (n.c2 != null) forces(p, n.c2, sx, y1, x2, sy);
				if (n.c3 != null) forces(p, n.c3, x1, sy, sx, y2);
				if (n.c4 != null) forces(p, n.c4, sx, sy, x2, y2);

            	if ( max ) return;
            	if ( n.p != null && n.p != p ) {
            		dd = dd<_min ? _min : dd;
                	f = _g * p.mass * n.p.mass / (dd*dd*dd);
                	p.fx += f*dx; p.fy += f*dy;
            	}
			}
		}
				
		// -- Helpers ---------------------------------------------------------
		
		private void insert(Particle p, QuadTreeNode n,
			double x1, double y1, double x2, double y2)
		{
			// ignore particles with NaN coordinates
			if (double.IsNaN(p.x) || double.IsNaN(p.y)) return;
			
			// try to insert Particle p at Particle n in the quadtree
        	// by construction, each leaf will contain either 1 or 0 particles
        	if ( n.hasChildren ) {
            	// n contains more than 1 Particle
            	insertHelper(p,n,x1,y1,x2,y2);
        	} else if ( n.p != null ) {
            	// n contains 1 Particle
            	if ( isSameLocation(n.p, p) ) {
            		// recurse
                	insertHelper(p,n,x1,y1,x2,y2);
            	} else {
            		// divide
            		Particle v = n.p; n.p = null;
                	insertHelper(v,n,x1,y1,x2,y2);
                	insertHelper(p,n,x1,y1,x2,y2);
            	}
        	} else { 
            	// n is empty, add p as leaf
            	n.p = p;
        	}
		}
		
		private void insertHelper(Particle p, QuadTreeNode n, 
			double x1, double y1, double x2, double y2)
    	{
    		// determine split
			double sx = (x1+x2)/2;
			double sy = (y1+y2)/2;
			int c = (p.x >= sx ? 1 : 0) + (p.y >= sy ? 2 : 0);
			
			// update bounds
			if (c==1 || c==3) x1 = sx; else x2 = sx;
			if (c>1) y1 = sy; else y2 = sy;
			
			// update children
			QuadTreeNode cn;
			if (c == 0) {
				if (n.c1==null) n.c1 = QuadTreeNode.Particle();
				cn = n.c1;
			} else if (c == 1) {
				if (n.c2==null) n.c2 = QuadTreeNode.Particle();
				cn = n.c2;
			} else if (c == 2) {
				if (n.c3==null) n.c3 = QuadTreeNode.Particle();
				cn = n.c3;
			} else {
				if (n.c4==null) n.c4 = QuadTreeNode.Particle();
				cn = n.c4;
			}
			n.hasChildren = true;
			insert(p,cn,x1,y1,x2,y2);
    	}
		
		private void clear(QuadTreeNode n)
		{
			if (n.c1 != null) clear(n.c1);
			if (n.c2 != null) clear(n.c2);
			if (n.c3 != null) clear(n.c3);
			if (n.c4 != null) clear(n.c4);
			QuadTreeNode.Reclaim(n);
		}
		
		private void bounds(Simulation graph)
		{
            double dx, dy;
			_x1 = _y1 = double.MaxValue;
			_x2 = _y2 = double.MinValue;

			// get bounding box
			foreach(var p in graph.Particles) {
				if (p.x < _x1) _x1 = p.x;
				if (p.y < _y1) _y1 = p.y;
				if (p.x > _x2) _x2 = p.x;
				if (p.y > _y2) _y2 = p.y;
			}
			
			// square the box
			dx = _x2 - _x1;
			dy = _y2 - _y1;
			if (dx > dy) {
				_y2 = _y1 + dx;
			} else {
				_x2 = _x1 + dy;
			}
		}
		
		private bool isSameLocation(Particle p1, Particle p2) {
        	return (Math.Abs(p1.x - p2.x) < _eps && 
        			Math.Abs(p1.y - p2.y) < _eps);
    	}
		
	} // end of class NBodyForce


    // -- Helper QuadTreeNode class -----------------------------------------------
    class QuadTreeNode
    {
        public double mass = 0;
        public double cx = 0;
        public double cy = 0;
        public Particle p = null;
        public QuadTreeNode c1 = null;
        public QuadTreeNode c2 = null;
        public QuadTreeNode c3 = null;
        public QuadTreeNode c4 = null;
        public bool hasChildren = false;

        // -- Factory ---------------------------------------------------------

        private static List<QuadTreeNode> _particles = new List<QuadTreeNode>();

        public static QuadTreeNode Particle()
        {
            QuadTreeNode n;
            if (_particles.Count > 0)
            {
                // n = QuadTreeNode(_particles.pop());
                n = _particles[_particles.Count - 1];
                _particles.RemoveAt(_particles.Count - 1);
            }
            else
            {
                n = new QuadTreeNode();
            }
            return n;
        }

        public static void Reclaim(QuadTreeNode n)
        {
            n.mass = n.cx = n.cy = 0.0;
            n.p = null;
            n.hasChildren = false;
            n.c1 = n.c2 = n.c3 = n.c4 = null;
            _particles.Add(n);
        }
    }
}

