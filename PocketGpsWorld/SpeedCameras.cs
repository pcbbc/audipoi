﻿//-----------------------------------------------------------------------
// <copyright file="SpeedCameras.cs" company="mcaddy">
//     All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace PocketGpsWorld
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Drawing;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using Mcaddy;
    using Mcaddy.AudiPoiDatabase;

    /// <summary>
    /// Speed Camera Class
    /// </summary>
    public class SpeedCameras
    {
        /// <summary>
        /// Loads the latest Speed camera database from PocketGpsWorld
        /// </summary>
        /// <param name="username">PocketGpsWorld username</param>
        /// <param name="password">PocketGpsWorld password</param>
        /// <returns>the latest bundle of cameras</returns>
        /// <exception cref="WebException">Thrown if unable to login, browse to or download the file</exception>
        public static byte[] Load(string username, string password)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            string loginAddress = "https://www.pocketgpsworld.com/modules.php?name=Your_Account";

            var client = new CookieAwareWebClient();
            client.Encoding = Encoding.UTF8;

            // Post values
            var values = new NameValueCollection();
            values.Add("username", username);
            values.Add("user_password", password);
            values.Add("op", "login");

            // Logging in
            string loggedInPage = Encoding.ASCII.GetString(client.UploadValues(loginAddress, values));

            // Check for a positive logon error.
            if (loggedInPage.Contains("Login Incorrect!"))
            {
                throw new WebException("Unable to logon to PocketGpsWorld.com, check your username and password");
            }

            // Check to see the download link exisits, failing this check is the first indicator 
            // that someting has changed on the PocketGpsWorld website
            if (!loggedInPage.Contains("<a href=\"/modules.php?name=Cameras#compat\" class=\"sidemenu\">Installation Guides</a>"))
            {
                throw new WebException("Unable to find download link at PocketGPSworld.com, Please report error 1000 to developer");
            }

            string downloadAddress = "https://www.pocketgpsworld.com/modules.php?name=Cameras";
            NameValueCollection postData = new NameValueCollection
            {
              { "op", "DownloadPackage" },
              { "idPackage", "14" },
              { "getPmob", "yes" },
              { "getFrance", "no" },
              { "getSwiss", "no" },
            };

            // Download is preped as a temp file which the browser would normally be redirected to, need to extract the path and download that file.
            string html = Encoding.ASCII.GetString(client.UploadValues(downloadAddress, postData));
            Regex metaRefreshRegex = new Regex("<meta http-equiv=refresh content=0;url=(.+)>\\n");
            Match match = metaRefreshRegex.Match(html);
            if (match.Success)
            {
                byte[] data = client.DownloadData(match.Groups[1].Value);
                return data;
            }
            else
            {
                throw new WebException("Unable to find download link at PocketGPSworld.com, Please report error 1010 to developer");
            }
        }

        /// <summary>
        /// Unpack the PocketGPSWorld CSV Zip file
        /// </summary>
        /// <param name="camerasZip">contents of the Zip file</param>
        /// <returns>List of Cameras</returns>
        public static List<PointOfInterest> UnpackCameras(byte[] camerasZip)
        {
            List<PointOfInterest> unsortedCameras = new List<PointOfInterest>();

            try
            {
                using (MemoryStream ms = new MemoryStream(camerasZip))
                {
                    using (ZipArchive archive = new ZipArchive(ms))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                            {
                                StreamReader streamReader = new StreamReader(entry.Open());
                                string csv = streamReader.ReadToEnd();

                                string[] csvLines = csv.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (string csvLine in csvLines)
                                {
                                    string[] csvEntry = csvLine.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                                    string name = csvEntry[2].Replace("\"", string.Empty);

                                    // We need to skip the title line and the last line of the file
                                    if (!name.Equals("Name") && !name.Equals("Copyright PocketGPSWorld.com"))
                                    {
                                        unsortedCameras.Add(
                                            new PointOfInterest()
                                            {
                                                Longitude = double.Parse(csvEntry[0]),
                                                Latitude = double.Parse(csvEntry[1]),
                                                Name = name
                                            });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new FileFormatException("Something went wrong with unpacking the Cameras Zip file, Please report error 2000 to developer");
            }

            return unsortedCameras;
        }

        /// <summary>
        /// Filter a given collection of POIs with the relevant settings
        /// </summary>
        /// <param name="poiCollection">The collection to filter</param>
        /// <param name="settings">The Settings</param>
        /// <returns>Filtered Collection</returns>
        public static Collection<PointOfInterestCategory> Filter(Collection<PointOfInterestCategory> poiCollection, CameraSettings settings)
        {
            Collection<PointOfInterestCategory> filteredList = new Collection<PointOfInterestCategory>();

            foreach (PointOfInterestCategory category in poiCollection)
            {
                string name = category.Name;
                if (name.StartsWith("ALL "))
				{
                    name = name.Substring(4);
				}
                if ((name.StartsWith(CameraCategory.Fixed.ToDescriptionString()) && settings.IncludeStatic) ||
                (name.StartsWith(CameraCategory.Mobile.ToDescriptionString()) && settings.IncludeMobile) ||
                (name.StartsWith(CameraCategory.Specs.ToDescriptionString()) && settings.IncludeSpecs) ||
                (name.StartsWith(CameraCategory.PMobile.ToDescriptionString()) && settings.IncludeUnverified) ||
                (name.StartsWith(CameraCategory.RedLight.ToDescriptionString()) && settings.IncludeRedLight))
                {
                    filteredList.Add(category);
                }
            }

            return filteredList;
        }

        /// <summary>
        /// Sort the cameras list into a the relevant categories.
        /// </summary>
        /// <param name="cameras">Unsorted list of cameras</param>
        /// <returns>Collection of Categories</returns>
        /// <exception cref="ArgumentException">Thrown when unable to identify camera type</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when unable to assign a camera to a category</exception>
        public static Collection<PointOfInterestCategory> SortCameras(List<PointOfInterest> cameras)
        {
            Collection<PointOfInterestCategory> categorisedCameras = new Collection<PointOfInterestCategory>();

            foreach (PointOfInterest camera in cameras)
            {
                // Parse the Camera Type
                CameraType cameraType = IdentifyCameraType(camera.Name);
                if (cameraType == CameraType.None)
                {
                    throw new ArgumentException(string.Format("Unknown camera Type - {0}", camera.Name));
                }

                int iCameraSpeed = IdentifyCameraSpeed(camera.Name);


                // Identify the correct Category
                CameraCategory cameraCategory = IdentifyCameraCategory(cameraType);
                if (cameraCategory == CameraCategory.None)
                {
                    throw new ArgumentOutOfRangeException(string.Format("Unable to identify Camera Category - {0}", cameraType.ToDescriptionString()));
                }

                // Special Case for Provisional Mobile cameras
                if (cameraCategory.Equals(CameraCategory.Mobile))
                {
                    if (camera.Name.Contains("pMOBILE"))
                    {
                        cameraCategory = CameraCategory.PMobile;
                    }
                }

                string sAlertDesc = "ALL " +cameraCategory.ToDescriptionString();
                string sIconDesc = cameraCategory.ToDescriptionString();
                if (iCameraSpeed > 0)
                {
                    sIconDesc += $" {iCameraSpeed}";
                }

                // Add to relevant alert category
                PointOfInterestCategory targetCategory = categorisedCameras.FirstOrDefault(c => c.Name.Equals(sAlertDesc));
                if (targetCategory == null)
                {
                    targetCategory = new PointOfInterestCategory((int)cameraCategory, sAlertDesc, GenerateIcon(cameraCategory, -2));
                    categorisedCameras.Add(targetCategory);
                }
                targetCategory.Items.Add(camera);

                // Add to relevant icon category
                targetCategory = categorisedCameras.FirstOrDefault(c => c.Name.Equals(sIconDesc));
                if (targetCategory == null)
                {
                    targetCategory = new PointOfInterestCategory((int)cameraCategory, sIconDesc, GenerateIcon(cameraCategory, iCameraSpeed));
                    categorisedCameras.Add(targetCategory);
                }
                targetCategory.Items.Add(camera);
            }

            return categorisedCameras;
        }

        /// <summary>
        /// Attempt to get the Camera Type
        /// </summary>
        /// <param name="name">the name value from the camera</param>
        /// <returns>the best guess at the Camera Type</returns>
        private static CameraType IdentifyCameraType(string name)
        {
            CameraType detectedCameraType = CameraType.None;

            foreach (CameraType cameraType in Enum.GetValues(typeof(CameraType)))
            {
                if (name.Contains(cameraType.ToString()))
                {
                    detectedCameraType = cameraType;
                    break;
                }
            }

            return detectedCameraType;
        }

        /// <summary>
        /// Attempt to get the Camera Speed
        /// </summary>
        /// <param name="name">the name value from the camera</param>
        /// <returns>the camera speed, or -1 for Variable</returns>
        private static int IdentifyCameraSpeed(string name)
        {
            // IconBuilder will display ? for Unknown
            int iSpeed = -2;
            int iPos = name.LastIndexOf('@');
            if (iPos >= 0)
			{
                int iValue;
                if (int.TryParse(name.Substring(iPos + 1), out iValue))
				{
                    iSpeed = iValue;
                    // Speed 0 in PGPSW CSV = Variable
                    if (iSpeed == 0)
                    {
                        // IconBuilder will display V for Variable
                        iSpeed = -1;
					}
				}
			}
            return iSpeed;
        }

        /// <summary>
        /// Identify the Camera category.
        /// </summary>
        /// <param name="cameraType">The detected Camera type</param>
        /// <returns>The category to which this camera type should belong</returns>
        private static CameraCategory IdentifyCameraCategory(CameraType cameraType)
        {
            CameraCategory cameraCategory;

            switch (cameraType)
            {
                case CameraType.GATSO:
                case CameraType.TRUVELO:
                case CameraType.MONITRON:
                case CameraType.REDSPEED:
                    cameraCategory = CameraCategory.Fixed;
                    break;
                case CameraType.REDLIGHT:
                    cameraCategory = CameraCategory.RedLight;
                    break;

                case CameraType.SPECS:
                    cameraCategory = CameraCategory.Specs;
                    break;
                case CameraType.MOBILE:
                    cameraCategory = CameraCategory.Mobile;
                    break;
                case CameraType.None:
                default:
                    cameraCategory = CameraCategory.None;
                    break;
            }

            return cameraCategory;
        }

        /// <summary>
        /// Lookup the relevant Icon from Resources
        /// </summary>
        /// <param name="cameraCategory">The desired Icon category</param>
        /// <returns>A bitmap from resources</returns>
        private static Bitmap LookupIcon(CameraCategory cameraCategory)
        {
            Bitmap icon = null;
            switch (cameraCategory)
            {
                case CameraCategory.Fixed:
                    icon = Properties.Resources.Fixed;
                    break;
                case CameraCategory.Mobile:
                    icon = Properties.Resources.Mobile;
                    break;
                case CameraCategory.PMobile:
                    icon = Properties.Resources.PMobile;
                    break;
                case CameraCategory.Specs:
                    icon = Properties.Resources.Specs;
                    break;
                case CameraCategory.RedLight:
                    icon = Properties.Resources.Red_Light;
                    break;
            }

            return icon;
        }

        private static Bitmap GenerateIcon(CameraCategory cameraCategory, int speed)
		{
            IconBuilder builder = new IconBuilder();
            switch (cameraCategory)
			{
                case CameraCategory.Fixed:
                    builder.TypeOfCamera = IconCameraType.Speed;
                    break;
                case CameraCategory.Mobile:
                    builder.TypeOfCamera = IconCameraType.Mobile;
                    break;
                case CameraCategory.PMobile:
                    builder.TypeOfCamera = IconCameraType.PMobile;
                    break;
                case CameraCategory.Specs:
                    builder.TypeOfCamera = IconCameraType.Average;
                    break;
                case CameraCategory.RedLight:
                    builder.TypeOfCamera = IconCameraType.RedLight;
                    break;
                default:
                    throw new NotImplementedException();
            }
            builder.Speed = speed;
            return builder.GenerateIcon();
		}
    }
}