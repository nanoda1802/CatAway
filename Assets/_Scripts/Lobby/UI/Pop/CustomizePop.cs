namespace _Scripts.Lobby.UI.Pop
{
    public class CustomizePop : PopBase
    {
        protected override void PopUp()
        {
            base.PopUp();
            
            Bg.OnClick += PopDown;
            Bg.OnSwipeDown += PopDown;
        }

        protected override void PopDown()
        {
            Bg.OnClick -= PopDown;
            Bg.OnSwipeDown -= PopDown;
            
            base.PopDown();
        }
    }
}