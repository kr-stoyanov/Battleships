using Battleships.Enums;
using System.Collections.Generic;

namespace Battleships.Models
{
    public class Battleship : IShip
    {
        public IList<Coordinates> Coordinates { get; set; } = new List<Coordinates>();

        public int Length { get; } = 5;

        public IList<BuildDirections> BuildDirections { get; set; } = new List<BuildDirections>();
    }
}
