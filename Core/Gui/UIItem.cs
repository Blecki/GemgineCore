using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace Gem.Gui
{
    public class UIItem
    {
        public Rectangle rect;
        public List<UIItem> children = new List<UIItem>();
        public UIItem parent;
        public int id;
        public bool Visible = true;
        public PropertySet defaults;
        public PropertySet settings;
        public PropertySet hoverSettings;
        public bool Hover { get; set; }
        
        public UIItem root { 
            get { if (parent == null) return this;
                return parent.root; }}

        public UIItem(Rectangle rect, PropertySet settings = null, PropertySet hoverSettings = null)
        {
            this.settings = settings;
            if (this.settings == null) this.settings = new PropertySet();
            this.hoverSettings = hoverSettings;

            this.rect = rect;
            Hover = false;
        }

        public virtual void HandleMouse(bool mouseValid, int x, int y, bool mousePressed, Simulation sim)
        {
            Hover = mouseValid && rect.Contains(x, y);
            if (Hover && mousePressed)
            {
                var handler = GetSetting("on-click", null);
                if (handler != null)
                    sim.EnqueueEvent("@raw-input-event", new ObjectList(handler, this));
            }
            if (Visible)
                foreach (var child in children) child.HandleMouse(mouseValid, x, y, mousePressed, sim);
           
        }

        public virtual void HandleMouseEx(bool mouseValid, int x, int y, bool mousePressed, Action<ObjectList> onEvent)
        {
            Hover = mouseValid && rect.Contains(x, y);
            if (Hover && mousePressed)
            {
                var handler = GetSetting("on-click", null);
                if (handler != null)
                    onEvent(new ObjectList(handler, this));
            }
            if (Visible)
                foreach (var child in children) child.HandleMouseEx(mouseValid, x, y, mousePressed, onEvent);

        }

        public virtual void KeyPressEvent(System.Windows.Forms.KeyPressEventArgs args) { }
        public virtual void KeyDownEvent(System.Windows.Forms.KeyEventArgs args) { }
        public virtual void KeyUpEvent(System.Windows.Forms.KeyEventArgs args) { }
		
		public virtual void AddChild(UIItem child)
		{
			children.Add(child);
            child.defaults = defaults;
			child.parent = this;
		}
		
		public virtual void RemoveChild(UIItem child)	
        {
            children.Remove(child);
        }
		
		public void Destroy()
		{
			if (parent != null)
				parent.RemoveChild(this);
			parent = null;
		}

        protected Object GetSetting(String name, Object _default)
        {
            if (Hover && hoverSettings != null && hoverSettings.ContainsKey(name) && hoverSettings[name] != null) return hoverSettings[name];
            if (settings != null && settings.ContainsKey(name) && settings[name] != null) return settings[name];
            if (defaults != null && defaults.ContainsKey(name) && defaults[name] != null) return defaults[name];
            return _default;
        }

        protected int GetIntegerSetting(String name, int _default)
        {
            var setting = GetSetting(name, null);
            if (setting == null) return _default;
            try
            {
                return Convert.ToInt32(setting);
            }
            catch (Exception) { return _default; }
        }
		
		public virtual void Render(Render.ImmediateMode2d context) 
		{
            if (Visible)
            {
                if (GetSetting("hidden-container", null) == null)
                {
                    if (GetSetting("transparent", null) == null)
                    {
                        context.Texture = context.White;
                        context.Color = (GetSetting("bg-color", Vector3.One) as Vector3?).Value;
                        context.Quad(rect);
                    }

                    var label = GetSetting("label", null);
                    var font = GetSetting("font", null);
                    if (label != null && font != null)
                    {
                        context.Color = (GetSetting("text-color", Vector3.Zero) as Vector3?).Value;
                        BitmapFont.RenderText(label.ToString(), rect.X, rect.Y, rect.Width + rect.X,
                            context, font as BitmapFont);
                    }
                }

                foreach (var child in children)
                    child.Render(context);
            }
        }

	}

}