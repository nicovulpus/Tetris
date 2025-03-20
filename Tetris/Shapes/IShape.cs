using System.Drawing;
using Tetris.Properties;

namespace Tetris
{
    public class IShapes : TetrominoBuilder
    {
        public IShapes() : base(
            new Point[]
            {
                new Point(5, 0),
                new Point(5, 1),
                new Point(5, 2),
                new Point(5, 3)
            },
            Color.Yellow) // Specify the color for LShape
            {
            }
        

    }
}
