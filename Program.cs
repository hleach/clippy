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
using System.Windows.Forms;

namespace clippy
{
    class MainClass
    {

        private static void doHelp()
        {
            Console.WriteLine("\r\n                                 -clippy-\r\n" +
                              "\r\n" +
                              "  --copy, -c <filename>:     Copy file to base64 encoded clipboard.\r\n" +
                              "  --paste, -p:               Paste base64 encoded clipboard to file.\r\n" +
                              "  --gui, -g:                 Show gui progress bar.\r\n" +
                              "  --dump, -d:                Dump copy output to stdout.\r\n" +
                              "  --help, -h:                This help.\r\n" +
                              "\r\n");
            Environment.Exit(0);
        }

        private static ArgInfo processArgs(string[] args)
        {
            ArgInfo argInfo;
            argInfo.bCopy = false;
            argInfo.bPaste = false;
            argInfo.bDump = false;
            argInfo.bShowProgress = false;
            argInfo.copyFileName = null;
            argInfo.pasteFileName = null;

            int count = 1;
            while (count <= args.Length)
            {
                switch (args[count - 1].ToLower())
                {
                    case "--copy":
                    case "-c":
                    case "/c":
                        argInfo.bCopy = true;
                        if (count < args.Length)
                        {
                            argInfo.copyFileName = args[count];
                            count++;
                        }
                        else doHelp();
                        break;
                    case "--paste":
                    case "-p":
                    case "/p":
                        argInfo.bPaste = true;
                        break;
                    case "--out":
                    case "-o":
                    case "/o":
                        argInfo.bPaste = true;
                        if (count < args.Length)
                        {
                            argInfo.pasteFileName = args[count];
                            count++;
                        }
                        else doHelp();
                        break;
                    case "--dump":
                    case "-d":
                    case "/d":
                        argInfo.bDump = true;
                        break;
                    case "--gui":
                    case "-g":
                    case "/g":
                        argInfo.bShowProgress = true;
                        break;
                    case "--help":
                    case "-h":
                    case "/h":
                        doHelp();
                        break;
                    default:
                        doHelp();
                        break;
                }
                count++;
            }

            return argInfo;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            ArgInfo argInfo = processArgs(args);

            if (!(argInfo.bCopy | argInfo.bPaste))
            {
                doHelp();
            }

            if (argInfo.bShowProgress)
            {
                Application.Run(new ProgressView(argInfo));
            }
            else
            {
                if (argInfo.bCopy)
                {
                    ClipboardCodec.toClip(argInfo.copyFileName, dump: argInfo.bDump);
                }
                if (argInfo.bPaste)
                {
                    ClipboardCodec.fromClip(argInfo.pasteFileName);
                }
            }
        }
    }
}
