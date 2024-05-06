using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace pb006 {

    public class Game
    {
        private static readonly char[] AllowedMoves = {'a', 's', 'w', 'd', 'x', 'r', 'q'};
        private List<GameObject> objs;
        private Player player;
        private Target finish;
        private List<List<GameObject>> board;

        private Random random = new Random();

        public Game(int width, int height)
        {
            // int height = map.Length;
            // int width = 0;

            objs = new List<GameObject>();

            // for (int y = 0; y < map.Length; y++)
            // {
            //     width = Math.Max(width, map[y].Length);

            //     for (int x = 0; x < map[y].Length; x++)
            //     {
            //         Position newPosition = new Position(x, y);

            //         switch (map[y][x])
            //         {
            //             case '#': objs.Add(new Wall(newPosition)); break;
            //             case '.': objs.Add(new Target(newPosition)); break;
            //             case '^': objs.Add(new Agent(newPosition, Direction.North)); break;
            //             case '>': objs.Add(new Agent(newPosition, Direction.East)); break;
            //             case 'v': objs.Add(new Agent(newPosition, Direction.South)); break;
            //             case '<': objs.Add(new Agent(newPosition, Direction.West)); break;
            //             case 'o': objs.Add(new Player(newPosition)); break;
            //             case 'X': objs.Add(new Crate(newPosition)); break;
            //             case 'x': objs.Add(new Crate(newPosition)); objs.Add(new Target(newPosition)); break;
            //         }
            //     }
            // }

            InitBoard(width, height);
            FillMaze();
            SetObjectsToBoard();
        }

        public void InitBoard(int width, int height) {
            board = new List<List<GameObject>>();
            
            for (int y = 0; y < height; y++) {
                List<GameObject> row = new List<GameObject>();
                
                for (int x = 0; x < width; x++)
                    if (x == 0 || y == 0 || x == width - 1 || y == height -1) {
                        Position newPosition = new Position(x, y);
                        var o = new Wall(newPosition);
                        row.Add(o);
                        objs.Add(o);
                    }
                    else {
                        row.Add(null);
                    }
                
                board.Add(row);
            }

            player = new Player(new Position(1, 1));
            board[player.Position.y][player.Position.x] = player;
            objs.Add(player);
            
            finish = new Target(new Position(width - 2, height - 2));
            board[finish.Position.y][finish.Position.x] = finish;
            objs.Add(finish);
        }

        private void FillMaze() {
            for (int y = 2; y < board.Count - 2; y += 2) {
                for (int x = 2; x < board[0].Count - 2; x += 2) {
                    if (board[y][x] != null) {
                        continue;
                    }
                    FillLine(new Position(x, y));
                }
            }
        }

        private void FillLine(Position start) {
            var w = new Wall(start);
            objs.Add(w);
            board[start.y][start.x] = w;
            
            Array values = Enum.GetValues(typeof(Direction));
            Direction randomDir = (Direction)values.GetValue(random.Next(values.Length));

            int maxDist = 0;

            switch (randomDir) {
                case Direction.East: {
                    maxDist = start.x;
                    break;
                }
                case Direction.North: {
                    maxDist = start.y;
                    break;
                }
                case Direction.West: {
                    maxDist = board[0].Count - start.x;
                    break;
                }
                case Direction.South: {
                    maxDist = board.Count - start.y;
                    break;
                }
            }
           
            for (int i = 0; i < random.Next(maxDist); i++) {
                start = start.Step(randomDir);
                if (GetFromBoard(start) is Wall) {
                    return;
                }
                w = new Wall(start);
                objs.Add(w);
                board[start.y][start.x] = w;
            }
            
        }

        private void SetObjectsToBoard()
        {
            for (int y = 0; y < board.Count; y++)
            {
                for (int x = 0; x < board[0].Count; x++)
                    board[y][x] = null;
            }

            foreach (GameObject obj in objs.Where(o => !(o is Target)))
            {
                board[obj.Position.y][obj.Position.x] = obj;
            }

            foreach (GameObject obj in objs.Where(o => o is Target))
            {
                if (board[obj.Position.y][obj.Position.x] == null)
                    board[obj.Position.y][obj.Position.x] = obj;
            }
        }

        public bool GameEnded()
        {
            // List<Position> cratePositions = objs.Where(o => o is Crate).Select(c => c.Position).ToList();
            // List<Position> targetPositions = objs.Where(o => o is Target).Select(t => t.Position).ToList();

            // return cratePositions.Count == targetPositions.Count && cratePositions.All(targetPositions.Contains);
            return player.Position.Equals(finish.Position);
        }

        public void ShowBoard()
        {
            Console.Clear();

            foreach (var row in board)
            {
                foreach(GameObject obj in row)
                {
                    if (obj == null) {
                        Console.Write(" ");
                        Console.Write(" ");
                    }

                    else {
                        Console.Write(obj.Repr());
                        Console.Write(obj.Repr());
                    }
                }
                Console.Write("\n");
            }
        }

        private static char GetNextPlayerMove()
        {
            char input = Console.ReadKey().KeyChar;
            while (!AllowedMoves.Contains(input))
            {
                Console.WriteLine("Allowed moves are {0}", String.Join(", ", AllowedMoves));
                input = Console.ReadKey().KeyChar;
            }

            return input;
        }

        private static Direction? MoveToDirection(char move)
        {
            switch (move)
            {
                case 'w': return Direction.North;
                case 'd': return Direction.East;
                case 's': return Direction.South;
                case 'a': return Direction.West;
                case 'x': return null;
            }
            return null; // unreachable
        }

        public void SetPlayerDirection(Direction? dir)
        {
            foreach (var obj in objs)
            {
                if (obj is Player p)
                    p.SetDirection(dir);
            }
        }

        private GameObject GetFromBoard(Position pos)
        {
            if (0 <= pos.x && pos.x < board[0].Count && 0 <= pos.y && pos.y < board.Count)
                return board[pos.y][pos.x];
            return null;
        }

        public Dictionary<Direction, GameObject> GetNeighborhood(Position pos)
        {
            var neighborhood = new Dictionary<Direction, GameObject>();

            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                neighborhood[dir] = GetFromBoard(pos.Step(dir));

            return neighborhood;
        }

        public bool AgentsCollide()
        {
            foreach (GameObject obj in objs)
            {
                if (obj is Agent a)
                {
                    foreach (var entry in GetNeighborhood(a.Position))
                    {
                        if (entry.Value is Agent o) {
                            Direction? thisDir = a.Direction;
                            Direction? otherDir = o.Direction;

                            if (thisDir != null && entry.Key == thisDir.Value)
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        public void Update()
        {
            foreach (GameObject obj in objs.Where(o => o is Crate))
            {
                obj.Update(GetNeighborhood(obj.Position));
            }
            SetObjectsToBoard();

            foreach (GameObject obj in objs.Where(o => !(o is Crate)))
            {
                obj.Update(GetNeighborhood(obj.Position));
            }
            SetObjectsToBoard();
        }

        public void Reset()
        {
            foreach (GameObject obj in objs)
                obj.Reset();
            SetObjectsToBoard();
        }

        public static void GameLoop(Game g)
        {
            while (!g.GameEnded())
            {
                g.ShowBoard();

                char move = GetNextPlayerMove();
                if (move == 'r') {
                    g.Reset();
                    continue;
                } 
                else if (move == 'q') {
                    return;
                }
                g.SetPlayerDirection(MoveToDirection(move));

                if (g.AgentsCollide())
                    break;

                g.Update();
            }

            g.ShowBoard();
            if (g.AgentsCollide())
                Console.WriteLine("Agents collided.");
            else
                Console.WriteLine("Solved!");
        }

        public static void Main()
        {
            string[] easy = {
                "###",
                "#.#",
                "#X#",
                "#o#",
                "###",
            };
            string[] medium = {
                "  #####",
                "###   #",
                "#.oX  #",
                "### X.#",
                "#.##X #",
                "# # . ##",
                "#X xXX.#",
                "#   .  #",
                "########",
            };
            string[] hard = {
                "    #####",
                "    #   #",
                "    #X  #",
                "  ###  X##",
                "  #  X X #",
                "### # ## #   ######",
                "#   # ## #####  ..#",
                "# X  X          ..#",
                "##### ### #o##  ..#",
                "    #     #########",
                "    #######",
            };
            string[] collaborative = {
            // agents may overlap for a moment
            // the behavior of a crate pushed by multiple
            // agents will not be tested
                "       ###",
                "########o#",
                "#>X      #",
                "######## #",
                "       #.#",
                "       ###",
            
            };
            string[] chase = {
                "##################",
                "#> oX           .#",
                "##################",
            };
            string[] gap = {
                " ###",
                " # #",
                "## #####",
                "#ovX  .#",
                "########"
            };

            GameLoop(new Game(21, 21));
        }
    }
}