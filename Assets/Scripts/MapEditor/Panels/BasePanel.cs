using UnityEngine;
using UnityEngine.UIElements;

namespace CardGame.MapEditor.Panels
{
    public abstract class BasePanel
    {
        protected string panel_name = "default";
        protected VisualElement panel = null;
        protected bool _open = false;

        public event System.Action<bool> enter_leave_event;

        protected BasePanel(VisualElement root, string name)
        {
            _open = false;
            panel_name = name;
            panel = root?.Q<VisualElement>(panel_name);

            if (panel == null)
                Debug.LogWarning($"BasePanel: Missing panel {name} !");

            panel.RegisterCallback<MouseEnterEvent>(mouse_enter_callback);
            panel.RegisterCallback<MouseLeaveEvent>(mouse_leave_callback);
        }

        void mouse_enter_callback(MouseEnterEvent e) {
            enter_leave_event?.Invoke(true);
        }

        void mouse_leave_callback(MouseLeaveEvent e) {
            enter_leave_event?.Invoke(false);
        }

        public virtual void register_enter_leave_callback(System.Action<bool> callback) {
            enter_leave_event += callback;
        }

        public virtual void open() 
        {
            panel.style.display = DisplayStyle.Flex;
            _open = true;
        }

        public virtual void close() 
        {
            panel.style.display = DisplayStyle.None;
            _open = false;
        }

        public bool is_open() {
            return _open;
        }
    }
}
