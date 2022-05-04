using System;
using System.Linq;
using System.Media;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Battleships
{
    public class Program
    {
        static int round;
        static string pattern;
        static char[][] map;
        static char[] rows;
        static int[] columns;
        static char targetHit;
        static List<Battleship> battleships;
        static List<Coordinates> moves;
        static Coordinates revealed;
        static string successfulAttackSound;
        static string unsuccessfulAttackSound;
        static string message;

        const int NumberOfShips = 3;

        public static void Main()
        {
            Console.Title = "Battleships";

            targetHit = 'x';
            pattern = @"([a-j]|[A-J])(([1-9]|10)$)";
            columns = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            rows = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' };

            map = new char[10][];
            moves = new List<Coordinates>();
            battleships = new List<Battleship>(NumberOfShips);

            successfulAttackSound = @"assets\mixkit-fuel-explosion-1705.wav";
            unsuccessfulAttackSound = @"assets\mixkit-jump-into-the-water-1180.wav";

            LoadMainMenu();
        }

        private static void CreateBattleships()
        {
            battleships.Clear();
            var random = new Random();
            int length = 4;

            for (int i = 0; i < NumberOfShips; i++)
            {
                if (i == 2) length++;
                int randomNumber = random.Next(1, 100);
                if (randomNumber % 2 == 0)  PositionHorizontally(length, i);
                else                        PositionVertically(length, i);
            }
        }

        private static void PositionVertically(int length, int index)
        {
            battleships.Add(new Battleship(length));
            int start, end = 0;
            Random rnd = new();
            int random = rnd.Next(0, columns.Length);
            int columnPosition = columns[random];

            while (battleships.Any(x => x.Coordinates != null) && battleships.Any(x => x.Coordinates.Any(s => s.Col == columnPosition || s.Row == columnPosition)))
            {
                random = rnd.Next(0, columns.Length);
                columnPosition = columns[random];
            }


            if (random + length > columns.Length && random - length >= 0)
            {
                start = random - length;
                end = random;

                for (int i = start; i < end; i++)
                {
                    battleships[index].Coordinates.Add(new Coordinates(rows[i], columnPosition, targetHit));
                }
            }
            else
            {
                start = random;
                end = random + length;

                for (int i = start; i < end; i++)
                {
                    battleships[index].Coordinates.Add(new Coordinates(rows[i], columnPosition, targetHit));
                }
            }
        }

        private static void PositionHorizontally(int length, int index)
        {
            battleships.Add(new Battleship(length));
            int start, end = 0;
            Random rnd = new();
            int random = rnd.Next(0, rows.Length);
            char rowPosition = rows[random];
            
            while (battleships.Any(x => x.Coordinates != null) && battleships.Any(x => x.Coordinates.Any(s => s.Row == rowPosition || s.Col == rowPosition)))
            {
                random = rnd.Next(0, rows.Length);
                rowPosition = rows[random];
            }

            if (random + length > rows.Length && random - length >= 0)
            {
                start = random - length;
                end = random;

                for (int i = start; i < end; i++)
                {
                    battleships[index].Coordinates.Add(new Coordinates(rowPosition, columns[i], targetHit));
                }
            }
            else
            {
                start = random;
                end = random + length;

                for (int i = start; i < end; i++)
                {
                    battleships[index].Coordinates.Add(new Coordinates(rowPosition, columns[i], targetHit));
                }
            }
        }

        private static void LoadMainMenu()
        {
            CreateBattleships();

            int key = -1;

            while (key != 9)
            {
                Console.WriteLine("[========== Main Menu ==========]");
                Console.WriteLine();
                Console.WriteLine("1. Start New Game");
                Console.WriteLine("9. Exit Game");
                Console.WriteLine();
                Console.Write("> Select: ");

                if (int.TryParse(Console.ReadLine(), out key))
                {
                    switch (key)
                    {
                        case 1:  StartGame(); break;
                        case 9:  Environment.Exit(0); break;
                        default: 
                                 Console.Clear();
                                 Console.WriteLine($"Command: [{key}] not supported!"); 
                        break;
                    }
                }
            }
        }

        private static void StartGame()
        {
            //Clear all objects and reset stats before each new game.
            round = 0;

            moves.Clear();
            revealed = null;
            message = "";

            while (true)
            {
                DrawBoard(revealed);
                revealed = null;
                Console.WriteLine();
                Console.WriteLine($"Moves: {round}");
                Console.WriteLine(message);
                Console.Write($"Enter Coordinates to attack(e.g [A9]): ");

                if (battleships.All(x => x.Coordinates.All(x => x.IsHit)))
                {
                    DrawBoard(revealed);
                    GameOver();
                    LoadMainMenu();
                    break;
                }

                string userInput = Console.ReadLine();
                if (IsValidInput(userInput))
                {
                    revealed = RevealAttackedObject(userInput);

                    if (revealed.Obj == targetHit)
                    {
                        PlaySound(successfulAttackSound);
                        var thisShip = battleships
                                            .FirstOrDefault(x => x.Coordinates.Any(c => c.Row == revealed.Row && c.Col == revealed.Col));

                        foreach (var coord in thisShip.Coordinates)
                            if (coord.Row == revealed.Row && coord.Col == revealed.Col)
                                coord.IsHit = true;

                        message = thisShip.Coordinates.All(x => x.IsHit) ? "***Sunk***" : "***Hit***";
                    }
                    else
                    {
                        PlaySound(unsuccessfulAttackSound);
                        revealed.Obj = '_';
                        message = "***Miss***";
                    }

                    round++;
                }

                if (userInput.ToLower() == "show")
                {
                    DrawRevealedBoard();
                    Console.WriteLine();
                    LoadMainMenu();
                }
            }
        }

        private static void DrawRevealedBoard()
        {
            PrintTitle();
            Console.WriteLine($"  {string.Join("  ", columns)}");

            for (int i = 0; i < map.Length; i++)
            {
                map[i] = new char[11];
                Console.Write($"{rows[i]  }");

                for (int j = 1; j < map[i].Length; j++)
                {
                    var move = CheckMoveOnShow(rows[i], j);
                    if (move != null) ProcessStep('x');
                    else ProcessStep('_'); 
                }

                Console.WriteLine();
            }
        }

        private static void DrawBoard(Coordinates revealed)
        {
            PrintTitle();
            Console.WriteLine($"  {string.Join("  ", columns)}");

            for (int i = 0; i < map.Length; i++)
            {
                map[i] = new char[11];
                Console.Write($"{rows[i]  }");

                for (int j = 1; j < map[i].Length; j++)
                {
                    var move = CheckMove(rows[i], j);

                    if (revealed != null && (revealed.Row == rows[i] && revealed.Col == j))
                    {
                        switch (revealed.Obj)
                        {
                            case 'x': ProcessStep(revealed.Obj); break;
                            case '_': ProcessStep('_'); break;
                            default: break;
                        }
                    }
                    else if (move != null && move.Row == rows[i] && move.Col == j)
                    {
                        ProcessStep(move.Obj);
                    }
                    else Console.Write(" . ");

                    if (revealed != null) moves.Add(revealed);
                    else moves.Add(new Coordinates());
                }
                Console.WriteLine();
            }
        }

        private static void PrintTitle()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("[==========BATTLESHIPS==========]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
        }

        private static Coordinates RevealAttackedObject(string userInput)
        {
            Match match = Regex.Match(userInput, pattern);
            var row = char.Parse(match.Groups[1].Value.ToUpper());
            var col = int.Parse(match.Groups[2].Value);

            var attackedShip = battleships.Where(x => x.Coordinates.Any(c => c.Row == row && c.Col == col)).FirstOrDefault();
            
            if (attackedShip == null) return new Coordinates(row, col, '\0');
            return attackedShip.Coordinates.First(x => x.Row == row && x.Col == col);
        }

        private static void GameOver()
        {
            battleships.Clear();
            Console.WriteLine();
            Console.WriteLine("         GAME OVER!");
            Console.WriteLine();
            Console.WriteLine($"You completed the game in {round} moves!");
            Console.WriteLine();
        }

        private static bool IsValidInput(string userInput)
        {
            Match match = Regex.Match(userInput, pattern);
            return match.Success;
        }

        private static Coordinates CheckMove(char row, int col) =>  moves.FirstOrDefault(x => x.Row == row && x.Col == col);
         
        private static Coordinates CheckMoveOnShow(char row, int col)
        {
            var result = battleships
                           .FirstOrDefault(x => x.Coordinates.Any(c => c.Row == row && c.Col == col));

            return result?.Coordinates.FirstOrDefault(x => x.Row == row && x.Col == col);
        }

        private static void ProcessStep(char stepObj)
        {
            switch (stepObj)
            {
                case 'x':
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($" {stepObj} ");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case '_':
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write($" {stepObj} ");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                default: break;
            }
        }

        private static void PlaySound(string file)
        {
            SoundPlayer player = new();
            player.SoundLocation = $".\\{file}";
            player.Play();
        }
    }

    internal class Coordinates
    {
        public Coordinates()
        { }
        public Coordinates(char row, int col, char obj, bool isHit = false)
        {
            Row = row;
            Col = col;
            Obj = obj;
            IsHit = isHit;
        }

        public char Row { get; set; }

        public int Col { get; set; }

        public char Obj { get; set; }

        public bool IsHit { get; set; }
    }

    internal class Battleship
    {
        public Battleship()
        { }
        public Battleship(int length)
        {
            Coordinates = new List<Coordinates>(length);
        }

        public List<Coordinates> Coordinates { get; set; }
    }
}
