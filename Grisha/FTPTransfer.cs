using MetroFramework.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace VCC
{
    public static class FTPTransfer
    {
        private static Queue queue;

        private static string ftpUser;
        private static string ftpPassword;
        private static int ftpPort;
        private static string ftpServer;

        private static bool offlineMode = false;
        private static Thread uploader;
        private static MetroUserControl progress;
        private static volatile bool isRunning = false;
        private static CancellationTokenSource cts;
        private static CancellationToken token;

        public static void Config(string username,string password,string server,int port, 
            MetroUserControl progressBar, bool offline = false)
        {
            queue = Functions.destringifyQueue(Properties.Settings.Default.FTPQueue);

            progress = progressBar;
            offlineMode = offline;
            ftpPassword = password;
            ftpPort = port;
            ftpUser = username;
            ftpServer = server;
            
            if (!offline) {
                if (uploader == null || (uploader.ThreadState == ThreadState.Stopped))
                { 
                    uploader = new Thread(upload);
                    isRunning = true;
                    uploader.Start();
                }
            }
        }
        private static FtpWebRequest getRequestObj(string filePath)
        {
            string ftpAddress = "ftp://" + ftpServer + "/";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(Path.Combine(ftpAddress, filePath)));
            request.Credentials = new NetworkCredential(ftpUser, ftpPassword);
            request.Timeout = -1;
            //request.Timeout = 5000;
            request.KeepAlive = true;
            request.UseBinary = true;
            request.UsePassive = true;
            return request;

        }
        public static bool getStatus()
        {
            if(uploader != null)
                return (uploader.ThreadState == ThreadState.Stopped) && isRunning;
            return false;
        }
        public static void close()
        {
            if (uploader != null && !(uploader.ThreadState == ThreadState.Stopped)) {
                isRunning = false;
                
                //uploader.Abort();
                cts.Cancel();
                
                //uploader.Join();
                //cts.Dispose();
            }
        }
        internal static void clearQueue()
        {
            if(queue != null)
                queue.Clear();
            Properties.Settings.Default.FTPQueue = ";";
            Properties.Settings.Default.Save();
            close();
        }
        public static void add(string element)
        {
            if (queue.Contains(element)) throw new Exception("file already in queue");
            queue.Enqueue(element);
            Properties.Settings.Default.FTPQueue += element + ";";
            Properties.Settings.Default.Save();
            string fName = Path.GetFileName(element);
              
            if (progress.InvokeRequired)
            {
                progress.BeginInvoke((MethodInvoker)delegate{
                    progress.Controls.Add(Form1.createProgress(fName,progress.Controls.Count));
                });
            }
            else
            {
                progress.Controls.Add(Form1.createProgress(fName, progress.Controls.Count));
            }
            
            Logger.add("FTP", "File was added to queue: " + fName);
            if (!isRunning && !offlineMode && uploader.ThreadState == ThreadState.Stopped)
            {
                uploader = new Thread(upload);
                isRunning = true;
                uploader.Start();
            }
            //ThreadPool.QueueUserWorkItem(upload);
        }

        public static void check(string username, string password, string server, int port,MetroProgressSpinner spinner)
        {
           
            try {
                spinner.Invoke((MethodInvoker)delegate { spinner.Visible = true; });

                string ftpAddress = "ftp://" + server + ":" + port + "/";
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpAddress));
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(username, password);
                request.Timeout = 5000;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                if (response.StatusCode.ToString() == "OpeningData" || response.StatusCode.ToString() == "DataAlreadyOpen")
                {
                    Console.WriteLine("asd");
                    MessageBox.Show("Good!", "FTP connection", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    FTPTransfer.Config(username, password, server, port,progress);
                    return;
                }
                throw new Exception(response.StatusCode.ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,"FTP connection failed",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            finally
            {
                spinner.Invoke((MethodInvoker)delegate { spinner.Visible = false; });
            }

        }
        private static Progress findControl(string name)
        {
            foreach (Progress prog in progress.Controls)
            {
                if (prog.getName().Equals(name))
                {
                    return prog;
                }
            }
            return null;
        }
        private static void updateProgress(string name, int num,string speed)
        {
            Progress p = findControl(name);
            
            if (progress.InvokeRequired)
            {
                progress.BeginInvoke((MethodInvoker)delegate
                {
                    p.setProgress(num);
                    p.setSpeed(speed);
                });
            }
            else
            {
                p.setProgress(num); 
            }
            
        }
        private static void cancelProgress(string name)
        {
            Progress p = findControl(name);
            if(p != null)
            if (progress.InvokeRequired)
            {
                progress.BeginInvoke((MethodInvoker)delegate
                {
                    p.cancel();
                });
            }
            else
            {
                p.cancel();
            }
        }
        private static long lastPosition = 0;
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            myTimer t = ((myTimer)source);
            bool open = t.stream.CanRead;
            double speed;
            string speedString;
            long complete;
            if (open)
            {
                speed = ((double)(t.stream.Position - lastPosition) / 2048);
                speedString = (speed > 1000) ? Convert.ToString(Math.Round(speed / 1024, 2)) + "MB/s" : Convert.ToString(speed) + "KB/s";
                complete = t.stream.Position * 100 / t.fileSize;
                lastPosition = t.stream.Position;
            }
            else
            {
                speedString = "lost Connection";
                complete = lastPosition * 100 / t.fileSize;
            }

            updateProgress(t.fileName, Convert.ToInt32(complete),speedString);
            
            
        }
        public static void createDir(string fullFilePath)
        {
            Logger.add("FTP", "creating folders on ftp");
            string[] dirList = fullFilePath.Split('\\');
            string fullpath = "";
            foreach (string dir in dirList) { 
                fullpath = Path.Combine(fullpath, dir);
                FtpWebRequest request = getRequestObj(fullpath);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                try
                {
                    using (var resp = (FtpWebResponse)request.GetResponse())
                    {
                        Console.WriteLine(resp.StatusCode);
                    }
                    Logger.add("DIR", dir + " was created.");
                }
                catch (WebException ex)
                {
                    // dir already exists.
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        response.Close();
                        throw new Exception(ex.Message);
                    }
                }
            }
        }
        
        public static void CopyTo(this Stream source, Stream destination, CancellationToken cancellationToken, int bufferSize = 4096)
        {
            var buffer = new byte[bufferSize];
            int count;
            while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                destination.WriteTimeout = 2000;
                destination.Write(buffer, 0, count);
            }
        }
        private static void upload()
        {      
            try
            {
                cts = new CancellationTokenSource();
                token = cts.Token;
              //  Logger.add("FTP", "Starting new thread");
                
                int tryBeforeSleep = 5;
                myTimer aTimer = new myTimer(2000);
                while (queue.Count > 0 && tryBeforeSleep > 0 && isRunning)
                {
                    
                    try
                    {
                        // take the next file to upload
                        string uploadFile = (string)queue.Peek();
                        string fileName = Path.GetFileName(uploadFile);
                        string relativePath = "";
                        if (Path.GetExtension(uploadFile) == ".DTA")
                        {
                            // ..\projectname\year\month\mode\filename.dta
                            relativePath = Functions.getRelativepath(uploadFile, 5);                  
                        }
                        else if (Path.GetExtension(uploadFile) == ".txt")
                        {
                            // upload to ftp:// ..\projectname\new\filename.txt
                            // ..\projectname\textfiles\filename.txt
                            relativePath = Functions.getRelativepath(uploadFile, 3).Split('\\')[0];
                            relativePath = Path.Combine(relativePath, "new");
                        }
                        else
                        {
                            queue.Dequeue();
                            cancelProgress(fileName);
                            Properties.Settings.Default.FTPQueue = Functions.stringifyQueue(queue);
                            Properties.Settings.Default.Save();
                            tryBeforeSleep++;
                            throw new Exception("Not allowed to upload an unknown file extenstion"+ 
                                Environment.NewLine+ "dequeing " + uploadFile);
                        }

                        relativePath = Path.Combine("data", relativePath);
                        // create the appropiate folders
                        createDir(relativePath);

                        Logger.add("FTP", "Prepare file to transfer :" + fileName);

                        // Get the object used to communicate with the server.
                        Logger.add("FTP", fileName);
                        FtpWebRequest request = getRequestObj(relativePath + "\\" +fileName);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                      

                        // Copy the contents of the file to the request stream.
                        using (var fs = File.OpenRead(uploadFile))
                        {
                            Stream requestStream = request.GetRequestStream();              
                            Logger.add("FTP", "Start transfering the file " + fileName);

                            // update file progress every 2 sec.
                            aTimer = new myTimer(2000); 
                            aTimer.stream = fs;
                            aTimer.fileName = fileName;
                            aTimer.fileSize = fs.Length;
                            aTimer.Elapsed += OnTimedEvent;
                            aTimer.Enabled = true;
                   
                            CopyTo((Stream)fs,requestStream,token);
                            aTimer.Dispose();
                            requestStream.Close();
                        }

                        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                        updateProgress(fileName, 100,"");
                        //Logger.add("FTP","Code: " + response.StatusCode);
                        Logger.add("FTP", "File upload completed.");
                        response.Close();
                        queue.Dequeue();
                        Properties.Settings.Default.FTPQueue = Functions.stringifyQueue(queue);
                        Properties.Settings.Default.Save();
                    }
                    catch (ThreadAbortException e)
                    {
                        aTimer.Dispose();
                        throw e;
                    }
                    catch (Exception e)
                    {

                        aTimer.Dispose();
                        if (token.IsCancellationRequested)
                        {
//                            aTimer.Dispose();
                            Logger.add("FTP", "cancnellation req.");
                        }
                        else { 
                        Logger.add("FTP", "*** Error *** " + e.Message);
                        //tryBeforeSleep--;
                        Thread.Sleep(120000);
                        }
                        //DialogResult result = MessageBox.Show("Transfering file failed", "FTP Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                        //if (result == DialogResult.Cancel) throw new Exception("File transfer was cancelled!");
                    }
                }
            }
            catch (ThreadAbortException e)
            {
                Console.WriteLine("FTP closed");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,"FTP");
                isRunning = false;
               // throw new Exception("FTP error",e);
                //Logger.add("ERROR", e.Message);
                //msgboxError("File upload failed!");
            }
            isRunning = false;
          //  Logger.add("FTP", "End of thread");
        }
    }
}

class myTimer : System.Timers.Timer
{
    public myTimer(double val):base(val){}
    public Stream stream { get; set; }
    public string fileName { get; set; }
    public long fileSize { get; set; }
}