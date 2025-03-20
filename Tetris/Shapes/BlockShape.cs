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
            Color.Orange)
            {
            }
        

    }
}
