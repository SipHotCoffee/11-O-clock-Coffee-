using CG.Test.Editor.Models.Nodes;
using CG.Test.Editor.Models.Types;
using CG.Test.Editor.ViewModels;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CG.Test.Editor.Views
{
    internal static class Native
    {
        [DllImport("ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr ptr);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }


        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern bool CredUnPackAuthenticationBuffer(int dwFlags,
                                                                       IntPtr pAuthBuffer,
                                                                       uint cbAuthBuffer,
                                                                       StringBuilder pszUserName,
                                                                       ref int pcchMaxUserName,
                                                                       StringBuilder pszDomainName,
                                                                       ref int pcchMaxDomainame,
                                                                       StringBuilder pszPassword,
                                                                       ref int pcchMaxPassword);

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern int CredUIPromptForWindowsCredentials(ref CREDUI_INFO notUsedHere,
                                                                     int authError,
                                                                     ref uint authPackage,
                                                                     IntPtr InAuthBuffer,
                                                                     uint InAuthBufferSize,
                                                                     out IntPtr refOutAuthBuffer,
                                                                     out uint refOutAuthBufferSize,
                                                                     ref bool fSave,
                                                                     int flags);



        public static bool TryGetCredentialsVistaAndUp(string serverName, [NotNullWhen(true)] out NetworkCredential? networkCredential)
        {
            var credui = new CREDUI_INFO
            {
                pszCaptionText = "Please enter the credentails for " + serverName,
                pszMessageText = "DisplayedMessage"
            };
            credui.cbSize = Marshal.SizeOf(credui);
            uint authPackage = 0;
            var save = false;
            var result = CredUIPromptForWindowsCredentials(ref credui, 0, ref authPackage, IntPtr.Zero, 0, out var outCredBuffer, out var outCredSize, ref save, 1 /* Generic */);

            var usernameBuf = new StringBuilder(100);
            var passwordBuf = new StringBuilder(100);
            var domainBuf   = new StringBuilder(100);

            var maxUserName = 100;
            var maxDomain   = 100;
            var maxPassword = 100;

            if (result == 0)
            {
                if (CredUnPackAuthenticationBuffer(0, outCredBuffer, outCredSize, usernameBuf, ref maxUserName,
                                                   domainBuf, ref maxDomain, passwordBuf, ref maxPassword))
                {
                    //TODO: ms documentation says we should call this but i can't get it to work
                    //SecureZeroMem(outCredBuffer, outCredSize);

                    //clear the memory allocated by CredUIPromptForWindowsCredentials 
                    CoTaskMemFree(outCredBuffer);
                    networkCredential = new NetworkCredential()
                    {
                        UserName = usernameBuf.ToString(),
                        Password = passwordBuf.ToString(),
                        Domain = domainBuf.ToString()
                    };
                    return true;
                }
            }

            networkCredential = null;
            return false;
        }
    }

    public partial class MainWindow : CustomWindow
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;

            viewModel.OwnerWindow = this;
        }

        public MainViewModel ViewModel => (MainViewModel)DataContext;

        private async void CustomWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OpenFileQueue();
        }

        private async void CustomWindow_Closing(object sender, CancelEventArgs e)
        {
            if (await ViewModel.CloseTabs())
            {
                e.Cancel = true;
            }
        }

        private void Array_ItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (ListViewItem)sender;
            var value = (NodeViewModelBase)item.Content;
            var selectedNode = ViewModel.SelectedTab!.Editor!.SelectedNode;

            if (selectedNode!.Node is JsonArrayNode arrayNode)
            {
                selectedNode.Save();
                ViewModel.SelectedTab!.Editor!.NavigateNode($"Element: [{arrayNode.Elements.IndexOf(value.Node)}]", value);
            }
        }

        private void Object_ItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (ListViewItem)sender;
            var (name, value) = (KeyValuePair<string, NodeViewModelBase>)item.Content;
            ViewModel.SelectedTab!.Editor!.NavigateNode(name, value);
        }
    }
}