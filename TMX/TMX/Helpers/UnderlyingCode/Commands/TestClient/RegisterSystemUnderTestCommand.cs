﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 7/17/2014
 * Time: 7:07 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Tmx
{
	using System;
	using Tmx;
	using Tmx.Client;
	using Tmx.Commands;
	
	/// <summary>
	/// Description of RegisterSystemUnderTestCommand.
	/// </summary>
    class RegisterSystemUnderTestCommand : TmxCommand
    {
        internal RegisterSystemUnderTestCommand(CommonCmdletBase cmdlet) : base (cmdlet)
        {
        }
        
        internal override void Execute()
        {
            var cmdlet = (RegisterTmxSystemUnderTestCommand)Cmdlet;
            ClientSettings.ServerUrl = cmdlet.ServerUrl;
            ClientSettings.StopImmediately = false;
            var registration = new Registration();
            // temporarily
            // TODO: to a template method
            var startTime = DateTime.Now;
            while (true) {
                // TODO: move to aspect
                try {
                    ClientSettings.ClientId = registration.SendRegistrationInfoAndGetClientId(cmdlet.CustomClientString);
                }
                catch (Exception e2) {
Console.WriteLine("registering " + e2.Message);
                }
                
                System.Threading.Thread.Sleep(Preferences.ClientRegistrationSleepIntervalMilliseconds);
                
                if (0 != ClientSettings.ClientId)
                    break;
                
                if ((DateTime.Now - startTime).TotalSeconds >= cmdlet.Seconds)
                    throw new Exception("Failed to register client in " + cmdlet.Seconds + " seconds");
            }
        }
    }
}
