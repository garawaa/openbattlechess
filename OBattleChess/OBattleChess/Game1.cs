using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace OBattleChess
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        List<GameObject> GameObjectCollection = new List<GameObject>();

        GameObject Chessboard = new GameObject("std");
        GameObject WhiteKing = new GameObject("std");
        GameObject WhiteQueen = new GameObject("std");
        GameObject WhiteRook = new GameObject("std");

        LogicHelper logicHelper = new LogicHelper();

        Vector3 cameraPosition = new Vector3(145.0f, 0.0f, -80.0f);
        Vector3 cameraLookAt = new Vector3(0.0f, 0.0f, 0.0f);

        Matrix cameraProjectionMatrix;
        Matrix cameraViewMatrix;

        GamePadState PrevGPS;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            cameraViewMatrix = Matrix.CreateLookAt(
                cameraPosition,
                cameraLookAt,
                Vector3.Forward);

            cameraProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                graphics.GraphicsDevice.Viewport.AspectRatio,
                1.0f,
                10000.0f);

            Chessboard.model = Content.Load<Model>("Models\\Chessboard");
            Chessboard.rotation = Vector3.Zero;
            Chessboard.position = new Vector3(0.0f, 0.0f, 19.2f);
            GameObjectCollection.Add(Chessboard);

            /* White King */
            WhiteKing.model = Content.Load<Model>("Models\\PWhite\\WhiteKing");
            WhiteKing.position = logicHelper.ToChessBoardPos(1, 4);
            GameObjectCollection.Add(WhiteKing);

            /* White Queen */
            WhiteQueen.model = Content.Load<Model>("Models\\PWhite\\WhiteQueen");
            WhiteQueen.position = logicHelper.ToChessBoardPos(1, 5);
            GameObjectCollection.Add(WhiteQueen);

            /* White Rook 1 */
            WhiteRook.model = Content.Load<Model>("Models\\PWhite\\WhiteRook");
            WhiteRook.position = logicHelper.ToChessBoardPos(2, 1);
            //GameObjectCollection.Add(WhiteRook);


        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            GamePadState Player1 = GamePad.GetState(PlayerIndex.One);

            if (Player1.Buttons.RightShoulder == ButtonState.Released)
            {

                if ((Player1.ThumbSticks.Left.X > 0.4f) && (PrevGPS.ThumbSticks.Left.X < 0.4f))
                {
                    //Right
                    WhiteKing.position -= new Vector3(0.0f, 14.8f, 0.0f);

                }
                if ((Player1.ThumbSticks.Left.X < -0.4f) && (PrevGPS.ThumbSticks.Left.X > -0.4f))
                {
                    //Left
                    WhiteKing.position += new Vector3(0.0f, 14.8f, 0.0f);
                }
                if ((Player1.ThumbSticks.Left.Y > 0.4f) && (PrevGPS.ThumbSticks.Left.Y < 0.4f))
                {
                    //Up
                    WhiteKing.position -= new Vector3(14.8f, 0.0f, 0.0f);
                }
                if ((Player1.ThumbSticks.Left.Y < -0.4f) && (PrevGPS.ThumbSticks.Left.Y > -0.4f))
                {
                    //Down
                    WhiteKing.position += new Vector3(14.8f, 0.0f, 0.0f);
                }




                if ((Player1.ThumbSticks.Right.X > 0.4f) && (PrevGPS.ThumbSticks.Right.X < 0.4f))
                {
                    //Right
                    WhiteQueen.position -= new Vector3(0.0f, 14.8f, 0.0f);

                }
                if ((Player1.ThumbSticks.Right.X < -0.4f) && (PrevGPS.ThumbSticks.Right.X > -0.4f))
                {
                    //Left
                    WhiteQueen.position += new Vector3(0.0f, 14.8f, 0.0f);
                }
                if ((Player1.ThumbSticks.Right.Y > 0.4f) && (PrevGPS.ThumbSticks.Right.Y < 0.4f))
                {
                    //Up
                    WhiteQueen.position -= new Vector3(14.8f, 0.0f, 0.0f);
                }
                if ((Player1.ThumbSticks.Right.Y < -0.4f) && (PrevGPS.ThumbSticks.Right.Y > -0.4f))
                {
                    //Down
                    WhiteQueen.position += new Vector3(14.8f, 0.0f, 0.0f);
                }


                if (Player1.Buttons.A == ButtonState.Pressed)
                {
                    if (GameObjectCollection.Contains(WhiteRook) == false)
                        GameObjectCollection.Add(WhiteRook);
                }

            }
            //WhiteKing.scale += Player1.ThumbSticks.Right.Y;
            else
            {

                cameraPosition.Y += Player1.ThumbSticks.Left.Y;
                cameraPosition.X += Player1.ThumbSticks.Left.X;
                cameraPosition.Z += Player1.ThumbSticks.Right.Y;
            }



            PrevGPS = Player1;

            //Chessboard.scale += Player1.ThumbSticks.Right.Y;

            this.Window.Title = "Open Battle Chess V0.1 PreAlpha";

            /*
            Vector2 Bla = logicHelper.GetAbsolutePosition('a', 1);
            this.Window.Title = Convert.ToString(Bla);
            */
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            foreach (GameObject Obj in GameObjectCollection)
            {
                DrawGameObject(Obj);
            }

            /******************************************************/
            cameraViewMatrix = Matrix.CreateLookAt(
                cameraPosition,
                cameraLookAt,
                Vector3.Forward);

            cameraProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                graphics.GraphicsDevice.Viewport.AspectRatio,
                1.0f,
                10000.0f);

            cameraLookAt = Chessboard.position;
            /*******************************************************/
            base.Draw(gameTime);
        }

        void DrawGameObject(GameObject gameobject)
        {
            foreach (ModelMesh mesh in gameobject.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    effect.World =
                        Matrix.CreateFromYawPitchRoll(
                            gameobject.rotation.Y,
                            gameobject.rotation.X,
                            gameobject.rotation.Z) *
                        Matrix.CreateScale(gameobject.scale) *

                        Matrix.CreateTranslation(gameobject.position);


                    effect.Projection = cameraProjectionMatrix;
                    effect.View = cameraViewMatrix;

                }
                mesh.Draw();
            }
        }
    }
}
