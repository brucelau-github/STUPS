﻿namespace TestRunDefaultPlugins
{
    using System.Collections.Generic;
    using Tmx.Interfaces.Remoting.Actions;

    public class ScriptRunner : ITestRunAction
    {
        public Dictionary<string, string> Settings { get; set; }

        public bool Run()
        {
            throw new System.NotImplementedException();
        }
    }
}