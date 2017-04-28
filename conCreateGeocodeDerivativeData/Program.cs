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
                strQuery = @"STREETNAME in ('PLATEAU','MAIN','100')";
                clsGlobals.arcQueryFilter_SGIDRoads = new QueryFilter();
                clsGlobals.arcQueryFilter_SGIDRoads.WhereClause = strQuery;

                // create a ComReleaser for feature cursor's life-cycle management
                using (ComReleaser comReleaser = new ComReleaser())
                {
                    clsGlobals.arcFeatureCurSGIDRoads = clsGlobals.arcFeatClass_SGIDRoads.Search(clsGlobals.arcQueryFilter_SGIDRoads, false);
                    comReleaser.ManageLifetime(clsGlobals.arcFeatureCurSGIDRoads);

                    // begin an edit session on the file geodatabase (maybe) that way we can roll back if it errors out
                    clsGlobals.arcWorkspaceEditGeocodeFGD = clsGlobals.arcFeatureWorkspaceGeocodeFGD as IWorkspaceEdit;
                    clsGlobals.arcWorkspaceEditGeocodeFGD.StartEditing(false);
                    clsGlobals.arcWorkspaceEditGeocodeFGD.StartEditOperation();

                    IFeature arcFeat_SGIDRoad = null;

                    // loop through the sgid roads' feature cursor
                    while ((arcFeat_SGIDRoad = clsGlobals.arcFeatureCurSGIDRoads.NextFeature()) != null)
                    {
                        // get the needed values from the source roads data
                        string strAddressSys = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("ADDR_SYS")).ToString().Trim().ToUpper();
                        double dblLeftFrom = Convert.ToDouble(arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("L_F_ADD")));
                        double dblLeftTo = Convert.ToDouble(arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("L_T_ADD")));
                        double dblRightFrom = Convert.ToDouble(arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("R_F_ADD")));
                        double dblRightTo = Convert.ToDouble(arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("R_T_ADD")));
                        string strPredir = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("PREDIR")).ToString().Trim().ToUpper();
                        string strStreetName = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("STREETNAME")).ToString().Trim().ToUpper();
                        string strStreetType = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("STREETTYPE")).ToString().Trim().ToUpper();
                        string strSufDir = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("SUFDIR")).ToString().Trim().ToUpper();
                        string strAlias1 = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("ALIAS1")).ToString().Trim().ToUpper();
                        string strAlias1Type = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("ALIAS1TYPE")).ToString().Trim().ToUpper();
                        string strAlias2 = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("ALIAS2")).ToString().Trim().ToUpper();
                        string strAlias2Type = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("ALIAS2TYPE")).ToString().Trim().ToUpper();
                        string strAcsAlias = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("ACSALIAS")).ToString().Trim().ToUpper();
                        string strAcsName = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("ACSNAME")).ToString().Trim().ToUpper();
                        string strAcsSuf = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("ACSSUF")).ToString().Trim().ToUpper();
                        string strZipLeft = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("ZIPLEFT")).ToString().Trim().ToUpper();
                        string strZipRight = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("ZIPRIGHT")).ToString().Trim().ToUpper();
                        string strGlobalID = arcFeat_SGIDRoad.get_Value(arcFeat_SGIDRoad.Fields.FindField("GLOBALID")).ToString().Trim().ToUpper();

                        // begin to populate the geocode feature class in the newly-created file geodatabase
                        // check if this segment has a streetname
                        if (strStreetName != "")
                        {
                            // create a new feature
                            clsStaticMethods.InsertFeatureGeocodeFGD(arcFeat_SGIDRoad, strAddressSys, dblLeftFrom, dblLeftTo, dblRightFrom, dblRightTo, strPredir, strStreetName,strStreetType,strSufDir,strZipRight, strZipLeft, strGlobalID);
                        }
                        // check if this segment has an alias name
                        if (strAlias1 != "")
                        {
                            // create a new feature
                            clsStaticMethods.InsertFeatureGeocodeFGD(arcFeat_SGIDRoad, strAddressSys, dblLeftFrom, dblLeftTo, dblRightFrom, dblRightTo, strPredir, strAlias1, strAlias1Type, strSufDir, strZipRight, strZipLeft, strGlobalID);
                        }
                        // check if this segment has a second alias name
                        if (strAlias2 != "")
                        {
                            // create a new feature
                            clsStaticMethods.InsertFeatureGeocodeFGD(arcFeat_SGIDRoad, strAddressSys, dblLeftFrom, dblLeftTo, dblRightFrom, dblRightTo, strPredir, strAlias2, strAlias2Type, strSufDir, strZipRight, strZipLeft, strGlobalID);
                        }
                        // check if this segment has an acs alias name
                        if (strAcsName != "")
                        {
                            // create a new feature
                            clsStaticMethods.InsertFeatureGeocodeFGD(arcFeat_SGIDRoad, strAddressSys, dblLeftFrom, dblLeftTo, dblRightFrom, dblRightTo, strPredir, strAcsName, strStreetType, strAcsSuf, strZipRight, strZipLeft, strGlobalID);
                        }




                    }
                    clsGlobals.arcWorkspaceEditGeocodeFGD.StopEditOperation();
                    clsGlobals.arcWorkspaceEditGeocodeFGD.StopEditing(true);

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error with the conCreateGeocodeDerivativeData console application, in the Main method." + ex.Message + " " + ex.Source + " " + ex.InnerException + " " + ex.HResult + " " + ex.StackTrace + " " + ex);
                Console.ReadLine();
            }
            finally
            {
                // stop editing and don't save the edits
                if (clsGlobals.arcWorkspaceEditGeocodeFGD != null & clsGlobals.arcWorkspaceEditGeocodeFGD.IsBeingEdited())
                {
                    clsGlobals.arcWorkspaceEditGeocodeFGD.StopEditOperation();
                    clsGlobals.arcWorkspaceEditGeocodeFGD.StopEditing(false);
                }

            }







        }
    }
}
