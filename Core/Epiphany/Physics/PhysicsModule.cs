using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Box2D.XNA;

namespace Gem.Epiphany
{
	public class PhysicsModule : IModule
	{
		public Box2D.XNA.World PhysicsSimulator;
        public List<ActorPhysics> Actors = new List<ActorPhysics>();
        private Dictionary<uint, List<Physics>> physicComponents = new Dictionary<uint,List<Physics>>();

        ContactListener ContactListener = new ContactListener();

        public void BeginSimulation(Simulation sim)
		{
			ContactListener.Clear();
            Actors.Clear();

			PhysicsSimulator = new Box2D.XNA.World(Vector2.Zero /*_settings.Gravity*/, true);
			PhysicsSimulator.ContinuousPhysics = true;
			PhysicsSimulator.WarmStarting = true;
			PhysicsSimulator.ContactListener = this.ContactListener;
		}

		public class RayCastResult
		{
            public bool Hit = false;
			public Physics PhysicsComponent;
			public Vector2 Point;
			public Vector2 Normal;
		}

		float RayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction, RayCastResult result,
            Func<Physics, bool> Filter)
		{
            if (Filter(fixture.GetBody().GetUserData() as Physics))
            {
                result.Hit = true;
                result.PhysicsComponent = fixture.GetBody().GetUserData() as Physics;
                result.Normal = normal;
                result.Point = point;
            }
            return fraction;
		}

		public RayCastResult RayCast(Vector2 Start, Vector2 End, Func<Physics, bool> Filter)
		{
			RayCastResult result = new RayCastResult();
			PhysicsSimulator.RayCast((A, B, C, D) => RayCastCallback(A, B, C, D, result, Filter), Start, End);
			return result;
		}

        public void Update(float time)
        {
            PhysicsSimulator.Step(time, 10, 3);
            PhysicsSimulator.ClearForces();
            ProcessContacts();

            for (int A = 0; A < Actors.Count; ++A)
            {
                Actors[A].ProcessContacts(time);
            }
            
            for (var Body = PhysicsSimulator.GetBodyList(); Body != null; Body = Body.GetNext())
            {
                Physics component = Body.GetUserData() as Physics;
                component.associatedEuler.Position = new Vector3(Body.Position, 0);
                component.associatedEuler.Orientation.Z = Body.GetAngle();
            }
        }

        void ProcessContacts()
		{

			foreach (ContactPoint cp in ContactListener._points)
			{
                Physics AP = cp.fixtureA.GetBody().GetUserData() as Physics;
                Physics BP = cp.fixtureB.GetBody().GetUserData() as Physics;

				if (AP == null || BP == null) throw new InvalidProgramException("How did this happen?");

                if (AP.OnContact != null) AP.OnContact(BP, cp.normal, cp.position);
				if (BP.OnContact != null) BP.OnContact(AP, -cp.normal, cp.position);
			}

			foreach (SensorContact sc in ContactListener.SensorContacts)
			{
				if (sc.Sensor == null || sc.Trigger == null) throw new InvalidProgramException("How did this happen?");
                if (sc.Sensor.OnContact != null) sc.Sensor.OnContact(sc.Trigger, Vector2.Zero, Vector2.Zero);
			}

			ContactListener._points.Clear();
		}

        public double UpdateInterval
        {
            get { return -1; }
        }

        public void EndSimulation()
        {
            //throw new NotImplementedException();
        }

        public void AddComponents(List<Component> components)
        {
            foreach (var component in components)
            {
                var physics = component as Physics;
                if (physics != null)
                {
                    physics.Initialize(PhysicsSimulator);
                    if (!physicComponents.ContainsKey(physics.EntityID))
                        physicComponents.Add(physics.EntityID, new List<Physics>());
                    physicComponents[physics.EntityID].Add(physics);

                    if (physics is ActorPhysics)
                        Actors.Add(physics as ActorPhysics);
                }
            }
        }

        public void RemoveEntities(List<uint> entities)
        {
            foreach (var id in entities)
                if (physicComponents.ContainsKey(id))
                {
                    foreach (var component in physicComponents[id])
                        PhysicsSimulator.DestroyBody(component.PhysicsBody);
                    physicComponents.Remove(id);
                }

            Actors.RemoveAll(ap => entities.Contains(ap.EntityID));
        }
    }
}
