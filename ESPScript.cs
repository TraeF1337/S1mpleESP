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
            AddState("resolve", new ResolveState(config, context));
            AddState("work", new WorkState(config, context));
            AddState("noob", new NoobState(config, context));
            EnterState("config");
            return base.OnStart(se);
        }

    }
}
