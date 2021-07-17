using System;
using System.Linq;
using System.Collections.Generic;
using System.Media;

namespace Battleship
{
    public class Program
    {
        static int score;
        static int round;
        static int objIdx;
        static char[][] map;
        static char[] rows;
        static int[] columns;
        static char[] objects;
        static List<Coordinates> moves;
        static Coordinates revealed;
        static List<Coordinates> coordinates;
        static string successfulAttack;
        static string bomb;
        static string unsuccessfulAttack;

        public static void Main()
        {
            Console.Title = "BattleShip";

            rows = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G' };
            columns = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            objects = new char[] { '_', '*', '>' }; // [_] blank, [*] mine, [>] ship

            map = new char[7][];
            moves = new List<Coordinates>();
            coordinates = new List<Coordinates>();

            successfulAttack = @"assets\mixkit-fuel-explosion-1705.wav";
            bomb = @"assets\mixkit-sea-mine-explosion-1184.wav";
            unsuccessfulAttack = @"assets\mixkit-jump-into-the-water-1180.wav";

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
            score = 0;
            round = 1;

            moves.Clear();
            coordinates.Clear();
            revealed.Col = 0;
            revealed.Row = '\0';
            revealed.Obj = '\0';

            while (true)
            {
                DrawBoard(revealed);

                Console.WriteLine();
                Console.WriteLine($"Score: {score}");
                Console.WriteLine($"Round: {round}");
                Console.WriteLine();
                Console.Write($"Enter Coordinates to attack(e.g [A, 9]): ");

                string[] userInput = Console.ReadLine()
                .Split(new char[] { ' ', ',', '-', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToUpper())
                .ToArray();

                if (IsValidInput(userInput))
                {
                    revealed = RevealAttackedObject(userInput);

                    if (revealed.Obj == objects[1])
                    {
                        PlaySound(bomb);
                        DrawBoard(revealed);
                        Console.WriteLine();
                        Console.WriteLine("         GAME OVER!");
                        GameOver();
                        break;
                    }
                    else if (revealed.Obj == objects[2])
                    {
                        PlaySound(successfulAttack);
                        score++;
                    }
                    else
                    {
                        PlaySound(unsuccessfulAttack);
                    }

                    round++;
                }
            }
        }

        private static void DrawBoard(Coordinates revealed)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("[==========BATTLESHIP==========]");
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
                    coordinates.Add(new Coordinates(rows[i], j, objects[objIdx]));
                    var move = CheckMove(rows[i], j);

                    if (revealed.Obj != '\0' && (revealed.Row == rows[i] && revealed.Col == j))
                    {
                        ProcessStep(revealed);
                        moves.Add(revealed);
                    }
                    else if (move.Obj != '\0')
                    {
                        ProcessStep(move);
                    }
                    else Console.Write(" . ");
                }
                Console.WriteLine();
            }
        }

        private static Coordinates RevealAttackedObject(string[] userInput)
        {
            var attackedObject = coordinates.FirstOrDefault(x => x.Row == char.Parse(userInput[0]) && x.Col == int.Parse(userInput[1]));

            return attackedObject;
        }

        private static void GameOver()
        {
            string points = score == 1 ? "point" : "points";

            Console.WriteLine();
            Console.WriteLine($"   You have died in round {round}\n" +
                              $"         with {score} {points}!");
            Console.WriteLine("     Good luck next time!");
            Console.WriteLine();
        }

        private static bool IsValidInput(string[] userInput)
        {
            if (userInput.Length != 2) return false;

            if (!rows.ToList().Contains(char.Parse(userInput[0])) || !columns.ToList().Contains(int.Parse(userInput[1]))) return false;

            return true;
        }

        private static Coordinates CheckMove(char row, int col)
        {
            return moves.FirstOrDefault(x => x.Row == row && x.Col == col);
        }

        private static void ProcessStep(Coordinates step)
        {
            switch (step.Obj)
            {
                case '>':
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($" {step.Obj} ");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case '*':
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write($" {step.Obj} ");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case '_':
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($" {step.Obj} ");
                    break;

                default:
                    break;
            }
        }

        private static void PlaySound(string file)
        {
            SoundPlayer player = new();
            player.SoundLocation = $".\\{file}";
            player.Play();
        }
    }

    public struct Coordinates
    {
        public Coordinates(char row, int col, char obj)
        {
            Row = row;
            Col = col;
            Obj = obj;
        }
        public char Row { get; set; }

        public int Col { get; set; }

        public char Obj { get; set; }
    }
}
