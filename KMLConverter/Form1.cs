using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using SharpKml.Base;
using SharpKml.Dom;
using Point = SharpKml.Dom.Point;

namespace KMLConverter
{
    public partial class Form1 : Form
    {
        private static readonly string tempPath = Path.Combine(Application.StartupPath, "tmp");

        public static string[] x;

        public Form1()
        {
            InitializeComponent();
        }

        public static string RawPath { get; set; }

        public static string[] Arrstyles { get; set; }

        public static bool WritePlacemark { get; set; }

        public static int Measpointcounter { get; set; }

        private static int LineControl { get; set; }
        public static string InformationElementNwType { get; private set; }

        public static string Kmlfoldername { get; set; }

        public static string Kmloutputfoldername { get; set; }
        public static string LogInputfoldername { get; set; }
        public static string filepathkml { get; set;}
        public static string filenamekml { get; set; }

        public static string filePath { get; set; }

        public static Folder folder;

        public static string ActivityLog { get; set; }

        public static bool Lastwrite { get; set; }

        public static bool WriteKmlCaption { get; set; }
        public Version Version { get; set; } = Assembly.GetEntryAssembly().GetName().Version;

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Text = "KML Converter (v"+ Version.Major + "." + Version.Minor + "." +Version.Build +")";
            filepathkml = Application.StartupPath;
            filenamekml = "Dummy.kml";
            InitializeApplication();

        }
        
        private void InitializeApplication()
        {
            WriteKmlCaption = false;
            chkWriteKmlCaption.Enabled = false;
            Arrstyles = new string[16];
            WritePlacemark = false;
            button1.Enabled = false;
            textFolderName.Enabled = false;
            grpBoxOutputName.Text = "Session Name";
            grpBoxOutputName.Enabled = false;
            grpBoxActivityLog.Text = "Activity Log";
            Kmlfoldername = string.Empty;
            DeleteTempFiles("Linestr.tmp");
            Kmloutputfoldername = Path.Combine(Application.StartupPath, "KML");
            LogInputfoldername = Application.StartupPath;
            LoadSettings();
            chkLastPathExists();
            InitializeControls();
        }

        private void InitializeControls()
        {
            chkWriteKmlCaption.Checked = WriteKmlCaption;
            setLblFileDest();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WriteKml();
        }

        public void WriteKml()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            Lastwrite = false;

            //Create a temp folder if not already existing

            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            //Clear ttemp files
            DeleteTempFiles("Linestr.tmp");

            try
            {
                //TO DO
                /* LTE

                 ********** Excellent   Good        Mid Cell        Cell Edge   Value
                 * RSRP     >=-69       -69 to -92  -92 to -99     <= -100     dBm

                 * RSRQ     >=-10       -10 to 15   -15 to -20      <-20        dB
                 * RSSNR    >=20        13 to 20    0 to 13         <=0         db
                 * 
                 * RSRP -44 dBm (Excellent) to -140 dBm (Bad) -> represented in 1 dB steps (RSRP_00 RSRP<=-140 ~ RSRP_97 -44 <= RSRP)
                 * 
                 * Output String Order TimeStamp; Latitude; Longitude; Speed; Heading; Network Type; Registered; MMC; MNC; Provider; 
                 * CI; PCI; TAC; EARFCN; SS; RSRP; RSRQ; RSSNR; CQI; TA; Address; Provider
                 * 
                 * Colors LTE Excellent 64F00014 Good 64F06714 Average 64F09714 Poor 64F0F514
                 * 
                 * * 2G
                 ********** Excellent   Good        Mid Cell        Cell Edge   Value
                 * RSSI     >=-59       -59 to -81  -81 to -101     <= -101     dBm
                 * 
                 * Output String Order TimeStamp; Latitude; Longitude; Speed; Heading; Network Type; Registered; MMC; MNC; LAC; Cid;
                 * Uarfcn; BSIC; RSSI; Ber ; TA; Address; Provider
                 * 
                 * Colors 2G Excellent 6400FF14 Good 6414F0FF Average 641478FF Poor 641400FF
                 */

                filePath = Application.StartupPath;
                var document = new Document();
                var docLteExcellent = new Document();
                var docLteGood = new Document();
                var docLteMid = new Document();
                var docLteEdge = new Document();
                var docMeasPoints = new Document();
                var docHandoverPoints = new Document();

                var doc3GExcellent = new Document();
                var doc3GGood = new Document();
                var doc3GMid = new Document();
                var doc3GEdge = new Document();

                var doc2GExcellent = new Document();
                var doc2GGood = new Document();
                var doc2GMid = new Document();
                var doc2GEdge = new Document();

                var docTrack = new Document();

                folder = new Folder();

                string[] acc;

                var kml = new Kml();
                var iCounter = 0;

                var workingArr = Readlog();
                var trackArr = new List<string>();
                var coordinates = new CoordinateCollection();
                var unknown = false;

                //Create a folder container
                folder.Id = "Main";
                folder.Name = GetFileName(RawPath);
                document.Name = "Test";

                //***Create styles for the linestring

                try
                {
                    txtActivityLog.Text = txtActivityLog.Text + "Extracting used Network Types" + Environment.NewLine;
                    foreach (var line in workingArr)
                    {
                        unknown = false;

                        acc = line.Split(',');

                        var internalNwType = acc[5].ToLower(); 
                        
                        if (internalNwType == "umts" || internalNwType == "hsdpa" ||
                            internalNwType == "hsupa" || internalNwType == "hspap" ||
                            internalNwType == "hspa" || internalNwType == "tdscdma")
                            internalNwType = "umts";

                        if (internalNwType == "gsm"|| internalNwType=="gprs"|| internalNwType=="edge")
                            internalNwType = "gsm";
                        
                        InformationElementNwType = internalNwType;

                        //Check for unknown signal strength

                        if (internalNwType=="gsm")
                        {
                            if (acc[13] == "unknown")
                                unknown = true;
                        }
                        else if (internalNwType == "umts")
                        {
                            if (acc[13] == "unknown")
                                unknown = true;
                        }
                        else if (internalNwType == "lte")
                        {
                            if (acc[14] == "unknown")
                                unknown = true;
                        }

                        if (!unknown)
                        {
                            switch (internalNwType)
                            {
                                case "lte":

                                    var rsrp = 0.0;

                                    // RSRP     >=-69       -69 to -92  -92 to -99     <= -100     dBm
                                    if (acc[14]=="unknown")
                                    {
                                        rsrp = -999;
                                    }
                                    else
                                    {
                                        rsrp = Convert.ToDouble(acc[14]);
                                    }
                                    
                                    if (rsrp > 0)
                                        rsrp = rsrp * -1;

                                    if (rsrp >= -69)
                                    {
                                        if (Arrstyles[0] == null)
                                        {
                                            StyleTrack(rsrp, docLteExcellent);
                                            txtActivityLog.Text = txtActivityLog.Text +
                                                                  "Found LTE with Signalstrength Excellent" +
                                                                  Environment.NewLine;
                                        }
                                    }
                                    else if (rsrp < -69 && rsrp >= -92)
                                    {
                                        if (Arrstyles[1] == null)
                                        {
                                            StyleTrack(rsrp, docLteGood);
                                            txtActivityLog.Text = txtActivityLog.Text + "Found LTE with Signalstrength Good" +
                                                                  Environment.NewLine;
                                        }
                                    }
                                    else if (rsrp < -92 && rsrp >= -99)
                                    {
                                        if (Arrstyles[2] == null)
                                        {
                                            StyleTrack(rsrp, docLteMid);
                                            txtActivityLog.Text = txtActivityLog.Text +
                                                                  "Found LTE with Signalstrength Average" +
                                                                  Environment.NewLine;
                                        }
                                    }
                                    else if (rsrp < -100)
                                    {
                                        if (Arrstyles[3] == null)
                                        {
                                            StyleTrack(rsrp, docLteEdge);
                                            txtActivityLog.Text = txtActivityLog.Text + "Found LTE with Signalstrength Poor" +
                                                                  Environment.NewLine;
                                        }
                                    }
                                    break;
                                case "cdma":
                                    break;
                                case "gsm":

                                    var rssi = 0.0;
                                    // RSSI     >=-59       -59 to -81  -81 to -101     <= -101     dBm

                                    if (acc[13] == "unknown")
                                    {
                                        rssi = 999;
                                    }
                                    else
                                    {
                                        rssi = Convert.ToDouble(acc[13]); 
                                    }

                                    if (rssi > 0)
                                        rssi = rssi * -1;

                                    if (rssi >= -59)
                                    {
                                        if (Arrstyles[8] == null)
                                        {
                                            StyleTrack2G(rssi, doc2GExcellent);
                                            txtActivityLog.Text = txtActivityLog.Text +
                                                                  "Found GSM with Signalstrength Excellent" +
                                                                  Environment.NewLine;
                                        }
                                    }
                                    else if (rssi < -59 && rssi >= -81)
                                    {
                                        if (Arrstyles[9] == null)
                                        {
                                            StyleTrack2G(rssi, doc2GGood);
                                            txtActivityLog.Text = txtActivityLog.Text +
                                                                  "Found GSM with Signalstrength Good" +
                                                                  Environment.NewLine;
                                        }
                                    }
                                    else if (rssi < -81 && rssi >= -101)
                                    {
                                        if (Arrstyles[10] == null)
                                        {
                                            StyleTrack2G(rssi, doc2GMid);
                                            txtActivityLog.Text = txtActivityLog.Text +
                                                                  "Found GSM with Signalstrength Average" +
                                                                  Environment.NewLine;
                                        }
                                    }
                                    else if (rssi < -101)
                                    {
                                        if (Arrstyles[11] == null)
                                        {
                                            StyleTrack2G(rssi, doc2GEdge);
                                            txtActivityLog.Text = txtActivityLog.Text +
                                                                  "Found GSM with Signalstrength Poor" +
                                                                  Environment.NewLine;
                                        }
                                    }

                                    break;
                                case "umts":
                                    // RSSI     >=-68       -68 to -84  -84 to -104     <= -104     dBm

                                    if (acc[13]=="unknown")
                                    {
                                        rssi = 999;
                                    }
                                    else
                                    {
                                        rssi = Convert.ToDouble(acc[13]);
                                    }

                                    if (rssi > 0)
                                        rssi = rssi * -1;

                                    if (rssi >= -68)
                                    {
                                        if (Arrstyles[12] == null)
                                        {
                                            StyleTrack3G(rssi, doc3GExcellent);
                                            txtActivityLog.Text = txtActivityLog.Text +
                                                                  "Found UMTS with Signalstrength Excellent" +
                                                                  Environment.NewLine;
                                        }
                                    }
                                    else if (rssi < -68 && rssi >= -84)
                                    {
                                        if (Arrstyles[13] == null)
                                        {
                                            StyleTrack3G(rssi, doc3GGood);
                                            txtActivityLog.Text = txtActivityLog.Text +
                                                                  "Found UMTS with Signalstrength Good" +
                                                                  Environment.NewLine;
                                        }
                                    }
                                    else if (rssi < -84 && rssi >= -104)
                                    {
                                        if (Arrstyles[14] == null)
                                        {
                                            StyleTrack3G(rssi, doc3GMid);
                                            txtActivityLog.Text = txtActivityLog.Text +
                                                                  "Found UMTS with Signalstrength Average" +
                                                                  Environment.NewLine;
                                        }
                                    }
                                    else if (rssi < -104)
                                    {
                                        if (Arrstyles[15] == null)
                                        {
                                            StyleTrack3G(rssi, doc3GEdge);
                                            txtActivityLog.Text = txtActivityLog.Text +
                                                                  "Found UMTS with Signalstrength Poor" +
                                                                  Environment.NewLine;
                                        }
                                    }
                                    break;
                            } 
                        }
                        Application.DoEvents();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    throw;
                }


                //*** Create Placemarks for the linestring

                var arrLength = workingArr.Length;
                iCounter = 0;
                unknown = false;

                try
                {
                    txtActivityLog.Text = txtActivityLog.Text + "Extracting data for track generation" +
                                          Environment.NewLine;
                    foreach (var line in workingArr)
                    {
                        unknown = false;
                        acc = line.Split(',');

                        arrLength--;
                        var internalNwType = acc[5].ToLower(); 
                        
                        if (internalNwType == "umts" || internalNwType == "hsdpa" ||
                            internalNwType == "hsupa" || internalNwType == "hspap" ||
                            internalNwType == "hspa" || internalNwType == "tdscdma")
                            internalNwType = "umts";

                        if (internalNwType == "gsm" || internalNwType == "gprs" || internalNwType == "edge")
                            internalNwType = "gsm";

                        //Check for unknown signal strength

                        if (internalNwType == "gsm")
                        {
                            if (acc[13] == "unknown")
                                unknown = true;
                        }
                        else if (internalNwType == "umts")
                        {
                            if (acc[13] == "unknown")
                                unknown = true;
                        }
                        else if (internalNwType == "lte")
                        {
                            if (acc[14] == "unknown")
                                unknown = true;
                        }

                        if (!unknown)
                        {
                            switch (internalNwType)
                            {
                                case "lte":

                                    var rsrp = 0.0;
                                    // RSRP     >=-69       -69 to -92  -92 to -99     <= -100     dBm

                                    if (acc[14]=="unknown")
                                    {
                                        rsrp = 999;
                                    }
                                    else
                                    {
                                        rsrp = Convert.ToDouble(acc[14]);
                                    }

                                    if (rsrp > 0)
                                        rsrp = rsrp * -1;

                                    if (rsrp >= -69)
                                    {
                                        iCounter++;
                                        File.AppendAllText(Path.Combine(tempPath, "Linestr.tmp"),
                                            acc[0] + "," + acc[1] + "," + acc[2] + "," + acc[14] +
                                            ",LteCellExcellent" + Environment.NewLine);
                                    }
                                    else if (rsrp < -69 && rsrp >= -92)
                                    {
                                        iCounter++;
                                        File.AppendAllText(Path.Combine(tempPath, "Linestr.tmp"),
                                            acc[0] + "," + acc[1] + "," + acc[2] + "," + acc[14] +
                                            ",LteCellGood" + Environment.NewLine);
                                    }
                                    else if (rsrp < -92 && rsrp >= -99)
                                    {
                                        iCounter++;
                                        File.AppendAllText(Path.Combine(tempPath, "Linestr.tmp"),
                                            acc[0] + "," + acc[1] + "," + acc[2] + "," + acc[14] +
                                            ",LteCellMid" + Environment.NewLine);
                                    }
                                    else if (rsrp < -100)
                                    {
                                        iCounter++;
                                        File.AppendAllText(Path.Combine(tempPath, "Linestr.tmp"),
                                            acc[0] + "," + acc[1] + "," + acc[2] + "," + acc[14] +
                                            ",LteCellEdge" + Environment.NewLine);
                                    }
                                    break;
                                case "cdma":
                                    //
                                    break;
                                case "gsm":

                                    var rssi = 0.0;

                                    //rssi values for 2G
                                    // RSSI     >=-59       -59 to -81  -81 to -101     <= -101     dBm

                                    if (acc[13]=="unknown")
                                    {
                                        rssi = 999; 
                                    }
                                    else
                                    {
                                        rssi = Convert.ToDouble(acc[13]);
                                    }

                                    if (rssi >= -59)
                                    {
                                        iCounter++;
                                        File.AppendAllText(Path.Combine(tempPath, "Linestr.tmp"),
                                            acc[0] + "," + acc[1] + "," + acc[2] + "," + acc[13] +
                                            ",2gCellExcellent" + Environment.NewLine);
                                    }
                                    else if (rssi < -59 && rssi >= -81)
                                    {
                                        iCounter++;
                                        File.AppendAllText(Path.Combine(tempPath, "Linestr.tmp"),
                                            acc[0] + "," + acc[1] + "," + acc[2] + "," + acc[13] +
                                            ",2gCellGood" + Environment.NewLine);
                                    }
                                    else if (rssi < -81 && rssi >= -101)
                                    {
                                        iCounter++;
                                        File.AppendAllText(Path.Combine(tempPath, "Linestr.tmp"),
                                            acc[0] + "," + acc[1] + "," + acc[2] + "," + acc[13] +
                                            ",2gCellMid" + Environment.NewLine);
                                    }
                                    else if (rssi < -101)
                                    {
                                        iCounter++;
                                        File.AppendAllText(Path.Combine(tempPath, "Linestr.tmp"),
                                            acc[0] + "," + acc[1] + "," + acc[2] + "," + acc[13] +
                                            ",2gCellEdge" + Environment.NewLine);
                                    }
                                    break;
                                case "umts":
                                    // RSSI     >=-68       -68 to -84  -84 to -104     <= -104     dBm

                                    if (acc[13]=="unknown")
                                    {
                                        rssi = 999; 
                                    }
                                    else
                                    {
                                        rssi = Convert.ToDouble(acc[13]);
                                    }

                                    if (rssi > 0)
                                        rssi = rssi * -1;

                                    if (rssi >= -68)
                                    {
                                        iCounter++;
                                        File.AppendAllText(Path.Combine(tempPath, "Linestr.tmp"),
                                            acc[0] + "," + acc[1] + "," + acc[2] + "," + acc[13] +
                                            ",3gCellExcellent" + Environment.NewLine);
                                    }
                                    else if (rssi < -68 && rssi >= -84)
                                    {
                                        iCounter++;
                                        File.AppendAllText(Path.Combine(tempPath, "Linestr.tmp"),
                                            acc[0] + "," + acc[1] + "," + acc[2] + "," + acc[13] +
                                            ",3gCellGood" + Environment.NewLine);
                                    }
                                    else if (rssi < -84 && rssi >= -104)
                                    {
                                        iCounter++;
                                        File.AppendAllText(Path.Combine(tempPath, "Linestr.tmp"),
                                            acc[0] + "," + acc[1] + "," + acc[2] + "," + acc[13] +
                                            ",3gCellMid" + Environment.NewLine);
                                    }
                                    else if (rssi < -104)
                                    {
                                        iCounter++;
                                        File.AppendAllText(Path.Combine(tempPath, "Linestr.tmp"),
                                            acc[0] + "," + acc[1] + "," + acc[2] + "," + acc[13] +
                                            ",3gCellEdge" + Environment.NewLine);
                                    }
                                    break;
                            }


                        }
                        Application.DoEvents();
                        coordinates.Clear();
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                try
                {
                    txtActivityLog.Text = txtActivityLog.Text +
                                          "Create driven track with measurement and handover point data" +
                                          Environment.NewLine;
                    Application.DoEvents();
                    using (var srReader = new StreamReader(Path.Combine(tempPath, "Linestr.tmp")))
                    {
                        string[] linearray;
                        var lines = new List<string>();
                        var compare = string.Empty;

                        try
                        {
                            while (srReader.Peek() >= 0)
                            {
                                linearray = srReader.ReadLine().Split(',');
                                lines.Add(linearray[0] + ";" + linearray[1] + ";" + linearray[2] + ";" + linearray[3] +
                                          ";" + linearray[4]);
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        lines.Sort();

                        LineControl = lines.Count;

                        iCounter = 0;

                        foreach (var line in lines)
                        {
                            acc = line.Split(';');

                            if (!trackArr.Exists(e => e.Contains(acc[1] + ";" + acc[2])))
                            {
                                trackArr.Add(acc[1] + ";" + acc[2]);
                            }
                        }
                        x = trackArr.ToArray();
                        StyleDrivingTrack(docTrack);
                        docTrack.Name = "Driving Path";
                        docTrack.AddFeature(TrackPlacemark(TrackLineString(x), "Track",
                            x[1]));
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                txtActivityLog.Text = txtActivityLog.Text + "Create Route Start point" + Environment.NewLine;
                Application.DoEvents();
                document.Name = "Route Start/End";
                document.AddFeature(PlaceStartEnd("Start", workingArr[0]));
                document.AddStyle(StylePoints("Start", "http://maps.google.com/mapfiles/kml/paddle/go.png", 1.0));

                txtActivityLog.Text = txtActivityLog.Text + "Create Route End point" + Environment.NewLine;
                Application.DoEvents();
                document.Name = "Route Start/End";
                document.AddFeature(PlaceStartEnd("End", workingArr[workingArr.Length - 1]));
                document.AddStyle(StylePoints("End", "http://maps.google.com/mapfiles/kml/paddle/red-stars.png", 1.0));
                folder.AddFeature(document);

                txtActivityLog.Text = txtActivityLog.Text + "Write track data to KML" + Environment.NewLine;
                Application.DoEvents();
                folder.AddFeature(docTrack);
               
                txtActivityLog.Text = txtActivityLog.Text + "Write Measurement points to KML" + Environment.NewLine;
                Application.DoEvents();
                folder.AddFeature(CreateMeasPoints(docMeasPoints, workingArr));

                txtActivityLog.Text = txtActivityLog.Text + "Write Handover points to KML" + Environment.NewLine;
                Application.DoEvents();
                folder.AddFeature(CreateHandoverPoints(docHandoverPoints, workingArr));

                var serializer = new Serializer();
                kml.Feature = folder;

                serializer.Serialize(kml);

                txtActivityLog.Text = txtActivityLog.Text + "Write KML file" + Environment.NewLine;
                Application.DoEvents();
                
                if (!Directory.Exists(Kmloutputfoldername))
                    Directory.CreateDirectory(Kmloutputfoldername);

                if (File.Exists(Path.Combine(Kmloutputfoldername, filenamekml)))
                    File.Delete(Path.Combine(Kmloutputfoldername, filenamekml));
                File.WriteAllText(Path.Combine(Kmloutputfoldername, filenamekml), serializer.Xml);
            }
            catch (Exception)
            {
                txtActivityLog.Text = txtActivityLog.Text + "KML creation unsuccessful!" + Environment.NewLine +
                                      "--End of operation--";
                Application.DoEvents();
                DeleteTempFiles("Linestr.tmp");
                MessageBox.Show("KML creation unsuccessful!. The Application will be terminated!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            txtActivityLog.Text = txtActivityLog.Text + "KML creation successful!" + Environment.NewLine +
                                  "--End of operation--";
            Application.DoEvents();
            DeleteTempFiles("Linestr.tmp");
            MessageBox.Show("KML creation successfully!. The Application will be terminated!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
        }


        public string[] Readlog()
        {
            var lineCoordinates = new List<string>();
            var FilePath = Application.StartupPath;
            var Filename = RawPath;
            var xs = string.Empty;
            string[] x;

            txtActivityLog.Text = "Read Log" + Environment.NewLine;
            Application.DoEvents();

            using (var srReader = new StreamReader(Path.Combine(FilePath, Filename)))
            {
                try
                {
                    while (srReader.Peek() >= 0)
                    {
                        x = checkForComma(srReader.ReadLine());

                        if (x.Length >= 13)
                        {
                            int n;
                            bool isnumeric = int.TryParse(x[x.Length - 1], out n);

                            if (x[0]=="636826173573814380")
                            {
                                MessageBox.Show(x[5]); 
                            }
                            var internalNwType = x[5].ToLower();

                            if (internalNwType == "umts" || internalNwType == "hsdpa" ||
                                internalNwType == "hsupa" || internalNwType == "hspap" ||
                                internalNwType == "hspa" || internalNwType == "tdscdma")
                                internalNwType = "umts";

                            if (internalNwType == "gsm" || internalNwType == "gprs" || internalNwType == "edge")
                                internalNwType = "gsm";

                            if (isnumeric)
                            {
                                if (x.Length == 22 && Convert.ToInt32(x[x.Length - 1]) >= 28)
                                    lineCoordinates.Add(x[0] /*time*/ + "," + x[1] /*lat*/ + "," + x[2] /*lon*/ + "," +
                                                        x[3] /*spd*/ + "," + x[4] /*hnd*/ + "," +
                                                        x[5] /*nwtype*/ + "," + x[6] /*registered*/ + "," +
                                                        x[11] /*mcc*/ + "," + x[12] /*mnc*/ + "," + x[7] /*lac*/ + ","
                                                        + x[8] /*ci*/ + "," + x[9] /*psc*/ + "," + x[10] /*channel*/ +
                                                        "," + x[13] /*rsrp*/ + "," + x[14] /*ber*/ + ","
                                                        + x[15] /*distance*/ + "," +
                                                        x[16].Replace(",", ";") /*Address*/ + "," + x[17] /*provider*/ +
                                                        ","
                                                        + x[18] /*acc*/ + "," + x[19] /*roaming*/ + "," +
                                                        x[20] /*poi*/ + "," + x[21] /*AndroidVersion*/);
                                else if (x.Length == 22 && Convert.ToInt32(x[x.Length - 1]) <= 28 && internalNwType=="umts")
                                    lineCoordinates.Add(x[0] /*time*/ + "," + x[1] /*lat*/ + "," + x[2] /*lon*/ + "," +
                                                        x[3] /*spd*/ + "," + x[4] /*hnd*/ + "," +
                                                        x[5] /*nwtype*/ + "," + x[6] /*registered*/ + "," +
                                                        x[7] /*mcc*/ + "," + x[8] /*mnc*/ + "," + x[9] /*lac*/ + ","
                                                        + x[10] /*ci*/ + "," + x[11] /*psc*/ + "," + x[12] /*channel*/ +
                                                        "," + x[13] /*rsrp*/ + "," + x[14] /*ber*/ + ","
                                                        + x[15] /*distance*/ + "," +
                                                        x[16].Replace(",", ";") /*Address*/ + "," + x[17] /*provider*/ +
                                                        ","+ x[18] /*acc*/ + "," + x[19] /*roaming*/ + "," +
                                                        x[20] /*poi*/ + "," + x[21] /*AndroidVersion*/);
                                else if (x.Length == 22 && (Convert.ToInt32(x[x.Length - 1]) <= 28) && internalNwType=="gsm")
                                    lineCoordinates.Add(x[0] /*time*/ + "," + x[1] /*lat*/ + "," + x[2] /*lon*/ + "," +
                                                        x[3] /*spd*/ + "," + x[4] /*hnd*/ + "," +
                                                        x[5] /*nwtype*/ + "," + x[6] /*registered*/ + "," +
                                                        x[7] /*mcc*/ + "," + x[8] /*mnc*/ + "," +
                                                        x[9] /*lac*/ + "," + x[10] /*channel*/ + "," + x[11] /*bsic*/ +
                                                        "," + x[12] /*rsrp*/ + "," + x[13] /*ber*/ +
                                                        "," + x[14] /*level*/ + "," + x[15] /*distance*/ + "," +
                                                        x[16].Replace(",", ";") /*address*/ + "," +
                                                        x[17] /*provider*/ + "," + x[18] /*acc*/ + "," +
                                                        x[19] /*roaming*/ + "," + x[20] /*poi*/);
                                else if (x.Length == 23 && (Convert.ToInt32(x[x.Length - 1]) >= 28))
                                    lineCoordinates.Add(x[0] /*time*/ + "," + x[1] /*lat*/ + "," + x[2] /*lon*/ + "," +
                                                        x[3] /*spd*/ + "," + x[4] /*hnd*/ + "," +
                                                        x[5] /*nwtype*/ + "," + x[6] /*registered*/ + "," +
                                                        x[12] /*mcc*/ + "," + x[13] /*mnc*/ + "," + x[7] /*ci*/ + "," +
                                                        x[8] /*lac*/ + "," + x[9] /*channel*/ + "," + x[10] /*bsic*/ +
                                                        "," + x[13] /*rsrp*/ + "," + x[14] /*ber*/ +
                                                        "," + x[15] /*level*/ + "," + x[16] /*distance*/ + "," +
                                                        x[17].Replace(",", ";") /*address*/ + "," +
                                                        x[18] /*provider*/ + "," + x[19] /*acc*/ + "," +
                                                        x[20] /*roaming*/ + "," + x[21] /*poi*/);
                                else if (x.Length == 23 && (Convert.ToInt32(x[x.Length - 1]) <= 28))
                                    lineCoordinates.Add(x[0] /*time*/ + "," + x[1] /*lat*/ + "," + x[2] /*lon*/ + "," +
                                                        x[3] /*spd*/ + "," + x[4] /*hnd*/ + "," +
                                                        x[5] /*nwtype*/ + "," + x[6] /*registered*/ + "," +
                                                        x[7] /*mcc*/ + "," + x[8] /*mnc*/ + "," + x[9] /*ci*/ + "," +
                                                        x[10] /*lac*/ + "," + x[11] /*channel*/ + "," + x[12] /*bsic*/ +
                                                        "," + x[13] /*rsrp*/ + "," + x[14] /*ber*/ +
                                                        "," + x[15] /*level*/ + "," + x[16] /*distance*/ + "," +
                                                        x[17].Replace(",", ";") /*address*/ + "," +
                                                        x[18] /*provider*/ + "," + x[19] /*acc*/ + "," +
                                                        x[20] /*roaming*/ + "," + x[21] /*poi*/);
                                else if (x.Length == 26)
                                    lineCoordinates.Add(x[0] + "," + x[1] + "," + x[2] + "," + x[3] + "," + x[4] +
                                                        "," +
                                                        x[5] + "," + x[6] + "," + x[7] + "," + x[8] + "," + x[9] +
                                                        "," + x[10] +
                                                        "," + x[11] + "," + x[12] + "," + x[13] + "," + x[14] +
                                                        "," + x[15] +
                                                        "," + x[16] + "," + x[17] + "," + x[18] + "," +
                                                        x[19].Replace(",", ";") +
                                                        "," + x[20] + "," + x[21] + "," + x[22] + "," + x[23] +
                                                        "," + x[24]);
                                else if (x.Length == 27 && Convert.ToInt32(x[x.Length - 1]) >= 28)
                                    lineCoordinates.Add(x[0] /*time*/ + "," + x[1] /*lat*/ + "," + x[2] /*lon*/ + "," +
                                                        x[3] /*spd*/ + "," + x[4] /*hnd*/ + "," +
                                                        x[5] /*nwtype*/ + "," + x[6] /*registered*/ + "," +
                                                        x[12] /*mcc*/ + "," + x[13] /*mnc*/ + "," + x[7] /*ci*/ + ","
                                                        + x[8] /*pci*/ + "," + x[9] /*tac*/ + "," + x[10] /*channel*/ +
                                                        "," + x[14] /*ss*/ + "," + x[15] /*rsrp*/ + ","
                                                        + x[16] /*rsrq*/ + "," + x[17] /*rssnr*/ + "," + x[18] /*cqi*/ +
                                                        "," + x[19] /*ta*/ + "," + x[20] /*distance*/ + "," +
                                                        x[21].Replace(",", ";") /*Address*/ + "," + x[22] /*provider*/ +
                                                        "," + x[23] /*acc*/ + "," + x[24] /*roaming*/ +
                                                        "," + x[25] /*poi*/ + "," + x[11] /*chbw*/ + "," +
                                                        x[26] /*AndroidVersion*/);
                            }
                            else
                            {
                                if (x.Length == 25)
                                    lineCoordinates.Add(x[0] + "," + x[1] + "," + x[2] + "," + x[3] + "," + x[4] +
                                                        "," +
                                                        x[5] + "," + x[6] + "," + x[7] + "," + x[8] + "," + x[9] +
                                                        "," + x[10] +
                                                        "," + x[11] + "," + x[12] + "," + x[13] + "," + x[14] +
                                                        "," + x[15] +
                                                        "," + x[16] + "," + x[17] + "," + x[18] + "," +
                                                        x[19].Replace(",", ";") +
                                                        "," + x[20] + "," + x[21] + "," + x[22] + "," + x[23] +
                                                        "," + x[24]);
                                else if (x.Length == 20)
                                    lineCoordinates.Add(x[0] + "," + x[1] + "," + x[2] + "," + x[3] + "," + x[4] +
                                                        "," +
                                                        x[5] + "," + x[6] + "," + x[7] + "," + x[8] + "," + x[9] +
                                                        "," + x[10] +
                                                        "," + x[11] + ",unknown," + x[12] + "," + x[13] + "," +
                                                        x[14] + "," +
                                                        x[15].Replace(",", ";") + "," + x[16] + "," + x[17] + "," +
                                                        x[18] + ","
                                                        + x[19]);
                                else if (x.Length == 21)
                                    lineCoordinates.Add(x[0] + "," + x[1] + "," + x[2] + "," + x[3] + "," + x[4] +
                                                        "," +
                                                        x[5] + "," + x[6] + "," + x[7] + "," + x[8] + "," + x[9] +
                                                        "," +
                                                        x[10] + "," + x[11] + "," + x[12] + "," + x[13] + "," +
                                                        x[14] +
                                                        "," + x[15] + "," + x[16].Replace(",", ";") + "," + x[17] +
                                                        "," + x[18] + "," + x[19] + "," + x[20]);

                                else if (x.Length == 22)
                                    lineCoordinates.Add(x[0] + "," + x[1] + "," + x[2] + "," + x[3] + "," + x[4] +
                                                        "," +
                                                        x[5] + "," + x[6] + "," + x[7] + "," + x[8] + "," + x[9] +
                                                        "," +
                                                        x[10] + "," + x[11] + "," + x[12] + "," + x[13] + "," +
                                                        x[14] +
                                                        "," + x[15] + "," + x[16] + "," + x[17].Replace(",", ";") +
                                                        "," +
                                                        x[18] + "," + x[19] + "," + x[20] + "," + x[21]);

                                else if (x.Length == 23)
                                    lineCoordinates.Add(x[0] /*time*/ + "," + x[1] /*lat*/ + "," + x[2] /*lon*/ +
                                                        "," + x[3] /*spd*/ + "," + x[4] /*hnd*/ + "," +
                                                        x[5] /*nwtype*/ + "," + x[6] /*registered*/ + "," +
                                                        x[11] /*mcc*/ + "," + x[12] /*mnc*/ + "," + x[7] /*ci*/ +
                                                        "," +
                                                        x[8] /*lac*/ + "," + x[9] /*channel*/ + "," +
                                                        x[10] /*bsic*/ + "," + x[13] /*rsrp*/ + "," +
                                                        x[14] /*ber*/ +
                                                        "," + x[15] /*level*/ + "," + x[16] /*distance*/ + "," +
                                                        x[17].Replace(",", ";") /*address*/ + "," +
                                                        x[18] /*provider*/ + "," + x[19] /*acc*/ + "," +
                                                        x[20] /*roaming*/ + "," + x[21] /*poi*/ + "," +
                                                        x[22]) /*AndroidVersion*/;

                                else if (x.Length == 26)
                                    lineCoordinates.Add(x[0] + "," + x[1] + "," + x[2] + "," + x[3] + "," + x[4] +
                                                        "," +
                                                        x[5] + "," + x[6] + "," + x[7] + "," + x[8] + "," + x[9] +
                                                        "," + x[10] +
                                                        "," + x[11] + "," + x[12] + "," + x[13] + "," + x[14] +
                                                        "," + x[15] +
                                                        "," + x[16] + "," + x[17] + "," + x[18] + "," +
                                                        x[19].Replace(",", ";") +
                                                        "," + x[20] + "," + x[21] + "," + x[22] + "," + x[23] +
                                                        "," + x[24]);
                                else if (x.Length == 13)
                                {
                                    //lineCoordinates.Add(x[0]/*time*/ + "," + x[1]/*lat*/ + "," + x[2]/*lon*/ + "," + x[3]/*spd*/ + "," + x[4]/*hnd*/ + "," + x[5]/*nwtype*/ + "," + 
                                    //      x[20]/*distance*/ + "," + x[21].Replace(",", ";")/*Address*/ + "," + x[22]/*provider*/  + "," + x[23]/*acc*/ + 
                                    //      "," + x[24]/*roaming*/ + "," + x[25]/*poi*/  + "," + x[26]/*AndroidVersion*/);
                                    //todo write import error log
                                    WriteInvalidLogFiles(x[0] /*time*/ + "," + x[1] /*lat*/ + "," + x[2] /*lon*/ +
                                                         "," + x[3] /*spd*/ + "," + x[4] /*hnd*/ + "," +
                                                         x[5] /*nwtype*/ + "," +
                                                         x[6] /*distance*/ + "," +
                                                         x[7].Replace(",", ";") /*Address*/ + "," +
                                                         x[8] /*provider*/ + "," + x[9] /*acc*/ +
                                                         "," + x[10] /*roaming*/ + "," + x[11] /*poi*/ + "," +
                                                         x[12] /*AndroidVersion*/);
                                }
                                else
                                {
                                    MessageBox.Show(
                                        "The logfile is not compatible with this Version of KML Converter.\r\rThe Application will be terminated!",
                                        "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    Application.Exit();
                                    break;
                                }
                            }
                        }
                        /*
                             0: 636814354936713870; time
                             1: 37.51966462; lat
                             2: 127.10568134; lon
                             3: 0.889999985694885; spd
                             4: 304.600006103516; hnd
                             5: Lte; nwtype
                             6: YES; registered
                             7: 1491982; ci
                             8: 101; pci 
                             9: 13; tac
                             10: 1550(eFDD3); earfcn
                             11: 20000; chbw
                             12: 450; mMcc
                             13: 08; mMnc 
                             14: 31; ss
                             15: -85; rsrp
                             16: -10; rsrq
                             17: unknown; rssnr
                             18: unknown; cqi
                             19: 2; ta
                             20: 0.0900840367737288; distance
                             21: Hyowon-ro, Maetan-dong, Yeongtong-gu, Suwon-si, Gyeonggi-do, South Korea,;address
                             22: KT Freetel Co. Ltd. (KR);provider
                             23: 48.2400016784668; accuracy
                             24: False; roaming
                             25: False; poi
                             26: 28 android version
                         */


                        //}
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            txtActivityLog.Text = txtActivityLog.Text + "Read " + lineCoordinates.Count + " Datalines" +
                                  Environment.NewLine;
            ;
            Application.DoEvents();
            var s = lineCoordinates.ToArray();
            return s;
        }

        private string[] ReadlogRedux()
        {
            var lineCoordinates = new List<string>();
            var FilePath = Application.StartupPath;
            var Filename = RawPath;
           
            txtActivityLog.Text = "Read Log" + Environment.NewLine;
            Application.DoEvents();

            using (var srReader = new StreamReader(Path.Combine(FilePath, Filename)))
            {
                try
                {
                    while (srReader.Peek() >= 0)
                    {
                        lineCoordinates.Add(srReader.ReadLine());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            txtActivityLog.Text = txtActivityLog.Text + "Read " + lineCoordinates.Count + " Datalines" +
                                  Environment.NewLine;
            ;
            Application.DoEvents();
            var s = lineCoordinates.ToArray();
            return s;
        }

        public static LineString TrackLineString(string[] trackCoordinates)
        {
            var lineString = new LineString();
            var coordinates = new CoordinateCollection();
            string[] acc;

            lineString.AltitudeMode = AltitudeMode.ClampToGround;

            foreach (var line in trackCoordinates)
                if (line != string.Empty)
                {
                    acc = line.Split(';');
                    coordinates.Add(new Vector(Convert.ToDouble(acc[0]), Convert.ToDouble(acc[1])));
                }
                else
                {
                    Console.WriteLine("Empty");
                }

            lineString.Coordinates = coordinates;
            return lineString;
        }
        
        public static Placemark TrackPlacemark(LineString lineString, string compare, string id)
        {
            var placemark = new Placemark
            {
                Name = id.Replace(";", ","),
                StyleUrl = new Uri("#" + compare, UriKind.Relative),
                Geometry = lineString
            };
            return placemark;
        }

        //New
        public static Placemark DrivingTrackPlacemark(LineString lineString, string compare, string id)
        {
            var placemark = new Placemark
            {
                Name = id.Replace(";", ","),
                StyleUrl = new Uri("#" + compare, UriKind.Relative),
                Geometry = lineString
            };
            return placemark;
        }

        public static Placemark PlaceStartEnd(string name, string coord)
        {
            var uri = "#" + name;
            var placemark = new Placemark();
            var point = new Point();
            var acc = coord.Split(',');

            placemark.Name = name + " - " + acc[1] + "," + acc[2];
            placemark.StyleUrl = new Uri(uri, UriKind.Relative);
            placemark.Geometry = point;
            point.Coordinate = new Vector(Convert.ToDouble(acc[1]), Convert.ToDouble(acc[2]));
            return placemark;
        }

        public static Placemark MeasPoints(string name, string nwtype, string lat, string lon, string distance)
        {
            var uri = "#" + name;
            var placemark = new Placemark();
            var point = new Point();

            if (WriteKmlCaption)
            {
                if (distance.Length > 5)
                    placemark.Name = nwtype + " - @Km " + distance.Substring(0, 5);
                else
                    placemark.Name = nwtype + " - @Km 0.00";
            }

            placemark.Visibility = false;
            placemark.StyleUrl = new Uri(uri, UriKind.Relative);
            placemark.Geometry = point;
            point.Coordinate = new Vector(Convert.ToDouble(lat), Convert.ToDouble(lon));
            return placemark;
        }

        public static Placemark HandoverPoint(string name, string lat, string lon, string distance)
        {
            var uri = "#" + name;
            var placemark = new Placemark();
            var point = new Point();

            if (WriteKmlCaption)
            {
                if (distance.Length > 5)
                    placemark.Name = "Handover @Km " + distance.Substring(0, 5);
                else
                    placemark.Name = "Handover @Km 0.00";
            }

            placemark.Visibility = false;
            placemark.StyleUrl = new Uri(uri, UriKind.Relative);
            placemark.Geometry = point;
            point.Coordinate = new Vector(Convert.ToDouble(lat), Convert.ToDouble(lon));
            return placemark;
        }

        public static Style HandoverPointStyle(string id, string time, string campedcell, string newcell, string address,
            string pci, string provider)
        {
            var outText = string.Empty;
            var dt = new DateTime(Convert.ToInt64(time));
            var uri = "";

            try
            {
                if (InformationElementNwType.ToLower() == "lte")
                {
                    uri = "http://www.e-i-t.de/markerL.png";
                    outText = "Cell Handover Info" + Environment.NewLine + Environment.NewLine
                              + "Handover Timestamp: " + dt + Environment.NewLine + Environment.NewLine
                              + "Location: " + address + Environment.NewLine
                              + "Provider: " + provider + Environment.NewLine + Environment.NewLine
                              + "Physical Cell ID : " + pci + Environment.NewLine + Environment.NewLine
                              + "Camped on Cell ID : " + campedcell + Environment.NewLine
                              + "Handover to Cell : " + newcell;
                }
                else if (InformationElementNwType.ToLower() == "gsm")
                {
                    uri = "http://www.e-i-t.de/markerG.png";
                    outText = "Cell Handover Info" + Environment.NewLine + Environment.NewLine
                              + "Handover Timestamp: " + dt + Environment.NewLine + Environment.NewLine
                              + "Location: " + address + Environment.NewLine
                              + "Provider: " + provider + Environment.NewLine + Environment.NewLine
                              + "BSIC : " + pci + Environment.NewLine + Environment.NewLine
                              + "Camped on Cell ID : " + campedcell + Environment.NewLine
                              + "Handover to Cell : " + newcell;
                }
                else
                {
                    uri = "http://www.e-i-t.de/markerU.png";
                    outText = "Cell Handover Info" + Environment.NewLine + Environment.NewLine
                              + "Handover Timestamp: " + dt + Environment.NewLine + Environment.NewLine
                              + "Location: " + address + Environment.NewLine
                              + "Provider: " + provider + Environment.NewLine + Environment.NewLine
                              + "Primary Scrambling Code : " + pci + Environment.NewLine + Environment.NewLine
                              + "Camped on Cell ID : " + campedcell + Environment.NewLine
                              + "Handover to Cell : " + newcell;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

            var style = new Style
                {
                    Balloon = new BalloonStyle(),
                    Icon = new IconStyle(),
                    Id = "HoStyle" + id
                };
                style.Icon.Icon = new IconStyle.IconLink(new Uri(uri));
                style.Icon.Scale = 1.0;
                style.Balloon.DisplayMode = DisplayMode.Default;
                style.Balloon.Text = outText;

                return style;
        }

        public static Style MeasPointStyle(string id, string time, string lat, string lon, string location, string spd,
            string hnd, string nwtype, string mmc, string mnc, string ci, string pci, string tac, string earfcn,
            string ss, string rsrp, string rsrq, string rssnr, string cqi, string ta, string provider, double accuracy, bool isroaming, bool poi, double chbw)
        {
            try
            {
                var dt = new DateTime(Convert.ToInt64(time));
                var newmnc = mnc;
                var newrsrp = 0.0;
                var roaminind = "";
                var ispoi = "";
                string bwdisplay = "";

                if (rsrp == "unknown")
                {
                    newrsrp = 0.1;
                    rsrp = "999";
                }
                else
                {
                    newrsrp = Convert.ToDouble(rsrp);
                }

                if (newrsrp > 0)
                    newrsrp = newrsrp * -1;

                if (isroaming)
                {
                    roaminind = "(Roaming)";
                }

                if (poi) 
                {
                    ispoi = "Yes";
                }
                else
                {
                    ispoi = "No";
                }

                if (chbw==-1)
                {
                    bwdisplay = " - Bandwidth not in Info";
                }
                else
                {
                    bwdisplay = " - Bandwidth: " + Convert.ToString(chbw / 1000) + " MHz";
                }

                var style = new Style
                {
                    Balloon = new BalloonStyle(),
                    Icon = new IconStyle(),
                    Id = "BalloonStyle" + id
                };
                style.Icon.Icon =
                    new IconStyle.IconLink(new Uri(MeasIconSelector(rsrp, nwtype)));
                style.Icon.Scale = 0.3; //todo 
                style.Balloon.DisplayMode = DisplayMode.Default;
                style.Balloon.Text = "Network Location Info" + System.Environment.NewLine
                                     + "Taken on " + dt + System.Environment.NewLine + System.Environment.NewLine
                                     + "Coordinates" + System.Environment.NewLine
                                     + "Latitude: " + lat + System.Environment.NewLine
                                     + "Longitude: " + lon + System.Environment.NewLine
                                     + "Location: " + location + System.Environment.NewLine
                                     + "Accuracy: " + Math.Round(accuracy,0) + " m" + System.Environment.NewLine + System.Environment.NewLine
                                     + "Moving Data" + System.Environment.NewLine
                                     + string.Format("Speed: {0:N1} kph", Convert.ToDouble(spd)) +
                                     System.Environment.NewLine
                                     + string.Format("Heading: {0:N1} degrees", Convert.ToDouble(hnd)) +
                                     System.Environment.NewLine + System.Environment.NewLine
                                     + "Network Data" + System.Environment.NewLine
                                     + "Provider: " + provider + " " + roaminind + System.Environment.NewLine
                                     + "Provider Info: MCC " + mmc + " MNC " + newmnc + " Network Type: [" + nwtype + "]" +
                                     System.Environment.NewLine
                                     + "Cell ID: " + ci + System.Environment.NewLine
                                     + "Physical Cell ID " + pci + System.Environment.NewLine
                                     + "Tracking Area Code: " + tac + System.Environment.NewLine +
                                     System.Environment.NewLine
                                     + "Measurements" + System.Environment.NewLine
                                     + "EARFCN: " + earfcn + bwdisplay +  System.Environment.NewLine
                                     + "Sync Signal: " + ss + System.Environment.NewLine
                                     + "Ref. Signal Receive Power: " + newrsrp + " dBm" + System.Environment.NewLine
                                     + "Ref. Signal Receive Qual.: " + rsrq + " dB" + System.Environment.NewLine
                                     + "Ref. Signal Signal-to-Noise Ratio: " + rssnr + System.Environment.NewLine
                                     + "Channel Quality Indicator: " + cqi + System.Environment.NewLine
                                     + "Timing Advance: " + ta + System.Environment.NewLine + System.Environment.NewLine
                                     + "Point of Interest: " + ispoi + System.Environment.NewLine;
               
                return style;
            }
            catch (Exception ex)
            {
                var style =new Style();
                MessageBox.Show(ex.Message);
                return style;

            }
        }

        public static Style MeasPointStyle3G(string id, string time, string lat, string lon, string location, string spd,
            string hnd, string nwtype, string mmc, string mnc, string lac, string ci, string psc, string uarfcn,
            string rsrp, string ber, string provider, double accuracy, bool isroaming, bool poi)
        {
            var dt = new DateTime(Convert.ToInt64(time));
            var newmnc = mnc;
            var newrssi = Convert.ToDouble(rsrp);
            var roaminind = "";
            var ispoi = "";

            try
            {
                if (newrssi > 0)
                    newrssi = newrssi * -1;

                if (isroaming)
                {
                    roaminind = "(Roaming)";
                }

                if (poi)
                {
                    ispoi = "Yes";
                }
                else
                {
                    ispoi = "No";
                }

                var style = new Style
                {
                    Balloon = new BalloonStyle(),
                    Icon = new IconStyle(),
                    Id = "BalloonStyle" + id
                };
                style.Icon.Icon =
                    new IconStyle.IconLink(new Uri(MeasIconSelector(rsrp, nwtype)));
                style.Icon.Scale = 0.3;
                style.Balloon.DisplayMode = DisplayMode.Default;
                style.Balloon.Text = "Network Location Info" + System.Environment.NewLine
                                     + "Taken on " + dt + System.Environment.NewLine + System.Environment.NewLine
                                     + "Coordinates" + System.Environment.NewLine
                                     + "Latitude: " + lat + System.Environment.NewLine
                                     + "Longitude: " + lon + System.Environment.NewLine
                                     + "Location: " + location + System.Environment.NewLine
                                     + "Accuracy: " + Math.Round(accuracy,0) + " m" + System.Environment.NewLine + System.Environment.NewLine
                                     + "Moving Data" + System.Environment.NewLine
                                     + string.Format("Speed: {0:N1} kph", Convert.ToDouble(spd)) +
                                     System.Environment.NewLine
                                     + string.Format("Heading: {0:N1} degrees", Convert.ToDouble(hnd)) +
                                     System.Environment.NewLine + System.Environment.NewLine
                                     + "Network Data" + System.Environment.NewLine
                                     + "Provider: " + provider + " " + roaminind + System.Environment.NewLine
                                     + "Provider Info: MCC " + mmc + " MNC " + newmnc + " Network Type: [" + nwtype + "]" +
                                     System.Environment.NewLine
                                     + "Cell ID: " + ci + System.Environment.NewLine
                                     + "Location Area Code: " + lac + System.Environment.NewLine +
                                     System.Environment.NewLine
                                     + "Measurements" + System.Environment.NewLine
                                     + "UARFCN: " + uarfcn + System.Environment.NewLine
                                     + "Rec. Signal Strength Power: " + newrssi + " dBm" + System.Environment.NewLine
                                     + "Ch. Bit Error Rate: " + ber + System.Environment.NewLine
                                     + "Primary scrambling code: " + psc + System.Environment.NewLine + System.Environment.NewLine
                                     + "Point of Interest: " + ispoi + System.Environment.NewLine;


                return style;
            }
            catch (Exception ex)
            {

                var style = new Style();
                MessageBox.Show(ex.Message);
                return style;
            }
        }
        public static Style MeasPointStyle2G(string id, string time, string lat, string lon, string location, string spd,
            string hnd, string nwtype, string mmc, string mnc, string lac, string ci, string bsic, string uarfcn,
            string rsrp, string ber, string ta, string provider, double accuracy, bool isroaming, bool poi)
        {
            var dt = new DateTime(Convert.ToInt64(time));
            var newmnc = mnc;
            var newrssi = Convert.ToDouble(rsrp);
            var roaminind = "";
            var ispoi = "";

            try
            {
                if (newrssi > 0)
                    newrssi = newrssi * -1;

                if (isroaming)
                {
                    roaminind = "(Roaming)";
                }

                if (poi)
                {
                    ispoi = "Yes";
                }
                else
                {
                    ispoi = "No";
                }

                var style = new Style
                {
                    Balloon = new BalloonStyle(),
                    Icon = new IconStyle(),
                    Id = "BalloonStyle" + id
                };
                style.Icon.Icon =
                    new IconStyle.IconLink(new Uri(MeasIconSelector(rsrp, nwtype)));
                style.Icon.Scale = 0.3;
                style.Balloon.DisplayMode = DisplayMode.Default;
                style.Balloon.Text = "Network Location Info" + System.Environment.NewLine
                                     + "Taken on " + dt + System.Environment.NewLine + System.Environment.NewLine
                                     + "Coordinates" + System.Environment.NewLine
                                     + "Latitude: " + lat + System.Environment.NewLine
                                     + "Longitude: " + lon + System.Environment.NewLine
                                     + "Location: " + location + System.Environment.NewLine
                                     + "Accuracy: " + Math.Round(accuracy,0) + " m" + System.Environment.NewLine + System.Environment.NewLine
                                     + "Moving Data" + System.Environment.NewLine
                                     + string.Format("Speed: {0:N1} kph", Convert.ToDouble(spd)) +
                                     System.Environment.NewLine
                                     + string.Format("Heading: {0:N1} degrees", Convert.ToDouble(hnd)) +
                                     System.Environment.NewLine + System.Environment.NewLine
                                     + "Network Data" + System.Environment.NewLine
                                     + "Provider: " + provider + " " + roaminind + System.Environment.NewLine
                                     + "Provider Info: MCC " + mmc + " MNC " + newmnc + " Network Type: [" + nwtype + "]" +
                                     System.Environment.NewLine
                                     + "Cell ID: " + ci + System.Environment.NewLine
                                     + "Location Area Code: " + lac + System.Environment.NewLine +
                                     System.Environment.NewLine
                                     + "Measurements" + System.Environment.NewLine
                                     + "UARFCN: " + uarfcn + System.Environment.NewLine
                                     + "RSSI: " + newrssi + " dBm" + System.Environment.NewLine
                                     + "Bit Error Rate: " + ber + System.Environment.NewLine
                                     + "BSIC: " + bsic + System.Environment.NewLine
                                     + "Timing Advance: " + ta + System.Environment.NewLine + System.Environment.NewLine
                                     + "Point of Interest: " + ispoi + System.Environment.NewLine;

                /*if (newrssi.ToString().Length == 4)
                {
                    MessageBox.Show(newrssi.ToString());
                }*/
                
                return style;
            }
            catch (Exception ex)
            {
                var style = new Style();
                MessageBox.Show(ex.Message);
                return style;
            }
        }

        public static Style StylePoints(string id, string uri, double scale)
        {
            var style = new Style
            {
                Icon = new IconStyle(),
                Id = id
            };
            style.Icon.Icon = new IconStyle.IconLink(new Uri(uri));
            style.Icon.Scale = scale;

            return style;
        }

        //New todo
        public static void StyleDrivingTrack(Document document)
        {
            var style = new Style();
            var linestyle = new LineStyle();

            style.Id = "Track";
            style.Line = linestyle;
            style.Line.Color = new Color32(Color32.Parse("641400FF").Abgr);
            style.Line.Width = 4;
            document.AddStyle(style);
            Arrstyles[0] = style.Id;
            
        }
        public static void StyleTrack(double rsrp, Document document)
        {
            var style = new Style();
            var linestyle = new LineStyle();


            if (rsrp >= -69)
            {
                if (Arrstyles[0] == null)
                {
                    style.Id = "LteCellExcellent";
                    style.Line = linestyle;
                    style.Line.Color = new Color32(Color32.Parse("641400FF").Abgr);
                    style.Line.Width = 10;
                    document.AddStyle(style);
                    Arrstyles[0] = style.Id;
                }
            }
            else if (rsrp < -69 && rsrp >= -92)
            {
                if (Arrstyles[1] == null)
                {
                    style.Id = "LteCellGood";
                    style.Line = linestyle;
                    style.Line.Color = new Color32(Color32.Parse("643900A7").Abgr);
                    style.Line.Width = 10;
                    document.AddStyle(style);
                    Arrstyles[1] = style.Id;
                }
            }
            else if (rsrp < -92 && rsrp >= -99)
            {
                if (Arrstyles[2] == null)
                {
                    style.Id = "LteCellMid";
                    style.Line = linestyle;
                    style.Line.Color = new Color32(Color32.Parse("6452006B").Abgr);
                    style.Line.Width = 10;
                    document.AddStyle(style);
                    Arrstyles[2] = style.Id;
                }
            }
            else if (rsrp < -100)
            {
                if (Arrstyles[3] == null)
                {
                    style.Id = "LteCellEdge";
                    style.Line = linestyle;
                    style.Line.Color = new Color32(Color32.Parse("6474001C").Abgr);
                    style.Line.Width = 10;
                    document.AddStyle(style);
                    Arrstyles[3] = style.Id;
                }
            }
        }

       public static void StyleTrack3G(double rsrp, Document document)
        {
            var style = new Style();
            var linestyle = new LineStyle();


            if (rsrp >= -68)
            {
                if (Arrstyles[12] == null)
                {
                    style.Id = "3gCellExcellent";
                    style.Line = linestyle;
                    style.Line.Color = new Color32(Color32.Parse("6414F0FF").Abgr);
                    style.Line.Width = 10;
                    document.AddStyle(style);
                    Arrstyles[12] = style.Id;
                }
            }
            else if (rsrp < -68 && rsrp >= -84)
            {
                if (Arrstyles[13] == null)
                {
                    style.Id = "3gCellGood";
                    style.Line = linestyle;
                    style.Line.Color = new Color32(Color32.Parse("6414ADA0").Abgr);
                    style.Line.Width = 10;
                    document.AddStyle(style);
                    Arrstyles[13] = style.Id;
                }
            }
            else if (rsrp < -84 && rsrp >= -104)
            {
                if (Arrstyles[14] == null)
                {
                    style.Id = "3gCellMid";
                    style.Line = linestyle;
                    style.Line.Color = new Color32(Color32.Parse("64147855").Abgr);
                    style.Line.Width = 10;
                    document.AddStyle(style);
                    Arrstyles[14] = style.Id;
                }
            }
            else if (rsrp < -104)
            {
                if (Arrstyles[15] == null)
                {
                    style.Id = "3gCellEdge";
                    style.Line = linestyle;
                    style.Line.Color = new Color32(Color32.Parse("64144209").Abgr);
                    style.Line.Width = 10;
                    document.AddStyle(style);
                    Arrstyles[15] = style.Id;
                }
            }
        }
        
        //Excellent 6400FF14 Good 6414F0FF Average 641478FF Poor 641400FF
        public static void StyleTrack2G(double rsrp, Document document)
        {
            var style = new Style();
            var linestyle = new LineStyle();


            if (rsrp >= -59)
            {
                if (Arrstyles[8] == null)
                {
                    style.Id = "2gCellExcellent";
                    style.Line = linestyle;
                    style.Line.Color = new Color32(Color32.Parse("6400FF14").Abgr);
                    style.Line.Width = 10;
                    document.AddStyle(style);
                    Arrstyles[8] = style.Id;
                }
            }
            else if (rsrp < -59 && rsrp >= -81)
            {
                if (Arrstyles[9] == null)
                {
                    style.Id = "2gCellGood";
                    style.Line = linestyle;
                    style.Line.Color = new Color32(Color32.Parse("6414F0FF").Abgr);
                    style.Line.Width = 10;
                    document.AddStyle(style);
                    Arrstyles[9] = style.Id;
                }
            }
            else if (rsrp < -81 && rsrp >= -101)
            {
                if (Arrstyles[10] == null)
                {
                    style.Id = "2gCellMid";
                    style.Line = linestyle;
                    style.Line.Color = new Color32(Color32.Parse("641478FF").Abgr);
                    style.Line.Width = 10;
                    document.AddStyle(style);
                    Arrstyles[10] = style.Id;
                }
            }
            else if (rsrp < -101)
            {
                if (Arrstyles[11] == null)
                {
                    style.Id = "2gCellEdge";
                    style.Line = linestyle;
                    style.Line.Color = new Color32(Color32.Parse("641400FF").Abgr);
                    style.Line.Width = 10;
                    document.AddStyle(style);
                    Arrstyles[11] = style.Id;
                }
            }
        }
        
        public static Document CreatePointDocument(string name)
        {
            var createPointDocument = new Document
            {
                Name = name
            };
            return createPointDocument;
        }

        public static void DeleteTempFiles(string tempFileName)
        {
            var intempPath = Path.Combine(tempPath, tempFileName);

            if (File.Exists(intempPath))
                File.Delete(intempPath);
        }

        public static Document CreateHandoverPoints(Document document, string[] workArray)
        {
            //Initialize Variables
            var id = 1;
            var handoverInd = false;
            var firstHandoverInd = true;
            var cellnew = string.Empty;
            var newinternalnetworktype = string.Empty;
            
            //Initialize documents
            document.Name = "Handover Points";
            document.Visibility = false;
            
            try
            {
                foreach (var item in workArray)
                {
                    //Initialize variables
                    var itemArr = item.Split(',');
                    var time = itemArr[0];
                    var campedcell = string.Empty;
                    var internalNwType = itemArr[5].ToLower(); //Todo changed in this version
                    
                    //internal Network Type Initialization
                    if (internalNwType == "umts" || internalNwType == "hsdpa" ||
                        internalNwType == "hsupa" || internalNwType == "hspap" ||
                        internalNwType == "hspa" || internalNwType == "tdscdma")
                    {
                        internalNwType = "umts";
                        InformationElementNwType = internalNwType;
                    }
                    else if (internalNwType == "gsm" || internalNwType == "gprs" || internalNwType == "edge")
                    {
                        internalNwType = "gsm";
                        InformationElementNwType = internalNwType;
                    }
                    else if (internalNwType == "lte")
                    {
                        internalNwType = "lte";
                        InformationElementNwType = internalNwType;
                    }

                    if (internalNwType != "unknown")
                    {
                        //Determine the camped cell for the Certain Network Type
                        if (internalNwType == "lte")
                            campedcell = itemArr[9];
                        else if (internalNwType == "gsm")
                            campedcell = itemArr[10];
                        else
                            campedcell = itemArr[10];


                        if (firstHandoverInd)
                        {
                            //firstHandoverInd = false;
                            if (InformationElementNwType == "lte")
                            {
                                //If campedcell ODD to cellnew AND cellnew is NOT empty the if branch will be taken otherwise the else
                                if (campedcell != cellnew && cellnew != string.Empty)
                                {
                                    campedcell = cellnew;
                                    cellnew = itemArr[9];
                                }
                                else
                                {
                                    cellnew = campedcell;
                                }

                                //If internalnwtype is ODD to newinternalnetworktype OR newnetworktype is NOT empty
                                if (internalNwType != newinternalnetworktype && newinternalnetworktype != string.Empty)
                                {
                                    internalNwType = newinternalnetworktype;
                                    newinternalnetworktype = itemArr[5].ToLower();
                                }
                                else
                                {
                                    newinternalnetworktype = InformationElementNwType;
                                }

                            }
                            else if (InformationElementNwType == "gsm")
                            {
                                if (campedcell != cellnew && cellnew != string.Empty)
                                {
                                    campedcell = cellnew;
                                    cellnew = itemArr[10];
                                }
                                else
                                {
                                    cellnew = campedcell;
                                }

                                if (InformationElementNwType != newinternalnetworktype ||
                                    newinternalnetworktype != string.Empty)
                                {
                                    internalNwType = newinternalnetworktype;
                                    newinternalnetworktype = itemArr[5].ToLower();
                                }
                                else
                                {
                                    newinternalnetworktype = InformationElementNwType;
                                }
                            }
                            else
                            {
                                if (campedcell != cellnew && cellnew != string.Empty)
                                {
                                    campedcell = cellnew;
                                    cellnew = itemArr[10];
                                }
                                else
                                {
                                    cellnew = campedcell;
                                    newinternalnetworktype = InformationElementNwType;
                                }

                                if (InformationElementNwType != newinternalnetworktype &&
                                    newinternalnetworktype != string.Empty)
                                {
                                    internalNwType = newinternalnetworktype;
                                    newinternalnetworktype = itemArr[5].ToLower();
                                }
                                else
                                {
                                    newinternalnetworktype = InformationElementNwType;
                                }
                            }

                            //internal Network Type Initialization
                            if (newinternalnetworktype == "umts" || newinternalnetworktype == "hsdpa" ||
                                newinternalnetworktype == "hsupa" || newinternalnetworktype == "hspap" ||
                                newinternalnetworktype == "hspa" || newinternalnetworktype == "tdscdma")
                            {
                                newinternalnetworktype = "umts";
                            }
                            else if (newinternalnetworktype == "gsm" || newinternalnetworktype == "gprs" ||
                                     newinternalnetworktype == "edge")
                            {
                                newinternalnetworktype = "gsm";
                            }
                            else if (newinternalnetworktype == "lte")
                            {
                                newinternalnetworktype = "lte";
                            }
                        }

                        if (campedcell != cellnew)
                            handoverInd = true;

                        if (InformationElementNwType != newinternalnetworktype)
                        {
                            handoverInd = true;
                            newinternalnetworktype = InformationElementNwType;
                        }


                        if (handoverInd)
                        {
                            if (InformationElementNwType == "lte")
                            {
                                if (firstHandoverInd == false)
                                {
                                    campedcell = cellnew;
                                    cellnew = itemArr[9];
                                }
                                document.AddStyle(HandoverPointStyle(Convert.ToString(id), time, campedcell, cellnew,
                                    itemArr[20], itemArr[10], itemArr[21]));
                                document.AddFeature(HandoverPoint("HoStyle" + id, itemArr[1], itemArr[2], itemArr[19]));
                            }
                            else if (InformationElementNwType == "umts")
                            {
                                if (firstHandoverInd == false)
                                {
                                    campedcell = cellnew;
                                    cellnew = itemArr[10];
                                }
                                document.AddStyle(HandoverPointStyle(Convert.ToString(id), time, campedcell, cellnew,
                                    itemArr[16], itemArr[11], itemArr[17]));
                                document.AddFeature(HandoverPoint("HoStyle" + id, itemArr[1], itemArr[2], itemArr[15]));
                            }
                            else if (InformationElementNwType == "gsm")
                            {
                                if (firstHandoverInd == false)
                                {
                                    campedcell = cellnew;
                                    cellnew = itemArr[10];
                                }
                                document.AddStyle(HandoverPointStyle(Convert.ToString(id), time, campedcell, cellnew,
                                    itemArr[17], itemArr[12], itemArr[18]));
                                document.AddFeature(HandoverPoint("HoStyle" + id, itemArr[1], itemArr[2], itemArr[16]));
                            }

                            id++;
                            handoverInd = false;
                            firstHandoverInd = false;
                        }
                    }
                }
                return document; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        public static Document CreateMeasPoints(Document document, string[] workArray)
        {
            var id = 0;
            try
            {
                id = workArray.Length;
                document.Name = "Measurement Points";
                document.Visibility = false;

                foreach (var item in workArray)
                {
                    /*if (id == 274)
                    {

                    }*/

                    var itemArr = item.Split(',');

                    var internalNwType = itemArr[5].ToLower(); //Todo changed in this version

                    //Todo Added in this Version
                    if (internalNwType == "umts" || internalNwType == "hsdpa" ||
                        internalNwType == "hsupa" || internalNwType == "hspap" ||
                        internalNwType == "hspa" || internalNwType == "tdscdma")
                        internalNwType = "umts";

                    if (internalNwType == "gsm" || internalNwType == "edge" || internalNwType == "gprs")
                        internalNwType = "gsm";

                    if (internalNwType == "lte" && itemArr.Length == 27 && Convert.ToInt32(itemArr[itemArr.Length-1])>=28)
                    {
                        var time = itemArr[0];
                        var lat = itemArr[1];
                        var lon = itemArr[2];
                        var spd = itemArr[3];
                        var hnd = itemArr[4];
                        var nwtype = "LTE";
                        var mcc = itemArr[7];
                        var mnc = itemArr[8];
                        var ci = itemArr[9];
                        var pci = itemArr[10];
                        var tac = itemArr[11];
                        var earfcn = itemArr[12];
                        var ss = itemArr[13];
                        var rsrp = itemArr[14];
                        var rsrq = itemArr[15];
                        var rssnr = itemArr[16];
                        var cqi = itemArr[17];
                        var ta = itemArr[18];
                        var location = itemArr[20];
                        var provider = itemArr[21];
                        var accuracy = itemArr[22];
                        var isroaming = itemArr[23];
                        var poi = itemArr[24];
                        var chbw = itemArr[25];

                        document.AddStyle(MeasPointStyle(Convert.ToString(id), time, lat, lon, location,
                            (Convert.ToDouble(spd) * 3.6).ToString("F1"), hnd, nwtype, mcc, mnc, ci, pci, tac, earfcn, ss,
                            rsrp, rsrq, rssnr, cqi, ta, provider, Convert.ToDouble(accuracy),Convert.ToBoolean(isroaming), Convert.ToBoolean(poi), Convert.ToDouble(chbw)));
                        document.AddFeature(MeasPoints("BalloonStyle" + id, itemArr[5], itemArr[1], itemArr[2],
                            itemArr[19]));
                    }
                    else if (internalNwType == "lte" && itemArr.Length == 25)
                    {
                        var time = itemArr[0];
                        var lat = itemArr[1];
                        var lon = itemArr[2];
                        var spd = itemArr[3];
                        var hnd = itemArr[4];
                        var nwtype = "LTE";
                        var mcc = itemArr[7];
                        var mnc = itemArr[8];
                        var ci = itemArr[9];
                        var pci = itemArr[10];
                        var tac = itemArr[11];
                        var earfcn = itemArr[12];
                        var ss = itemArr[13];
                        var rsrp = itemArr[14];
                        var rsrq = itemArr[15];
                        var rssnr = itemArr[16];
                        var cqi = itemArr[17];
                        var ta = itemArr[18];
                        var location = itemArr[20];
                        var provider = itemArr[21];
                        var accuracy = itemArr[22];
                        var isroaming = itemArr[23];
                        var poi = itemArr[24];

                        document.AddStyle(MeasPointStyle(Convert.ToString(id), time, lat, lon, location,
                            (Convert.ToDouble(spd) * 3.6).ToString(), hnd, nwtype, mcc, mnc, ci, pci, tac, earfcn, ss,
                            rsrp, rsrq, rssnr, cqi, ta, provider, Convert.ToDouble(accuracy),
                            Convert.ToBoolean(isroaming), Convert.ToBoolean(poi),-1));
                        document.AddFeature(MeasPoints("BalloonStyle" + id, itemArr[5], itemArr[1], itemArr[2],
                            itemArr[19]));
                    }
                    else if (internalNwType == "umts" && (itemArr.Length == 21|| itemArr.Length == 22))
                    {
                        var time = itemArr[0];
                        var lat = itemArr[1];
                        var lon = itemArr[2];
                        var spd = itemArr[3];
                        var hnd = itemArr[4];
                        var nwtype = "UMTS";
                        var mcc = itemArr[7];
                        var mnc = itemArr[8];
                        var lac = itemArr[9];
                        var ci = itemArr[10];
                        var psc = itemArr[11];
                        var uarfcn = itemArr[12];
                        var rssi = itemArr[13];
                        var ber = itemArr[14];
                        var location = itemArr[16];
                        var provider = itemArr[17];
                        var accuracy = itemArr[18];
                        var isroaming = itemArr[19];
                        var poi = itemArr[20];

                        document.AddStyle(MeasPointStyle3G(Convert.ToString(id), time, lat, lon, location,
                            (Convert.ToDouble(spd) * 3.6).ToString(), hnd, nwtype, mcc, mnc, lac, ci, psc, uarfcn, rssi,
                            ber, provider, Convert.ToDouble(accuracy), Convert.ToBoolean(isroaming),
                            Convert.ToBoolean(poi)));
                        document.AddFeature(MeasPoints("BalloonStyle" + id, itemArr[5], itemArr[1], itemArr[2],
                            itemArr[15]));
                    }
                    else if (internalNwType == "gsm" && (itemArr.Length == 22|| itemArr.Length == 23))
                    {
                        //Output String Order TimeStamp; Latitude; Longitude; Speed; Heading; Network Type; Registered; MMC; MNC; LAC; Cid;
                        //Uarfcn; BSIC; RSSI; Ber; TA; Address; Provider

                        var time = itemArr[0];
                        var lat = itemArr[1];
                        var lon = itemArr[2];
                        var spd = itemArr[3];
                        var hnd = itemArr[4];
                        var nwtype = "GSM";
                        var mcc = itemArr[7];
                        var mnc = itemArr[8];
                        var lac = itemArr[9];
                        var ci = itemArr[10];
                        var bsic = itemArr[12];
                        var uarfcn = itemArr[11];
                        var rssi = itemArr[13];
                        var ber = itemArr[14];
                        var ta = itemArr[15];
                        var location = itemArr[17];
                        var provider = itemArr[18];
                        var accuracy = itemArr[19];
                        var isroaming = itemArr[20];
                        var poi = itemArr[21];

                        document.AddStyle(MeasPointStyle2G(Convert.ToString(id), time, lat, lon, location,
                            (Convert.ToDouble(spd) * 3.6).ToString(), hnd, nwtype, mcc, mnc, lac, ci, bsic, uarfcn,
                            rssi, ber, ta, provider, Convert.ToDouble(accuracy), Convert.ToBoolean(isroaming),
                            Convert.ToBoolean(poi)));
                        document.AddFeature(MeasPoints("BalloonStyle" + id, itemArr[5], itemArr[1], itemArr[2],
                            itemArr[16]));
                    }
                   
                    id--;
                }

                return document;
            }
            catch (Exception ex)
            {
                var style = new Style();
                MessageBox.Show(ex.Message + " " +id);
                return document;
            }
        }

        public static string GetFileName(string path)
        {
            var outname = "";

            if (Kmlfoldername == string.Empty)
            {
                var arr = path.Split(Convert.ToChar(92));
                outname = arr[arr.Length - 1];
                outname = outname.Substring(0, outname.Length - 4);
            }
            else
            {
                outname = Kmlfoldername;
            }

            return outname;
        }

        private void btnopenfile_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = LogInputfoldername;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Log Files|*.log|Text Files|*.txt|All Files|*.*";
            openFileDialog1.Multiselect = false;
            openFileDialog1.AddExtension = true;
            openFileDialog1.AutoUpgradeEnabled = true;
            
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                RawPath = openFileDialog1.FileName;
                string[] displayfilename = new string[RawPath.Split('\\').Length];
                displayfilename=RawPath.Split('\\');

                //write the filepath to new Log Input Folder
                if (displayfilename.Length >1)
                {
                    LogInputfoldername = "";

                    for (int i = 0; i < displayfilename.Length-1; i++)
                    {
                        LogInputfoldername = LogInputfoldername + displayfilename[i] + "\\";
                    }

                    LogInputfoldername = LogInputfoldername.Substring(0, LogInputfoldername.Length - 1);
                }

                if (displayfilename[displayfilename.Length - 1].StartsWith("Log-Part"))
                {
                    filenamekml = displayfilename[displayfilename.Length - 1].Replace("Log", displayfilename[displayfilename.Length - 2] ); 
                }
                else
                {
                    filenamekml = displayfilename[displayfilename.Length - 1];
                }
                textFolderName.Text = filenamekml.Substring(0, filenamekml.Length - 4);
                filenamekml = filenamekml.Replace(".log", ".kml");
                button1.Enabled = true;
                textFolderName.Enabled = true;
                grpBoxOutputName.Enabled = true;
                chkWriteKmlCaption.Enabled = true;
            }
        }
//Only in the PC code
        private void textFolderName_TextChanged(object sender, EventArgs e)
        {
            if (textFolderName.Text.Length > 0)
            {
                Kmlfoldername = textFolderName.Text;
                //filenamekml = Kmlfoldername;
            }
            else
                Kmlfoldername = string.Empty;
        }

        private static string[] checkForComma(string x)
        {
            var Arr = x.Split(';');

            var len = Arr.Length;


            for (var i = 0; i < len - 1; i++)
                if (Regex.IsMatch(Arr[i], @"^-?(?:\d+|\d{1,3}(?:,\d{3})+)(?:(\.|,)\d+)?$"))
                {
                    if (Arr[i].Contains(","))
                        Arr[i] = Arr[i].Replace(",", ".");
                }
                else
                {
                    if (Arr[i].Contains(","))
                        Arr[i] = Arr[i].Replace(",", ";");
                }

            return Arr;
        }

        private static string getCampedBand(string ran, int arfcn, string mcc, string mnc)
        {
            string band = null;
            string mccmnc = mcc + mnc;

            if (ran.ToLower() == "gsm")
            {
                //Todo GSM1800/1900 determination

                if (arfcn >= 258 && arfcn<=293)
                {
                    band = "GSM450";
                }
                else if (arfcn >= 306 && arfcn <= 340)
                {
                    band = "GSM480";
                }
                else if (arfcn >= 128 && arfcn <= 251)
                {
                    band = "GSM850";
                }
                else if ((arfcn >= 0 && arfcn <= 124)||(arfcn >= 955 && arfcn <= 1023))
                {
                    band = "GSM900";
                }
                else if (arfcn >= 512 && arfcn <= 855)
                {
                    band = "GSM1800";
                }
                else if (arfcn >= 512 && arfcn <= 810)
                {
                    band = "GSM1900";
                }

            }
            else if (ran.ToLower() == "umts")
            {
                if (arfcn >= 10562 && arfcn <= 10838)
                {
                    band = "FDD1";
                }
                else if (arfcn >= 9662 && arfcn <= 9938)
                {
                    band = "FDD2"; //Todo TDD36
                }
                else if (arfcn >= 1062 && arfcn <= 1513)
                {
                    band = "FDD3";
                }
                else if (arfcn >= 1537 && arfcn <= 1738)
                {
                    band = "FDD4";
                }
                else if (arfcn >= 4357 && arfcn <= 4458)
                {
                    band = "FDD5"; //Todo FDD6
                }
                else if (arfcn >= 4357 && arfcn <= 4413)
                {
                    band = "FDD6"; //Todo FDD5
                }
                else if (arfcn >= 2237 && arfcn <= 2563)
                {
                    band = "FDD7";
                }
                else if (arfcn >= 2937 && arfcn <= 3088)
                {
                    band = "FDD8";
                }
                else if (arfcn >= 9237 && arfcn <= 9387)
                {
                    band = "FDD9";
                }
                else if (arfcn >= 3112 && arfcn <= 3388)
                {
                    band = "FDD10";
                }
                else if (arfcn >= 3712 && arfcn <= 3787)
                {
                    band = "FDD11";
                }
                else if (arfcn >= 3842 && arfcn <= 3903)
                {
                    band = "FDD12";
                }
                else if (arfcn >= 4017 && arfcn <= 4043)
                {
                    band = "FDD13";
                }
                else if (arfcn >= 4117 && arfcn <= 4143)
                {
                    band = "FDD14";
                }
                else if (arfcn >= 712 && arfcn <= 763)
                {
                    band = "FDD19";
                }
                else if (arfcn >= 4512 && arfcn <= 4638)
                {
                    band = "FDD20";
                }
                else if (arfcn >= 862 && arfcn <= 912)
                {
                    band = "FDD21";
                }
                else if (arfcn >= 4662 && arfcn <= 5038)
                {
                    band = "FDD22";
                }
                else if (arfcn >= 5112 && arfcn <= 5413)
                {
                    band = "FDD25";
                }
                else if (arfcn >= 5762 && arfcn <= 5913)
                {
                    band = "FDD26";
                }
                else if (arfcn >= 6617 && arfcn <= 6813)
                {
                    band = "FDD32";
                }
                else if (arfcn >= 9500 && arfcn <= 9600)
                {
                    band = "TDD33"; //Todo TDD35/37
                }
                else if (arfcn >= 10050 && arfcn <= 10125)
                {
                    band = "TDD34";
                }
                else if (arfcn >= 9250 && arfcn <= 9550)
                {
                    band = "TDD35"; //Todo TDD33/37
                }
                else if (arfcn >= 9650 && arfcn <= 9950)
                {
                    band = "TDD36"; //Todo FDD2
                }
                else if (arfcn >= 9550 && arfcn <= 9650)
                {
                    band = "TDD37"; //Todo TDD33/39/35
                }
                else if (arfcn >= 12850 && arfcn <= 13100)
                {
                    band = "TDD38";
                }
                else if (arfcn >= 9400 && arfcn <= 9600)
                {
                    band = "TDD39"; //Todo TDD33/37
                }
                else if (arfcn >= 11500 && arfcn <= 12000)
                {
                    band = "TDD40";
                }
            }
            else if (ran.ToLower() == "lte")
            {
                if (arfcn >= 1 && arfcn <= 599)
                {
                    band = "eFDD1";
                }
                else if (arfcn >= 600 && arfcn <= 1199)
                {
                    band = "eFDD2";
                }
                else if (arfcn >= 1200 && arfcn <= 1949)
                {
                    band = "eFDD3";
                }
                else if (arfcn >= 1950 && arfcn <= 2399)
                {
                    band = "eFDD4";
                }
                else if (arfcn >= 2400 && arfcn <= 2649)
                {
                    band = "eFDD5";
                }
                else if (arfcn >= 2650 && arfcn <= 2749)
                {
                    band = "eFDD6";
                }
                else if (arfcn >= 2750 && arfcn <= 3449)
                {
                    band = "eFDD7";
                }
                else if (arfcn >= 3450 && arfcn <= 3799)
                {
                    band = "eFDD8";
                }
                else if (arfcn >= 3800 && arfcn <= 4149)
                {
                    band = "eFDD9";
                }
                else if (arfcn >= 4150 && arfcn <= 4749)
                {
                    band = "eFDD10";
                }
                else if (arfcn >= 4750 && arfcn <= 4949)
                {
                    band = "eFDD11";
                }
                else if (arfcn >= 5010 && arfcn <= 5179)
                {
                    band = "eFDD12";
                }
                else if (arfcn >= 5180 && arfcn <= 5279)
                {
                    band = "eFDD13";
                }
                else if (arfcn >= 5280 && arfcn <= 5379)
                {
                    band = "eFDD14";
                }
                else if (arfcn >= 5730 && arfcn <= 5849)
                {
                    band = "eFDD17";
                }
                else if (arfcn >= 5850 && arfcn <= 5999)
                {
                    band = "eFDD18";
                }
                else if (arfcn >= 6000 && arfcn <= 6149)
                {
                    band = "eFDD19";
                }
                else if (arfcn >= 6150 && arfcn <= 6449)
                {
                    band = "eFDD20";
                }
                else if (arfcn >= 6450 && arfcn <= 6599)
                {
                    band = "eFDD21";
                }
                else if (arfcn >= 6600 && arfcn <= 7399)
                {
                    band = "eFDD22";
                }
                else if (arfcn >= 7500 && arfcn <= 7699)
                {
                    band = "eFDD23";
                }
                else if (arfcn >= 7700 && arfcn <= 8039)
                {
                    band = "eFDD24";
                }
                else if (arfcn >= 8040 && arfcn <= 8689)
                {
                    band = "eFDD25";
                }
                else if (arfcn >= 8690 && arfcn <= 9039)
                {
                    band = "eFDD26";
                }
                else if (arfcn >= 9040 && arfcn <= 9209)
                {
                    band = "eFDD27";
                }
                else if (arfcn >= 9210 && arfcn <= 9659)
                {
                    band = "eFDD28";
                }
                else if (arfcn >= 9660 && arfcn <= 9769)
                {
                    band = "eFDD29";
                }
                else if (arfcn >= 9770 && arfcn <= 9869)
                {
                    band = "eFDD30";
                }
                else if (arfcn >= 9870 && arfcn <= 9919)
                {
                    band = "eFDD31";
                }
                else if (arfcn >= 9920 && arfcn <= 10359)
                {
                    band = "eFDD32";
                }
                else if (arfcn >= 65536 && arfcn <= 66435)
                {
                    band = "eFDD65";
                }
                else if (arfcn >= 66436 && arfcn <= 67335)
                {
                    band = "eFDD66";
                }
                else if (arfcn >= 67336 && arfcn <= 67535)
                {
                    band = "eFDD67";
                }
                else if (arfcn >= 67536 && arfcn <= 67835)
                {
                    band = "eFDD68";
                }
                else if (arfcn >= 67836 && arfcn <= 68335)
                {
                    band = "eFDD69";
                }
                else if (arfcn >= 68336 && arfcn <= 68585)
                {
                    band = "eFDD70";
                }
                else if (arfcn >= 68586 && arfcn <= 68935)
                {
                    band = "eFDD71";
                }
                else if (arfcn >= 68936 && arfcn <= 68985)
                {
                    band = "eFDD72";
                }
                else if (arfcn >= 68986 && arfcn <= 69035)
                {
                    band = "eFDD73";
                }
                else if (arfcn >= 69036 && arfcn <= 69465)
                {
                    band = "eFDD74";
                }
                else if (arfcn >= 69466 && arfcn <= 70315)
                {
                    band = "eFDD75";
                }
                else if (arfcn >= 70316 && arfcn <= 70365)
                {
                    band = "eFDD76";
                }
                else if (arfcn >= 255144 && arfcn <= 256143)
                {
                    band = "eFDD252";
                }
                else if (arfcn >= 260894 && arfcn <= 262143)
                {
                    band = "eFDD255";
                }
                else if (arfcn >= 36000 && arfcn <= 36199)
                {
                    band = "eTDD33";
                }
                else if (arfcn >= 36200 && arfcn <= 36349)
                {
                    band = "eTDD34";
                }
                else if (arfcn >= 36350 && arfcn <= 36949)
                {
                    band = "eTDD35";
                }
                else if (arfcn >= 36950 && arfcn <= 37549)
                {
                    band = "eTDD36";
                }
                else if (arfcn >= 37550 && arfcn <= 37749)
                {
                    band = "eTDD37";
                }
                else if (arfcn >= 37750 && arfcn <= 38249)
                {
                    band = "eTDD38";
                }
                else if (arfcn >= 38250 && arfcn <= 38649)
                {
                    band = "eTDD39";
                }
                else if (arfcn >= 38650 && arfcn <= 39649)
                {
                    band = "eTDD40";
                }
                else if (arfcn >= 39650 && arfcn <= 41589)
                {
                    band = "eTDD41";
                }
                else if (arfcn >= 41590 && arfcn <= 43589)
                {
                    band = "eTDD42";
                }
                else if (arfcn >= 43590 && arfcn <= 45589)
                {
                    band = "eTDD43";
                }
                else if (arfcn >= 45590 && arfcn <= 46589)
                {
                    band = "eTDD44";
                }
                else if (arfcn >= 46590 && arfcn <= 46789)
                {
                    band = "eTDD45";
                }
                else if (arfcn >= 46790 && arfcn <= 54539)
                {
                    band = "eTDD46";
                }
                else if (arfcn >= 54540 && arfcn <= 55239)
                {
                    band = "eTDD47";
                }
                else if (arfcn >= 55240 && arfcn <= 56739)
                {
                    band = "eTDD48";
                }
                else if (arfcn >= 56740 && arfcn <= 58239)
                {
                    band = "eTDD49";
                }
                else if (arfcn >= 58240 && arfcn <= 59089)
                {
                    band = "eTDD50";
                }
                else if (arfcn >= 59090 && arfcn <= 59139)
                {
                    band = "eTDD51";
                }
            }
            return band;
        }

        private void chkWriteKmlCaption_CheckedChanged(object sender, EventArgs e)
        {
            WriteKmlCaption = chkWriteKmlCaption.Checked;
        }

        private void LoadSettings()
        {
            if (File.Exists(Path.Combine(Application.StartupPath, "Settings.xml")))
            {
                var settings = File.ReadAllText(Path.Combine(Application.StartupPath, "Settings.xml"));
                ReadSettingsXML(settings);
            }
        }

        private static void WriteSettingsXmlToFile()
        {
            try
            {
                if (File.Exists(Path.Combine(Application.StartupPath, "Settings.xml")))
                {
                    File.Delete(Path.Combine(Application.StartupPath, "Settings.xml"));
                }

                // Use StringWriter as backing for XmlTextWriter.
                using (StringWriter str = new StringWriter())
                using (XmlTextWriter xml = new XmlTextWriter(str))
                {
                    // Root.
                    xml.WriteStartDocument();
                    xml.WriteWhitespace("\n");
                    xml.WriteStartElement("AppSettings");
                    xml.WriteWhitespace("\n\t");
                    xml.WriteElementString("FileDestination", Kmloutputfoldername);
                    xml.WriteWhitespace("\n\t");
                    xml.WriteElementString("FileSource", LogInputfoldername);
                    xml.WriteWhitespace("\n\t");
                    xml.WriteElementString("WriteKmlCaption", WriteKmlCaption.ToString());
                    xml.WriteWhitespace("\n");
                    xml.WriteEndElement();
                    xml.WriteEndDocument();
                    
                    string result = str.ToString();
                    File.WriteAllText(Path.Combine(Application.StartupPath, "Settings.xml"),result);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private static void WriteInvalidLogFiles(string InvalidData)
        {
            // Write single line to new file.
            using (StreamWriter writer = new StreamWriter(Path.Combine(Application.StartupPath, "InvalidLogData.txt"), true))
            {
                writer.WriteLine(DateTime.Now + " : " + InvalidData );
            }
           
        }

        
        private static void ReadSettingsXML(string xmlNode)
        {
            var settingsname = "";

            XmlReader xReader = XmlReader.Create(new StringReader(xmlNode));
            while (xReader.Read())
            {
                switch (xReader.NodeType)
                {
                    case XmlNodeType.Element:
                        settingsname = xReader.Name;
                        break;
                    case XmlNodeType.Text:

                        if (settingsname.ToLower()=="filedestination")
                        {
                            Kmloutputfoldername = xReader.Value;
                        }
                        else if (settingsname.ToLower() == "filesource")
                        {
                            LogInputfoldername = xReader.Value;
                        }
                        else if (settingsname.ToLower()=="writekmlcaption")
                        {
                            WriteKmlCaption = Convert.ToBoolean(xReader.Value);
                        }
                       break;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteSettingsXmlToFile();
        }

        private void btnSetSavLoc_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = Kmloutputfoldername;
            folderBrowserDialog1.ShowNewFolderButton = true;
            
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                Kmloutputfoldername = folderBrowserDialog1.SelectedPath;
                setLblFileDest();
               
            }
        }

        private void lblFileDest_MouseHover(object sender, EventArgs e)
        {
            ToolTip ttFileDest = new ToolTip();
            ttFileDest.ShowAlways = true;
            ttFileDest.SetToolTip(lblFileDest, Kmloutputfoldername);
        }

        private void setLblFileDest()
        {
            string [] lblFileDestArr = new string[Kmloutputfoldername.Split('\\').Length];

            if (lblFileDestArr.Length>3)
            {
                lblFileDestArr = Kmloutputfoldername.Split('\\');
                lblFileDest.Text = "KML Folder ->  " + lblFileDestArr[0] + "\\" + lblFileDestArr[1] + "\\...\\" + lblFileDestArr[lblFileDestArr.Length - 1]; 
            }
            else
            {
                lblFileDest.Text = "KML Folder ->  " + Kmloutputfoldername;
            }
        }

        private void chkLastPathExists()
        {
            if (!Directory.Exists(Kmloutputfoldername))
            {
                Kmloutputfoldername = Path.Combine(Application.StartupPath, "KML");
            }
        }

        private void dataReduction()
        {
            /*Data reduction process
             * 1. read log line by line
             * 2. check network type Cell Id and signal strength 
             * 3. if compare variable is empty or not matching, write into new file
             * 4. store the three parameter in a compare variable
             * 5. repeat until eof  
             */


            var workingArr = ReadlogRedux();
            var compare = "";
            String [] reduced = new string[workingArr.Length];
            int counter = 0;
            
            foreach (var line in workingArr)
            {
                //using (StreamWriter sw = File.AppendText(Path.Combine(Application.StartupPath, "Reduced.log")))

                var acc = line.Split(';');

                var internalNwType = acc[5].ToLower();

                if (internalNwType == "umts" || internalNwType == "hsdpa" ||
                    internalNwType == "hsupa" || internalNwType == "hspap" ||
                    internalNwType == "hspa" || internalNwType == "tdscdma")
                    internalNwType = "umts";

                if (internalNwType == "gsm" || internalNwType == "gprs" || internalNwType == "edge")
                    internalNwType = "gsm";

                //Check for unknown signal strength

                if (internalNwType == "gsm")
                {
                    if (acc[13] != "unknown")
                    {
                        var check = acc[5] + ";" + acc[13];

                        if (compare==String.Empty || check != compare)
                        {
                            //MessageBox.Show("write to File");
                            reduced[counter] = line;
                            counter++;
                            compare = check;
                        }
                        /*else
                        {
                            MessageBox.Show("equal");
                        }*/
                    }
                }
                else if (internalNwType == "umts")
                {
                    if (acc[13] != "unknown")
                    {
                        var check = acc[5] + ";" + acc[13];

                        if (compare == String.Empty || check != compare)
                        {
                            //MessageBox.Show("write to File");
                            reduced[counter] = line;
                            counter++;
                            compare = check;
                        }
                        /*else
                        {
                            MessageBox.Show("equal");
                        }*/
                    }
                }
                else if (internalNwType == "lte")
                {
                    if (acc[14] != "unknown")
                    {
                        var check = acc[5] + ";" + acc[14];

                        if (compare == String.Empty || check != compare)
                        {
                            //MessageBox.Show("write to File");
                            reduced[counter] = line;
                            counter++;
                            compare = check;
                        }
                        /*else
                        {
                            MessageBox.Show("equal");
                        }*/
                    }
                    
                }
            }

            if (!File.Exists(Path.Combine(Application.StartupPath,"KML", "Reduced.log")))
            {
                // Create a file to write to.
                File.WriteAllLines(Path.Combine(Application.StartupPath, "KML", "Reduced.log"), reduced);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            dataReduction();
        }

        private static string MeasIconSelector(string rsrpIn, string internalNwType)
        {
            string iconSelect = "error.png";
            
            int rsrp = Convert.ToInt16(rsrpIn);

            if (rsrp > 0)
            {
                rsrp = rsrp * -1;
            }

            try
            {
                switch (internalNwType.ToLower())
                {
                    case "lte":
                        // RSRP     >=-69       -69 to -92  -92 to -99     <= -100     dBm


                        if (rsrp >= -69)
                        {
                            iconSelect = "http://e-i-t.de/PNG/lteexec.png";
                        }
                        else if (rsrp < -69 && rsrp >= -92)
                        {
                            iconSelect = "http://e-i-t.de/PNG/ltegood.png";
                        }
                        else if (rsrp < -92 && rsrp >= -99)
                        {
                            iconSelect = "http://e-i-t.de/PNG/ltemid.png";
                        }
                        else if (rsrp <= -100)
                        {
                            iconSelect = "http://e-i-t.de/PNG/lteedge.png";
                        }
                        break;
                    case "gsm":
                        // RSSI     >=-59       -59 to -81  -81 to -101     <= -101     dBm

                        if (rsrp >= -59)
                        {
                            iconSelect = "http://e-i-t.de/PNG/gsmexec.png";
                        }
                        else if (rsrp < -59 && rsrp >= -81)
                        {
                            iconSelect = "http://e-i-t.de/PNG/gsmgood.png";
                        }
                        else if (rsrp < -81 && rsrp >= -101)
                        {
                            iconSelect = "http://e-i-t.de/PNG/gsmmid.png";
                        }
                        else if (rsrp < -101)
                        {
                            iconSelect = "http://e-i-t.de/PNG/gsmedge.png";
                        }

                        break;
                    case "cdma":
                        // RSSI     >=-68       -68 to -84  -84 to -104     <= -104     dBm

                        if (rsrp >= -68)
                        {
                            iconSelect = "http://e-i-t.de/PNG/cdmaexec.png";
                        }
                        else if (rsrp < -68 && rsrp >= -84)
                        {
                            iconSelect = "http://e-i-t.de/PNG/cmdagood.png";
                        }
                        else if (rsrp < -84 && rsrp >= -104)
                        {
                            iconSelect = "http://e-i-t.de/PNG/cdmamid.png";
                        }
                        else if (rsrp < -104)
                        {
                            iconSelect = "http://e-i-t.de/PNG/cdmaedge.png";
                        }
                        break;
                    case "umts":
                        // RSSI     >=-68       -68 to -84  -84 to -104     <= -104     dBm
                       if (rsrp >= -68)
                        {
                            iconSelect = "http://e-i-t.de/PNG/umtsexec.png";
                        }
                        else if (rsrp < -68 && rsrp >= -84)
                        {
                            iconSelect = "http://e-i-t.de/PNG/umtsgood.png";
                        }
                        else if (rsrp < -84 && rsrp >= -104)
                        {
                            iconSelect = "http://e-i-t.de/PNG/umtsmid.png";
                        }
                        else if (rsrp < -104)
                        {
                            iconSelect = "http://e-i-t.de/PNG/umtsedge.png";
                        }
                        break;
                }

                return (iconSelect);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return ("");
            }
        }
    }
}