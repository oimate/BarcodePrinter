using System;
using System.Windows.Forms;

namespace BarcodePrinter
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                //AttachConsole(ATTACH_PARENT_PROCESS);
                var s = new StringEncryption();
                var c = new Cfg.Config()
                {
                    Pwd = s.Encrypt(args[0]),
                    ErrorTag = "CT_CC06.BarPrint.SB.S1002_Error",
                    ResponseTag = "CT_CC06.BarPrint.SB.S1001_Response",
                    TriggerTag = "CT_CC06.BarPrint.SB.S1000_Trigger",
                    SkidIdTag = "CT_CC06.BarPrint.AA.I1030_RBDataSkidNo",
                    User = "example_db_login",
                    Db = "example_database",
                    Server = "example_server\\instance"
                };
                System.IO.File.WriteAllText("BP.json", Cfg.Serialize.ToJson(c), System.Text.Encoding.UTF8);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BPMain());
        }
    }
}