using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Gem
{
    public class InputBinding
    {
        public virtual bool Check(Input input) { return false; }
    }

    public enum KeyBindingType
    {
        Pressed,
        Held
    }

    public class KeyboardBinding : InputBinding
    {
        public Keys key;
        public KeyBindingType bindingType;

        public KeyboardBinding(Keys key, KeyBindingType bindingType)
        {
            this.key = key;
            this.bindingType = bindingType;
        }

        public override bool Check(Input input)
        {
            if (bindingType == KeyBindingType.Held)
                return input.currentState.IsKeyDown(key);
            else if (bindingType == KeyBindingType.Pressed)
                return input.currentState.IsKeyDown(key) && input.previousState.IsKeyUp(key);
            else
                return false;
        }
    }

    public class GamepadButtonBinding : InputBinding
    {
        public Buttons button;
        public KeyBindingType bindingType;

        public GamepadButtonBinding(Buttons button, KeyBindingType bindingType)
        {
            this.button = button;
            this.bindingType = bindingType;
        }

        public override bool Check(Input input)
        {
            return false;
        }
    }

    public class MouseButtonBinding : InputBinding
    {
        public String button;
        public KeyBindingType bindingType;
        private System.Reflection.PropertyInfo buttonProperty;

        public MouseButtonBinding(String button, KeyBindingType bindingType)
        {
            this.button = button;
            this.bindingType = bindingType;

            buttonProperty = typeof(MouseState).GetProperty(button);
            if (buttonProperty == null || buttonProperty.PropertyType != typeof(ButtonState))
                throw new InvalidProgramException("Could not find button " + button);
        }

        private bool CheckButton(ref MouseState state)
        {
            return (buttonProperty.GetValue(state, null) as ButtonState?).Value == ButtonState.Pressed;
        }

        public override bool Check(Input input)
        {
            if (bindingType == KeyBindingType.Pressed)
                return CheckButton(ref input.currentMouseState) && !CheckButton(ref input.previousMouseState);
            else
                return CheckButton(ref input.currentMouseState);
                
        }
    }
        
    public class AxisBinding
    {
        public virtual Vector2 QueryAxis(Input input) { return Vector2.Zero; }
        public virtual bool ContainedBy(Input input, Rectangle rect) { return false; }
    }

    public class MouseAxisBinding : AxisBinding
    {
        public override Vector2 QueryAxis(Input input)
        {
            return new Vector2(input.currentMouseState.X, input.currentMouseState.Y);
        }

        public override bool ContainedBy(Input input, Rectangle rect)
        {
            return rect.Contains(input.currentMouseState.X, input.currentMouseState.Y);
        }
    }

    public class Input
    {
        internal KeyboardState previousState;
        internal KeyboardState currentState;
        internal MouseState previousMouseState;
        internal MouseState currentMouseState;
        internal GamePadState previousPadState;
        internal GamePadState currentPadState;

        private Common.MultiDictionary<String, InputBinding> bindings = new Common.MultiDictionary<string, InputBinding>();
        private Dictionary<String, AxisBinding> axisBindings = new Dictionary<string, AxisBinding>();

        internal XnaTextInput.TextInputHandler textHook;

		public void UnhookKeyboard()
		{
			textHook.Unhook();
		}

		public void RehookKeyboard()
		{
			textHook.Rehook();
		}

        public Input(IntPtr WindowHandle)
        {
            textHook = new XnaTextInput.TextInputHandler(WindowHandle);
        }

        public void AddBinding(String actionName, InputBinding binding) { bindings.Add(actionName, binding); }
        public void AddAxis(String axisName, AxisBinding binding) { axisBindings.Add(axisName, binding); }

        public bool Check(String actionName)
        {
            if (bindings.ContainsKey(actionName))
                foreach (var binding in bindings[actionName])
                    if (binding.Check(this)) return true;
            return false;
        }

        public Vector2 QueryAxis(String axisName)
        {
            if (axisBindings.ContainsKey(axisName))
                return axisBindings[axisName].QueryAxis(this);
            return Vector2.Zero;
        }

        public AxisBinding GetAxis(String axisName)
        {
            if (axisBindings.ContainsKey(axisName))
                return axisBindings[axisName];
            return null;
        }

        public void Update(float ElapsedSeconds)
        {
            previousState = currentState;
            currentState = Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

        }



        public void ClearBindings()
        {
            bindings = new Common.MultiDictionary<string, InputBinding>();
            axisBindings.Clear();
        }
    }
}
