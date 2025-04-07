using System.Drawing;
using Tetris.Properties;

namespace Tetris
{
    public class BShapes : TetrominoBuilder
    {
        public BShapes() : base(
            new Point[]
        {
            new Point(5, 0),
            new Point(5, 1),
            new Point(6, 0),
            new Point(6, 1)
        },
            Color.FromArgb(255, 249, 176)
                                )
        {
            }
        public override TetrominoBuilder Clone()
        {
            var clone = new BShapes();
            clone.Blocks = (Point[])this.Blocks.Clone();
            clone.Color = Color.FromArgb(80, this.Color); // Transparent ghost version
            return clone;
        }

    }
}
