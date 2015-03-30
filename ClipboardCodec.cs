//
// The MIT License
//
// Copyright (c) 2015 Heath Leach
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;

namespace clippy
{
  public static class ClipboardCodec {

        public static void toClip(String fileName, BackgroundWorker worker = null, 
                                    Form parent = null, Boolean dump = false) {
            const int LINE_SIZE = 60;
            
            BinaryReader inFile = null;
            byte[] buf;
            System.Text.StringBuilder strOut = new System.Text.StringBuilder();
                
            try {
                inFile = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read));
            } catch (Exception) {
                Console.Error.WriteLine("File not found.");
                Environment.Exit(1);
            }
            
            FileInfo fi = new FileInfo(fileName);
            long longFileSize = fi.Length / LINE_SIZE;
            int totalChunks = 0;
            if (longFileSize <= 2147483647) {
                totalChunks = Math.Max((int) longFileSize+1, 1);
            } else {
                Console.Error.WriteLine("File too large.");
                Environment.Exit(2);
            }
            
            int chunksPerUpdate = Math.Max((totalChunks / 100), 1);
            
            strOut.AppendFormat("CLIPFILE={0}\r\n", Path.GetFileName(fileName));
            strOut.AppendFormat("LINES={0}\r\n", totalChunks);

            buf = inFile.ReadBytes(LINE_SIZE);
            int chunkCount = 0;
            while (buf.Length > 0) {
                strOut.AppendFormat("{0}\r\n", Convert.ToBase64String(buf));
                buf = inFile.ReadBytes(LINE_SIZE);
                if ((worker != null) && ((chunkCount % chunksPerUpdate) == 0)) {
                    worker.ReportProgress(Math.Max(Math.Min((int) (chunkCount * 100 / totalChunks), 100), 1));
                }
                chunkCount++;
            }
            
            string content = strOut.ToString();
            
            if (parent == null) {
                System.Windows.Forms.Clipboard.SetText(content);
            } else {
                parent.Invoke((MethodInvoker)delegate() {
                    System.Windows.Forms.Clipboard.SetText(strOut.ToString());
               });
            }
            inFile.Close();
            if (dump) {
                Console.WriteLine(content);
            }
        }

        public static void fromClip(string fileNameOverride = null, 
                                      BackgroundWorker worker = null, Form parent = null) {
            String decodeString = null;
            if (parent == null) {
                decodeString = System.Windows.Forms.Clipboard.GetText();
            } else {
                parent.Invoke((MethodInvoker)delegate() {
                    decodeString = System.Windows.Forms.Clipboard.GetText();
               });
            }            
            
            StringReader inStr = new StringReader(decodeString);
            String fileName;
            String line = inStr.ReadLine();
            
            if (line == null) {
                Console.Error.WriteLine("Null line from clipboard (1).");
                Environment.Exit(1);
            }
            if (!line.StartsWith("CLIPFILE=")) {
                Console.Error.WriteLine("Clipfile argument missing.");
                Environment.Exit(1);
            }
            if (fileNameOverride == null) {
                fileName = line.Substring(9);
            } else {
                fileName = fileNameOverride;
            }
            
            line = inStr.ReadLine();
            
            if (line == null) {
                Console.Error.WriteLine("Null line from clipboard (2).");
                Environment.Exit(1);
            }
            if (!line.StartsWith("LINES=")) {
                Console.Error.WriteLine("Lines argument missing.");
                Environment.Exit(1);
            }
            int totalChunks = 0;
            try {
                totalChunks = Math.Max(Convert.ToInt32(line.Substring(6)), 1);
            } catch (Exception) {
                Console.Error.WriteLine("Error converting lines argument to integer.");
                Environment.Exit(1);
            }
            
            int chunksPerUpdate = Math.Max((totalChunks / 100), 1);
            int chunkCount = 0;
                        
            BinaryWriter outFile = new BinaryWriter(File.Create(fileName));
            while ((line = inStr.ReadLine()) != null) {
                outFile.Write(Convert.FromBase64String(line));
                if ((worker != null) && ((chunkCount % chunksPerUpdate) == 0)) {
                    worker.ReportProgress(Math.Max(Math.Min((int) (chunkCount * 100 / totalChunks), 100), 1));
                }
                chunkCount++;
            }
            outFile.Flush();
            outFile.Close();
        }

    }
}

