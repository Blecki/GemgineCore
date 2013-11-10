using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Gem.ComponentModel;

namespace Gem.Epiphany
{
	public class Physics : Component
	{
        internal Euler associatedEuler = null;
		protected Box2D.XNA.Body _physicsBody = null;

        public Action<Physics, Vector2, Vector2> OnContact;

        public float Mass { get; set; }

        public void SetActive(bool Active)
        {
            if (PhysicsBody != null) PhysicsBody.SetActive(Active);
        }

		public Box2D.XNA.Body PhysicsBody { get { return _physicsBody; } }

		public bool IsBullet { get; set; }
		public bool AffectedByGravity { get; set; }
        public float Friction { get; set; }
		public float LinearDamping { get; set; }
        public bool AllowSleep { get; set; }

		public void SetCollisionGroup(short Group)
		{
			//TODO : Use SetCollisionGroup for all collision group changes, including initial setup.
			for (Box2D.XNA.Fixture Fixture = PhysicsBody.GetFixtureList(); Fixture != null; Fixture = Fixture.GetNext())
			{
				Box2D.XNA.Filter Filter;
				Fixture.GetFilterData(out Filter);
				Filter.groupIndex = (short)Group;
				Fixture.SetFilterData(ref Filter);
			}
		}

        public Vector2 Gravity { get; set; }

        public void SetGravity(Vector2 Gravity)
        {
            if (AffectedByGravity)
            {
                this.Gravity = Gravity;
                if (_physicsBody != null) _physicsBody.Gravity = Gravity;
            }
        }

		public Physics(Euler associatedEuler)
		{
            this.associatedEuler = associatedEuler;

			AffectedByGravity = true;
			IsBullet = false;
			Friction = 0.0f;
			LinearDamping = 0.0f;
            Gravity = new Vector2(0, 2);
            AllowSleep = true;
            Mass = 1;
		}

        public virtual void Initialize(Box2D.XNA.World physicsSimulation)
        { }
    }
}
