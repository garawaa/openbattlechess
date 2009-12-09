using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KiloWatt.Base.Graphics;
using KiloWatt.Base.Animation;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace Modell_Lader
{
	public class Modell
	{

		public Modell()
		{

			isKw = false; 
		}
		
		// Da man in C# anscheinend überhaupt keine vernünftigen Zeiger auf Objekte erstellen kann,
		// ist das jetzt natürlich alles andere als Optimal: Wenn das Modell ein KiloWatt-Model enthält,
		// dann ist isKw logischerweise true und in ModelDraw steht alles drin.
		// Ist es als normales XNA-Model geladen worden, dann steht alles in xnaModel.
		// ("Das Übernehmen der Adresse eines verwalteten Objekts, das Abrufen seiner Größe oder das Deklarieren eines Zeigers für den verwalteten Typ ist auch in Verbindung mit dem unsafe-Schlüsselwort nicht zulässig.")
		public ModelDraw kwModel;
		public Model xnaModel;
		public bool isKw;

		public Vector3 position;
		public Vector3 rotation;


	

	
	



		public Model getModel() {
			if (!isKw) return xnaModel;
			else return kwModel.Model;
		}
			


		

	}
}
