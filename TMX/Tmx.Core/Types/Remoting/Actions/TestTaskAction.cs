﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 12/7/2014
 * Time: 6:29 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Tmx.Core.Types.Remoting.Actions
{
    using System;
    using Interfaces.Remoting.Actions;
    
    /// <summary>
    /// Description of TestTaskAction.
    /// </summary>
    public class TestTaskAction : ITestTaskAction
    {
        public bool Run()
        {
            return true;
        }
    }
}
