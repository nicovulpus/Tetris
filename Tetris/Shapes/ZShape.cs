using System.Drawing;
using Tetris.Properties;

namespace Tetris
{
    public class ZShapes : TetrominoBuilder
    {
        public ZShapes() : base(
            new Point[]
            {
                new Point(5, 0),
                new Point(5, 1),
                new Point(5, 2),
                new Point(6, 2)
            },
            Color.White) // Specify the color for LShape
            {
            }
        

    }
}
