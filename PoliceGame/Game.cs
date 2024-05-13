using System;
using System.Collections.Generic;


namespace pb006
{

    public abstract class GameObject
    {
        public char objectRepr;
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



        public abstract void Update(Dictionary<Direction, GameObject>  inputDir, List<List<GameObject>> board);
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

    public class Marker : GameObject
    {
        public override void Update(Dictionary<Direction, GameObject>  inputDir, List<List<GameObject>> board){
            return;
        }

        public Marker(char ch) {
            objectRepr = ch;
        }

    }
    public abstract class StaticObject : GameObject
    {
        public new bool? IsMoving() {
            return false;
        }
        public override void Update(Dictionary<Direction, GameObject>  inputDir, List<List<GameObject>> board){
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

        public Direction BFS(List<List<GameObject>> board) {
            
            Queue<Position> queue = new();
            int width = board.Count();
            int height = board[0].Count();
            bool[,] visited = new bool[width, height];
            int [,] values = new int[width, height];

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    values[x,y] = 999999;
                }
            }

            queue.Enqueue(Position);
            visited[Position.x, Position.y] = true;
            values[Position.x, Position.y] = 0;



            while (queue.Count > 0) {
                Position currentPos = queue.Dequeue();
                foreach (Direction dir in Enum.GetValues(typeof(Direction))) {
                    Position possiblePos = currentPos.Step(dir);
                    if (board[possiblePos.y][possiblePos.x] is Player) {
                        visited[possiblePos.x, possiblePos.y] = true;
                        values[possiblePos.x, possiblePos.y] = values[currentPos.x, currentPos.y] + 1;
                        return ShortestPath(values, possiblePos);
                    }
                    if (board[possiblePos.y][possiblePos.x] is not Wall && visited[possiblePos.x, possiblePos.y] == false) {
                        queue.Enqueue(possiblePos);
                        visited[possiblePos.x, possiblePos.y] = true;
                        values[possiblePos.x, possiblePos.y] = values[currentPos.x, currentPos.y] + 1;
                        //board[possiblePos.x][possiblePos.y] = new Marker((char) values[possiblePos.x, possiblePos.y]);
                    }
                }
            }

            return pb006.Direction.North;

        }

        public Direction ShortestPath(int[,] values, Position currPos) {
            foreach (Direction dir in Enum.GetValues(typeof(Direction))) {
                Position possiblePos = currPos.Step(dir);
                if (values[possiblePos.x, possiblePos.y] == 0) {
                    return dir.Opposite();
                }
                if (values[currPos.x, currPos.y] == values[possiblePos.x, possiblePos.y] + 1) {
                    return ShortestPath(values, new Position(possiblePos.x, possiblePos.y));
                }
            }
            return pb006.Direction.West;
        }

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
            //objectRepr = Direction == null ? 'Ѽ' : Direction.Value.ToSymbol();
            objectRepr = '¥';
        }

        public override void Update(Dictionary<Direction, GameObject>  inputDir, List<List<GameObject>> board)
        {
            Direction currDirecton;
            Position = Position.Step(BFS(board));
            return;
            if (Direction != null) {
                currDirecton = (Direction) Direction;
            }
            else {
                return;
            }
            if (inputDir[currDirecton] == null || inputDir[currDirecton].Repr() == '.') {
                Position = Position.Step(BFS(board));
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

        public override void Update(Dictionary<Direction, GameObject>  inputDir, List<List<GameObject>> board)
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
        public override void Update(Dictionary<Direction, GameObject>  inputDir, List<List<GameObject>> board)
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