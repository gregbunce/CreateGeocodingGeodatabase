using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Editor;


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

                // get the source roads feature class
                clsGlobals.arcFeatClass_Roads = clsGlobals.arcFeatureWorkspaceSGID.OpenFeatureClass(args[0]);

                //if (clsGlobals.arcFeat_Roads != null)
                //{
                //    Console.WriteLine("Got access to the SGID feature class named: " + args[0]);                    
                //}
                //else
                //{
                //    Console.WriteLine("Cannot get access to the SGID feature class named: " + args[0]);
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




                ////get access to the file geodatabases and feature classes needed for this project
                ////open file geodatabase workspace
                //IWorkspaceFactory pWorkspaceFactory = new FileGDBWorkspaceFactory();
                //IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile("C:\\Users\\gbunce\\Documents\\ArcGIS\\WeberPSAP_DataCSharpAddrPnts.gdb", 0);
                //IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspace;

                //IFeatureClass pFeatClass = pFeatureWorkspace.OpenFeatureClass("CommonNames_1");
                //Console.WriteLine("Got access to the feature class");




            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error with the conCreateGeocodeDerivativeData console application, in the Main method." + ex.Message + " " + ex.Source + " " + ex.InnerException + " " + ex.HResult + " " + ex.StackTrace + " " + ex);
                Console.ReadLine();
            }








        }
    }
}
