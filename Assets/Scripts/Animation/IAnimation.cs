namespace CardGame.Animation
{
    public interface IAnimation
    {
        public void on_finished();
        public bool frame_update();
    }
}
