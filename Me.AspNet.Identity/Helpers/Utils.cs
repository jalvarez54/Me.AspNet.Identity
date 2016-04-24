using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Net.Mail;
using System.Text;
using System.Security.Cryptography;

namespace JA.Helpers
{
    public class Utils
    {
        public static Me.AspNet.Identity.Models.ApplicationUser CurrentUserObject(string UserId)
        {
            var userManager = new UserManager<Me.AspNet.Identity.Models.ApplicationUser>
                (new Microsoft.AspNet.Identity.EntityFramework.UserStore<Me.AspNet.Identity.Models.ApplicationUser>
                (new Me.AspNet.Identity.Models.ApplicationDbContext()));

            // Get the current logged in User and look up the user in ASP.NET Identity
            var currentUser = userManager.FindById(UserId);

            return currentUser;

        }

        public static string AppPath()
        {

            string appPath = "";
            var a = HttpRuntime.AppDomainAppVirtualPath;
            if (a == "/")
            {
                appPath = "";
            }
            else
            {
                appPath = a;
            }
            return appPath;
        }

        /// <summary>
        /// 2005/04/17
        /// Gets the currently active Gravatar image URL for the email address supplied to this method call
        /// Throws a <see cref="Gravatar.NET.GravatarEmailHashFailedException"/> if the provided email address is invalid
        /// </summary>
        /// <param name="address">The address to retireve the image for</param>
        /// /// <param name="pars">The available parameters passed by the request to Gravatar when retrieving the image</param>
        /// <returns>The Gravatar image URL</returns>
        public static string GetGravatarUrlForAddress(string address)
        {

            const string GRAVATAR_URL_BASE = "http://s.gravatar.com/avatar/";
            var sbResult = new StringBuilder(GRAVATAR_URL_BASE);
            sbResult.Append(HashString(address));
            sbResult.Append("?");
            // https://ja.gravatar.com/site/implement/images/
            string defaultGravatarImageType = GetAppSetting("DefaultGravatarImageType");
            sbResult.Append("d=");
            sbResult.Append(defaultGravatarImageType);
            sbResult.Append("&");
            sbResult.Append("s=100");


            return sbResult.ToString();
        }

        /// <summary>
        /// 2005/04/17
        /// Hashes a string with MD5.  Suitable for use with Gravatar profile
        /// image urls
        /// </summary>
        /// <param name="myString"></param>
        /// <returns></returns>
        public static string HashString(string myString)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.  
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.  
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(myString));

            // Create a new Stringbuilder to collect the bytes  
            // and create a string.  
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string.  
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();  // Return the hexadecimal string. 
        }


        public static void MakeCategoryFolder(string category, System.Web.Mvc.Controller controller)
        {
            if (category != string.Empty)
            {
                var folder = controller.Server.MapPath("~/Medias/_Photos/" + category);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

        }
        public static void MakeSubCategoryFolder(string category, string subCategory, System.Web.Mvc.Controller controller)
        {
            if (category != string.Empty)
            {
                if (subCategory != string.Empty)
                {
                    var folder = controller.Server.MapPath("~/Medias/_Photos/" + category + "/" + subCategory);
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                }
            }

        }
        public static string GetGlobalValue(HttpContextBase cela, string key)
        {
            try
            {
                return cela.ApplicationInstance.Application.Get(key).ToString();
            }
            catch (Exception)
            {

            }
            return null;
        }

        public static string GetCountGuessBooks(HttpContextBase cela)
        {
            try
            {
                return cela.ApplicationInstance.Application.Get("CountGuessBooks").ToString();
            }
            catch (Exception)
            {

            }
            return null;
        }
        public static string GetCountMembers(HttpContextBase cela)
        {
            try
            {
                return cela.ApplicationInstance.Application.Get("CountMembers").ToString();
            }
            catch (Exception)
            {

            }
            return null;
        }
        public static void SendMail(string body, string subject, string firstName, string lastName, string email)
        {
            string myBody = string.Empty;
            string mySubject = string.Empty;

            if (body == "") { myBody = ConfigurationManager.AppSettings["cdf54.DefaultBody"].ToString(); } else { myBody = body; };
            if (subject == "") { 
                mySubject = ConfigurationManager.AppSettings["cdf54.DefaultSubject"].ToString();
                myBody = string.Format("{0}: {1} {2} avec email: {3}  [ Ne pas répondre à ce mail ]", myBody, firstName, lastName, email);
            } 
            else 
            { 
                mySubject = subject; 
            };
            try
            {
                using (var client = new SmtpClient())
                {
                    var msg = new MailMessage()
                    {
                        Body = myBody,
                        Subject = mySubject
                    };

                    string[] toUsers = ConfigurationManager.AppSettings["cdf54.EmailsRegistration"].ToString().Split(',');
                    foreach (string destination in toUsers)
                    {
                        msg.To.Add(destination);
                    }
                    client.Send(msg);
                    System.Diagnostics.Debug.WriteLine(msg);
                }
           }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Save photo to disk, used by Edit and Register with two different models.
        /// </summary>
        /// <param name="photo">HttpPostedFileWrapper</param>
        /// <param name="controller">Controller calling</param>
        /// <returns>Path where photo is stored with it's calculated filename, or default photo "BlankPhoto.jpg" or null on error</returns>
        public static string SavePhotoFileToDisk(object myphoto, System.Web.Mvc.Controller controller, string oldPhotoUrl, bool isNoPhotoChecked)
        {
            HttpPostedFileBase photo = (HttpPostedFileBase)myphoto;

            string photoPath = string.Empty;
            string fileName = string.Empty;

            // If photo is uploaded calculate his name
            if (photo != null)
            {
                fileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
            }
            else
            {
                // if user want to remove his photo
                if (oldPhotoUrl != null && isNoPhotoChecked == true)
                {
                    if (!oldPhotoUrl.Contains("BlankPhoto.jpg"))
                    {
                        string fileToDelete = Path.GetFileName(oldPhotoUrl);
                        var path = Path.Combine(controller.Server.MapPath("~/Content/Avatars"), fileToDelete);
                        FileInfo fi = new FileInfo(path);
                        if (fi.Exists)
                            fi.Delete();
                    }
                }

                // If no previews photo it's a new user who don't provide photo
                if (oldPhotoUrl == null || isNoPhotoChecked == true)
                {
                    fileName = "BlankPhoto.jpg";
                }
                else
                {
                    // User don't want to change his photo
                    return oldPhotoUrl;
                }
            }
            // We save the new/first photo on disk
            try
            {
                string path;
                path = Path.Combine(controller.Server.MapPath("~/Content/Avatars"), fileName);
                photoPath = Path.Combine(HttpRuntime.AppDomainAppVirtualPath, "Content/Avatars", fileName);
                // We save the new/first photo or nothing because BlankPhoto is in the folder
                if (photo != null) photo.SaveAs(path);
            }
            catch (Exception ex)
            {
                // Handled exception catch code
                Helpers.Utils.SignalExceptionToElmahAndTrace(ex, controller);
                return null;
            }
            return photoPath;
        }

        /// <summary>
        /// Save photo to disk, used by Edit and Register with two different models. model.Photo, this, string model.CategoryName, string model.SubCategoryName
        /// </summary>
        /// <param name="photo">HttpPostedFileWrapper</param>
        /// <param name="controller">Controller calling</param>
        /// <returns>Path where photo is stored with it's calculated filename, or default photo "BlankPhoto.jpg" or null on error</returns>
        //public static string[] SaveAcdfPhotoFileToDisk(System.Web.Helpers.WebImage webImage, Me.AspNet.Identity.Controllers.PhotoController controller, string categoryName, string subCategoryName)
        //{
        //    string[] paths = new string[2];
        //    string photoPath = string.Empty;
        //    string thumbPath = string.Empty;
        //    string fileName = string.Empty;
        //    string thumbFileName = string.Empty;
        //    string subCatName = subCategoryName.Contains("AUCUNE") ? string.Empty : subCategoryName;


        //    if (webImage != null)
        //    {
        //        // If photo is uploaded calculate his name
        //        string guid = Guid.NewGuid().ToString();
        //        fileName = guid + "_" + webImage.FileName;
        //        thumbFileName = guid + "_thumb_" + webImage.FileName;
        //        //Calculate photoUrl and save photo on disk
        //        try
        //        {
        //            string path = @"/ACDF/Medias\_Photos\" + categoryName + @"\" + subCatName;
        //            //string path = @"/ACDF/Medias\_Photos\";

        //            string filePath = Path.Combine(controller.Server.MapPath(path), fileName);
        //            webImage.AddTextWatermark("http://jow-alva.net/ACDF", "White", 15);
        //            webImage.Save(filePath);
        //            photoPath = Path.Combine(HttpRuntime.AppDomainAppVirtualPath, path, fileName);

        //            string thumbFilePath = Path.Combine(controller.Server.MapPath(path), thumbFileName);
        //            webImage.Resize(150, 150, preserveAspectRatio: true).Save(thumbFilePath);
        //            thumbPath = Path.Combine(HttpRuntime.AppDomainAppVirtualPath, path, thumbFileName);

        //            paths[0] = photoPath;
        //            paths[1] = thumbPath;

        //        }
        //        catch (Exception ex)
        //        {
        //            // Handled exception catch code
        //            Helpers.Utils.SignalExceptionToElmahAndTrace(ex, controller);
        //            return null;
        //        }
        //    }
        //    else
        //    {
        //        // Handled exception catch code
        //        //TODO:         Helpers.Utils.SignalExceptionToElmahAndTrace(null, controller);
        //        return null;
        //    }
        //    return paths;
        //}

        /// <summary>
        /// 2005/05/15
        /// Save photo to disk.
        /// </summary>
        /// <param name="myphoto">string</param>
        /// <param name="controller">Controller calling</param>
        /// <returns>Path where photo is stored with it's calculated filename.Or null on error.</returns>
        public static string SaveBytesPhotoFileToDisk(Object myphoto, System.Web.Mvc.Controller controller, string oldPhotoUrl, bool isNoPhotoChecked)
        {

            string dump = (string)myphoto;

            string photoPath = string.Empty;
            string fileName = string.Empty;

            // If photo is uploaded calculate his name
            if (dump != null)
            {
                fileName = Guid.NewGuid().ToString() + ".jpg";
            }

            try
            {
                //Delete old file if exist.
                if (oldPhotoUrl != null)
                {
                    if (!oldPhotoUrl.Contains("BlankPhoto.jpg"))
                    {
                        string fileToDelete = Path.GetFileName(oldPhotoUrl);
                        var path = Path.Combine(controller.Server.MapPath("~/Content/Avatars"), fileToDelete);
                        photoPath = Path.Combine(HttpRuntime.AppDomainAppVirtualPath, "Content/Avatars", fileName);
                        FileInfo fi = new FileInfo(path);
                        if (fi.Exists)
                            fi.Delete();
                    }
                }
                // We save the photo on disk
                var p = Path.Combine(controller.Server.MapPath("~/Content/Avatars"), fileName);
                System.IO.File.WriteAllBytes(p, String_To_Bytes2(dump));
                photoPath = Path.Combine(HttpRuntime.AppDomainAppVirtualPath, "Content/Avatars", fileName);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                // Handled exception catch code
                //Helpers.Utils.SignalExceptionToElmahAndTrace(ex, controller);
                return null;
            }
            return photoPath;
        }
        /// <summary>
        /// 2005/05/15
        /// 
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static byte[] String_To_Bytes2(string strInput)
        {
            int numBytes = (strInput.Length) / 2;
            byte[] bytes = new byte[numBytes];

            for (int x = 0; x < numBytes; ++x)
            {
                bytes[x] = System.Convert.ToByte(strInput.Substring(x * 2, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// Save photo to disk, used by Edit and Register with two different models. model.Photo, this, string model.CategoryName, string model.SubCategoryName
        /// </summary>
        /// <param name="photo">HttpPostedFileWrapper</param>
        /// <param name="controller">Controller calling</param>
        /// <returns>Path where photo is stored with it's calculated filename, or default photo "BlankPhoto.jpg" or null on error</returns>
        public static string[] SaveAcdfPhotoFileToDisk(System.Web.Helpers.WebImage webImage, System.Web.Mvc.Controller controller, string categoryName, string subCategoryName)
        {
            string[] paths = new string[2];
            string photoPath = string.Empty;
            string thumbPath = string.Empty;
            string fileName = string.Empty;
            string thumbFileName = string.Empty;
            string subCatName = subCategoryName.Contains("AUCUNE") ? string.Empty : subCategoryName;


            if (webImage != null)
            {
                // If photo is uploaded calculate his name
                string guid = Guid.NewGuid().ToString();
                fileName = guid + "_" + webImage.FileName;
                thumbFileName = guid + "_thumb_" + webImage.FileName;
                //Calculate photoUrl and save photo on disk
                try
                {
                    string path = HttpContext.Current.Request.ApplicationPath + "/Medias/_Photos/" + categoryName + @"\" + subCatName;
                    //string path = @"~/Medias\_Photos\" + categoryName + @"\" + subCatName;
                    //string path = @"/ACDF/Medias\_Photos\";

                    string filePath = Path.Combine(controller.Server.MapPath(path), fileName);
                    webImage.AddTextWatermark("http://jow-alva.net/ACDF", "White", 15);
                    webImage.Save(filePath);
                    photoPath = Path.Combine(HttpRuntime.AppDomainAppVirtualPath, path, fileName);

                    string thumbFilePath = Path.Combine(controller.Server.MapPath(path), thumbFileName);
                    webImage.Resize(150, 150, preserveAspectRatio: true).Save(thumbFilePath);
                    thumbPath = Path.Combine(HttpRuntime.AppDomainAppVirtualPath, path, thumbFileName);

                    paths[0] = photoPath;
                    paths[1] = thumbPath;

                }
                catch (Exception ex)
                {
                    // Handled exception catch code
                    Helpers.Utils.SignalExceptionToElmahAndTrace(ex, controller);
                    throw ex;
                    //return null;
                }
            }
            else
            {
                // Handled exception catch code
                //TODO:         Helpers.Utils.SignalExceptionToElmahAndTrace(null, controller);
                return null;
            }
            return paths;
        }

        /// <summary>
        /// This function is used to check specified file being used or not
        /// http://dotnet-assembly.blogspot.fr/2012/10/c-check-file-is-being-used-by-another.html
        /// </summary>
        /// <param name="file">FileInfo of required file</param>
        /// <returns>If that specified file is being processed 
        /// or not found is return true</returns>
        public static Boolean IsFileLocked(string file)
        {
            FileStream stream = null;
            try
            {
                //Don't change FileAccess to ReadWrite, 
                //because if a file is in readOnly, it fails.
                stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            //file is not locked
            return false;
        }

        public static void SignalExceptionToElmahAndTrace(Exception ex, System.Web.Mvc.Controller lui)
        {
            MyTracer.MyTrace(
                System.Diagnostics.TraceLevel.Error,
                lui.GetType(),
                lui.ControllerContext.RouteData.Values["controller"].ToString(),
                lui.ControllerContext.RouteData.Values["action"].ToString(),
                ex.Message, ex);

            //Elmah.ErrorSignal.FromCurrentContext().Raise(ex); //ELMAH Signaling
        }


        // http://www.nullskull.com/a/10450951/aspnet-mvc-display-images-directly-from-the-viewmodel-into-your-views.aspx
        public static String ByteToStringImage(byte[] picture)
        {
            byte[] photo = picture;
            string imageSrc = string.Empty;
            if (photo != null)
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(photo, 78, photo.Length - 78); // strip out 78 byte OLE header (don't need to do this for normal images)
                string imageBase64 = Convert.ToBase64String(ms.ToArray());
                imageSrc = string.Format("data:image/png;base64,{0}", imageBase64);
            }
            return imageSrc;
        }
        public static string GetAssemblyName()
        {
            Assembly assembly = HttpContext.Current.ApplicationInstance.GetType().BaseType.Assembly;
            return assembly.GetName().Name;
        }
        public static DateTime GetAssemblyDateTime()
        {
            Assembly assembly = HttpContext.Current.ApplicationInstance.GetType().BaseType.Assembly;
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(assembly.Location);
            DateTime lastModified = fileInfo.LastWriteTime;
            return lastModified;
        }
        public static string GetAssemblyInformationnalVersion()
        {
            Assembly assembly = HttpContext.Current.ApplicationInstance.GetType().BaseType.Assembly;
            return System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
        }
        public static string GetAssemblyVersion()
        {
            Assembly assembly = HttpContext.Current.ApplicationInstance.GetType().BaseType.Assembly;
            return assembly.GetName().Version.ToString();
        }
        public static string GetAssemblyProduct()
        {
            Assembly assembly = HttpContext.Current.ApplicationInstance.GetType().BaseType.Assembly;
            AssemblyProductAttribute assemblyProductAttribut = (AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));
            return assemblyProductAttribut.Product;
        }
        public static string GetAssemblyCompany()
        {
            Assembly assembly = HttpContext.Current.ApplicationInstance.GetType().BaseType.Assembly;
            AssemblyCompanyAttribute assemblyCompanyAttribute = (AssemblyCompanyAttribute)(Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute), false));
            return assemblyCompanyAttribute.Company;
        }
        public static string GetAssemblyCopyright()
        {
            Assembly assembly = HttpContext.Current.ApplicationInstance.GetType().BaseType.Assembly;
            AssemblyCopyrightAttribute assemblyCompanyAttribute = (AssemblyCopyrightAttribute)(Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute), false));
            return assemblyCompanyAttribute.Copyright;
        }
        public static string GetConfigCompanyName()
        {
            return ConfigurationManager.AppSettings["cdf54.CompanyName"].ToString();
        }
        public static string GetConfigCompanyAddress()
        {
            return ConfigurationManager.AppSettings["cdf54.CompanyAddress"].ToString();
        }
        public static string GetGoogleMapKey()
        {
            return ConfigurationManager.AppSettings["cdf54.GoogleMapKey"].ToString();
        }
        public static string GetUserName()
        {
            string returned = HttpContext.Current.User.Identity.Name;
            returned = returned == "" ? "Guest" : returned;
            return returned;
        }
        public static string GetCulture()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
        }
        public static string GetUiCulture()
        {
            return System.Threading.Thread.CurrentThread.CurrentUICulture.ToString();
        }
        public static string GetCnxNORTHWNDEntities()
        {
            string northwindEntities = "No ConnectionString";
            string cnx = string.Empty;
            ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;
            if ((cnx = connections["NORTHWNDEntities"].ConnectionString) != string.Empty)
                northwindEntities = cnx;
            return northwindEntities;
        }
        public static string GetCnxDefaultConnection()
        {
            string defaultConnection = "No ConnectionString";
            string cnx = string.Empty;
            ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;
            if ((cnx = connections["DefaultConnection"].ConnectionString) != string.Empty)
                defaultConnection = cnx;
            return defaultConnection;
        }
        public static string GetApplicationStatus()
        {
            return ConfigurationManager.AppSettings["cdf54.Status"].ToString();
        }
        public static bool IsAspNetTraceEnabled()
        {
            return HttpContext.Current.Trace.IsEnabled;
        }
        public static bool IsCustomErrorEnabled()
        {
            return HttpContext.Current.IsCustomErrorEnabled;
        }
        public static bool IsDebuggingEnabled()
        {
            return HttpContext.Current.IsDebuggingEnabled;
        }
        public static string GetCompilationMode()
        {
            string compilationMode = "Release";
            if (IsDebuggingEnabled())
            {
                compilationMode = "Debug";
            }
            return compilationMode;
        }
        public static bool IsDemoExceptionLinksEnabled()
        {
            return (ConfigurationManager.AppSettings["ShowDemoExceptionLinks"].ToString() == "true");
        }
        public static string GetAppSetting(string name)
        {
            return ConfigurationManager.AppSettings[name].ToString();
        }

    }
}