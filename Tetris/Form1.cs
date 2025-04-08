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
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Media;


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
        private bool isEnterKeyPressed = false;
        
        // CREATE HELD-BOOL
        private bool hasHeldThisTurn = false;
        // DEFINE GHOSTTETROMINO 
        private TetrominoBuilder ghostTetromino;
        // SET UP MUSIC
        private SoundPlayer backgroundMusic;
        private int selectedMenuIndex = 0;

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
            this.KeyUp += new KeyEventHandler(KeyUpC);
            this.KeyDown += new KeyEventHandler(KeyDownUp);
            this.KeyUp += new KeyEventHandler(KeyUpUp);
            this.KeyDown += new KeyEventHandler(KeyDownEnter);
            this.KeyUp += new KeyEventHandler(KeyUpEnter);


            backgroundMusic = new SoundPlayer("tetris.wav");
            backgroundMusic.PlayLooping();


        }

        // HELPER METHOD TO DRAW STYLED BLOCKS

        private void DrawStyledBlock(Graphics g, int x, int y, int size, Color baseColor)
        {
            Rectangle rect = new Rectangle(x, y, size, size);

            using (GraphicsPath path = CreateRoundedRectangle(rect, 6))
            {
                // BASE COLOR
                using (SolidBrush baseBrush = new SolidBrush(baseColor))
                    g.FillPath(baseBrush, path);

                // HIGHLIGHT TOP LEFT
                using (LinearGradientBrush highlight = new LinearGradientBrush(
                    rect,
                    Color.FromArgb(100, Color.White),
                    Color.FromArgb(0, Color.White),
                    LinearGradientMode.ForwardDiagonal))
                {
                    g.FillPath(highlight, path);
                }

                // SHADOW BOTTOM RIGHT
                using (LinearGradientBrush shadow = new LinearGradientBrush(
                    rect,
                    Color.FromArgb(0, Color.Black),
                    Color.FromArgb(100, Color.Black),
                    LinearGradientMode.BackwardDiagonal))
                {
                    g.FillPath(shadow, path);
                }

                // OUTLINE
                g.DrawPath(Pens.FloralWhite, path);
            }
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
                    {
                        int x = col * cellSize;
                        int y = row * cellSize;
                        DrawStyledBlock(e.Graphics, x, y, cellSize, gridColors[col, row]);
                    }
                }
            }


            // DRAW THE GHOST TETROMINO
            DrawGhostPiece(e.Graphics, ghostTetromino);



            // DRAW THE ACTIVE TETROMINO
            if (activeTetromino != null)
            {
                DrawTetromino(e.Graphics, activeTetromino);
            }

            // DRAW THE STRINGS ON THE SIDE OF THE GRID
            using (Font font = new Font("Consolas", 8))
            using (SolidBrush brush = new SolidBrush(Color.FloralWhite))
            {
                e.Graphics.DrawString($"Score: {score}", font, brush, new PointF(320, 20));
                e.Graphics.DrawString("Next Tetromino", font, brush, new PointF(300, 70));
                e.Graphics.DrawString("Held Tetromino", font, brush , new PointF(300, 200));
                e.Graphics.DrawString($"Level: {currentLevel}", font, brush, new PointF(320, 350));
                


            }
            // DRAW THE NEXT AND HELD TETROMINOS ON THE SIDE
            DrawNextTetromino(e.Graphics, nextTetromino);
            DrawHeldTetromino(e.Graphics, heldTetromino);

            if (CheckGameOver())
            {       
                string[] options = { "Restart", "Quit" };
                using (Font bigFont = new Font("Consolas", 8))
                using (Font smallFont = new Font("Consolas", 8))
                using (SolidBrush textBrush = new SolidBrush(Color.FloralWhite))
                {
                    e.Graphics.DrawString("GAME OVER!", smallFont, textBrush,  new PointF(310, 420));
                   
                    

                    for (int i = 0; i < options.Length; i++)
                    {
                        Brush optionBrush = (i == selectedMenuIndex)
                            ? new SolidBrush(Color.FromArgb(255, 200, 100))
                            : textBrush;

                        e.Graphics.DrawString(options[i], smallFont, optionBrush, new PointF(310, 480 + i * 30));
                    }

                }
            }


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
            if (next == null) return;

            foreach (Point block in next.Blocks)
            {
                int x = 280 + block.X * cellSize / 2;
                int y = 120 + block.Y * cellSize / 2;
                int size = cellSize / 2;
                Rectangle rect = new Rectangle(x, y, size, size);
                Color baseColor = next.Color;

                using (GraphicsPath path = CreateRoundedRectangle(rect, 4))
                {
                    // BASE COLOR
                    using (SolidBrush baseBrush = new SolidBrush(baseColor))
                        g.FillPath(baseBrush, path);

                    // HIGHLIGHT TOP LEFT
                    using (GraphicsPath highlightPath = CreateRoundedRectangle(rect, 4))
                    using (LinearGradientBrush highlight = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(80, Color.White),
                        Color.FromArgb(0, Color.White),
                        LinearGradientMode.ForwardDiagonal))
                    {
                        g.FillPath(highlight, highlightPath);
                    }

                    // SHADOW BOTTOM RIGHT
                    using (GraphicsPath shadowPath = CreateRoundedRectangle(rect, 4))
                    using (LinearGradientBrush shadow = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(0, Color.Black),
                        Color.FromArgb(60, Color.Black),
                        LinearGradientMode.BackwardDiagonal))
                    {
                        g.FillPath(shadow, shadowPath);
                    }

                    // OUTLINE
                    g.DrawPath(Pens.FloralWhite, path);
                }
            }
        }



        // METHOD TO DRAW THE HELD TETROMINO ON THE SIDE OF THE GRID
        private void DrawHeldTetromino(Graphics g, TetrominoBuilder held)
        {
            if (held == null) return;

            // BOUNDING BOX
            int minX = held.Blocks.Min(b => b.X);
            int minY = held.Blocks.Min(b => b.Y);
            int maxX = held.Blocks.Max(b => b.X);
            int maxY = held.Blocks.Max(b => b.Y);

            int shapeWidth = (maxX - minX + 1) * (cellSize / 2);
            int shapeHeight = (maxY - minY + 1) * (cellSize / 2);

            // WHERE TO DRAW
            int offsetX = 323;
            int offsetY = 235;

            // OFFSET WITH DISPLAY BOX
            int centerOffsetX = (80 - shapeWidth) / 2;
            int centerOffsetY = (80 - shapeHeight) / 2;

            foreach (Point block in held.Blocks)
            {
                int x = offsetX + centerOffsetX + (block.X - minX) * cellSize / 2;
                int y = offsetY + centerOffsetY + (block.Y - minY) * cellSize / 2;
                int size = cellSize / 2;
                Rectangle rect = new Rectangle(x, y, size, size);

                using (GraphicsPath path = CreateRoundedRectangle(rect, 4))
                {
                    using (SolidBrush baseBrush = new SolidBrush(held.Color))
                        g.FillPath(baseBrush, path);

                    using (LinearGradientBrush highlight = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(80, Color.White),
                        Color.FromArgb(0, Color.White),
                        LinearGradientMode.ForwardDiagonal))
                    {
                        g.FillPath(highlight, path);
                    }

                    using (LinearGradientBrush shadow = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(0, Color.Black),
                        Color.FromArgb(60, Color.Black),
                        LinearGradientMode.BackwardDiagonal))
                    {
                        g.FillPath(shadow, path);
                    }

                    g.DrawPath(Pens.FloralWhite, path);
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
            if (shape == null) return;

            foreach (Point block in shape.Blocks)
            {
                int x = block.X * cellSize;
                int y = block.Y * cellSize;
                DrawStyledBlock(g, x, y, cellSize, shape.Color);
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
        
        private void KeyDownUp(object sender, KeyEventArgs e)
        {
            if ( e.KeyCode == Keys.Up)
            {
                isUpKeyPressed = true;
                if(CheckGameOver())
                {
                    if (e.KeyCode == Keys.Up )
                    {
                        selectedMenuIndex = (selectedMenuIndex - 1 + 2) % 2;
                        Invalidate();
                    }
                }
            }
        }
        private void KeyUpUp(object sender, KeyEventArgs e)
        {
            if( e.KeyCode == Keys.Up)
            {
                isUpKeyPressed = false;
            }
        }
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
                if(CheckGameOver())
                {
                    
                    
                        selectedMenuIndex = (selectedMenuIndex + 1) % 2;
                        Invalidate();
                    
                }

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

        private void KeyDownEnter( object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && CheckGameOver())
            {
                isEnterKeyPressed = true;
                if (selectedMenuIndex == 0) 
                {
                    RestartGame();
                    
                }
                else if (selectedMenuIndex == 1) 
                {
                    Application.Exit();
                }
            }
        }
        private void KeyUpEnter (  object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                isEnterKeyPressed = false;
            }
        }


        // KEYS -- END

        // *** // 


        // HELPER METHOD FOR THE HOLD FUNCTION 
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


        // HOLD TETROMINO

        private void HoldTetromino()
        {
            // IF ALREADY HELD THIS TURN, RETURN 
            if (hasHeldThisTurn) return;

            // IF HELD TETROMINO IS NULL, THEN SET THE ACTIVE TETROMINO AS THE HELD ONE, AND SPAWN A NEW ONE
            if (heldTetromino == null)
            {
                heldTetromino = activeTetromino;
                activeTetromino = SpawnTetromino();
            }
            // IF ALREADY HOLDING A TETROMINO, SWAP THE ACTIVE ONE FOR THE HELD ONE 
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

                gameTimer.Interval = Math.Max(50, 150 - (currentLevel - 1) * 10);
                level = currentLevel;
            }
        }

        // HELPER FUNCTION FOR WALL KICK
        private List<Point> Shift(List<Point> blocks, int dx)
        {
            // LOOP THROUGH EACH POINT IN BLOCKS , USE LINQ TO "SHIFT" EACH BLOCK HORIZONTALLY TO CHECK IF IT COLLIDES 
            // OR GOES OUT OF BOUNDS
            return blocks.Select(p => new Point(p.X + dx, p.Y)).ToList();
        }

        // HELPER FUNCTION FOR WALL KICK
        private bool IsValidPosition(List<Point> blocks)
        {
            // CHECKS IF BLOCKS ARE IN A VALID POSITION 
            foreach (var block in blocks)
            {
                if (block.X < 0 || block.X >= gridWidth || block.Y < 0 || block.Y >= gridHeight)
                    return false;

                if (grid[block.X, block.Y] != 0)
                    return false;
            }
            return true;
        }

        // WALL KICK

        public bool TryWallKick(TetrominoBuilder shape)
        {
            List<Point> rotated = shape.RotationCheck(shape);

            // TRY NORMAL ROTATION
            if (IsValidPosition(rotated))
            {
                shape.RotateLeft(shape);
                return true;
            }

            // TRY MULTIPLE DIFFERENT "KICKS" SO THAT THE "I" TETROMINO ALSO IS ABLE TO ROTATE
            int[] shifts = { -1, 1, -2, 2, -3, 3 };

            // LOOP OVER ALL SHIFTS
            foreach (int dx in shifts)
            {

                var shifted = Shift(rotated, dx);
                if (IsValidPosition(shifted))
                {
                    // IF IS VALID POSITION , APPLY SHIFT
                    for (int i = 0; i < Math.Abs(dx); i++)
                    {
                        if (dx < 0) shape.MoveLeft(shape);
                        else shape.MoveRight(shape);
                    }

                    // APPLY THE ROTATION NEXT
                    shape.RotateLeft(shape);
                    return true;
                }
            }

            // IF FALSE, THEN CANNOT ROTATE
            return false;
        }



       

        


        // APPLY GRAVITY 

        private void ApplyGravity()
        {
            List<int> fullRows = ListCollapseHeights();

            if (fullRows.Count == 0)
                return;

            // CREATE A TEMPORARY GRID
            int[,] newGrid = new int[gridWidth, gridHeight];
            Color[,] newGridColors = new Color[gridWidth, gridHeight];

            int targetRow = gridHeight - 1;

            // GO FROM BOTTOM UP , CHECKING WHICH ROWS TO KEEP 
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

            // NOW REPLACE THE ORIGINAL GRID
            grid = newGrid;
            gridColors = newGridColors;

            // INCREASE SCORE * CLEARED ROWS
            foreach (int _ in fullRows)
                UpdateScore();

            Invalidate(); 
        }

        // HARD DROP

        public void HardDrop()
        {
            if (activeTetromino == null || activeTetromino.IsLocked)
                return;

            while (true)
            {
                // TRY TO MOVE
                foreach (Point block in activeTetromino.Blocks)
                {
                    int newY = block.Y + 1;
                    // BEFORE COLLISION
                    if (newY >= gridHeight || grid[block.X, newY] != 0)
                    {
                        // LOCK TETROMINO, SPAWN A NEW ONE, APPLY GRAVITY
                        LockTetromino(activeTetromino);
                        activeTetromino = SpawnTetromino();
                        ApplyGravity();
                        Invalidate();
                        return;
                    }
                }

                // IF SAFE TO MOVE DOWN, MOVE DOWN
                activeTetromino.MoveDown(activeTetromino);
            }
        }
        
        // CHECK GAME OVER

        private bool CheckGameOver()
        {
            // IF TOP ROW CONTAINS LOCKED PIECE, GAME OVER
            for (int col = 0; col < gridWidth; col++)
            {
                if (grid[col, 0] == 1)
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
                gameTimer.Stop(); // IF GAME OVER, STOP GAME LOOP
             
            }
        }


        // UPDATE GAME
        private void UpdateGame()
        {
                
                ApplyGravity();
                IncreaseSpeed();
                ghostTetromino = CheckWhereGhostPieceWouldDrop(activeTetromino);






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


        // RESTART GAME

        private void RestartGame()
        {
            // CLEAR GRID
            for (int row = 0; row < gridHeight; row++)
            {
                for (int col = 0; col < gridWidth; col++)
                {
                    grid[col, row] = 0;
                    gridColors[col, row] = Color.FromArgb(30, 30, 30);
                }
            }

            // RESET SCORE AND ACTIVE PIECE
            score = 0;
            activeTetromino = SpawnTetromino();

            heldTetromino = null;
            hasHeldThisTurn = false;

            // RESTART GAME LOOP
            gameTimer.Start();
        }

        // CREATE ROUNDED RECTANGLES 

        public GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

           
            path.AddArc(arc, 180, 90);

            
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private void DrawGhostPiece(Graphics g, TetrominoBuilder shape)
        {
            if (shape == null) return;

            Color ghostColor = Color.FromArgb(80, shape.Color); // semi-transparent

            foreach (Point block in shape.Blocks)
            {
                int x = block.X * cellSize;
                int y = block.Y * cellSize;
                DrawStyledBlock(g, x, y, cellSize, ghostColor);
            }
        }




        private TetrominoBuilder CheckWhereGhostPieceWouldDrop(TetrominoBuilder activeTetromino)
        {
            TetrominoBuilder ghost = activeTetromino.Clone();

            // Keep moving down until it hits something
            bool canMove = true;
            while (canMove)
            {
                foreach (Point block in ghost.Blocks)
                {
                    int newY = block.Y + 1;
                    if (newY >= gridHeight || grid[block.X, newY] != 0)
                    {
                        canMove = false;
                        break;
                    }
                }

                if (canMove)
                {
                    ghost.MoveDown(ghost);
                }
            }

            return ghost;
        }



    }
}

   
