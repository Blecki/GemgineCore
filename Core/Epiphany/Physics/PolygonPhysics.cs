using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Gem.ComponentModel;

namespace Gem.Epiphany
{
	public class PolygonPhysics : Physics
	{
        public bool IsSensor { get; set; }
		public Box2D.XNA.BodyType Type { get; set; }
        public List<Box2D.XNA.Shape> shapes;

		public PolygonPhysics(Euler associatedEuler, params Object[] shapes) : base(associatedEuler)
		{
            IsSensor = false;
            Type = Box2D.XNA.BodyType.Dynamic;
            this.shapes = new List<Box2D.XNA.Shape>();
            foreach (var obj in shapes)
            {
                if (obj is Box2D.XNA.Shape) this.shapes.Add(obj as Box2D.XNA.Shape);
                else if (obj is IShape) this.shapes.AddRange((obj as IShape).GetPhysicsShapes());
                else throw new InvalidProgramException();
            }
		}

        public override void Initialize(Box2D.XNA.World physicsSimulation)
        {
            Box2D.XNA.BodyDef BodyDef = new Box2D.XNA.BodyDef();
            BodyDef.position = new Vector2(this.associatedEuler.Position.X, this.associatedEuler.Position.Y);
            BodyDef.type = Type;
            BodyDef.bullet = IsBullet;
            BodyDef.allowSleep = AllowSleep;
            BodyDef.linearDamping = LinearDamping;
            _physicsBody = physicsSimulation.CreateBody(BodyDef);
            _physicsBody.SetUserData(this);
            _physicsBody.Gravity = (AffectedByGravity ? Gravity : Vector2.Zero);

            //Apply initial orientations from Euler
            _physicsBody.Rotation = associatedEuler.Orientation.Z;
            _physicsBody.Position = new Vector2(associatedEuler.Position.X, associatedEuler.Position.Y);

            Box2D.XNA.MassData MassData = new Box2D.XNA.MassData();

            var fd = new Box2D.XNA.FixtureDef();
            fd.density = 1;
            fd.isSensor = IsSensor;
            fd.friction = Friction;

            foreach (var shape in shapes)
            {
                fd.shape = shape;
                _physicsBody.CreateFixture(fd);
            }

            MassData.mass = Mass;
            MassData.center = Vector2.Zero;
            MassData.I = MassData.mass * MassData.mass / 12;
            _physicsBody.SetMassData(ref MassData);
        }
	}
}
