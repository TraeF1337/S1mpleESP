using Ennui.Api;
using Ennui.Api.Script;

namespace S1mpleESP
{ 
    public class ResolveState : StateScript
    {
        private Configuration config;
        private Context context;

        public ResolveState(Configuration config, Context context)
        {
            this.config = config;
            this.context = context;
        }

        public override int OnLoop(IScriptEngine se)
        {
            Time.SleepUntil(() => !Game.InLoadingScreen, 30000);
            if (Game.InLoadingScreen)
            {
                Logging.Log("In loading screen too long, exiting script...", LogLevel.Error);
                se.StopScript();
                return 0;
            }

            context.State = "Resolving...";

            var localPlayer = Players.LocalPlayer;
            if (localPlayer != null)
            {
               parent.EnterState("work");
            }

            return 10_000;
        }
    }
}
