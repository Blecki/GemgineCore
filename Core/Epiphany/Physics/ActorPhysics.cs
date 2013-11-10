using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Gem.ComponentModel;

namespace Gem.Epiphany
{
	public class ActorPhysics : Physics
	{
		public Vector2 Dimensions { get; set; }
		public bool PreventRotation { get; set; }

        public bool IsOnGround { get; set; }
        public float TimeSinceOnGround { get; set; }
        public Vector2 GroundNormal { get; set; }

        public ActorPhysics(Euler associatedEuler) : base(associatedEuler)
        {
            Mass = 8;
        }

        public void ApplyBasicForce(Vector2 force)
        {
            if (PhysicsBody != null) PhysicsBody.ApplyForce(force, PhysicsBody.Position);
        }

        public override void Initialize(Box2D.XNA.World physicsSimulation)
        {
            Box2D.XNA.BodyDef BodyDef = new Box2D.XNA.BodyDef();
            BodyDef.position = new Vector2(this.associatedEuler.Position.X, this.associatedEuler.Position.Y);
            BodyDef.type = Box2D.XNA.BodyType.Dynamic;
            BodyDef.bullet = IsBullet;
            BodyDef.allowSleep = AllowSleep;
            BodyDef.linearDamping = LinearDamping;
            _physicsBody = physicsSimulation.CreateBody(BodyDef);
            _physicsBody.SetUserData(this);
            _physicsBody.Gravity = (AffectedByGravity ? Gravity : Vector2.Zero);

            //Apply initial orientations from Euler
            _physicsBody.Rotation = associatedEuler.Orientation.Z;
            _physicsBody.Position = new Vector2(associatedEuler.Position.X, associatedEuler.Position.Y);
            
            var fd = new Box2D.XNA.FixtureDef();
            fd.density = 1;
            fd.friction = Friction;

            Box2D.XNA.PolygonShape Shape = new Box2D.XNA.PolygonShape();
            Box2D.XNA.CircleShape FootShape = new Box2D.XNA.CircleShape();

            Shape.SetAsBox(Dimensions.X / 2, (Dimensions.Y - (Dimensions.X / 2)) / 2, new Vector2(0, Dimensions.X / 4), 0.0f);
            //Shape._radius = Dimensions.Length();
            fd.shape = Shape;
            //fd.filter.groupIndex = (short)CollisionGroup;
            fd.density = 1;
            fd.friction = Friction;

            _physicsBody.CreateFixture(fd);

            FootShape._radius = Dimensions.X / 2;
            FootShape._p = new Vector2(0, -Dimensions.Y / 2 + Dimensions.X / 2);
            fd.shape = FootShape;
            _physicsBody.CreateFixture(fd);

            Box2D.XNA.MassData MassData = new Box2D.XNA.MassData();
            MassData.mass = Mass;
            MassData.center = Vector2.Zero;
            MassData.I = PreventRotation ? 0 : MassData.mass * MassData.mass / 12;
            _physicsBody.SetMassData(ref MassData);
        }

        public struct ContactImpulse
        {
            public Vector2 Normal;
            public Vector2 Impulse;
        }

        public List<ContactImpulse> ContactImpulses = new List<ContactImpulse>();
        public float CrushForce = 0.0f;
        public float CrushThreshold = 512.0f;

        static private float CrossZ(Vector2 A, Vector2 B)
        {
            return (B.Y * A.X) - (B.X * A.Y);
        }

        static private float AngleBetweenVectors(Vector2 A, Vector2 B)
        {
            A.Normalize();
            B.Normalize();
            float DotProduct = Vector2.Dot(A, B);
            DotProduct = MathHelper.Clamp(DotProduct, -1.0f, 1.0f);
            float Angle = (float)System.Math.Acos(DotProduct);
            if (CrossZ(A, B) < 0) return -Angle;
            return Angle;
        }

        internal void ProcessContacts(float time)
        {
            IsOnGround = false;
            CrushForce = 0.0f;
            Vector2 GroundNormalAccum = Vector2.Zero;
            int GroundVectors = 0;

            for (int A = 0; A < ContactImpulses.Count; ++A)
            {
                if (Vector2.Dot(Vector2.Normalize(Gravity), ContactImpulses[A].Normal) > 0.33f)
                {
                    GroundNormalAccum += -ContactImpulses[A].Normal;
                    GroundVectors += 1;
                }

                for (int B = A + 1; B < ContactImpulses.Count; ++B)
                {
                    float Angle = System.Math.Abs(AngleBetweenVectors(
                        ContactImpulses[A].Normal, ContactImpulses[B].Normal));
                    if (Angle > System.Math.PI * 0.75f)
                    {
                        float Force = ContactImpulses[A].Impulse.Length() + ContactImpulses[B].Impulse.Length();
                        if (Force > CrushForce) CrushForce = Force;
                    }
                }
            }

            if (GroundVectors > 0)
            {
                GroundNormal = GroundNormalAccum / GroundVectors;
                IsOnGround = true;
                TimeSinceOnGround = 0;
            }
            else
            {
                TimeSinceOnGround += time;
            }

            ContactImpulses.Clear();

        }
	}
}
