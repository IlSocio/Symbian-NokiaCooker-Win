using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Firmware;
using FuzzyByte.Forms;
using FuzzyByte.Utils;
using System.Threading;
using System.Reflection;
using System.IO;
//using System.IO.Pipes;


namespace FuzzyByte.NokiaCooker
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            string appGuid = Assembly.GetExecutingAssembly().GetType().GUID.ToString();

            /*            using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
                        {
                            if (!mutex.WaitOne(0, false))
                            {
                                if (args.Length > 0)
                                {
                                    using (NamedPipeClientStream cliStream = new NamedPipeClientStream(".", "NokiaCookerPipe", PipeDirection.InOut, PipeOptions.None))
                                    {
                                        cliStream.Connect(2000);
                                        using (StreamWriter sw = new StreamWriter(cliStream))
                                        {
                                            sw.WriteLine(args[0]);
                                        }
                                        cliStream.Close();
                                    }
                                }
                                return;
                            }

                            using (Form1 form = new Form1(args))
                            {
                                Application.Run(form);
                            }
                        }*/

            using (Form1 form = new Form1(args))
            {
                Application.Run(form);
            }
        }

        // Non GUI
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = e.ExceptionObject as Exception;
                if (ex != null)
                {
                    if (ex.InnerException is FuzzyByteException)
                    {
                        Notes.ShowWarning(ex.InnerException.Message);
                        return;
                    }
                    else
                    {
#if DEBUG
                        Notes.ShowError("Unexpected Error", ex.Message + "\n"+ ex.StackTrace);
#else
                        Notes.ShowError("Unexpected Error", ex.Message);
#endif
                    }
                }
            }
            finally
            {
                Application.Exit();
            }
            throw new NotImplementedException();
        }

        // GUI Exception
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            try
            {
                if (e.Exception is FuzzyByteException)
                {
                    Notes.ShowWarning(e.Exception.Message);
                    return;
                }
                else
                {
#if DEBUG
                    Notes.ShowError("Unexpected Error", e.Exception.Message + "\n" + e.Exception.StackTrace);
#else
                    Notes.ShowError("Unexpected Error", e.Exception.Message);
#endif
                }
            }
            finally
            {
                Application.Exit();
            }
        }
    }
}
