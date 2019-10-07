using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FabSoftUpd.Wizard.Workflows_v1.Properties;
using System.IO;
using FabSoftUpd;
using FabSoftUpd.Wizard;
using System.Net;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace SampleUpdPlugin
{
    public class SampleEmptyUpdPlugin : FabSoftUpd.Wizard.Workflows_v1.BaseDeliveryWorkflow
    {
        public SampleEmptyUpdPlugin()
        {

        }

        private const int LATEST_VERSION = 1;
        public override int WorkflowVersion { get; set; } = LATEST_VERSION; // Don't return a hard code value in the get. Change the LATEST_VERSION variable. This is needed for upgrading workflows.

        public override DocumentWorkflow UpgradeWorkflow()
        {
            DocumentWorkflow upgradedWorkflow = base.UpgradeWorkflow();
            if (this.WorkflowVersion == LATEST_VERSION)
            {
                // Upgrade Workflow
            }
            return upgradedWorkflow;
        }


        public override bool CanAddFields
        {
            get
            {
                return false;
            }
        }

        public override bool CanTestOutput
        {
            get
            {
                return true;
            }
        }

        public override string DisplayName
        {
            get
            {
                return "Sample Plugin";
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void AttachDebugger() // Attach debugger if none is already attached and it is compiled for DEBUG.
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
            else
            {
                System.Diagnostics.Debugger.Launch();
            }
        }

        public override List<BaseProperty> GetDefaultWorkflowProperties(string jobName, string printerName)
        {
            AttachDebugger();

            return new List<BaseProperty> {
                    new OutputImageTypeProperty("outputImageType", "Output File Type")
                    {
                        DocxOutputAllowed = true,
                        PdfOutputAllowed = true,
                        XpsOutputAllowed = true,
                        TiffOutputAllowed = true
                    },
                    new PrinterOutputProperty("Printer", "Printer")
                    {
                        JobName = jobName,
                        PrintEnabled = false,
                        UserGenerated = false,
                        IsConfigured = true
                    },
                    new StaticTextProperty("username", "Username")
                    {
                        CanHaveUserInteraction = false
                    },
                    new StaticPasswordProperty("password", "Password")
                    {
                        CanHaveUserInteraction = false
                    },
                    new AnyInputSourceProperty("destination", "Destination")
                    {
                        CanHaveUserInteraction = true,
                    }
            };
        }

        public override SubmissionStatus ServiceSubmit(string jobName, string fclInfo, Dictionary<string, string> driverSettings, Logger externalHandler, Stream xpsStream, int pageIndexStart, int pageIndexEnd, List<PageDimensions> pageDimensions)
        {

            AttachDebugger();

            var status = new SubmissionStatus();
            status.Result = false;              

            try
            {

                string username = GetPropertyResult("username", "", false);
                string password = GetPropertyResult("password", "", false);
                string destination = GetPropertyResult("destination", "", false);
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new Exception("Missing property: username");
                }
                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new Exception("Missing property: password");
                }
                if (string.IsNullOrWhiteSpace(destination))
                {
                    throw new Exception("Missing property: destination");
                }

                var outputImageType = GetProperty<OutputImageTypeProperty>("outputImageType", false);
                if (outputImageType == null)
                {
                    throw new Exception("Missing property: outputImageType");
                }
                
                DocumentRenderer renderingConverter = GetRenderer(outputImageType);
                if (renderingConverter != null)
                {
                    TempFileStream outputStream = null;
                    try
                    {
                        try
                        {
                            string tempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Temp\");
                            Directory.CreateDirectory(tempFolder);
                            outputStream = new TempFileStream(tempFolder);
                        }
                        catch (Exception)
                        {
                            // Ignore - attempt another temp location (UAC may block UI from accessing Temp folder.
                        }

                        if (outputStream == null)
                        {
                            string tempFolder = Path.Combine(Path.GetTempPath(), @"FS_UPD_v4\NewUpdPlugin\");
                            Directory.CreateDirectory(tempFolder);
                            outputStream = new TempFileStream(tempFolder);
                        }

                        renderingConverter.RenderXpsToOutput(xpsStream, outputStream, pageIndexStart, pageIndexEnd, externalHandler);
                        outputStream.Seek(0, SeekOrigin.Begin);

                        string jobId;
                        if (SubmitDocumentWithCustomLogic(outputStream, username, password, destination, out jobId))
                        {
                            status.Result = true;
                            status.Message = "Successfully Submitted";
                            status.StatusCode = 0;
                            status.LogDetails = "Submitted Document with ID: " + jobId + "\r\n";
                        }
                    }
                    finally
                    {
                        if (outputStream != null)
                        {
                            outputStream.Dispose();
                            outputStream = null;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                status.Result = false;
                status.Message = "An error has occurred.";
                status.LogDetails = ex.Message;
                status.NotifyUser = true;
                status.StatusCode = 12;
            }

            return status;
        }
        
        private static bool SubmitDocumentWithCustomLogic(System.IO.Stream document, string username, string password, string destination, out string jobId)
        {
            jobId = null;
            bool success = false;
            
            // Custom Code

            return success;
        }
    
        
    }
}
