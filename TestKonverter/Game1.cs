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

// NEU
using EES.ContentCompiler;

/* == XNB-Testkonverter ==
 * 
 * Wandelt mittels .\bin\x86\Debug\ccompiler.dll die Datei .\INPUT\pferd_x.x und alle Dateien, die
 * eben von pferd_x.x benötigt werden um. Ausgabepfad ist .\OUTPUT\*
 * 
 * Wozu das ganze gut ist:
 * http://koehler.sk-medien.net/wiki/index.php/XNA:Dateien_dynamisch_Laden
 * */


namespace TestKonverter
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;

		bool erfolg = false;
		int zeit;

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
			ContentCompiler.Initialize();

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{

			ContentCompiler.XNAVersion = "3.0"; // Eigentlich müsste es auf 3.1 stehen
			// Da der ESS.ContentCompiler zur Laufzeit irgendwo unter \[Benutzername]\Lokale Einstellungen\Temp
			// lauert, kann das Ding nur mit absoluten Pfadangaben arbeiten ...
			ContentCompiler.OutputDirectory = Environment.CurrentDirectory+".\\..\\..\\..\\OUTPUT";
			ContentCompiler.Files.Add(Environment.CurrentDirectory+".\\..\\..\\..\\INPUT\\pferd_x.fbx");

			zeit = System.Environment.TickCount;
			erfolg = ContentCompiler.BuildContent();
			zeit = System.Environment.TickCount - zeit;

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

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			if (erfolg) GraphicsDevice.Clear(Color.AliceBlue);
			else GraphicsDevice.Clear(Color.Black);		

			Window.Title = "Benötigte Zeit: "+Convert.ToString(zeit)+ " ms";

			base.Draw(gameTime);
		}
	}
}
