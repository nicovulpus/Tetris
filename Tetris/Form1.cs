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
        // LIST OF ALL LOCKED TETROMINOS
        private List<TetrominoBuilder> lockedTetrominoes = new List<TetrominoBuilder>();

        // DEFINE GRIDSIZE
        private const int gridWidth = 10;
        private const int gridHeight = 20;
        private const int cellSize = 30;
        private int[,] grid;
        private Color[,] gridColors;
        // CREATE ACTIVE TETROMINO
        private TetrominoBuilder activeTetromino;
        // CREATE KEYPRESSES
        private bool isLeftKeyPressed = false;
        private bool isRightKeyPressed = false;
        private bool isUpKeyPressed = false;
        private bool isDownKeyPressed = false;
        private bool isZKeyPressed = false;
        private bool isXKeyPressed = false;
        private bool isCKeyPressed = false;
        // CREATE HELD-BOOL
        private bool hasHeldThisTurn = false;

        // CREATE SCORE AND LEVEL
        private int score = 0;
        private int level = 1;
        private int currentLevel = 1;
        
        // CREATE NEXT AND HELD TETROMINOS
        private TetrominoBuilder nextTetromino;
        private TetrominoBuilder heldTetromino;

        // SET TIMER
        private System.Windows.Forms.Timer gameTimer;


        // DEFINE GAMELOOP
        private void GameLoop(object sender , EventArgs e)
        {
                UpdateGame();
                Invalidate();
        }

        // DEFINE FORM1
        public Form1()
        {

            // SET UP THE CLIENT
            InitializeComponent();
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.DoubleBuffered = true;
            this.ClientSize = new Size(gridWidth * cellSize + 130, gridHeight * cellSize);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // SET UP A TIMER
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 150;
            gameTimer.Tick += new EventHandler(GameLoop);
            gameTimer.Start();
            
            // SPAWN TETROMINO
            activeTetromino = SpawnTetromino();
           
            // INITIALIZE GRID
            grid= new int[gridWidth, gridHeight];
            
            gridColors = new Color[gridWidth, gridHeight];
            
            // POPULATE GRID WITH ZEROES
            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    grid[col, row] = 0; 
                }
            }

            // MAKE ACTIVE TETROMINO FALL DOWN
            FallDownTetromino(activeTetromino);

            // SET UP EVENT LISTENER FOR KEYPRESSES
            this.KeyDown += new KeyEventHandler(KeyDownLeft);
            this.KeyUp += new KeyEventHandler(KeyUpLeft);
            this.KeyDown += new KeyEventHandler(KeyDownRight);
            this.KeyUp += new KeyEventHandler(KeyUpRight);
            this.KeyDown += new KeyEventHandler(KeyDownZ);
            this.KeyUp += new KeyEventHandler(KeyUpZ);
            this.KeyDown += new KeyEventHandler(KeyDownDown);
            this.KeyUp += new KeyEventHandler(KeyUpDown);
            this.KeyDown += new KeyEventHandler(KeyDownX);
            this.KeyUp += new KeyEventHandler(KeyUpX);
            this.KeyDown += new KeyEventHandler(KeyDownC);
            this.KeyDown += new KeyEventHandler(KeyUpC);




        }

        // ONPAINT
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // DRAW THE OUTLINE OF THE GRID
            DrawGrid(e.Graphics);

            // DRAW THE LOCKED TETROMINOS
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

            // DRAW THE ACTIVE TETROMINO
            if (activeTetromino != null)
            {
                DrawTetromino(e.Graphics, activeTetromino);
            }

            // DRAW THE STRINGS ON THE SIDE OF THE GRID
            using (Font font = new Font("Arial", 8))
            using (SolidBrush brush = new SolidBrush(Color.FloralWhite))
            {
                e.Graphics.DrawString($"Score: {score}", font, brush, new PointF(320, 20));
                e.Graphics.DrawString("Next Tetromino", font, brush, new PointF(310, 70));
                e.Graphics.DrawString("Held Tetromino", font, brush , new PointF(310, 200));
                e.Graphics.DrawString($"Level: {currentLevel}", font, brush, new PointF(320, 400));


            }
            // DRAW THE NEXT AND HELD TETROMINOS ON THE SIDE
            DrawNextTetromino(e.Graphics, nextTetromino);
            DrawHeldTetromino(e.Graphics, heldTetromino);

        }



        // METHOD TO SPAWN THE NEXT TETROMINO
        public TetrominoBuilder SpawnTetromino()
        {   
            // IF nextTetromino NOT NULL, ASSIGN IT TO activeTetromino
            activeTetromino = nextTetromino ?? PickTetromino();
            // RESET TETROMINOS SO THAT THE HOLDTETROMINO METHOD FUNCTIONS PROPERLY
            ResetTetrominoPosition(activeTetromino);
            // PICK NEXT TETROMINO
            nextTetromino = PickTetromino();

            return activeTetromino;
        }

        // METHOD TO DRAW THE NEXT TETROMINO ON THE SIDE OF THE GRID
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

        // METHOD TO DRAW THE HELD TETROMINO ON THE SIDE OF THE GRID
        private void DrawHeldTetromino(Graphics g, TetrominoBuilder next)
        {
            if (next == null) return;

            using (SolidBrush brush = new SolidBrush(next.Color))
            {
                foreach (Point block in next.Blocks)
                {
                    int x = 290 + block.X * cellSize / 2;
                    int y = 180 + block.Y * cellSize / 2;
                    g.FillRectangle(brush, x, y, cellSize / 2, cellSize / 2);
                    g.DrawRectangle(Pens.White, x, y, cellSize / 2, cellSize / 2);
                }
            }
        }



        // METHOD TO DRAW THE GRID
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

        // METHOD TO DRAW THE ACTIVE TETROMINO
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

        // RANDOMLY PICK THE NEXT TETROMINO
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

        // LOCK TETROMINO
        private void LockTetromino(TetrominoBuilder shape)
        {
            
              foreach ( Point block in shape.Blocks)
                {
                    grid[block.X, block.Y] = 1;
                    gridColors[block.X, block.Y] = shape.Color;
                    
                }
              shape.LockTetromino();
            hasHeldThisTurn = false;

        }


        // MAKE TETROMINO FALL DOWN
        private void FallDownTetromino(TetrominoBuilder active)
        {   
            // IF THE ACTIVE TETROMINO IS LOCKED, RETURN
            if(active.IsLocked)
            {
                return;
            }
            // BOOL TO CHECK IF TETROMINO WILL BE " OUT OF BOUNDS " 
            bool willBeOutofBounds = false;


            foreach( Point block in active.Blocks )
            {
                if (block.Y + 1 >= gridHeight || grid[block.X, block.Y + 1] != 0)
                {
                    willBeOutofBounds = true;
                    LockTetromino(active);
                }
            }
            // IF WILL NOT BE " OUT OF BOUNDS " THEN MOVE THE ACTIVE PIECE DOWN 
            if (!willBeOutofBounds)
            {
                active.MoveDown(active);
            }
            
        }

        // MOVE LEFT
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

       // MOVE RIGHT
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

        // GET A LIST OF ROWS TO "COLLAPSE"
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
  
        // UPDATE SCORE
        private void UpdateScore()
        {
            score += 100;
            Invalidate();
        }
        
        
        //KEYS -- START
        
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
                gameTimer.Interval -= 50;

            }
        }
        private void KeyUpDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                isDownKeyPressed = false;
                gameTimer.Interval += 50;
            }
        }

        private void RotateLeft2(TetrominoBuilder shape)
        {
            if (shape.IsLocked)
                return;

            TryWallKick(shape);


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

        private void KeyDownX(object sender, KeyEventArgs e)
        {
            if ( e.KeyCode == Keys.X)
            {
                HardDrop();
                isXKeyPressed = true;
            }
        }
        
        private void KeyUpX(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.X)
            { 
                isXKeyPressed = false; 
            }

        }

        private void KeyDownC(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C)
            {
                HoldTetromino();
                isCKeyPressed = true;
            }
        }

        private void KeyUpC(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C)
            {
                isCKeyPressed = false;
            }

        }



        // KEYS -- END

        // HOLD TETROMINO

        private void HoldTetromino()
        {
            if (hasHeldThisTurn) return;

            if (heldTetromino == null)
            {
                heldTetromino = activeTetromino;
                activeTetromino = SpawnTetromino();
            }
            else
            {
                TetrominoBuilder temp = heldTetromino;
                heldTetromino = activeTetromino;
                activeTetromino = temp;
                ResetTetrominoPosition(activeTetromino);
            }

            hasHeldThisTurn = true;
        }

        // INCREASE SPEED 

        private void IncreaseSpeed()
        {
            int newLevel = score switch
            {
                >= 9500 => 10,
                >= 8500 => 9,
                >= 7500 => 8,
                >= 6500 => 7,
                >= 5500 => 6,
                >= 4500 => 5,
                >= 3500 => 4,
                >= 2500 => 3,
                >= 1500 => 2,
                _ => 1
            };

            if (newLevel > currentLevel)
            {
                currentLevel = newLevel;

                // Adjust the interval based on level
                gameTimer.Interval = Math.Max(50, 150 - (currentLevel - 1) * 10);
                level = currentLevel;
            }
        }

        // WALL KICK

        public bool TryWallKick(TetrominoBuilder shape)
        {
            List<Point> rotated = shape.RotationCheck(shape);

            // Try normal rotation
            if (IsValidPosition(rotated))
            {
                shape.RotateLeft(shape);
                return true;
            }

            // Try multiple horizontal shifts (left and right)
            int[] shifts = { -1, 1, -2, 2, -3, 3 }; // More aggressive kicks

            foreach (int dx in shifts)
            {
                var shifted = Shift(rotated, dx);
                if (IsValidPosition(shifted))
                {
                    // Apply the shift
                    for (int i = 0; i < Math.Abs(dx); i++)
                    {
                        if (dx < 0) shape.MoveLeft(shape);
                        else shape.MoveRight(shape);
                    }

                    // Apply the rotation
                    shape.RotateLeft(shape);
                    return true;
                }
            }

            // All wall kicks failed
            return false;
        }



        private List<Point> Shift(List<Point> blocks, int dx)
        {
            return blocks.Select(p => new Point(p.X + dx, p.Y)).ToList();
        }

        private bool IsValidPosition(List<Point> blocks)
        {
            foreach (var block in blocks)
            {
                if (block.X < 0 || block.X >= gridWidth || block.Y < 0 || block.Y >= gridHeight)
                    return false;

                if (grid[block.X, block.Y] != 0)
                    return false;
            }
            return true;
        }


        // APPLY GRAVITY 

        private void ApplyGravity()
        {
            List<int> fullRows = ListCollapseHeights();

            if (fullRows.Count == 0)
                return;

            // Create a new temp grid
            int[,] newGrid = new int[gridWidth, gridHeight];
            Color[,] newGridColors = new Color[gridWidth, gridHeight];

            int targetRow = gridHeight - 1;

            // Go from bottom up, copying only non-full rows
            for (int row = gridHeight - 1; row >= 0; row--)
            {
                if (!fullRows.Contains(row))
                {
                    for (int col = 0; col < gridWidth; col++)
                    {
                        newGrid[col, targetRow] = grid[col, row];
                        newGridColors[col, targetRow] = gridColors[col, row];
                    }
                    targetRow--;
                }
            }

            // Replace original grid
            grid = newGrid;
            gridColors = newGridColors;

            // Score once per row cleared
            foreach (int _ in fullRows)
                UpdateScore();

            Invalidate(); // Redraw
        }

        // HARD DROP

        public void HardDrop()
        {
            if (activeTetromino == null || activeTetromino.IsLocked)
                return;

            while (true)
            {
                // Try to move the tetromino down
                foreach (Point block in activeTetromino.Blocks)
                {
                    int newY = block.Y + 1;
                    // If the block would go out of bounds or land on another block
                    if (newY >= gridHeight || grid[block.X, newY] != 0)
                    {
                        LockTetromino(activeTetromino);
                        activeTetromino = SpawnTetromino();
                        ApplyGravity();
                        Invalidate();
                        return;
                    }
                }

                // If it's safe, move the tetromino down
                activeTetromino.MoveDown(activeTetromino);
            }
        }
        



        // HOLD TETROMINO



        // PAUSE GAME 



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
                IncreaseSpeed();

            

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
        private void ResetTetrominoPosition(TetrominoBuilder tetromino)
        {
            if (tetromino == null) return;

            int minX = tetromino.Blocks.Min(b => b.X);
            int minY = tetromino.Blocks.Min(b => b.Y);

            // Offset to position near top center
            int xOffset = (gridWidth / 2) - minX;
            int yOffset = 0 - minY;  // Bring it to top

            for (int i = 0; i < tetromino.Blocks.Length; i++)
            {
                tetromino.Blocks[i] = new Point(
                    tetromino.Blocks[i].X + xOffset,
                    tetromino.Blocks[i].Y + yOffset
                );
            }
        }



    }
}

   
