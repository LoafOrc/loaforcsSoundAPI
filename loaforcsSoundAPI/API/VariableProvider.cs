using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.API;
public abstract class VariableProvider {
    public abstract object Evaluate(SoundReplaceGroup group, JObject varDef);
}
