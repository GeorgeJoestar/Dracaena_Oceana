using Verse;

namespace RimWorldProj.TestLogic
{
    public class ModController : Mod
    {
        public ModController(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(ShowHelloWorldWindow, "Initializing", false, null);
        }

        private void ShowHelloWorldWindow()
        {
            Find.WindowStack.Add(new TestWindow.HelloWorldWindow());
        }
    }
}