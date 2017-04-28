using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace conCreateGeocodeDerivativeData
{
    class clsStaticMethods
    {

        //connect to sde - method
        #region "Connect to SDE"
        public static ESRI.ArcGIS.Geodatabase.IWorkspace ConnectToTransactionalVersion(String server, String instance, String database, String authenication, String version)
        {
            IPropertySet propertySet = new PropertySetClass();
            propertySet.SetProperty("SERVER", server);
            //propertySet.SetProperty("DBCLIENT", dbclient);
            propertySet.SetProperty("INSTANCE", instance);
            propertySet.SetProperty("DATABASE", database);
            propertySet.SetProperty("AUTHENTICATION_MODE", authenication);
            propertySet.SetProperty("VERSION", version);

            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            return workspaceFactory.Open(propertySet, 0);
        }
        #endregion

        //connect to sde - method (this method has the same name so we can use method overloading)
        #region "Connect to SDE"
        public static ESRI.ArcGIS.Geodatabase.IWorkspace ConnectToTransactionalVersion(String server, String instance, String database, String authenication, String version, String username, String pass)
        {
            IPropertySet propertySet = new PropertySetClass();
            propertySet.SetProperty("SERVER", server);
            //propertySet.SetProperty("DBCLIENT", dbclient);
            propertySet.SetProperty("INSTANCE", instance);
            propertySet.SetProperty("DATABASE", database);
            propertySet.SetProperty("AUTHENTICATION_MODE", authenication);
            propertySet.SetProperty("VERSION", version);
            propertySet.SetProperty("USER", username);
            propertySet.SetProperty("PASSWORD", pass);

            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            return workspaceFactory.Open(propertySet, 0);
        }
        #endregion

        // create a file geodatabase in user-specified location
        #region "Create FileGeodatabase"
        public static IWorkspace CreateFileGdbWorkspace(string strFgdPath, string strFgdName)
        {
            IWorkspaceName workspaceName = null;
            // Instantiate a file geodatabase workspace factory and create a new file geodatabase.
            // The Create method returns a workspace name object.
            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass() as IWorkspaceFactory;

            // check if file geodatabase exists, before creating it
            if (!(workspaceFactory.IsWorkspace(strFgdPath + strFgdName)))
            {
                workspaceName = workspaceFactory.Create(strFgdPath, strFgdName, null, 0);
            }
            else
            {
                IFileNames arcFileNames = new FileNames();
                arcFileNames.Add(strFgdPath + strFgdName);
                workspaceName = workspaceFactory.GetWorkspaceName(strFgdPath, arcFileNames);
            }

            // Cast the workspace name object to the IName interface and open the workspace.
            IName name = (IName)workspaceName;
            IWorkspace workspace = (IWorkspace)name.Open();
            return workspace;
        }
        #endregion

        // create feature class in file geodatabase
        #region "create feature class in file geodatabase"
        public static ESRI.ArcGIS.Geodatabase.IFeatureClass CreateFeatureClass(String featureClassName, UID classExtensionUID, IFeatureWorkspace featureWorkspace)
        {
            // check if the fc exist, if so rename it



            // Create a fields collection for the feature class.
            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

            // Add an object ID field to the fields collection. This is mandatory for feature classes.
            IField oidField = new FieldClass();
            IFieldEdit oidFieldEdit = (IFieldEdit)oidField;
            oidFieldEdit.Name_2 = "OBJECTID";
            oidFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            fieldsEdit.AddField(oidField);

            // Create a geometry definition (and spatial reference) for the feature class.
            IGeometryDef geometryDef = new GeometryDefClass();
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
            ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
            ISpatialReference spatialReference = spatialReferenceFactory.CreateProjectedCoordinateSystem((int)esriSRProjCSType.esriSRProjCS_NAD1983UTM_12N);
            ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)spatialReference;
            spatialReferenceResolution.ConstructFromHorizon();
            ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)spatialReference;
            spatialReferenceTolerance.SetDefaultXYTolerance();
            geometryDefEdit.SpatialReference_2 = spatialReference;

            // Add a geometry field to the fields collection. This is where the geometry definition is applied.
            IField geometryField = new FieldClass();
            IFieldEdit geometryFieldEdit = (IFieldEdit)geometryField;
            geometryFieldEdit.Name_2 = "Shape";
            geometryFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            geometryFieldEdit.GeometryDef_2 = geometryDef;
            fieldsEdit.AddField(geometryField);

            // Create a text field called "ADDRSYS_L" for the fields collection.
            IField addrSysLField = new FieldClass();
            IFieldEdit addrSysLFieldEdit = (IFieldEdit)addrSysLField;
            addrSysLFieldEdit.Name_2 = "ADDRSYS_L";
            addrSysLFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            addrSysLFieldEdit.Length_2 = 30;
            fieldsEdit.AddField(addrSysLField);

            // Create a text field called "ADDRSYS_R" for the fields collection.
            IField addrSysRField = new FieldClass();
            IFieldEdit addrSysRFieldEdit = (IFieldEdit)addrSysRField;
            addrSysRFieldEdit.Name_2 = "ADDRSYS_R";
            addrSysRFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            addrSysRFieldEdit.Length_2 = 30;
            fieldsEdit.AddField(addrSysRField);


            // Create a text field called "FROMADDR_L" for the fields collection.
            IField rangeL_Ffield = new FieldClass();
            IFieldEdit rangeL_FfieldEdit = (IFieldEdit)rangeL_Ffield;
            rangeL_FfieldEdit.Name_2 = "FROMADDR_L";
            rangeL_FfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeL_FfieldEdit.Precision_2 = 38;
            rangeL_FfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeL_Ffield);

            // Create a text field called "TOADDR_L" for the fields collection.
            IField rangeL_Tfield = new FieldClass();
            IFieldEdit rangeL_TfieldEdit = (IFieldEdit)rangeL_Tfield;
            rangeL_TfieldEdit.Name_2 = "TOADDR_L";
            rangeL_TfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeL_TfieldEdit.Precision_2 = 38;
            rangeL_TfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeL_Tfield);

            // Create a text field called "FROMADDR_R" for the fields collection.
            IField rangeR_Ffield = new FieldClass();
            IFieldEdit rangeR_FfieldEdit = (IFieldEdit)rangeR_Ffield;
            rangeR_FfieldEdit.Name_2 = "FROMADDR_R";
            rangeR_FfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeR_FfieldEdit.Precision_2 = 38;
            rangeR_FfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeR_Ffield);

            // Create a text field called "TOADDR_R" for the fields collection.
            IField rangeR_Tfield = new FieldClass();
            IFieldEdit rangeR_TfieldEdit = (IFieldEdit)rangeR_Tfield;
            rangeR_TfieldEdit.Name_2 = "TOADDR_R";
            rangeR_TfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeR_TfieldEdit.Precision_2 = 38;
            rangeR_TfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeR_Tfield);

            // Create a text field called "PREDIR" for the fields collection.
            IField predirField = new FieldClass();
            IFieldEdit predirFieldEdit = (IFieldEdit)predirField;
            predirFieldEdit.Name_2 = "PREDIR";
            predirFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            predirFieldEdit.Length_2 = 1;
            fieldsEdit.AddField(predirField);

            // Create a text field called "NAME" for the fields collection.
            IField nameField = new FieldClass();
            IFieldEdit nameFieldEdit = (IFieldEdit)nameField;
            nameFieldEdit.Name_2 = "NAME";
            nameFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            nameFieldEdit.Length_2 = 30;
            fieldsEdit.AddField(nameField);

            // Create a text field called "POSTTYPE" for the fields collection.
            IField streettypeField = new FieldClass();
            IFieldEdit streettypeFieldEdit = (IFieldEdit)streettypeField;
            streettypeFieldEdit.Name_2 = "POSTTYPE";
            streettypeFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            streettypeFieldEdit.Length_2 = 4;
            fieldsEdit.AddField(streettypeField);

            // Create a text field called "POSTDIR" for the fields collection.
            IField sufdirField = new FieldClass();
            IFieldEdit sufdirFieldEdit = (IFieldEdit)sufdirField;
            sufdirFieldEdit.Name_2 = "POSTDIR";
            sufdirFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            sufdirFieldEdit.Length_2 = 2;
            fieldsEdit.AddField(sufdirField);

            // Create a text field called "ZIPCODE_L" for the fields collection.
            IField zipleftField = new FieldClass();
            IFieldEdit zipleftFieldEdit = (IFieldEdit)zipleftField;
            zipleftFieldEdit.Name_2 = "ZIPCODE_L";
            zipleftFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            zipleftFieldEdit.Length_2 = 5;
            fieldsEdit.AddField(zipleftField);

            // Create a text field called "ZIPCODE_R" for the fields collection.
            IField ziprightField = new FieldClass();
            IFieldEdit ziprightFieldEdit = (IFieldEdit)ziprightField;
            ziprightFieldEdit.Name_2 = "ZIPCODE_R";
            ziprightFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            ziprightFieldEdit.Length_2 = 5;
            fieldsEdit.AddField(ziprightField);

            // Create a text field called "GLOBALID_SGID" for the fields collection - to join to the table or sgid.
            IField globalidField = new FieldClass();
            IFieldEdit globalidFieldEdit = (IFieldEdit)globalidField;
            globalidFieldEdit.Name_2 = "GLOBALID_SGID";
            globalidFieldEdit.Type_2 = esriFieldType.esriFieldTypeString; // use string and not the globalid type b/c it might that might assign it's own unique global id and this is for joinging back to sgid
            globalidFieldEdit.Length_2 = 50;
            fieldsEdit.AddField(globalidField);

            // Use IFieldChecker to create a validated fields collection.
            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = (IWorkspace)featureWorkspace;
            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            // The enumFieldError enumerator can be inspected at this point to determine 
            // which fields were modified during validation.

            // Create the feature class. Note that the CLSID parameter is null - this indicates to use the
            // default CLSID, esriGeodatabase.Feature (acceptable in most cases for feature classes).
            IFeatureClass featureClass = featureWorkspace.CreateFeatureClass(featureClassName, validatedFields, null, classExtensionUID, esriFeatureType.esriFTSimple, "Shape", "");

            return featureClass;
        }
        #endregion

        // create table in file geodatabase
        #region "create table in file geodatabase"
        public static ESRI.ArcGIS.Geodatabase.ITable CreateTable(String tableName, UID classExtensionUID, IFeatureWorkspace featureWorkspace)
        {
            // Create a fields collection for the feature class.
            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

            // Add an object ID field to the fields collection. This is mandatory for feature classes.
            IField oidField = new FieldClass();
            IFieldEdit oidFieldEdit = (IFieldEdit)oidField;
            oidFieldEdit.Name_2 = "OBJECTID";
            oidFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            fieldsEdit.AddField(oidField);

            // Create a text field called "ADDRSYS_L" for the fields collection.
            IField addrSysLField = new FieldClass();
            IFieldEdit addrSysLFieldEdit = (IFieldEdit)addrSysLField;
            addrSysLFieldEdit.Name_2 = "ADDRSYS_L";
            addrSysLFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            addrSysLFieldEdit.Length_2 = 30;
            fieldsEdit.AddField(addrSysLField);

            // Create a text field called "ADDRSYS_R" for the fields collection.
            IField addrSysRField = new FieldClass();
            IFieldEdit addrSysRFieldEdit = (IFieldEdit)addrSysRField;
            addrSysRFieldEdit.Name_2 = "ADDRSYS_R";
            addrSysRFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            addrSysRFieldEdit.Length_2 = 30;
            fieldsEdit.AddField(addrSysRField);

            // Create a text field called "FROMADDR_L" for the fields collection.
            IField rangeL_Ffield = new FieldClass();
            IFieldEdit rangeL_FfieldEdit = (IFieldEdit)rangeL_Ffield;
            rangeL_FfieldEdit.Name_2 = "FROMADDR_L";
            rangeL_FfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeL_FfieldEdit.Precision_2 = 38;
            rangeL_FfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeL_Ffield);

            // Create a text field called "TOADDR_L" for the fields collection.
            IField rangeL_Tfield = new FieldClass();
            IFieldEdit rangeL_TfieldEdit = (IFieldEdit)rangeL_Tfield;
            rangeL_TfieldEdit.Name_2 = "TOADDR_L";
            rangeL_TfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeL_TfieldEdit.Precision_2 = 38;
            rangeL_TfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeL_Tfield);

            // Create a text field called "FROMADDR_R" for the fields collection.
            IField rangeR_Ffield = new FieldClass();
            IFieldEdit rangeR_FfieldEdit = (IFieldEdit)rangeR_Ffield;
            rangeR_FfieldEdit.Name_2 = "FROMADDR_R";
            rangeR_FfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeR_FfieldEdit.Precision_2 = 38;
            rangeR_FfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeR_Ffield);

            // Create a text field called "TOADDR_R" for the fields collection.
            IField rangeR_Tfield = new FieldClass();
            IFieldEdit rangeR_TfieldEdit = (IFieldEdit)rangeR_Tfield;
            rangeR_TfieldEdit.Name_2 = "TOADDR_R";
            rangeR_TfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeR_TfieldEdit.Precision_2 = 38;
            rangeR_TfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeR_Tfield);

            // Create a text field called "PREDIR" for the fields collection.
            IField predirField = new FieldClass();
            IFieldEdit predirFieldEdit = (IFieldEdit)predirField;
            predirFieldEdit.Name_2 = "PREDIR";
            predirFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            predirFieldEdit.Length_2 = 1;
            fieldsEdit.AddField(predirField);

            // Create a text field called "NAME" for the fields collection.
            IField nameField = new FieldClass();
            IFieldEdit nameFieldEdit = (IFieldEdit)nameField;
            nameFieldEdit.Name_2 = "NAME";
            nameFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            nameFieldEdit.Length_2 = 30;
            fieldsEdit.AddField(nameField);

            // Create a text field called "POSTTYPE" for the fields collection.
            IField streettypeField = new FieldClass();
            IFieldEdit streettypeFieldEdit = (IFieldEdit)streettypeField;
            streettypeFieldEdit.Name_2 = "POSTTYPE";
            streettypeFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            streettypeFieldEdit.Length_2 = 4;
            fieldsEdit.AddField(streettypeField);

            // Create a text field called "POSTDIR" for the fields collection.
            IField sufdirField = new FieldClass();
            IFieldEdit sufdirFieldEdit = (IFieldEdit)sufdirField;
            sufdirFieldEdit.Name_2 = "POSTDIR";
            sufdirFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            sufdirFieldEdit.Length_2 = 2;
            fieldsEdit.AddField(sufdirField);

            // Create a text field called "ZIPCODE_L" for the fields collection.
            IField zipleftField = new FieldClass();
            IFieldEdit zipleftFieldEdit = (IFieldEdit)zipleftField;
            zipleftFieldEdit.Name_2 = "ZIPCODE_L";
            zipleftFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            zipleftFieldEdit.Length_2 = 5;
            fieldsEdit.AddField(zipleftField);

            // Create a text field called "ZIPCODE_R" for the fields collection.
            IField ziprightField = new FieldClass();
            IFieldEdit ziprightFieldEdit = (IFieldEdit)ziprightField;
            ziprightFieldEdit.Name_2 = "ZIPCODE_R";
            ziprightFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            ziprightFieldEdit.Length_2 = 5;
            fieldsEdit.AddField(ziprightField);

            // Create a text field called "GLOBALID_SGID" for the fields collection - to join back to the feature class or sgid.
            IField globalidField = new FieldClass();
            IFieldEdit globalidFieldEdit = (IFieldEdit)globalidField;
            globalidFieldEdit.Name_2 = "GLOBALID_SGID";
            globalidFieldEdit.Type_2 = esriFieldType.esriFieldTypeString; // use string and not the globalid type b/c it might that might assign it's own unique global id and this is for joinging back to sgid
            globalidFieldEdit.Length_2 = 50;
            fieldsEdit.AddField(globalidField);

            // Use IFieldChecker to create a validated fields collection.
            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = (IWorkspace)featureWorkspace;
            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            // Create the feature class. Note that the CLSID parameter is null - this indicates to use the
            // default CLSID, esriGeodatabase.Feature (acceptable in most cases for feature classes).
            ESRI.ArcGIS.Geodatabase.ITable arcTable = featureWorkspace.CreateTable(tableName, validatedFields, null, classExtensionUID, "");

            return arcTable;
        }
        #endregion

        // check if data exists in file geodatabase
        #region "check if name exists in database"
        // this method checks if the feature class or table exist in the geodatabase
        public static bool NameExists(string strFCName, esriDatasetType dataType)
        {
            bool blnNameExists = clsGlobals.arcWorkspace2GeocodeFGD.get_NameExists(dataType, strFCName);
            return blnNameExists;
        }
        #endregion

        // insert new row/record in the geocode file geodatabase
        public static void InsertFeatureGeocodeFGD(IFeature arcFeatSGIDRoad, string strAddrSys, double dblLF, double dblLT, double dblRF, double dblRT, string strPredir, string strStreetname, string strStreetType, string strPostDir, string strZipRight, string strZipLeft, string strGlobalID)
        {
            try
            {
                // create a new feature
                clsGlobals.arcFeat_GeocodeRoad = clsGlobals.arcFeatClass_GeocodeRoads.CreateFeature();
                IGeometry arcGeomSGIDRoad = arcFeatSGIDRoad.ShapeCopy;                
                
                // simplify feature (not sure if this simplify is needed)
                IFeatureSimplify arcFeaureSimplify = clsGlobals.arcFeat_GeocodeRoad as IFeatureSimplify;
                arcFeaureSimplify.SimplifyGeometry(arcGeomSGIDRoad);

                // set the geometry for the new feature
                clsGlobals.arcFeat_GeocodeRoad.Shape = arcGeomSGIDRoad;

                // set the field values for the new feature
                clsGlobals.arcFeat_GeocodeRoad.set_Value(clsGlobals.arcFeatClass_GeocodeRoads.Fields.FindField("ADDRSYS_L"), strAddrSys);
                clsGlobals.arcFeat_GeocodeRoad.set_Value(clsGlobals.arcFeatClass_GeocodeRoads.Fields.FindField("ADDRSYS_R"), strAddrSys);
                clsGlobals.arcFeat_GeocodeRoad.set_Value(clsGlobals.arcFeatClass_GeocodeRoads.Fields.FindField("FROMADDR_L"), dblLF);
                clsGlobals.arcFeat_GeocodeRoad.set_Value(clsGlobals.arcFeatClass_GeocodeRoads.Fields.FindField("TOADDR_L"), dblLT);
                clsGlobals.arcFeat_GeocodeRoad.set_Value(clsGlobals.arcFeatClass_GeocodeRoads.Fields.FindField("FROMADDR_R"), dblRF);
                clsGlobals.arcFeat_GeocodeRoad.set_Value(clsGlobals.arcFeatClass_GeocodeRoads.Fields.FindField("TOADDR_R"), dblRT);
                clsGlobals.arcFeat_GeocodeRoad.set_Value(clsGlobals.arcFeatClass_GeocodeRoads.Fields.FindField("PREDIR"), strPredir);
                clsGlobals.arcFeat_GeocodeRoad.set_Value(clsGlobals.arcFeatClass_GeocodeRoads.Fields.FindField("NAME"), strStreetname);
                clsGlobals.arcFeat_GeocodeRoad.set_Value(clsGlobals.arcFeatClass_GeocodeRoads.Fields.FindField("POSTTYPE"), strStreetType);
                clsGlobals.arcFeat_GeocodeRoad.set_Value(clsGlobals.arcFeatClass_GeocodeRoads.Fields.FindField("POSTDIR"), strPostDir);
                clsGlobals.arcFeat_GeocodeRoad.set_Value(clsGlobals.arcFeatClass_GeocodeRoads.Fields.FindField("ZIPCODE_L"), strZipLeft);
                clsGlobals.arcFeat_GeocodeRoad.set_Value(clsGlobals.arcFeatClass_GeocodeRoads.Fields.FindField("ZIPCODE_R"), strZipRight);
                clsGlobals.arcFeat_GeocodeRoad.set_Value(clsGlobals.arcFeatClass_GeocodeRoads.Fields.FindField("GLOBALID_SGID"), strGlobalID);
                
                // store the new feature
                clsGlobals.arcFeat_GeocodeRoad.Store();

            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error with the conCreateGeocodeDerivativeData console application, in the clsStaticMethods.InsertFeatureGeocodeFGD method." + ex.Message + " " + ex.Source + " " + ex.InnerException + " " + ex.HResult + " " + ex.StackTrace + " " + ex);
                Console.ReadLine();
            }
        
        }


    }
}
