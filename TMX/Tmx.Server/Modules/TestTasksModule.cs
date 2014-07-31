﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 7/21/2014
 * Time: 10:16 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Tmx.Server.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
	using Nancy;
	using Nancy.ModelBinding;
	using TMX.Interfaces.Server;
	using Tmx.Interfaces.Remoting;
	using Tmx.Interfaces.Types.Remoting;
    
    /// <summary>
    /// Description of TestTasksModule.
    /// </summary>
    public class TestTasksModule : NancyModule
    {
        public TestTasksModule() : base(UrnList.TestTasks_Root)
        {
            Get[UrnList.TestTasks_CurrentClient] = parameters => {
                var taskSorter = new TaskSorter();
                ITestTask actualTask = taskSorter.GetFirstLegibleTask(parameters.id);
                return null != actualTask ? Response.AsJson(actualTask).WithStatusCode(HttpStatusCode.OK) : HttpStatusCode.NotFound;
            };
            
            Put[UrnList.TestTasks_Task] = parameters => {
                var loadedTask = this.Bind<TestTask>();
                var storedTask = TaskPool.TasksForClients.First(task => task.Id == loadedTask.Id && task.ClientId == loadedTask.ClientId);
                storedTask.Completed = loadedTask.Completed;
                storedTask.Status = loadedTask.Status;
                storedTask.TaskResult = loadedTask.TaskResult;
                var taskSorter = new TaskSorter();
                ITestTask nextTask = null;
                try {
                    nextTask = taskSorter.GetNextLegibleTask(loadedTask.ClientId, loadedTask.Id);
                }
                catch (Exception eeeee) {
Console.WriteLine("put -> getNextLegibleTask " + eeeee.Message);
                }
                
                if (null == nextTask) return HttpStatusCode.OK;
                nextTask.PreviousTaskResult = storedTask.TaskResult ?? new string[] {};
                nextTask.PreviousTaskId = loadedTask.Id;
                return HttpStatusCode.OK;
            };
        }
    }
}
