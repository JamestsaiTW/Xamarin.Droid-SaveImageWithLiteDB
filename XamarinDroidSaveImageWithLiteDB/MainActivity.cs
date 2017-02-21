using System;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Widget;
using Android.OS;
using LiteDB;

namespace App1
{
    [Activity(Label = "App1", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private static string dbPath;

        private static string dbConnectionString;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            dbPath = System.IO.Path.Combine(FilesDir.Path, "MyData.db");
            dbConnectionString = $"Filename={dbPath}; Password=MyTestLiteDbPwd";

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);
            var mainImageView = FindViewById<ImageView>(Resource.Id.MainImageView);

            string fileKey = null;

            var saveImageFromUrlToLiteDbButton = FindViewById<Button>(Resource.Id.SaveImageFromUrlToLiteDbButton);
            saveImageFromUrlToLiteDbButton.Click += async  (sender, e) =>
            {
                fileKey = await SaveImageFromUrlToLiteDbAsync("https://xamarinclassdemo.azurewebsites.net/images/xamarin_logo5.png");
            };

            var loadImageFromLiteDbButton = FindViewById<Button>(Resource.Id.LoadImageFromLiteDbButton);
            loadImageFromLiteDbButton.Click += (sender, e) =>
            {
                using (var db = new LiteDatabase(dbConnectionString))
                {
                    if (fileKey == null)
                        fileKey = "Xamarin_Logo5.png".GetHashCode().ToString();

                    using (var myStream = db.FileStorage.OpenRead(fileKey))
                    {
                        mainImageView.SetImageBitmap(BitmapFactory.DecodeStream(myStream));
                    }
                }
            };

            var saveImageFromLiteDbToLocalByIdButton = FindViewById<Button>(Resource.Id.SaveImageFromLiteDbToLocalByIdButton);
            
            //若只已知id，要透過找到的檔案來決定寫出的檔名，可使用此方式寫出檔案
            saveImageFromLiteDbToLocalByIdButton.Click += async (sender, e) =>
            {
                using (var db = new LiteDatabase(dbConnectionString))
                {
                    if (fileKey == null)
                        fileKey = "Xamarin_Logo5.png".GetHashCode().ToString();

                    using (var myStream = db.FileStorage.OpenRead(fileKey))
                    {
                        using (var fileStream = System.IO.File.Create(System.IO.Path.Combine(FilesDir.Path, myStream.FileInfo.Filename)))
                        {
                            await myStream.CopyToAsync(fileStream);
                            Console.WriteLine($"{myStream.FileInfo.Filename} 寫出成功");
                        }
                    }
                }
            };


            //若是已知id、先確定寫出的File檔名，即可使用此方式寫出檔案
            var saveImageFromLiteDbToLocalByIdAndKnownFileNameButton = FindViewById<Button>(Resource.Id.SaveImageFromLiteDbToLocalByIdAndKnownFileNameButton);

            saveImageFromLiteDbToLocalByIdAndKnownFileNameButton.Click += (sender, e) =>
            {
                using (var db = new LiteDatabase(dbConnectionString))
                {
                    if (fileKey == null)
                        fileKey = "Xamarin_Logo5.png".GetHashCode().ToString();

                    using (var fileStream = System.IO.File.Create(System.IO.Path.Combine(FilesDir.Path, "Xamarin_Logo5.png")))
                    {
                        var liteDbFileInfo = db.FileStorage.Download(fileKey, fileStream);
                        Console.WriteLine($"{liteDbFileInfo.Filename} 寫出成功");
                    }
                }
            };
        }

        private static async Task<string> SaveImageFromUrlToLiteDbAsync(string url)
        {
            var fileKey = string.Empty;
            using (var httpClient = new HttpClient())
            {
                var downloadStream = await httpClient.GetStreamAsync(url);

                if (downloadStream != null && downloadStream.Length > 0)
                {
                    using (var db = new LiteDatabase(dbConnectionString))
                    {
                        fileKey = "Xamarin_Logo5.png".GetHashCode().ToString();
                        db.FileStorage.Upload(fileKey, "Xamarin_Logo5.png", downloadStream);
                    }
                }
            }
            return fileKey;
        }
    }
}

