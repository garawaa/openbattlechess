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
    class LogicHelper
    {

        //Returns Absolute X,Y Position by Chessboard Position
        public Vector2 GetAbsolutePosition(Vector2 Pos)
        {
            // A1 = (56,52)

            int x, y;
            x = (int)Pos.X;
            y = (int)Pos.Y;

            Vector2 RetVal = new Vector2(x, y);

            RetVal.X = 56 - ((x - 1) * 14.8f);
            RetVal.Y = 52 - ((y - 1) * 14.8f);

            return RetVal;
        }

        //Returns Chessboard Position by Absolute Position
        public Vector2 GetChessboardPosition(Vector2 Pos)
        {
            Vector2 RetVal = Vector2.Zero; ;
            float x, y;
            x = Pos.X;
            y = Pos.Y;

            RetVal.X = Convert.ToInt16(((x - 56.0f) / (-14.8f)) + 0.1f) + 1;
            RetVal.Y = Convert.ToInt16(((y - 52.0f) / (-14.8f)) + 0.1f) + 1;
            return RetVal;
        }

        public Vector2 Vector3to2(Vector3 V3)
        {
            return new Vector2(V3.X, V3.Y);
        }

        public Vector3 Vector2to3(Vector2 V2)
        {
            return new Vector3(V2.X, V2.Y, 0.0f);
        }

        public Vector3 ToChessBoardPos(int x, int y)
        {
            return Vector2to3(GetAbsolutePosition(new Vector2(x, y)));
        }

    }
}
