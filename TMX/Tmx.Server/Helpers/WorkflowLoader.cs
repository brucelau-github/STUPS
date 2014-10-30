﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 7/24/2014
 * Time: 5:42 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Tmx.Server
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;
	using Tmx.Interfaces.Exceptions;
	using Tmx.Core.Types.Remoting;
	using Tmx.Interfaces.Remoting;
	
	/// <summary>
	/// Description of WorkflowLoader.
	/// </summary>
	public class WorkflowLoader
	{
		const string taskElement_id = "id";
		const string taskElement_afterTask = "afterTask";
		const string taskElement_isActive = "isActive";
		const string taskElement_isCritical = "isCritical";
		const string taskElement_name = "name";
		const string taskElement_rule = "rule";
		const string taskElement_storyId = "storyId";
		const string taskElement_taskType = "taskType";
		const string taskElement_timeLimit = "timelimit";
		const string taskElement_retryCount = "retryCount";
		
		const string taskElement_action = "action";
		const string taskElement_beforeAction = "beforeAction";
		const string taskElement_afterAction = "afterAction";
		const string taskElement_code = "code";
		const string taskElement_parameters = "parameters";
		
		public virtual bool LoadWorkflow(string pathToWorkflowFile)
		{
            try {
				if (!System.IO.File.Exists(pathToWorkflowFile))
				    throw new WorkflowLoadingException("There is no such file '" + pathToWorkflowFile + "'.");
                ImportXdocument(XDocument.Load(pathToWorkflowFile));
            }
            catch (Exception eImportDocument) {
                throw new WorkflowLoadingException(
                    "Unable to load an XML workflow from the file '" +
                    pathToWorkflowFile +
                    "'. " + 
                    eImportDocument.Message);
            }
			
			return true;
		}

		// public virtual int ImportXdocument(XContainer xDocument)
		public virtual Guid ImportXdocument(XContainer xDocument)
		{
            var workflowId = getWorkflowId(xDocument);
            var tasks = from task in xDocument.Descendants("task")
                        where task.Element(taskElement_isActive).Value == "1"
                        select task;
            var importedTasks = tasks.Select(tsk => getNewTestTask(tsk, workflowId));
            addTasksToCommonPool(importedTasks);
            return workflowId;
		}
		
        // int getWorkflowId(XContainer xDocument)
        Guid getWorkflowId(XContainer xDocument)
        {
            var wfl = xDocument.Descendants("workflow").FirstOrDefault();
            if (null == wfl)
                throw new WorkflowLoadingException("There's no workflow element in the document");
            var nameAttribute = wfl.Attribute("name");
            var workflowName = null != nameAttribute ? nameAttribute.Value : "unnamed workflow";
            
            return addWorkflow(workflowName);
        }
        
        // int addWorkflow(string name)
        Guid addWorkflow(string name)
        {
            var workflow = new TestWorkflow(TestLabCollection.TestLabs.First()) { Name = name };
			// workflow.Id = generateWorkflowId();
			WorkflowCollection.AddWorkflow(workflow);
			return workflow.Id;
        }
        
//        int generateWorkflowId()
//        {
//			int maxId = 0;
//			if (0 < WorkflowCollection.Workflows.Count)
//				maxId = WorkflowCollection.Workflows.Max(wkfl => wkfl.Id);
//			return ++maxId;
//        }
        
		internal virtual void addTasksToCommonPool(IEnumerable<ITestTask> importedTasks)
		{
			TaskPool.Tasks.AddRange(importedTasks);
		}
		
		// internal virtual ITestTask getNewTestTask(XContainer taskNode, int workflowId)
		internal virtual ITestTask getNewTestTask(XContainer taskNode, Guid workflowId)
		{
			return new TestTask {
		        Action = getActionCode(taskNode, taskElement_action),
		        ActionParameters = getActionParameters(taskNode, taskElement_action),
				AfterAction = getActionCode(taskNode, taskElement_afterAction),
				AfterActionParameters = getActionParameters(taskNode, taskElement_afterAction),
				BeforeAction = getActionCode(taskNode, taskElement_beforeAction),
				BeforeActionParameters = getActionParameters(taskNode, taskElement_beforeAction),
				TaskFinished = false,
				// ExpectedResult
				Id = convertTestTaskElementValue(taskNode, taskElement_id),
				AfterTask = convertTestTaskElementValue(taskNode, taskElement_afterTask),
				IsActive = "1" == getTestTaskElementValue(taskNode, taskElement_isActive),
				IsCritical = "1" == getTestTaskElementValue(taskNode, taskElement_isCritical),
				Name = getTestTaskElementValue(taskNode, taskElement_name),
				// PreviousTaskId
				RetryCount = convertTestTaskElementValue(taskNode, taskElement_retryCount),
				Rule = getTestTaskElementValue(taskNode, taskElement_rule),
				TaskStatus = TestTaskStatuses.New,
				StoryId = getTestTaskElementValue(taskNode, taskElement_storyId),
				// TaskResult
				TaskType = getTestTaskType(taskNode.Element(taskElement_taskType).Value),
				TimeLimit = convertTestTaskElementValue(taskNode, taskElement_timeLimit),
				WorkflowId = workflowId
			};
		}
		
		internal virtual string getActionCode(XContainer taskNode, string elementName)
		{
		    var actionNode = taskNode.Element(elementName);
		    return getTestTaskElementValue(actionNode, taskElement_code);
		}
		
		internal virtual IDictionary<string, string> getActionParameters(XContainer taskNode, string elementName)
		{
		    var resultDict = new Dictionary<string, string>();
		    var nodeParameters = taskNode.Element(elementName);
		    try {
                nodeParameters = nodeParameters.Element(taskElement_parameters);
                if (null == nodeParameters) return resultDict;
                foreach (var parameterNode in nodeParameters.Elements())
                    resultDict.Add(parameterNode.Name.LocalName, parameterNode.Value.ToString());
		    }
		    catch {}
		    return resultDict;
		}
		
		internal virtual int convertTestTaskElementValue(XContainer taskNode, string elementName)
		{
			return Convert.ToInt32(string.Empty == getTestTaskElementValue(taskNode, elementName) ? "0" : getTestTaskElementValue(taskNode, elementName));
		}
		
		internal virtual string getTestTaskElementValue(XContainer taskNode, string elementName)
		{
			try {
				return taskNode.Element(elementName).Value ?? string.Empty;
			}
			catch {
				// throw new WrongTaskDataException("failed to read the value of element '" + elementName + "'");
				return string.Empty;
			}
		}
		
		internal virtual TestTaskExecutionTypes getTestTaskType(string taskTypeStringValue)
		{
			switch (taskTypeStringValue.ToUpper()) {
				case "RDP":
					return TestTaskExecutionTypes.RemoteApp;
				case "PSREMOTING":
					return TestTaskExecutionTypes.PsRemoting;
				case "INLINE":
				case "":
					return TestTaskExecutionTypes.Inline;
				case "SSH":
					return TestTaskExecutionTypes.Ssh;
				default:
					throw new WrongTaskDataException("Failed to read the taskType element");
			}
		}
	}
}
