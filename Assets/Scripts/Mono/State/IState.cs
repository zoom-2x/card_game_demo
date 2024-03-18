namespace CardGame.Mono.State
{
    public interface IState
    {
        public void on_enter();
        public void on_exit();
        public void frame_update();
    }
}
