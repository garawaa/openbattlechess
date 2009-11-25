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
    class GameObject
    {
        public Model model = null;
        public Vector3 position = Vector3.Zero;
        public Vector3 rotation = Vector3.Zero;
        public float scale = 1.0f;

        public GameObject(string PreSettings)
        {
            if (PreSettings == "std")
            {
                rotation = new Vector3(MathHelper.ToRadians(-90.0f), 0.0f, 0.0f);
                scale = 60.0f;
            }
        }

    }
}
