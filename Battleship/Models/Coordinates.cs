namespace Battleships.Models
{
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
