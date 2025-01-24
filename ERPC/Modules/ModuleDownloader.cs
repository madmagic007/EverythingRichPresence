using ERPC.LuaStuff;
using Microsoft.VisualBasic;
using System.Net;

namespace ERPC.Modules {

    public class ModuleDownloader {

        public static void ShowDialog() {
            string url = Interaction.InputBox("Fill in download URL", "Download Module", "");

            if (string.IsNullOrEmpty(url)) {
                return;
            }

            if (!(Uri.TryCreate(url, UriKind.Absolute, out Uri uri) && uri != null &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))) {

                ShowInvalid();
                return;
            }

            string fileName = Path.GetFileName(uri.LocalPath);

            SaveFileDialog sfd = new() {
                FileName = fileName,
                InitialDirectory = Program.modulesDir.FullName,
                Title = "Save Module",
                Filter = "lua files (*.lua)|*.lua"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            using WebClient c = new();
            c.DownloadFile(url, sfd.FileName);

            LuaHandler.Init();
            ModuleHandler.Init();

            Program.SendNotif("Downloaded " + fileName);
        }

        private static void ShowInvalid() {
            MessageBox.Show("Please fill in a valid URL", "Infalid URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ShowDialog();
        }
    }
}
