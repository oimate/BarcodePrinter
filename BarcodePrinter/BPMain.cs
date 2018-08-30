using BarcodePrinter.Database;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodePrinter
{
    public partial class BPMain : Form
    {
        private BackgroundWorker _bg;
        private string _db;
        private string _filePath = @"BP.ini";
        private IntPtr _handle = IntPtr.Zero;
        private string _pwd;
        private bool _registered = false;
        private StringEncryption _se;
        private string _server;
        private int _skidIdVal = 0;
        private string _triggerTag, _skidIdTag, _responseTag, _errorTag;
        private bool _triggerTagOn = false, _triggered = false;
        private string _user;
        private DataTable DT;
        private DbBarcodePrinter mainBarcodePrinter;
        private ushort WM_COMMLIBX = 0;

        public BPMain()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_COMMLIBX)
            {
                HandleCommLibMsg(ref m);
            }
            base.WndProc(ref m);
        }

        private static T ReadStruct<T>(IntPtr pointer)
        {
            try
            {
                if (pointer == IntPtr.Zero)
                {
                    return default(T);
                }

                var temp = (T)Marshal.PtrToStructure(pointer, typeof(T));
                return temp;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        private static bool SplitTagname(string tagName, out string myPlc, out string myTag)
        {
            myPlc = "";
            myTag = "";

            if (tagName.Length == 0)
            {
                return false;
            }

            int p = tagName.IndexOf('.');
            if (p == -1)
            {
                myPlc = tagName;
                return false;
            }
            myPlc = tagName.Substring(0, p);
            myTag = tagName.Substring(p);
            return true;
        }

        private void AddLineToDT(string value)
        {
            if (InvokeRequired)
                Invoke(new Action<string>(AddLineToDT), value);
            else
            {
                if (value.Length > 0)
                {
                    var dts = DT.Select(@"[tag]='" + value + "'");
                    if (dts.Length == 0)
                    {
                        DT.Rows.Add(value, "0", "0", false);
                        dgv_Result.Refresh();
                    }
                }
            }
        }

        private void AdviseAllDT()
        {
            foreach (DataRow r in DT.Rows)
            {
                if (!(bool)(r["advised"]) & ((string)(r["tag"])).Length > 0)
                {
                    r["advised"] = this.SymbolX_Advise((string)(r["tag"]));
                }
            }
        }

        private void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = mainBarcodePrinter.ExecSP_WriteToERPTransferTab(e.Argument.ToString());
        }

        private void Bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool)e.Result)
            {
                this.SymbolX_Poke(_responseTag, "-1");
            }
            else
            {
                this.SymbolX_Poke(_errorTag, "-1");
            }
        }

        private async void BPMain_Load(object sender, EventArgs e)
        {
            var h = Handle;
            await Task.Run(() =>
            {
                _se = new StringEncryption();

                var s = _se.Encrypt("user");

                Init_DT();
                RegisterCOMMLIB(h);
                ImportConfig(_filePath);
                mainBarcodePrinter = new DbBarcodePrinter(_server, _user, _pwd, _db);

                var a = new Action<bool>((ok) => panel1.BackColor = (ok) ? System.Drawing.Color.Green : System.Drawing.Color.Red);

                mainBarcodePrinter.ConnectionChange += new EventHandler<bool>( (o, ok) =>
                {
                    if (InvokeRequired)
                        Invoke(new Action<bool>(a), ok);
                    else
                        a(ok);
                });

                if (InvokeRequired)
                    Invoke(new Action<bool>(a), mainBarcodePrinter.ConnectionOk);
                else
                    a(mainBarcodePrinter.ConnectionOk);

                _bg = new BackgroundWorker();
                _bg.DoWork += new DoWorkEventHandler(Bg_DoWork);
                _bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Bg_RunWorkerCompleted);

                AdviseAllDT();
            });
            h = IntPtr.Zero;
        }

        private void Click_AddIOManager(object sender, EventArgs e)
        {
            textBox6.Enabled = false;
            if (textBox6.Text.Length > 0)
            {
                this.CommLib_AddIOManager(textBox6.Text);
            }
        }

        private void Click_RemoveIOManager(object sender, EventArgs e)
        {
            textBox6.Enabled = true;
            if (textBox6.Text.Length > 0)
            {
                this.CommLib_RemoveIOManager(textBox6.Text);
            }
        }

        private void Click_Show(object sender, EventArgs e)
        {
            CommLib.CommLib_Show();
        }

        private void Click_ShowDiag(object sender, EventArgs e)
        {
            CommLib.CommLib_CommDiag(_handle);
        }

        private bool CommLib_AddIOManager(string host)
        {
            bool res = CommLib.CommLib_AddIOManager(host);
            return res;
        }

        private bool CommLib_RemoveIOManager(string host)
        {
            bool res = CommLib.CommLib_RemoveIOManager(host);
            return res;
        }

        private void HandleCommLibMsg(ref Message m)
        {
            var dataRec = ReadStruct<CommLib.DataRec>(m.WParam);
            if (dataRec == null)
            {
                return;
            }

            var tag = dataRec.Tagname;
            if (tag.Length == 0)
            {
                return;
            }

            DataRow[] dts = DT.Select(@"[tag]='" + dataRec.Tagname + "'");
            dts[0]["value"] = dataRec.Value;
            dts[0]["quality"] = dataRec.Quality;
            dts = null;

            dts = DT.Select(@"[tag]='" + _triggerTag + "'");
            _triggerTagOn = Int32.Parse(dts[0]["value"].ToString(), System.Globalization.NumberStyles.Integer) != 0;
            dts = null;

            dts = DT.Select(@"[tag]='" + _skidIdTag + "'");
            _skidIdVal = Int32.Parse(dts[0]["value"].ToString(), System.Globalization.NumberStyles.Integer);
            dts = null;

            if (_triggerTagOn & _skidIdVal != 0 & !_triggered)
            {
                _triggered = true;
                _bg.RunWorkerAsync(_skidIdVal.ToString("D4"));
            }

            if (!_triggerTagOn & _triggered)
            {
                _triggered = false;
                SymbolX_Poke(_errorTag, "0");
                SymbolX_Poke(_responseTag, "0");
            }

            dgv_Result.Refresh();
        }

        private void ImportConfig(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    var c = Cfg.Config.FromJson(File.ReadAllText(filePath));

                    _server = c.Server;
                    _db = c.Db;
                    _user = c.User;
                    _pwd = _se.Decrypt(c.Pwd);
                    _triggerTag = c.TriggerTag;
                    _skidIdTag = c.SkidIdTag;
                    _responseTag = c.ResponseTag;
                    _errorTag = c.ErrorTag;

                    AddLineToDT(_triggerTag);
                    AddLineToDT(_skidIdTag);
                    AddLineToDT(_responseTag);
                    AddLineToDT(_errorTag);
                }
                catch (Exception)
                {
                    MessageBox.Show("problem while reading BP.ini file!" + Environment.NewLine +
                        "fix the file and try again or " + Environment.NewLine +
                        "contact durr: Support.EcoEmos@durr.com");
                }
            }
            else
                File.WriteAllText(filePath, Cfg.Serialize.ToJson(new Cfg.Config()));
        }

        private void Init_DT()
        {
            DT = new DataTable("IOMData");
            DT.Columns.Add("tag", typeof(string));
            DT.Columns.Add("value", typeof(string));
            DT.Columns.Add("quality", typeof(string));
            DT.Columns.Add("advised", typeof(bool));
            UniqueConstraint uq = new UniqueConstraint(DT.Columns["tag"]);
            DT.Constraints.Add(uq);

            if (dgv_Result.InvokeRequired)
                dgv_Result.Invoke(new Action<DataTable>((source) => dgv_Result.DataSource = source), DT);
            else
                dgv_Result.DataSource = DT;
        }

        private void RegisterCOMMLIB(IntPtr handle)
        {
            if (InvokeRequired)
                Invoke(new Action<IntPtr>(RegisterCOMMLIB), handle);
            else
            {
                if (!_registered)
                {
                    WM_COMMLIBX = WinUser32.RegisterWindowMessage("WM_COMMLIBX");
                    _handle = handle;
                    uint res = CommLib.SymbolX_PassPointers(_handle);
                    _registered = true;
                }
            }
        }

        private bool SymbolX_Advise(string tagName)
        {
            string plc, tag;
            SplitTagname(tagName, out plc, out tag);
            int res = CommLib.SymbolX_Advise(_handle, plc, tag);
            return res != 0;
        }

        private bool SymbolX_Poke(string tagName, string value)
        {
            string plc, tag;
            SplitTagname(tagName, out plc, out tag);
            int res = CommLib.SymbolX_Poke(_handle, plc, tag, value, 1);
            return res != 0;
        }
    }
}