
namespace Gem.Common
{
	public class Coordinate 
	{
		public int x = 0;
		public int y = 0;
		
		public Coordinate(int x = 0, int y = 0) 
		{
			this.x = x;
			this.y = y;
		}
		
		public Coordinate Neighbor(int direction)
		{
			switch (direction)
			{
				case Direction.North:
					return new Coordinate(x, y - 1);
				case Direction.East:
					return new Coordinate(x + 1, y);
				case Direction.South:
					return new Coordinate(x, y + 1);
				case Direction.West:
					return new Coordinate(x - 1, y);
			}
			return new Coordinate(x, y);
		}
		
	}

}