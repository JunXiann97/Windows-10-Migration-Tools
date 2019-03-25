using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel; // CancelEventArgs
using System.Windows.Threading;

namespace Window_10_Migration_Tools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Boolean OCV = false;
        public static Boolean Fringe = false;
        public static Boolean restore = true;
        public static long contentSize = 0;
        Boolean win7 = true;
        Boolean win10 = true;
        public static CustomiseMessageBox messageForm = new CustomiseMessageBox();
        BackgroundWorker backgroundWorker1 = new System.ComponentModel.BackgroundWorker();

        public MainWindow()
        {
            
            System.Windows.Forms.Application.DoEvents();
            InitializeComponent();
            System.Windows.Forms.Application.DoEvents();
            CenterWindowOnScreen();
            System.Windows.Forms.Application.DoEvents();
            String subKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion";
            RegistryKey key = Registry.LocalMachine;
            RegistryKey skey = key.OpenSubKey(subKey);

            string ProductName = skey.GetValue("ProductName").ToString();
            char WindowsOS = ProductName[8];

            if (WindowsOS == '7')
            {
                win10 = false;
                restoreBtn.IsEnabled = false;
            }

            else
            {
                
                win7 = false;
                browseBackup.IsEnabled = false;
            }

        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);

        }

        public void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            DialogResult closeConfirmation = System.Windows.Forms.MessageBox.Show("Confirm to Exit?", "Warning", MessageBoxButtons.YesNo);

            if (closeConfirmation == System.Windows.Forms.DialogResult.Yes)
            {
                int MainWinActive = 0;
                Process[] MainWin = Process.GetProcessesByName("Windows 10 Migration Tools");
                if (MainWin.Length == 0)
                {
                    MainWinActive = 0;
                }

                else
                {
                    MainWinActive = 1;
                }

                //---------------Kill Windows 10 Migration Tools Process ---------------//
                if (MainWinActive == 1)
                {
                    foreach (System.Diagnostics.Process myProc in System.Diagnostics.Process.GetProcessesByName("Windows 10 Migration Tools"))
                    {
                        if (myProc.ProcessName == "Windows 10 Migration Tools")
                        {

                            myProc.Kill();

                        }
                    }
                }
                //--------------- End Kill Windows 10 Migration Tools Process ---------------//
            }

            else
            {
                e.Cancel = true;
            }
                
        }

        public void Displose()
        {
            this.Close();
            
        }

        public void ExportKey(string RegKey, string SavePath)
        {
            System.Windows.Forms.Application.DoEvents();
            string path = "\"" + SavePath + "\"";
            string key = "\"" + RegKey + "\"";

            Process proc = new Process();
            try
            {

                proc.StartInfo.FileName = "regedit.exe";
                proc.StartInfo.UseShellExecute = false;
                proc = Process.Start("regedit.exe", "/e " + path + " " + key + "");

                if (proc != null) proc.WaitForExit();
            }
            finally
            {
                if (proc != null) proc.Dispose();
            }
        }

        public static long GetFileSizeSumFromDirectory(string searchDirectory)
        {
            
            System.Windows.Forms.Application.DoEvents();
            var files = Directory.EnumerateFiles(searchDirectory);

            //get the sizeof all files in the current directory
            var currentSize = (from file in files let fileInfo = new FileInfo(file) select fileInfo.Length).Sum();

            var directories = Directory.EnumerateDirectories(searchDirectory);

            //get the size of all files in all subdirectories
            var subDirSize = (from directory in directories select GetFileSizeSumFromDirectory(directory)).Sum();

            return currentSize + subDirSize;
        }

        public static List<Task> TaskList = new List<Task>();

        public static async Task CopyFolderContents(string sourceFolder, string destinationFolder)
        {
            
            

            System.Windows.Forms.Application.DoEvents();
            if (Directory.Exists(sourceFolder))
            {
                // Copy folder structure
                foreach (string sourceSubFolder in Directory.GetDirectories(sourceFolder, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(sourceSubFolder.Replace(sourceFolder, destinationFolder));
                }

                // Copy files
                foreach (string sourceFile in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
                {
                    Uri path = new Uri(sourceFile);
                    string[] segment = path.Segments;
                    string goodImages = segment[2];
                    long length = new System.IO.FileInfo(sourceFile).Length;
                    long mbSize = ((length / 1024) / 1024);

                    

                    if (goodImages == "Goodimages/")
                    {
                        string destinationFile = sourceFile.Replace(sourceFolder, destinationFolder);
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                                   new Action(() => {
                                       File.Copy(sourceFile, destinationFile, true);
                                   }));

                        contentSize = contentSize + mbSize;

                        System.Windows.Forms.Application.DoEvents();
                        messageForm.TotalCompletedSize.Content = contentSize;
                        System.Windows.Forms.Application.DoEvents();

                        messageForm.LogBox.Text = "Copying " + sourceFile;
                        System.Windows.Forms.Application.DoEvents();
                        messageForm.progress_bar_TotalFile.Value += mbSize;
                    }
                    
                    else
                    {
                        string myFolder = segment[4];

                        if (MainWindow.OCV == false && MainWindow.Fringe == true)
                        {
                            if (myFolder == "OCV/" || myFolder == "CalImages/" || goodImages == "Goodimages/")
                            {

                            }

                            else
                            {
                                contentSize = contentSize + mbSize;

                                System.Windows.Forms.Application.DoEvents();
                                messageForm.TotalCompletedSize.Content = contentSize;
                                System.Windows.Forms.Application.DoEvents();
                                string destinationFile = sourceFile.Replace(sourceFolder, destinationFolder);
                                

                                System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                                   new Action(() => {
                                       File.Copy(sourceFile, destinationFile, true);
                                   }));

                                

                                System.Windows.Forms.Application.DoEvents();
                                messageForm.LogBox.Text = "Copying " + sourceFile;

                                System.Windows.Forms.Application.DoEvents();
                                messageForm.progress_bar_TotalFile.Value += mbSize;

                                System.Windows.Forms.Application.DoEvents();

                                /*System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                                   new Action(() => { messageForm.LogBox.Text = "Copying " + sourceFile;
                                   }));*/

                            }


                        }

                        else if (MainWindow.OCV == false && MainWindow.Fringe == false)
                        {
                            if (myFolder == "OCV/" || myFolder == "CalImages/" || goodImages == "Goodimages/")
                            {

                            }

                            else
                            {
                                contentSize = contentSize + mbSize;

                                System.Windows.Forms.Application.DoEvents();
                                messageForm.TotalCompletedSize.Content = contentSize;
                                System.Windows.Forms.Application.DoEvents();

                                string destinationFile = sourceFile.Replace(sourceFolder, destinationFolder);
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                                   new Action(() => {
                                       File.Copy(sourceFile, destinationFile, true);
                                   }));
                                System.Windows.Forms.Application.DoEvents();
                                messageForm.LogBox.Text = "Copying " + sourceFile;

                                System.Windows.Forms.Application.DoEvents();
                                messageForm.progress_bar_TotalFile.Value += mbSize;
                            }
                        }

                        else if (MainWindow.Fringe == true)
                        {
                            if (myFolder == "CalImages/" || goodImages == "Goodimages/")
                            {
                                
                            }

                            else
                            {
                                contentSize = contentSize + mbSize;

                                System.Windows.Forms.Application.DoEvents();
                                messageForm.TotalCompletedSize.Content = contentSize;
                                System.Windows.Forms.Application.DoEvents();

                                string destinationFile = sourceFile.Replace(sourceFolder, destinationFolder);
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                                   new Action(() => {
                                       File.Copy(sourceFile, destinationFile, true);
                                   }));
                                System.Windows.Forms.Application.DoEvents();
                                messageForm.LogBox.Text = "Copying " + sourceFile;

                                System.Windows.Forms.Application.DoEvents();
                                messageForm.progress_bar_TotalFile.Value += mbSize;
                            }
                        }

                        else
                        {
                            contentSize = contentSize + mbSize;

                            System.Windows.Forms.Application.DoEvents();
                            messageForm.TotalCompletedSize.Content = contentSize;
                            System.Windows.Forms.Application.DoEvents();

                            string destinationFile = sourceFile.Replace(sourceFolder, destinationFolder);
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                                   new Action(() => {
                                       File.Copy(sourceFile, destinationFile, true);
                                   }));
                            System.Windows.Forms.Application.DoEvents();
                            messageForm.LogBox.Text = "Copying " + sourceFile;

                            System.Windows.Forms.Application.DoEvents();
                            messageForm.progress_bar_TotalFile.Value += mbSize;


                        }
                    }
                    

                }
            }

            //Delete empty OCV and CalImages folder

            if (MainWindow.OCV == false && MainWindow.Fringe == true)
            {
                if (Directory.Exists(destinationFolder + "\\OCV"))
                {
                    Directory.Delete(destinationFolder + "\\OCV", true);
                }

                if (Directory.Exists(destinationFolder + "\\CalImages"))
                {
                    Directory.Delete(destinationFolder + "\\CalImages", true);
                }
            }

            else if (MainWindow.OCV == false && MainWindow.Fringe == false)
            {
                if (Directory.Exists(destinationFolder + "\\OCV"))
                {
                    Directory.Delete(destinationFolder + "\\OCV", true);
                }
            }


            else if (MainWindow.Fringe == true)
            {
                if (Directory.Exists(destinationFolder + "\\CalImages"))
                {
                    Directory.Delete(destinationFolder + "\\CalImages", true);
                }
            }
        }



        //end CopyFolderContents

        public static async Task RestoreFolderContent(string sourceFolder, string destinationFolder)
        {
            if (Directory.Exists(sourceFolder))
            {
                // Copy folder structure
                foreach (string sourceSubFolder in Directory.GetDirectories(sourceFolder, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(sourceSubFolder.Replace(sourceFolder, destinationFolder));
                }

                // Copy files
                foreach (string sourceFile in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
                {

                    long length = new System.IO.FileInfo(sourceFile).Length;
                    long mbSize = ((length / 1024) / 1024);

                    contentSize = contentSize + mbSize;

                    System.Windows.Forms.Application.DoEvents();
                    messageForm.TotalCompletedSize.Content = contentSize;
                    System.Windows.Forms.Application.DoEvents();

                    string destinationFile = sourceFile.Replace(sourceFolder, destinationFolder);
                    File.Copy(sourceFile, destinationFile, true);

                    System.Windows.Forms.Application.DoEvents();
                    messageForm.LogBox.Text = "Copying " + sourceFile;

                    System.Windows.Forms.Application.DoEvents();
                    messageForm.progress_bar_TotalFile.Value += mbSize;
                }
            }
        }

        //Get Current Date and Time
        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy-MM-dd-HHmmtt");
        }
        //End of GetTimestamp

        //For Log file record
        public static String LogTime(DateTime value)
        {
            return value.ToString("[yyyy-MM-dd-HHmmtt]-");
        }
        //End Log file record


        private void win7_backup(Object sender, RoutedEventArgs e)
        {
            contentSize = 0;
            messageForm.TotalCompletedSize.Content = "0";
            messageForm.progress_bar_TotalFile.Value = 0;
            //-------------------For Back Up-------------------//
            string V510folder = "C:\\Program Files (x86)\\V510 Series";
            string VitroxLicenseServerfolder = "C:\\Program Files (x86)\\VitroxLicenseServer";
            string PLCfolder = "C:\\cpi\\plc";
            string DefectPackagerfolder = @"C:\Program Files\DefectPackager";
            string Jetpowerfolder = @"C:\cpi\jetpower";
            string Configfolder = @"C:\cpi\config";
            string Toolsfolder = @"C:\cpi\tools";
            string Data = @"C:\cpi\data";
            string ocvFolder = @"C:\cpi\data\OCV";
            string CADfolder = @"C:\cpi\cad";
            string GoodImagefolder = @"C:\Goodimages";
            string fringeFolder = @"C:\cpi\fringe";
            string calImagesFolder = @"C:\cpi\fringe\CalImages";
            string regFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            //-------------------For Back Up-------------------//

            //-------------------------For Summary Message Shown-------------------------//
            string summary = "Back up completed\nBack up status stated below\n\nBackup Status\n======================\n";
            

            string Registry      = "- Registry (Completed)\n";
            string v510          = "- V510 Series (Completed)\n";
            string vitroxLicense = "- VitroxLicenseServer (Completed)\n";
            string plc           = "- PLC (Completed)\n";
            string defect        = "- Defect Packager (Completed)\n";
            string jetpower      = "- Jetpower (Completed)\n";
            string config        = "- Config (Completed)\n";
            string tools         = "- Tools (Completed)\n";
            string data          = "- Data (Completed)\n";
            string ocv           = "- OCV (Completed)\n";
            string cad           = "- Cad (Completed)\n";
            string goodImage     = "- Good Images (Completed)\n";
            string fringe        = "- Fringe (Completed)\n";
            //-------------------------End of For Summary Message Shown-------------------------//

            //-------------------For Dialog Result----------------------//
            OCV = false;
            Boolean CAD = false;
            Boolean GoodImage = false;
            Fringe = false;
            //-------------------For Dialog Result----------------------//
            
            long OCVSize = 0;

            long cad_MB = 0;
            long ocv_MB = 0;
            long gImg_MB = 0;
            long fringe_MB = 0;
            long v510_MB = 0;
            long VLS_MB = 0;
            long PLC_MB = 0;
            long DefectPackager_MB = 0;
            long Jetpower_MB = 0;
            long Config_MB = 0;
            long Tools_MB = 0;
            long Data_MB = 0;
            long calImg_MB = 0;

            //----------------------For start and kill process----------------------------//
            int aoiActive = 0;
            int gsActive = 0;
            int guiActive = 0;
            int ntActive = 0;
            int engineActive = 0;
            //----------------------For start and kill process----------------------------//
            
            string logTime = "";

            int FileTotalCount = 0;
            int FileComplete = 0;
            

            if (win7 == true)
            {
                DriveInfo[] freespace = DriveInfo.GetDrives();

                long totalSize = 0;
                
                System.IO.DirectoryInfo V510_info = new System.IO.DirectoryInfo(V510folder);
                System.IO.DirectoryInfo vitroxLicense_info = new System.IO.DirectoryInfo(VitroxLicenseServerfolder);
                System.IO.DirectoryInfo plc_info = new System.IO.DirectoryInfo(PLCfolder);
                System.IO.DirectoryInfo defectPackage_info = new System.IO.DirectoryInfo(DefectPackagerfolder);
                System.IO.DirectoryInfo jetPower_info = new System.IO.DirectoryInfo(Jetpowerfolder);
                System.IO.DirectoryInfo config_info = new System.IO.DirectoryInfo(Configfolder);
                System.IO.DirectoryInfo tools_info = new System.IO.DirectoryInfo(Toolsfolder);
                System.IO.DirectoryInfo data_info = new System.IO.DirectoryInfo(Data);
                System.IO.DirectoryInfo ocv_info = new System.IO.DirectoryInfo(ocvFolder);
                System.IO.DirectoryInfo cad_info = new System.IO.DirectoryInfo(CADfolder);
                System.IO.DirectoryInfo goodImage_info = new System.IO.DirectoryInfo(GoodImagefolder);
                System.IO.DirectoryInfo fringe_info = new System.IO.DirectoryInfo(fringeFolder);

                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Do you want to back up CAD folder?", "Confirmation", MessageBoxButtons.YesNo);
                if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    CAD = true;
                    if (Directory.Exists(CADfolder))
                    {
                        long bSize = GetFileSizeSumFromDirectory(CADfolder);
                        long mbSize = ((bSize / 1024) / 1024);
                        cad_MB = mbSize;
                        totalSize += mbSize;

                        FileTotalCount = FileTotalCount + 1;
                    }

                    else
                    {
                        string cadNotFound = "- CAD Folder (Not Found in C:\\CPI\\cad)\n";
                        summary += cadNotFound;
                    }

                }
                if (dialogResult == System.Windows.Forms.DialogResult.No)
                {
                    if (Directory.Exists(CADfolder))
                    {
                        string cadDecline = "- CAD Folder (Not Chosen)\n";
                        summary += cadDecline;
                        CAD = false;
                    }

                    else
                    {
                        string cadNotFound = "- CAD Folder (Not Found in C:\\CPI\\cad)\n";
                        summary += cadNotFound;
                    }

                }

                DialogResult dialogResult1 = System.Windows.Forms.MessageBox.Show("Do you want to back up OCV folder?", "Confirmation", MessageBoxButtons.YesNo);

                if (dialogResult1 == System.Windows.Forms.DialogResult.Yes)
                {
                    if(Directory.Exists(ocvFolder))
                    {
                        OCV = true;
                        long bSize = GetFileSizeSumFromDirectory(ocvFolder);
                        OCVSize = ((bSize / 1024) / 1024);
                        ocv_MB = OCVSize;
                        totalSize += OCVSize;
                        FileTotalCount = FileTotalCount + 1;
                    }

                    else
                    {
                        string noOCV = "- OCV Folder (Not Found in C:\\CPI\\data\\OCV)\n";
                        summary += noOCV;
                    }
                }

                if (dialogResult1 == System.Windows.Forms.DialogResult.No)
                {
                    if (Directory.Exists(ocvFolder))
                    {
                        string ocvDecline = "- OCV Folder (Not Chosen)\n";
                        summary += ocvDecline;
                        OCV = false;
                    }

                    else
                    {
                        string noOCV = "- OCV Folder (Not Found in C:\\CPI\\data\\OCV)\n";
                        summary += noOCV;
                    }
                }

                DialogResult dialogResult2 = System.Windows.Forms.MessageBox.Show("Do you want to back up Good Image folder?", "Confirmation", MessageBoxButtons.YesNo);

                if (dialogResult2 == System.Windows.Forms.DialogResult.Yes)
                {
                    GoodImage = true;
                    if (Directory.Exists(GoodImagefolder))
                    {
                        long bSize = GetFileSizeSumFromDirectory(GoodImagefolder);
                        long mbSize = ((bSize / 1024) / 1024);
                        gImg_MB = mbSize;
                        FileTotalCount = FileTotalCount + 1;
                        totalSize += mbSize;
                    }

                    else
                    {
                        string noGoodImages = "- GoodImages Folder (Not Found in C:\\Goodimages)\n";
                        summary += noGoodImages;
                    }

                }
                if (dialogResult2 == System.Windows.Forms.DialogResult.No)
                {
                    if (Directory.Exists(GoodImagefolder))
                    {
                        string goodImgDecline = "- Good Images Folder (Not Chosen)\n";
                        summary += goodImgDecline;
                        GoodImage = false;
                    }

                    else
                    {
                        string noGoodImages = "- GoodImages Folder (Not Found in C:\\Goodimages)\n";
                        summary += noGoodImages;
                    }
                }


                DialogResult dialogResult3 = System.Windows.Forms.MessageBox.Show("Do you want to back up fringe folder?", "Confirmation", MessageBoxButtons.YesNo);

                if (dialogResult3 == System.Windows.Forms.DialogResult.Yes)
                {
                    if (Directory.Exists(fringeFolder))
                    {
                        long calBsize = 0;
                        long calMBsize = 0;
                        Fringe = true;
                        long bSize = GetFileSizeSumFromDirectory(fringeFolder);
                        long mbSize = ((bSize / 1024) / 1024);
                        
                        if (Directory.Exists(calImagesFolder))
                        {
                            calBsize = GetFileSizeSumFromDirectory(calImagesFolder);
                            calMBsize = ((calBsize / 1024) / 1024);
                        }
                        
                        calImg_MB = calMBsize;
                        FileTotalCount = FileTotalCount + 1;
                        fringe_MB = mbSize - calMBsize;
                        totalSize += fringe_MB;

                    }

                    else
                    {
                        string fringeNotFound = "- Fringe Folder (Not Found in C:\\cpi\\fringe)\n";
                        summary += fringeNotFound;
                    }
                }

                if (dialogResult3 == System.Windows.Forms.DialogResult.No)
                {
                    DialogResult dialogResult4 = System.Windows.Forms.MessageBox.Show("Are you sure you DONT WANT to back up fringe folder? \n\n(Need to REDO CALIBRATION if fringe folder is not back up)\n\n'Yes' to continue   |   'No' to back up", "Confirmation", MessageBoxButtons.YesNo);
                    if (dialogResult4 == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (Directory.Exists(fringeFolder))
                        {
                            string fringeDecline = "- Fringe Folder (Not Chosen)\n";
                            summary += fringeDecline;
                            Fringe = false;
                        }

                        else
                        {
                            string fringeNotFound = "- Fringe Folder (Not Found in C:\\cpi\\fringe)\n";
                            summary += fringeNotFound;
                        }

                    }
                    
                    if (dialogResult4 == System.Windows.Forms.DialogResult.No)
                    {
                        if (Directory.Exists(fringeFolder))
                        {
                            long calBsize = 0;
                            long calMBsize = 0;
                            Fringe = true;
                            long bSize = GetFileSizeSumFromDirectory(fringeFolder);
                            long mbSize = ((bSize / 1024) / 1024);

                            if(Directory.Exists(calImagesFolder))
                            {
                                calBsize = GetFileSizeSumFromDirectory(calImagesFolder);
                                calMBsize = ((calBsize / 1024) / 1024);
                            }
                            
                            calImg_MB = calMBsize;

                            FileTotalCount = FileTotalCount + 1;
                            fringe_MB = mbSize - calMBsize;
                            totalSize += fringe_MB;
                        }
                        else
                        {
                            string fringeNotFound = "- Fringe Folder (Not Found in C:\\cpi\\fringe)\n";
                            summary += fringeNotFound;
                        }
                    }
                }


                if (Directory.Exists(V510folder))
                {
                    long bSize = GetFileSizeSumFromDirectory(V510folder);
                    long mbSize = ((bSize / 1024) / 1024);
                    v510_MB = mbSize;
                    totalSize += mbSize;
                    FileTotalCount = FileTotalCount + 1;

                }

                if (Directory.Exists(VitroxLicenseServerfolder))
                {
                    long bSize = GetFileSizeSumFromDirectory(VitroxLicenseServerfolder);
                    long mbSize = ((bSize / 1024) / 1024);
                    VLS_MB = mbSize;
                    totalSize += mbSize;
                    FileTotalCount = FileTotalCount + 1;
                }

                if (Directory.Exists(PLCfolder))
                {
                    long bSize = GetFileSizeSumFromDirectory(PLCfolder);
                    long mbSize = ((bSize / 1024) / 1024);
                    PLC_MB = mbSize;
                    totalSize += mbSize;
                    FileTotalCount = FileTotalCount + 1;
                }

                if (Directory.Exists(DefectPackagerfolder))
                {
                    long bSize = GetFileSizeSumFromDirectory(DefectPackagerfolder);
                    long mbSize = ((bSize / 1024) / 1024);
                    DefectPackager_MB = mbSize;
                    totalSize += mbSize;
                    FileTotalCount = FileTotalCount + 1;

                }

                if (Directory.Exists(Jetpowerfolder))
                {
                    long bSize = GetFileSizeSumFromDirectory(Jetpowerfolder);
                    long mbSize = ((bSize / 1024) / 1024);
                    Jetpower_MB = mbSize;
                    totalSize += mbSize;
                    FileTotalCount = FileTotalCount + 1;
                }

                if (Directory.Exists(Configfolder))
                {
                    long bSize = GetFileSizeSumFromDirectory(Configfolder);
                    long mbSize = ((bSize / 1024) / 1024);
                    Config_MB = mbSize;
                    totalSize += mbSize;
                    FileTotalCount = FileTotalCount + 1;
                }

                if (Directory.Exists(Toolsfolder))
                {
                    long bSize = GetFileSizeSumFromDirectory(Toolsfolder);
                    long mbSize = ((bSize / 1024) / 1024);
                    Tools_MB = mbSize;
                    totalSize += mbSize;
                    FileTotalCount = FileTotalCount + 1;
                }

                if (Directory.Exists(Data))
                {
                    long bSize = GetFileSizeSumFromDirectory(Data);
                    long ocvMB = 0;

                    if (Directory.Exists(ocvFolder))
                    {
                        long ocvSize = GetFileSizeSumFromDirectory(ocvFolder);
                        ocvMB = ((ocvSize / 1024) / 1024);
                    }

                    long mbSize = ((bSize / 1024) / 1024);
                    long TotalDataSize = mbSize - ocvMB;
                    Data_MB = TotalDataSize;
                    totalSize += TotalDataSize;
                    FileTotalCount = FileTotalCount + 1;

                }

                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                string timestamp = GetTimestamp(DateTime.Now);
                logTime = LogTime(DateTime.Now);
                dialog.FileName = "Win7Backup_" + timestamp;
                dialog.Filter = "All files (*.*)|*.*";

                int progressValue = 0;
                
                if (dialog.ShowDialog() == true)
                {
                    string path = dialog.FileName;

                    //if (File.Exists(regFolder + "\\log(backup).txt"))
                    //{
                    //    File.Delete(regFolder + "\\log(backup).txt");
                    //    File.Create(regFolder + "\\log(backup).txt").Dispose();
                        
                    //}

                    //else
                    //{
                    //    File.Create(regFolder + "\\log(backup).txt").Dispose();
                        
                    //}

                    //logPath = regFolder + "\\log(backup).txt";
                    //File.AppendAllText(logPath, "Start Log"+Environment.NewLine+"===================" + Environment.NewLine + Environment.NewLine);
                    


                    MainWin.IsEnabled = false;
                    //MainWin.IsHitTestVisible = false;
                    //messageForm.IsHitTestVisible = false;
                    //calling a form from main form
                    //File.AppendAllText(logPath, logTime + "Start Checking Disk Space.." + Environment.NewLine);

                    System.IO.DriveInfo di = new System.IO.DriveInfo(path);
                    
                    //File.AppendAllText(logPath, logTime + "End of Checking disk space" + Environment.NewLine);

                    long driveAvailableSpace = ((di.AvailableFreeSpace / 1024) / 1024); //mb
                    int IntdriveAvailableSpace = Convert.ToInt32(driveAvailableSpace);
                    long folderTotalSize = totalSize;
                    int IntfolderTotalSize = Convert.ToInt32(folderTotalSize);

                    
                    if (driveAvailableSpace >= folderTotalSize)
                    {
                        messageForm.Show();
                        MainWin.Visibility = System.Windows.Visibility.Hidden;
                        messageForm.totalFolderSize.Content = totalSize;

                        System.Windows.Forms.Application.DoEvents();

                        messageForm.LogBox.Text = "";
                        messageForm.LogBox.Text = "Start Export HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\MV Technology..";

                        ExportKey("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\MV Technology", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Exported_MVTechnology.reg");

                        messageForm.LogBox.Text = "";
                        messageForm.LogBox.Text = "End Export HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\MV Technology..";

                        //File.AppendAllText(logPath, logTime + "End Export Registry" + Environment.NewLine);

                        if (Directory.Exists(regFolder))
                        {
                            //File.AppendAllText(logPath, logTime + "Check Registry Existence" + Environment.NewLine);

                            if (File.Exists(regFolder + "\\Exported_MVTechnology.reg"))
                            {
                                Directory.CreateDirectory(path);
                                if (File.Exists(path + "\\Exported_MVTechnology.reg"))
                                {
                                    File.Delete(path + "\\Exported_MVTechnology.reg");
                                    File.Create(path + "\\Exported_MVTechnology.reg");
                                    File.Move(regFolder + "\\Exported_MVTechnology.reg", path + "\\Exported_MVTechnology.reg");
                                    summary = summary + Registry;

                                }
                                else
                                {
                                    File.Move(regFolder + "\\Exported_MVTechnology.reg", path + "\\Exported_MVTechnology.reg");
                                    summary = summary + Registry;
                                }
                            }
                            else
                            {
                                string noReg = "- Registry (Not Found)\n";
                                summary = summary + noReg;
                            }
                            //File.AppendAllText(logPath, logTime + "End Check Registry Existence" + Environment.NewLine);

                        }
                        
                            
                        
                        
                        messageForm.progress_bar_TotalFile.Maximum = folderTotalSize;
                        //File.AppendAllText(logPath, logTime + "Start Check V510 Existence" + Environment.NewLine);
                        if (Directory.Exists(V510folder))
                        {
                            System.Windows.Forms.Application.DoEvents();
                            //------------------------Kill Process--------------------------//
                            //Need to kill process or else cannot copy file
                            //File.AppendAllText(logPath, logTime + "Start Kill Process" + Environment.NewLine);
                            try
                            {


                                Process[] AOIEName = Process.GetProcessesByName("AOInEngine");
                                Process[] GUISoftware = Process.GetProcessesByName("V510GUI");
                                Process[] GUI = Process.GetProcessesByName("VGUI");
                                Process[] NegativeTester = Process.GetProcessesByName("Negative Tester");
                                Process[] engine = Process.GetProcessesByName("2DEngine");

                                if (AOIEName.Length == 0)
                                {
                                    aoiActive = 0;
                                }
                                else
                                {
                                    aoiActive = 1;
                                }

                                if (GUISoftware.Length == 0)
                                {
                                    gsActive = 0;
                                }
                                else
                                {
                                    gsActive = 1;
                                }

                                if (GUI.Length == 0)
                                {
                                    guiActive = 0;
                                }
                                else
                                {
                                    guiActive = 1;
                                }

                                if (NegativeTester.Length == 0)
                                {
                                    ntActive = 0;
                                }
                                else
                                {
                                    ntActive = 1;
                                }

                                if (engine.Length == 0)
                                {
                                    engineActive = 0;
                                }

                                else
                                {
                                    engineActive = 1;
                                }

                                //---------------Kill Automated Optical Inspection Engine---------------//
                                if (aoiActive == 1)
                                {
                                    foreach (System.Diagnostics.Process myProc in System.Diagnostics.Process.GetProcessesByName("AOInEngine"))
                                    {
                                        if (myProc.ProcessName == "AOInEngine")
                                        {

                                            myProc.Kill();

                                        }
                                    }
                                }
                                //---------------End of Kill Automated Optical Inspection Engine---------------//

                                //---------------Kill V510 Series GUI Software---------------//
                                if (gsActive == 1)
                                {
                                    foreach (System.Diagnostics.Process myProc1 in System.Diagnostics.Process.GetProcessesByName("V510GUI"))
                                    {
                                        if (myProc1.ProcessName == "V510GUI")
                                        {

                                            myProc1.Kill();

                                        }
                                    }
                                }
                                //---------------End of Kill V510 Series GUI Software---------------//

                                //---------------Kill VGUI---------------//
                                if (guiActive == 1)
                                {
                                    foreach (System.Diagnostics.Process myProc2 in System.Diagnostics.Process.GetProcessesByName("VGUI"))
                                    {
                                        if (myProc2.ProcessName == "VGUI")
                                        {

                                            myProc2.Kill();
                                        }
                                    }
                                }
                                //---------------End of Kill VGUI---------------//

                                //---------------Kill Negative Tester---------------//
                                if (ntActive == 1)
                                {
                                    foreach (System.Diagnostics.Process myProc3 in System.Diagnostics.Process.GetProcessesByName("Negative Tester"))
                                    {
                                        if (myProc3.ProcessName == "Negative Tester")
                                        {

                                            myProc3.Kill();
                                        }
                                    }
                                }
                                //---------------End of Kill Negative Tester---------------//

                                if (engineActive == 1)
                                {
                                    foreach (System.Diagnostics.Process myProc4 in System.Diagnostics.Process.GetProcessesByName("2DEngine"))
                                    {
                                        if (myProc4.ProcessName == "2DEngine")
                                        {

                                            myProc4.Kill();

                                        }
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                System.Windows.MessageBox.Show(ex.Message.ToString());
                            }

                            //File.AppendAllText(logPath, logTime + "End Kill Process" + Environment.NewLine);

                            //File.AppendAllText(logPath, logTime + "Start Back Up V510 Series" + Environment.NewLine);

                            if (!Directory.Exists(path + "\\V510 Series"))
                            {
                                Directory.CreateDirectory(path + "\\V510 Series");
                            }
                            TaskList.Add(CopyFolderContents(V510folder, path + "\\V510 Series"));

                            //File.AppendAllText(logPath, V510folder + Environment.NewLine);           
                            //File.AppendAllText(logPath, logTime + path + " - \\V510 Series" + Environment.NewLine);

                            
                            //File.AppendAllText(logPath, logTime + "End Back Up V510 Series" + Environment.NewLine);
                            summary += v510;
                           
                            
                            
                        }
                        else
                        {
                            string nov510 = "- V510 Series (Not Found in C:\\Program Files (x86)\\V510 Series)\n";
                            summary = summary + nov510;
                            
                        }
                        //File.AppendAllText(logPath, logTime + "End of Checking V510 Series" + Environment.NewLine);


                        //File.AppendAllText(logPath, logTime + "Start Checking Vitrox License Server Existence" + Environment.NewLine);

                        if (Directory.Exists(VitroxLicenseServerfolder))
                        {
                            System.Windows.Forms.Application.DoEvents();
                            //File.AppendAllText(logPath, logTime + "Start Kill Vitrox License Process" + Environment.NewLine);
                            try
                            {
                                int vlsActive = 0;
                                Process[] vlsName = Process.GetProcessesByName("ViTroxLicenseService");

                                if (vlsName.Length == 1)
                                {
                                    vlsActive = 1;
                                }

                                else
                                {
                                    vlsActive = 0;
                                }

                                if (vlsActive == 1)
                                {
                                    foreach (System.Diagnostics.Process VLS in System.Diagnostics.Process.GetProcessesByName("ViTroxLicenseService"))
                                    {
                                        if (VLS.ProcessName == "ViTroxLicenseService")
                                        {
                                            VLS.Kill();
                                        }
                                    }
                                }

                            }

                            catch (Exception ex)
                            {
                                System.Windows.MessageBox.Show(ex.Message.ToString());
                            }

                            //File.AppendAllText(logPath, logTime + "End of kill Vitrox License Server" + Environment.NewLine);

                            //File.AppendAllText(logPath, logTime + "Start of Copy Vitrox License Server" + Environment.NewLine);

                            Directory.CreateDirectory(path + "\\vitroxLicenseServer");
                            TaskList.Add(CopyFolderContents(VitroxLicenseServerfolder, path + "\\vitroxLicenseServer"));

                            summary += vitroxLicense;

                            //File.AppendAllText(logPath, logTime + "End of Copy Vitrox License Server" + Environment.NewLine);
                            
                            
                        }

                        else
                        {
                            string noVLS = "- vitroxLicenseServer (Not Found in C:\\Program Files (x86)\\VitroxLicenseServer)\n";
                            summary = summary + noVLS;
                        }

                        //File.AppendAllText(logPath, logTime + "End of Checking Vitrox License Server" + Environment.NewLine);

                        //File.AppendAllText(logPath, logTime + "Start of Checking PLC Folder" + Environment.NewLine);
                        if (Directory.Exists(PLCfolder))
                        {
                            //File.AppendAllText(logPath, logTime + "Start Copy PLC Folder" + Environment.NewLine);

                            System.Windows.Forms.Application.DoEvents();
                            Directory.CreateDirectory(path + "\\plc");
                            TaskList.Add(CopyFolderContents(PLCfolder, path + "\\plc"));
                            FileComplete = FileComplete + 1;

                            //File.AppendAllText(logPath, logTime + "End Copy PLC Folder" + Environment.NewLine);

                            summary += plc;
                            
                            
                        }
                        else
                        {
                            string noPLC = "- PLC (Not Found in C:\\cpi\\plc)\n";
                            summary = summary + noPLC;
                        }
                        //File.AppendAllText(logPath, logTime + "End of Checking PLC Folder" + Environment.NewLine);



                        //File.AppendAllText(logPath, logTime + "Start of Cheking Defect Packager Folder" + Environment.NewLine);
                        if (Directory.Exists(DefectPackagerfolder))
                        {
                            //File.AppendAllText(logPath, logTime + "Start of Copy Defect Packager" + Environment.NewLine);

                            System.Windows.Forms.Application.DoEvents();
                            Directory.CreateDirectory(path + "\\DefectPackager");
                            TaskList.Add(CopyFolderContents(DefectPackagerfolder, path + "\\DefectPackager"));
                            FileComplete = FileComplete + 1;

                            //File.AppendAllText(logPath, logTime + "End of Copy Defect packager" + Environment.NewLine);

                            summary += defect;
                        }
                        else
                        {
                            string noDefect = "- Defect Packager (Not Found in C:\\Program Files\\DefectPackager)\n";
                            summary = summary + noDefect;
                        }
                        //File.AppendAllText(logPath, logTime + "End of Checking Defect Packager Folder" + Environment.NewLine);



                        //File.AppendAllText(logPath, logTime + "Start of Checking Jetpower Folder" + Environment.NewLine);
                        if (Directory.Exists(Jetpowerfolder))
                        {
                            System.Windows.Forms.Application.DoEvents();
                            //File.AppendAllText(logPath, logTime + "Start of Copy Jetpower" + Environment.NewLine);

                            Directory.CreateDirectory(path + "\\Jetpower");
                            TaskList.Add(CopyFolderContents(Jetpowerfolder, path + "\\Jetpower"));

                            //File.AppendAllText(logPath, logTime + "End of Copy Jetpower" + Environment.NewLine);
                            summary += jetpower;

                            
                        }

                        else
                        {
                            string noJet = "- Jetpower (Not Found in C:\\CPI\\jetpower)\n";
                            summary = summary + noJet;
                        }
                        //File.AppendAllText(logPath, logTime + "End of Checking Jetpower Folder" + Environment.NewLine);


                        //File.AppendAllText(logPath, logTime + "Start of Checking Config Folder" + Environment.NewLine);
                        if (Directory.Exists(Configfolder))
                        {
                            //File.AppendAllText(logPath, logTime + "Start of Copy Config Folder" + Environment.NewLine);

                            System.Windows.Forms.Application.DoEvents();
                            Directory.CreateDirectory(path + "\\Config");
                            TaskList.Add(CopyFolderContents(Configfolder, path + "\\Config"));

                            //File.AppendAllText(logPath, logTime + "End of Copy Config Folder" + Environment.NewLine);

                            summary += config;
                            
                        }

                        else
                        {
                            string noConfig = "- Config (Not Found in C:\\CPI\\config)\n";
                            summary = summary + noConfig;
                        }
                        //File.AppendAllText(logPath, logTime + "End of Checking Config Folder" + Environment.NewLine);



                        //File.AppendAllText(logPath, logTime + "Start of Checking Tools Folder" + Environment.NewLine);
                        if (Directory.Exists(Toolsfolder))
                        {
                            //File.AppendAllText(logPath, logTime + "Start of Copy Tools Folder" + Environment.NewLine);
                            
                            Directory.CreateDirectory(path + "\\Tools");
                            //Task.Factory.StartNew(() => CopyFolderContents(Toolsfolder, path + "\\Tools"));
                            //CopyFolderContents(Toolsfolder, path + "\\Tools");
                            TaskList.Add(CopyFolderContents(Toolsfolder, path + "\\Tools"));

                            //File.AppendAllText(logPath, logTime + "End of Copy Tools Folder" + Environment.NewLine);

                            summary += tools;
                            
                        }

                        else
                        {
                            string noTools = "- Tools (Not Found in C:\\CPI\\tools)\n";
                            summary = summary + noTools;
                        }
                        //File.AppendAllText(logPath, logTime + "End of Checking Tools Folder" + Environment.NewLine);


                        //File.AppendAllText(logPath, logTime + "Start of Check Data Folder" + Environment.NewLine);
                        
                        if (Directory.Exists(Data))
                        {
                            if (Directory.Exists(Data + "\\OCV"))
                            {
                                //File.AppendAllText(logPath, logTime + "Checking OCV Confirmation" + Environment.NewLine);
                                if (OCV == true)
                                {
                                    //File.AppendAllText(logPath, logTime + "Start Copy Data Folder with OCV" + Environment.NewLine);

                                    Directory.CreateDirectory(path + "\\Data");
                                    //File.AppendAllText(logPath, "1/ " + path + Environment.NewLine);
                                    //File.AppendAllText(logPath, Data + Environment.NewLine);

                                    Directory.CreateDirectory(path + "\\Data");

                                    TaskList.Add(CopyFolderContents(Data, path + "\\Data"));

                                    //File.AppendAllText(logPath, logTime + "End of Copy Data Folder with OCV" + Environment.NewLine);

                                    summary += ocv;
                                    summary += data;
                                }

                                else
                                {
                                    //File.AppendAllText(logPath, logTime + "Start Copy Data Folder without OCV" + Environment.NewLine);

                                    Directory.CreateDirectory(path + "\\Data");
                                    //Task.Factory.StartNew(() => CopyFolderContents(Data, path + "\\Data"));
                                    TaskList.Add(CopyFolderContents(Data, path + "\\Data"));

                                    //File.AppendAllText(logPath, logTime + "End Copy Data Folder without OCV" + Environment.NewLine);
                                    summary += data;

                                    
                                }
                                //File.AppendAllText(logPath, logTime + "End of Check OCV Confirmation" + Environment.NewLine);
                            }

                            else
                            {
                                //File.AppendAllText(logPath, logTime + "Start Copy Data Folder without OCV" + Environment.NewLine);
                                Directory.CreateDirectory(path + "\\Data");
                                TaskList.Add(CopyFolderContents(Data, path + "\\Data"));

                                summary += data;
                                //File.AppendAllText(logPath, logTime + "End Copy Data Folder without OCV" + Environment.NewLine);
                            }

                        }

                        else
                        {
                            string noData = "- Data (Not Found in C:\\CPI\\data)\n";
                            summary = summary + noData;
                        }
                        //File.AppendAllText(logPath, logTime + "End of Check Data Folder" + Environment.NewLine);


                        if (CAD == true)
                        {
                            //File.AppendAllText(logPath, logTime + "Start of Check CAD Folder" + Environment.NewLine);
                            if (Directory.Exists(CADfolder))
                            {
                                //File.AppendAllText(logPath, logTime + "Start of Copy CAD Folder" + Environment.NewLine);

                                Directory.CreateDirectory(path + "\\CAD");
                                TaskList.Add(CopyFolderContents(CADfolder, path + "\\CAD"));

                                //File.AppendAllText(logPath, logTime + "End of Copy CAD Folder" + Environment.NewLine);
                                summary += cad;
                                
                            }
                            //File.AppendAllText(logPath, logTime + "End of Checking CAD Folder" + Environment.NewLine);

                        }


                        
                        if (GoodImage == true)
                        {
                            //File.AppendAllText(logPath, logTime + "Start Check Good Image Folder" + Environment.NewLine);
                            if (Directory.Exists(GoodImagefolder))
                            {
                                //File.AppendAllText(logPath, logTime + "Start Copy Good Image Folder" + Environment.NewLine);

                                //File.AppendAllText(logPath, path + Environment.NewLine);
                                Directory.CreateDirectory(path + "\\GoodImages");

                                //File.AppendAllText(logPath, "Path: " + path + Environment.NewLine);
                                TaskList.Add(CopyFolderContents(GoodImagefolder, path + "\\GoodImages"));
                                //File.AppendAllText(logPath, logTime + "End Copy Good Image Folder" + Environment.NewLine);

                                summary += goodImage;
                            }
                            //File.AppendAllText(logPath, logTime + "End Check Good Image Folder" + Environment.NewLine);
                        }


                        
                        //File.AppendAllText(logPath, logTime + "Start Check Fringe Folder" + Environment.NewLine);
                        if (Fringe == true)
                        {
                            if (Directory.Exists(fringeFolder))
                            {
                                //File.AppendAllText(logPath, logTime + "Start Copy Fringe" + Environment.NewLine);

                                Directory.CreateDirectory(path + "\\fringe");

                                TaskList.Add(CopyFolderContents(fringeFolder, path + "\\fringe"));

                                summary += fringe;
                                
                            }

                            //File.AppendAllText(logPath, logTime + "End of Check Fringe Folder" + Environment.NewLine);
                        }
                        Task.WaitAll(TaskList.ToArray());
                        messageForm.Visibility = System.Windows.Visibility.Hidden;
                        MainWin.Visibility = System.Windows.Visibility.Visible;
                        System.Windows.MessageBox.Show(summary, "Summary");

                        MainWin.IsEnabled = true;



                    }

                    else
                    {
                        //long availableSpace = totalSize - di.AvailableFreeSpace;
                        //long totalMBSize = (((availableSpace / 1024) / 1024));
                        int neededSpace = IntfolderTotalSize - IntdriveAvailableSpace ;
                        System.Windows.MessageBox.Show("Back up process FAILED! Need more " + neededSpace + "MB to back up.");
                        MainWin.IsEnabled = true;

                        if (win7)
                        {
                            browseBackup.IsEnabled = true;
                            restoreBtn.IsEnabled = false;
                        }
                        else
                        {
                            browseBackup.IsEnabled = false;
                            restoreBtn.IsEnabled = true;
                        }
                  
                    }

                }



            }

        }

        private void win10_restore(Object sender, RoutedEventArgs e)
        {
            contentSize = 0;
            messageForm.TotalCompletedSize.Content = "0";
            messageForm.progress_bar_TotalFile.Value = 0;
            //------------------------Folder Path-----------------------------//
            string V510folder = "C:\\Program Files (x86)\\V510 Series";
            string VitroxLicenseServerfolder = "C:\\Program Files (x86)\\VitroxLicenseServer";
            string PLCfolder = "C:\\cpi\\plc";
            string DefectPackagerfolder = @"C:\Program Files\DefectPackager";
            string Jetpowerfolder = @"C:\CPI\jetpower";
            string Configfolder = @"C:\CPI\config";
            string Toolsfolder = @"C:\CPI\tools";
            string Datafolder = @"C:\CPI\data";
            string CADfolder = @"C:\CPI\cad";
            string GoodImagefolder = @"C:\Goodimages";
            string fringeFolder = @"C:\CPI\fringe";
            string regFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //------------------------Folder Path-----------------------------//

            string logTime = LogTime(DateTime.Now);

            //-------------------------Message Shown-------------------------//
            string summary = "Restore completed\nRestore status stated below\n\nRestore Status\n======================\n";

            string Registry = "- Registry (Completed)\n";
            string v510 = "- V510 Series (Completed)\n";
            string vitroxLicense = "- VitroxLicenseServer (Completed)\n";
            string plc = "- PLC (Completed)\n";
            string defect = "- Defect Packager (Completed)\n";
            string jetpower = "- Jetpower (Completed)\n";
            string config = "- Config (Completed)\n";
            string tools = "- Tools (Completed)\n";
            string data = "- Data (Completed)\n";
            string OCV = "- OCV (Completed)\n";
            string cad = "- Cad (Completed)\n";
            string goodImage = "- Good Images (Completed)\n";
            string fringe = "- Fringe (Completed)\n";
            //-------------------------Message Shown-------------------------//

            long cad_MB = 0;
            long gImg_MB = 0;
            long fringe_MB = 0;
            long v510_MB = 0;
            long VLS_MB = 0;
            long PLC_MB = 0;
            long DefectPackager_MB = 0;
            long Jetpower_MB = 0;
            long Config_MB = 0;
            long Tools_MB = 0;
            long Data_MB = 0;

            
            long totalSize = 0;

            if (win10 == true)
            {
                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Do you want to restore from the back up file?", "Confirmation", MessageBoxButtons.YesNo);

                if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    restore = true;
                }
                if (dialogResult == System.Windows.Forms.DialogResult.No)
                {
                    restore = false;
                }

                //if (File.Exists(regFolder + "\\log(restore).txt"))
                //{
                //    File.Delete(regFolder + "\\log(restore).txt");
                //    File.Create(regFolder + "\\log(restore).txt").Dispose();

                //}

                //else
                //{
                //    File.Create(regFolder + "\\log(restore).txt").Dispose();

                //}

                //string logPath = regFolder + "\\log(restore).txt";
                //File.AppendAllText(logPath, logTime+ "Start Restore Log" + Environment.NewLine + "===================" + Environment.NewLine + Environment.NewLine);

                if (restore == true)
                {
                    System.Windows.Forms.Application.DoEvents();
                    FolderBrowserDialog browseFolder = new FolderBrowserDialog();
                    browseFolder.Description = "Choose PARENT Folder to continue restore";
                    
                    if (browseFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        MainWin.Visibility = System.Windows.Visibility.Hidden;
                        string path = Path.GetFullPath(browseFolder.SelectedPath);
                        string timestamp = GetTimestamp(DateTime.Now);

                        System.IO.DirectoryInfo V510_info = new System.IO.DirectoryInfo(path + "\\V510 Series");
                        System.IO.DirectoryInfo vitroxLicense_info = new System.IO.DirectoryInfo(path + "\\vitroxLicenseServer");
                        System.IO.DirectoryInfo plc_info = new System.IO.DirectoryInfo(path + "\\PLC");
                        System.IO.DirectoryInfo defectPackage_info = new System.IO.DirectoryInfo(path + "\\DefectPackager");
                        System.IO.DirectoryInfo jetPower_info = new System.IO.DirectoryInfo(path+ "\\jetpower");
                        System.IO.DirectoryInfo config_info = new System.IO.DirectoryInfo(path + "\\config");
                        System.IO.DirectoryInfo tools_info = new System.IO.DirectoryInfo(path + "\\tools");
                        System.IO.DirectoryInfo data_info = new System.IO.DirectoryInfo(path + "\\data");
                        System.IO.DirectoryInfo ocv_info = new System.IO.DirectoryInfo(path+"\\data\\ocv");
                        System.IO.DirectoryInfo cad_info = new System.IO.DirectoryInfo(path+"\\cad");
                        System.IO.DirectoryInfo goodImage_info = new System.IO.DirectoryInfo(path + "\\Goodimages");
                        System.IO.DirectoryInfo fringe_info = new System.IO.DirectoryInfo(path +"\\fringe");

                        if (Directory.Exists(path + "\\V510 Series"))
                        {
                            long bSize = GetFileSizeSumFromDirectory(path + "\\V510 Series");
                            long mbSize = ((bSize / 1024) / 1024);
                            v510_MB = mbSize;
                            totalSize += mbSize;
                        }

                        if (Directory.Exists(path + "\\vitroxLicenseServer"))
                        {
                            long bSize = GetFileSizeSumFromDirectory(path + "\\vitroxLicenseServer");
                            long mbSize = ((bSize / 1024) / 1024);
                            VLS_MB = mbSize;
                            totalSize += mbSize;
                        }

                        if (Directory.Exists(path + "\\PLC"))
                        {
                            long bSize = GetFileSizeSumFromDirectory(path + "\\PLC");
                            long mbSize = ((bSize / 1024) / 1024);
                            PLC_MB = mbSize;
                            totalSize += mbSize;
                        }

                        if (Directory.Exists(path + "\\DefectPackager"))
                        {
                            long bSize = GetFileSizeSumFromDirectory(path + "\\DefectPackager");
                            long mbSize = ((bSize / 1024) / 1024);
                            DefectPackager_MB = mbSize;
                            totalSize += mbSize;
                        }

                        if (Directory.Exists(path + "\\jetpower"))
                        {
                            long bSize = GetFileSizeSumFromDirectory(path + "\\jetpower");
                            long mbSize = ((bSize / 1024) / 1024);
                            Jetpower_MB = mbSize;
                            totalSize += mbSize;
                        }

                        if (Directory.Exists(path + "\\config"))
                        {
                            long bSize = GetFileSizeSumFromDirectory(path + "\\config");
                            long mbSize = ((bSize / 1024) / 1024);
                            Config_MB = mbSize;
                            totalSize += mbSize;
                        }

                        if (Directory.Exists(path + "\\tools"))
                        {
                            long bSize = GetFileSizeSumFromDirectory(path + "\\tools");
                            long mbSize = ((bSize / 1024) / 1024);
                            Tools_MB = mbSize;
                            totalSize += mbSize;
                        }

                        if (Directory.Exists(path + "\\data"))
                        {
                            long bSize = GetFileSizeSumFromDirectory(path + "\\data");
                            long mbSize = ((bSize / 1024) / 1024);
                            Data_MB = mbSize;
                            totalSize += mbSize;
                        }

                        if (Directory.Exists(path + "\\cad"))
                        {
                            long bSize = GetFileSizeSumFromDirectory(path + "\\cad");
                            long mbSize = ((bSize / 1024) / 1024);
                            cad_MB = mbSize;
                             totalSize += mbSize;
                        }

                        if (Directory.Exists(path + "\\Goodimages"))
                        {
                            long bSize = GetFileSizeSumFromDirectory(path + "\\Goodimages");
                            long mbSize = ((bSize / 1024) / 1024);
                            gImg_MB = mbSize;
                             totalSize += mbSize;
                        }

                        if (Directory.Exists(path + "\\fringe"))
                        {
                            long bSize = GetFileSizeSumFromDirectory(path + "\\fringe");
                            long mbSize = ((bSize / 1024) / 1024);
                            fringe_MB = mbSize;
                            totalSize += mbSize;
                        }

                        MainWin.IsEnabled = false;
                        messageForm.totalFolderSize.Content = totalSize;
                        messageForm.progress_bar_TotalFile.Maximum = totalSize;

                        messageForm.Show();

                        //File.AppendAllText(logPath, logTime + "Start Check Path Selected Existence" + Environment.NewLine);
                        if (Directory.Exists(path))
                        {
                            string keyname = "\\Windows10_backup"+timestamp+".reg";

                            //File.AppendAllText(logPath, logTime + "Start Check Registry" + Environment.NewLine);

                            if (File.Exists(path + "\\Exported_MVTechnology.reg"))
                            {
                                //File.AppendAllText(logPath, logTime + "Start Import Registry" + Environment.NewLine);
                                if (File.Exists(path+keyname))
                                {
                                    File.Delete(path + keyname);
                                    ExportKey("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\MV Technology", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Exported_MVTechnology.reg");
                                    File.Move(regFolder + "\\Exported_MVTechnology.reg", path + keyname);
                                }

                                else
                                {
                                    ExportKey("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\MV Technology", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Exported_MVTechnology.reg");
                                    File.Move(regFolder + "\\Exported_MVTechnology.reg", path + keyname);
                                    
                                }

                                Process regeditProcess = Process.Start("regedit.exe", path + "\\Exported_MVTechnology.reg");
                                regeditProcess.WaitForExit();

                                //File.AppendAllText(logPath, logTime + "End Import Registry" + Environment.NewLine);
                                summary += Registry;
                            }

                            else
                            {
                                string noReg = "- Registry (Not Found)\n";
                                summary = summary + noReg;
                            }

                            //File.AppendAllText(logPath, logTime + "End Check Registry" + Environment.NewLine);


                            //File.AppendAllText(logPath, logTime + "Check V510 Series" + Environment.NewLine);
                            if (Directory.Exists(path + "\\V510 Series"))
                            {
                                //File.AppendAllText(logPath, logTime + "Start Copy V510 Series" + Environment.NewLine);

                                TaskList.Add(RestoreFolderContent(path + "\\V510 Series", V510folder));
                                //File.AppendAllText(logPath, logTime + "End Copy V510 Series" + Environment.NewLine);

                                summary += v510;
                            }
                            else
                            {
                                string nov510 = "- V510 Series (Not Found in " + path + "\\V510 Series)\n";
                                summary = summary + nov510;
                            }
                            //File.AppendAllText(logPath, logTime + "End Check V510 Registry" + Environment.NewLine);


                            //File.AppendAllText(logPath, logTime + "Start Check Vitrox License Server" + Environment.NewLine);

                            //File.AppendAllText(logPath, path + "\\VitroxLicenseServer" + Environment.NewLine);
                            if (Directory.Exists(path + "\\VitroxLicenseServer"))
                            {
                                System.Windows.Forms.Application.DoEvents();
                                //File.AppendAllText(logPath, logTime + "Start Kill Vitrox License Process" + Environment.NewLine);
                                try
                                {
                                    int vlsActive = 0;
                                    Process[] vlsName = Process.GetProcessesByName("ViTroxLicenseService");

                                    if (vlsName.Length == 1)
                                    {
                                        vlsActive = 1;
                                    }

                                    else
                                    {
                                        vlsActive = 0;
                                    }

                                    if (vlsActive == 1)
                                    {
                                        foreach (System.Diagnostics.Process VLS in System.Diagnostics.Process.GetProcessesByName("ViTroxLicenseService"))
                                        {
                                            if (VLS.ProcessName == "ViTroxLicenseService")
                                            {
                                                VLS.Kill();
                                            }
                                        }
                                    }

                                }

                                catch (Exception ex)
                                {
                                    System.Windows.MessageBox.Show(ex.Message.ToString());
                                }

                                //File.AppendAllText(logPath, logTime + "End of kill Vitrox License Server" + Environment.NewLine);

                                //File.AppendAllText(logPath, logTime + "Start Copy Vitrox License Server" + Environment.NewLine);

                                //File.AppendAllText(logPath, VitroxLicenseServerfolder + Environment.NewLine);
                                Directory.CreateDirectory(VitroxLicenseServerfolder);
                                //File.AppendAllText(logPath, path + "\\VitroxLicenseServer" + Environment.NewLine);
                                TaskList.Add(CopyFolderContents(path + "\\VitroxLicenseServer", VitroxLicenseServerfolder));
                                //RestoreFolderContent(path + "\\VitroxLicenseServer", VitroxLicenseServerfolder);

                                //File.AppendAllText(logPath, logTime + "End Copy Vitrox License Server" + Environment.NewLine);
                                summary += vitroxLicense;
                            }
                            else
                            {
                                //File.AppendAllText(logPath, "- Vitrox License Server (Not Found in)" + Environment.NewLine);
                                string noVLS = "- Vitrox License Server (Not Found in "+ path + "\\VitroxLicenseServer)\n";
                                summary = summary + noVLS;

                            }
                            //File.AppendAllText(logPath, logTime + "End Check Vitrox License Server" + Environment.NewLine);

                            //File.AppendAllText(logPath, logTime + "Start Check PLC Folder" + Environment.NewLine);
                            if (Directory.Exists(path + "\\plc"))
                            {
                                System.Windows.Forms.Application.DoEvents();
                                //File.AppendAllText(logPath, logTime + "Start Copy PLC Folder" + Environment.NewLine);

                                Directory.CreateDirectory(PLCfolder);
                                TaskList.Add(RestoreFolderContent(path + "\\plc", PLCfolder));

                                //File.AppendAllText(logPath, logTime + "End Copy PLC Folder" + Environment.NewLine);
                                summary += plc;
                            }
                            else
                            {
                                string noPLC = "- PLC (Not Found in "+ path + "\\plc\n";
                                summary = summary + noPLC;
                            }
                            //File.AppendAllText(logPath, logTime + "End Check PLC Folder" + Environment.NewLine);

                            //File.AppendAllText(logPath, logTime + "Start Check Defect Packager" + Environment.NewLine);
                            if (Directory.Exists(path + "\\DefectPackager"))
                            {
                                System.Windows.Forms.Application.DoEvents();
                                //File.AppendAllText(logPath, logTime + "Start Copy Defect Packager" + Environment.NewLine);

                                Directory.CreateDirectory(DefectPackagerfolder);
                                TaskList.Add(RestoreFolderContent(path + "\\DefectPackager", DefectPackagerfolder));

                                //File.AppendAllText(logPath, logTime + "End Copy Defect packager" + Environment.NewLine);
                                summary += defect;
                            }
                            else
                            {
                                string noDefect = "- Defect Packager (Not Found in "+ path + "\\DefectPackager)\n";
                                summary = summary + noDefect;
                            }
                            //File.AppendAllText(logPath, logTime + "End Check Defect packager" + Environment.NewLine);

                            //File.AppendAllText(logPath, logTime + "Start Check Jetpower" + Environment.NewLine);
                            if (Directory.Exists(path + "\\jetpower"))
                            {
                                System.Windows.Forms.Application.DoEvents();
                                //File.AppendAllText(logPath, logTime + "Start Copy Jetpower" + Environment.NewLine);

                                Directory.CreateDirectory(Jetpowerfolder);
                                TaskList.Add(RestoreFolderContent(path + "\\jetpower", Jetpowerfolder));

                                //File.AppendAllText(logPath, logTime + "End Copy Jetpower" + Environment.NewLine);
                                summary += jetpower;
                            }
                            else
                            {
                                string noJet = "- Jetpower (Not Found in "+ path + "\\jetpower)\n";
                                summary = summary + noJet;
                            }
                            //File.AppendAllText(logPath, logTime + "End Check Jetpower" + Environment.NewLine);

                            //File.AppendAllText(logPath, logTime + "Start Check Config Folder" + Environment.NewLine);
                            if (Directory.Exists(path + "\\config"))
                            {
                                System.Windows.Forms.Application.DoEvents();
                                //File.AppendAllText(logPath, logTime + "Start Copy Config Folder" + Environment.NewLine);

                                Directory.CreateDirectory(Configfolder);
                                TaskList.Add(RestoreFolderContent(path + "\\config", Configfolder));

                                //File.AppendAllText(logPath, logTime + "End Copy Config Folder" + Environment.NewLine);
                                summary += config;
                            }
                            else
                            {
                                string noConfig = "- Config (Not Found in "+path + "\\config)\n";
                                summary = summary + noConfig;
                            }
                            //File.AppendAllText(logPath, logTime + "End Check Config Folder" + Environment.NewLine);

                            //File.AppendAllText(logPath, logTime + "Start Check Tools Folder" + Environment.NewLine);
                            if (Directory.Exists(path + "\\tools"))
                            {
                                System.Windows.Forms.Application.DoEvents();
                                //File.AppendAllText(logPath, logTime + "Start Copy Tools Folder" + Environment.NewLine);

                                Directory.CreateDirectory(Toolsfolder);
                                TaskList.Add(RestoreFolderContent(path + "\\tools", Toolsfolder));

                                //File.AppendAllText(logPath, logTime + "End Copy Tools Folder" + Environment.NewLine);
                                summary += tools;
                            }
                            else
                            {
                                string noTools = "- Tools (Not Found in "+ path + "\\tools)\n";
                                summary = summary + noTools;
                            }
                            //File.AppendAllText(logPath, logTime + "End Check Tools Folder" + Environment.NewLine);

                            //File.AppendAllText(logPath, logTime + "Start Check Data Folder" + Environment.NewLine);
                            if (Directory.Exists(path + "\\data"))
                            {
                                if(Directory.Exists(path+"\\data\\OCV"))
                                {
                                    System.Windows.Forms.Application.DoEvents();

                                    TaskList.Add(RestoreFolderContent(path + "\\data", Datafolder));

                                    summary += OCV;
                                    summary += data;
                                }

                                else
                                {
                                    System.Windows.Forms.Application.DoEvents();

                                    TaskList.Add(RestoreFolderContent(path + "\\data", Datafolder));
                                    string noOCV = "- OCV (Not Found in " + path + "\\data\\OCV)\n";
                                    summary += noOCV;
                                    summary += data;
                                }
                                
                            }
                            else
                            {
                                string noData = "- Data (Not Found in "+ path + "\\data)\n";
                                string noOCV = "- OCV (Not Found in " + path + "\\data\\OCV)\n";
                                
                                summary = summary + noData + noOCV;
                            }
                            //File.AppendAllText(logPath, logTime + "End Check Data Folder" + Environment.NewLine);

                            //File.AppendAllText(logPath, logTime + "Start Check CAD Folder" + Environment.NewLine);
                            if (Directory.Exists(path + "\\cad"))
                            {
                                System.Windows.Forms.Application.DoEvents();

                                //File.AppendAllText(logPath, logTime + "Start Copy CAD Folder" + Environment.NewLine);
                                //Task.Run(() => RestoreFolderContent(path + "\\cad", CADfolder));
                                TaskList.Add(RestoreFolderContent(path + "\\cad", CADfolder));

                                summary += cad;
                                //File.AppendAllText(logPath, logTime + "End Copy CAD Folder" + Environment.NewLine);
                            }
                            else
                            {
                                string noCAD = "- CAD (Not Found in "+ path + "\\cad)\n";
                                summary = summary + noCAD;
                            }
                            //File.AppendAllText(logPath, logTime + "End Check CAD Folder" + Environment.NewLine);

                            //File.AppendAllText(logPath, logTime + "Start Check Good Image Folder" + Environment.NewLine);

                            //System.Windows.MessageBox.Show("Start Check Good Image Folder");
                            if (Directory.Exists(path + "\\GoodImages"))
                            {
                                System.Windows.Forms.Application.DoEvents();
                                //File.AppendAllText(logPath, logTime + "Start Copy Good Image Folder" + Environment.NewLine);

                                TaskList.Add(RestoreFolderContent(path + "\\GoodImages", GoodImagefolder));

                                //File.AppendAllText(logPath, logTime + "End Copy Good Image Folder" + Environment.NewLine);

                                summary += goodImage;
                            }

                            else
                            {
                                string noGoodImage = "- Good Images (Not Found in " + path + "\\GoodImages)\n";
                                summary = summary + noGoodImage;
                            }
                            //File.AppendAllText(logPath, logTime + "End Check GoodImages Folder" + Environment.NewLine);
                            

                            //File.AppendAllText(logPath, logTime + "Start Check Fringe Folder" + Environment.NewLine);
                            if (Directory.Exists(path + "\\fringe"))
                            {
                                System.Windows.Forms.Application.DoEvents();
                                //File.AppendAllText(logPath, logTime + "Start Copy Fringe Folder" + Environment.NewLine);
                                
                                Directory.CreateDirectory(fringeFolder);
                                TaskList.Add(RestoreFolderContent(path + "\\fringe", fringeFolder));
                                

                                //File.AppendAllText(logPath, logTime + "End Copy Fringe Folder" + Environment.NewLine);
                                summary += fringe;
                            }
                            else
                            {
                                string noFringe = "- Fringe (Not Found in "+ path + "\\fringe)\n";
                                summary = summary + noFringe;
                            }
                            //File.AppendAllText(logPath, logTime + "End Check Fringe Folder" + Environment.NewLine);

                            Task.WaitAll(TaskList.ToArray());
                            messageForm.Visibility = System.Windows.Visibility.Hidden;
                            MainWin.Visibility = System.Windows.Visibility.Visible;
                            System.Windows.MessageBox.Show(summary, "Summary");

                            MainWin.IsEnabled = true;
                        }
                        else
                        {
                            messageForm.Visibility = System.Windows.Visibility.Hidden;
                            MainWin.Visibility = System.Windows.Visibility.Visible;
                            System.Windows.MessageBox.Show("No back up folder found");

                            MainWin.IsEnabled = true;
                            
                        }
                        

                    }
                }

            }
        }


    }
}
