 using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

 using KiloWatt.Base.Graphics;
using KiloWatt.Base.Animation;
 
 namespace Modell_Lader
{
	public partial class Game1
	{

		
		
		public void LadeModell(string dateiname)
		{

			// Da die Dateien unterschiedlich kompiliert sind, muss schon beim Einlesen unterschieden werden,
			// ob sie als normales XNA "Model" oder als Kilowatt "ModelDraw" kompiliert wurden.

			// Bei KW erstnochmal aussteigen ;-)
			if (dateiname.Substring(0, 2).ToLower().Equals("kw"))
			{
				geladenesMod.kwModel = new ModelDraw(Content.Load<Model>(dateiname), dateiname);
				geladenesMod.isKw = true;
			}
			else
			{
				geladenesMod.xnaModel = Content.Load<Model>(dateiname);
				geladenesMod.isKw = false;
			}


			// Alle Objekt-Meshes in eine große Bounding-Sphere packen um den Abstand
			// der Kamera so zu bestimmen, dass man gleich alles sieht.
			BoundingSphere szeneSphere = new BoundingSphere();
			foreach (ModelMesh mesh in geladenesMod.getModel().Meshes)
				szeneSphere = BoundingSphere.CreateMerged(szeneSphere, mesh.BoundingSphere);
			geladenesMod.position = -szeneSphere.Center;
			camDist = szeneSphere.Radius*2; camPos = new Vector3(0, 0, camDist);
			camTarget = szeneSphere.Center;
			camClip1 = camPos.Length() / 20; camClip2 = camPos.Length() * 2;





			float distanz = szeneSphere.Radius / (float)Math.Sin(MathHelper.ToRadians(45) / 2);
			camClip1 = distanz / 100; camClip2 = distanz * 40;


			camView = Matrix.CreateLookAt(camPos, Vector3.Backward, Vector3.Up);
			//camPos, kugel.Center, Vector3.Up);
			Vector3 back = camView.Backward;
			back.X = -back.X;
			camPos += (back * distanz);

			// debug
			szeneSphereGLOB = szeneSphere;

			/*
			camPos = Vector3.Zero;
			camView = Matrix.CreateLookAt(camPos, Vector3.Backward, Vector3.Up);
			

			Vector3 back = camView.Backward;
			back.X = -back.X;
			camPos += (back * distanz);
			//Window.Title = Convert.ToString(geladenesModell.Meshes.Count);
			//Convert.ToString(camClip1) + " // " + Convert.ToString(camClip2);
			camProj = Matrix.CreatePerspectiveFieldOfView(
				(float)Math.Sin(MathHelper.ToRadians(45)),
				GraphicsDevice.Viewport.AspectRatio,
				camClip1, camClip2);
			*/


		}



		public void SetupRenderer()
		{

			//camTarget = szeneSphereGLOB.Center;

			camView = Matrix.CreateLookAt(camPos, camTarget, Vector3.Up);
			camProj = Matrix.CreatePerspectiveFieldOfView(
				(float)Math.Sin(MathHelper.ToRadians(45)),
				GraphicsDevice.Viewport.AspectRatio,
				camClip1, camClip2);


		}


		public void DrawModel(bool kWStyle)
		{


			if (!kWStyle)
			{

				Model tempMod = geladenesMod.getModel();

				// Relative Positionen der Bones berücksichtigen
				Matrix[] relpos = new Matrix[tempMod.Bones.Count];
				tempMod.CopyAbsoluteBoneTransformsTo(relpos);

				foreach (ModelMesh mesh in tempMod.Meshes)
				{
					foreach (BasicEffect effekt in mesh.Effects)
					{
						effekt.EnableDefaultLighting();
						effekt.Projection = camProj;
						effekt.View = camView;
						effekt.World = relpos[mesh.ParentBone.Index] *
							Matrix.CreateRotationY(MathHelper.ToRadians(yRot)) * Matrix.CreateScale((float)zoomfaktor)
							// *Matrix.CreateTranslation(-modOffset.X, -modOffset.Y, -modOffset.Z)
							;

					}

					mesh.Draw();
				}



			}
			else
			{
			
				Matrix[] relpos = new Matrix[geladenesMod.getModel().Bones.Count];
				geladenesMod.getModel().CopyAbsoluteBoneTransformsTo(relpos);
	
				dd.dev = GraphicsDevice;
				dd.fogColor = new Vector4(0.5f, 0.5f, 0.5f, 1);

				dd.fogDistance = camClip2;
				dd.lightAmbient = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
				dd.lightDiffuse = new Vector4(0.8f, 0.8f, 0.8f, 0);
				dd.lightDir = Vector3.Normalize(new Vector3(1, 3, 2));

				// Die die dd.World-Scheiße irgendwie grundsätzlich ohne Effekt bleibt,
				// wird die Objekt-Rotation halt noch in die Kamera-Matrix reingerechnet,
				// auf Dauer irgendwie kein großer Spaß, vor allem weil das für jedes Objekt
				// einzeln gemacht werden muss, obwohl man es ja eigentlich schon hat...

				
				camPos = new Vector3(
					(float)(camDist * Math.Cos(MathHelper.ToRadians(yRot))),
					0,
					(float)(camDist * Math.Sin(MathHelper.ToRadians(yRot)) )
				);
				Matrix tempView = Matrix.CreateLookAt(camPos, camTarget, Vector3.Up);

				dd.viewInv = Matrix.Invert(tempView);
				dd.viewProj = tempView * camProj * Matrix.CreateScale((float)zoomfaktor);



				dd.world = Matrix.Identity; // *
							//Matrix.CreateRotationY(MathHelper.ToRadians(yRot)); // * 
							//Matrix.CreateScale((float)zoomfaktor);
				geladenesMod.kwModel.SceneDraw(dd);
				geladenesMod.kwModel.SceneDrawTransparent(dd); 


			}




		}




	}
}