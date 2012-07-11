// Type: Microsoft.VisualStudio.Shell.Package
// Assembly: Microsoft.VisualStudio.Shell.10.0, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Assembly location: C:\Program Files (x86)\Microsoft Visual Studio 2010 SDK SP1\VisualStudioIntegration\Common\Assemblies\v4.0\Microsoft.VisualStudio.Shell.10.0.dll

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Microsoft.VisualStudio.Shell
{
  [CLSCompliant(false)]
  [ComVisible(true)]
  [PackageRegistration]
  public abstract class Package : IVsPackage, Microsoft.VisualStudio.OLE.Interop.IServiceProvider, IOleCommandTarget, IVsPersistSolutionOpts, IServiceContainer, System.IServiceProvider, IVsUserSettings, IVsUserSettingsMigration, IVsToolWindowFactory, IVsToolboxItemProvider
  {
    private ServiceCollection<object> _services = new ServiceCollection<object>();
    private Dictionary<string, System.Windows.Forms.IDataObject> _tbxItemDataCache = new Dictionary<string, System.Windows.Forms.IDataObject>();
    private ServiceProvider _provider;
    private Hashtable _editorFactories;
    private Hashtable _projectFactories;
    private ToolWindowCollection _toolWindows;
    private Container _componentToolWindows;
    private Container _pagesAndProfiles;
    private ArrayList _optionKeys;
    private EventHandler ToolboxInitialized;
    private EventHandler ToolboxUpgraded;
    private bool zombie;

    public RegistryKey ApplicationRegistryRoot
    {
      get
      {
        return VSRegistry.RegistryRoot((System.IServiceProvider) this._provider, __VsLocalRegistryType.RegType_Configuration, false);
      }
    }

    public string UserDataPath
    {
      get
      {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), this.GetRegistryRoot().Substring("SOFTWARE\\".Length));
      }
    }

    public string UserLocalDataPath
    {
      get
      {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), this.GetRegistryRoot().Substring("SOFTWARE\\".Length));
      }
    }

    public RegistryKey UserRegistryRoot
    {
      get
      {
        return VSRegistry.RegistryRoot((System.IServiceProvider) this._provider, __VsLocalRegistryType.RegType_UserSettings, true);
      }
    }

    public bool Zombied
    {
      get
      {
        return this.zombie;
      }
    }

    protected event EventHandler ToolboxInitialized
    {
      add
      {
        EventHandler eventHandler = this.ToolboxInitialized;
        EventHandler comparand;
        do
        {
          comparand = eventHandler;
          eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.ToolboxInitialized, comparand + value, comparand);
        }
        while (eventHandler != comparand);
      }
      remove
      {
        EventHandler eventHandler = this.ToolboxInitialized;
        EventHandler comparand;
        do
        {
          comparand = eventHandler;
          eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.ToolboxInitialized, comparand - value, comparand);
        }
        while (eventHandler != comparand);
      }
    }

    protected event EventHandler ToolboxUpgraded
    {
      add
      {
        EventHandler eventHandler = this.ToolboxUpgraded;
        EventHandler comparand;
        do
        {
          comparand = eventHandler;
          eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.ToolboxUpgraded, comparand + value, comparand);
        }
        while (eventHandler != comparand);
      }
      remove
      {
        EventHandler eventHandler = this.ToolboxUpgraded;
        EventHandler comparand;
        do
        {
          comparand = eventHandler;
          eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.ToolboxUpgraded, comparand - value, comparand);
        }
        while (eventHandler != comparand);
      }
    }

    protected Package()
    {
      ServiceCreatorCallback callback = new ServiceCreatorCallback(this.OnCreateService);
      this.AddService(typeof (IMenuCommandService), callback);
      this.AddService(typeof (IOleCommandTarget), callback);
    }

    protected void AddOptionKey(string name)
    {
      if (this.zombie)
        Marshal.ThrowExceptionForHR(-2147418113);
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.IndexOf('.') != -1 || name.Length > 31)
      {
        throw new ArgumentException(string.Format((IFormatProvider) Resources.Culture, Resources.Package_BadOptionName, new object[1]
        {
          (object) name
        }));
      }
      else
      {
        if (this._optionKeys == null)
          this._optionKeys = new ArrayList();
        else if (this._optionKeys.Contains((object) name))
          throw new ArgumentException(string.Format((IFormatProvider) Resources.Culture, Resources.Package_OptionNameUsed, new object[1]
          {
            (object) name
          }));
        this._optionKeys.Add((object) name);
      }
    }

    int IVsUserSettings.ExportSettings(string strPageGuid, IVsSettingsWriter writer)
    {
      IProfileManager profileManager = this.GetProfileManager(new Guid(strPageGuid), Package.ProfileManagerLoadAction.LoadPropsFromRegistry);
      if (profileManager != null)
        profileManager.SaveSettingsToXml(writer);
      return 0;
    }

    int IVsUserSettingsMigration.MigrateSettings(IVsSettingsReader reader, IVsSettingsWriter writer, string strPageGuid)
    {
      Guid objectGuid = Guid.Empty;
      try
      {
        objectGuid = new Guid(strPageGuid);
      }
      catch (FormatException ex)
      {
      }
      IProfileMigrator profileMigrator = !(objectGuid == Guid.Empty) ? this.GetProfileManager(objectGuid, Package.ProfileManagerLoadAction.None) as IProfileMigrator : this.GetAutomationObject(strPageGuid) as IProfileMigrator;
      if (profileMigrator != null)
        profileMigrator.MigrateSettings(reader, writer);
      return 0;
    }

    int IVsUserSettings.ImportSettings(string strPageGuid, IVsSettingsReader reader, uint flags, ref int restartRequired)
    {
      if (restartRequired > 0)
        restartRequired = 0;
      bool flag = ((int) flags & 1) == 0;
      IProfileManager profileManager = this.GetProfileManager(new Guid(strPageGuid), flag ? Package.ProfileManagerLoadAction.LoadPropsFromRegistry : Package.ProfileManagerLoadAction.ResetSettings);
      if (profileManager != null)
      {
        profileManager.LoadSettingsFromXml(reader);
        profileManager.SaveSettingsToStorage();
      }
      return 0;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      if (this._editorFactories != null)
      {
        Hashtable hashtable = this._editorFactories;
        this._editorFactories = (Hashtable) null;
        try
        {
          IVsRegisterEditors vsRegisterEditors = this.GetService(typeof (SVsRegisterEditors)) as IVsRegisterEditors;
          foreach (DictionaryEntry dictionaryEntry in hashtable)
          {
            try
            {
              if (vsRegisterEditors != null)
                vsRegisterEditors.UnregisterEditor((uint) dictionaryEntry.Value);
            }
            catch (Exception ex)
            {
            }
            finally
            {
              IDisposable disposable = dictionaryEntry.Key as IDisposable;
              if (disposable != null)
                disposable.Dispose();
            }
          }
        }
        catch (Exception ex)
        {
        }
      }
      if (this._projectFactories != null)
      {
        Hashtable hashtable = this._projectFactories;
        this._projectFactories = (Hashtable) null;
        try
        {
          IVsRegisterProjectTypes registerProjectTypes = this.GetService(typeof (SVsRegisterProjectTypes)) as IVsRegisterProjectTypes;
          foreach (DictionaryEntry dictionaryEntry in hashtable)
          {
            try
            {
              if (registerProjectTypes != null)
                registerProjectTypes.UnregisterProjectType((uint) dictionaryEntry.Value);
            }
            finally
            {
              IDisposable disposable = dictionaryEntry.Key as IDisposable;
              if (disposable != null)
                disposable.Dispose();
            }
          }
        }
        catch (Exception ex)
        {
        }
      }
      if (this._componentToolWindows != null)
      {
        Container container = this._componentToolWindows;
        this._componentToolWindows = (Container) null;
        try
        {
          container.Dispose();
        }
        catch (Exception ex)
        {
        }
      }
      if (this._pagesAndProfiles != null)
      {
        Container container = this._pagesAndProfiles;
        this._pagesAndProfiles = (Container) null;
        try
        {
          container.Dispose();
        }
        catch (Exception ex)
        {
        }
      }
      if (this._services != null)
      {
        if (this._services.Count > 0)
        {
          try
          {
            IProfferService profferService = (IProfferService) this.GetService(typeof (SProfferService));
            ServiceCollection<object> serviceCollection = this._services;
            this._services = (ServiceCollection<object>) null;
            foreach (object obj1 in serviceCollection.Values)
            {
              object obj2 = obj1;
              Package.ProfferedService profferedService = obj2 as Package.ProfferedService;
              try
              {
                if (profferedService != null)
                {
                  obj2 = profferedService.Instance;
                  if ((int) profferedService.Cookie != 0)
                  {
                    if (profferService != null)
                    {
                      if (Microsoft.VisualStudio.NativeMethods.Failed(profferService.RevokeService(profferedService.Cookie)))
                        Trace.WriteLine(string.Format((IFormatProvider) CultureInfo.CurrentUICulture, "Failed to unregister service {0}", new object[1]
                        {
                          (object) obj2.GetType().FullName
                        }));
                    }
                  }
                }
              }
              finally
              {
                if (obj2 is IDisposable)
                  ((IDisposable) obj2).Dispose();
              }
            }
          }
          catch (Exception ex)
          {
          }
        }
      }
      if (this._provider != null)
      {
        try
        {
          this._provider.Dispose();
        }
        catch (Exception ex)
        {
        }
        this._provider = (ServiceProvider) null;
      }
      if (this._toolWindows != null)
      {
        this._toolWindows.Dispose();
        this._toolWindows = (ToolWindowCollection) null;
      }
      if (this._optionKeys != null)
        this._optionKeys = (ArrayList) null;
      SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
    }

    protected virtual object GetAutomationObject(string name)
    {
      if (this.zombie)
        Marshal.ThrowExceptionForHR(-2147418113);
      if (name == null)
        return (object) null;
      string[] strArray = name.Split(new char[1]
      {
        '.'
      });
      if (strArray.Length != 2)
        return (object) null;
      strArray[0] = strArray[0].Trim();
      strArray[1] = strArray[1].Trim();
      foreach (Attribute attribute in TypeDescriptor.GetAttributes((object) this))
      {
        ProvideOptionPageAttribute optionPageAttribute = attribute as ProvideOptionPageAttribute;
        if (optionPageAttribute != null && optionPageAttribute.SupportsAutomation && (string.Compare(optionPageAttribute.CategoryName, strArray[0], StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(optionPageAttribute.PageName, strArray[1], StringComparison.OrdinalIgnoreCase) == 0))
          return this.GetDialogPage(optionPageAttribute.PageType).AutomationObject;
      }
      return (object) null;
    }

    protected DialogPage GetDialogPage(Type dialogPageType)
    {
      if (this.zombie)
        Marshal.ThrowExceptionForHR(-2147418113);
      if (dialogPageType == (Type) null)
        throw new ArgumentNullException("dialogPageType");
      if (!typeof (DialogPage).IsAssignableFrom(dialogPageType))
      {
        throw new ArgumentException(string.Format((IFormatProvider) Resources.Culture, Resources.Package_BadDialogPageType, new object[1]
        {
          (object) dialogPageType.FullName
        }));
      }
      else
      {
        if (this._pagesAndProfiles != null)
        {
          foreach (object obj in (ReadOnlyCollectionBase) this._pagesAndProfiles.Components)
          {
            if (obj.GetType() == dialogPageType)
              return (DialogPage) obj;
          }
        }
        ConstructorInfo constructor = dialogPageType.GetConstructor(new Type[0]);
        if (constructor == (ConstructorInfo) null)
        {
          throw new ArgumentException(string.Format((IFormatProvider) Resources.Culture, Resources.Package_PageCtorMissing, new object[1]
          {
            (object) dialogPageType.FullName
          }));
        }
        else
        {
          DialogPage dialogPage = (DialogPage) constructor.Invoke(new object[0]);
          if (this._pagesAndProfiles == null)
            this._pagesAndProfiles = (Container) new Package.PackageContainer((System.IServiceProvider) this);
          this._pagesAndProfiles.Add((IComponent) dialogPage);
          return dialogPage;
        }
      }
    }

    private IProfileManager GetProfileManager(Guid objectGuid, Package.ProfileManagerLoadAction loadAction)
    {
      IProfileManager profileManager = (IProfileManager) null;
      if (objectGuid == Guid.Empty)
        throw new ArgumentNullException("objectGuid");
      if (this._pagesAndProfiles != null)
      {
        foreach (object obj in (ReadOnlyCollectionBase) this._pagesAndProfiles.Components)
        {
          if (obj.GetType().GUID.Equals(objectGuid))
          {
            if (obj is IProfileManager)
            {
              profileManager = obj as IProfileManager;
              if (profileManager != null)
              {
                switch (loadAction)
                {
                  case Package.ProfileManagerLoadAction.LoadPropsFromRegistry:
                    profileManager.LoadSettingsFromStorage();
                    goto label_15;
                  case Package.ProfileManagerLoadAction.ResetSettings:
                    profileManager.ResetSettings();
                    goto label_15;
                  default:
                    goto label_15;
                }
              }
              else
                break;
            }
            else
              break;
          }
        }
      }
label_15:
      if (profileManager == null)
      {
        foreach (Attribute attribute in TypeDescriptor.GetAttributes((object) this))
        {
          if (attribute is ProvideProfileAttribute)
          {
            Type objectType = ((ProvideProfileAttribute) attribute).ObjectType;
            if (objectType.GUID.Equals(objectGuid))
            {
              ConstructorInfo constructor = objectType.GetConstructor(new Type[0]);
              if (constructor == (ConstructorInfo) null)
              {
                throw new ArgumentException(string.Format((IFormatProvider) Resources.Culture, Resources.Package_PageCtorMissing, new object[1]
                {
                  (object) objectType.FullName
                }));
              }
              else
              {
                profileManager = (IProfileManager) constructor.Invoke(new object[0]);
                if (profileManager != null)
                {
                  if (this._pagesAndProfiles == null)
                    this._pagesAndProfiles = (Container) new Package.PackageContainer((System.IServiceProvider) this);
                  this._pagesAndProfiles.Add((IComponent) profileManager);
                  break;
                }
                else
                  break;
              }
            }
          }
        }
      }
      return profileManager;
    }

    private string GetRegistryRoot()
    {
      IVsShell vsShell = (IVsShell) this.GetService(typeof (SVsShell));
      string str;
      if (vsShell == null)
      {
        DefaultRegistryRootAttribute registryRootAttribute = (DefaultRegistryRootAttribute) TypeDescriptor.GetAttributes(this.GetType())[typeof (DefaultRegistryRootAttribute)];
        if (registryRootAttribute == null)
          throw new NotSupportedException();
        str = "SOFTWARE\\Microsoft\\VisualStudio\\" + registryRootAttribute.Root;
      }
      else
      {
        object pvar;
        Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(vsShell.GetProperty(-9002, out pvar));
        str = pvar.ToString();
      }
      return str;
    }

    protected object GetService(Type serviceType)
    {
      if (this.zombie)
        return (object) null;
      if (serviceType == (Type) null)
        throw new ArgumentNullException("serviceType");
      if (serviceType.IsEquivalentTo(typeof (IServiceContainer)) || serviceType.IsEquivalentTo(typeof (Package)) || serviceType.IsEquivalentTo(this.GetType()))
        return (object) this;
      object obj = (object) null;
      if (this._services != null && this._services.Count > 0)
      {
        lock (serviceType)
        {
          if (this._services.ContainsKey(serviceType))
            obj = this._services[serviceType];
          if (obj is Package.ProfferedService)
            obj = ((Package.ProfferedService) obj).Instance;
          if (obj is ServiceCreatorCallback)
          {
            this._services[serviceType] = (object) null;
            obj = ((ServiceCreatorCallback) obj)((IServiceContainer) this, serviceType);
            if (obj == null)
            {
              string local_1 = "An object was not returned from a service creator callback for the registered type of " + serviceType.Name + ".  This may mean that it failed a type equivalence comparison.  To compare type objects you must use Type.IsEquivalentTo(Type).  Do not use .Equals or the == operator.";
              IVsAppCommandLine local_2 = this.GetService(typeof (SVsAppCommandLine)) as IVsAppCommandLine;
              if (local_2 != null)
              {
                int local_4 = 0;
                string local_3;
                local_2.GetOption("RootSuffix", out local_4, out local_3);
                if (local_4 == 1 && string.Compare(local_3, "Exp", StringComparison.OrdinalIgnoreCase) == 0)
                {
                  int temp_101 = (int) MessageBox.Show(local_1);
                }
              }
            }
            else if (!obj.GetType().IsCOMObject && !serviceType.IsAssignableFrom(obj.GetType()))
              obj = (object) null;
            this._services[serviceType] = obj;
          }
        }
      }
      if (obj == null && this._provider != null && (this._services == null || this._services.Count == 0 || !this._services.ContainsKey(serviceType)))
        obj = this._provider.GetService(serviceType);
      return obj;
    }

    protected virtual void Initialize()
    {
      if (this._services.Count > 0)
      {
        IProfferService profferService = (IProfferService) this.GetService(typeof (SProfferService));
        if (profferService != null)
        {
          foreach (KeyValuePair<Type, object> keyValuePair in (Dictionary<Type, object>) this._services)
          {
            Package.ProfferedService profferedService = keyValuePair.Value as Package.ProfferedService;
            if (profferedService != null)
            {
              Guid guid = keyValuePair.Key.GUID;
              uint pdwCookie;
              Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(profferService.ProfferService(ref guid, (Microsoft.VisualStudio.OLE.Interop.IServiceProvider) this, out pdwCookie));
              profferedService.Cookie = pdwCookie;
            }
          }
        }
      }
      Thread.CurrentThread.CurrentUICulture = new CultureInfo(this.GetProviderLocale());
      SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
      if (this._optionKeys == null)
        return;
      try
      {
        IVsSolutionPersistence solutionPersistence = (IVsSolutionPersistence) this.GetService(typeof (SVsSolutionPersistence));
        if (solutionPersistence == null)
          return;
        foreach (string pszKey in this._optionKeys)
          solutionPersistence.LoadPackageUserOpts((IVsPersistSolutionOpts) this, pszKey);
      }
      catch (Exception ex)
      {
      }
    }

    protected virtual int QueryClose(out bool canClose)
    {
      canClose = true;
      return 0;
    }

    public int GetProviderLocale()
    {
      int num = CultureInfo.CurrentCulture.LCID;
      IUIHostLocale uiHostLocale = (IUIHostLocale) this.GetService(typeof (IUIHostLocale));
      if (uiHostLocale != null)
      {
        uint plcid;
        Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(uiHostLocale.GetUILocale(out plcid));
        num = (int) plcid;
      }
      return num;
    }

    public object CreateInstance(ref Guid clsid, ref Guid iid, Type type)
    {
      object obj = (object) null;
      IntPtr instance = this.CreateInstance(ref clsid, ref iid);
      if (instance != IntPtr.Zero)
      {
        try
        {
          obj = Marshal.GetTypedObjectForIUnknown(instance, type);
        }
        finally
        {
          Marshal.Release(instance);
        }
      }
      else
        obj = Activator.CreateInstance(type);
      return obj;
    }

    private IntPtr CreateInstance(ref Guid clsid, ref Guid iid)
    {
      IntPtr ppvObj;
      Microsoft.VisualStudio.NativeMethods.ThrowOnFailure((this.GetService(typeof (SLocalRegistry)) as ILocalRegistry3).CreateInstance(clsid, (object) null, ref iid, 1U, out ppvObj));
      return ppvObj;
    }

    public IVsOutputWindowPane GetOutputPane(Guid page, string caption)
    {
      IVsOutputWindow vsOutputWindow = this.GetService(typeof (SVsOutputWindow)) as IVsOutputWindow;
      IVsOutputWindowPane ppPane = (IVsOutputWindowPane) null;
      if (Microsoft.VisualStudio.NativeMethods.Failed(vsOutputWindow.GetPane(ref page, out ppPane)) && caption != null && Microsoft.VisualStudio.NativeMethods.Succeeded(vsOutputWindow.CreatePane(ref page, caption, 1, 1)))
        vsOutputWindow.GetPane(ref page, out ppPane);
      if (ppPane != null)
        Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(ppPane.Activate());
      return ppPane;
    }

    private object OnCreateService(IServiceContainer container, Type serviceType)
    {
      if (serviceType.IsEquivalentTo(typeof (IOleCommandTarget)))
      {
        object service = this.GetService(typeof (IMenuCommandService));
        if (service is IOleCommandTarget)
          return service;
      }
      else if (serviceType.IsEquivalentTo(typeof (IMenuCommandService)))
        return (object) new OleMenuCommandService((System.IServiceProvider) this);
      return (object) null;
    }

    protected virtual void OnLoadOptions(string key, Stream stream)
    {
    }

    protected virtual void OnSaveOptions(string key, Stream stream)
    {
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
      if (e.Category != UserPreferenceCategory.Locale)
        return;
      CultureInfo.CurrentCulture.ClearCachedData();
    }

    protected void ParseToolboxResource(TextReader resourceData, ResourceManager localizedCategories)
    {
      this.ParseToolboxResource(resourceData, localizedCategories, Guid.Empty);
    }

    protected void ParseToolboxResource(TextReader resourceData, Guid packageGuid)
    {
      this.ParseToolboxResource(resourceData, (ResourceManager) null, packageGuid);
    }

    private void ParseToolboxResource(TextReader resourceData, ResourceManager localizedCategories, Guid packageGuid)
    {
      if (resourceData == null)
        throw new ArgumentNullException("resourceData");
      IToolboxService toolboxService = this.GetService(typeof (IToolboxService)) as IToolboxService;
      if (toolboxService == null)
      {
        throw new InvalidOperationException(string.Format((IFormatProvider) Resources.Culture, Resources.General_MissingService, new object[1]
        {
          (object) typeof (IToolboxService).FullName
        }));
      }
      else
      {
        IVsToolbox vsToolbox = this.GetService(typeof (SVsToolbox)) as IVsToolbox;
        IVsToolbox2 vsToolbox2 = vsToolbox as IVsToolbox2;
        IVsToolbox3 vsToolbox3 = vsToolbox as IVsToolbox3;
        if (vsToolbox3 == null)
        {
          throw new InvalidOperationException(string.Format((IFormatProvider) Resources.Culture, Resources.General_MissingService, new object[1]
          {
            (object) typeof (SVsToolbox).FullName
          }));
        }
        else
        {
          string str1 = resourceData.ReadLine();
          string str2 = (string) null;
          string str3 = (string) null;
          for (; str1 != null; str1 = resourceData.ReadLine())
          {
            try
            {
              string str4 = str1.Trim();
              if (str4.Length != 0)
              {
                if (!str4.StartsWith(";", StringComparison.OrdinalIgnoreCase))
                {
                  if (str4.StartsWith("[", StringComparison.OrdinalIgnoreCase) && str4.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                  {
                    str2 = str4.Trim('[', ']').Trim();
                    string lpszTabID = str2;
                    if (localizedCategories != null)
                    {
                      string @string = localizedCategories.GetString(str2);
                      if (@string != null)
                        str2 = @string;
                    }
                    bool flag = false;
                    if (!string.IsNullOrEmpty(str2))
                    {
                      if (packageGuid != Guid.Empty && vsToolbox2 != null)
                      {
                        vsToolbox2.AddTab2(str2, ref packageGuid);
                        if (!string.IsNullOrEmpty(lpszTabID) && vsToolbox3 != null)
                        {
                          vsToolbox3.SetIDOfTab(str2, packageGuid.ToString("B") + "-" + lpszTabID);
                          lpszTabID = (string) null;
                        }
                        flag = true;
                      }
                      else if (vsToolbox != null)
                      {
                        vsToolbox.AddTab(str2);
                        flag = true;
                      }
                      if (flag)
                      {
                        if (!string.IsNullOrEmpty(lpszTabID))
                        {
                          if (vsToolbox3 != null)
                          {
                            vsToolbox3.SetIDOfTab(str2, lpszTabID);
                            str3 = (string) null;
                          }
                        }
                      }
                    }
                  }
                  else
                  {
                    int length = str4.IndexOf(",");
                    if (length != -1)
                    {
                      string name = str4.Substring(0, length).Trim();
                      string str5 = str4.Substring(length + 1).Trim();
                      if (str5.IndexOf(",") == -1)
                      {
                        IEnumerator enumerator = new AssemblyEnumerationService((System.IServiceProvider) this).GetAssemblyNames(str5).GetEnumerator();
                        try
                        {
                          if (enumerator.MoveNext())
                            str5 = ((AssemblyName) enumerator.Current).FullName;
                        }
                        finally
                        {
                          IDisposable disposable = enumerator as IDisposable;
                          if (disposable != null)
                            disposable.Dispose();
                        }
                      }
                      Assembly assembly = Assembly.Load(str5);
                      if (assembly != (Assembly) null)
                      {
                        Type type = assembly.GetType(name);
                        if (type != (Type) null)
                        {
                          ToolboxItem toolboxItem = ToolboxService.GetToolboxItem(type);
                          if (toolboxItem != null)
                          {
                            if (str2 == null)
                              toolboxService.AddToolboxItem(toolboxItem);
                            else
                              toolboxService.AddToolboxItem(toolboxItem, str2);
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
            catch (Exception ex)
            {
            }
          }
        }
      }
    }

    protected void RegisterEditorFactory(IVsEditorFactory factory)
    {
      IVsRegisterEditors vsRegisterEditors = this.GetService(typeof (SVsRegisterEditors)) as IVsRegisterEditors;
      if (vsRegisterEditors == null)
      {
        throw new InvalidOperationException(string.Format((IFormatProvider) Resources.Culture, Resources.Package_MissingService, new object[1]
        {
          (object) typeof (SVsRegisterEditors).FullName
        }));
      }
      else
      {
        Guid guid = factory.GetType().GUID;
        uint pdwCookie;
        Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(vsRegisterEditors.RegisterEditor(ref guid, factory, out pdwCookie));
        if (this._editorFactories == null)
          this._editorFactories = new Hashtable();
        this._editorFactories[(object) factory] = (object) pdwCookie;
      }
    }

    protected void RegisterProjectFactory(IVsProjectFactory factory)
    {
      IVsRegisterProjectTypes registerProjectTypes = this.GetService(typeof (SVsRegisterProjectTypes)) as IVsRegisterProjectTypes;
      if (registerProjectTypes == null)
      {
        throw new InvalidOperationException(string.Format((IFormatProvider) Resources.Culture, Resources.Package_MissingService, new object[1]
        {
          (object) typeof (SVsRegisterProjectTypes).FullName
        }));
      }
      else
      {
        Guid guid = factory.GetType().GUID;
        uint pdwCookie;
        Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(registerProjectTypes.RegisterProjectType(ref guid, factory, out pdwCookie));
        if (this._projectFactories == null)
          this._projectFactories = new Hashtable();
        this._projectFactories[(object) factory] = (object) pdwCookie;
      }
    }

    public void ShowOptionPage(Type optionsPageType)
    {
      if (optionsPageType == (Type) null)
        throw new ArgumentNullException("optionsPageType");
      MenuCommandService menuCommandService = this.GetService(typeof (IMenuCommandService)) as MenuCommandService;
      if (menuCommandService == null)
        return;
      CommandID commandId = new CommandID(Microsoft.VisualStudio.NativeMethods.GUID_VSStandardCommandSet97, 264);
      menuCommandService.GlobalInvoke(commandId, (object) optionsPageType.GUID.ToString());
    }

    int IOleCommandTarget.Exec(ref Guid guidGroup, uint nCmdId, uint nCmdExcept, IntPtr pIn, IntPtr vOut)
    {
      IOleCommandTarget oleCommandTarget = (IOleCommandTarget) this.GetService(typeof (IOleCommandTarget));
      if (oleCommandTarget != null)
        return oleCommandTarget.Exec(ref guidGroup, nCmdId, nCmdExcept, pIn, vOut);
      else
        return -2147221248;
    }

    int IOleCommandTarget.QueryStatus(ref Guid guidGroup, uint nCmdId, OLECMD[] oleCmd, IntPtr oleText)
    {
      IOleCommandTarget oleCommandTarget = (IOleCommandTarget) this.GetService(typeof (IOleCommandTarget));
      if (oleCommandTarget != null)
        return oleCommandTarget.QueryStatus(ref guidGroup, nCmdId, oleCmd, oleText);
      else
        return -2147221248;
    }

    int Microsoft.VisualStudio.OLE.Interop.IServiceProvider.QueryService(ref Guid sid, ref Guid iid, out IntPtr ppvObj)
    {
      ppvObj = (IntPtr) 0;
      int num = 0;
      object o = (object) null;
      if (this._services != null && this._services.Count > 0)
      {
        foreach (Type serviceType in this._services.Keys)
        {
          if (serviceType.GUID.Equals(sid))
          {
            o = this.GetService(serviceType);
            break;
          }
        }
      }
      if (o == null)
        num = -2147467262;
      else if (iid.Equals(Microsoft.VisualStudio.NativeMethods.IID_IUnknown))
      {
        ppvObj = Marshal.GetIUnknownForObject(o);
      }
      else
      {
        IntPtr iunknownForObject = Marshal.GetIUnknownForObject(o);
        num = Marshal.QueryInterface(iunknownForObject, ref iid, out ppvObj);
        Marshal.Release(iunknownForObject);
      }
      return num;
    }

    void IServiceContainer.AddService(Type serviceType, object serviceInstance)
    {
      this.AddService(serviceType, serviceInstance, false);
    }

    void IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote)
    {
      if (serviceType == (Type) null)
        throw new ArgumentNullException("serviceType");
      if (serviceInstance == null)
        throw new ArgumentNullException("serviceInstance");
      if (this._services.ContainsKey(serviceType))
        throw new InvalidOperationException(string.Format((IFormatProvider) Resources.Culture, Resources.Package_DuplicateService, new object[1]
        {
          (object) serviceType.FullName
        }));
      else if (promote)
      {
        Package.ProfferedService profferedService = new Package.ProfferedService();
        profferedService.Instance = serviceInstance;
        if (this._provider == null)
          return;
        IProfferService profferService = (IProfferService) this.GetService(typeof (SProfferService));
        if (profferService == null)
          return;
        Guid guid = serviceType.GUID;
        uint pdwCookie;
        Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(profferService.ProfferService(ref guid, (Microsoft.VisualStudio.OLE.Interop.IServiceProvider) this, out pdwCookie));
        profferedService.Cookie = pdwCookie;
        this._services[serviceType] = (object) profferedService;
      }
      else
        this._services[serviceType] = serviceInstance;
    }

    void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback)
    {
      this.AddService(serviceType, callback, false);
    }

    void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
    {
      if (serviceType == (Type) null)
        throw new ArgumentNullException("serviceType");
      if (callback == null)
        throw new ArgumentNullException("callback");
      if (this._services.ContainsKey(serviceType))
        throw new InvalidOperationException(string.Format((IFormatProvider) Resources.Culture, Resources.Package_DuplicateService, new object[1]
        {
          (object) serviceType.FullName
        }));
      else if (promote)
      {
        Package.ProfferedService profferedService = new Package.ProfferedService();
        this._services[serviceType] = (object) profferedService;
        profferedService.Instance = (object) callback;
        if (this._provider == null)
          return;
        IProfferService profferService = (IProfferService) this.GetService(typeof (SProfferService));
        if (profferService == null)
          return;
        Guid guid = serviceType.GUID;
        uint pdwCookie;
        Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(profferService.ProfferService(ref guid, (Microsoft.VisualStudio.OLE.Interop.IServiceProvider) this, out pdwCookie));
        profferedService.Cookie = pdwCookie;
      }
      else
        this._services[serviceType] = (object) callback;
    }

    void IServiceContainer.RemoveService(Type serviceType)
    {
      this.RemoveService(serviceType, false);
    }

    void IServiceContainer.RemoveService(Type serviceType, bool promote)
    {
      if (serviceType == (Type) null)
        throw new ArgumentNullException("serviceType");
      if (this._services == null || this._services.Count <= 0)
        return;
      object obj = (object) null;
      if (this._services.ContainsKey(serviceType))
        obj = this._services[serviceType];
      if (obj == null)
        return;
      this._services.Remove(serviceType);
      try
      {
        Package.ProfferedService profferedService = obj as Package.ProfferedService;
        if (profferedService == null)
          return;
        obj = profferedService.Instance;
        if ((int) profferedService.Cookie == 0)
          return;
        IProfferService profferService = (IProfferService) this.GetService(typeof (SProfferService));
        if (profferService != null)
          Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(profferService.RevokeService(profferedService.Cookie));
        profferedService.Cookie = 0U;
      }
      finally
      {
        if (obj is IDisposable)
          ((IDisposable) obj).Dispose();
      }
    }

    object System.IServiceProvider.GetService(Type serviceType)
    {
      return this.GetService(serviceType);
    }

    int IVsPackage.Close()
    {
      if (!this.zombie)
        this.Dispose(true);
      this.zombie = true;
      return 0;
    }

    public int CreateTool(ref Guid persistenceSlot)
    {
      if (this.zombie)
        Marshal.ThrowExceptionForHR(-2147418113);
      return this.CreateToolWindow(ref persistenceSlot, 0U);
    }

    int IVsToolWindowFactory.CreateToolWindow(ref Guid toolWindowType, uint id)
    {
      if (id > (uint) int.MaxValue)
      {
        throw new ArgumentOutOfRangeException(string.Format((IFormatProvider) CultureInfo.CurrentUICulture, "Instance ID cannot be more then {0}", new object[1]
        {
          (object) int.MaxValue
        }));
      }
      else
      {
        int id1 = (int) id;
        foreach (Attribute attribute in Attribute.GetCustomAttributes((MemberInfo) this.GetType()))
        {
          if (attribute is ProvideToolWindowAttribute)
          {
            ProvideToolWindowAttribute tool = (ProvideToolWindowAttribute) attribute;
            if (tool.ToolType.GUID == toolWindowType)
            {
              this.FindToolWindow(tool.ToolType, id1, true, tool);
              break;
            }
          }
        }
        return 0;
      }
    }

    protected WindowPane CreateToolWindow(Type toolWindowType, int id)
    {
      if (toolWindowType == (Type) null)
        throw new ArgumentNullException("toolWindowType");
      if (id < 0)
      {
        throw new ArgumentOutOfRangeException(string.Format((IFormatProvider) Resources.Culture, Resources.Package_InvalidInstanceID, new object[1]
        {
          (object) id
        }));
      }
      else
      {
        if (!toolWindowType.IsSubclassOf(typeof (WindowPane)))
          throw new ArgumentException(Resources.Package_InvalidToolWindowClass);
        foreach (Attribute attribute in Attribute.GetCustomAttributes((MemberInfo) this.GetType()))
        {
          if (attribute is ProvideToolWindowAttribute)
          {
            ProvideToolWindowAttribute tool = (ProvideToolWindowAttribute) attribute;
            if (tool.ToolType == toolWindowType)
              return this.CreateToolWindow(toolWindowType, id, tool);
          }
        }
        return (WindowPane) null;
      }
    }

    private WindowPane CreateToolWindow(Type toolWindowType, int id, ProvideToolWindowAttribute tool)
    {
      if (toolWindowType == (Type) null)
        throw new ArgumentNullException("toolWindowType");
      if (id < 0)
      {
        throw new ArgumentOutOfRangeException(string.Format((IFormatProvider) Resources.Culture, Resources.Package_InvalidInstanceID, new object[1]
        {
          (object) id
        }));
      }
      else
      {
        if (!toolWindowType.IsSubclassOf(typeof (WindowPane)))
          throw new ArgumentException(Resources.Package_InvalidToolWindowClass);
        if (tool == null)
          throw new ArgumentNullException("tool");
        WindowPane windowPane = (WindowPane) Activator.CreateInstance(toolWindowType);
        ToolWindowPane pane = windowPane as ToolWindowPane;
        bool flag = false;
        Guid rguidAutoActivate = Guid.Empty;
        Guid rclsidTool = Guid.Empty;
        string pszCaption = (string) null;
        if (pane != null)
        {
          rclsidTool = pane.ToolClsid;
          pszCaption = pane.Caption;
          flag = pane.ToolBar != null;
          pane.Package = (object) this;
        }
        uint grfCTW = 65536U;
        if (!tool.Transient)
          grfCTW |= 524288U;
        if (flag)
          grfCTW |= 4194304U;
        if (tool.MultiInstances)
          grfCTW |= 2097152U;
        object punkTool = (object) null;
        if (rclsidTool.CompareTo(Guid.Empty) == 0)
          punkTool = pane == null ? (object) windowPane : pane.GetIVsWindowPane();
        Guid guid1 = toolWindowType.GUID;
        IVsUIShell vsUiShell = (IVsUIShell) this.GetService(typeof (SVsUIShell));
        if (vsUiShell == null)
        {
          throw new Exception(string.Format((IFormatProvider) Resources.Culture, Resources.General_MissingService, new object[1]
          {
            (object) typeof (SVsUIShell).FullName
          }));
        }
        else
        {
          IVsWindowFrame ppWindowFrame;
          Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(vsUiShell.CreateToolWindow(grfCTW, (uint) id, punkTool, ref rclsidTool, ref guid1, ref rguidAutoActivate, (Microsoft.VisualStudio.OLE.Interop.IServiceProvider) null, pszCaption, (int[]) null, out ppWindowFrame));
          IComponent component = (windowPane.Content == null ? windowPane.Window as IComponent : windowPane.Content as IComponent) ?? punkTool as IComponent;
          if (component != null)
          {
            if (this._componentToolWindows == null)
              this._componentToolWindows = (Container) new Package.PackageContainer((System.IServiceProvider) this);
            this._componentToolWindows.Add(component);
          }
          if (pane != null)
            pane.Frame = (object) ppWindowFrame;
          if (flag && ppWindowFrame != null && pane != null)
          {
            object pvar;
            Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(ppWindowFrame.GetProperty(-5008, out pvar));
            // ISSUE: variable of a compiler-generated type
            IVsToolWindowToolbarHost2 windowToolbarHost2 = (IVsToolWindowToolbarHost2) pvar;
            if (windowToolbarHost2 != null)
            {
              Guid guid2 = pane.ToolBar.Guid;
              // ISSUE: reference to a compiler-generated method
              Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(windowToolbarHost2.AddToolbar2((VSTWT_LOCATION) pane.ToolBarLocation, ref guid2, (uint) pane.ToolBar.ID, pane.ToolBarDropTarget));
            }
            pane.OnToolBarAdded();
          }
          if (pane != null)
          {
            if (this._toolWindows == null)
              this._toolWindows = new ToolWindowCollection();
            this._toolWindows.Add(toolWindowType.GUID, id, pane);
          }
          return windowPane;
        }
      }
    }

    public ToolWindowPane FindToolWindow(Type toolWindowType, int id, bool create)
    {
      return this.FindToolWindow(toolWindowType, id, create, (ProvideToolWindowAttribute) null) as ToolWindowPane;
    }

    public WindowPane FindWindowPane(Type toolWindowType, int id, bool create)
    {
      return this.FindToolWindow(toolWindowType, id, create, (ProvideToolWindowAttribute) null);
    }

    private WindowPane FindToolWindow(Type toolWindowType, int id, bool create, ProvideToolWindowAttribute tool)
    {
      if (toolWindowType == (Type) null)
        throw new ArgumentNullException("toolWindowType");
      WindowPane windowPane = (WindowPane) null;
      if (this._toolWindows != null)
        windowPane = (WindowPane) this._toolWindows.GetToolWindowPane(toolWindowType.GUID, id);
      if (windowPane == null && create)
        windowPane = tool == null ? this.CreateToolWindow(toolWindowType, id) : this.CreateToolWindow(toolWindowType, id, tool);
      return windowPane;
    }

    int IVsPackage.GetAutomationObject(string propName, out object auto)
    {
      if (this.zombie)
        Marshal.ThrowExceptionForHR(-2147418113);
      auto = this.GetAutomationObject(propName);
      if (auto == null)
        Marshal.ThrowExceptionForHR(-2147467263);
      return 0;
    }

    int IVsPackage.GetPropertyPage(ref Guid rguidPage, VSPROPSHEETPAGE[] ppage)
    {
      if (ppage == null || ppage.Length < 1)
        throw new ArgumentException(string.Format((IFormatProvider) Resources.Culture, Resources.General_ArraySizeShouldBeAtLeast1, new object[0]), "ppage");
      if (this.zombie)
        Marshal.ThrowExceptionForHR(-2147418113);
      IWin32Window win32Window1 = (IWin32Window) null;
      if (this._pagesAndProfiles != null)
      {
        foreach (object obj in (ReadOnlyCollectionBase) this._pagesAndProfiles.Components)
        {
          if (obj.GetType().GUID.Equals(rguidPage))
          {
            IWin32Window win32Window2 = obj as IWin32Window;
            if (win32Window2 != null)
            {
              if (win32Window2 is DialogPage)
                ((DialogPage) win32Window2).ResetContainer();
              win32Window1 = win32Window2;
              break;
            }
          }
        }
      }
      if (win32Window1 == null)
      {
        DialogPage dialogPage = (DialogPage) null;
        foreach (Attribute attribute in TypeDescriptor.GetAttributes((object) this))
        {
          if (attribute is ProvideOptionDialogPageAttribute)
          {
            Type pageType = ((ProvideOptionDialogPageAttribute) attribute).PageType;
            if (pageType.GUID.Equals(rguidPage))
            {
              win32Window1 = (IWin32Window) this.GetDialogPage(pageType);
              break;
            }
          }
          if (dialogPage != null)
          {
            if (this._pagesAndProfiles == null)
              this._pagesAndProfiles = (Container) new Package.PackageContainer((System.IServiceProvider) this);
            this._pagesAndProfiles.Add((IComponent) dialogPage);
            break;
          }
        }
      }
      if (win32Window1 == null)
        Marshal.ThrowExceptionForHR(-2147467263);
      ppage[0].dwSize = (uint) Marshal.SizeOf(typeof (VSPROPSHEETPAGE));
      ppage[0].hwndDlg = win32Window1.Handle;
      ppage[0].dwFlags = 0U;
      ppage[0].HINSTANCE = 0U;
      ppage[0].dwTemplateSize = 0U;
      ppage[0].pTemplate = IntPtr.Zero;
      ppage[0].pfnDlgProc = IntPtr.Zero;
      ppage[0].lParam = IntPtr.Zero;
      ppage[0].pfnCallback = IntPtr.Zero;
      ppage[0].pcRefParent = IntPtr.Zero;
      ppage[0].dwReserved = 0U;
      ppage[0].wTemplateId = (ushort) 0;
      return 0;
    }

    int IVsPackage.QueryClose(out int close)
    {
      close = 1;
      bool canClose = true;
      int num = this.QueryClose(out canClose);
      if (!canClose)
        close = 0;
      return num;
    }

    int IVsPackage.ResetDefaults(uint grfFlags)
    {
      if (this.zombie)
        Marshal.ThrowExceptionForHR(-2147418113);
      if ((int) grfFlags == 1)
      {
        if (this.ToolboxInitialized != null)
          this.ToolboxInitialized((object) this, EventArgs.Empty);
      }
      else if ((int) grfFlags == 2 && this.ToolboxUpgraded != null)
        this.ToolboxUpgraded((object) this, EventArgs.Empty);
      return 0;
    }

    int IVsPackage.SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp)
    {
      if (this.zombie)
        Marshal.ThrowExceptionForHR(-2147418113);
      if (sp != null)
      {
        if (this._provider != null)
        {
          throw new InvalidOperationException(string.Format((IFormatProvider) Resources.Culture, Resources.Package_SiteAlreadySet, new object[1]
          {
            (object) this.GetType().FullName
          }));
        }
        else
        {
          this._provider = ServiceProvider.CreateFromSetSite(sp);
          this.Initialize();
        }
      }
      else if (this._provider != null)
        this.Dispose(true);
      return 0;
    }

    int IVsPersistSolutionOpts.LoadUserOptions(IVsSolutionPersistence pPersistance, uint options)
    {
      int hr = 0;
      if (((int) options & 1) != 0)
        return hr;
      if (this._optionKeys != null)
      {
        foreach (string pszKey in this._optionKeys)
        {
          hr = pPersistance.LoadPackageUserOpts((IVsPersistSolutionOpts) this, pszKey);
          if (Microsoft.VisualStudio.NativeMethods.Failed(hr))
            break;
        }
      }
      Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(hr);
      return hr;
    }

    int IVsPersistSolutionOpts.ReadUserOptions(IStream pStream, string pszKey)
    {
      Microsoft.VisualStudio.NativeMethods.DataStreamFromComStream streamFromComStream = new Microsoft.VisualStudio.NativeMethods.DataStreamFromComStream(pStream);
      using (streamFromComStream)
        this.OnLoadOptions(pszKey, (Stream) streamFromComStream);
      return 0;
    }

    int IVsPersistSolutionOpts.SaveUserOptions(IVsSolutionPersistence pPersistance)
    {
      if (this._optionKeys != null)
      {
        foreach (string pszKey in this._optionKeys)
          Microsoft.VisualStudio.NativeMethods.ThrowOnFailure(pPersistance.SavePackageUserOpts((IVsPersistSolutionOpts) this, pszKey));
      }
      return 0;
    }

    int IVsPersistSolutionOpts.WriteUserOptions(IStream pStream, string pszKey)
    {
      Microsoft.VisualStudio.NativeMethods.DataStreamFromComStream streamFromComStream = new Microsoft.VisualStudio.NativeMethods.DataStreamFromComStream(pStream);
      using (streamFromComStream)
        this.OnSaveOptions(pszKey, (Stream) streamFromComStream);
      return 0;
    }

    int IVsToolboxItemProvider.GetItemContent(string itemId, ushort format, out IntPtr global)
    {
      if (this.zombie)
        Marshal.ThrowExceptionForHR(-2147418113);
      object toolboxItemData = this.GetToolboxItemData(itemId, DataFormats.GetFormat((int) format));
      if (toolboxItemData == null)
      {
        global = IntPtr.Zero;
      }
      else
      {
        OleDataObject oleDataObject = new OleDataObject();
        oleDataObject.SetData(DataFormats.GetFormat((int) format).Name, toolboxItemData);
        FORMATETC[] pformatetcIn = new FORMATETC[1]
        {
          new FORMATETC()
        };
        pformatetcIn[0].cfFormat = format;
        pformatetcIn[0].dwAspect = 1U;
        pformatetcIn[0].lindex = -1;
        pformatetcIn[0].tymed = 1U;
        STGMEDIUM[] pRemoteMedium = new STGMEDIUM[1]
        {
          new STGMEDIUM()
        };
        pRemoteMedium[0].tymed = 1U;
        oleDataObject.GetData(pformatetcIn, pRemoteMedium);
        global = pRemoteMedium[0].unionmember;
      }
      return 0;
    }

    protected virtual object GetToolboxItemData(string itemId, DataFormats.Format format)
    {
      if (string.IsNullOrEmpty(itemId))
        throw new ArgumentNullException("itemId");
      System.Windows.Forms.IDataObject dataObject;
      if (this._tbxItemDataCache.TryGetValue(itemId, out dataObject))
      {
        if (dataObject.GetDataPresent(format.Name))
          return dataObject.GetData(format.Name);
        throw new InvalidOperationException(string.Format((IFormatProvider) Resources.Culture, Resources.Toolbox_UnsupportedFormat, new object[1]
        {
          (object) format.Name
        }));
      }
      else
      {
        int length = itemId.IndexOf(",");
        if (length == -1)
        {
          throw new InvalidOperationException(string.Format((IFormatProvider) Resources.Culture, Resources.Toolbox_InvalidItemId, new object[1]
          {
            (object) itemId
          }));
        }
        else
        {
          string name = itemId.Substring(0, length).Trim();
          string str = itemId.Substring(length + 1).Trim();
          if (str.IndexOf(",") == -1)
          {
            IEnumerator enumerator = new AssemblyEnumerationService((System.IServiceProvider) this).GetAssemblyNames(str).GetEnumerator();
            try
            {
              if (enumerator.MoveNext())
                str = ((AssemblyName) enumerator.Current).FullName;
            }
            finally
            {
              IDisposable disposable = enumerator as IDisposable;
              if (disposable != null)
                disposable.Dispose();
            }
          }
          Assembly assembly = Assembly.Load(str);
          if (assembly != (Assembly) null)
          {
            Type type = assembly.GetType(name);
            if (type != (Type) null)
            {
              ToolboxItem toolboxItem = ToolboxService.GetToolboxItem(type);
              if (toolboxItem != null)
              {
                System.Windows.Forms.IDataObject toolboxData = new ToolboxItemContainer(toolboxItem).ToolboxData;
                this._tbxItemDataCache.Add(itemId, toolboxData);
                if (toolboxData.GetDataPresent(format.Name))
                  return toolboxData.GetData(format.Name);
                throw new InvalidOperationException(string.Format((IFormatProvider) Resources.Culture, Resources.Toolbox_UnsupportedFormat, new object[1]
                {
                  (object) format.Name
                }));
              }
            }
          }
          throw new InvalidOperationException(string.Format((IFormatProvider) Resources.Culture, Resources.Toolbox_ItemNotFound, new object[1]
          {
            (object) itemId
          }));
        }
      }
    }

    public static object GetGlobalService(Type serviceType)
    {
      object obj = (object) null;
      ServiceProvider globalProvider = ServiceProvider.GlobalProvider;
      if (globalProvider != null)
        obj = globalProvider.GetService(serviceType);
      return obj;
    }

    private enum ProfileManagerLoadAction
    {
      None,
      LoadPropsFromRegistry,
      ResetSettings,
    }

    private sealed class PackageContainer : Container
    {
      private IUIService _uis;
      private AmbientProperties _ambientProperties;
      private System.IServiceProvider _provider;

      internal PackageContainer(System.IServiceProvider provider)
      {
        this._provider = provider;
      }

      protected override object GetService(Type serviceType)
      {
        if (serviceType == (Type) null)
          throw new ArgumentNullException("serviceType");
        if (this._provider != null)
        {
          if (serviceType.IsEquivalentTo(typeof (AmbientProperties)))
          {
            if (this._uis == null)
              this._uis = (IUIService) this._provider.GetService(typeof (IUIService));
            if (this._ambientProperties == null)
              this._ambientProperties = new AmbientProperties();
            if (this._uis != null)
              this._ambientProperties.Font = (Font) this._uis.Styles[(object) "DialogFont"];
            return (object) this._ambientProperties;
          }
          else
          {
            object service = this._provider.GetService(serviceType);
            if (service != null)
              return service;
          }
        }
        return base.GetService(serviceType);
      }
    }

    private sealed class ProfferedService
    {
      public object Instance;
      public uint Cookie;
    }
  }
}
