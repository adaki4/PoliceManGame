using System.Net;

namespace pb006 {
    public struct Position
    {
        public readonly int x;
        public readonly int y;

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // vrátí pozici nacházející se jedno políčko ve směru d
        public Position Step(Direction d)
        {
            int[,] offsets = { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
            Position res = new Position(this.x + offsets[(int)d, 0], this.y + offsets[(int)d, 1]);
            return res;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        public static bool operator ==(Position p1, Position p2)
        {
            return ((p1.x == p2.x) && (p1.y == p2.y));
        }
        public static bool operator !=(Position p1, Position p2)
        {
            return !((p1.x == p2.x) && (p1.y == p2.y));
        }
    }

    public static class Extensions
    {
        public static Direction Opposite(this Direction dir)
        {
            switch (dir)
            {
                case Direction.North: return Direction.South;
                case Direction.South: return Direction.North;
                case Direction.East: return Direction.West;
                case Direction.West: return Direction.East;
            }
            return Direction.North; // unreachable
        }

        public static char ToSymbol(this Direction dir)
        {
            switch (dir) {
                case Direction.North: return '^';
                case Direction.South: return 'v';
                case Direction.East: return '>';
                case Direction.West: return '<';
            }
            return 'x'; // unreachable
        }
    }
    public enum Direction
    {
        North, East, South, West
    }
}