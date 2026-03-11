namespace _Scripts.Lobby.UI.Pop
{
    public class TutorialPop : PopBase
    {
        protected override void PopUp()
        {
            base.PopUp();
            
            Bg.OnClick += PopDown;
        }

        protected override void PopDown()
        {
            Bg.OnClick -= PopDown;
            
            base.PopDown();
        }
    }
}