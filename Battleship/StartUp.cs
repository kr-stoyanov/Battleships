using System;
using System.Linq;
using System.Media;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Battleships.Models;
using Battleships.Enums;

namespace Battleships
{
    public class Program
    {
        static int round;
        static int objIdx;
        static string pattern;
        static char[][] map;
        static char[] rows;
        static int[] columns;
        static IList<Coordinates> moves;
        static Coordinates revealed;
        static IList<Coordinates> coordinates;
        static IList<int> nums;
        static IList<int> chars;
        static IShip[] ships;
        static string hitSound;
        static string missSound;

        public static void Main()
        {
            Console.Title = "BattleShip";

            rows = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' };
            columns = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            nums = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            chars = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            pattern = @"([a-j]|[A-J])(([1-9]|10)$)";

            map = new char[10][];
            moves = new List<Coordinates>();
            coordinates = new List<Coordinates>();

            hitSound = @"assets\mixkit-fuel-explosion-1705.wav";
            missSound = @"assets\mixkit-jump-into-the-water-1180.wav";

            ships = new IShip[]
            {
                new Battleship(),
                new Destroyer(),
                new Destroyer(),
            };

            InitializeShips(ships);
            PrintCoordinates(ships);
            LoadMainMenu();
        }

        private static void LoadMainMenu()
        {
            int key = -1;

            while (key != 0)
            {
                Console.WriteLine("[========== Main Menu ==========]");
                Console.WriteLine();
                Console.WriteLine("1. Start New Game");
                Console.WriteLine("0. Exit Game");
                Console.WriteLine();
                Console.Write("> Select: ");

                if (int.TryParse(Console.ReadLine(), out key))
                {
                    switch (key)
                    {
                        case 1:
                            StartGame();
                            break;

                        case 2:
                            Environment.Exit(0);
                            break;

                        default:
                            break;
                    }
                }
            }

        }

        private static void StartGame()
        {
            //Clear all objects and reset stats before each new game.
            round = 1;

            moves.Clear();
            coordinates.Clear();
            revealed.Col = 0;
            revealed.Row = '\0';

            while (true)
            {
                DrawBoard(revealed);

                Console.WriteLine();
                Console.WriteLine($"Round: {round}");
                Console.WriteLine();
                Console.Write($"Enter Coordinates to attack(e.g [A9]): ");

                string userInput = Console.ReadLine();
                Match match = Regex.Match(userInput, pattern);

                if (match.Success)
                {
                    var row = char.Parse(match.Groups[1].Value.ToUpper());
                    var col = int.Parse(match.Groups[2].Value);

                    revealed = RevealAttackedObject(userInput, row, col);

                    //if (revealed.Obj == objects[1])
                    //{
                    //    DrawBoard(revealed);
                    //    Console.WriteLine();
                    //    Console.WriteLine("         GAME OVER!");
                    //    GameOver();
                    //    break;
                    //}

                    //var a = ships.FirstOrDefault(x => x.Coordinates.First(x => x.Row == row && x.Col == col));
                    if (revealed.Row == 'x')
                    {
                        PlaySound(hitSound);
                    }
                    else
                    {
                        PlaySound(missSound);
                    }

                    round++;
                }
            }
        }

        private static void DrawBoard(Coordinates revealed)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("[==========BATTLESHIPS==========]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();

            Random rnd = new();

            Console.WriteLine($"  {string.Join("  ", columns)}");

            for (int i = 0; i < map.Length; i++)
            {
                map[i] = new char[11];
                Console.Write($"{rows[i]  }");

                for (int j = 1; j < map[i].Length; j++)
                {
                    objIdx = rnd.Next(0, 3);
                    coordinates.Add(new Coordinates(rows[i], j, '_'));
                    var move = CheckMove(rows[i], j);

                    if (revealed.Obj != '\0' && (revealed.Row == rows[i] && revealed.Col == j))
                    {
                        moves.Add(revealed);
                    }
                    else if (move.Obj != '\0')
                    {
                       // ProcessStep(move);
                    }
                    else Console.Write(" . ");
                }
                Console.WriteLine();
            }
        }

        private static Coordinates RevealAttackedObject(string userInput, char row, int col)
        {
            var attackedObject = coordinates.FirstOrDefault(x => x.Row == row && x.Col == col);

            return attackedObject;
        }

        private static void GameOver()
        {
           
            Console.WriteLine();
            Console.WriteLine($"   You have died in round {round}\n");
            Console.WriteLine("     Good luck next time!");
            Console.WriteLine();
        }

        private static Coordinates CheckMove(char row, int col)
        {
            return moves.FirstOrDefault(x => x.Row == row && x.Col == col);
        }

        private static void PlaySound(string file)
        {
            SoundPlayer player = new();
            player.SoundLocation = $".\\{file}";
            player.Play();
        }

        private static void InitializeShips(IShip[] ships)
        {
            foreach (var ship in ships)
            {
                CheckPossibleBuildDirections(ship);
                BuildShip(ship);
            }
        }

        private static void CheckPossibleBuildDirections(IShip ship)
        {
            Random rnd = new();
            int colIdx = nums.OrderBy(x => rnd.Next()).First();
            int rowIdx = chars.OrderBy(x => rnd.Next()).First();

            nums.Remove(colIdx);
            chars.Remove(rowIdx);

            //Add starting building position
            ship.Coordinates.Add(new Coordinates(rows[rowIdx], columns[colIdx], 'x'));

            if (rows[rowIdx] + (ship.Length - 1) < 'K')   ship.BuildDirections.Add(BuildDirections.Down);
            if (rows[rowIdx] - (ship.Length - 1) >= 'A')  ship.BuildDirections.Add(BuildDirections.Up);
            if (columns[colIdx] + (ship.Length - 1) < 11) ship.BuildDirections.Add(BuildDirections.Right);
            if (columns[colIdx] - (ship.Length - 1) >= 0) ship.BuildDirections.Add(BuildDirections.Left);
        }

        private static void BuildShip(IShip ship)
        {
            //TODO Fix Rows issues

            Random rnd = new();

            var buildDirection = ship.BuildDirections.OrderBy(x => rnd.Next()).First();
            int idx = 0;

            switch (buildDirection.ToString())
            {
                case "Right": //Build Right 
                    idx = ship.Coordinates[0].Col + 1;
                    for (int i = idx; i < (idx + ship.Length); i++)
                    {
                        ship.Coordinates.Add(new Coordinates(rows[idx], columns[i], 'x'));
                    }
                    break;

                case "Left": //Build Left
                    idx = ship.Coordinates[0].Col - 1;
                    for (int i = idx; i > (idx - ship.Length); --i)
                    {
                        ship.Coordinates.Add(new Coordinates(rows[idx], columns[i], 'x'));
                    }
                    break;

                case "Down": //Build Down 
                    idx = ship.Coordinates[0].Row + 1;
                    for (int i = idx; i < (idx + ship.Length); i++)
                    {
                        ship.Coordinates.Add(new Coordinates(rows[i], columns[idx], 'x'));
                    }
                    break;

                case "Up": //Build Up 
                    idx = ship.Coordinates[0].Row - 1;
                    for (int i = idx; i > (idx - ship.Length); --i)
                    {
                        ship.Coordinates.Add(new Coordinates(rows[i], columns[idx], 'x'));
                    }
                    break;

                default:
                    break;
            }

        }

        private static void PrintCoordinates(IShip[] ships)
        {
            foreach (var s in ships)
            {
                Console.WriteLine($"{string.Join(' ', s.Coordinates)}");
            }
        }

    } 
}
