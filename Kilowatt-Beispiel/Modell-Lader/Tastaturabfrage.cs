using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Input;


/* 
 * Einfach eine Zeile über die Tastatur Buchstaben (groß und klein) und Leerzeichen eingeben, sowie löschen.
 * Umlaute, Zahlen, Punkte und Backslash bleiben erstmal außen vor.
 * */

namespace Modell_Lader
{
	public class Tastaturabfrage
	{

		// Benötigte Variablen
		private string kompletteEingabe = ""; // Die bislang eingegebene Zeile
		private KeyboardState altKState = Keyboard.GetState();
		public List<string> gültigeChars =
			new List<string>(
				new string[] {
						"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" // , "ä", "ü", "ö", "ß", ".", "\\"
					}
			)
		;

		bool fertigEingegeben; // True, sobald Return gedrückt wurde

		// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ 
		

		public string getEingabe()
		{
			return kompletteEingabe;
		}

		public void resetEingabe()
		{
			kompletteEingabe = "";
			fertigEingegeben = false;
		}

		public bool fertig()
		{
			return fertigEingegeben;
		}


		public void syncTastaturEingabeZeile() // aktualisiert string kompletteEingabe, aufrufen in Game.Update()
		{

			KeyboardState kState = Keyboard.GetState();
			Keys[] currentlyPressed = kState.GetPressedKeys();

			foreach (Keys key in currentlyPressed)
			{
				if (gültigeChars.Contains(key.ToString().ToLower()) && (!altKState.GetPressedKeys().Contains(key)))
				{
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
					&& kompletteEingabe.Length > 0)
						kompletteEingabe = kompletteEingabe.Substring(0, kompletteEingabe.Length - 1);


				if (kState.GetPressedKeys().Contains(Keys.Enter)
					&& !(altKState.GetPressedKeys().Contains(Keys.Enter))) fertigEingegeben = true;

			}

			altKState = kState;


		}



	}
}
