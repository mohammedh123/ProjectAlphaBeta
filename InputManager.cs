using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace CS6613_Final
{
    public static class InputManager
    {
        static MouseState oldMouseState = Mouse.GetState();

        public static bool LeftMouseClick()
        {
            return Mouse.GetState().LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released;
        }

        public static bool LeftMouseDown()
        {
            return Mouse.GetState().LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed;
        }

        public static bool LeftMouseReleased()
        {
            return Mouse.GetState().LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed;
        }

        public static void PostUpdate()
        {
            oldMouseState = Mouse.GetState();
        }

        public static Location GetLocationFromMouse()
        {
            return new Location(Mouse.GetState().X, Mouse.GetState().Y);
        }
    }
}
