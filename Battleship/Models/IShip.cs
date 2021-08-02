using Battleships.Enums;
using System.Collections.Generic;

namespace Battleships.Models
{
    public interface IShip
    {
        IList<Coordinates> Coordinates { get; set; }

        int Length { get; }

        IList<BuildDirections> BuildDirections {get; set;}
    }
}
