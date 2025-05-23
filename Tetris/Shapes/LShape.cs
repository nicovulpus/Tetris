using System.Drawing;
using Tetris.Properties;

namespace Tetris
{
    public class LShapes : TetrominoBuilder
    {
        public LShapes() : base(
            new Point[]
            {
                new Point(5, 0),
                new Point(5, 1),
                new Point(5, 2),
                new Point(6, 2)
            },
            Color.FromArgb(255, 182, 182))
            {
            }

        public override TetrominoBuilder Clone()
        {
            var clone = new LShapes();
            clone.Blocks = (Point[])this.Blocks.Clone();
            clone.Color = Color.FromArgb(80, this.Color); // Transparent ghost version
            return clone;
        }


    }
}
