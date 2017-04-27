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


        #region "Create FileGeodatabase"
        // create a file geodatabase in user-specified location
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
                
                workspaceName = (IWorkspaceName)workspaceFactory.OpenFromFile(strFgdPath + strFgdName, 0);
            }

            // Cast the workspace name object to the IName interface and open the workspace.
            IName name = (IName)workspaceName;
            IWorkspace workspace = (IWorkspace)name.Open();
            return workspace;
        }
        #endregion


        #region "MAYBE NOT THIS ONE...create feature class in file geodatabase"
        ///<summary>Simple helper to create a featureclass in a geodatabase.</summary>
        /// 
        ///<param name="workspace">An IWorkspace2 interface</param>
        ///<param name="featureDataset">An IFeatureDataset interface or Nothing</param>
        ///<param name="featureClassName">A System.String that contains the name of the feature class to open or create. Example: "states"</param>
        ///<param name="fields">An IFields interface</param>
        ///<param name="CLSID">A UID value or Nothing. Example "esriGeoDatabase.Feature" or Nothing</param>
        ///<param name="CLSEXT">A UID value or Nothing (this is the class extension if you want to reference a class extension when creating the feature class).</param>
        ///<param name="strConfigKeyword">An empty System.String or RDBMS table string for ArcSDE. Example: "myTable" or ""</param>
        ///  
        ///<returns>An IFeatureClass interface or a Nothing</returns>
        ///  
        ///<remarks>
        ///  (1) If a 'featureClassName' already exists in the workspace a reference to that feature class 
        ///      object will be returned.
        ///  (2) If an IFeatureDataset is passed in for the 'featureDataset' argument the feature class
        ///      will be created in the dataset. If a Nothing is passed in for the 'featureDataset'
        ///      argument the feature class will be created in the workspace.
        ///  (3) When creating a feature class in a dataset the spatial reference is inherited 
        ///      from the dataset object.
        ///  (4) If an IFields interface is supplied for the 'fields' collection it will be used to create the
        ///      table. If a Nothing value is supplied for the 'fields' collection, a table will be created using 
        ///      default values in the method.
        ///  (5) The 'strConfigurationKeyword' parameter allows the application to control the physical layout 
        ///      for this table in the underlying RDBMSfor example, in the case of an Oracle database, the 
        ///      configuration keyword controls the tablespace in which the table is created, the initial and 
        ///     next extents, and other properties. The 'strConfigurationKeywords' for an ArcSDE instance are 
        ///      set up by the ArcSDE data administrator, the list of available keywords supported by a workspace 
        ///      may be obtained using the IWorkspaceConfiguration interface. For more information on configuration 
        ///      keywords, refer to the ArcSDE documentation. When not using an ArcSDE table use an empty 
        ///      string (ex: "").
        ///</remarks>
        public static ESRI.ArcGIS.Geodatabase.IFeatureClass CreateFeatureClassMAYBE(ESRI.ArcGIS.Geodatabase.IWorkspace2 workspace, ESRI.ArcGIS.Geodatabase.IFeatureDataset featureDataset, System.String featureClassName, ESRI.ArcGIS.Geodatabase.IFields fields, ESRI.ArcGIS.esriSystem.UID CLSID, ESRI.ArcGIS.esriSystem.UID CLSEXT, System.String strConfigKeyword)
        {
            if (featureClassName == "") return null; // name was not passed in 

            ESRI.ArcGIS.Geodatabase.IFeatureClass featureClass;
            ESRI.ArcGIS.Geodatabase.IFeatureWorkspace featureWorkspace = (ESRI.ArcGIS.Geodatabase.IFeatureWorkspace)workspace; // Explicit Cast

            if (workspace.get_NameExists(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTFeatureClass, featureClassName)) //feature class with that name already exists 
            {
                featureClass = featureWorkspace.OpenFeatureClass(featureClassName);
                return featureClass;
            }

            // assign the class id value if not assigned
            if (CLSID == null)
            {
                CLSID = new ESRI.ArcGIS.esriSystem.UIDClass();
                CLSID.Value = "esriGeoDatabase.Feature";
            }

            ESRI.ArcGIS.Geodatabase.IObjectClassDescription objectClassDescription = new ESRI.ArcGIS.Geodatabase.FeatureClassDescriptionClass();

            // if a fields collection is not passed in then supply our own
            if (fields == null)
            {
                // create the fields using the required fields method
                fields = objectClassDescription.RequiredFields;
                ESRI.ArcGIS.Geodatabase.IFieldsEdit fieldsEdit = (ESRI.ArcGIS.Geodatabase.IFieldsEdit)fields; // Explicit Cast
                ESRI.ArcGIS.Geodatabase.IField field = new ESRI.ArcGIS.Geodatabase.FieldClass();

                // create a user defined text field
                ESRI.ArcGIS.Geodatabase.IFieldEdit fieldEdit = (ESRI.ArcGIS.Geodatabase.IFieldEdit)field; // Explicit Cast

                // setup field properties
                fieldEdit.Name_2 = "SampleField";
                fieldEdit.Type_2 = ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeString;
                fieldEdit.IsNullable_2 = true;
                fieldEdit.AliasName_2 = "Sample Field Column";
                fieldEdit.DefaultValue_2 = "test";
                fieldEdit.Editable_2 = true;
                fieldEdit.Length_2 = 100;

                // add field to field collection
                fieldsEdit.AddField(field);
                fields = (ESRI.ArcGIS.Geodatabase.IFields)fieldsEdit; // Explicit Cast
            }

            System.String strShapeField = "";

            // locate the shape field
            for (int j = 0; j < fields.FieldCount; j++)
            {
                if (fields.get_Field(j).Type == ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeGeometry)
                {
                    strShapeField = fields.get_Field(j).Name;
                }
            }

            // Use IFieldChecker to create a validated fields collection.
            ESRI.ArcGIS.Geodatabase.IFieldChecker fieldChecker = new ESRI.ArcGIS.Geodatabase.FieldCheckerClass();
            ESRI.ArcGIS.Geodatabase.IEnumFieldError enumFieldError = null;
            ESRI.ArcGIS.Geodatabase.IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = (ESRI.ArcGIS.Geodatabase.IWorkspace)workspace;
            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            // The enumFieldError enumerator can be inspected at this point to determine 
            // which fields were modified during validation.


            // finally create and return the feature class
            if (featureDataset == null)// if no feature dataset passed in, create at the workspace level
            {
                featureClass = featureWorkspace.CreateFeatureClass(featureClassName, validatedFields, CLSID, CLSEXT, ESRI.ArcGIS.Geodatabase.esriFeatureType.esriFTSimple, strShapeField, strConfigKeyword);
            }
            else
            {
                featureClass = featureDataset.CreateFeatureClass(featureClassName, validatedFields, CLSID, CLSEXT, ESRI.ArcGIS.Geodatabase.esriFeatureType.esriFTSimple, strShapeField, strConfigKeyword);
            }
            return featureClass;
        }
        #endregion



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

            // Create a text field called "ADDR_SYS" for the fields collection.
            IField addrSysField = new FieldClass();
            IFieldEdit addrSysFieldEdit = (IFieldEdit)addrSysField;
            addrSysFieldEdit.Name_2 = "ADDR_SYS";
            addrSysFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            addrSysFieldEdit.Length_2 = 50;
            fieldsEdit.AddField(addrSysField);

            // Create a text field called "L_F_ADD" for the fields collection.
            IField rangeL_Ffield = new FieldClass();
            IFieldEdit rangeL_FfieldEdit = (IFieldEdit)rangeL_Ffield;
            rangeL_FfieldEdit.Name_2 = "L_F_ADD";
            rangeL_FfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeL_FfieldEdit.Precision_2 = 38;
            rangeL_FfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeL_Ffield);

            // Create a text field called "L_F_ADD" for the fields collection.
            IField rangeL_Tfield = new FieldClass();
            IFieldEdit rangeL_TfieldEdit = (IFieldEdit)rangeL_Tfield;
            rangeL_TfieldEdit.Name_2 = "L_F_ADD";
            rangeL_TfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeL_TfieldEdit.Precision_2 = 38;
            rangeL_TfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeL_Tfield);

            // Create a text field called "R_F_ADD" for the fields collection.
            IField rangeR_Ffield = new FieldClass();
            IFieldEdit rangeR_FfieldEdit = (IFieldEdit)rangeR_Ffield;
            rangeR_FfieldEdit.Name_2 = "R_F_ADD";
            rangeR_FfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeR_FfieldEdit.Precision_2 = 38;
            rangeR_FfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeR_Ffield);

            // Create a text field called "R_T_ADD" for the fields collection.
            IField rangeR_Tfield = new FieldClass();
            IFieldEdit rangeR_TfieldEdit = (IFieldEdit)rangeR_Tfield;
            rangeR_TfieldEdit.Name_2 = "R_T_ADD";
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

            // Create a text field called "STREETNAME" for the fields collection.
            IField nameField = new FieldClass();
            IFieldEdit nameFieldEdit = (IFieldEdit)nameField;
            nameFieldEdit.Name_2 = "STREETNAME";
            nameFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            nameFieldEdit.Length_2 = 30;
            fieldsEdit.AddField(nameField);

            // Create a text field called "STREETTYPE" for the fields collection.
            IField streettypeField = new FieldClass();
            IFieldEdit streettypeFieldEdit = (IFieldEdit)streettypeField;
            streettypeFieldEdit.Name_2 = "STREETTYPE";
            streettypeFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            streettypeFieldEdit.Length_2 = 4;
            fieldsEdit.AddField(streettypeField);

            // Create a text field called "SUFDIR" for the fields collection.
            IField sufdirField = new FieldClass();
            IFieldEdit sufdirFieldEdit = (IFieldEdit)sufdirField;
            sufdirFieldEdit.Name_2 = "SUFDIR";
            sufdirFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            sufdirFieldEdit.Length_2 = 1;
            fieldsEdit.AddField(sufdirField);

            // Create a text field called "ZIPLEFT" for the fields collection.
            IField zipleftField = new FieldClass();
            IFieldEdit zipleftFieldEdit = (IFieldEdit)zipleftField;
            zipleftFieldEdit.Name_2 = "ZIPLEFT";
            zipleftFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            zipleftFieldEdit.Length_2 = 5;
            fieldsEdit.AddField(zipleftField);

            // Create a text field called "ZIPRIGHT" for the fields collection.
            IField ziprightField = new FieldClass();
            IFieldEdit ziprightFieldEdit = (IFieldEdit)ziprightField;
            ziprightFieldEdit.Name_2 = "ZIPRIGHT";
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

            // Create a text field called "ADDR_SYS" for the fields collection.
            IField addrSysField = new FieldClass();
            IFieldEdit addrSysFieldEdit = (IFieldEdit)addrSysField;
            addrSysFieldEdit.Name_2 = "ADDR_SYS";
            addrSysFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            addrSysFieldEdit.Length_2 = 50;
            fieldsEdit.AddField(addrSysField);

            // Create a text field called "L_F_ADD" for the fields collection.
            IField rangeL_Ffield = new FieldClass();
            IFieldEdit rangeL_FfieldEdit = (IFieldEdit)rangeL_Ffield;
            rangeL_FfieldEdit.Name_2 = "L_F_ADD";
            rangeL_FfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeL_FfieldEdit.Precision_2 = 38;
            rangeL_FfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeL_Ffield);

            // Create a text field called "L_F_ADD" for the fields collection.
            IField rangeL_Tfield = new FieldClass();
            IFieldEdit rangeL_TfieldEdit = (IFieldEdit)rangeL_Tfield;
            rangeL_TfieldEdit.Name_2 = "L_F_ADD";
            rangeL_TfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeL_TfieldEdit.Precision_2 = 38;
            rangeL_TfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeL_Tfield);

            // Create a text field called "R_F_ADD" for the fields collection.
            IField rangeR_Ffield = new FieldClass();
            IFieldEdit rangeR_FfieldEdit = (IFieldEdit)rangeR_Ffield;
            rangeR_FfieldEdit.Name_2 = "R_F_ADD";
            rangeR_FfieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
            rangeR_FfieldEdit.Precision_2 = 38;
            rangeR_FfieldEdit.Scale_2 = 8;
            fieldsEdit.AddField(rangeR_Ffield);

            // Create a text field called "R_T_ADD" for the fields collection.
            IField rangeR_Tfield = new FieldClass();
            IFieldEdit rangeR_TfieldEdit = (IFieldEdit)rangeR_Tfield;
            rangeR_TfieldEdit.Name_2 = "R_T_ADD";
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

            // Create a text field called "STREETNAME" for the fields collection.
            IField nameField = new FieldClass();
            IFieldEdit nameFieldEdit = (IFieldEdit)nameField;
            nameFieldEdit.Name_2 = "STREETNAME";
            nameFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            nameFieldEdit.Length_2 = 30;
            fieldsEdit.AddField(nameField);

            // Create a text field called "STREETTYPE" for the fields collection.
            IField streettypeField = new FieldClass();
            IFieldEdit streettypeFieldEdit = (IFieldEdit)streettypeField;
            streettypeFieldEdit.Name_2 = "STREETTYPE";
            streettypeFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            streettypeFieldEdit.Length_2 = 4;
            fieldsEdit.AddField(streettypeField);

            // Create a text field called "SUFDIR" for the fields collection.
            IField sufdirField = new FieldClass();
            IFieldEdit sufdirFieldEdit = (IFieldEdit)sufdirField;
            sufdirFieldEdit.Name_2 = "SUFDIR";
            sufdirFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            sufdirFieldEdit.Length_2 = 1;
            fieldsEdit.AddField(sufdirField);

            // Create a text field called "ZIPLEFT" for the fields collection.
            IField zipleftField = new FieldClass();
            IFieldEdit zipleftFieldEdit = (IFieldEdit)zipleftField;
            zipleftFieldEdit.Name_2 = "ZIPLEFT";
            zipleftFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            zipleftFieldEdit.Length_2 = 5;
            fieldsEdit.AddField(zipleftField);

            // Create a text field called "ZIPRIGHT" for the fields collection.
            IField ziprightField = new FieldClass();
            IFieldEdit ziprightFieldEdit = (IFieldEdit)ziprightField;
            ziprightFieldEdit.Name_2 = "ZIPRIGHT";
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



        // this method checks if the feature class or table exist in the geodatabase
        public static bool NameExists(IWorkspace2 arcWorkspace2, string strFCName)
        {
            bool blnNameExists;
            return blnNameExists = arcWorkspace2.get_NameExists(esriDatasetType.esriDTAny, strFCName);
        }




    }
}
