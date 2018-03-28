using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Asteroids
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Textures
        Texture2D shipTexture;
        Texture2D backgroundTexture;
        Texture2D enemyTexture;
        Texture2D laserTexture;
        Texture2D enemyDeadTexture;
        Texture2D splashScreenTexture;

        // Player Variables
        Vector2 playerPosition = new Vector2(0, 0);
        Vector2 playerOffset = new Vector2(0, 0);
        float playerTurnSpeed = 6f;
        float playerSpeed = 250f;
        float playerRotation = 0;
        float playerRadius = 0;
        bool playerIsAlive = true;

        // Enemy Variables
        Vector2[] enemyPositions = new Vector2[numberOfEnemies];
        Vector2[] enemyOffsets = new Vector2[numberOfEnemies];
        float[] enemyRotations = new float[numberOfEnemies];   
        float enemySpeed = 100f;
        float enemyRadius = 0;
        const int numberOfEnemies = 4;
        bool[] enemyLifeStates = new bool[numberOfEnemies];

        // Bullet Variables
        Vector2 laserOffset = new Vector2(23, 0);
        Vector2[] laserPositions = new Vector2[numberOfLasers];
        Vector2[] laserVelocity = new Vector2[numberOfLasers];
        float laserSpeed = 350;
        float laserRadius = 5;
        float laserShootTimer = 0;
        float shootDelay = 0.25f;
        const int numberOfLasers = 100;
        bool[] laserLifeStates = new bool[numberOfLasers];

        // Font
        SpriteFont berlinFont;

        // FPS Counter
        int currentFPS = 0;
        int fpsCounter = 0;
        float fpsTimer = 0;

        // Score
        int score = 0;

        // Game States
        const int STATE_SPLASH = 0;
        const int STATE_GAME = 1;
        const int STATE_GAMEOVER = 2;
        const int STATE_WIN = 3;

        // Starting State
        int gameState = STATE_SPLASH;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            this.IsFixedTimeStep = false;
            this.graphics.SynchronizeWithVerticalRetrace = false;
        }

        // Initialize 
        protected override void Initialize()
        {
            ResetGame();

            base.Initialize();
        }

        // Reset Game
        protected void ResetGame()
        {
            int halfWidth = graphics.GraphicsDevice.Viewport.Width / 2;
            int halfHeight = graphics.GraphicsDevice.Viewport.Height / 2;

            playerPosition = new Vector2(
                graphics.GraphicsDevice.Viewport.Width / 6,
                graphics.GraphicsDevice.Viewport.Height / 6);

            enemyPositions[0] = new Vector2(80, halfHeight);
            enemyRotations[0] = 4.5f;
            enemyLifeStates[0] = true;

            enemyPositions[1] = new Vector2(graphics.GraphicsDevice.Viewport.Width - 80, halfHeight);
            enemyRotations[1] = 1.0f;
            enemyLifeStates[1] = true;

            enemyPositions[2] = new Vector2(halfWidth, 80);
            enemyRotations[2] = 0.2f;
            enemyLifeStates[2] = true;

            enemyPositions[3] = new Vector2(halfWidth, graphics.GraphicsDevice.Viewport.Height - 80);
            enemyRotations[3] = 3.7f;
            enemyLifeStates[3] = true;

            playerRotation = 0;
            playerIsAlive = true;
            score = 0;

            for (int i = 0; i < numberOfLasers; i++)
            {
                laserLifeStates[i] = false;
            }

        }

        // Load Content 
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            shipTexture = Content.Load<Texture2D>("ship");
            backgroundTexture = Content.Load<Texture2D>("black");
            enemyTexture = Content.Load<Texture2D>("enemyShip");
            laserTexture = Content.Load<Texture2D>("playerLaser");
            enemyDeadTexture = Content.Load<Texture2D>("enemyDead");
            splashScreenTexture = Content.Load<Texture2D>("splashscreen");

            berlinFont = Content.Load<SpriteFont>("berlinsansfb");

            playerOffset = new Vector2(shipTexture.Width / 2, shipTexture.Height / 2);

            for (int i = 0; i < numberOfEnemies; i++)
            {
                enemyOffsets[i] = new Vector2(enemyTexture.Width / 2, enemyTexture.Height / 2);
            }

            playerRadius = shipTexture.Height / 2f;
            enemyRadius = enemyTexture.Height / 2f;

        }

        // Shoot Laser 
        void ShootLaser(Vector2 position, float rotation)
        {
            int indexOfDeadLaser = -1;

            for (int i = 0; i < numberOfLasers; i++)
            {
                if (laserLifeStates[i] == false)
                {
                    indexOfDeadLaser = i;
                    break;
                }
            }

            if (indexOfDeadLaser == -1)
                return;

            Vector2 direction = new Vector2((float)Math.Sin(rotation), (float)-Math.Cos(rotation));
            direction.Normalize();
            laserVelocity[indexOfDeadLaser] = direction * laserSpeed;
            laserPositions[indexOfDeadLaser] = position;
            laserLifeStates[indexOfDeadLaser] = true;
        }

        // Update Laser 
        void UpdateLaser(int laserIdx, float deltaTime)
        {

            laserPositions[laserIdx] += laserVelocity[laserIdx] * deltaTime;

            if (laserPositions[laserIdx].X < 0 ||
                laserPositions[laserIdx].X > graphics.GraphicsDevice.Viewport.Width ||
                laserPositions[laserIdx].Y < 0 ||
                laserPositions[laserIdx].Y > graphics.GraphicsDevice.Viewport.Height)
            {
                laserLifeStates[laserIdx] = false;
            }

        }

        // Update Player 
        protected void UpdatePlayer(float deltaTime)
        {
            if (playerIsAlive == false)
                return;

            KeyboardState state = Keyboard.GetState();

            float xSpeed = 0;
            float ySpeed = 0;

            laserShootTimer += deltaTime;

            if (state.IsKeyDown(Keys.W) == true)
            {
                ySpeed -= playerSpeed * deltaTime;
            }
            if (state.IsKeyDown(Keys.S) == true)
            {
                ySpeed += playerSpeed * deltaTime;
            }
            if (state.IsKeyDown(Keys.A) == true)
            {
                playerRotation -= playerTurnSpeed * deltaTime;
            }
            if (state.IsKeyDown(Keys.D) == true)
            {
                playerRotation += playerTurnSpeed * deltaTime;
            }
            if (state.IsKeyDown(Keys.Space) == true)
            {
                if (laserShootTimer >= shootDelay)
                {
                    ShootLaser(playerPosition, playerRotation);
                    laserShootTimer = 0;
                }
            }

            double x = (xSpeed * Math.Cos(playerRotation)) - (ySpeed * Math.Sin(playerRotation));
            double y = (xSpeed * Math.Sin(playerRotation)) + (ySpeed * Math.Cos(playerRotation));

            // calculate players new position
            playerPosition.X += (float)x;
            playerPosition.Y += (float)y;

            if (playerPosition.X < -playerOffset.Y)
            {
                playerPosition.X = graphics.GraphicsDevice.Viewport.Width - playerOffset.Y;
            }
            if (playerPosition.Y < -playerOffset.Y)
            {
                playerPosition.Y = graphics.GraphicsDevice.Viewport.Height - playerOffset.Y;
            }
            if (playerPosition.X > graphics.GraphicsDevice.Viewport.Width + playerOffset.Y)
            {
                playerPosition.X = playerOffset.Y;
            }
            if (playerPosition.Y > graphics.GraphicsDevice.Viewport.Height + playerOffset.Y)
            {
                playerPosition.Y = playerOffset.Y;
            }
        }

        // Update Enemies 
        protected void UpdateEnemies(float deltaTime)
        {

            // call EnemyUpdate for every enemy in our array ----------------------------------
            for (int i = 0; i < numberOfEnemies; i++)
            {
                if (enemyLifeStates[i] == true)
                {
                    Vector2 velocity = new Vector2(
                        (float)(-enemySpeed * Math.Sin(enemyRotations[i])),
                        (float)(enemySpeed * Math.Cos(enemyRotations[i])));

                    enemyPositions[i] += velocity * deltaTime;


                    if (enemyPositions[i].X < 0)
                    {
                        enemyPositions[i].X = 0;
                        velocity.X = -velocity.X;
                        enemyRotations[i] = (float)Math.Atan2(velocity.Y, velocity.X) - 1.5708f;
                    }

                    if (enemyPositions[i].Y < 0)
                    {
                        enemyPositions[i].Y = 0;
                        velocity.Y = -velocity.Y;
                        enemyRotations[i] = (float)Math.Atan2(velocity.Y, velocity.X) - 1.5708f;
                    }

                    if (enemyPositions[i].X > graphics.GraphicsDevice.Viewport.Width)
                    {
                        enemyPositions[i].X = graphics.GraphicsDevice.Viewport.Width;
                        velocity.X = -velocity.X;
                        enemyRotations[i] = (float)Math.Atan2(velocity.Y, velocity.X) - 1.5708f;
                    }

                    if (enemyPositions[i].Y > graphics.GraphicsDevice.Viewport.Height)
                    {
                        enemyPositions[i].Y = graphics.GraphicsDevice.Viewport.Height;
                        velocity.Y = -velocity.Y;
                        enemyRotations[i] = (float)Math.Atan2(velocity.Y, velocity.X) - 1.5708f;
                    }

                    enemyPositions[i] += velocity * deltaTime;

                }
            }
        }

        // Update Enemy Collisions 
        protected void UpdateEnemyCollisions()
        {
            for (int i = 0; i < numberOfEnemies; i++)
            {
                if (enemyLifeStates[i] == false)
                    continue;

                for (int j = 1; j < numberOfEnemies; j++)
                {
                    if (enemyLifeStates[j] == false)
                        continue;

                    if (i == j)
                        continue;

                    if (IsColliding(enemyPositions[i], enemyRadius, enemyPositions[j], enemyRadius) == true)
                    {

                        enemyRotations[i] += 3.14159f;
                        enemyRotations[j] += 3.14159f;
                        return;
                    }
                }
            }
        }

        // Update Splash State 
        protected void UpdateSplashState(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {
                gameState = STATE_GAME;
            }
        }

        // Update Game State 
        protected void UpdateGameState(float deltaTime)
        {

            UpdatePlayer(deltaTime);
            UpdateEnemies(deltaTime);
            UpdateEnemyCollisions();

            for (int i = 0; i < numberOfEnemies; i++)
            {
                if (enemyLifeStates[i] == false)
                    continue;

                if (IsColliding(enemyPositions[i], enemyRadius, playerPosition, playerRadius) == true)
                {
                    playerIsAlive = false;
                    break;
                }
            }



            // Laser -----------------------------------------------------------------------
            for (int laserIdx = 0; laserIdx < numberOfLasers; laserIdx++)
            {
                if (laserLifeStates[laserIdx] == true)
                {
                    UpdateLaser(laserIdx, deltaTime);

                    for (int i = 0; i < numberOfEnemies; i++)
                    {
                        if (enemyLifeStates[i] == true)
                        {
                            bool isColliding = IsColliding(laserPositions[laserIdx], laserRadius,
                                                    enemyPositions[i], enemyRadius);

                            if (isColliding == true)
                            {
                                // bullet and pirate ship are colliding
                                // kill the bullet, then the enemy
                                laserLifeStates[laserIdx] = false;
                                enemyLifeStates[i] = false;
                                score += 1;
                                break;
                            }
                        }
                    }
                }
            }


            if (playerIsAlive == false)
            {
                gameState = STATE_GAMEOVER;
            }

            if (score >= 4)
            {
                gameState = STATE_WIN;
            }
        }

        // Update Game Over State 
        protected void UpdateGameOverState(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {
                gameState = STATE_GAME;
                ResetGame();

            }
        }

        // Update Win State 
        protected void UpdateWinState(float deltaTime)
        {
            if (score == 4)
            {
                gameState = STATE_WIN;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {
                gameState = STATE_GAME;
                ResetGame();
            }
        }

        // Update Function 
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            fpsTimer += deltaTime;
            fpsCounter++;
            if (fpsTimer >= 1.0f)
            {
                currentFPS = fpsCounter;
                fpsCounter = 0;
                fpsTimer -= 1;
            }

            switch (gameState)
            {
                case STATE_SPLASH:
                    UpdateSplashState(deltaTime);
                    break;

                case STATE_GAME:
                    UpdateGameState(deltaTime);
                    break;

                case STATE_GAMEOVER:
                    UpdateGameOverState(deltaTime);
                    break;

                case STATE_WIN:
                    UpdateWinState(deltaTime);
                    break;
            }




            base.Update(gameTime);

        }

        // Draw Splash State 
        protected void DrawSplashState(SpriteBatch spriteBatch)
        {
            int tileWidth = (graphics.GraphicsDevice.Viewport.Width / backgroundTexture.Width) + 1;
            int tileHeight = (graphics.GraphicsDevice.Viewport.Height / backgroundTexture.Height) + 1;

            for (int column = 0; column < tileWidth; column += 1)
            {
                for (int row = 0; row < tileHeight; row += 1)
                {
                    Vector2 position = new Vector2(column * backgroundTexture.Width, row * backgroundTexture.Height);

                    spriteBatch.Draw(backgroundTexture, position, Color.White);
                }
            }

            

           // spriteBatch.DrawString(berlinFont, "WASD to Move", new Vector2(300, 275), Color.White);

           // spriteBatch.DrawString(berlinFont, "Space to Shoot!", new Vector2(300, 300), Color.White);


            float xScale = (float)graphics.GraphicsDevice.Viewport.Width / splashScreenTexture.Width;
            float yScale = (float)graphics.GraphicsDevice.Viewport.Height / splashScreenTexture.Height;

            spriteBatch.Draw(splashScreenTexture, new Vector2(0, -1), null, Color.White, 0, 
                             new Vector2 (0, 0), new Vector2(xScale, yScale), SpriteEffects.None, 0);

            spriteBatch.DrawString(berlinFont, "Press ENTER to Play!", new Vector2(300, graphics.GraphicsDevice.Viewport.Height - 40), Color.White);
        }

        // Draw Game State 
        protected void DrawGameState(SpriteBatch spriteBatch)
        {


            int tileWidth = (graphics.GraphicsDevice.Viewport.Width / backgroundTexture.Width) + 1;
            int tileHeight = (graphics.GraphicsDevice.Viewport.Height / backgroundTexture.Height) + 1;

            for (int column = 0; column < tileWidth; column += 1)
            {
                for (int row = 0; row < tileHeight; row += 1)
                {
                    Vector2 position = new Vector2(column * backgroundTexture.Width, row * backgroundTexture.Height);

                    spriteBatch.Draw(backgroundTexture, position, Color.White);
                }
            }

            for (int i = 0; i < numberOfLasers; i++)
            {
                if (laserLifeStates[i] == true)
                {
                    spriteBatch.Draw(laserTexture, laserPositions[i], null, Color.White, 0, laserOffset, 1, SpriteEffects.None, 0);

                }
            }

            if (playerIsAlive == true)
            {
                spriteBatch.Draw(shipTexture, playerPosition, null, Color.White, playerRotation, playerOffset, 1, SpriteEffects.None, 0);
            }



            if (playerPosition.X < playerOffset.Y)
            {
                Vector2 wrapPos = new Vector2(graphics.GraphicsDevice.Viewport.Width + playerPosition.X, playerPosition.Y);
                spriteBatch.Draw(shipTexture, wrapPos, null, Color.White, playerRotation, playerOffset, 1, SpriteEffects.None, 0);
            }
            if (playerPosition.Y < playerOffset.Y)
            {
                Vector2 wrapPos = new Vector2(playerPosition.X, graphics.GraphicsDevice.Viewport.Height + playerPosition.Y);
                spriteBatch.Draw(shipTexture, wrapPos, null, Color.White, playerRotation, playerOffset, 1, SpriteEffects.None, 0);
            }
            if (playerPosition.X > graphics.GraphicsDevice.Viewport.Width - playerOffset.Y)
            {
                Vector2 wrapPos = new Vector2(-(graphics.GraphicsDevice.Viewport.Width - playerPosition.X), playerPosition.Y);
                spriteBatch.Draw(shipTexture, wrapPos, null, Color.White, playerRotation, playerOffset, 1, SpriteEffects.None, 0);
            }
            if (playerPosition.Y > graphics.GraphicsDevice.Viewport.Height - playerOffset.Y)
            {
                Vector2 wrapPos = new Vector2(playerPosition.X, -(graphics.GraphicsDevice.Viewport.Height - playerPosition.Y));
                spriteBatch.Draw(shipTexture, wrapPos, null, Color.White, playerRotation, playerOffset, 1, SpriteEffects.None, 0);
            }

            for (int i = 0; i < numberOfEnemies; i++)
            {
                if (enemyLifeStates[i] == true)
                {
                    spriteBatch.Draw(enemyTexture,
                        enemyPositions[i], null, Color.White,
                        enemyRotations[i],
                        enemyOffsets[i], 1, SpriteEffects.None, 0);
                }
            }


            // Draw FPS Counter ---------------------------------------------------------------------
            spriteBatch.DrawString(berlinFont, "FPS: " + currentFPS, new Vector2(10, 5), Color.LimeGreen);

            // Draw FPS Counter ---------------------------------------------------------------------
            spriteBatch.DrawString(berlinFont, "SCORE: " + score, new Vector2(10, 30), Color.LimeGreen);

            if (score >= 4)
            {
                spriteBatch.DrawString(berlinFont, "YOU WON!", new Vector2(graphics.GraphicsDevice.Viewport.Width / 2.5f, 5), Color.LimeGreen);
            }

            if (playerIsAlive == false)
            {
                spriteBatch.DrawString(berlinFont, "YOU DIED!", new Vector2(graphics.GraphicsDevice.Viewport.Width / 2.5f, 5), Color.LimeGreen);
            }

        }

        // Draw Game Over State 
        protected void DrawGameOverState(SpriteBatch spriteBatch)
        {
            int tileWidth = (graphics.GraphicsDevice.Viewport.Width / backgroundTexture.Width) + 1;
            int tileHeight = (graphics.GraphicsDevice.Viewport.Height / backgroundTexture.Height) + 1;

            for (int column = 0; column < tileWidth; column += 1)
            {
                for (int row = 0; row < tileHeight; row += 1)
                {
                    Vector2 position = new Vector2(column * backgroundTexture.Width, row * backgroundTexture.Height);

                    spriteBatch.Draw(backgroundTexture, position, Color.White);
                }
            }

            spriteBatch.DrawString(berlinFont, "GAME OVER!", new Vector2(300, 150), Color.White);
            spriteBatch.DrawString(berlinFont, "Press ENTER to Try Again!", new Vector2(300, 200), Color.White);
            spriteBatch.DrawString(berlinFont, "Press ESC to Ragequit!", new Vector2(300, 150), Color.White);
        }

        // Draw Win State 
        protected void DrawWinState(SpriteBatch spriteBatch)
        {
            int tileWidth = (graphics.GraphicsDevice.Viewport.Width / backgroundTexture.Width) + 1;
            int tileHeight = (graphics.GraphicsDevice.Viewport.Height / backgroundTexture.Height) + 1;

            for (int column = 0; column < tileWidth; column += 1)
            {
                for (int row = 0; row < tileHeight; row += 1)
                {
                    Vector2 position = new Vector2(column * backgroundTexture.Width, row * backgroundTexture.Height);

                    spriteBatch.Draw(backgroundTexture, position, Color.White);
                }
            }

            spriteBatch.DrawString(berlinFont, "YOU WON!", new Vector2(300, 150), Color.White);
            spriteBatch.DrawString(berlinFont, "Press ENTER to Play Again!", new Vector2(300, 200), Color.White);
            spriteBatch.DrawString(berlinFont, "Press ESC to Exit!", new Vector2(300, 250), Color.White);
        }

        // Draw Function 
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            spriteBatch.Begin();

            switch (gameState)
            {
                case STATE_SPLASH:
                    DrawSplashState(spriteBatch);
                    break;

                case STATE_GAME:
                    DrawGameState(spriteBatch);
                    break;

                case STATE_GAMEOVER:
                    DrawGameOverState(spriteBatch);
                    break;

                case STATE_WIN:
                    DrawWinState(spriteBatch);
                    break;
            }


            spriteBatch.End();

            base.Draw(gameTime);
        }

        // Circle-to-Circle Collision Test 
        protected bool IsColliding(Vector2 position1, float radius1, Vector2 position2, float radius2)
        {
            Vector2 distance = position2 - position1;

            if (distance.Length() < radius1 + radius2)
            {
                return true;
            }
            return false;
        }

    }

}