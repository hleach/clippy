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
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace clippy
{
  public class ProgressView : Form {
  
    private ProgressBar pb = new ProgressBar();
    private BackgroundWorker worker;
  
    public ProgressView(ArgInfo argInfo) {
        init();

        worker = new BackgroundWorker();
        worker.DoWork += (s, args) => {
            if (argInfo.bCopy) {
            ClipboardCodec.toClip(argInfo.copyFileName, worker, this);
            }
            if (argInfo.bPaste) {
                ClipboardCodec.fromClip(argInfo.pasteFileName, worker, this);
            }
        };
        worker.ProgressChanged += (s, args) => {
            pb.Value = args.ProgressPercentage;
        };
        worker.WorkerReportsProgress = true;
        worker.RunWorkerCompleted += (s, args) => {
            Close();
        };
        worker.RunWorkerAsync();
     
    }

    private void init() {
        pb.Minimum = 1;
        pb.Maximum = 100;
        pb.Step = 1;

        Size = new Size(280, 50);
        CenterToScreen();
        TopMost = true;
        
        pb.Dock = DockStyle.Fill;
        pb.Parent = this;    
    }  
    
    public void setClipboardText(string str) {
         System.Windows.Forms.Clipboard.SetText(str);
    }

  }
}

