using System.Drawing;
using Tetris.Properties;

namespace Tetris
{
    public class TShapes : TetrominoBuilder
    {
        public TShapes() : base(
            new Point[]
            {
                new Point(5, 0),
                new Point(5, 1),
                new Point(5, 2),
                new Point(6, 1)
            },
            Color.FromArgb(215, 189, 226)) 
            {
            }
        public override TetrominoBuilder Clone()
        {
            var clone = new TShapes();
            clone.Blocks = (Point[])this.Blocks.Clone();
            clone.Color = Color.FromArgb(80, this.Color); // Transparent ghost version
            return clone;
        }

    }
}
