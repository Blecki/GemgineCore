using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Epiphany
{
	public struct ContactPoint
	{
		public Box2D.XNA.Fixture fixtureA;
		public Box2D.XNA.Fixture fixtureB;
		public Vector2 normal;
		public Vector2 position;
	}

	internal class SensorContact
	{
		public Physics Sensor;
		public Physics Trigger;
        public int Count;

        public SensorContact()
        {
            Count = 0;
            Sensor = null;
            Trigger = null;
        }
	}

	class ContactListener : Box2D.XNA.IContactListener
	{
		internal List<SensorContact> SensorContacts = new List<SensorContact>();

        public void AddSensorContact(Box2D.XNA.Fixture Sensor, Box2D.XNA.Fixture Trigger)
        {
            var SensorPhysics = Sensor.GetBody().GetUserData() as Physics;
            var TriggerPhysics = Trigger.GetBody().GetUserData() as Physics;

            var SensorContact = SensorContacts.FirstOrDefault((A) =>
                {
                    return A.Sensor == SensorPhysics && A.Trigger == TriggerPhysics;
                });
            if (SensorContact == null)
            {
                SensorContact = new SensorContact();
                SensorContact.Trigger = TriggerPhysics;
                SensorContact.Sensor = SensorPhysics;
                SensorContacts.Add(SensorContact);
            }

            SensorContact.Count += 1;
        }

		public void BeginContact(Box2D.XNA.Contact contact)
		{
			Box2D.XNA.Fixture fixtureA = contact.GetFixtureA();
			Box2D.XNA.Fixture fixtureB = contact.GetFixtureB();

            if (fixtureA.IsSensor())
                AddSensorContact(fixtureA, fixtureB);

            if (fixtureB.IsSensor())
                AddSensorContact(fixtureB, fixtureA);
		}

		public void EndContact(Box2D.XNA.Contact contact)
		{
			Box2D.XNA.Fixture fixtureA = contact.GetFixtureA();
			Box2D.XNA.Fixture fixtureB = contact.GetFixtureB();

			if (fixtureA.IsSensor())
				RemoveSensorContact(fixtureA, fixtureB);

			if (fixtureB.IsSensor())
				RemoveSensorContact(fixtureB, fixtureA);

		}

		private void RemoveSensorContact(Box2D.XNA.Fixture Sensor, Box2D.XNA.Fixture Trigger)
		{
            var SensorPhysics = Sensor.GetBody().GetUserData() as Physics;
            var TriggerPhysics = Trigger.GetBody().GetUserData() as Physics;

			for (int i = 0; i < SensorContacts.Count; ++i)
			{
				SensorContact sc = SensorContacts[i];
				if (sc.Sensor == SensorPhysics && sc.Trigger == TriggerPhysics)
				{
                    SensorContacts[i].Count -= 1;
                    if (SensorContacts[i].Count == 0)
					    SensorContacts.RemoveAt(i);
					return;
				}
			}
		}

        public void PostSolve(Box2D.XNA.Contact contact, ref Box2D.XNA.ContactImpulse impulse)
        {
            if (!contact.IsTouching()) return;

            Box2D.XNA.WorldManifold Manifold;
            contact.GetWorldManifold(out Manifold);

            Object DataA = contact.GetFixtureA().GetBody().GetUserData();
            Object DataB = contact.GetFixtureB().GetBody().GetUserData();

            if (DataA is ActorPhysics) ( DataA as ActorPhysics ).ContactImpulses.Add(new ActorPhysics.ContactImpulse
                {
                    Normal = Manifold._normal,
                    Impulse = new Vector2(impulse.normalImpulses[0], impulse.normalImpulses[1])
                });

            if (DataB is ActorPhysics) ( DataB as ActorPhysics ).ContactImpulses.Add(new ActorPhysics.ContactImpulse
            {
                Normal = -Manifold._normal,
                Impulse = new Vector2(impulse.normalImpulses[0], impulse.normalImpulses[1])
            });
        }

		public void PreSolve(Box2D.XNA.Contact contact, ref Box2D.XNA.Manifold oldManifold)
		{
			if (!contact.IsTouching()) return;

			Box2D.XNA.Manifold manifold;
			contact.GetManifold(out manifold);

			if (manifold._pointCount == 0)
			{
				return;
			}

			Box2D.XNA.Fixture fixtureA = contact.GetFixtureA();
			Box2D.XNA.Fixture fixtureB = contact.GetFixtureB();

			Box2D.XNA.FixedArray2<Box2D.XNA.PointState> state1, state2;
			Box2D.XNA.Collision.GetPointStates(out state1, out state2, ref oldManifold, ref manifold);


			Box2D.XNA.WorldManifold worldManifold;
			contact.GetWorldManifold(out worldManifold);

			for (int i = 0; i < manifold._pointCount && _points.Count < 128; ++i)
			{
				ContactPoint cp = new ContactPoint();
				cp.fixtureA = fixtureA;
				cp.fixtureB = fixtureB;
				cp.position = worldManifold._points[i];
				cp.normal = worldManifold._normal;
				_points.Add(cp);
                
			}
		}

    	internal List<ContactPoint> _points = new List<ContactPoint>();

		public void Clear()
		{
			_points.Clear();
			SensorContacts.Clear();
		}

	}
}
