using Ennui.Api;
using Ennui.Api.Direct;
using Ennui.Api.Method;
using Ennui.Api.Script;
using System;
using System.Collections.Generic;

namespace S1mpleESP
{
    [LocalScript]
    public class ESPScript : StateScript
    {
        private Configuration config;
        private Context context;

        private void LoadConfig()
        {
            try
            {
                if (Files.Exists("s1mpleESP.json"))
                {
                    config = Codecs.FromJson<Configuration>(Files.ReadAllText("s1mpleESP.json"));
                    if (config.TypeSetsToUse == null)
                    {
                        config.TypeSetsToUse = new List<SafeTypeSet>();
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Log("Failed to log config " + e, LogLevel.Error);
            }

            if (config == null)
            {
                config = new Configuration();
            }
        }

        public override bool OnStart(IScriptEngine se)
        {
            LoadConfig();

            context = new Context();

            Logging.Log("Load ESP Script", LogLevel.Info);

            AddState("config", new ConfigState(config, context));
            AddState("work", new ConfigState(config, context));
            EnterState("config");

            return base.OnStart(se);
        }

        public override void OnPaint(IScriptEngine se, GraphicContext g)
        {
            g.SetColor(new Color(0.3f, 0.3f, 0.3f, 1.0f));
            g.FillRect(15, 100, 265, 195);
            g.SetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));
            g.DrawString("S1mpleESP", 20, 100);
            g.DrawString(string.Format("State: {0}", context.State), 20, 130);
        }
    }
}
