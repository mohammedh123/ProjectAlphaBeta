using Microsoft.Xna.Framework.Input;

namespace CS6613_Final
{
    public static class InputManager
    {
        private static MouseState _oldMouseState = Mouse.GetState();

        public static bool LeftMouseClick()
        {
            return Mouse.GetState().LeftButton == ButtonState.Pressed &&
                   _oldMouseState.LeftButton == ButtonState.Released;
        }

        public static bool LeftMouseDown()
        {
            return Mouse.GetState().LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Pressed;
        }

        public static bool LeftMouseReleased()
        {
            return Mouse.GetState().LeftButton == ButtonState.Released &&
                   _oldMouseState.LeftButton == ButtonState.Pressed;
        }

        public static void PostUpdate()
        {
            _oldMouseState = Mouse.GetState();
        }

        public static Location GetLocationFromMouse()
        {
            return new Location(Mouse.GetState().X, Mouse.GetState().Y);
        }
    }
}