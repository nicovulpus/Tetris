using System;
using System.Diagnostics;
using System.Drawing;

namespace Tetris
{
    public class TetrominoBuilder
    {
        public Point[] Blocks { get; protected set; }
        public Color Color { get; protected set; }

        public int X { get;  set; } 
        public int Y { get; private set; }

        public bool IsLocked { get; protected set; }

        public TetrominoBuilder(Point[] blocks, Color color)
        {
            Blocks = blocks;
            Color = color;
            this.X = 0;
            this.Y = 0;
            IsLocked = false;
        }

        
        public void MoveLeft(TetrominoBuilder activeTetromino)
        {
            for (int i = 0; i < activeTetromino.Blocks.Length; i++)
            {
                activeTetromino.Blocks[i] = new Point(activeTetromino.Blocks[i].X - 1, activeTetromino.Blocks[i].Y);
            }
        }

        
        public void MoveRight(TetrominoBuilder activeTetromino)
        {
            for (int i = 0; i < activeTetromino.Blocks.Length; i++)
            {
                activeTetromino.Blocks[i] = new Point(activeTetromino.Blocks[i].X + 1, activeTetromino.Blocks[i].Y);
            }
        }

        
        public void MoveDown(TetrominoBuilder activeTetromino)
        {
            for (int i = 0; i < activeTetromino.Blocks.Length; i++)
            {
                activeTetromino.Blocks[i] = new Point(activeTetromino.Blocks[i].X, activeTetromino.Blocks[i].Y + 1);
            }
        }
        public virtual void RotateLeft(TetrominoBuilder shape)
        {
            Point pivot = Blocks[1]; 

            for (int i = 0; i < Blocks.Length; i++)
            {
                int relativeX = Blocks[i].X - pivot.X;
                int relativeY = Blocks[i].Y - pivot.Y;

                
                int newX = -relativeY + pivot.X;
                int newY = relativeX + pivot.Y;

                Blocks[i] = new Point(newX, newY);
            }
        }

        public virtual void RotateLeftUnsafe(TetrominoBuilder shape)
        {
            


        }
        public virtual List<Point> RotationCheck(TetrominoBuilder shape)
        {
            List<Point> rotatedPoints = new List<Point>(); 

            Point pivot = shape.Blocks[1]; 

            for (int i = 0; i < shape.Blocks.Length; i++)
            {
                int relativeX = shape.Blocks[i].X - pivot.X;
                int relativeY = shape.Blocks[i].Y - pivot.Y;

                int newX = -relativeY + pivot.X;
                int newY = relativeX + pivot.Y;

                rotatedPoints.Add(new Point(newX, newY));
            }

            return rotatedPoints;
        }

        public void LockTetromino()
        {
            IsLocked = true;
        }
        public void UnlockTetromino()
        {
            IsLocked = false;
        }
        
    }
}
