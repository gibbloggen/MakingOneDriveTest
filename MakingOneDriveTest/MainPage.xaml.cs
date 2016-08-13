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
//      Company To Be Created: Essential Software Products  http://essentialsoftwareproducts.org
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
                    "onedrive.readwrite", // Commented out, thinking this was too much clearance, actually, it requires this much clearance.
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
                    //this catch will happen if they deny the app rights.

                    var failureDialog = new MessageDialog(
       $"You have not been authenticated, you cannot use this program without being Authenticated.",
       "Not Authenticated!");
                    await failureDialog.ShowAsync();
                    return;


                }
                //This will happen if they approve rights.  They only have to approve them the first time.

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
                MyScrollViewer.Height = j - 35;
                MainGrid.Height = j;
                MainStackPanel.Height = j + 425;
            }
            else if ((j < 500) && (jj > 500))
            {
                MyScrollViewer.Height = j - 35;
                MainStackPanel.Height = jj + 425;
                MainGrid.Height = j;

            }
            else
            {
                MyScrollViewer.Height = j - 35;
                MainStackPanel.Height = j + 275;
                MainGrid.Height = j;

            }
            return;

        }

        private async void MakeFolderAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //this whatbuttons method is for multi-user synching.  It confirms that neither another instance of the app
            // nor user interventions on the folder itself have happened.  Depending on what these are, _folderAppSettings ID
            // is either a folder ID or the word none, we are looking for none, if not, it aborts after setting the proper buttons.
            await WhatButtons();
            if (_folderAppSettingsID == "none")
            {
                var folderToCreate = new Item { Name = "AppSettingsDoNotTouch", Folder = new Folder() };
                var newFolder = await _client.Drive.Special.AppRoot.Children.Request().AddAsync(folderToCreate);
                status.Text = "The App Settings folder has been created. ";
                //status2.Text = "EssentialSoftwareProducts Folder created in App Root with id of: " + newFolder.Id.ToString();
                _folderAppSettingsID = newFolder.Id;
                MakeFileAppSettings.IsEnabled = true;
                MakeFolderAppSettings.IsEnabled = false;
                DeleteFolderAppSettings.IsEnabled = true;
            }
            else
            {
                MakeFileAppSettings.IsEnabled = true;
                MakeFolderAppSettings.IsEnabled = false;
                DeleteFolderAppSettings.IsEnabled = true;
            }
        

        }

        private async void DeleteFolderAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // This is the inverse of the MakeFolder method, here it is looking for a folder id.
            //of note, even if another instance of the app has created this file.
            // the whatbuttons routine will populate it with the appropriate ID

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
                ReadAppSettings.IsEnabled = false;
                UpdateAppSettings.IsEnabled = false;
                MakeFileAppSettings.IsEnabled = false;
                DeleteFileAppSettings.IsEnabled = false;
                DeleteFolderAppSettings.IsEnabled = false;
                MakeFolderAppSettings.IsEnabled = true;
            
            }
            else
            {
                status.Text = "App Settings Folder Not found, cannot delete!";
                ReadAppSettings.IsEnabled = false;
                  DeleteFolderAppSettings.IsEnabled = false;
                UpdateAppSettings.IsEnabled = false;
                MakeFileAppSettings.IsEnabled = false;
                DeleteFileAppSettings.IsEnabled = false;
                MakeFolderAppSettings.IsEnabled = true;
                
            }


        
        }

        private async void MakeFileAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {

            //This routine creates the file by working with windows storage.  I later found that I can do the same thing with a memory stream.
            // Only doc'ing the code now, but that might be something to update.
            await WhatButtons();
            if (_fileAppSettingsID == "none")
            {

                StorageFile fileToCreate = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("AppSettings.txt", CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(fileToCreate, "Setting A");

                string q = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\AppSettings.txt";
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
          

                _fileAppSettingsID = item.Id;
                DeleteFileAppSettings.IsEnabled = true;
                UpdateAppSettings.IsEnabled = true;
                ReadAppSettings.IsEnabled = true;
                MakeFileAppSettings.IsEnabled = false;

            }


        }

        private async void DeleteFileAppSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //similar to delete folder, only used on a file.
            await WhatButtons();
            if (_fileAppSettingsID != "none")
            {
                await _client
                      .Drive
                      .Items[_fileAppSettingsID]
                      .Request()
                      .DeleteAsync();
                DeleteFileAppSettings.IsEnabled = false;
                MakeFileAppSettings.IsEnabled = true;
                UpdateAppSettings.IsEnabled = false;
                ReadAppSettings.IsEnabled = false;
            } else
            {
                DeleteFileAppSettings.IsEnabled = false;
                MakeFileAppSettings.IsEnabled = true;
                UpdateAppSettings.IsEnabled = false;
                ReadAppSettings.IsEnabled = false;

            }
            //await WhatButtons();
        }

        private async void MakeFolderPersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //This is virtually the same as the make folder for appsettings.  Something that would probably be nice to do
            // and more proper, to put them in one method, but just pass them a parameter or two.

            await WhatButtons();
            if (_folderPersonalFilesID == "none")
            {
                var folderToCreate = new Item { Name = "PersonalFilesYoursToShare", Folder = new Folder() };
                var newFolder = await _client.Drive.Special.AppRoot.Children.Request().AddAsync(folderToCreate);
                status.Text = "The Personal Files folder has been created. ";
                _folderPersonalFilesID = newFolder.Id;
                MakeFolderPersonalFiles.IsEnabled = false;
                DeleteFolderPersonalFiles.IsEnabled = true;
                DeleteFilePersonalFiles.IsEnabled =false;
                MakeFilePersonalFiles.IsEnabled = true;

            } else
            {
                MakeFolderPersonalFiles.IsEnabled = false;
                DeleteFolderPersonalFiles.IsEnabled = true;
                DeleteFilePersonalFiles.IsEnabled = false;
                MakeFilePersonalFiles.IsEnabled = true;

            }
         
        }

        private async void DeleteFolderPersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
           //  Another similar to the DeleteAppSettingsFolder.  Again, this would probably benefit from combining, especially true if you have more than
           // 2 folders to create or delete.


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
                
            }
            else
            {
                status.Text = "Personal Files Folder Not found, cannot delete!";
             
            }

            await WhatButtons();

        }

        private async void MakeFilePersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {

            //same as the MakeFileAppSettings routine, could benefit from,,,,


            await WhatButtons();
            if (_filePersonalFilesID == "none")
            {
                StorageFile fileToCreate = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("PersonalFiles.txt", CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(fileToCreate, "Personal A");
               

                
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

                DeleteFilePersonalFiles.IsEnabled = true;
                MakeFilePersonalFiles.IsEnabled = false;
                UpdatePersonalFiles.IsEnabled = true;
                ReadPersonalFiles.IsEnabled = true;

            }else
            {
                DeleteFilePersonalFiles.IsEnabled = false;
                MakeFilePersonalFiles.IsEnabled = true;
                UpdatePersonalFiles.IsEnabled = false;
                ReadPersonalFiles.IsEnabled =  false;
            }
          
        }

        private async void DeleteFilePersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {

            //This routine has a bug in it, it should also reset Read and Update Personal Files to disabled.

            await WhatButtons();
            if (_filePersonalFilesID != "none")
            {
                await _client
                      .Drive
                      .Items[_filePersonalFilesID]
                      .Request()
                      .DeleteAsync();
                DeleteFilePersonalFiles.IsEnabled = false;
                MakeFilePersonalFiles.IsEnabled =true;
            } else
            {
                DeleteFilePersonalFiles.IsEnabled = true;
                MakeFilePersonalFiles.IsEnabled = false;
            }
         

        }
        private async void ReadPersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await WhatButtons();
            if (_filePersonalFilesID != "none")
            {
                var contentStream = await _client
                                  .Drive
                                  .Items[_filePersonalFilesID]
                                  .Content
                                  .Request()
                                  .GetAsync();


                int r = (int)contentStream.Length;
                byte[] q = new byte[r];

                await contentStream.ReadAsync(q, 0, r);

                var str = System.Text.Encoding.ASCII.GetString(q);

                var PersonalFilesDialog = new MessageDialog(
    "Your personal file is,,,," + str,
    "Peronsal Files");
                await PersonalFilesDialog.ShowAsync();

            }

        }

        private async void UpdatePersonalFiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //These Update Methods use the memory stream, I want to look into this further.
            //This actually may be propogated to more areas when I put this in Essential Grocer App.

            await WhatButtons();

            if (_filePersonalFilesID != "none")
            {
                var contentStream = await _client
                               .Drive
                               .Items[_filePersonalFilesID]
                               .Content
                               .Request()
                               .GetAsync();


                int r = (int)contentStream.Length;
                byte[] q = new byte[r];

                await contentStream.ReadAsync(q, 0, r);

                var str = System.Text.Encoding.ASCII.GetString(q);


                string t = "WTF";
                if (str == "Personal A")
                {
                    t = "Personal B";
                }
                else if (str == "Personal B")
                {
                    t = "Personal C";
                }
                else if (str == "Personal C")
                {
                    t = "Personal A";
                }

                byte[] z = new byte[(int)t.Length];
                z = System.Text.Encoding.ASCII.GetBytes(t);
                System.IO.MemoryStream streamitSoft = new MemoryStream(z);
                // await streamitSoft.WriteAsync(z, 0, z.Length);
                ///StreamWriter writeitbaby = new StreamWriter(streamitSoft);


                //await writeitbaby.WriteAsync(q);


                var uploadedItem = await _client
                                             .Drive
                                             .Items[_filePersonalFilesID]
                                             .Content
                                             .Request()
                                             .PutAsync<Item>(streamitSoft);



                var PersonalFilesDialog = new MessageDialog(
    "Your personal file has been changed to,,,," + t,
    "Personal File");
                await PersonalFilesDialog.ShowAsync();
            }


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

        //this is collapse all, should be disable all.  In an earlier incarnation it was actually
        //collapsing everything, in the latest version they are still visible, just disabled.
        private void Collapse_All()
        {
            MakeFolderAppSettings.IsEnabled = false;
            DeleteFolderAppSettings.IsEnabled = false;
            MakeFileAppSettings.IsEnabled = false;
            DeleteFileAppSettings.IsEnabled = false;
            MakeFolderPersonalFiles.IsEnabled = false;
            DeleteFolderPersonalFiles.IsEnabled = false;
            MakeFilePersonalFiles.IsEnabled = false;
            DeleteFilePersonalFiles.IsEnabled = false;
            ReadAppSettings.IsEnabled = false;
            UpdateAppSettings.IsEnabled = false;
            UpdatePersonalFiles.IsEnabled = false;
            ReadPersonalFiles.IsEnabled = false;


        }


        //This is a long one, but it represents how this program operates Multi-Client.  A very important ingredient,
        // and the whole point of working with OneDrive.

        private async Task<int> WhatButtons()
        {
            Collapse_All();  // Start with all buttons disabled.

            //First volley set the booleans so we will know if the folders exist.

            bool hasFolderAppSettings = false;
            bool hasFolderPersonalFiles = false;
            //folder Status

            //These are all my id flags, these are globals.  The default "none" signals to the rest of the 
            //program that they can write these objects, when they have a oneDrive ID, it signals
            //that they can be built upon, read or updated.
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
                //if we got nothing, set it  up to build the folders.
                hasFolderAppSettings = false;
                hasFolderPersonalFiles = false;
                MakeFolderAppSettings.IsEnabled = true;
                MakeFolderPersonalFiles.IsEnabled = true;
                
                return 1;

            }

            if (item.Count == 0)
            {
                hasFolderAppSettings = false;
                hasFolderPersonalFiles = false;
                MakeFolderAppSettings.IsEnabled = true;
                MakeFolderPersonalFiles.IsEnabled = true;
            }
            else
            {

                //This is another area with some stuff hardcoded, like folder and later file names.
                //Going forward this should be getting this info from a config of some sort.
                //Or it could just be globals with the names, so you would change them at 
                //the top of the program and it would propogate.
                foreach (var entity in item)
                {
                    if (entity.Name == "AppSettingsDoNotTouch")
                    {
                        //We know the name of the folder, so we are letting the system know they need to offer a delete
                        hasFolderAppSettings = true;
                        _folderAppSettingsID = entity.Id;
                        DeleteFolderAppSettings.IsEnabled = true;

                    }
                    else if (entity.Name == "PersonalFilesYoursToShare")
                    {
                        //We know the name of the folder, so we are letting the system know they need to offer a delete
                        hasFolderPersonalFiles = true;
                        _folderPersonalFilesID = entity.Id;
                        DeleteFolderPersonalFiles.IsEnabled = true;

                    }



                }

            }

            status.Text = "";
            status2.Text = "";


            //This is a path for the AppSettings folder, another redundency that could be handled more for the generic.

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

                 //Once we know it has children, then check the name (Generic?)
                 //This is required, in case someone/something else write to the folder.  
                 //We are only interested in our file.
                if (hasAppChildren)
                {
                    if (AppChildren.Count > 0)
                    {
                        foreach (var entity in AppChildren)
                        {
                            if (entity.Name == "AppSettings.txt")
                            {
                                //We know the name of the folder, so we are letting the system know they need to offer a delete
                                DeleteFileAppSettings.IsEnabled = true;
                                UpdateAppSettings.IsEnabled = true;
                                ReadAppSettings.IsEnabled = true;
                                _fileAppSettingsID = entity.Id;
                                hasAppChildren = true;
                                

                            }

                        }
                    }
                    else
                    {


                        DeleteFileAppSettings.IsEnabled = false;
                        UpdateAppSettings.IsEnabled = false;
                        ReadAppSettings.IsEnabled = false;
                        MakeFileAppSettings.IsEnabled = true;
                        _fileAppSettingsID = "none";



                    }
                }
                else
                {


                    DeleteFileAppSettings.IsEnabled = false;
                    UpdateAppSettings.IsEnabled = false;
                    ReadAppSettings.IsEnabled = false;
                    MakeFileAppSettings.IsEnabled = true;
                    _fileAppSettingsID = "none";



                }





            }
            else
            {
                //status.Text = "The personal files folder does not exist, you will need to click \"Make Personal Files Folder\" ";
                MakeFolderAppSettings.IsEnabled = true;
                DeleteFileAppSettings.IsEnabled = false;
                MakeFileAppSettings.IsEnabled = false;
                DeleteFolderAppSettings.IsEnabled = false;
                _fileAppSettingsID = "none";
                _folderAppSettingsID = "none";




            }


            //this is the redundent for Personal Files, same drill,,,
            if (hasFolderPersonalFiles)
            {


                bool hasPersonalChildren = false;
                IChildrenCollectionPage PersonalChildren = null;


                try
                {
                    PersonalChildren = await _client
                                                 .Drive
                                                 .Items[_folderPersonalFilesID]
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
                            if (entity.Name == "PersonalFiles.txt")
                            {
                                //We know the name of the folder, so we are letting the system know they need to offer a delete
                                DeleteFilePersonalFiles.IsEnabled = true;
                                UpdatePersonalFiles.IsEnabled = true;
                                ReadPersonalFiles.IsEnabled = true;
                                _filePersonalFilesID = entity.Id;
                                hasPersonalChildren = true;

                            }

                        }
                    }
                    else {

                        DeleteFilePersonalFiles.IsEnabled = false;
                        UpdatePersonalFiles.IsEnabled = false;
                        ReadPersonalFiles.IsEnabled = false;
                        MakeFilePersonalFiles.IsEnabled = true;
                        _filePersonalFilesID = "none";

                    }
                }
                else
                {

                    DeleteFilePersonalFiles.IsEnabled = false;
                    UpdatePersonalFiles.IsEnabled = false;
                    ReadPersonalFiles.IsEnabled = false;
                    MakeFilePersonalFiles.IsEnabled = true;
                    _filePersonalFilesID = "none";


                }





            }
            else
            {
                //status.Text = "The personal files folder does not exist, you will need to click \"Make Personal Files Folder\" ";
                MakeFolderPersonalFiles.IsEnabled = true;
                DeleteFilePersonalFiles.IsEnabled = false;
                MakeFilePersonalFiles.IsEnabled = false;
                DeleteFolderPersonalFiles.IsEnabled = false;
                _filePersonalFilesID = "none";
                _folderPersonalFilesID = "none";




            }
            return 1;


        }

   
    }
}


