﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 7/18/2012
 * Time: 8:12 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace SePSX
{
    using System;
    using System.Management.Automation;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.IE;
    using PSTestLib;
    
    using System.IO;
    
    using System.Collections;
    using System.Collections.ObjectModel;
    using UIAutomation;
    using Tmx.Interfaces.TestStructure;
    
    /// <summary>
    /// Description of CommonCmdletBase.
    /// </summary>
    public class CommonCmdletBase : PSCmdletBase//, ICommonCmdletBase
    {
        public CommonCmdletBase()
        {
            // ??
            if (!UnitTestMode && !ModuleAlreadyLoaded) {

                WebDriverFactory.AutofacModule = new WebDriverModule();

                WebDriverFactory.Init();

                ModuleAlreadyLoaded = true;
            }
            
            CurrentData.Init();
        }
        
//        internal static new bool UnitTestMode { get; set; }
        internal static bool ModuleAlreadyLoaded { get; set; }
//        internal static System.Collections.Generic.List<object> UnitTestOutput { get; set; }
        
        private const string ExceptionMessageNull = 
            "The pipeline input is null";
        private const string ExceptionMessageWrongTypeWebDriver = 
            "The pipeline input is not of IWebDriver type";
        private const string ExceptionMessageWrongTypeWebElement = 
            "The pipeline input is not of IWebElement type";
        private const string ExceptionMessageWrongTypeWebDriverOrWebElement = 
            "The pipeline input is null or not of IWebDriver or IWebElement type";
        
        // 20130430
        //protected override void WriteLog(string logRecord)
        protected override void WriteLog(LogLevels logLevel, string logRecord)
        {
            if (Preferences.AutoLog) {
                
                // 20140317
                // turning off the logger
//                switch (logLevel) {
//                    case LogLevels.Fatal:
//                        Tmx.Logger.Fatal(logRecord);
//                        break;
//                    case LogLevels.Error:
//                        Tmx.Logger.Error(logRecord);
//                        break;
//                    case LogLevels.Warn:
//                        Tmx.Logger.Warn(logRecord);
//                        break;
//                    case LogLevels.Info:
//                        Tmx.Logger.Info(logRecord);
//                        break;
//                    case LogLevels.Debug:
//                        Tmx.Logger.Debug(logRecord);
//                        break;
//                    case LogLevels.Trace:
//                        Tmx.Logger.Trace(logRecord);
//                        break;
//                }
            }
            
            try {
                //Global.WriteToLogFile(record);
                //WriteToLogFile(record);
                WriteToLogFile(logRecord);
            } catch (Exception e) {
                WriteVerbose(this, "Unable to write to the log file: " +
                             Preferences.LogPath);
                WriteVerbose(this, e.Message);
            }
        }
        
        protected void WriteLog(LogLevels logLevel, ErrorRecord errorRecord)
        {
            if (Preferences.AutoLog) {
                
                WriteLog(logLevel, errorRecord.Exception.Message);
                WriteLog(logLevel, "Script: '" + errorRecord.InvocationInfo.ScriptName + "', line: " + errorRecord.InvocationInfo.Line.ToString());
            }
        }
        
        protected void CheckInputWebDriver(bool strict)
        {
            if (null == ((HasWebDriverInputCmdletBase)this).InputObject) {
                WriteError(
                    this,
                    ExceptionMessageNull,
                    "WrongInput",
                    ErrorCategory.InvalidArgument,
                    true);
            } else {
                if (strict) {
                    if (!(((HasWebDriverInputCmdletBase)this).InputObject is IWebDriver[])) {
                        WriteError(
                            this,
                            ExceptionMessageWrongTypeWebDriver,
                            "WrongInput",
                            ErrorCategory.InvalidArgument,
                            true);
                    }
                }
                WriteVerbose(this, "The pipeline input is good");
            }
        }
        
        protected void CheckInputWebDriverOrWebElement()
        {
            IWebDriver[] driver = null;
            var driverList = 
                new System.Collections.Generic.List<IWebDriver>();
            IWebElement[] element = null;
            var elementList = 
                new System.Collections.Generic.List<IWebElement>();
            //foreach(object inputObject in ((HasWebElementInputCmdletBase)this).InputObject) {
                try {
                    WriteVerbose(this, "Checking whether the input is of WebDriver type");
                    var driverTest = 
                        ((HasWebElementInputCmdletBase)this).InputObject as IWebDriver[];
                    if (null != driverTest) {
                        WriteVerbose(this, "input is IWebDriver");
                        driver = (IWebDriver[])driverTest;
                    } else {
                        WriteVerbose(this, "input is PSObject");
//                        driver = 
//                            ((PSObject[])((HasWebElementInputCmdletBase)this).InputObject).BaseObject as IWebDriver[];
                        for (var i = 0; i < ((HasWebElementInputCmdletBase)this).InputObject.Length; i++) {
                            //driver[i] = 
                            var rawInputItemDriver = 
                                ((HasWebElementInputCmdletBase)this).InputObject[i];
                            if (rawInputItemDriver is IWebDriver) {
                                driverList.Add((rawInputItemDriver as IWebDriver));
                            } else {
                                driverList.Add((((PSObject)rawInputItemDriver).BaseObject as IWebDriver));
                            }
//                            driverList.Add(
//                                ((PSObject)((HasWebElementInputCmdletBase)this).InputObject[i]).BaseObject as IWebDriver
//                               );
                        }
                    }
                    //if (driver == null) {
                    if (driverList.Count == 0) {
                        throw (new Exception("The input object is not of IWebDriver type"));
                    }
                    //driver =
                    //    //((PSObject)((HasWebElementInputCmdletBase)this).InputObject).BaseObject as IWebDriver;
                    //    ((HasWebElementInputCmdletBase)this).InputObject as IWebDriver;
                    WriteVerbose(this, "The pipeline input is of WebDriver type");
                    //if (driver != null) {
                    if (driverList.Count > 0) {
                        WriteVerbose(this, "set InputObject");
                        //((HasWebElementInputCmdletBase)this).InputObject[0] = driver;
                        for (var i = 0; i < driverList.Count; i++) {
                            ((HasWebElementInputCmdletBase)this).InputObject[i] =
                                driverList[i];
                        }
                    }
                    //((HasWebElementInputCmdletBase)this).InputObject =
                    //    ((HasWebElementInputCmdletBase)this).InputObject as IWebDriver;
                 }
                catch (Exception eNotWebDriver) {
                    WriteVerbose(this, "The pipeline input is not of WebDriver type");
                    WriteVerbose(this, eNotWebDriver.Message);
                    try {
                        WriteVerbose(this, "Checking whether the input is of WebElement type");
                        var elementTest = 
                            ((HasWebElementInputCmdletBase)this).InputObject as IWebElement[];
                        if (elementTest != null) {
                            WriteVerbose(this, "input is IWebElement");
                            element = elementTest;
                        } else {
                            WriteVerbose(this, "input is PSObject");
//                            element =
//                                ((PSObject[])((HasWebElementInputCmdletBase)this).InputObject).BaseObject as IWebElement[];
                            for (var i = 0; i < ((HasWebElementInputCmdletBase)this).InputObject.Length; i++) {
                                //element[i] = 
//                                elementList.Add(
//                                    ((PSObject)((HasWebElementInputCmdletBase)this).InputObject[i]).BaseObject as IWebElement);
                                var rawInputItemElement = 
                                    ((HasWebElementInputCmdletBase)this).InputObject[i];
                                if (rawInputItemElement is IWebElement) {
                                    elementList.Add((rawInputItemElement as IWebElement));
                                } else {
                                    elementList.Add((((PSObject)rawInputItemElement).BaseObject as IWebElement));
                                }
                            }
                        }
                        //if (element == null) {
                        if (0 == elementList.Count) {
                            throw (new Exception("The input object is not of IWebElement type"));
                        }
                        //element = 
                        //    //((PSObject)((HasWebElementInputCmdletBase)this).InputObject).BaseObject as IWebElement;
                        //    ((HasWebElementInputCmdletBase)this).InputObject as IWebElement;
                        WriteVerbose(this, "The pipeline input is of WebElement type");
                        //if (element != null) {
                        if (elementList.Count > 0) {
                            WriteVerbose(this, "set InputObject");
                            //((HasWebElementInputCmdletBase)this).InputObject = element;
                            for (var i = 0; i < elementList.Count; i++) {
                                ((HasWebElementInputCmdletBase)this).InputObject[i] =
                                    elementList[i];
                            }
                        }
                        //((HasWebElementInputCmdletBase)this).InputObject = 
                        //    ((HasWebElementInputCmdletBase)this).InputObject as IWebElement;
                    }
                    catch (Exception eNotWebElement) {
                        WriteVerbose(this, "The pipeline input is not of WebElement type");
                        WriteVerbose(this, eNotWebElement.Message);
                        WriteError(
                            this,
                            ExceptionMessageWrongTypeWebDriverOrWebElement,
                            "WrongInput",
                            ErrorCategory.InvalidArgument,
                            true);
                    }
                }
            //}
        }
        
        protected void checkInputWebElementOnly(IWebElement[] input)
        {
            try {
                if (!(input is IWebElement[])) {
                    throw (new Exception("The pipeline input is not of WebElement type"));
                }
            }
            catch (Exception eNotWebElement) {
                WriteVerbose(this, "The pipeline input is not of WebElement type");
                WriteVerbose(this, eNotWebElement.Message);
                WriteError(
                    this,
                    ExceptionMessageWrongTypeWebDriverOrWebElement,
                    "WrongInput",
                    ErrorCategory.InvalidArgument,
                    true);
            }
        }
        
        protected void checkInputWebElementOnly(object[] input)
            //HasWebElementInputCmdletBase cmdlet)
        {
            //IWebDriver driver = null;
            IWebElement[] element = null; // = new IWebElement[]; //null;
            var elementList = 
                new System.Collections.Generic.List<IWebElement>();
            
                try {
                    WriteVerbose(this, "Checking whether the input is of WebElement type");
                    var elementTest = 
                        //((HasWebElementInputCmdletBase)this).InputObject as IWebElement;
                        //input as IWebElement[];
                        ((HasWebElementInputCmdletBase)this).InputObject as IWebElement[];
                    if (null != elementTest) {
                        WriteVerbose(this, "input is IWebElement");
                        element = elementTest;
                    } else {
                        WriteVerbose(this, "input is PSObject");
                        
                        for (var i = 0; i < ((HasWebElementInputCmdletBase)this).InputObject.Length; i++) {
                            //element[i] = 
//                                elementList.Add(
//                                    ((PSObject)((HasWebElementInputCmdletBase)this).InputObject[i]).BaseObject as IWebElement);
                            var rawInputItemElement = 
                                ((HasWebElementInputCmdletBase)this).InputObject[i];
                            if (rawInputItemElement is IWebElement) {
                                elementList.Add((rawInputItemElement as IWebElement));
                            } else {
                                elementList.Add((((PSObject)rawInputItemElement).BaseObject as IWebElement));
                            }
                        }
                        
                    }
                    //if (element == null) {
                    if (0 == elementList.Count) {
                        throw (new Exception("The input object is not of IWebElement type"));
                    }
                    //element = 
                    //    //((PSObject)((HasWebElementInputCmdletBase)this).InputObject).BaseObject as IWebElement;
                    //    ((HasWebElementInputCmdletBase)this).InputObject as IWebElement;
                    WriteVerbose(this, "The pipeline input is of WebElement type");
                    //if (element != null) {
                    if (elementList.Count > 0) {
                        WriteVerbose(this, "set InputObject");
                        //((HasWebElementInputCmdletBase)this).InputObject = element;
                        //cmdlet.InputObject = element;
                        for (var i = 0; i < elementList.Count; i++) {
                            ((HasWebElementInputCmdletBase)this).InputObject[i] =
                                (IWebElement)elementList[i];
                        }
                    }
                    //((HasWebElementInputCmdletBase)this).InputObject = 
                    //    ((HasWebElementInputCmdletBase)this).InputObject as IWebElement;
                }
                catch (Exception eNotWebElement) {
                    WriteVerbose(this, "The pipeline input is not of WebElement type");
                    WriteVerbose(this, eNotWebElement.Message);
                    WriteError(
                        this,
                        ExceptionMessageWrongTypeWebDriverOrWebElement,
                        "WrongInput",
                        ErrorCategory.InvalidArgument,
                        true);
                }
//            }
        }

        
        protected void CheckInputAlert(bool strict)
        {
            if (null == ((AlertCmdletBase)this).InputObject) {

                WriteError(
                    this,
                    "The alert is null.",
                    "WrongInput",
                    ErrorCategory.InvalidArgument,
                    true);
            } else {
                if (strict) {
                    if (!(((AlertCmdletBase)this).InputObject is IAlert)) {

                        WriteError(
                            this,
                            "The alert is null.",
                            "WrongInput",
                            ErrorCategory.InvalidArgument,
                            true);
                    }
                }
                WriteVerbose(this, "The pipeline input is good");
            }
        }
        
        protected void CheckInputFirefoxProfile(bool strict)
        {
            if (null == ((EditFirefoxProfileCmdletBase)this).InputObject) {

                WriteError(
                    this,
                    "The input Firefox profile object is null.",
                    "WrongInput",
                    ErrorCategory.InvalidArgument,
                    true);
            } else {
                if (strict) {
                    if (!(((EditFirefoxProfileCmdletBase)this).InputObject is FirefoxProfile)) {

                        WriteError(
                            this,
                            "The input Firefox profile object is null.",
                            "WrongInput",
                            ErrorCategory.InvalidArgument,
                            true);
                    }
                }
                WriteVerbose(this, "The pipeline input is good");
            }
        }
        
        protected void CheckInputChromeOptions(bool strict)
        {
            if (null == ((EditChromeOptionsCmdletBase)this).InputObject) {

                WriteError(
                    this,
                    "The input Chrome options object is null.",
                    "WrongInput",
                    ErrorCategory.InvalidArgument,
                    true);
            } else {
                if (strict) {
                    if (!(((EditChromeOptionsCmdletBase)this).InputObject is ChromeOptions)) {

                        WriteError(
                            this,
                            "The input Chrome options object is null.",
                            "WrongInput",
                            ErrorCategory.InvalidArgument,
                            true);
                    }
                }
                WriteVerbose(this, "The pipeline input is good");
            }
        }
        
        protected void CheckInputInternetExplorerOptions(bool strict)
        {
            if (null == ((EditIeOptionsCmdletBase)this).InputObject) {

                WriteError(
                    this,
                    "The input Internet Explorer options object is null.",
                    "WrongInput",
                    ErrorCategory.InvalidArgument,
                    true);
            } else {
                if (strict) {
                    if (!(((EditIeOptionsCmdletBase)this).InputObject is InternetExplorerOptions)) {

                        WriteError(
                            this,
                            "The input Internet Explorer options object is null.",
                            "WrongInput",
                            ErrorCategory.InvalidArgument,
                            true);
                    }
                }
                WriteVerbose(this, "The pipeline input is good");
            }
        }
        
        public virtual void WriteObject(PSCmdletBase cmdlet, ReadOnlyCollection<IWebElement> outputObjectCollection)
        {
            for (var i = 0; i < outputObjectCollection.Count; i++) {
                WriteObject(cmdlet, outputObjectCollection[i]);
            }
        }
        
        protected override bool CheckSingleObject(PSCmdletBase cmdlet, object outputObject) { return WriteObjectMethod010CheckOutputObject(cmdlet, outputObject); }
        protected override void BeforeWriteCollection(PSCmdletBase cmdlet, object[] outputObjectCollection) {}
        protected override void BeforeWriteCollection(PSCmdletBase cmdlet, System.Collections.Generic.List<object> outputObjectCollection) {}
        protected override void BeforeWriteCollection(PSCmdletBase cmdlet, ArrayList outputObjectCollection) {}
        protected override void BeforeWriteCollection(PSCmdletBase cmdlet, IList outputObjectCollection) {}
        protected override void BeforeWriteCollection(PSCmdletBase cmdlet, IEnumerable outputObjectCollection) {}
        protected override void BeforeWriteCollection(PSCmdletBase cmdlet, ICollection outputObjectCollection) {}
        protected override void BeforeWriteCollection(PSCmdletBase cmdlet, Hashtable outputObjectCollection) {}
        protected override void BeforeWriteSingleObject(PSCmdletBase cmdlet, object outputObject) {}

        protected override void WriteSingleObject(PSCmdletBase cmdlet, object outputObject)
        {
Console.WriteLine("WriteSingleObject 00001");
            WriteObjectMethod020Highlight(cmdlet, outputObject);
Console.WriteLine("WriteSingleObject 00002");
            WriteObjectMethod030RunScriptBlocks(cmdlet, outputObject);
Console.WriteLine("WriteSingleObject 00003");
            WriteObjectMethod040SetTestResult(cmdlet, outputObject);
Console.WriteLine("WriteSingleObject 00004");
            WriteObjectMethod045OnSuccessScreenshot(cmdlet, outputObject);
Console.WriteLine("WriteSingleObject 00005");
            WriteObjectMethod050OnSuccessDelay(cmdlet, outputObject);
Console.WriteLine("WriteSingleObject 00006");
            WriteObjectMethod060OutputResult(cmdlet, outputObject);
Console.WriteLine("WriteSingleObject 00007");
            WriteObjectMethod070Report(cmdlet, outputObject);
Console.WriteLine("WriteSingleObject 00008");
            WriteObjectMethod080ReportFailure(cmdlet, outputObject);
        }
        
        protected override void AfterWriteSingleObject(PSCmdletBase cmdlet, object outputObject) {}
        protected override void AfterWriteCollection(PSCmdletBase cmdlet, object[] outputObjectCollection) {}
        protected override void AfterWriteCollection(PSCmdletBase cmdlet, System.Collections.Generic.List<object> outputObjectCollection) {}
        protected override void AfterWriteCollection(PSCmdletBase cmdlet, ArrayList outputObjectCollection) {}
        protected override void AfterWriteCollection(PSCmdletBase cmdlet, IList outputObjectCollection) {}
        protected override void AfterWriteCollection(PSCmdletBase cmdlet, IEnumerable outputObjectCollection) {}
        protected override void AfterWriteCollection(PSCmdletBase cmdlet, ICollection outputObjectCollection) {}
        protected override void AfterWriteCollection(PSCmdletBase cmdlet, Hashtable outputObjectCollection) {}
        
        // 20130204
        //protected override bool WriteObjectMethod010CheckOutputObject(object outputObject)
        protected bool WriteObjectMethod010CheckOutputObject(PSCmdletBase cmdlet, object outputObject)
        {
            var result = false || outputObject != null;

            return result;
        }
        
        // 20130204
        //protected override void WriteObjectMethod020Highlight(PSCmdletBase cmdlet, object outputObject)
        protected void WriteObjectMethod020Highlight(PSCmdletBase cmdlet, object outputObject)
        {
            WriteVerbose(this, "IWebDriver or IWebElement");
//Console.WriteLine("WriteObjectMethod020Highlight 00001");
            if (Preferences.Highlight && outputObject is IWebElement) {
//Console.WriteLine("WriteObjectMethod020Highlight 00002");
                WriteVerbose(this, "Highlighting");
//Console.WriteLine("WriteObjectMethod020Highlight 00003");
                WriteVerbose(this, outputObject.GetType().Name);
//Console.WriteLine("WriteObjectMethod020Highlight 00004");
                WriteVerbose(this, ((IWebElement)outputObject).GetType().Name);
//Console.WriteLine("WriteObjectMethod020Highlight 00005");
                SeHelper.Highlight((IWebElement)outputObject);
//Console.WriteLine("WriteObjectMethod020Highlight 00006");
            }
        }
        
        // 20130204
        //protected override void WriteObjectMethod030RunScriptBlocks(PSCmdletBase cmdlet, object outputObject)
        protected void WriteObjectMethod030RunScriptBlocks(PSCmdletBase cmdlet, object outputObject)
        {
            WriteVerbose(this, "is going to run scriptblocks");
            if (cmdlet != null) {
                // run scriptblocks
                //if (cmdlet is HasScriptBlockCmdletBase) {
                if (cmdlet is PSCmdletBase) {
                    WriteVerbose(this, "cmdlet is of the HasScriptBlockCmdletBase type");
                    if (outputObject == null) {
                        WriteVerbose(this, "run OnError script blocks (null)");
                        //RunOnErrorScriptBlocks(((HasScriptBlockCmdletBase)cmdlet));
                        // 20130318
                        //RunOnErrorScriptBlocks(cmdlet);
                        RunOnErrorScriptBlocks(cmdlet, null);
                    } else if (outputObject is bool && ((bool)outputObject) == false) {
                        WriteVerbose(this, "run OnError script blocks (false)");
                        //RunOnErrorScriptBlocks(((HasScriptBlockCmdletBase)cmdlet));
                        // 20130318
                        //RunOnErrorScriptBlocks(cmdlet);
                        RunOnErrorScriptBlocks(cmdlet, null);
                    } else if (outputObject != null) {
                        WriteVerbose(this, "run OnSuccess script blocks");
                        //RunOnSuccessScriptBlocks(((HasScriptBlockCmdletBase)cmdlet));
                        // 20130318
                        //RunOnSuccessScriptBlocks(cmdlet);
                        RunOnSuccessScriptBlocks(cmdlet, null);
                    }
                }
            }
        }
        
        // 20130204
        //protected override void WriteObjectMethod040SetTestResult(PSCmdletBase cmdlet, object outputObject)
        protected void WriteObjectMethod040SetTestResult(PSCmdletBase cmdlet, object outputObject)
        {
            if (cmdlet != null) {
                try {
                    CurrentData.LastResult = outputObject;
                    var iInfo = string.Empty;
                    //if (((HasScriptBlockCmdletBase)cmdlet).TestResultName != null &&
                    if (cmdlet.TestResultName != null &&
                        //((HasScriptBlockCmdletBase)cmdlet).TestResultName.Length > 0) {
                        cmdlet.TestResultName.Length > 0) {

                        //Tmx.TmxHelper.CloseTestResult(((HasScriptBlockCmdletBase)cmdlet).TestResultName,
                        Tmx.TmxHelper.CloseTestResult(cmdlet.TestResultName,
                                                      //((HasScriptBlockCmdletBase)cmdlet).TestResultId,
                                                      cmdlet.TestResultId,
                                                      //((HasScriptBlockCmdletBase)cmdlet).TestPassed,
                                                      cmdlet.TestPassed,
                                                      false, // isKnownIssue
                                                      // 20160116
                                                      // this.MyInvocation,
                                                      null, // Error
                                                      string.Empty,
                                                      // 20130322
                                                      //false);
                                                      // 20130626
                                                      //false,
                                                      TestResultOrigins.Logical,
                                                      true);
                    } else {
                        if (Preferences.EveryCmdletAsTestResult) {
                            
                            //((HasScriptBlockCmdletBase)cmdlet).TestResultName = 
                            cmdlet.TestResultName = 
                                GetGeneratedTestResultNameByPosition(
                                    MyInvocation.Line,
                                    MyInvocation.PipelinePosition);
                            //((HasScriptBlockCmdletBase)cmdlet).TestResultId = string.Empty;
                            cmdlet.TestResultId = string.Empty;
                            //((HasScriptBlockCmdletBase)cmdlet).TestPassed = true;
                            cmdlet.TestPassed = true;

                            //Tmx.TmxHelper.CloseTestResult(((HasScriptBlockCmdletBase)cmdlet).TestResultName,
                            Tmx.TmxHelper.CloseTestResult(cmdlet.TestResultName,
                                                          string.Empty, //((HasScriptBlockCmdletBase)cmdlet).TestResultId, // empty, to be generated
                                                          //((HasScriptBlockCmdletBase)cmdlet).TestPassed,
                                                          cmdlet.TestPassed,
                                                          false, // isKnownIssue
                                                          // 20160116
                                                          // this.MyInvocation,
                                                          null, // Error
                                                          string.Empty,
                                                          // 20130322
                                                          //true);
                                                          // 20130626
                                                          //true,
                                                          TestResultOrigins.Automatic,
                                                          false);
                        }
                    }
                }
                catch (Exception) {
                    WriteVerbose(this, "for working with test results you need to import the TMX module");
                }
            }
        }
        
        // 20130204
        //protected override void WriteObjectMethod045OnSuccessScreenshot(PSCmdletBase cmdlet, object outputObject)
        protected void WriteObjectMethod045OnSuccessScreenshot(PSCmdletBase cmdlet, object outputObject)
        {
            //this.WriteVerbose(this, "WriteObjectMethod045OnSuccessScreenshot SePSX");
            
            if (Preferences.OnSuccessScreenShot) {
                //UIAutomation.UiaHelper.GetScreenshotOfSquare(
                SeHelper.GetScreenshotOfWebElement(
                    //(cmdlet as HasWebElementInputCmdletBase),
                    (cmdlet as CommonCmdletBase),
                    outputObject,
                    CmdletName(cmdlet), //string.Empty,
                    true,
                    // 20140111
                    // 0,
                    // 0,
                    // 0,
                    // 0,
                    new ScreenshotRect(),
                    string.Empty,
                    Preferences.OnSuccessScreenShotFormat);
            }
        }
        
        //protected override void WriteObjectMethod050OnSuccessDelay(PSCmdletBase cmdlet, object outputObject)
        // 20130204
        //protected override void WriteObjectMethod050OnSuccessDelay(PSCmdletBase cmdlet, object outputObject)
        protected void WriteObjectMethod050OnSuccessDelay(PSCmdletBase cmdlet, object outputObject)
        {
            
            WriteVerbose(this, "sleeping if sleep time is provided");
            WriteVerbose(this, (Preferences.OnSuccessDelay / 1000).ToString() + " seconds");
            System.Threading.Thread.Sleep(Preferences.OnSuccessDelay);
        }
        
        // 20130204
        //protected override void WriteObjectMethod060OutputResult(PSCmdletBase cmdlet, object outputObject)
        protected void WriteObjectMethod060OutputResult(PSCmdletBase cmdlet, object outputObject)
        {
            try {

//                //if (UnitTestMode) {
//                if (PSCmdletBase.UnitTestMode) {
//    
//                    if (null == UnitTestOutput) {
//
//                        UnitTestOutput =
//                           new System.Collections.Generic.List<object>();
//                    }
//                    UnitTestOutput.Add(outputObject);
//                } else {
                    base.WriteObject(outputObject);
//                }
            }
            catch {}
        }
        
        // 20130204
        //protected override void WriteObjectMethod070Report(PSCmdletBase cmdlet, object outputObject)
        protected void WriteObjectMethod070Report(PSCmdletBase cmdlet, object outputObject)
        {
            
            if (Preferences.AutoLog) {
                
                var reportString =
                    CmdletSignature(((CommonCmdletBase)cmdlet)) +
                    outputObject.ToString();
                
                if (cmdlet != null && reportString != null && reportString != string.Empty) { //try { WriteVerbose(this, reportString);
                    WriteVerbose(this, reportString);
                } 
                //this.WriteVerbose(this, "writing into the log");
                // 20130430
                //WriteLog(reportString);
                //WriteLog(
                WriteLog(LogLevels.Info, reportString);
                //this.WriteVerbose(this, "the log record has been written");
            
            }
        }
        
        // 20131114
        protected override void WriteSingleError(PSCmdletBase cmdlet, ErrorRecord errorRecord, bool terminating)
        {
            WriteErrorMethod010RunScriptBlocks(cmdlet);
            
            WriteErrorMethod020SetTestResult(cmdlet, errorRecord);
            
            WriteErrorMethod030ChangeTimeoutSettings(cmdlet, terminating);
            
            WriteErrorMethod040AddErrorToErrorList(cmdlet, errorRecord);
            
            WriteErrorMethod045OnErrorScreenshot(cmdlet);
            
            WriteErrorMethod050OnErrorDelay(cmdlet);
            
            WriteErrorMethod060OutputError(cmdlet, errorRecord, terminating);
            
            WriteErrorMethod070Report(cmdlet);
        }
        
        protected override void WriteErrorMethod010RunScriptBlocks(PSCmdletBase cmdlet) //, object outputObject)
        {
            WriteVerbose(this, "WriteErrorMethod010RunScriptBlocks SePSX");
        }
        
        protected override void WriteErrorMethod020SetTestResult(PSCmdletBase cmdlet, ErrorRecord errorRecord)
        {

            if (cmdlet != null) {
                // write error to the test results collection
                // CurrentData.TestResults[CurrentData.TestResults.Count - 1].Details.Add(err);
                //TestData.AddTestResultDetail(err);
                // 20160116
                // Tmx.TmxHelper.AddTestResultErrorDetail(errorRecord);
                Tmx.TmxHelper.AddTestResultErrorDetail(errorRecord.Exception);
                
                // write test result label
                try {
                    
                    // 20120328
                    CurrentData.LastResult = null;
                    var iInfo = string.Empty;
                    //if (((HasScriptBlockCmdletBase)cmdlet).TestResultName != null &&
                    if (cmdlet.TestResultName != null &&
                        //((HasScriptBlockCmdletBase)cmdlet).TestResultName.Length > 0) {
                        cmdlet.TestResultName.Length > 0) {
                        //TestData.AddTestResult
                        //string iInfo = string.Empty;
//                        if (((HasScriptBlockCmdletBase)cmdlet).TestLog){
//                            iInfo = Tmx.TmxHelper.GetInvocationInfo(this.MyInvocation);
//                        }

                        //Tmx.TmxHelper.CloseTestResult(((HasScriptBlockCmdletBase)cmdlet).TestResultName,
                        Tmx.TmxHelper.CloseTestResult(cmdlet.TestResultName,
                                                      //((HasScriptBlockCmdletBase)cmdlet).TestResultId,
                                                      cmdlet.TestResultId,
                                                      //((HasScriptBlockCmdletBase)cmdlet).TestPassed,
                                                      cmdlet.TestPassed,
                                                      //((HasScriptBlockCmdletBase)cmdlet).KnownIssue,
                                                      cmdlet.KnownIssue,
                                                      // 20160116
                                                      // this.MyInvocation,
                                                      // 20160116
                                                      // errorRecord,
                                                      errorRecord.Exception,
                                                      string.Empty,
                                                      // 20130322
                                                      //true);
                                                      // 20130626
                                                      //true,
                                                      TestResultOrigins.Automatic,
                                                      false);
                                                      //((HasScriptBlockCmdletBase)cmdlet).TestLog);
                                                      
                    } else {
                        if (Preferences.EveryCmdletAsTestResult) {
                                //((HasScriptBlockCmdletBase)cmdlet).TestResultName = 
                                cmdlet.TestResultName = 
                                    GetGeneratedTestResultNameByPosition(
                                        MyInvocation.Line,
                                        MyInvocation.PipelinePosition);
//                                    this.MyInvocation.Line + 
//                                    ", position: " +
//                                    this.MyInvocation.PipelinePosition.ToString();

                                //((HasScriptBlockCmdletBase)cmdlet).TestResultId = string.Empty;
                                cmdlet.TestResultId = string.Empty;
                                //((HasScriptBlockCmdletBase)cmdlet).TestPassed = false;
                                cmdlet.TestPassed = false;
//                                iInfo = Tmx.TmxHelper.GetInvocationInfo(this.MyInvocation);

                                //Tmx.TmxHelper.CloseTestResult(((HasScriptBlockCmdletBase)cmdlet).TestResultName,
                                Tmx.TmxHelper.CloseTestResult(cmdlet.TestResultName,
                                                              string.Empty, //((HasScriptBlockCmdletBase)cmdlet).TestResultId, // empty, to be generated
                                                              //((HasScriptBlockCmdletBase)cmdlet).TestPassed,
                                                              cmdlet.TestPassed,
                                                              false, // isKnownIssue
                                                              // 20160116
                                                              // this.MyInvocation,
                                                              // 20160116
                                                              // errorRecord,
                                                              errorRecord.Exception,
//                                                              Tmx.TmxHelper.GetScriptLineNumber(this.MyInvocation),
//                                                              Tmx.TmxHelper.GetPipelinePosition(this.MyInvocation),
//                                                              iInfo,
                                                              string.Empty,
                                                              // 20130322
                                                              //true);
                                                              // 20130626
                                                              //true,
                                                              TestResultOrigins.Automatic,
                                                              false);
                        }
                    }
                }
                catch {
                    WriteVerbose(this, "for working with test results you need to import the TMX module");
                }
            }
        }
        
        protected override void WriteErrorMethod030ChangeTimeoutSettings(PSCmdletBase cmdlet, bool terminating)
        {
            WriteVerbose(this, "WriteErrorMethod030ChangeTimeoutSettings SePSX");
        }
        
        protected override void WriteErrorMethod040AddErrorToErrorList(PSCmdletBase cmdlet, ErrorRecord errorRecord)
        {
            //WriteVerbose(this, "WriteErrorMethod040AddErrorToErrorList SePSX");
            
            // write an error to the Error list
            WriteErrorToTheList(errorRecord);
        }
        
        protected override void WriteErrorMethod045OnErrorScreenshot(PSCmdletBase cmdlet)
        {
            WriteVerbose(this, "WriteErrorMethod045OnErrorScreenshot SePSX");
            
            if (Preferences.OnErrorScreenShot) {
                //UIAutomation.UiaHelper.GetScreenshotOfSquare(
//                SeHelper.GetScreenshotOfWebElement(
//                    //(cmdlet as HasWebElementInputCmdletBase),
//                    (cmdlet as CommonCmdletBase),
//                    CmdletName(cmdlet), //string.Empty,
//                    true,
//                    0,
//                    0,
//                    0,
//                    0,
//                    string.Empty,
//                    SePSX.Preferences.OnErrorScreenShotFormat);
                
                UiaHelper.GetScreenshotOfAutomationElement(
                    (new HasControlInputCmdletBase()),
                    // 20131109
                    //AutomationElement.RootElement,
                    UiElement.RootElement,
                    CmdletName(cmdlet),
                    true,
                    // 20140111
                    // 0,
                    // 0,
                    // 0,
                    // 0,
                    new ScreenshotRect(),
                    string.Empty,
                    Preferences.OnErrorScreenShotFormat);
                
            }
        }
        
        protected override void WriteErrorMethod050OnErrorDelay(PSCmdletBase cmdlet)
        {
            System.Threading.Thread.Sleep(Preferences.OnErrorDelay);
        }
        
        protected override void WriteErrorMethod060OutputError(PSCmdletBase cmdlet, ErrorRecord errorRecord, bool terminating)
        {
            if (terminating) {
                //this.WriteVerbose(this, "terminating error !!!");
                
                // 20130430
                WriteLog(LogLevels.Fatal, errorRecord);
                
                ThrowTerminatingError(errorRecord);
            } else {
                //this.WriteVerbose(this, "regular error !!!");
                
                // 20130430
                WriteLog(LogLevels.Error, errorRecord);
                
                WriteError(errorRecord);
            }
        }
        
        protected override void WriteErrorMethod070Report(PSCmdletBase cmdlet)
        {
            //this.WriteVerbose(this, "WriteErrorMethod070Report PSePSX");
        }
        
        // 20130204
        //protected override void WriteObjectMethod080ReportFailure()
        protected void WriteObjectMethod080ReportFailure(PSCmdletBase cmdlet, object outputObject)
        {
            //this.WriteVerbose(this, "WriteErrorMethod070Report PSePSX");
        }
        
        private void WriteErrorToTheList(ErrorRecord err)
        {
            CurrentData.Error.Add(err);
            if (CurrentData.Error.Count > Preferences.MaximumErrorCount) {
                do{
                    CurrentData.Error.RemoveAt(0);
                } while (CurrentData.Error.Count > Preferences.MaximumErrorCount);
                CurrentData.Error.Capacity = Preferences.MaximumErrorCount;
            }
        }
        
#region commented
//        private void WriteLog(string record)
//        {
//            try {
//                //Global.WriteToLogFile(record);
//                WriteToLogFile(record);
//            } catch (Exception e) {
//                this.WriteVerbose(this, "Unable to write to the log file: " +
//                             Preferences.LogPath);
//                this.WriteVerbose(this, e.Message);
//            }
//        }
#endregion commented
        
        // 20120816
        // 20120209 protected void RunOnSuccessScriptBlocks(HasScriptBlockCmdletBase cmdlet)
        //protected internal void RunOnSuccessScriptBlocks(HasScriptBlockCmdletBase cmdlet)
        // 20130318
        //protected internal void RunOnSuccessScriptBlocks(PSCmdletBase cmdlet)
        protected internal void RunOnSuccessScriptBlocks(PSCmdletBase cmdlet, object[] parameters)
        {
            runTwoScriptBlockCollections(
                // 20141211
                Preferences.OnSuccessAction,
                // Preferences.OnSuccessAction.ToString(),
                cmdlet.OnSuccessAction,
                // 20130318
                //cmdlet);
                cmdlet,
                parameters);
        }
        
        // 20120209 protected void RunOnErrorScriptBlocks(HasScriptBlockCmdletBase cmdlet)
        //protected internal void RunOnErrorScriptBlocks(HasScriptBlockCmdletBase cmdlet)
        // 20130318
        //protected internal void RunOnErrorScriptBlocks(PSCmdletBase cmdlet)
        protected internal void RunOnErrorScriptBlocks(PSCmdletBase cmdlet, object[] parameters)
        {
            runTwoScriptBlockCollections(
                Preferences.OnErrorAction,
                cmdlet.OnErrorAction,
                // 20130318
                //cmdlet);
                cmdlet,
                parameters);
        }
        
        // 20120209 protected void RunOnSleepScriptBlocks(HasTimeoutCmdletBase cmdlet)
        // 20120312 0.6.11
        //protected internal void RunOnSleepScriptBlocks(HasTimeoutCmdletBase cmdlet)
        //protected internal void RunOnSleepScriptBlocks(HasControlInputCmdletBase cmdlet)
        // 20130318
        //protected internal void RunOnSleepScriptBlocks(PSCmdletBase cmdlet)
        protected internal void RunOnSleepScriptBlocks(PSCmdletBase cmdlet, object[] parameters)
        {
            //if (cmdlet is HasTimeoutCmdletBase) {
            if (cmdlet is PSCmdletBase) {
                runTwoScriptBlockCollections(
                    Preferences.OnSleepAction,
                    //((HasTimeoutCmdletBase)cmdlet).OnSleepAction,
                    ((PSCmdletBase)cmdlet).OnSleepAction,
                    // 20130318
                    //cmdlet);
                    cmdlet,
                    parameters);
            }
        }
        
        // 20130318
        //protected internal void RunOnTranscriptIntervalScriptBlocks(PSCmdletBase cmdlet)
        protected internal void RunOnTranscriptIntervalScriptBlocks(PSCmdletBase cmdlet, object[] parameters)
        {
            //if (cmdlet is HasTimeoutCmdletBase) {
            if (cmdlet is PSCmdletBase) {
                runTwoScriptBlockCollections(
                    Preferences.OnTranscriptIntervalAction,
                    //((HasTimeoutCmdletBase)cmdlet).OnSleepAction,
                    null, //((PSCmdletBase)cmdlet).OnSleepAction,
                    // 20130318
                    //cmdlet);
                    cmdlet,
                    parameters);
            }
        }
        
#region commented        
//        // temporary
//        public void WriteVerbose(this, PSCmdletBase cmdlet, string text)
//        {
//            WriteVerbose(this, text);
//        }
#endregion commented
        
        #region Log
        private static StreamWriter LogStream { get; set; }
        private static Stream Stream { get; set; }
        
        internal static void CreateLogFile()
        {
            if (Preferences.Log) {
                try {
                    Stream = 
                        File.Open(
                            Preferences.LogPath,
                            FileMode.OpenOrCreate | FileMode.Append,
                            FileAccess.Write,
                            FileShare.Write);
                    LogStream = 
                        new StreamWriter(Stream);
                } catch {
                    Preferences.LogPath = 
                        "'" +
                        Environment.GetEnvironmentVariable(
                            "TEMP",
                            EnvironmentVariableTarget.User) + 
                            @"\UIAutomation_" +
                            //UiaHelper.GetTimedFileName() +
                            PSTestHelper.GetTimedFileName() +
                            ".log" +
                            "'";
                    try {
                        Stream =
                            File.Open(
                                Preferences.LogPath,
                                FileMode.OpenOrCreate | FileMode.Append,
                                FileAccess.Write,
                                FileShare.Write);
                        LogStream = 
                            new StreamWriter(Stream);
                    } 
                    catch {
                        Preferences.Log = false;
                    }
                }
            }
        }
        
        internal static void CloseLogFile()
        {
            if (Preferences.Log) {
                if (LogStream != null) {
                    try {
                        LogStream.Flush();
                        LogStream.Close();
                    } 
                    catch { }
                    LogStream = null;
                }
            }
        }
        
        internal static void WriteToLogFile(string record)
        {
            if (Preferences.Log) {
                if (File.Exists(Preferences.LogPath)) {
                    if (LogStream == null) {
                        Stream = 
                            File.Open(
                                Preferences.LogPath,
                                FileMode.OpenOrCreate | FileMode.Append,
                                FileAccess.Write,
                                FileShare.Write);
                        LogStream = 
                            new StreamWriter(Stream);
                    }
                    var dateAndTime = 
                        DateTime.Now.ToShortDateString() + 
                        " " +
                        DateTime.Now.ToShortTimeString();
                    LogStream.WriteLine(dateAndTime + "\t" + record);
                    //  //  // LogStream.Flush();
                    //  // 
                }
            }
        }
        #endregion Log
    }
}
