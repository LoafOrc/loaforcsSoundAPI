using loaforcsSoundAPI.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace loaforcsSoundAPI.API;
public abstract class Conditonal {

    internal JObject ConditionSettings { get; private set; }

    ConditionProvider GroupCondition = null;

    SoundReplaceGroup group;

    protected void Setup(SoundReplaceGroup group, JObject settings) {
        this.group = group;
        if (settings == null) return;
        ConditionSettings = settings;
        GroupCondition = SoundAPI.ConditionProviders[(string)ConditionSettings["type"]];
    }

    public virtual bool TestCondition() {
        if (GroupCondition == null) return true;

        return GroupCondition.Evaluate(group, ConditionSettings);
    }
}
