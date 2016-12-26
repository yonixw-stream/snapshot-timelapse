using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TimeLapse
{
    class GoogleAnalytics
    {
        public static string myTrackID = "UA-89453063-1";

        public GoogleAnalytics()
        {
            myUserAnonIDCache = myUserAnonID();
        }

        // For further reading: 
        // (1) https://ga-dev-tools.appspot.com/hit-builder/
        // (2) https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide#overview

        private const string BASE_API_URL = "https://www.google-analytics.com/collect";
        private string myUserAnonIDCache = null;

        private static string myUserAnonID()
        {
            string result = System.Guid.NewGuid().ToString();
            bool foundValidSavedGuid = false;

            try
            {
                string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                FileInfo mySaveFile = new FileInfo(Path.Combine(myDocuments, "Snapshot-Timelapse", "ga.txt"));
                if (mySaveFile.Exists)
                {
                    Guid validGuid;
                    string mySavedGuid = File.ReadAllText(mySaveFile.FullName);
                    if (System.Guid.TryParse(mySavedGuid, out validGuid))
                    {
                        result = mySavedGuid;
                        foundValidSavedGuid = true;
                    }
                }

                if (!foundValidSavedGuid)
                {
                    Directory.CreateDirectory(mySaveFile.DirectoryName);
                    File.WriteAllText(mySaveFile.FullName, result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problem with filesystem when trying to fing guid."
                    + "\n" + ex.Message
                    + "\n\n" + ex.StackTrace
                    );
            }

            return result;
        }

        private void sendPayload(NameValueCollection payload)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.UploadValuesAsync(new Uri(BASE_API_URL), payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problem with sending payload."
                  + "\n" + ex.Message
                  + "\n\n" + ex.StackTrace
                  );
            }
        }

        public void sendScreenView(string ScreenName, string AppName, string AppVersion)
        {
            NameValueCollection payload = new NameValueCollection();
            payload.Add("v", "1"); // Version
            payload.Add("t", "screenview"); // Type of hit
            payload.Add("tid", myTrackID); // track-id
            payload.Add("cid", myUserAnonIDCache); // user anon id
            payload.Add("cd", ScreenName); // screen-description
            payload.Add("an", AppName); // app-name
            payload.Add("av", AppVersion); // app-verion

            sendPayload(payload);
        }

        public void sendEvent(string Category, string Action, string Label = "nolabel", int PositiveValue = 0)
        {
            NameValueCollection payload = new NameValueCollection();
            payload.Add("v", "1"); // Version
            payload.Add("t", "event"); // Type of hit
            payload.Add("tid", myTrackID); // track-id
            payload.Add("cid", myUserAnonIDCache); // user anon id
            payload.Add("ec", Category); // screen-description
            payload.Add("ea", Action); // app-name
            payload.Add("el", Label); // app-verion
            payload.Add("ev", PositiveValue.ToString()); // app-verion

            sendPayload(payload);
        }

        /*
        *   PREDEFINED Objects:
        */

        public class gaScreenView
        {
            private gaScreenView()
            {
            }

            string _Name;
            string _AppName;
            string _AppVersion;
            GoogleAnalytics _parent;

            public gaScreenView(GoogleAnalytics parent, string Name, string AppName, string AppVersion)
            {
                _Name = Name;
                _AppName = AppName;
                _AppVersion = AppVersion;
                _parent = parent;
            }

            public void send()
            {
                _parent.sendScreenView(_Name, _AppName, _AppVersion);
            }
        }

        public gaScreenView preDefineScreenView(string Name, string AppName, string AppVersion)
        {
            return new gaScreenView(this, Name, AppName, AppVersion);
        }

        public class gaEvent
        {
            private gaEvent()
            {
            }

            string _Category;
            string _Action;
            string _Label;
            int    _PositiveValue;
            GoogleAnalytics _parent;

            public gaEvent(GoogleAnalytics parent, string Category, string Action, string Label = "nolabel", int PositiveValue = 0)
            {
                _Category       = Category;
                _Action         = Action;
                _Label          = Label;
                _PositiveValue  = PositiveValue;
                _parent = parent;
            }

            public void send(int PositiveValue = 0)
            {
                _parent.sendEvent(_Category, _Action,_Label , _PositiveValue );
            }
        }

        public gaEvent preDefineEvent(string Category, string Action, string Label = "nolabel", int PositiveValue = 0)
        {
            return new gaEvent(this,  Category,  Action, Label,PositiveValue);
        }
    }
}
