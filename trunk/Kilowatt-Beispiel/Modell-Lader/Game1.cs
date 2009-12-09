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

using System.IO; // für File.Exists()

using KiloWatt.Base.Graphics;

namespace Modell_Lader
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public partial class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;


		// debug
		int yRot;
		BoundingSphere szeneSphereGLOB;
		float zoomfaktor=1;

		Tastaturabfrage tastatur = new Tastaturabfrage();


		MouseState mState, altMState;

		// Programmstatus
		enum eProgrammStatus {
			Anleitung_anzeigen,
			Dateinamen_einlesen,
			Dateinamen_einlesen_falsch,
			Dauerschleife
		};
		eProgrammStatus sProgrammStatus = eProgrammStatus.Anleitung_anzeigen;


		// Anleitung anzeigen
			Texture2D Hinweisbildschirm;
			SpriteFont eingabeFont;


		// Das Modell
			Modell geladenesMod = new Modell();
			//Model geladenesModell;
			//Vector3 modOffset;

		// Kamera-Variablen
			Matrix camView, camProj;
			Vector3 camPos, camTarget;
			float camDist, camClip1, camClip2;
			DrawDetails dd = new DrawDetails();

		// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ 	

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = ".\\..\\Data";
		
	
			

		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			

			tastatur.resetEingabe();
			altMState = Mouse.GetState();
			


			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			
			spriteBatch = new SpriteBatch(GraphicsDevice);
			Hinweisbildschirm = Content.Load<Texture2D>("hinweisbild");
			eingabeFont = Content.Load<SpriteFont>("Schriftart");

			
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

			if (sProgrammStatus == eProgrammStatus.Anleitung_anzeigen)
			{
				tastatur.syncTastaturEingabeZeile();
				if (tastatur.fertig())
				{
					sProgrammStatus = eProgrammStatus.Dateinamen_einlesen;
					tastatur.resetEingabe(); 
				}
			}


			if (sProgrammStatus==eProgrammStatus.Dateinamen_einlesen
				|| sProgrammStatus==eProgrammStatus.Dateinamen_einlesen_falsch)
				tastatur.syncTastaturEingabeZeile();


			if (sProgrammStatus == eProgrammStatus.Dauerschleife)
			{

				// Mausklick + Bewegung: Rotiert.
				mState = Mouse.GetState();
				yRot -= altMState.X - mState.X;


				// Mausrad-Zoom
				int dif = altMState.ScrollWheelValue - mState.ScrollWheelValue;
				if (dif != 0)
					zoomfaktor *= (float) Math.Pow(0.5, 120f / dif);
				//Window.Title = Convert.ToString(szeneSphereGLOB.Center)+"  //  "+Convert.ToString(szeneSphereGLOB.Radius) ;
					//"zoomf = " + Convert.ToString(((float)zoomfaktor));


				altMState = mState;

			}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			


			if (sProgrammStatus == eProgrammStatus.Anleitung_anzeigen)
			{
				// SaveState ist wichtig, damit es nicht das 3D-Rendern zerfickt.				
				spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
				spriteBatch.Draw(Hinweisbildschirm, new Rectangle(0, 0, 800, 600), Color.White);
				spriteBatch.End();
			}




			if (sProgrammStatus == eProgrammStatus.Dateinamen_einlesen
				|| sProgrammStatus == eProgrammStatus.Dateinamen_einlesen_falsch)
			{

				if (tastatur.fertig())
				{
					// Dateinamen prüfen
					if (File.Exists(".\\..\\Data\\" + tastatur.getEingabe() + ".xnb")) {
						LadeModell(tastatur.getEingabe());
						SetupRenderer();
						//Window.Title = Convert.ToString(modOffset);
						sProgrammStatus = eProgrammStatus.Dauerschleife;}
					else
					{
						sProgrammStatus = eProgrammStatus.Dateinamen_einlesen_falsch;
						tastatur.resetEingabe();
					}

				}
				else
				{
					if (sProgrammStatus == eProgrammStatus.Dateinamen_einlesen)
						GraphicsDevice.Clear(Color.LightBlue);
					else GraphicsDevice.Clear(Color.Red);
					
					spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
					spriteBatch.DrawString(eingabeFont, "Dateinamen (ohne '.xnb') eingeben:", new Vector2(0, 0), Color.Black);
					spriteBatch.DrawString(eingabeFont, ">" + tastatur.getEingabe(), new Vector2(0, 26), Color.Black);
					spriteBatch.End();
					//Window.Title = Convert.ToString(resetCount);
				}
			}



			if (sProgrammStatus == eProgrammStatus.Dauerschleife)
			{
				GraphicsDevice.Clear(Color.White);
				SetupRenderer();
				DrawModel(geladenesMod.isKw);


			}




			base.Draw(gameTime);
		}
	}
}
