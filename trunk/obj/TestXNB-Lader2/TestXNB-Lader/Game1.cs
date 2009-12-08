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

using System.IO; // für file.exists()

using KiloWatt.Base.Graphics;
using KiloWatt.Base.Animation;

/*
 * 
 * NEU: Jetzt mit Kilowatt (wenn auch erstmal vollkommen sinnlos, da Objekte alle nicht animiert sind).
 * Gültige Dateinamen sind "Raumschiff" und "Pferdefigur".
 * Wichtig zu wissen ist auch, dass durch die Tatsache, dass Kilowatt ein "Custom Importer" ist, dieser
 * (noch) nicht von externen Tools/APIs wie bsp. dem "EES Content Compiler" abgedeckt wird und somit
 * das Laden von unkonvertierten (also nicht-xnb-objekten) erstmal nicht möglich ist, die Dateien,
 * die im Verzeichnis liegen, aus denen geladen wird, wurden also mit dem neuen Content Processor von
 * Kilowatt (wie normal aus dem Visual Studio heraus) nach xnb konvertiert.
 * */



/* -- 3D-Modelle Dateien dynamisch zur Laufzeit laden -- */

/* Anleitung:
 * Auf der Tastatur den Dateinamen des Models eingeben. Mögliche Eingaben sind alle Models, die
 * unter .\bin\x86\Debug\Content\INPUT liegen - In diesem Fall BlackKing, Pferdefigur, Raumschiff (absichtlich
 * falsche Normalen) und ship, der Rest sind die Texturen für die Modelle.
 * Nach Eingabe den Namen mit Enter bestätigen, Linke Maustaste+Bausbewegung: Um das Objekt rotieren,
 * Mausrad: Rein/Rauszoomen.
 * */


namespace TestXNB_Lader
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;


		// Variablen für Tastatur-Eingabe
			string kompletteEingabe = "";
			KeyboardState altKState = Keyboard.GetState();

			List<string> gültigeChars = 
				new List<string>(
					new string[] {
						"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" // , "ä", "ü", "ö", "ß", ".", "\\"
					}
				)
			;

			SpriteFont font;
			bool fertigEingegeben = false;
		// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ 

		
		// Modell drehen mit der Maus
		MouseState mState, altMState;
		float zoomfaktor = 1;

		enum eProgrammStatus
		{
			sDateinamen_eingeben,
			sDateiname_eingeben_falsch,
			sModell_laden,
			sModell_anzeigen
		};
		eProgrammStatus programmStatus = eProgrammStatus.sDateinamen_eingeben;


		// Kilowatt
		DrawDetails drawDetails = new DrawDetails();
		ModelDraw KilowattModel;
		// test
		float baseDist;
		BoundingSphere szenesphereGLOB; // 

		
		Model geladenesModell;
		Matrix camView, camProj;
		Vector3 camPos, camLook;
		float camDist, camRot, camKipp; // Kamera-Rotation um die Y und um die Z-Achse des Objekts
		float camClip1, camClip2;

		float objektYRot = 0;


		public void GetTastaturEingabeZeile() {

			KeyboardState kState = Keyboard.GetState();
			Keys[] currentlyPressed = kState.GetPressedKeys();
			
			foreach (Keys key in currentlyPressed)
			{
				if (gültigeChars.Contains(key.ToString().ToLower()) && (!altKState.GetPressedKeys().Contains(key) )  ) {
					kompletteEingabe += 
						(kState.IsKeyDown(Keys.LeftShift) || kState.IsKeyDown(Keys.RightShift)
						? key.ToString().ToUpper()
						: key.ToString().ToLower());
				}

				if (
					kState.GetPressedKeys().Contains(Keys.Space)
					&& !altKState.GetPressedKeys().Contains(Keys.Space))
						kompletteEingabe = kompletteEingabe + " ";

				if (
					kState.GetPressedKeys().Contains(Keys.Back)
					&& !altKState.GetPressedKeys().Contains(Keys.Back)
					&& kompletteEingabe.Length>0)
						kompletteEingabe = kompletteEingabe.Substring(0, kompletteEingabe.Length - 1)
				;


				if (kState.GetPressedKeys().Contains(Keys.Enter)) fertigEingegeben = true;

			}

			altKState = kState;
			
	
		}




		public void LadeModell(string dateiname)
		{
			//Content.RootDirectory = Directory.GetCurrentDirectory() + "\\..\\..\\..\\INPUT\\";
			//geladenesModell = Content.Load<Model>("INPUT\\"+dateiname);
			KilowattModel = new ModelDraw(Content.Load<Model>("INPUT\\"+dateiname), "INPUT\\"+dateiname);
			
			geladenesModell = KilowattModel.Model;

			// Das Objekt in eine Kugel packen, Größe bestimmen
			BoundingSphere kugel = new BoundingSphere(); 
			foreach(ModelMesh mesh in geladenesModell.Meshes) 
				kugel = BoundingSphere.CreateMerged(kugel, mesh.BoundingSphere);
			szenesphereGLOB = kugel;
			camPos = kugel.Center + new Vector3(0, 0, kugel.Radius);
			camDist = kugel.Radius;  camLook = kugel.Center;
			float distanz = kugel.Radius / (float)Math.Sin(MathHelper.ToRadians(45) / 2);
			camClip1 = distanz / 100; camClip2 = distanz * 40;
			

			camView = Matrix.CreateLookAt(camPos, Vector3.Backward, Vector3.Up);
				//camPos, kugel.Center, Vector3.Up);
			Vector3 back = camView.Backward;
            back.X = -back.X;
            camPos += (back * distanz);
			//Window.Title = Convert.ToString(geladenesModell.Meshes.Count);
				//Convert.ToString(camClip1) + " // " + Convert.ToString(camClip2);
			camProj = Matrix.CreatePerspectiveFieldOfView(
				(float)Math.Sin(MathHelper.ToRadians(45)),
				GraphicsDevice.Viewport.AspectRatio,
				camClip1, camClip2);



			programmStatus = eProgrammStatus.sModell_anzeigen;
			//Window.Title = "fertig geladen";
				

		
		}


		public void RenderModell()
		{
			
			Matrix[] relPos = new Matrix[geladenesModell.Bones.Count];
			geladenesModell.CopyAbsoluteBoneTransformsTo(relPos);

			
			foreach (ModelMesh mesh in geladenesModell.Meshes)
			{
				foreach (BasicEffect effekt in mesh.Effects)
				{
					effekt.EnableDefaultLighting();
					effekt.View = camView;
					effekt.Projection = camProj;
					effekt.World = relPos[mesh.ParentBone.Index] * Matrix.CreateRotationY(objektYRot) * Matrix.CreateScale(1f/Math.Abs(zoomfaktor));

				}
				//GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
				mesh.Draw();
			}
			
			  
			  
			// Wie unten sollte es normalerweise (?) gerendert werden, auf die obige Art und Weise hat man ja
			// nach wie vor keinen Zugriff auf die neuen Kilowatt-Sachen.
			// Um ehrlich zu sein, kriege ich es aber partout nicht gebacken, dass das Ding richtig
			// rendert, an den Matrizen muss irgendwas falsch sein, wär nett, wenn sich das
			// jemand mal ansehen könnte ...

			//  the drawdetails set-up can be re-used for all items in the scene

			/*
			DrawDetails dd = drawDetails;
			
			dd.dev = GraphicsDevice;
			dd.fogColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
			
			dd.fogDistance = 100000;
			dd.lightAmbient = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
			dd.lightDiffuse = new Vector4(0.8f, 0.8f, 0.8f, 0);
			dd.lightDir = Vector3.Normalize(new Vector3(1, 3, 2));

			dd.viewInv = Matrix.Invert(camView);
			dd.viewProj = camView * camProj;
			dd.world = Matrix.Identity;

			//  draw the loaded model (the only model I have)
			KilowattModel.SceneDraw(dd);
			KilowattModel.SceneDrawTransparent(dd); 
			*/
			

			
		}


		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{

			altMState = Mouse.GetState();
			base.Initialize();

			

	
			
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			font = Content.Load<SpriteFont>("SpriteFont1");


			// TODO: use this.Content to load your game content here
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			// TODO: Add your update logic here

			if (programmStatus < eProgrammStatus.sModell_laden)
			{
				if (!fertigEingegeben) GetTastaturEingabeZeile();
				else
				{
					if (File.Exists(
						Directory.GetCurrentDirectory() + "\\Content" +
						 "\\Input\\" + kompletteEingabe + ".xnb"))
						programmStatus = eProgrammStatus.sModell_laden;
					else
					{
						programmStatus = eProgrammStatus.sDateiname_eingeben_falsch;
						kompletteEingabe = "";
						fertigEingegeben = false;

					};

				};
			}
			else if (programmStatus == eProgrammStatus.sModell_anzeigen)
			{

				// Maussteuerung
				mState = Mouse.GetState();
				if (mState.LeftButton == ButtonState.Pressed)
				{
					camRot -= (float)(altMState.X - mState.X);
					camKipp += (float)(altMState.Y - mState.Y);
				}

					zoomfaktor =
					(float)mState.ScrollWheelValue / 200;


					// Kamera-Matrizen neu setzen
					Vector3 dist = camLook-camPos; // Vektor von Lookat zu Pos

					double l = camDist*zoomfaktor;
					camPos = new Vector3(
						(float)(l * Math.Cos(MathHelper.ToRadians(camRot))*Math.Sin(MathHelper.ToRadians(camKipp))),
						(float)((l * Math.Cos(MathHelper.ToRadians(camKipp)))),
						(float)(l * Math.Sin(MathHelper.ToRadians(camRot)) * Math.Sin(MathHelper.ToRadians(camKipp)))
					); 

					Window.Title = Convert.ToString(szenesphereGLOB.Radius );
					
					/*camPos = (camPos - dist);
					//dist =	Vector3.Transform(dist, Matrix.CreateRotationY(camRot));					
					camPos += dist;
					camPos.Y += camKipp*3; */


					objektYRot += 0.01f;

					camProj = Matrix.CreatePerspectiveFieldOfView(
						(float)Math.Sin(MathHelper.ToRadians(45)),
						GraphicsDevice.Viewport.AspectRatio,
						camClip1, camClip2);
					camView = Matrix.CreateLookAt(camPos, camLook, Vector3.Up);

				}
				altMState = mState;


			//base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			
			Color drawFarbe = Color.BurlyWood;
			if(programmStatus < eProgrammStatus.sModell_laden)
			{
				switch (programmStatus) {
					case eProgrammStatus.sDateinamen_eingeben : drawFarbe = Color.CornflowerBlue; break;
					case eProgrammStatus.sDateiname_eingeben_falsch : drawFarbe = Color.Red; break;
				}
			} else drawFarbe=Color.White;


			GraphicsDevice.Clear(drawFarbe);


			if ((programmStatus < eProgrammStatus.sModell_laden))
			{
				spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);

				if (programmStatus == eProgrammStatus.sDateiname_eingeben_falsch)
				{
					spriteBatch.DrawString(font, "Datei existiert nicht!", Vector2.Zero, Color.Wheat);
					spriteBatch.DrawString(font, kompletteEingabe, new Vector2(0, 28), Color.Wheat);
				}
				else spriteBatch.DrawString(font, kompletteEingabe, Vector2.Zero, Color.Wheat);

				spriteBatch.End();
			}
			// Das beste wäre es wohl, das Modell in der Draw-Schleife zu laden, die Update-Methode
			// sollte man wahrscheinlich so wenig wie möglich belasten, sollte das Laden lange dauern,
			// gibt es sonst vielleicht das typische "Anwendung reagiert nicht".
			// Um ehrlich zu sein: Keine Ahnung ...
			else
			{

				if (programmStatus == eProgrammStatus.sModell_laden)
					LadeModell(kompletteEingabe);
				else
					RenderModell();
								
			}
			

			base.Draw(gameTime);
		}
	}
}
