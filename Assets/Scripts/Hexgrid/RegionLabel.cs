using UnityEngine;

namespace CardGame.Hexgrid
{
    public interface RegionLabel
    {
        // public void set_map(Transform map);
        // public void set_camera(Camera camera);
        public void set_object_position(Vector3 position);
        public void set_name(string s);
        public void set_extra_info(string s = null);
        public void set_value(int resource, uint v);
        public void update_ui_position();
        public void hide();
        public void show();
        public void destroy();
    }
}
