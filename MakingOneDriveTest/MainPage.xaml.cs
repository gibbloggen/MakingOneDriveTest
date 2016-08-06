using Microsoft.OneDrive.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
            MakeFolderAppSettings.Visibility = Visibility.Collapsed;
            DeleteFolderAppSettings.Visibility = Visibility.Collapsed;
            MakeFileAppSettings.Visibility = Visibility.Collapsed;
            DeleteFileAppSettings.Visibility = Visibility.Collapsed;
            MakeFolderPersonalFiles.Visibility = Visibility.Collapsed;
            DeleteFolderPersonalFiles.Visibility = Visibility.Collapsed;
            MakeFilePersonalFiles.Visibility = Visibility.Collapsed;
            DeleteFilePersonalFiles.Visibility = Visibility.Collapsed;



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
                bool hasFolderAppSettings = false;
                bool hasFolderPersonalFiles = false;
                //folder Status
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

                        }else if (entity.Name == "PersonalFilesYoursToShare")
                        {
                            //We know the name of the folder, so we are letting the system know they need to offer a delete
                            hasFolderPersonalFiles = true;
                            _folderPersonalFilesID = entity.Id;

                        }



                    }

                }

                    status.Text = "";
                    status2.Text = "";




                    if (hasFolderPersonalFiles)
                    {
                        status.Text = "The personal files folder already exists, we can go forward or delete the folder";
                        DeleteFolderPersonalFiles.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        status.Text = "The personal files folder does not exist, you will need to click \"Make Personal Files Folder\" ";
                        MakeFolderPersonalFiles.Visibility = Visibility.Visible;


                    }

                if (hasFolderAppSettings)
                {
                    status2.Text = "The app settings files folder already exists, we can go forward or delete the folder";
                    DeleteFolderAppSettings.Visibility = Visibility.Visible;
                }
                else
                {
                    status2.Text = "The app settings files folder does not exist, you will need to click \"Make app settings Files Folder\" ";
                    MakeFolderAppSettings.Visibility = Visibility.Visible;


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
      
        private async void MakeFolderAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var folderToCreate = new Item { Name = "AppSettingsDoNotTouch", Folder = new Folder() };
            var newFolder = await _client.Drive.Special.AppRoot.Children.Request().AddAsync(folderToCreate);
            status.Text = "The App Settings folder has been created. ";
            //status2.Text = "EssentialSoftwareProducts Folder created in App Root with id of: " + newFolder.Id.ToString();
            _folderAppSettingsID = newFolder.Id;
            DeleteFolderAppSettings.Visibility = Visibility.Visible;
            MakeFolderAppSettings.Visibility = Visibility.Collapsed;
            MakeFileAppSettings.Visibility = Visibility.Visible;
            DeleteFileAppSettings.Visibility = Visibility.Collapsed;
        }

        private async void DeleteFolderAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {

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

            if (hasFolder)
            {
                await _client
                  .Drive
                  .Items[_folderAppSettingsID]
                  .Request()
                  .DeleteAsync()
                  ;
                status.Text = "The App Settings Folder has been deleted,,,,";
               // status2.Text = "EssentialSoftwareProducts Folder deleted in App Root with id of: " + _folderID;
            }
            else
            {
                status.Text = "App Settings Folder Not found, cannot delete!";
                //status2.Text = "You can create it again though,,,";
            }



            DeleteFolderAppSettings.Visibility = Visibility.Collapsed;
            MakeFolderAppSettings.Visibility = Visibility.Visible;
            MakeFileAppSettings.Visibility = Visibility.Collapsed;
            DeleteFileAppSettings.Visibility = Visibility.Collapsed;

        }

        private async void MakeFileAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
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
            MakeFileAppSettings.Visibility = Visibility.Collapsed;

        }

        private async void DeleteFileAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await _client
                  .Drive
                  .Items[_fileAppSettingsID]
                  .Request()
                  .DeleteAsync();

            MakeFileAppSettings.Visibility = Visibility.Visible;
            DeleteFileAppSettings.Visibility = Visibility.Collapsed;

        }

        private async void MakeFolderPersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var folderToCreate = new Item { Name = "PersonalFilesYoursToShare", Folder = new Folder() };
            var newFolder = await _client.Drive.Special.AppRoot.Children.Request().AddAsync(folderToCreate);
            status.Text = "The Personal Files folder has been created. ";
            //status2.Text = "EssentialSoftwareProducts Folder created in App Root with id of: " + newFolder.Id.ToString();
            _folderPersonalFilesID = newFolder.Id;
            DeleteFolderPersonalFiles.Visibility = Visibility.Visible;
            MakeFolderPersonalFiles.Visibility = Visibility.Collapsed;
            MakeFilePersonalFiles.Visibility = Visibility.Visible;
            DeleteFilePersonalFiles.Visibility = Visibility.Collapsed;

        }

        private async void DeleteFolderPersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {

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

            }

            if (hasFolder)
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



            DeleteFolderPersonalFiles.Visibility = Visibility.Collapsed;
            MakeFolderPersonalFiles.Visibility = Visibility.Visible;
            DeleteFilePersonalFiles.Visibility = Visibility.Collapsed;
            MakeFilePersonalFiles.Visibility = Visibility.Collapsed;


        }

        private async void MakeFilePersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
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
           // g.Flush();
           // z.Close();
           


            //g.Dispose();
           // g.Flush();
            _filePersonalFilesID = item.Id;
            MakeFilePersonalFiles.Visibility = Visibility.Collapsed;
            DeleteFilePersonalFiles.Visibility = Visibility.Visible;



        }

        private async void DeleteFilePersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await _client
                  .Drive
                  .Items[_filePersonalFilesID]
                  .Request()
                  .DeleteAsync();

            MakeFilePersonalFiles.Visibility = Visibility.Visible;
            DeleteFilePersonalFiles.Visibility = Visibility.Collapsed;

        }
    }
    }



