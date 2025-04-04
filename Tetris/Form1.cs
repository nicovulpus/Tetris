using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Diagnostics;
using Accessibility;
using System.Net.NetworkInformation;
using System.Diagnostics.Eventing.Reader;

namespace Tetris
{
    public partial class Form1 : Form
    {
        //List over all tetrominoes
        private List<TetrominoBuilder> lockedTetrominoes = new List<TetrominoBuilder>();

        // Initialize grid
        private const int gridWidth = 10;
        private const int gridHeight = 20;
        private const int cellSize = 30;
        private int[,] grid;
        private Color[,] gridColors;
        private TetrominoBuilder activeTetromino;
        private bool isLeftKeyPressed = false;
        private bool isRightKeyPressed = false;
        private bool isUpKeyPressed = false;
        private bool isDownKeyPressed = false;
        private bool isZKeyPressed = false;
        private bool isXKeyPressed = false;
        private int score = 0;
        private TetrominoBuilder nextTetromino;


        // Set timer 
        private System.Windows.Forms.Timer gameTimer;


        // Define GameLoop
        private void GameLoop(object sender , EventArgs e)
        {
                UpdateGame();
                Invalidate();
        }

        // Define Form1 
        public Form1()
        {

            // Set up the Client
            InitializeComponent();
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.DoubleBuffered = true;
            this.ClientSize = new Size(gridWidth * cellSize + 130, gridHeight * cellSize);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Set up a timer
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 150;
            gameTimer.Tick += new EventHandler(GameLoop);
            gameTimer.Start();
            
            //Spawn Tetromino
            activeTetromino = SpawnTetromino();
           
            // Initialize grid
            grid= new int[gridWidth, gridHeight];
            
            gridColors = new Color[gridWidth, gridHeight];
            
            // Populate the grid with zeroes
            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    grid[col, row] = 0; // Initialize all cells to 0
                }
            }

            // Make the active tetromino fall down 
            FallDownTetromino(activeTetromino);

            // Set up Eventlisteners for player input
            this.KeyDown += new KeyEventHandler(KeyDownLeft);
            this.KeyUp += new KeyEventHandler(KeyUpLeft);
            this.KeyDown += new KeyEventHandler(KeyDownRight);
            this.KeyUp += new KeyEventHandler(KeyUpRight);
            this.KeyDown += new KeyEventHandler(KeyDownZ);
            this.KeyUp += new KeyEventHandler(KeyUpZ);
            this.KeyDown += new KeyEventHandler(KeyDownDown);
            this.KeyUp += new KeyEventHandler(KeyUpDown);




        }

        // OnPaint method to paint in the grid and the blocks
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //Draw Grid
            DrawGrid(e.Graphics);

            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    if (grid[col, row] != 0)
                        using (SolidBrush brush = new SolidBrush(gridColors[col, row]))
                        {
                            e.Graphics.FillRectangle(brush, col * cellSize, row * cellSize, cellSize, cellSize);
                            e.Graphics.DrawRectangle(Pens.White, col * cellSize, row * cellSize, cellSize, cellSize);
                        }
                }
            }

            if (activeTetromino != null)
            {
                DrawTetromino(e.Graphics, activeTetromino);
            }
            using (Font font = new Font("Arial", 8))
            using (SolidBrush brush = new SolidBrush(Color.FloralWhite))
            {
                e.Graphics.DrawString($"Score: {score}", font, brush, new PointF(320, 20));
                e.Graphics.DrawString("Next tetromino", font, brush, new PointF(310, 70));

            }
            DrawNextTetromino(e.Graphics, nextTetromino);

        }



        //Spawn Tetromino 
        public TetrominoBuilder SpawnTetromino()
        {
            activeTetromino = nextTetromino ?? PickTetromino();
            nextTetromino = PickTetromino(); 
            return activeTetromino;
        }

        private void DrawNextTetromino(Graphics g, TetrominoBuilder next)
        {
            using (SolidBrush brush = new SolidBrush(next.Color))
            {
                foreach (Point block in next.Blocks)
                {
                    int x = 290 + block.X * cellSize / 2; 
                    int y = 120 + block.Y * cellSize / 2;
                    g.FillRectangle(brush, x, y, cellSize / 2, cellSize / 2);
                    g.DrawRectangle(Pens.White, x, y, cellSize / 2, cellSize / 2);
                }
            }
        }


        // Draw Grid Method
        private void DrawGrid(Graphics g)
        {
            Pen gridPen = new Pen(Color.Gray, 1);

            for (int row = 0; row < gridHeight ; row++)
            {
                for (int col = 0; col < gridWidth ; col++)
                {
                    int x = col * cellSize;
                    int y = row * cellSize;
                    g.DrawRectangle(gridPen, x, y, cellSize, cellSize);
                  

                }
            }
        }

        // Draw Tetromino Method
        public void DrawTetromino(Graphics g, TetrominoBuilder shape)
        {
            //  Here I want to draw the selected shape on top of the grid
            using (SolidBrush brush = new SolidBrush(shape.Color))
            {
                if (!shape.IsLocked)
                    foreach (Point block in shape.Blocks)
                    {
                        int x = block.X * cellSize;
                        int y = block.Y * cellSize;
                        g.FillRectangle(brush, x, y, cellSize, cellSize);
                        g.DrawRectangle(Pens.White, x, y, cellSize, cellSize);

                    }
            }
        }

        public TetrominoBuilder PickTetromino()
        {
            // Here I want to randomly pick a shape
            Random random = new Random();
            int number = random.Next(0, 5);

            switch (number)
            {
                case 0:
                    return new BShapes();

                case 1:
                    return new IShapes();

                case 2:
                    return new LShapes();

                case 3:
                    return new TShapes();

                case 4:
                    return new ZShapes();

                default:
                    return new BShapes();

            }
        }
        private void LockTetromino(TetrominoBuilder shape)
        {
            
              foreach ( Point block in shape.Blocks)
                {
                    grid[block.X, block.Y] = 1;
                    gridColors[block.X, block.Y] = shape.Color;
                    
                }
              shape.LockTetromino();
        }



        private void FallDownTetromino(TetrominoBuilder active)
        {
            if(active.IsLocked)
            {
                return;
            }
            bool willBeOutofBounds = false;
            foreach( Point block in active.Blocks )
            {
                if (block.Y + 1 >= gridHeight || grid[block.X, block.Y + 1] != 0)
                {
                    willBeOutofBounds = true;
                    LockTetromino(active);
                }
            }
            if (!willBeOutofBounds)
            {
                active.MoveDown(active);
            }
            
        }

        private void MoveLeft(TetrominoBuilder shape)
        {
            if ( shape.IsLocked )
            {
                return;
            }
            bool willBeOutside = false;
            if(isLeftKeyPressed)
            {
                foreach (Point block in shape.Blocks)
                {
                    if (block.X < 1 || grid[block.X - 1 , block.Y ] != 0 )
                    {
                        willBeOutside = true;
                        break;
                    }
                }
                if (!willBeOutside)
                {
                    shape.MoveLeft(shape);
                    Invalidate();
                }
            }
        }

        private bool SafeRotation(TetrominoBuilder shape)
        {
            List<Point> points = shape.RotationCheck(shape);

            foreach (Point point in points)
            {
                // Check if the new position is out of bounds
                if (point.X < 0 || point.X >= gridWidth || point.Y < 0 || point.Y >= gridHeight)
                {
                    return false; // Rotation is not safe
                }

                // Check if the new position is occupied by another piece
                if (grid[point.X, point.Y] == 1)
                {
                    return false; // Rotation is not safe
                }
            }

            return true; // Rotation is safe
        }
        private void MoveRight(TetrominoBuilder shape)
        {
            if (shape.IsLocked)
            {
                return;
            }
            bool willBeOutside = false;
            if (isRightKeyPressed)
            {

                foreach (Point block in shape.Blocks)
                {
                    if (block.X > gridWidth - 2 || grid[block.X + 1, block.Y] != 0)
                    {
                        willBeOutside = true;
                    }

                }
                if (!willBeOutside)
                {
                    shape.MoveRight(shape);
                    Invalidate();
                }
            }
        }

        
        private List<int> ListCollapseHeights()
        {
            List<int> fullRows = new List<int>();

            bool isFull = true;
            for (int row = gridHeight - 1 ; row >= 0 ; row--)
            {
                isFull = true;

                for (int col = 0; col < gridWidth  ; col++)
                {
                    if (grid[col, row] == 0)
                    {
                        isFull = false;
                        break;
                    }
                }
                if (isFull)
                {
                    fullRows.Add(row);
                }
            }

            return fullRows; 
            
        }
  

        private void UpdateScore()
        {
            score += 100;
            Invalidate();
        }
        
        
        //KEYS START
        
        private void KeyDownLeft(object sender , KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Left)
            {
                isLeftKeyPressed = true;
                MoveLeft(activeTetromino);
            }
        }
        private void KeyDownZ(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z)
            {
                isZKeyPressed = true;
                
                RotateLeft2(activeTetromino);
            }
        }
        private void KeyUpZ(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Z)
            {
                isZKeyPressed = false;
            }
        }

        private void KeyDownDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                isDownKeyPressed = true;
                gameTimer.Interval = 100;

            }
        }
        private void KeyUpDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                isDownKeyPressed = false;
                gameTimer.Interval = 150;
            }
        }

        private void RotateLeft2(TetrominoBuilder shape)
        {
            if(SafeRotation(shape))
            {
                shape.RotateLeft(activeTetromino);

            }
            
            
        }
        private void KeyUpRight(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                isRightKeyPressed = false;
            }
        }
        private void KeyDownRight(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                isRightKeyPressed = true;
                MoveRight(activeTetromino);
                
            }
        }
       
        private void KeyUpLeft(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                isLeftKeyPressed = false;
            }
        }
        
        // KEYS END


        // Populate zeroes
        
        private void PopulateZeroes(List<int> list )
        {
            int listCount = list.Count;
            for( int i = 0; i < gridWidth; i++)
            {
                for(int j = 0; j < listCount; j++)
                {
                    grid[i, list[j] ] = 0;
                }
            }
        }
        
        // APPLY GRAVITY 
        
        private void ApplyGravity()
        {
            List<int> fullRows = ListCollapseHeights(); // Get rows that need to be cleared
            fullRows = fullRows.OrderByDescending(x => x).ToList(); // Sort in descending order

            if (fullRows.Count > 0)
            {
                PopulateZeroes(fullRows);
                foreach( int row in fullRows)
                {
                    UpdateScore();
                }

            

                // Shift only rows ABOVE cleared rows downward
                foreach (int clearedRow in fullRows)
                {
                    for (int row = clearedRow; row >0; row--)
                    {
                        for (int col = 0; col < gridWidth; col++)
                        {
                            grid[col, row] = grid[col, row - 1];   // Move row above down
                            gridColors[col, row] = gridColors[col, row - 1];
                        }
                    }
                }
            }
           
            Invalidate(); // Refresh the UI
        }
        



        // CHECK GAME OVER

        private bool CheckGameOver()
        {
            // If the top row contains a locked piece, return true
            for (int col = 0; col < gridWidth; col++)
            {
                if (grid[col, 0] == 1) // Top row occupied
                {
                    return true;
                }
            }
            return false;
        }


        // GAME OVER

        private void GameOver()
        {
            if (CheckGameOver())
            {
                gameTimer.Stop(); // Stop the game loop
                MessageBox.Show("Game Over!", "Tetris", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RestartGame(); // Optionally restart the game
            }
        }


        // UPDATE GAME

        private void UpdateGame()
        {

          

                ApplyGravity();

            

            if (activeTetromino.IsLocked)
            {
                GameOver();
                activeTetromino = SpawnTetromino();
            }
            else
            {
                FallDownTetromino(activeTetromino);
            }


        }

        // SET NEW ACTIVE TETROMINO

        private TetrominoBuilder SetNewActive(bool locked)
        {
            if (locked)
            {
                
                return activeTetromino = SpawnTetromino();

            }
            else return activeTetromino;
        }

        // CHECK FOR COLLISION

        private bool CheckCollision(TetrominoBuilder shape, int[,] grid)
        {
            foreach ( Point block in shape.Blocks)
            {
                int x = block.X;
                int y = block.Y;
                if ( y >= grid.GetLength(1))
                {
                    return true;
                }
                if (grid[x, y] != 0)
                {
                    return true; 
                }
               
            }
            return false;
        }

        // RESTART GAME

        private void RestartGame()
        {
            // Clear the grid
            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    grid[col, row] = 0;
                    gridColors[col, row] = Color.FromArgb(30, 30, 30);
                }
            }

            // Reset score and active piece
            score = 0;
            activeTetromino = SpawnTetromino();

            // Restart the game loop
            gameTimer.Start();
        }



    }
}

   
