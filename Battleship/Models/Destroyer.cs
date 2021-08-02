using Battleships.Enums;
using System.Collections.Generic;

namespace Battleships.Models
{
    public class Destroyer : IShip
    {
        public IList<Coordinates> Coordinates { get; set; } = new List<Coordinates>();

        public int Length { get; } = 4;

        public IList<BuildDirections> BuildDirections { get; set; } = new List<BuildDirections>();
    }
}
