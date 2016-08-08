using Microsoft.OneDrive.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
//
//
//      References:  Example is an adaptation of the material found here,,,  https://msdn.microsoft.com/en-us/magazine/mt632271.aspx
//          And this Stack Overflow Post,,, http://stackoverflow.com/questions/37397443/onedrive-api-create-folder-if-not-exist
//          And this Stack Overflow Post,,,  //Many Thanks to ginach  http://stackoverflow.com/questions/33398348/create-app-folder-and-upload-file-using-onedrive-api 
//
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
        private IOneDriveClient _client;  //This is the client, that all one drive interactions are based off of.  Comes from MSDN Article

        //These are various one drive id files.  2 for the folders, and two for the files created.
        private string _folderAppSettingsID;
        private string _folderPersonalFilesID;
        private string _fileAppSettingsID;
        private string _filePersonalFilesID;



        public MainPage()
        {
            this.InitializeComponent();

            //All Buttons on the form are not visible, except for the authentication.  Buttons are only visible, when they can be used.
            Collapse_All();




            AuthenticationButton.Click += async (s, e) =>
            {
                var scopes = new[]
                {
                  // "onedrive.readwrite", // Commented out, this was requesting read/write for all of one drive
                    "onedrive.appfolder",  //This is the only one I really need to do this demo.
                    "wl.signin"  //for now I'm keeping this one, it is for automatically signing on from the app, I think this is ok, otherwise will adjut
                 };
                _client = OneDriveClientExtensions.GetClientUsingOnlineIdAuthenticator(
                   scopes);
                AccountSession session = null;
                try
                {
                    session = await _client.AuthenticateAsync();
                }
                catch
                {

                    var failureDialog = new MessageDialog(
       $"You have not been authenticated, you cannot use this program without being Authenticated.",
       "Not Authenticated!");
                    await failureDialog.ShowAsync();
                    return;


                }

                System.Diagnostics.Debug.WriteLine($"Token: {session.AccessToken}");
                var successDialog = new MessageDialog(
            $"You have been authenticated with Tocken  {session.AccessToken}",
            "Authenticated!");
                await successDialog.ShowAsync();
                await WhatButtons();

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

        private async void MakeFolderAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await WhatButtons();
            if (_folderAppSettingsID == "none")
            {
                var folderToCreate = new Item { Name = "AppSettingsDoNotTouch", Folder = new Folder() };
                var newFolder = await _client.Drive.Special.AppRoot.Children.Request().AddAsync(folderToCreate);
                status.Text = "The App Settings folder has been created. ";
                //status2.Text = "EssentialSoftwareProducts Folder created in App Root with id of: " + newFolder.Id.ToString();
                _folderAppSettingsID = newFolder.Id;
                MakeFileAppSettings.Visibility = Visibility.Visible;
            }
           // await WhatButtons();

        }

        private async void DeleteFolderAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            /*
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
                    if (entity.Name == "AppSettingsDoNotTouch")
                    {
                        //We know the name of the folder, so we are letting the system know they need to offer a delete
                        hasFolder = true;
                        _folderAppSettingsID = entity.Id;

                    }



                }

            }
            */
            await WhatButtons();
            if (_folderAppSettingsID != "none")
            {
                await _client
                  .Drive
                  .Items[_folderAppSettingsID]
                  .Request()
                  .DeleteAsync()
                  ;
                status.Text = "The App Settings Folder has been deleted,,,,";
                ReadAppSettings.Visibility = Visibility.Collapsed;
                UpdateAppSettings.Visibility = Visibility.Collapsed;
                MakeFileAppSettings.Visibility = Visibility.Collapsed;
                DeleteFileAppSettings.Visibility = Visibility.Collapsed;
                // status2.Text = "EssentialSoftwareProducts Folder deleted in App Root with id of: " + _folderID;
            }
            else
            {
                status.Text = "App Settings Folder Not found, cannot delete!";
                //status2.Text = "You can create it again though,,,";
            }


            //await WhatButtons();
        }

        private async void MakeFileAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await WhatButtons();
            if (_fileAppSettingsID == "none")
            {

                StorageFile fileToCreate = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("AppSettings.txt", CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(fileToCreate, "Setting A");
                //var q = await _client.Drive.Special.AppRoot.ItemWithPath
                // var file2ToCreate = new Item { Name = "MERDE.TXT", File =  fileToCreate };

                //Windows.Storage.KnownFolders.
                string q = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\AppSettings.txt";//"\\AppSettings" + annoying++ + ".txt";
                FileStream g = System.IO.File.Open(q, FileMode.OpenOrCreate);


                //The following sequence creates the file on One Drive, 
                //Many Thanks to ginach  http://stackoverflow.com/questions/33398348/create-app-folder-and-upload-file-using-onedrive-api 

                var item = await _client
                    .Drive
                    .Items[_folderAppSettingsID]
                    .Children["AppSettings.txt"]
                    .Content
                    .Request()
                    .PutAsync<Item>(g);
                // g.Flush();
                //z.Close();

                // await  fileToCreate.DeleteAsync();


                _fileAppSettingsID = item.Id;
                DeleteFileAppSettings.Visibility = Visibility.Visible;
                UpdateAppSettings.Visibility = Visibility.Visible;
                ReadAppSettings.Visibility = Visibility.Visible;

            }

           // await WhatButtons();
//
        }

        private async void DeleteFileAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await WhatButtons();
            if (_fileAppSettingsID != "none")
            {
                await _client
                      .Drive
                      .Items[_fileAppSettingsID]
                      .Request()
                      .DeleteAsync();
                DeleteFileAppSettings.Visibility = Visibility.Collapsed;
                MakeFileAppSettings.Visibility = Visibility.Visible;
            } else
            {
                DeleteFileAppSettings.Visibility = Visibility.Visible;
                MakeFileAppSettings.Visibility = Visibility.Collapsed;

            }
            //await WhatButtons();
        }

        private async void MakeFolderPersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await WhatButtons();
            if (_folderPersonalFilesID == "none")
            {
                var folderToCreate = new Item { Name = "PersonalFilesYoursToShare", Folder = new Folder() };
                var newFolder = await _client.Drive.Special.AppRoot.Children.Request().AddAsync(folderToCreate);
                status.Text = "The Personal Files folder has been created. ";
                //status2.Text = "EssentialSoftwareProducts Folder created in App Root with id of: " + newFolder.Id.ToString();
                _folderPersonalFilesID = newFolder.Id;

            }
            await WhatButtons();
        }

        private async void DeleteFolderPersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            /*
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
                    if (entity.Name == "PersonalFilesYoursToShare")
                    {
                        //We know the name of the folder, so we are letting the system know they need to offer a delete
                        hasFolder = true;
                        _folderPersonalFilesID = entity.Id;

                    }



                }

            }*/
            await WhatButtons();

            if (_folderPersonalFilesID != "none")
            {
                await _client
                  .Drive
                  .Items[_folderPersonalFilesID]
                  .Request()
                  .DeleteAsync()
                  ;
                status.Text = "The Personal Files Folder has been deleted,,,,";
                // status2.Text = "EssentialSoftwareProducts Folder deleted in App Root with id of: " + _folderID;
            }
            else
            {
                status.Text = "Personal Files Folder Not found, cannot delete!";
                //status2.Text = "You can create it again though,,,";
            }

            await WhatButtons();

        }

        private async void MakeFilePersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {

            await WhatButtons();
            if (_filePersonalFilesID == "none")
            {
                StorageFile fileToCreate = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("PersonalFiles.txt", CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(fileToCreate, "Personal A");
                //var q = await _client.Drive.Special.AppRoot.ItemWithPath
                // var file2ToCreate = new Item { Name = "MERDE.TXT", File =  fileToCreate };

                //Windows.Storage.KnownFolders.
                string q = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\PersonalFiles.txt";
                FileStream g = System.IO.File.Open(q, FileMode.Open);


                //The following sequence creates the file on One Drive, 
                //Many Thanks to ginach  http://stackoverflow.com/questions/33398348/create-app-folder-and-upload-file-using-onedrive-api 

                var item = await _client
                    .Drive
                    .Items[_folderPersonalFilesID]
                    .Children["PersonalFiles.txt"]
                    .Content
                    .Request()
                    .PutAsync<Item>(g);


                await fileToCreate.DeleteAsync();

            }
            await WhatButtons();
        }

        private async void DeleteFilePersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await WhatButtons();
            if (_filePersonalFilesID != "none")
            {
                await _client
                      .Drive
                      .Items[_filePersonalFilesID]
                      .Request()
                      .DeleteAsync();
            }
            await WhatButtons();

        }

        private async void UpdateAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await WhatButtons();

            if (_fileAppSettingsID != "none")
            {
                var contentStream = await _client
                               .Drive
                               .Items[_fileAppSettingsID]
                               .Content
                               .Request()
                               .GetAsync();


                int r = (int)contentStream.Length;
                byte[] q = new byte[r];

                await contentStream.ReadAsync(q, 0, r);

                var str = System.Text.Encoding.ASCII.GetString(q);


                string t = "WTF";
                if (str == "Setting A")
                {
                    t = "Setting B";
                }
                else if (str == "Setting B")
                {
                    t = "Setting C";
                }
                else if (str == "Setting C")
                {
                    t = "Setting A";
                }

                byte[] z = new byte[(int)t.Length];
                z = System.Text.Encoding.ASCII.GetBytes(t);
                System.IO.MemoryStream streamitSoft = new MemoryStream(z);
                // await streamitSoft.WriteAsync(z, 0, z.Length);
                ///StreamWriter writeitbaby = new StreamWriter(streamitSoft);


                //await writeitbaby.WriteAsync(q);


                var uploadedItem = await _client
                                             .Drive
                                             .Items[_fileAppSettingsID]
                                             .Content
                                             .Request()
                                             .PutAsync<Item>(streamitSoft);



                var AppSettingDialog = new MessageDialog(
    "Your app setting has been changed to,,,," + t,
    "App Setting");
                await AppSettingDialog.ShowAsync();
            }
           // await WhatButtons();
        }

        private async void ReadAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await WhatButtons();
            if (_fileAppSettingsID != "none")
            {
                var contentStream = await _client
                                  .Drive
                                  .Items[_fileAppSettingsID]
                                  .Content
                                  .Request()
                                  .GetAsync();


                int r = (int)contentStream.Length;
                byte[] q = new byte[r];

                await contentStream.ReadAsync(q, 0, r);

                var str = System.Text.Encoding.ASCII.GetString(q);

                var AppSettingDialog = new MessageDialog(
    "Your app setting is,,,," + str,
    "App Setting");
                await AppSettingDialog.ShowAsync();

            }
           // await WhatButtons();


        }
        private void Collapse_All()
        {
            MakeFolderAppSettings.Visibility = Visibility.Collapsed;
            DeleteFolderAppSettings.Visibility = Visibility.Collapsed;
            MakeFileAppSettings.Visibility = Visibility.Collapsed;
            DeleteFileAppSettings.Visibility = Visibility.Collapsed;
            MakeFolderPersonalFiles.Visibility = Visibility.Collapsed;
            DeleteFolderPersonalFiles.Visibility = Visibility.Collapsed;
            MakeFilePersonalFiles.Visibility = Visibility.Collapsed;
            DeleteFilePersonalFiles.Visibility = Visibility.Collapsed;
            ReadAppSettings.Visibility = Visibility.Collapsed;
            UpdateAppSettings.Visibility = Visibility.Collapsed;


        }

        private async Task<int> WhatButtons()
        {
            Collapse_All();
            bool hasFolderAppSettings = false;
            bool hasFolderPersonalFiles = false;
            //folder Status

            _fileAppSettingsID = "none";
            _filePersonalFilesID = "none";
            _folderAppSettingsID = "none";
            _folderPersonalFilesID = "none";
            IChildrenCollectionPage item = null;
            try
            {
                //Checks if approot has children, going to be sprinkling more try and catches around, but this is it for the first release
                item = await _client.Drive.Special.AppRoot.Children.Request().GetAsync();
            }
            catch
            {
                hasFolderAppSettings = false;
                hasFolderPersonalFiles = false;

            }

            if (item.Count == 0)
            {
                hasFolderAppSettings = false;
                hasFolderPersonalFiles = false;
            }
            else
            {
                foreach (var entity in item)
                {
                    if (entity.Name == "AppSettingsDoNotTouch")
                    {
                        //We know the name of the folder, so we are letting the system know they need to offer a delete
                        hasFolderAppSettings = true;
                        _folderAppSettingsID = entity.Id;
                        DeleteFolderAppSettings.Visibility = Visibility.Visible;

                    }
                    else if (entity.Name == "PersonalFilesYoursToShare")
                    {
                        //We know the name of the folder, so we are letting the system know they need to offer a delete
                        hasFolderPersonalFiles = true;
                        _folderPersonalFilesID = entity.Id;
                        DeleteFolderPersonalFiles.Visibility = Visibility.Visible;

                    }



                }

            }

            status.Text = "";
            status2.Text = "";




            if (hasFolderAppSettings)
            {


                bool hasAppChildren = false;
                IChildrenCollectionPage AppChildren = null;


                try
                {
                    AppChildren = await _client
                                                 .Drive
                                                 .Items[_folderAppSettingsID]
                                                 .Children
                                                 .Request()
                                                 .GetAsync();
                    hasAppChildren = true;

                }
                catch
                {
                    hasAppChildren = false;


                }
                if (hasAppChildren)
                {
                    if (AppChildren.Count > 0)
                    {
                        foreach (var entity in AppChildren)
                        {
                            if (entity.Name == "AppSettings.txt")
                            {
                                //We know the name of the folder, so we are letting the system know they need to offer a delete
                                DeleteFileAppSettings.Visibility = Visibility.Visible;
                                UpdateAppSettings.Visibility = Visibility.Visible;
                                ReadAppSettings.Visibility = Visibility.Visible;
                                _fileAppSettingsID = entity.Id;
                                hasAppChildren = true;

                            }

                        }
                    }
                    else
                    {


                        DeleteFileAppSettings.Visibility = Visibility.Collapsed;
                        UpdateAppSettings.Visibility = Visibility.Collapsed;
                        ReadAppSettings.Visibility = Visibility.Collapsed;
                        MakeFileAppSettings.Visibility = Visibility.Visible;
                        _fileAppSettingsID = "none";



                    }
                }
                else
                {


                    DeleteFileAppSettings.Visibility = Visibility.Collapsed;
                    UpdateAppSettings.Visibility = Visibility.Collapsed;
                    ReadAppSettings.Visibility = Visibility.Collapsed;
                    MakeFileAppSettings.Visibility = Visibility.Visible;
                    _fileAppSettingsID = "none";



                }





            }
            else
            {
                //status.Text = "The personal files folder does not exist, you will need to click \"Make Personal Files Folder\" ";
                MakeFolderAppSettings.Visibility = Visibility.Visible;
                DeleteFileAppSettings.Visibility = Visibility.Collapsed;
                MakeFileAppSettings.Visibility = Visibility.Collapsed;
                DeleteFolderAppSettings.Visibility = Visibility.Collapsed;
                _fileAppSettingsID = "none";
                _folderAppSettingsID = "none";




            }



            if (hasFolderPersonalFiles)
            {


                bool hasPersonalChildren = false;
                IChildrenCollectionPage PersonalChildren = null;


                try
                {
                    PersonalChildren = await _client
                                                 .Drive
                                                 .Items[_folderAppSettingsID]
                                                 .Children
                                                 .Request()
                                                 .GetAsync();
                    hasPersonalChildren = true;

                }
                catch
                {
                    hasPersonalChildren = false;


                }
                if (hasPersonalChildren)
                {
                    if (PersonalChildren.Count > 0)
                    {
                        foreach (var entity in PersonalChildren)
                        {
                            if (entity.Name == "PeronsalSettings.txt")
                            {
                                //We know the name of the folder, so we are letting the system know they need to offer a delete
                                DeleteFilePersonalFiles.Visibility = Visibility.Visible;
                                _filePersonalFilesID = entity.Id;
                                hasPersonalChildren = true;

                            }

                        }
                    }
                }
                else
                {


                    DeleteFilePersonalFiles.Visibility = Visibility.Collapsed;
                    MakeFilePersonalFiles.Visibility = Visibility.Visible;
                    _filePersonalFilesID = "none";



                }





            }
            else
            {
                //status.Text = "The personal files folder does not exist, you will need to click \"Make Personal Files Folder\" ";
                MakeFolderPersonalFiles.Visibility = Visibility.Visible;
                _filePersonalFilesID = "none";





            }
            return 1;


        }
    }
}


