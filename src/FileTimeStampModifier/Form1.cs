using System.Text.RegularExpressions;

namespace FileTimeStampModifier
{
    public partial class Form1 : Form
    {
        FileTimeStampModifier.ListViewItemComparer sorter = new FileTimeStampModifier.ListViewItemComparer();

        public Form1()
        {
            InitializeComponent();

            this.sorter.ColumnModes = new ListViewItemComparer.ComparerMode[]
            {
                ListViewItemComparer.ComparerMode.String,
                ListViewItemComparer.ComparerMode.Integer,
                ListViewItemComparer.ComparerMode.DateTime,
                ListViewItemComparer.ComparerMode.DateTime,
            };
            this.listView1.ListViewItemSorter = this.sorter;
        }

        private void buttonOpenDirDialog_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == this.folderBrowserDialog1.ShowDialog())
            {
                this.textBoxDirPath.Text = this.folderBrowserDialog1.SelectedPath;
            }

            this.loadFileList(this.textBoxDirPath.Text);
        }

        private void loadFileList(string path)
        {
            this.listView1.Items.Clear();

            var regex = new Regex(@"^\D*(?<num>\d+)\D*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            var regexAsciiNum = new Regex(@"^[0-9]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            var map = new Dictionary<char, char>();
            map.Add('０', '0');
            map.Add('１', '1');
            map.Add('２', '2');
            map.Add('３', '3');
            map.Add('４', '4');
            map.Add('５', '5');
            map.Add('６', '6');
            map.Add('７', '7');
            map.Add('８', '8');
            map.Add('９', '9');

            var files = Directory.GetFiles(path);
            foreach (var item in files)
            {
                var fi = new FileInfo(item);
                var m = regex.Match(fi.Name);

                var num = 0;
                if (m.Success)
                {
                    var tmp = m.Groups["num"].Value.Select(x =>
                    {
                        if (regexAsciiNum.IsMatch(x.ToString()))
                        {
                            return x;
                        }
                        else
                        {
                            return map[x];
                        }
                    }).ToList();
                    num = int.Parse(string.Join("", tmp));
                }

                var format = "yyyy/MM/dd hh:mm:ss";
                this.listView1.Items.Add(new ListViewItem(new string[] { fi.Name, num.ToString(), fi.LastWriteTime.ToString(format), fi.CreationTime.ToString(format) }));
            }


            this.sorter.Column = 1;
            this.listView1.Sort();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            this.sorter.Column = e.Column;
            this.listView1.Sort();
        }

        private void buttonRenewFileTime_Click(object sender, EventArgs e)
        {
            this.label3.Text = "START: " + DateTime.Now.ToString("hh:mm:ss");

            DateTime datetime = this.dateTimePicker1.Value;
            foreach (ListViewItem item in this.listView1.Items)
            {
                var fi = new FileInfo(Path.Combine(this.textBoxDirPath.Text, item.SubItems[0].Text));
                fi.LastWriteTime = datetime;
                fi.CreationTime = datetime;

                datetime = datetime.AddSeconds(1);
            }

            this.label3.Text = "FINISHED: " + DateTime.Now.ToString("hh:mm:ss");
        }

        private void buttonReload_Click(object sender, EventArgs e)
        {
            var path = this.textBoxDirPath.Text;
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                // orderが変わってしまうが、あまり困らないので放置する
                this.loadFileList(this.textBoxDirPath.Text);
            }
            else
            {
                this.label3.Text = "無効なディレクトリです";
            }
        }
    }
}