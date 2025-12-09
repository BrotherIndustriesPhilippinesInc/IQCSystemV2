using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IQCSystemV2.Functions
{
    class OpenToolHandler
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

        public OpenToolHandler(string searchItem)
        {

            this.searchItem = searchItem.Substring(0, searchItem.Length - 3);

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
                JObject result = new JObject();

                result["threeD"] = ThreeDList();
                result["twoD"] = TwoDList();
                result["wi"] = WIList();
                result["artwork"] = ArtWorkList();
                result["dci"] = DCIList();
                result["ng"] = NGList();
                result["qhc"] = QHCList();
                result["generalWI"] = GeneralWIList();

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

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    MessageBox.Show("Directory does not exist.");
                    return result;
                }

                var files = Directory.GetFiles(directoryPath, searchPattern);

                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);

                    // Skip files with '~' in the name
                    if (fileInfo.Name.Contains("~"))
                        continue;

                    JObject fileEntry = new JObject
                    {
                        ["fileName"] = fileInfo.Name,
                        ["pdfName"] = null // Default to null; assign later if applicable
                    };

                    if (fileInfo.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        // Already a PDF, no conversion needed
                        MoveToServerResource(fileInfo.FullName, pdfDestinationPath);
                        result.Add(fileEntry);

                    }
                    else if (fileInfo.Extension.Equals(".xls", StringComparison.OrdinalIgnoreCase) ||
                             fileInfo.Extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                             fileInfo.Extension.Equals(".xlsm", StringComparison.OrdinalIgnoreCase))
                    {
                        // Convert Excel file to PDF
                        string pdfFilePath = Path.Combine(pdfDestinationPath, $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.pdf");
                        string pdfName = ConvertToPdf(fileInfo.FullName, pdfDestinationPath);
                        fileEntry["pdfName"] = pdfName;
                        result.Add(fileEntry);
                    }
                    else
                    {
                        // Other file types, no conversion
                        result.Add(fileEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                // Consider logging the error
                Console.WriteLine($"Error processing files: {ex.Message}");
                MessageBox.Show($"Error processing files: {ex.Message}");
            }

            return result;
        }

        private string ConvertToPdf(string excelFilePath, string pdfFilePath)
        {
            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook workbook = null;

            try
            {
                // Open the Excel file
                workbook = excelApp.Workbooks.Open(excelFilePath, ReadOnly: true);

                // Generate the full path of the PDF by combining the directory and the file name
                string pdfFullPath = Path.Combine(pdfFilePath, Path.GetFileNameWithoutExtension(excelFilePath) + ".pdf");

                // Export as PDF
                workbook.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, pdfFullPath);

                Console.WriteLine("Excel file successfully converted to PDF.");

                // Extract and return the PDF file name (not the full path)
                string pdfFileName = Path.GetFileName(pdfFullPath);

                Console.WriteLine("Generated PDF file name: " + pdfFileName);
                return pdfFileName; // Return the file name (e.g., "QHC_D01G8F001_Calamba Shinei Industry Philippines.pdf")
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null; // Return null if an error occurs
            }
            finally
            {
                // Clean up
                if (workbook != null)
                {
                    workbook.Close(false);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                }

                if (excelApp != null)
                {
                    excelApp.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                }
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
                    File.Delete(destinationPath);
                }
                else
                {
                    // Copy file to destination
                    //File.Copy(sourcePath, destinationPath);
                }

                File.Copy(sourcePath, destinationPath);

                Console.WriteLine($"Successfully moved file to server resource: {destinationPath}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied: {ex.Message}");
                MessageBox.Show($"Access denied while pasting file: {ex.Message}");
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
