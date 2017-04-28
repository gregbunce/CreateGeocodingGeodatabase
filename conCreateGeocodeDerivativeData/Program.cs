using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.ADF;


namespace conCreateGeocodeDerivativeData
{
    class Program
    {
        private static LicenseInitializer m_AOLicenseInitializer = new conCreateGeocodeDerivativeData.LicenseInitializer();

        [STAThread()]
        static void Main(string[] args)
        {
            try
            {
                //ESRI License Initializer generated code
                //try to check out an arcinfo license
                if (!m_AOLicenseInitializer.InitializeApplication(new esriLicenseProductCode[] { esriLicenseProductCode.esriLicenseProductCodeAdvanced },
                new esriLicenseExtensionCode[] { }))
                {
                    //if the license could not be initalized, shut it down
                    System.Console.WriteLine(m_AOLicenseInitializer.LicenseMessage());
                    System.Console.WriteLine("This application could not initialize with the correct ArcGIS license and will shutdown.");
                    m_AOLicenseInitializer.ShutdownApplication();
                    return;
                }
                Console.WriteLine("Checked out a license");

                // get/create the name for the new feature class of derived roads
                clsGlobals.strNewGeocodeFeatClassName = "GeocodeRoads";
                clsGlobals.strNewGeocodeTableName = "GeocodeRoadAtlNames";

                // connect to sgid
                clsGlobals.arcWorkspaceSGID = clsStaticMethods.ConnectToTransactionalVersion("", "sde:sqlserver:sgid.agrc.utah.gov", "SGID10", "DBMS", "sde.DEFAULT", "agrc", "agrc");
                clsGlobals.arcFeatureWorkspaceSGID = (IFeatureWorkspace)clsGlobals.arcWorkspaceSGID;

                // get the source roads feature class (SGID)
                clsGlobals.arcFeatClass_SGIDRoads = clsGlobals.arcFeatureWorkspaceSGID.OpenFeatureClass(args[0]);

                //// check if feature class was found
                //if (clsGlobals.arcFeatClass_Roads != null)
                //{
                //    Console.WriteLine("Got access to the SGID feature class named: " + args[0]);
                //}
                //else
                //{
                //    Console.WriteLine("ERROR: Cannot get access to the SGID feature class named: " + args[0] + ". Correct format for SDE should look something like this: SGID10.TRANSPORTATION.FeatureClassName");
                //    //Console.ReadLine();  // make sure this line is commented out when running in forklift
                //    return;
                //}

                // create the file geodatabase for the derived geocoding data
                clsGlobals.arcWorkspaceGeocodeFGD = clsStaticMethods.CreateFileGdbWorkspace(args[1], args[2]);
                clsGlobals.arcFeatureWorkspaceGeocodeFGD = (IFeatureWorkspace)clsGlobals.arcWorkspaceGeocodeFGD;

                // check if the feature class and table exist in the file geodatabase - if so rename them before adding new data
                clsGlobals.arcWorkspace2GeocodeFGD = (IWorkspace2)clsGlobals.arcWorkspaceGeocodeFGD;
                bool blnFC_Exists = clsStaticMethods.NameExists(clsGlobals.strNewGeocodeFeatClassName, esriDatasetType.esriDTFeatureClass);
                bool blnTable_Exists = clsStaticMethods.NameExists(clsGlobals.strNewGeocodeTableName, esriDatasetType.esriDTTable);
                if (blnFC_Exists)
                {
                    // rename existing fc
                    IFeatureClass arcFC_ToRename = clsGlobals.arcFeatureWorkspaceGeocodeFGD.OpenFeatureClass(clsGlobals.strNewGeocodeFeatClassName);
                    clsGlobals.arcDatasetGeocodeFGD = (IDataset)arcFC_ToRename;
                    clsGlobals.arcDatasetGeocodeFGD.Rename(clsGlobals.strNewGeocodeFeatClassName + "OldOn" + DateTime.Now.ToString("yyyyMMdd"));
                    arcFC_ToRename = null;

                }
                if (blnTable_Exists)
                {
                    // rename existing table
                    ITable arcTable_ToRemane = clsGlobals.arcFeatureWorkspaceGeocodeFGD.OpenTable(clsGlobals.strNewGeocodeTableName);
                    clsGlobals.arcDatasetGeocodeFGD = (IDataset)arcTable_ToRemane;
                    clsGlobals.arcDatasetGeocodeFGD.Rename(clsGlobals.strNewGeocodeTableName + "OldOn" + DateTime.Now.ToString("yyyyMMdd"));
                    arcTable_ToRemane = null;

                }

                // create a feature class in the newly-created file geodatabase
                clsGlobals.arcFeatClass_GeocodeRoads = clsStaticMethods.CreateFeatureClass(clsGlobals.strNewGeocodeFeatClassName, null, clsGlobals.arcFeatureWorkspaceGeocodeFGD);

                // create a table in the newly-created file geodatabase
                clsGlobals.arcTable_AltNames = clsStaticMethods.CreateTable(clsGlobals.strNewGeocodeTableName, null, clsGlobals.arcFeatureWorkspaceGeocodeFGD);


                // create a feature cursor from the source roads data and loop through this subset
                // create the query filter to filter results
                string strQuery = string.Empty;
                strQuery = @"STREETNAME = 'PLATEAU'";
                clsGlobals.arcQueryFilter_SGIDRoads = new QueryFilter();
                clsGlobals.arcQueryFilter_SGIDRoads.WhereClause = strQuery;

                // create a ComReleaser for feature cursor's life-cycle management
                using (ComReleaser comReleaser = new ComReleaser())
                {
                    clsGlobals.arcFeatureCurSGIDRoads = clsGlobals.arcFeatClass_SGIDRoads.Search(clsGlobals.arcQueryFilter_SGIDRoads, false);
                    comReleaser.ManageLifetime(clsGlobals.arcFeatureCurSGIDRoads);

                    // loop through the sgid roads' feature cursor
                    while ((clsGlobals.arcFeat_SGIDRoad = clsGlobals.arcFeatureCurSGIDRoads.NextFeature()) != null)
                    {
                        // get the needed values from the source roads data
                        string strAddressSys = clsGlobals.arcFeat_SGIDRoad.get_Value(clsGlobals.arcFeat_SGIDRoad.Fields.FindField("ADDR_SYS")).ToString().Trim().ToUpper();
                        string strLeftFrom = clsGlobals.arcFeat_SGIDRoad.get_Value(clsGlobals.arcFeat_SGIDRoad.Fields.FindField("L_F_ADD")).ToString().Trim().ToUpper();
                        string strLeftTo = clsGlobals.arcFeat_SGIDRoad.get_Value(clsGlobals.arcFeat_SGIDRoad.Fields.FindField("L_T_ADD")).ToString().Trim().ToUpper();
                        string strRightFrom = clsGlobals.arcFeat_SGIDRoad.get_Value(clsGlobals.arcFeat_SGIDRoad.Fields.FindField("R_F_ADD")).ToString().Trim().ToUpper();
                        string strRightTo = clsGlobals.arcFeat_SGIDRoad.get_Value(clsGlobals.arcFeat_SGIDRoad.Fields.FindField("R_T_ADD")).ToString().Trim().ToUpper();
                        string strPredir = clsGlobals.arcFeat_SGIDRoad.get_Value(clsGlobals.arcFeat_SGIDRoad.Fields.FindField("PREDIR")).ToString().Trim().ToUpper();
                        string strStreetName = clsGlobals.arcFeat_SGIDRoad.get_Value(clsGlobals.arcFeat_SGIDRoad.Fields.FindField("STREETNAME")).ToString().Trim().ToUpper();
                        string strStreetType = clsGlobals.arcFeat_SGIDRoad.get_Value(clsGlobals.arcFeat_SGIDRoad.Fields.FindField("STREETTYPE")).ToString().Trim().ToUpper();
                        string strSufDir = clsGlobals.arcFeat_SGIDRoad.get_Value(clsGlobals.arcFeat_SGIDRoad.Fields.FindField("SUFDIR")).ToString().Trim().ToUpper();

                        // begin to populate the geocode feature class in the newly-created file geodatabase
                        



                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error with the conCreateGeocodeDerivativeData console application, in the Main method." + ex.Message + " " + ex.Source + " " + ex.InnerException + " " + ex.HResult + " " + ex.StackTrace + " " + ex);
                Console.ReadLine();
            }








        }
    }
}
