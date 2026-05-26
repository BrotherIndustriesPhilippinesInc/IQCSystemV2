using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IQCSystemV2.Functions
{
    class OpenToolHandlerV2
    {
        string searchItem;
        string twodDirectory;
        string wiDirectory;
        string artworkDirectory;
        string dciDirectory;
        string ngDirectory;
        string qhcDirectory;
        string threedDirectory;
        string generalWIDirectory;

        string serverResourceDirectory;

        private CancellationToken _token;

        public OpenToolHandlerV2(string searchItem, CancellationToken token)
        {
            this.searchItem = searchItem.Substring(0, searchItem.Length - 3);
            this._token = token;
            threedDirectory = "\\\\apbiphsh04\\41_PQCDept\\41a_IQC\\04 Inspection\\0000 OPEN Tool System\\01 3D Drawing";
            twodDirectory = "\\\\apbiphsh04\\41_PQCDept\\41a_IQC\\04 Inspection\\0000 OPEN Tool System\\02 2D Drawing";
            wiDirectory = "\\\\apbiphsh04\\41_PQCDept\\41a_IQC\\04 Inspection\\0000 OPEN Tool System\\03 Work Instruction";
            artworkDirectory = "\\\\apbiphsh04\\41_PQCDept\\41a_IQC\\04 Inspection\\0000 OPEN Tool System\\04 Artwork";
            dciDirectory = "\\\\apbiphsh04\\41_PQCDept\\41a_IQC\\04 Inspection\\0000 OPEN Tool System\\06 DCI";
            ngDirectory = "\\\\apbiphsh04\\41_PQCDept\\41a_IQC\\04 Inspection\\0000 OPEN Tool System\\07 NG Parts Illustration";
            //qhcDirectory = "\\\\apbiphsh04\\41_PQCDept\\41a_IQC\\04 Inspection\\000 OPEN System\\03 Quality History Card\\00 QHC FILES";
            qhcDirectory = "\\\\apbiphsh04\\41_PQCDept\\41a_IQC\\04 Inspection\\0000 OPEN Tool System\\05 QHC rev6";
            generalWIDirectory = "\\\\apbiphsh04\\41_PQCDept\\41a_IQC\\04 Inspection\\0000 OPEN Tool System\\10 General Work I";
            serverResourceDirectory = "\\\\apbiphiqcwb01\\htdocs\\iqcv2\\resources\\open_tool\\";
        }

        public Task<JObject> ReturnAllAsync()
        {
            return Task.Run(() =>
            {
                _token.ThrowIfCancellationRequested();
                JObject result = new JObject();
                result["threeD"] = ThreeDList();

                _token.ThrowIfCancellationRequested();
                result["twoD"] = TwoDList();

                _token.ThrowIfCancellationRequested();
                result["wi"] = WIList();

                _token.ThrowIfCancellationRequested();
                result["artwork"] = ArtWorkList();

                _token.ThrowIfCancellationRequested();
                result["dci"] = DCIList();

                _token.ThrowIfCancellationRequested();
                result["ng"] = NGList();

                _token.ThrowIfCancellationRequested();
                result["qhc"] = QHCList();

                _token.ThrowIfCancellationRequested();
                result["generalWI"] = GeneralWIList();

                return result;
            }, _token);
        }

        public Task<JObject> ReturnSpecifiedAsync(string type)
        {
            return Task.Run(() =>
            {
                JObject result = new JObject();
                switch (type)
                {
                    case "3d":
                        result["threeD"] = ThreeDList();
                        break;
                    case "2d":
                        result["twoD"] = TwoDList();
                        break;
                    case "wi":
                        result["wi"] = WIList();
                        break;
                    case "artwork":
                        result["artwork"] = ArtWorkList();
                        break;
                    case "dci":
                        result["dci"] = DCIList();
                        break;
                    case "ng":
                        result["ng"] = NGList();
                        break;
                    case "qhc":
                        result["qhc"] = QHCList();
                        break;
                    case "generalWI":
                        result["generalWI"] = GeneralWIList();
                        break;
                }
                return result;
            });
        }

        public JObject ThreeDList()
        {
            string searchPattern = $"*{searchItem}*"; // Search for files containing the search term
            JObject threeDList = new JObject
            {
                ["threeD"] = LoadFilesFromDirectory(threedDirectory, serverResourceDirectory + "3d", searchPattern)
            };
            return threeDList;
        }
        public JObject TwoDList()
        {
            string searchPattern = $"*{searchItem}*"; // Search for files containing the search term
            JObject twoDList = new JObject
            {
                ["twoD"] = LoadFilesFromDirectory(twodDirectory, serverResourceDirectory + "twoD", searchPattern)
            };
            return twoDList;
        }
        public JObject WIList()
        {
            string searchPattern = $"*{searchItem}*"; // Search for files containing the search term
            JObject wiList = new JObject
            {
                ["wi"] = LoadFilesFromDirectory(wiDirectory, serverResourceDirectory + "wi", searchPattern)
            };

            foreach (var item in wiList["wi"] as JArray)
            {
                string filePath = item["fileName"].ToString();
                MoveToServerResource(wiDirectory + "\\" + filePath, serverResourceDirectory + "03 Work Instruction");
            }

            return wiList;
        }
        public JObject ArtWorkList()
        {
            string searchPattern = $"*{searchItem}*"; // Search for files containing the search term
            JObject artworkList = new JObject
            {
                ["artwork"] = LoadFilesFromDirectory(artworkDirectory, serverResourceDirectory + "artwork", searchPattern)
            };
            return artworkList;
        }
        public JObject DCIList()
        {
            string searchPattern = $"*{searchItem}*"; // Search for files containing the search term
            JObject dciList = new JObject
            {
                ["dci"] = LoadFilesFromDirectory(dciDirectory, serverResourceDirectory + "dci", searchPattern)
            };
            return dciList;
        }
        public JObject NGList()
        {
            string searchPattern = $"*{searchItem}*"; // Search for files containing the search term
            JObject ngList = new JObject
            {
                ["ng"] = LoadFilesFromDirectory(ngDirectory, serverResourceDirectory + "ng", searchPattern)
            };
            return ngList;
        }
        public JObject QHCList()
        {
            string searchPattern = $"*{searchItem}*"; // Search for files containing the search term
            JObject qhcList = new JObject
            {
                ["qhc"] = LoadFilesFromDirectory(qhcDirectory, serverResourceDirectory + "qhc", searchPattern)
            };
            return qhcList;
        }
        public JObject GeneralWIList()
        {
            string searchPattern = $"*{searchItem}*"; // Search for files containing the search term
            JObject generalWIList = new JObject
            {
                ["generalWI"] = LoadFilesFromDirectory(generalWIDirectory, serverResourceDirectory + "generalWI")
            };
            return generalWIList;
        }

        private JArray LoadFilesFromDirectory(string directoryPath, string pdfDestinationPath, string searchPattern = "*")
        {
            JArray result = new JArray();
            // Use a HashSet to track names and prevent duplicates (case-insensitive)
            HashSet<string> processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    WriteToLocalLog("INFO", directoryPath, "N/A", $"Directory does not exist.");
                    MessageBox.Show("Directory does not exist.");
                    return result;
                }

                WriteToLocalLog("INFO", directoryPath, "N/A", $"Started scanning directory for pattern: {searchPattern}");
                var files = Directory.GetFiles(directoryPath, searchPattern);
                WriteToLocalLog("DEBUG", directoryPath, "N/A", "LoadFiles", $"Found {files.Length} files matching {searchPattern}");

                foreach (var file in files)
                {
                    if (_token.IsCancellationRequested)
                        return result;
                    FileInfo fileInfo = new FileInfo(file);

                    // 1. Skip files with '~' (temp files)
                    if (fileInfo.Name.Contains("~"))
                        continue;

                    // 2. Check if this file name has already been added to our list
                    if (processedFiles.Contains(fileInfo.Name))
                    {
                        WriteToLocalLog("DEBUG", fileInfo.FullName, "N/A", "Skipped: Duplicate file name");
                        continue;
                    }

                    JObject fileEntry = new JObject
                    {
                        ["fileName"] = fileInfo.Name,
                        ["pdfName"] = null
                    };

                    bool shouldAdd = false;
                    try
                    {

                        if (fileInfo.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            string destinationFile = Path.Combine(pdfDestinationPath, fileInfo.Name);

                            if (!destinationFile.Contains("generalWI")) {
                                File.Copy(fileInfo.FullName, destinationFile, true);
                            } 
                            

                            WriteToLocalLog("SUCCESS", fileInfo.FullName, destinationFile, "PDF copied");
                            shouldAdd = true;
                        }
                        else if (fileInfo.Extension.Equals(".xls", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                        fileInfo.Extension.Equals(".xlsm", StringComparison.OrdinalIgnoreCase))
                        {
                            // Logic for potential conversion (commented out per your snippet)
                            string pdfName = ConvertToPdf(fileInfo.FullName, pdfDestinationPath);
                            fileEntry["pdfName"] = pdfName;
                            shouldAdd = true;
                        }
                        else
                        {
                            WriteToLocalLog("INFO", fileInfo.FullName, "N/A", "Skipped: Unsupported file type");
                            shouldAdd = true;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Break the loop immediately and return what we have
                        WriteToLocalLog("INFO", directoryPath, "N/A", "LoadFiles", "Scan stopped due to cancellation.");
                        return result;
                    }

                    // 3. Finalize adding the entry
                    if (shouldAdd)
                    {
                        result.Add(fileEntry);
                        processedFiles.Add(fileInfo.Name); // Mark as processed
                    }
                }
            }

            catch (Exception ex)
            {

                WriteToLocalLog("DEBUG", "APP", "N/A", $"Identity: {System.Security.Principal.WindowsIdentity.GetCurrent().Name}");
                //MessageBox.Show($"Error processing files: {ex.Message}");
            }

            return result;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private string ConvertToPdf(string excelFilePath, string pdfFilePath)
        {
            Microsoft.Office.Interop.Excel.Application excelApp = null;
            Workbook workbook = null;
            uint processId = 0;

            try
            {
                _token.ThrowIfCancellationRequested();

                excelApp = new Microsoft.Office.Interop.Excel.Application();

                // Capture the Process ID of THIS specific Excel instance
                GetWindowThreadProcessId((IntPtr)excelApp.Hwnd, out processId);

                excelApp.DisplayAlerts = false;
                excelApp.Visible = false;

                _token.ThrowIfCancellationRequested();

                workbook = excelApp.Workbooks.Open(excelFilePath, UpdateLinks: 0, ReadOnly: true);

                _token.ThrowIfCancellationRequested();

                string pdfFullPath = Path.Combine(pdfFilePath, Path.GetFileNameWithoutExtension(excelFilePath) + ".pdf");
                workbook.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, pdfFullPath);

                return Path.GetFileName(pdfFullPath);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException || _token.IsCancellationRequested)
                {
                    WriteToLocalLog("ABORT", excelFilePath, "N/A", "ConvertToPdf", "User cancelled. Killing process.");
                    // Force kill the process if we are in a cancellation state
                    KillExcelProcess(processId);
                    throw new OperationCanceledException(_token);
                    throw;
                }
                WriteToLocalLog("ERROR", excelFilePath, "N/A", "ConvertToPdf", ex.Message);
                return null;
            }
            finally
            {
                // 4. Proper COM Release Sequence
                if (workbook != null)
                {
                    try
                    {
                        workbook.Close(false);
                    }
                    catch { }
                    Marshal.ReleaseComObject(workbook);
                    workbook = null;
                }

                if (excelApp != null)
                {
                    try
                    {
                        excelApp.Quit();
                    }
                    catch { }
                    Marshal.ReleaseComObject(excelApp);
                    excelApp = null;
                }

                // 5. Final Garbage Collection (Forces the Task Manager to clear)
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // 6. If it's STILL in Task Manager because of a hang/cancellation, kill it
                if (_token.IsCancellationRequested)
                {
                    KillExcelProcess(processId);
                }
            }
        }

        private void KillExcelProcess(uint pid)
        {
            if (pid == 0)
                return;
            try
            {
                // This targets ONLY the ID we captured, leaving your other work safe
                Process.GetProcessById((int)pid).Kill();
            }
            catch { /* Process already closed naturally */ }
        }

        private void WriteToLocalLog(string status, string source, string output, string methodName, string message = "")
        {
            try
            {
                string logFolder = "C:\\IQC_v2_logs\\Conversion";
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                // Capture System Info
                string computerName = Environment.MachineName;
                string systemUser = Environment.UserName; // The Windows account running the app

                // New Naming Format: Log_2026-03-18_PCNAME_User.txt
                string logFileName = $"Log_{DateTime.Now:yyyy-MM-dd}_{computerName}_{systemUser}.txt";
                string logFile = Path.Combine(logFolder, logFileName);

                // Formatting the entry
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"[{DateTime.Now:HH:mm:ss}] [{status}]");
                sb.AppendLine($"  Method: {methodName}");
                sb.AppendLine($"  PC:     {computerName} | Windows User: {systemUser}");
                sb.AppendLine($"  Source: {source}");
                sb.AppendLine($"  Detail: {message}");
                sb.AppendLine(new string('-', 60));

                File.AppendAllText(logFile, sb.ToString());
            }
            catch
            {
                Console.WriteLine("Error writing to log file.");
            }
        }

        private void MoveToServerResource(string sourcePath, string destinationDirectory)
        {
            try
            {
                // Ensure destination directory exists
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                string fileName = Path.GetFileName(sourcePath);
                string destinationPath = Path.Combine(destinationDirectory, fileName);



                // If file already exists in destination, delete it first
                if (File.Exists(destinationPath))
                {
                    SetFileAttributesToNormal(destinationPath);
                    Console.WriteLine("File already exist: " + destinationPath);
                    WriteToLocalLog("CLEANUP", "N/A", destinationPath, "MoveToServer", "Deleting existing file to overwrite.");
                    File.Delete(destinationPath);
                }
                else
                {
                    // Copy file to destination
                    //File.Copy(sourcePath, destinationPath);
                }

                File.Copy(sourcePath, destinationPath);
                WriteToLocalLog("SYNC", sourcePath, destinationPath, "MoveToServer", "File successfully copied to web server resources.");
                Console.WriteLine($"Successfully moved file to server resource: {destinationPath}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied: {ex.Message}");
                MessageBox.Show($"Access denied while pasting file: {ex.Message}");
                WriteToLocalLog("CRITICAL", sourcePath, destinationDirectory, "MoveToServer", $"Copy failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving file to server resource: {ex.Message}");
                MessageBox.Show($"Error moving file to server resource: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetFileAttributesToNormal(string filePath)
        {
            try
            {
                // Get current file attributes
                FileAttributes attributes = File.GetAttributes(filePath);

                // Remove Hidden and ReadOnly attributes
                if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    attributes &= ~FileAttributes.Hidden; // Remove Hidden attribute
                }
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    attributes &= ~FileAttributes.ReadOnly; // Remove ReadOnly attribute
                }

                // Set the file attributes back to normal
                File.SetAttributes(filePath, attributes);

                Console.WriteLine($"File attributes for {filePath} set to normal.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting file attributes to normal for {filePath}: {ex.Message}");
            }
        }
    }
}
