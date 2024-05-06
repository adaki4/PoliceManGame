using System;
using System.Collections.Generic;


namespace pb006
{

    public abstract class GameObject
    {
        protected char objectRepr;
        private Position _objPos;
        public Position Position
        {
            get
            {
                return new Position(_objPos.x, _objPos.y);
            }
            protected set
            {
                _objPos = value;
            }
        }
        protected Position initialPosition;
        protected Direction? initialDirection;



        public abstract void Update(Dictionary<Direction, GameObject>  inputDir);
        public bool? IsMoving() {
            return null;
        }

        public char Repr() {
            return objectRepr;
        }
        public virtual void Reset() {
            Position = initialPosition;
        }
    }

    public abstract class StaticObject : GameObject
    {
        public new bool? IsMoving() {
            return false;
        }
        public override void Update(Dictionary<Direction, GameObject>  inputDir){
            return;
        }

    }

    public class Target : StaticObject
    {
        public Target(Position position)
        {
            Position = new Position(position.x, position.y);
            initialPosition = new Position(position.x, position.y);
            objectRepr = '.';
        }
    }

    public class Wall : StaticObject
    {
        public Wall(Position position)
        {
            Position = new Position(position.x, position.y);
            initialPosition = new Position(position.x, position.y);
            objectRepr = '█';
        }
    }

    public class Agent : GameObject
    {
        private Direction? _objDirection;

        public Direction? Direction
        {
            get
            {
                return _objDirection;

            }

            protected set
            {
                _objDirection = value;
            }
        }
        public Agent(Position position, Direction? agentDir)
        {
            Position = new Position(position.x, position.y);
            initialPosition = new Position(position.x, position.y);
            Direction = agentDir;
            initialDirection = agentDir;
            objectRepr = Direction == null ? 'o' : Direction.Value.ToSymbol();
        }

        public override void Update(Dictionary<Direction, GameObject>  inputDir)
        {
            Direction currDirecton;

            if (Direction != null) {
                currDirecton = (Direction) Direction;
            }
            else {
                return;
            }
            if (inputDir[currDirecton] == null || inputDir[currDirecton].Repr() == '.') {
                Position = Position.Step(currDirecton);
            }
            else {
                Direction = currDirecton.Opposite();
            }
            objectRepr = Direction == null ? 'o' : Direction.Value.ToSymbol();
        }

        public new bool? IsMoving()
        {
            return Direction != null;
        }

        public override void Reset()
        {
            Position = initialPosition;
            Direction = initialDirection;
            objectRepr = Direction == null ? 'o' : Direction.Value.ToSymbol();
        }


    }

    public class Player : Agent
    {
        public Player(Position position) : base(position, null)
        {
            Position = new Position(position.x, position.y);
            initialPosition = new Position(position.x, position.y);
            initialDirection = null;
        }
        public void SetDirection(Direction? newDirection)
        {
            Direction = newDirection;
            objectRepr =  newDirection == null ? 'o' : newDirection.Value.ToSymbol();
        }

    }

    class Crate : GameObject
    {
        public Crate(Position position)
        {
            objectRepr = 'X';
            Position = new Position(position.x, position.y);
            initialPosition = new Position(position.x, position.y);
        }

        public new bool? IsMoving() {
            return false;
        }
        public override void Update(Dictionary<Direction, GameObject>  inputDir)
        {
            foreach(Direction currDir in Enum.GetValues(typeof(Direction))) {
                GameObject currObject = inputDir[currDir];
                if (currObject == null) {
                    continue;
                }
                if (currObject.Repr() != '.' && currObject.Repr() != '#' && currObject.Repr() != 'X') {
                    Agent currAgent = (Agent) currObject;
                    if (currAgent.Direction == currDir.Opposite() && (inputDir[currDir.Opposite()] == null || inputDir[currDir.Opposite()].Repr() == '.')) {
                        Position = Position.Step(currDir.Opposite());
                        break;
                    }
                }
            }
        }
        
        // public  static void Main(string[] args)
        // {
        //     StaticObject staticObjectWall = new Wall(new Position(0, 0));
        //     Console.WriteLine(staticObjectWall.Position); // ~> Position(0, 0)
        //     //staticObjectWall.Position = new Position(0, 1); // compilation fails
        //     Console.WriteLine(staticObjectWall.Repr()); // ~>* '#'
        //     staticObjectWall.Update(new Dictionary<Direction, GameObject>(
        //     new KeyValuePair<Direction, GameObject>[] {
        //     new KeyValuePair<Direction, GameObject>(Direction.North, null),
        //     new KeyValuePair<Direction, GameObject>(Direction.East, null),
        //     new KeyValuePair<Direction, GameObject>(Direction.South, null),
        //     new KeyValuePair<Direction, GameObject>(Direction.West, null)
        //     }
        //     )); // ~>* no change
        //     Console.WriteLine(staticObjectWall.IsMoving()); // ~>* false
        //     GameObject gameObjectWall = staticObjectWall;
        //     Console.WriteLine(gameObjectWall.Position);  // ~> Position(0, 0)
        //     Console.WriteLine(gameObjectWall.Repr()); // ~>* '#'
        //     gameObjectWall.Update(new Dictionary<Direction, GameObject>(
        //     new KeyValuePair<Direction, GameObject>[] {
        //     new KeyValuePair<Direction, GameObject>(Direction.North, null),
        //     new KeyValuePair<Direction, GameObject>(Direction.East, null),
        //     new KeyValuePair<Direction, GameObject>(Direction.South, null),
        //     new KeyValuePair<Direction, GameObject>(Direction.West, null)
        //     }
        //     )); // ~>* no change
        //     Console.WriteLine(gameObjectWall.IsMoving() == null); // ~>* null
        //     var emptyNeighborhood = new Dictionary<Direction, GameObject>(
        //     new KeyValuePair<Direction, GameObject>[] {
        //     new KeyValuePair<Direction, GameObject>(Direction.North, null),
        //     new KeyValuePair<Direction, GameObject>(Direction.East, null),
        //     new KeyValuePair<Direction, GameObject>(Direction.South, null),
        //     new KeyValuePair<Direction, GameObject>(Direction.West, null)
        //     }
        //     );
        //     Player player = new Player(new Position(0, 0));
        //     Console.WriteLine(player.Repr()); // ~>* 'o'
        //     player.Update(emptyNeighborhood); // ~>* no change
        //     Console.WriteLine(player.IsMoving()); // false
        //     player.SetDirection(Direction.North);
        //     Console.WriteLine(player.Repr()); // ~>* '^'
        //     Console.WriteLine(player.IsMoving()); // true
        //     Console.WriteLine(player.Position); // ~>* (0, 0)
        //     player.Update(emptyNeighborhood); // ~>* change
        //     Console.WriteLine(player.Position); // ~>* (0, -1)
        //     var targetToNorth = new Dictionary<Direction, GameObject>(emptyNeighborhood);
        //     targetToNorth[Direction.North] = new Target(new Position(0, -2));
        //     player.Update(targetToNorth); // ~>* change
        //     Console.WriteLine(player.Position); // ~>* (0, -2)
        //     var wallToNorth = new Dictionary<Direction, GameObject>(emptyNeighborhood);
        //     wallToNorth[Direction.North] = new Wall(new Position(0, -3));
        //     player.Update(wallToNorth); // ~>* change in direction
        //     Console.WriteLine(player.Position); // ~>* (0, -2)
        //     Console.WriteLine(player.Direction.Value); // ~>* Direction.South
        //     Agent agentPlayer = player;
        //     //agentPlayer.SetDirection(Direction.East); // compilation fails
        //     Crate crate = new Crate(new Position(1, -2));
        //     var agentToWestFacingAway = new Dictionary<Direction, GameObject>(emptyNeighborhood);
        //     agentToWestFacingAway[Direction.West] = player;
        //     crate.Update(agentToWestFacingAway); // ~>* no change (direction of the agent is not toward
        //     Console.WriteLine(crate.Position);
        //     var agentToNorthFacingTowards = new Dictionary<Direction, GameObject>(emptyNeighborhood);
        //     agentToNorthFacingTowards[Direction.North] = player; // the position of the objects is notcrate.Update(agentToNorthFacingTowards); // ~>* change (direction of the agent is towards tcrate.Position; // ~>* (1, -1)
        //     crate.Update(agentToNorthFacingTowards);
        //     Console.WriteLine(crate.Position); // ~>* (1, -1)
        //     player.Reset();
        //     Console.WriteLine(player.Repr()); // ~>* 'o'
        //     Console.WriteLine(player.Position); // ~>* (0, 0)
        //     Console.WriteLine(player.Direction.HasValue); // ~>* False
        //     crate.Reset();
        //     Console.WriteLine(crate.Position); // ~>* (1, -2)
        // }
    }
}