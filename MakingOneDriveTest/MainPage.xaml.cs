using Microsoft.OneDrive.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409


//      Author: John Leone, Gibbloggen@outlook.com
//      Date:   7/16/2016
//      License: MIT License
//      References:  Example is an adaptation of the material found here,,,  https://msdn.microsoft.com/en-us/magazine/mt632271.aspx
//          And this Stack Overflow Post,,, http://stackoverflow.com/questions/37397443/onedrive-api-create-folder-if-not-exist
//      Company: Essential Software Products  http://essentialsoftwareproducts.org
//

//The MIT License(MIT)
//Copyright(c) <2016> <John Leone, Essential Software Products>

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace OneDriveAdaptation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IOneDriveClient _client;
        private string _folderID;
        public MainPage()
        {
            this.InitializeComponent();
            MakeFolder.Visibility = Visibility.Collapsed;
            DeleteFolder.Visibility = Visibility.Collapsed;


            
            AuthenticationButton.Click += async (s, e) =>
            {
                var scopes = new[]
                {
                    "onedrive.readwrite",
                    "onedrive.appfolder",
                    "wl.signin"
                 };
                _client = OneDriveClientExtensions.GetClientUsingOnlineIdAuthenticator(
                   scopes);
                var session = await _client.AuthenticateAsync();
                System.Diagnostics.Debug.WriteLine($"Token: {session.AccessToken}");
                var successDialog = new MessageDialog(
            $"You have been authenticated with Tocken  {session.AccessToken}",
            "Authenticated!");
                await successDialog.ShowAsync();
                bool hasFolder = false;
                //folder Status
                IChildrenCollectionPage item = null;
                try
                {
                    //Checks if approot has children, going to be sprinkling more try and catches around, but this is it for the first release
                    item = await _client.Drive.Special.AppRoot.Children.Request().GetAsync();
                }
                catch
                {
                    hasFolder = false;
                }

                if (item.Count == 0) 
                { hasFolder = false; }
                else
                {
                    foreach (var entity in item)
                    {
                        if (entity.Name == "EssentialSoftwareProducts")
                        {
                            //We know the name of the folder, so we are letting the system know they need to offer a delete
                            hasFolder = true;
                            _folderID = entity.Id;

                        }



                    }

                }

               status.Text = "";
               status2.Text = "";




                if (hasFolder) 
                {
                    status.Text = "The folder already exists, we can go forward or delete the folder";
                    DeleteFolder.Visibility = Visibility.Visible;
                }
                else
                {
                    status.Text = "The folder does not exist, you will need to click \"Make Folder\" ";
                    MakeFolder.Visibility = Visibility.Visible;


                }
                
            };
           
        }



        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            //Adjust heights so that the scroll bars work on the main screen.
            //It is a bit of a hack, but I have not found a better way to do this yet.
            double j = e.NewSize.Height;
            double jj = e.NewSize.Width;
            if ((j > 500) && (jj < 500))
            {
                MyScrollViewer.Height = j - 105;
                MainStackPanel.Height = j - 65;
            }
            else
            {
                MyScrollViewer.Height = j - 35;
                MainStackPanel.Height = j - 5;

            }
                return;

        }
        private async void MakeFolder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var folderToCreate = new Item { Name = "EssentialSoftwareProducts", Folder = new Folder() };
            var newFolder = await _client.Drive.Special.AppRoot.Children.Request().AddAsync(folderToCreate);
            status.Text = "The Folder has been created, you can now delete it. ";
            status2.Text = "EssentialSoftwareProducts Folder created in App Root with id of: " + newFolder.Id.ToString();
            _folderID = newFolder.Id;
            DeleteFolder.Visibility = Visibility.Visible;
            MakeFolder.Visibility = Visibility.Collapsed;

        }

        private async void DeleteFolder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            
            await _client
              .Drive
              .Items[_folderID]
              .Request()
              .DeleteAsync()
              ;
            status.Text = "The Folder has been deleted,,,,";
            status2.Text = "EssentialSoftwareProducts Folder deleted in App Root with id of: " + _folderID;

            DeleteFolder.Visibility = Visibility.Collapsed;
            MakeFolder.Visibility = Visibility.Visible;


        }

        
    }
}
