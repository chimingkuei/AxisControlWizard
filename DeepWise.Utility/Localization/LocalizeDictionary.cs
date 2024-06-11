namespace DeepWise.Localization
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using XAMLMarkupExtensions.Base;
    using WPFLocalizeExtension;
    using System.Collections;
    using System.Data.SqlTypes;
    using System.Reflection;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;
    using WPFLocalizeExtension.TypeConverters;
    using DeepWise.Localization;
    using System.Resources;
    #endregion

    /// <summary>
    /// Represents the culture interface for localization
    /// </summary>
    public sealed class LocalizeDictionary : DependencyObject, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        /// <summary>
        /// Informiert über sich ändernde Eigenschaften.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify that a property has changed
        /// </summary>
        /// <param name="property">
        /// The property that changed
        /// </param>
        internal void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion

        #region Dependency Properties
        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultProvider to set the default ILocalizationProvider.
        /// </summary>
        public static readonly DependencyProperty DefaultProviderProperty =
            DependencyProperty.RegisterAttached(
                "DefaultProvider",
                typeof(ILocalizationProvider),
                typeof(LocalizeDictionary),
                new PropertyMetadata(null, SetDefaultProviderFromDependencyProperty));

        /// <summary>
        /// <see cref="DependencyProperty"/> Provider to set the ILocalizationProvider.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.RegisterAttached(
                "Provider",
                typeof(ILocalizationProvider),
                typeof(LocalizeDictionary),
                new PropertyMetadata(null, SetProviderFromDependencyProperty));

        /// <summary>
        /// <see cref="DependencyProperty"/> DesignCulture to set the Culture.
        /// Only supported at DesignTime.
        /// </summary>
        [DesignOnly(true)]
        public static readonly DependencyProperty DesignCultureProperty =
            DependencyProperty.RegisterAttached(
                "DesignCulture",
                typeof(string),
                typeof(LocalizeDictionary),
                new PropertyMetadata(SetCultureFromDependencyProperty));

        /// <summary>
        /// <see cref="DependencyProperty"/> Separation to set the separation character/string for resource name patterns.
        /// </summary>
        public static readonly DependencyProperty SeparationProperty =
            DependencyProperty.RegisterAttached(
                "Separation",
                typeof(string),
                typeof(LocalizeDictionary),
                new PropertyMetadata(DefaultSeparation, SetSeparationFromDependencyProperty));

        /// <summary>
        /// A flag indicating that the invariant culture should be included.
        /// </summary>
        public static readonly DependencyProperty IncludeInvariantCultureProperty =
            DependencyProperty.RegisterAttached(
                "IncludeInvariantCulture",
                typeof(bool),
                typeof(LocalizeDictionary),
                new PropertyMetadata(true, SetIncludeInvariantCultureFromDependencyProperty));

        /// <summary>
        /// A flag indicating that the cache is disabled.
        /// </summary>
        public static readonly DependencyProperty DisableCacheProperty =
            DependencyProperty.RegisterAttached(
                "DisableCache",
                typeof(bool),
                typeof(LocalizeDictionary),
                new PropertyMetadata(false, SetDisableCacheFromDependencyProperty));

        /// <summary>
        /// A flag indicating that missing keys should be output.
        /// </summary>
        public static readonly DependencyProperty OutputMissingKeysProperty =
            DependencyProperty.RegisterAttached(
                "OutputMissingKeys",
                typeof(bool),
                typeof(LocalizeDictionary),
                new PropertyMetadata(true, SetOutputMissingKeysFromDependencyProperty));
        #endregion

        #region Dependency Property Callbacks
        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.Culture if set in Xaml.
        /// Only supported at DesignTime.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        [DesignOnly(true)]
        private static void SetCultureFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (!Instance.GetIsInDesignMode())
                return;

            CultureInfo culture;

            try
            {
                culture = new CultureInfo((string)args.NewValue);
            }
            catch
            {
                if (Instance.GetIsInDesignMode())
                    culture = DefaultCultureInfo;
                else
                    throw;
            }

            if (culture != null)
                Instance.Culture = culture;
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.Provider if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetProviderFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DictionaryEvent.Invoke(obj, new DictionaryEventArgs(DictionaryEventType.ProviderChanged, args.NewValue));

            if (args.OldValue is ILocalizationProvider oldProvider)
            {
                oldProvider.ProviderChanged -= ProviderUpdated;
                oldProvider.ValueChanged -= ValueChanged;
                oldProvider.AvailableCultures.CollectionChanged -= Instance.AvailableCulturesCollectionChanged;
            }

            if (args.NewValue is ILocalizationProvider provider)
            {
                provider.ProviderChanged += ProviderUpdated;
                provider.ValueChanged += ValueChanged;
                provider.AvailableCultures.CollectionChanged += Instance.AvailableCulturesCollectionChanged;

                foreach (var c in provider.AvailableCultures)
                    if (!Instance.MergedAvailableCultures.Contains(c))
                        Instance.MergedAvailableCultures.Add(c);
            }
        }

        private static void ProviderUpdated(object sender, ProviderChangedEventArgs args)
        {
            DictionaryEvent.Invoke(args.Object, new DictionaryEventArgs(DictionaryEventType.ProviderUpdated, sender));
        }

        private static void ValueChanged(object sender, ValueChangedEventArgs args)
        {
            DictionaryEvent.Invoke(null, new DictionaryEventArgs(DictionaryEventType.ValueChanged, args));
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.DefaultProvider if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetDefaultProviderFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is ILocalizationProvider provider)
                Instance.DefaultProvider = provider;
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.Separation if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetSeparationFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.IncludeInvariantCulture if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetIncludeInvariantCultureFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is bool b)
                Instance.IncludeInvariantCulture = b;
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.DisableCache if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetDisableCacheFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is bool b)
                Instance.DisableCache = b;
        }

        /// <summary>
        /// Callback function. Used to set the <see cref="LocalizeDictionary"/>.OutputMissingKeys if set in Xaml.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void SetOutputMissingKeysFromDependencyProperty(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue is bool b)
                Instance.OutputMissingKeys = b;
        }
        #endregion

        #region Dependency Property Management
        #region Get
        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> Provider.
        /// </summary>
        /// <param name="obj">The dependency object to get the provider from.</param>
        /// <returns>The provider.</returns>
        public static ILocalizationProvider GetProvider(DependencyObject obj)
        {
            return obj.GetValueSync<ILocalizationProvider>(ProviderProperty);
        }

#pragma warning disable IDE0060
        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> DefaultProvider.
        /// </summary>
        /// <param name="obj">The dependency object to get the default provider from.</param>
        /// <returns>The default provider.</returns>
        public static ILocalizationProvider GetDefaultProvider(DependencyObject obj)
        {
            return Instance.DefaultProvider;
        }

        /// <summary>
        /// Tries to get the separation from the given target object or of one of its parents.
        /// </summary>
        /// <param name="target">The target object for context.</param>
        /// <returns>The separation of the given context or the default.</returns>
        public static string GetSeparation(DependencyObject target)
        {
            return Instance.Separation;
        }

        /// <summary>
        /// Tries to get the flag from the given target object or of one of its parents.
        /// </summary>
        /// <param name="target">The target object for context.</param>
        /// <returns>The flag.</returns>
        public static bool GetIncludeInvariantCulture(DependencyObject target)
        {
            return Instance.IncludeInvariantCulture;
        }

        /// <summary>
        /// Tries to get the flag from the given target object or of one of its parents.
        /// </summary>
        /// <param name="target">The target object for context.</param>
        /// <returns>The flag.</returns>
        public static bool GetDisableCache(DependencyObject target)
        {
            return Instance.DisableCache;
        }

        /// <summary>
        /// Tries to get the flag from the given target object or of one of its parents.
        /// </summary>
        /// <param name="target">The target object for context.</param>
        /// <returns>The flag.</returns>
        public static bool GetOutputMissingKeys(DependencyObject target)
        {
            return Instance.OutputMissingKeys;
        }
#pragma warning restore IDE0060

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> DesignCulture.
        /// Only supported at DesignTime.
        /// If its in Runtime, <see cref="LocalizeDictionary"/>.Culture will be returned.
        /// </summary>
        /// <param name="obj">The dependency object to get the design culture from.</param>
        /// <returns>The design culture at design time or the current culture at runtime.</returns>
        [DesignOnly(true)]
        public static string GetDesignCulture(DependencyObject obj)
        {
            if (Instance.GetIsInDesignMode())
                return obj.GetValueSync<string>(DesignCultureProperty);

            return Instance.Culture.ToString();
        }
        #endregion

        #region Set
        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> Provider.
        /// </summary>
        /// <param name="obj">The dependency object to set the provider to.</param>
        /// <param name="value">The provider.</param>
        public static void SetProvider(DependencyObject obj, ILocalizationProvider value)
        {
            obj.SetValueSync(ProviderProperty, value);
        }

#pragma warning disable IDE0060
        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> DefaultProvider.
        /// </summary>
        /// <param name="obj">The dependency object to set the default provider to.</param>
        /// <param name="value">The default provider.</param>
        public static void SetDefaultProvider(DependencyObject obj, ILocalizationProvider value)
        {
            Instance.DefaultProvider = value;
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> Separation.
        /// </summary>
        /// <param name="obj">The dependency object to set the separation to.</param>
        /// <param name="value">The separation.</param>
        public static void SetSeparation(DependencyObject obj, string value)
        {
            Instance.Separation = value;
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> IncludeInvariantCulture.
        /// </summary>
        /// <param name="obj">The dependency object to set the separation to.</param>
        /// <param name="value">The flag.</param>
        public static void SetIncludeInvariantCulture(DependencyObject obj, bool value)
        {
            Instance.IncludeInvariantCulture = value;
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> DisableCache.
        /// </summary>
        /// <param name="obj">The dependency object to set the separation to.</param>
        /// <param name="value">The flag.</param>
        public static void SetDisableCache(DependencyObject obj, bool value)
        {
            Instance.DisableCache = value;
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> OutputMissingKeys.
        /// </summary>
        /// <param name="obj">The dependency object to set the separation to.</param>
        /// <param name="value">The flag.</param>
        public static void SetOutputMissingKeys(DependencyObject obj, bool value)
        {
            Instance.OutputMissingKeys = value;
        }
#pragma warning restore IDE0060

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> DesignCulture.
        /// Only supported at DesignTime.
        /// </summary>
        /// <param name="obj">The dependency object to set the culture to.</param>
        /// <param name="value">The value.</param>
        [DesignOnly(true)]
        public static void SetDesignCulture(DependencyObject obj, string value)
        {
            if (Instance.GetIsInDesignMode())
                obj.SetValueSync(DesignCultureProperty, value);
        }
        #endregion
        #endregion

        #region Variables
        /// <summary>
        /// Holds a SyncRoot to be thread safe
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Holds the instance of singleton
        /// </summary>
        private static LocalizeDictionary _instance;

        /// <summary>
        /// Holds the current chosen <see cref="CultureInfo"/>
        /// </summary>
        private CultureInfo _culture;

        /// <summary>
        /// Holds the separation char/string.
        /// </summary>
        private string _separation = DefaultSeparation;

        /// <summary>
        /// Determines, if the <see cref="MergedAvailableCultures"/> contains the invariant culture.
        /// </summary>
        private bool _includeInvariantCulture = true;

        /// <summary>
        /// Determines, if the cache is disabled.
        /// </summary>
        private bool _disableCache = true;

        /// <summary>
        /// Determines, if missing keys should be output.
        /// </summary>
        private bool _outputMissingKeys = true;

        /// <summary>
        /// A default provider.
        /// </summary>
        private ILocalizationProvider _defaultProvider;

        /// <summary>
        /// Determines, if the CurrentThread culture is set along with the Culture property.
        /// </summary>
        private bool _setCurrentThreadCulture = true;

        /// <summary>
        /// Determines if the code is run in DesignMode or not.
        /// </summary>
        private bool? _isInDesignMode;

        #endregion

        #region Constructor
        /// <summary>
        /// Prevents a default instance of the <see cref="T:WPFLocalizeExtension.LocalizeDictionary" /> class from being created.
        /// Static Constructor
        /// </summary>
        private LocalizeDictionary()
        {
            DefaultProvider = ResxLocalizationProvider.Instance;
            SetCultureCommand = new CultureInfoDelegateCommand(SetCulture);
        }

        private void AvailableCulturesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action<NotifyCollectionChangedEventArgs>(args =>
            {
                if (args.NewItems != null)
                {
                    foreach (CultureInfo c in args.NewItems)
                    {
                        if (!MergedAvailableCultures.Contains(c))
                            MergedAvailableCultures.Add(c);
                    }
                }

                if (args.OldItems != null)
                {
                    foreach (CultureInfo c in args.OldItems)
                    {
                        if (MergedAvailableCultures.Contains(c))
                            MergedAvailableCultures.Remove(c);
                    }
                }

                if (!_includeInvariantCulture && MergedAvailableCultures.Count > 1 && MergedAvailableCultures.Contains(CultureInfo.InvariantCulture))
                    MergedAvailableCultures.Remove(CultureInfo.InvariantCulture);
            }), e);
        }

        /// <summary>
        /// Destructor code.
        /// </summary>
        ~LocalizeDictionary()
        {
            LocExtension.ClearResourceBuffer();
            FELoc.ClearResourceBuffer();
            BLoc.ClearResourceBuffer();
        }
        #endregion

        #region Static Properties
        /// <summary>
        /// Gets the default <see cref="CultureInfo"/> to initialize the <see cref="LocalizeDictionary"/>.<see cref="CultureInfo"/>
        /// </summary>
        public static CultureInfo DefaultCultureInfo => CultureInfo.InvariantCulture;

        /// <summary>
        /// Gets the default separation char/string.
        /// </summary>
        public static string DefaultSeparation => "_";

        /// <summary>
        /// Gets the <see cref="LocalizeDictionary"/> singleton.
        /// If the underlying instance is null, a instance will be created.
        /// </summary>
        public static LocalizeDictionary Instance
        {
            get
            {
                // check if the underlying instance is null
                if (_instance == null)
                {
                    // if it is null, lock the syncroot.
                    // if another thread is accessing this too,
                    // it have to wait until the syncroot is released
                    lock (SyncRoot)
                    {
                        // check again, if the underlying instance is null
                        if (_instance == null)
                        {
                            // create a new instance
                            _instance = new LocalizeDictionary();
                            _instance.Culture = Thread.CurrentThread.CurrentUICulture;
                            _instance.SetCurrentThreadCulture = true;
                        }
                    }
                }

                // return the existing/new instance
                return _instance;
            }
        }

        /// <summary>
        /// Gets the culture of the singleton instance.
        /// </summary>
        public static CultureInfo CurrentCulture
        {
            get => Instance.Culture;
            set => Instance.Culture = value;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> for localization.
        /// On set, <see cref="DictionaryEvent"/> is raised.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// You have to set <see cref="LocalizeDictionary"/>.Culture first or
        /// wait until System.Windows.Application.Current.MainWindow is created.
        /// Otherwise you will get an Exception.</exception>
        /// <exception cref="System.ArgumentNullException">thrown if Culture will be set to null</exception>
        public CultureInfo Culture
        {
            get
            {
                if (_culture == null)
                    _culture = DefaultCultureInfo;

                return _culture;
            }

            set
            {
                // the cultureinfo cannot contain a null reference
                if (value == null)
                    value = DefaultCultureInfo;

                // Let's see if we already got this culture
                var newCulture = value;

                if (!GetIsInDesignMode())
                {
                    foreach (var c in MergedAvailableCultures)
                        if (c == CultureInfo.InvariantCulture && !IncludeInvariantCulture)
                            continue;
                        else if (c.Name == value.Name)
                        {
                            newCulture = c;
                            break;
                        }
                        else if (c.Parent.Name == value.Name)
                        {
                            // We found a parent culture, but continue - maybe there is a specific one available too.
                            newCulture = c;
                        }
                        else if (value.Parent.Name == c.Name)
                        {
                            // We found a parent culture, but continue - maybe there is a specific one available too.
                            newCulture = value;
                        }
                }

                if (_culture != newCulture)
                {
                    if (newCulture != null && !MergedAvailableCultures.Contains(newCulture))
                        MergedAvailableCultures.Add(newCulture);

                    _culture = newCulture;

                    // Change the CurrentThread culture if needed.
                    if (_setCurrentThreadCulture && !GetIsInDesignMode())
                    {
                        Thread.CurrentThread.CurrentCulture = _culture;
                        Thread.CurrentThread.CurrentUICulture = _culture;
                    }

                    // Raise the OnLocChanged event
                    DictionaryEvent.Invoke(null, new DictionaryEventArgs(DictionaryEventType.CultureChanged, value));

                    RaisePropertyChanged(nameof(Culture));
                }
            }
        }

        /// <summary>
        /// Gets or sets a flag that determines, if the CurrentThread culture should be changed along with the Culture property.
        /// </summary>
        public bool SetCurrentThreadCulture
        {
            get => _setCurrentThreadCulture;
            set
            {
                if (_setCurrentThreadCulture != value)
                {
                    _setCurrentThreadCulture = value;
                    RaisePropertyChanged(nameof(SetCurrentThreadCulture));
                }
            }
        }

        /// <summary>
        /// Gets or sets the flag indicating if the invariant culture is included in the <see cref="MergedAvailableCultures"/> list.
        /// </summary>
        public bool IncludeInvariantCulture
        {
            get => _includeInvariantCulture;
            set
            {
                if (_includeInvariantCulture != value)
                {
                    _includeInvariantCulture = value;

                    var c = CultureInfo.InvariantCulture;
                    var existing = MergedAvailableCultures.Contains(c);

                    if (_includeInvariantCulture && !existing)
                        MergedAvailableCultures.Insert(0, c);
                    else if (!_includeInvariantCulture && existing && MergedAvailableCultures.Count > 1)
                        MergedAvailableCultures.Remove(c);
                }
            }
        }

        /// <summary>
        /// Gets or sets the flag that disables the cache.
        /// </summary>
        public bool DisableCache
        {
            get => _disableCache;
            set => _disableCache = value;
        }

        /// <summary>
        /// Gets or sets the flag that controls the output of missing keys.
        /// </summary>
        public bool OutputMissingKeys
        {
            get => _outputMissingKeys;
            set => _outputMissingKeys = value;
        }

        /// <summary>
        /// The separation char for automatic key retrieval.
        /// </summary>
        public string Separation
        {
            get => _separation;
            set
            {
                _separation = value;
                DictionaryEvent.Invoke(null, new DictionaryEventArgs(DictionaryEventType.SeparationChanged, value));
            }
        }

        /// <summary>
        /// Gets or sets the default <see cref="ILocalizationProvider"/>.
        /// </summary>
        public ILocalizationProvider DefaultProvider
        {
            get => _defaultProvider;
            set
            {
                if (_defaultProvider != value)
                {
                    if (_defaultProvider != null)
                    {
                        _defaultProvider.ProviderChanged -= ProviderUpdated;
                        _defaultProvider.ValueChanged -= ValueChanged;
                        _defaultProvider.AvailableCultures.CollectionChanged -= AvailableCulturesCollectionChanged;
                    }

                    _defaultProvider = value;

                    if (_defaultProvider != null)
                    {
                        _defaultProvider.ProviderChanged += ProviderUpdated;
                        _defaultProvider.ValueChanged += ValueChanged;
                        _defaultProvider.AvailableCultures.CollectionChanged += AvailableCulturesCollectionChanged;

                        foreach (var c in _defaultProvider.AvailableCultures)
                        {
                            if (!MergedAvailableCultures.Contains(c))
                                MergedAvailableCultures.Add(c);
                        }
                    }

                    RaisePropertyChanged(nameof(DefaultProvider));
                }
            }
        }

        private ObservableCollection<CultureInfo> _mergedAvailableCultures;

        /// <summary>
        /// Gets the merged list of all available cultures.
        /// </summary>
        public ObservableCollection<CultureInfo> MergedAvailableCultures
        {
            get
            {
                if (_mergedAvailableCultures == null)
                {
                    _mergedAvailableCultures = new ObservableCollection<CultureInfo> { CultureInfo.InvariantCulture };
                    _mergedAvailableCultures.CollectionChanged += (s, e) => { Culture = Culture; };
                }

                return _mergedAvailableCultures;
            }
        }

        /// <summary>
        /// A command for culture changes.
        /// </summary>
        public ICommand SetCultureCommand { get; }

        /// <summary>
        /// Gets the specific <see cref="CultureInfo"/> of the current culture.
        /// This can be used for format manners.
        /// If the Culture is an invariant <see cref="CultureInfo"/>,
        /// SpecificCulture will also return an invariant <see cref="CultureInfo"/>.
        /// </summary>
        public CultureInfo SpecificCulture => CultureInfo.CreateSpecificCulture(Culture.ToString());

        #endregion

        #region Localization Core
        /// <summary>
        /// Get the localized object using the built-in ResxLocalizationProvider.
        /// </summary>
        /// <param name="source">The source of the dictionary.</param>
        /// <param name="dictionary">The dictionary with key/value pairs.</param>
        /// <param name="key">The key to the value.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The value corresponding to the source/dictionary/key path for the given culture (otherwise NULL).</returns>
        public object GetLocalizedObject(string source, string dictionary, string key, CultureInfo culture)
        {
            return GetLocalizedObject(source + ":" + dictionary + ":" + key, null, culture, DefaultProvider);
        }

        /// <summary>
        /// Get the localized object using the given target for context information.
        /// </summary>
        /// <param name="key">The key to the value.</param>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The value corresponding to the source/dictionary/key path for the given culture (otherwise NULL).</returns>
        public object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture)
        {
            if (DefaultProvider is IInheritingLocalizationProvider)
                return GetLocalizedObject(key, target, culture, DefaultProvider);

            var provider = target?.GetValue(GetProvider);

            if (provider == null)
                provider = DefaultProvider;

            return GetLocalizedObject(key, target, culture, provider);
        }

        /// <summary>
        /// Get the localized object using the given target and provider.
        /// </summary>
        /// <param name="key">The key to the value.</param>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="culture">The culture to use.</param>
        /// <param name="provider">The provider to use.</param>
        /// <returns>The value corresponding to the source/dictionary/key path for the given culture (otherwise NULL).</returns>
        public object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture, ILocalizationProvider provider)
        {
            if (provider == null)
                throw new InvalidOperationException("No provider found and no default provider given.");

            return provider.GetLocalizedObject(key, target, culture);
        }

        /// <summary>
        /// Uses the key and target to build a fully qualified resource key (Assembly, Dictionary, Key)
        /// </summary>
        /// <param name="key">Key used as a base to find the full key</param>
        /// <param name="target">Target used to help determine key information</param>
        /// <returns>Returns an object with all possible pieces of the given key (Assembly, Dictionary, Key)</returns>
        public FullyQualifiedResourceKeyBase GetFullyQualifiedResourceKey(string key, DependencyObject target)
        {
            if (DefaultProvider is IInheritingLocalizationProvider)
                return GetFullyQualifiedResourceKey(key, target, DefaultProvider);

            var provider = target?.GetValue(GetProvider);

            if (provider == null)
                provider = DefaultProvider;

            return GetFullyQualifiedResourceKey(key, target, provider);
        }

        /// <summary>
        /// Uses the key and target to build a fully qualified resource key (Assembly, Dictionary, Key)
        /// </summary>
        /// <param name="key">Key used as a base to find the full key</param>
        /// <param name="target">Target used to help determine key information</param>
        /// <param name="provider">Provider to use</param>
        /// <returns>Returns an object with all possible pieces of the given key (Assembly, Dictionary, Key)</returns>
        public FullyQualifiedResourceKeyBase GetFullyQualifiedResourceKey(string key, DependencyObject target, ILocalizationProvider provider)
        {
            if (provider == null)
                throw new InvalidOperationException("No provider found and no default provider given.");

            return provider.GetFullyQualifiedResourceKey(key, target);
        }

        /// <summary>
        /// Looks up the ResourceManagers for the searched <paramref name="resourceKey"/>
        /// in the <paramref name="resourceDictionary"/> in the <paramref name="resourceAssembly"/>
        /// with an Invariant Culture.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly</param>
        /// <param name="resourceDictionary">The dictionary to look up</param>
        /// <param name="resourceKey">The key of the searched entry</param>
        /// <returns>
        /// TRUE if the searched one is found, otherwise FALSE
        /// </returns>
        public bool ResourceKeyExists(string resourceAssembly, string resourceDictionary, string resourceKey)
        {
            return ResourceKeyExists(resourceAssembly, resourceDictionary, resourceKey, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Looks up the ResourceManagers for the searched <paramref name="resourceKey"/>
        /// in the <paramref name="resourceDictionary"/> in the <paramref name="resourceAssembly"/>
        /// with the passed culture. If the searched one does not exists with the passed culture, is will searched
        /// until the invariant culture is used.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly</param>
        /// <param name="resourceDictionary">The dictionary to look up</param>
        /// <param name="resourceKey">The key of the searched entry</param>
        /// <param name="cultureToUse">The culture to use.</param>
        /// <returns>
        /// TRUE if the searched one is found, otherwise FALSE
        /// </returns>
        public bool ResourceKeyExists(string resourceAssembly, string resourceDictionary, string resourceKey, CultureInfo cultureToUse)
        {
            var provider = ResxLocalizationProvider.Instance;

            return ResourceKeyExists(resourceAssembly + ":" + resourceDictionary + ":" + resourceKey, cultureToUse, provider);
        }

        /// <summary>
        /// Looks up the ResourceManagers for the searched <paramref name="key"/>
        /// with the passed culture. If the searched one does not exists with the passed culture, is will searched
        /// until the invariant culture is used.
        /// </summary>
        /// <param name="key">The key of the searched entry</param>
        /// <param name="cultureToUse">The culture to use.</param>
        /// <param name="provider">The localization provider.</param>
        /// <returns>
        /// TRUE if the searched one is found, otherwise FALSE
        /// </returns>
        public bool ResourceKeyExists(string key, CultureInfo cultureToUse, ILocalizationProvider provider)
        {
            return provider.GetLocalizedObject(key, null, cultureToUse) != null;
        }
        #endregion

        #region Helper Functions
        /// <summary>
        /// Gets the status of the design mode
        /// </summary>
        /// <returns>TRUE if in design mode, else FALSE</returns>
        public bool GetIsInDesignMode()
        {
            lock (SyncRoot)
            {
                if (_isInDesignMode.HasValue)
                    return _isInDesignMode.Value;

                if (Dispatcher?.Thread == null || !Dispatcher.Thread.IsAlive)
                {
                    _isInDesignMode = false;
                    return _isInDesignMode.Value;
                }

                if (!Dispatcher.CheckAccess())
                {
                    try
                    {
                        _isInDesignMode = (bool)Dispatcher.Invoke(DispatcherPriority.Normal, TimeSpan.FromMilliseconds(100), new Func<bool>(GetIsInDesignMode));
                    }
                    catch (Exception)
                    {
                        _isInDesignMode = default(bool);
                    }

                    return _isInDesignMode.Value;
                }
                _isInDesignMode = DesignerProperties.GetIsInDesignMode(this);
                return _isInDesignMode.Value;
            }
        }
        #endregion

        #region MissingKeyEvent (standard event)
        /// <summary>
        /// An event for missing keys.
        /// </summary>
        public event EventHandler<MissingKeyEventArgs> MissingKeyEvent;

        /// <summary>
        /// Triggers a MissingKeyEvent.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="key">The missing key.</param>
        /// <returns>True, if a reload should be performed.</returns>
        internal MissingKeyEventArgs OnNewMissingKeyEvent(object sender, string key)
        {
            var args = new MissingKeyEventArgs(key);
            MissingKeyEvent?.Invoke(sender, args);
            return args;
        }
        #endregion

        #region DictionaryEvent (using weak references)
        internal static class DictionaryEvent
        {
            /// <summary>
            /// The list of listeners
            /// </summary>
            private static readonly ListenersList Listeners = new ListenersList();
            private static readonly object ListenersLock = new object();

            /// <summary>
            /// Fire the event.
            /// </summary>
            /// <param name="sender">The sender of the event.</param>
            /// <param name="args">The event arguments.</param>
            internal static void Invoke(DependencyObject sender, DictionaryEventArgs args)
            {
                lock (ListenersLock)
                {
                    var exceptions = new List<Exception>();

                    foreach (var listener in Listeners.GetListeners())
                    {
                        try
                        {
                            listener.ResourceChanged(sender, args);
                        }
                        catch (Exception e)
                        {
                            exceptions.Add(e);
                        }
                    }

                    if (exceptions.Count > 0)
                        throw new AggregateException(exceptions);
                }
            }

            /// <summary>
            /// Adds a listener to the inner list of listeners.
            /// </summary>
            /// <param name="listener">The listener to add.</param>
            internal static void AddListener(IDictionaryEventListener listener)
            {
                if (listener == null)
                    return;

                lock (ListenersLock)
                {
                    Listeners.AddListener(listener);
                }
            }

            /// <summary>
            /// Removes a listener from the inner list of listeners.
            /// </summary>
            /// <param name="listener">The listener to remove.</param>
            internal static void RemoveListener(IDictionaryEventListener listener)
            {
                if (listener == null)
                    return;

                lock (ListenersLock)
                {
                    Listeners.RemoveListener(listener);
                }
            }

            /// <summary>
            /// Enumerates all listeners of type T.
            /// </summary>
            /// <typeparam name="T">The listener type.</typeparam>
            /// <returns>An enumeration of listeners.</returns>
            internal static IEnumerable<T> EnumerateListeners<T>()
            {
                lock (ListenersLock)
                {
                    foreach (var listener in Listeners.GetListeners().OfType<T>())
                    {
                        yield return listener;
                    }
                }
            }
        }
        #endregion

        #region CultureInfoDelegateCommand
        private void SetCulture(CultureInfo c)
        {
            Culture = c;
        }

        /// <summary>
        /// A class for culture commands.
        /// </summary>
        internal class CultureInfoDelegateCommand : ICommand
        {
            #region Functions for execution and evaluation
            /// <summary>
            /// Predicate that determines if an object can execute
            /// </summary>
            private readonly Predicate<CultureInfo> _canExecute;

            /// <summary>
            /// The action to execute when the command is invoked
            /// </summary>
            private readonly Action<CultureInfo> _execute;
            #endregion

            #region Constructor
            /// <summary>
            /// Initializes a new instance of the <see cref="CultureInfoDelegateCommand"/> class.
            /// Creates a new command that can always execute.
            /// </summary>
            /// <param name="execute">
            /// The execution logic.
            /// </param>
            public CultureInfoDelegateCommand(Action<CultureInfo> execute)
                : this(execute, null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CultureInfoDelegateCommand"/> class.
            /// Creates a new command.
            /// </summary>
            /// <param name="execute">
            /// The execution logic.
            /// </param>
            /// <param name="canExecute">
            /// The execution status logic.
            /// </param>
            public CultureInfoDelegateCommand(Action<CultureInfo> execute, Predicate<CultureInfo> canExecute)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }
            #endregion

            #region ICommand interface
            /// <summary>
            /// Occurs when changes occur that affect whether or not the command should execute.
            /// </summary>
            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            /// <summary>
            /// Determines whether the command can execute in its current state.
            /// </summary>
            /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
            /// <returns>true if this command can be executed; otherwise, false.</returns>
            public bool CanExecute(object parameter)
            {
                return _canExecute == null || _canExecute((CultureInfo)parameter);
            }

            /// <summary>
            /// Is called when the command is invoked.
            /// </summary>
            /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
            public void Execute(object parameter)
            {
                var c = new CultureInfo((string)parameter);
                _execute(c);
            }
            #endregion
        }
        #endregion
    }

    /// <summary>
    /// A singleton RESX provider that uses attached properties and the Parent property to iterate through the visual tree.
    /// </summary>
    public class ResxLocalizationProvider : ResxLocalizationProviderBase
    {
        #region Dependency Properties
        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultDictionary to set the fallback resource dictionary.
        /// </summary>
        public static readonly DependencyProperty DefaultDictionaryProperty =
                DependencyProperty.RegisterAttached(
                "DefaultDictionary",
                typeof(string),
                typeof(ResxLocalizationProvider),
                new PropertyMetadata(null, DefaultDictionaryChanged));

        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultAssembly to set the fallback assembly.
        /// </summary>
        public static readonly DependencyProperty DefaultAssemblyProperty =
            DependencyProperty.RegisterAttached(
                "DefaultAssembly",
                typeof(string),
                typeof(ResxLocalizationProvider),
                new PropertyMetadata(null, DefaultAssemblyChanged));

        /// <summary>
        /// <see cref="DependencyProperty"/> IgnoreCase to set the case sensitivity.
        /// </summary>
        public static readonly DependencyProperty IgnoreCaseProperty =
            DependencyProperty.RegisterAttached(
                "IgnoreCase",
                typeof(bool),
                typeof(ResxLocalizationProvider),
                new PropertyMetadata(true, IgnoreCaseChanged));
        #endregion

        #region Dependency Property Callback
        /// <summary>
        /// Indicates, that the <see cref="DefaultDictionaryProperty"/> attached property changed.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event argument.</param>
        private static void DefaultDictionaryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Instance.FallbackDictionary = e.NewValue?.ToString();
            Instance.OnProviderChanged(obj);
        }

        /// <summary>
        /// Indicates, that the <see cref="DefaultAssemblyProperty"/> attached property changed.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event argument.</param>
        private static void DefaultAssemblyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Instance.FallbackAssembly = e.NewValue?.ToString();
            Instance.OnProviderChanged(obj);
        }

        /// <summary>
        /// Indicates, that the <see cref="IgnoreCaseProperty"/> attached property changed.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event argument.</param>
        private static void IgnoreCaseChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Instance.IgnoreCase = (bool)e.NewValue;
            Instance.OnProviderChanged(obj);
        }

        #endregion

        #region Dependency Property Management
        #region Get
        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> default dictionary.
        /// </summary>
        /// <param name="obj">The dependency object to get the default dictionary from.</param>
        /// <returns>The default dictionary.</returns>
        public static string GetDefaultDictionary(DependencyObject obj)
        {
            return obj.GetValueSync<string>(DefaultDictionaryProperty);
        }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to get the default assembly from.</param>
        /// <returns>The default assembly.</returns>
        public static string GetDefaultAssembly(DependencyObject obj)
        {
            return obj.GetValueSync<string>(DefaultAssemblyProperty);
        }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> ignore case flag.
        /// </summary>
        /// <param name="obj">The dependency object to get the ignore case flag from.</param>
        /// <returns>The ignore case flag.</returns>
        public static bool GetIgnoreCase(DependencyObject obj)
        {
            return obj.GetValueSync<bool>(IgnoreCaseProperty);
        }
        #endregion

        #region Set
        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> default dictionary.
        /// </summary>
        /// <param name="obj">The dependency object to set the default dictionary to.</param>
        /// <param name="value">The dictionary.</param>
        public static void SetDefaultDictionary(DependencyObject obj, string value)
        {
            obj.SetValueSync(DefaultDictionaryProperty, value);
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to set the default assembly to.</param>
        /// <param name="value">The assembly.</param>
        public static void SetDefaultAssembly(DependencyObject obj, string value)
        {
            obj.SetValueSync(DefaultAssemblyProperty, value);
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> ignore case flag.
        /// </summary>
        /// <param name="obj">The dependency object to set the ignore case flag to.</param>
        /// <param name="value">The ignore case flag.</param>
        public static void SetIgnoreCase(DependencyObject obj, bool value)
        {
            obj.SetValueSync(IgnoreCaseProperty, value);
        }
        #endregion
        #endregion

        #region Variables
        /// <summary>
        /// A dictionary for notification classes for changes of the individual target Parent changes.
        /// </summary>
        private readonly ParentNotifiers _parentNotifiers = new ParentNotifiers();

        /// <summary>
        /// To use when no assembly is specified.
        /// </summary>
        public string FallbackAssembly { get; set; }

        /// <summary>
        /// To use when no dictionary is specified.
        /// </summary>
        public string FallbackDictionary { get; set; }
        #endregion

        #region Singleton Variables, Properties & Constructor
        /// <summary>
        /// The instance of the singleton.
        /// </summary>
        private static ResxLocalizationProvider _instance;

        /// <summary>
        /// Lock object for the creation of the singleton instance.
        /// </summary>
        private static readonly object InstanceLock = new object();

        /// <summary>
        /// Gets the <see cref="ResxLocalizationProvider"/> singleton.
        /// </summary>
        public static ResxLocalizationProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (_instance == null)
                            _instance = new ResxLocalizationProvider();
                    }
                }

                // return the existing/new instance
                return _instance;
            }

            set
            {
                lock (InstanceLock)
                {
                    _instance = value;
                }
            }
        }

        /// <summary>
        /// Resets the instance that is used for the ResxLocationProvider
        /// </summary>
        public static void Reset()
        {
            Instance = null;
        }

        /// <summary>
        /// The singleton constructor.
        /// </summary>
        protected ResxLocalizationProvider()
        {
            ResourceManagerList = new Dictionary<string, ResourceManager>();
            AvailableCultures = new ObservableCollection<CultureInfo> { CultureInfo.InvariantCulture };
        }
        #endregion

        #region Abstract assembly & dictionary lookup
        /// <summary>
        /// An action that will be called when a parent of one of the observed target objects changed.
        /// </summary>
        /// <param name="obj">The target <see cref="DependencyObject"/>.</param>
        private void ParentChangedAction(DependencyObject obj)
        {
            OnProviderChanged(obj);
        }

        /// <inheritdoc/>
        protected override string GetAssembly(DependencyObject target)
        {
            if (target == null)
                return FallbackAssembly;

            var assembly = target.GetValueOrRegisterParentNotifier<string>(DefaultAssemblyProperty, ParentChangedAction, _parentNotifiers);
            return string.IsNullOrEmpty(assembly) ? FallbackAssembly : assembly;
        }

        /// <inheritdoc/>
        protected override string GetDictionary(DependencyObject target)
        {
            if (target == null)
                return FallbackDictionary;

            var dictionary = target.GetValueOrRegisterParentNotifier<string>(DefaultDictionaryProperty, ParentChangedAction, _parentNotifiers);
            return string.IsNullOrEmpty(dictionary) ? FallbackDictionary : dictionary;
        }
        #endregion
    }

    /// <summary>
    /// A generic localization extension.
    /// </summary>
    [ContentProperty("ResourceIdentifierKey")]
    public class LocExtension : NestedMarkupExtension, INotifyPropertyChanged, IDictionaryEventListener, IDisposable
    {
        #region PropertyChanged Logic
        /// <summary>
        /// Informiert über sich ändernde Eigenschaften.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify that a property has changed
        /// </summary>
        /// <param name="property">
        /// The property that changed
        /// </param>
        internal void OnNotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion

        #region Variables
        private static readonly object ResourceBufferLock = new object();
        private static readonly object ResolveLock = new object();

        private static Dictionary<string, object> _resourceBuffer = new Dictionary<string, object>();

        /// <summary>
        /// Holds the Key to a .resx object
        /// </summary>
        private string _key;

        /// <summary>
        /// Holds the Binding to get the key
        /// </summary>
        private Binding _binding;

        /// <summary>
        /// the Name of the cached dynamic generated DependencyProperties
        /// </summary>
        private string cacheDPName = null;

        /// <summary>
        /// Cached DependencyProperty for this object
        /// </summary>
        private DependencyProperty cacheDPThis;

        /// <summary>
        /// Cached DependencyProperty for key string
        /// </summary>
        private DependencyProperty cacheDPKey;

        /// <summary>
        /// A custom converter, supplied in the XAML code.
        /// </summary>
        private IValueConverter _converter;

        /// <summary>
        /// A parameter that can be supplied along with the converter object.
        /// </summary>
        private object _converterParameter;

        /// <summary>
        /// The last endpoint that was used for this extension.
        /// </summary>
        private SafeTargetInfo _lastEndpoint;
        #endregion

        #region Resource buffer handling.
        /// <summary>
        /// Clears the common resource buffer.
        /// </summary>
        public static void ClearResourceBuffer()
        {
            lock (ResourceBufferLock)
            {
                _resourceBuffer?.Clear();
                _resourceBuffer = null;
            }
        }

        /// <summary>
        /// Adds an item to the resource buffer (threadsafe).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        internal static void SafeAddItemToResourceBuffer(string key, object item)
        {
            lock (ResourceBufferLock)
            {
                if (!LocalizeDictionary.Instance.DisableCache && !_resourceBuffer.ContainsKey(key))
                    _resourceBuffer.Add(key, item);
            }
        }

        /// <summary>
        /// Removes an item from the resource buffer (threadsafe).
        /// </summary>
        /// <param name="key">The key.</param>
        internal static void SafeRemoveItemFromResourceBuffer(string key)
        {
            lock (ResourceBufferLock)
            {
                if (_resourceBuffer.ContainsKey(key))
                    _resourceBuffer.Remove(key);
            }
        }
        #endregion

        #region GetBoundExtension
        /// <summary>
        /// Gets the extension that is bound to a given target. Please note, that only the last endpoint of each extension can be evaluated.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="property">The target property name.</param>
        /// <param name="propertyIndex">The index in the property (if applicable).</param>
        /// <returns>The bound extension or null, if not available.</returns>
        public static LocExtension GetBoundExtension(object target, string property, int propertyIndex = -1)
        {
            foreach (var ext in LocalizeDictionary.DictionaryEvent.EnumerateListeners<LocExtension>())
            {
                var ep = ext._lastEndpoint;

                if (ep.TargetObjectReference.Target == null)
                    continue;

                var epProp = GetPropertyName(ep.TargetProperty);

                if (ep.TargetObjectReference.Target == target &&
                    epProp == property &&
                    ep.TargetPropertyIndex == propertyIndex)
                    return ext;
            }

            return null;
        }

        /// <summary>
        /// Get the name of a property (regular or DependencyProperty).
        /// </summary>
        /// <param name="property">The property object.</param>
        /// <returns>The name of the property.</returns>
        private static string GetPropertyName(object property)
        {
            var epProp = "";

            if (property is PropertyInfo info)
                epProp = info.Name;
            else if (property is DependencyProperty)
            {
                epProp = ((DependencyProperty)property).Name;
            }

            // What are these names during design time good for? Any suggestions?
            if (epProp.Contains("FrameworkElementWidth5"))
                epProp = "Height";
            else if (epProp.Contains("FrameworkElementWidth6"))
                epProp = "Width";
            else if (epProp.Contains("FrameworkElementMargin12"))
                epProp = "Margin";

            return epProp;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the Key to a .resx object
        /// </summary>
        public string Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    UpdateNewValue();

                    OnNotifyPropertyChanged(nameof(Key));
                }
            }
        }

        /// <summary>
        /// Gets or sets the custom value converter.
        /// </summary>
        public IValueConverter Converter
        {
            get
            {
                if (_converter == null)
                    _converter = new DefaultConverter();

                return _converter;
            }
            set => _converter = value;
        }

        /// <summary>
        /// Gets or sets the converter parameter.
        /// </summary>
        public object ConverterParameter
        {
            get => _converterParameter;
            set => _converterParameter = value;
        }

        /// <summary>
        /// Gets or sets the culture to force a fixed localized object
        /// </summary>
        public string ForceCulture { get; set; }

        /// <summary>
        /// Gets or sets the initialize value.
        /// This is ONLY used to support the localize extension in blend!
        /// </summary>
        /// <value>The initialize value.</value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [ConstructorArgument("key")]
        public object InitializeValue { get; set; }

        /// <summary>
        /// Gets or sets the Key that identifies a resource (Assembly:Dictionary:Key)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object ResourceIdentifierKey
        {
            get => _key ?? "(null)";
            set => _key = value.ToString();
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocExtension"/> class.
        /// </summary>
        public LocExtension()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocExtension"/> class.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        public LocExtension(object key)
            : this()
        {
            if (key is TemplateBindingExpression tbe)
            {
                var newBinding = new Binding();

                var tb = tbe.TemplateBindingExtension;
                newBinding.Converter = tb.Converter;
                newBinding.ConverterParameter = tb.ConverterParameter;
                newBinding.Path = new PropertyPath(tb.Property.Name);
                newBinding.RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent);
                key = newBinding;
            }

            if (key is Binding binding)
                _binding = binding;
            else
                Key = key?.ToString();
        }

        #endregion

        #region OnFirstTargetAdded/OnLastTargetRemoved

        /// <inheritdoc />
        protected override void OnFirstTargetAdded()
        {
            base.OnFirstTargetAdded();

            LocalizeDictionary.DictionaryEvent.AddListener(this);
        }

        /// <inheritdoc />
        protected override void OnLastTargetRemoved()
        {
            base.OnLastTargetRemoved();

            LocalizeDictionary.DictionaryEvent.RemoveListener(this);
        }

        #endregion

        #region IDictionaryEventListener implementation
        /// <summary>
        /// This method is called when the resource somehow changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        public void ResourceChanged(DependencyObject sender, DictionaryEventArgs e)
        {
            ClearItemFromResourceBuffer(e);
            if (sender == null)
            {
                UpdateNewValue();
                return;
            }

            // Update, if this object is in our endpoint list.
            var targetDOs = (from p in GetTargetPropertyPaths()
                             select p.EndPoint.TargetObject as DependencyObject);

            foreach (var dObj in targetDOs)
            {
                if (LocalizeDictionary.Instance.DefaultProvider is IInheritingLocalizationProvider)
                {
                    UpdateNewValue();
                    break;
                }

                var doParent = dObj;
                while (doParent != null)
                {
                    if (sender == doParent)
                    {
                        UpdateNewValue();
                        break;
                    }
                    if (!(doParent is Visual) && !(doParent is Visual3D) && !(doParent is FrameworkContentElement))
                    {
                        UpdateNewValue();
                        break;
                    }
                    try
                    {
                        DependencyObject doParent2;

                        if (doParent is FrameworkContentElement element)
                            doParent2 = element.Parent;
                        else
                            doParent2 = doParent.GetParent(true);

                        if (doParent2 == null && doParent is FrameworkElement)
                            doParent2 = ((FrameworkElement)doParent).Parent;

                        doParent = doParent2;
                    }
                    catch
                    {
                        UpdateNewValue();
                        break;
                    }
                }
            }
        }

        private void ClearItemFromResourceBuffer(DictionaryEventArgs dictionaryEventArgs)
        {
            if (dictionaryEventArgs.Type == DictionaryEventType.ValueChanged && (dictionaryEventArgs.Tag is ValueChangedEventArgs vceArgs))
            {
                string ciName = (vceArgs.Tag as CultureInfo)?.Name;

                lock (ResolveLock)
                {
                    foreach (var key in _resourceBuffer.Keys.ToList())
                    {
                        if (key.EndsWith(vceArgs.Key))
                        {
                            if (ciName == null || key.StartsWith(ciName))
                            {
                                if (_resourceBuffer[key] != vceArgs.Value)
                                    SafeRemoveItemFromResourceBuffer(key);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Forced culture handling
        /// <summary>
        /// If Culture property defines a valid <see cref="CultureInfo"/>, a <see cref="CultureInfo"/> instance will get
        /// created and returned, otherwise <see cref="LocalizeDictionary"/>.Culture will get returned.
        /// </summary>
        /// <returns>The <see cref="CultureInfo"/></returns>
        /// <exception cref="System.ArgumentException">
        /// thrown if the parameter Culture don't defines a valid <see cref="CultureInfo"/>
        /// </exception>
        protected CultureInfo GetForcedCultureOrDefault()
        {
            // define a culture info
            CultureInfo cultureInfo;

            // check if the forced culture is not null or empty
            if (!string.IsNullOrEmpty(ForceCulture))
            {
                // try to create a valid cultureinfo, if defined
                try
                {
                    // try to create a specific culture from the forced one
                    // cultureInfo = CultureInfo.CreateSpecificCulture(this.ForceCulture);
                    cultureInfo = new CultureInfo(ForceCulture);
                }
                catch (ArgumentException ex)
                {
                    // on error, check if designmode is on
                    if (LocalizeDictionary.Instance.GetIsInDesignMode())
                    {
                        // cultureInfo will be set to the current specific culture
                        cultureInfo = LocalizeDictionary.Instance.SpecificCulture;
                    }
                    else
                    {
                        // tell the customer, that the forced culture cannot be converted propperly
                        throw new ArgumentException("Cannot create a CultureInfo with '" + ForceCulture + "'", ex);
                    }
                }
            }
            else
            {
                // take the current specific culture
                cultureInfo = LocalizeDictionary.Instance.SpecificCulture;
            }

            // return the evaluated culture info
            return cultureInfo;
        }
        #endregion

        #region TargetMarkupExtension implementation
        /// <inheritdoc/>
        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            if (_binding != null && endPoint.TargetObject is DependencyObject dpo && endPoint.TargetProperty is DependencyProperty dp)
            {
                try
                {
                    var name = "LocExtension." + dp.OwnerType.FullName + "." + dp.Name;
                    if (endPoint.TargetPropertyIndex != -1)
                        name += $"[{endPoint.TargetPropertyIndex}]";

                    if (name != cacheDPName)
                    {
                        MethodInfo mi = typeof(DependencyProperty).GetMethod("FromName", BindingFlags.Static | BindingFlags.NonPublic);

                        cacheDPThis = mi.Invoke(null, new object[] { name, typeof(LocExtension) }) as DependencyProperty
                            ?? DependencyProperty.RegisterAttached(name, typeof(NestedMarkupExtension), typeof(LocExtension),
                                           new PropertyMetadata(null));

                        cacheDPKey = mi.Invoke(null, new object[] { name + ".Key", typeof(LocExtension) }) as DependencyProperty
                            ?? DependencyProperty.RegisterAttached(name + ".Key", typeof(string), typeof(LocExtension),
                                            new PropertyMetadata("", (d, e) => { (d?.GetValue(cacheDPThis) as LocExtension)?.UpdateNewValue(); }));
                        cacheDPName = name;
                    }

                    if (dpo.GetValue(cacheDPThis) == null)
                    {
                        BindingOperations.SetBinding(dpo, cacheDPKey, _binding);
                        dpo.SetValue(cacheDPThis, this);
                    }

                    _key = (string)dpo.GetValue(cacheDPKey);
                }
                catch
                {
                }
            }

            object result = null;

            if (endPoint == null)
                return null;
            else
                _lastEndpoint = SafeTargetInfo.FromTargetInfo(endPoint);

            var targetObject = endPoint.TargetObject as DependencyObject;

            // Get target type. Change ImageSource to BitmapSource in order to use our own converter.
            var targetType = info.TargetPropertyType;

            if (targetType == typeof(ImageSource))
                targetType = typeof(BitmapSource);

            // In case of a list target, get the correct list element type.
            if ((info.TargetPropertyIndex != -1) && typeof(IList).IsAssignableFrom(info.TargetPropertyType))
                targetType = info.TargetPropertyType.GetGenericArguments()[0];

            // Try to get the localized input from the resource.
            var resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(Key, targetObject);
            var ci = GetForcedCultureOrDefault();

            // Extract the names of the endpoint object and property
            var epProp = GetPropertyName(endPoint.TargetProperty);
            var epName = "";

            if (endPoint.TargetObject is FrameworkElement)
                epName = ((FrameworkElement)endPoint.TargetObject).GetValueSync<string>(FrameworkElement.NameProperty);
            else if (endPoint.TargetObject is FrameworkContentElement)
                epName = ((FrameworkContentElement)endPoint.TargetObject).GetValueSync<string>(FrameworkContentElement.NameProperty);

            var resKeyBase = ci.Name + ":" + targetType.Name + ":";
            // Check, if the key is already in our resource buffer.
            object input = null;
            var isDefaultConverter = Converter is DefaultConverter;

            if (!String.IsNullOrEmpty(resourceKey))
            {
                // We've got a resource key. Try to look it up or get it from the dictionary.
                lock (ResourceBufferLock)
                {
                    if (isDefaultConverter && _resourceBuffer.ContainsKey(resKeyBase + resourceKey))
                        result = _resourceBuffer[resKeyBase + resourceKey];
                    else
                    {
                        input = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, targetObject, ci);
                        resKeyBase += resourceKey;
                    }
                }
            }
            else
            {
                var resKeyNameProp = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(epName + LocalizeDictionary.GetSeparation(targetObject) + epProp, targetObject);

                // Try the automatic lookup function.
                // First, look for a resource entry named: [FrameworkElement name][Separator][Property name]
                lock (ResourceBufferLock)
                {
                    if (isDefaultConverter && _resourceBuffer.ContainsKey(resKeyBase + resKeyNameProp))
                        result = _resourceBuffer[resKeyBase + resKeyNameProp];
                    else
                    {
                        // It was not stored in the buffer - try to retrieve it from the dictionary.
                        input = LocalizeDictionary.Instance.GetLocalizedObject(resKeyNameProp, targetObject, ci);

                        if (input == null)
                        {
                            var resKeyName = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(epName, targetObject);

                            // Now, try to look for a resource entry named: [FrameworkElement name]
                            // Note - this has to be nested here, as it would take precedence over the first step in the buffer lookup step.
                            if (isDefaultConverter && _resourceBuffer.ContainsKey(resKeyBase + resKeyName))
                                result = _resourceBuffer[resKeyBase + resKeyName];
                            else
                            {
                                input = LocalizeDictionary.Instance.GetLocalizedObject(resKeyName, targetObject, ci);
                                resKeyBase += resKeyName;
                            }
                        }
                        else
                            resKeyBase += resKeyNameProp;
                    }
                }
            }

            // If no result was found, convert the input and add it to the buffer.
            if (result == null)
            {
                if (input != null)
                {
                    result = Converter.Convert(input, targetType, ConverterParameter, ci);
                    if (isDefaultConverter)
                        SafeAddItemToResourceBuffer(resKeyBase, result);
                }
                else
                {
                    var missingKeyEventResult = LocalizeDictionary.Instance.OnNewMissingKeyEvent(this, _key);

                    if (missingKeyEventResult.Reload)
                        UpdateNewValue();

                    if (LocalizeDictionary.Instance.OutputMissingKeys
                        && !string.IsNullOrEmpty(_key) && (targetType == typeof(String) || targetType == typeof(object)))
                    {
                        if (missingKeyEventResult.MissingKeyResult != null)
                            result = missingKeyEventResult.MissingKeyResult;
                        else
                            result = "Key: " + _key;
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        protected override bool UpdateOnEndpoint(TargetInfo endpoint)
        {
            // This extension must be updated, when an endpoint is reached.
            return true;
        }
        #endregion

        #region Resolve functions
        /// <summary>
        /// Gets a localized value.
        /// </summary>
        /// <typeparam name="TValue">The type of the returned value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="converter">An optional converter.</param>
        /// <param name="converterParameter">An optional converter parameter.</param>
        /// <returns>The resolved localized object.</returns>
        public static TValue GetLocalizedValue<TValue>(string key, IValueConverter converter = null, object converterParameter = null)
        {
            var targetCulture = LocalizeDictionary.Instance.SpecificCulture;
            return GetLocalizedValue<TValue>(key, targetCulture, null, converter, converterParameter);
        }

        /// <summary>
        /// Gets a localized value.
        /// </summary>
        /// <typeparam name="TValue">The type of the returned value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="targetCulture">The target culture.</param>
        /// <param name="converter">An optional converter.</param>
        /// <param name="converterParameter">An optional converter parameter.</param>
        /// <returns>The resolved localized object.</returns>
        public static TValue GetLocalizedValue<TValue>(string key, CultureInfo targetCulture, IValueConverter converter = null, object converterParameter = null)
        {
            return GetLocalizedValue<TValue>(key, targetCulture, null, converter, converterParameter);
        }

        /// <summary>
        /// Gets a localized value.
        /// </summary>
        /// <typeparam name="TValue">The type of the returned value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="converter">An optional converter.</param>
        /// <param name="converterParameter">An optional converter parameter.</param>
        /// <returns>The resolved localized object.</returns>
        public static TValue GetLocalizedValue<TValue>(string key, DependencyObject target, IValueConverter converter = null, object converterParameter = null)
        {
            var targetCulture = LocalizeDictionary.Instance.SpecificCulture;
            return GetLocalizedValue<TValue>(key, targetCulture, target, converter, converterParameter);
        }

        /// <summary>
        /// Gets a localized value.
        /// </summary>
        /// <typeparam name="TValue">The type of the returned value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="targetCulture">The target culture.</param>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="converter">An optional converter.</param>
        /// <param name="converterParameter">An optional converter parameter.</param>
        /// <returns>The resolved localized object.</returns>
        public static TValue GetLocalizedValue<TValue>(string key, CultureInfo targetCulture, DependencyObject target, IValueConverter converter = null, object converterParameter = null)
        {
            lock (ResolveLock)
            {
                var result = default(TValue);

                var resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(key, target);

                // Get the localized object from the dictionary
                var resKey = targetCulture.Name + ":" + typeof(TValue).Name + ":" + resourceKey;
                var isDefaultConverter = converter is DefaultConverter;

                if (isDefaultConverter && _resourceBuffer.ContainsKey(resKey))
                    result = (TValue)_resourceBuffer[resKey];
                else
                {
                    var localizedObject = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, target,
                        targetCulture);

                    if (localizedObject == null)
                        return result;

                    if (converter == null)
                        converter = new DefaultConverter();

                    var tmp = converter.Convert(localizedObject, typeof(TValue), converterParameter, targetCulture);

                    if (tmp is TValue value)
                    {
                        result = value;
                        if (isDefaultConverter)
                            SafeAddItemToResourceBuffer(resKey, result);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets a localized value.
        /// </summary>
        /// <param name="t">The type of the returned value.</param>
        /// <param name="key">The key.</param>
        /// <param name="targetCulture">The target culture.</param>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="converter">An optional converter.</param>
        /// <param name="converterParameter">An optional converter parameter.</param>
        /// <returns>The resolved localized object.</returns>
        public static object GetLocalizedValue(Type t, string key, CultureInfo targetCulture, DependencyObject target, IValueConverter converter = null, object converterParameter = null)
        {
            lock (ResolveLock)
            {
                object result = null;

                var resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(key, target);

                // Get the localized object from the dictionary
                var resKey = targetCulture.Name + ":" + t.Name + ":" + resourceKey;
                var isDefaultConverter = converter is DefaultConverter;

                if (isDefaultConverter && _resourceBuffer.ContainsKey(resKey))
                    result = _resourceBuffer[resKey];
                else
                {
                    var localizedObject = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, target,
                        targetCulture);

                    if (localizedObject == null)
                        return result;

                    if (converter == null)
                        converter = new DefaultConverter();

                    var tmp = converter.Convert(localizedObject, t, converterParameter, targetCulture);

                    if (t.IsAssignableFrom(tmp.GetType()))
                    {
                        result = tmp;
                        if (isDefaultConverter)
                            SafeAddItemToResourceBuffer(resKey, result);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Resolves the localized value of the current Assembly, Dict, Key pair.
        /// </summary>
        /// <param name="resolvedValue">The resolved value.</param>
        /// <typeparam name="TValue">The type of the return value.</typeparam>
        /// <returns>
        /// True if the resolve was success, otherwise false.
        /// </returns>
        public bool ResolveLocalizedValue<TValue>(out TValue resolvedValue)
        {
            // return the resolved localized value with the current or forced culture.
            return ResolveLocalizedValue(out resolvedValue, GetForcedCultureOrDefault(), null);
        }

        /// <summary>
        /// Resolves the localized value of the current Assembly, Dict, Key pair and the given target.
        /// </summary>
        /// <param name="resolvedValue">The resolved value.</param>
        /// <typeparam name="TValue">The type of the return value.</typeparam>
        /// <param name="target">The target object.</param>
        /// <returns>
        /// True if the resolve was success, otherwise false.
        /// </returns>
        public bool ResolveLocalizedValue<TValue>(out TValue resolvedValue, DependencyObject target)
        {
            // return the resolved localized value with the current or forced culture.
            return ResolveLocalizedValue(out resolvedValue, GetForcedCultureOrDefault(), target);
        }

        /// <summary>
        /// Resolves the localized value of the current Assembly, Dict, Key pair.
        /// </summary>
        /// <param name="resolvedValue">The resolved value.</param>
        /// <param name="targetCulture">The target culture.</param>
        /// <typeparam name="TValue">The type of the return value.</typeparam>
        /// <returns>
        /// True if the resolve was success, otherwise false.
        /// </returns>
        public bool ResolveLocalizedValue<TValue>(out TValue resolvedValue, CultureInfo targetCulture)
        {
            return ResolveLocalizedValue(out resolvedValue, targetCulture, null);
        }

        /// <summary>
        /// Resolves the localized value of the current Assembly, Dict, Key pair and the given target.
        /// </summary>
        /// <param name="resolvedValue">The resolved value.</param>
        /// <param name="targetCulture">The target culture.</param>
        /// <param name="target">The target object.</param>
        /// <typeparam name="TValue">The type of the return value.</typeparam>
        /// <returns>
        /// True if the resolve was success, otherwise false.
        /// </returns>
        public bool ResolveLocalizedValue<TValue>(out TValue resolvedValue, CultureInfo targetCulture, DependencyObject target)
        {
            // define the default value of the resolved value
            resolvedValue = default;

            var resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(Key, target);

            // get the localized object from the dictionary
            var resKey = targetCulture.Name + ":" + typeof(TValue).Name + ":" + resourceKey;
            var isDefaultConverter = Converter is DefaultConverter;

            lock (ResourceBufferLock)
            {
                if (isDefaultConverter && _resourceBuffer.ContainsKey(resKey))
                {
                    resolvedValue = (TValue)_resourceBuffer[resKey];
                }
                else
                {
                    var localizedObject = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, target, targetCulture);

                    if (localizedObject == null)
                        return false;

                    var result = Converter.Convert(localizedObject, typeof(TValue), ConverterParameter, targetCulture);

                    if (result is TValue value)
                    {
                        resolvedValue = value;
                        if (isDefaultConverter)
                            SafeAddItemToResourceBuffer(resKey, resolvedValue);
                    }
                }
            }

            if (resolvedValue != null)
                return true;

            return false;
        }
        #endregion

        #region Code-behind binding
        /// <summary>
        /// Sets a binding between a <see cref="DependencyObject"/> with its <see cref="DependencyProperty"/>
        /// or <see cref="PropertyInfo"/> and the <c>LocExtension</c>.
        /// </summary>
        /// <param name="targetObject">The target dependency object</param>
        /// <param name="targetProperty">The target property</param>
        /// <returns>
        /// TRUE if the binding was setup successfully, otherwise FALSE (Binding already exists).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the <paramref name="targetProperty"/> is
        /// not a <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>.
        /// </exception>
        public bool SetBinding(DependencyObject targetObject, object targetProperty)
        {
            return SetBinding((object)targetObject, targetProperty, -1);
        }

        /// <summary>
        /// Sets a binding between a <see cref="DependencyObject"/> with its <see cref="DependencyProperty"/>
        /// or <see cref="PropertyInfo"/> and the <c>LocExtension</c>.
        /// </summary>
        /// <param name="targetObject">The target object</param>
        /// <param name="targetProperty">The target property</param>
        /// <returns>
        /// TRUE if the binding was setup successfully, otherwise FALSE (Binding already exists).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the <paramref name="targetProperty"/> is
        /// not a <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>.
        /// </exception>
        public bool SetBinding(object targetObject, object targetProperty)
        {
            return SetBinding(targetObject, targetProperty, -1);
        }

        /// <summary>
        /// Sets a binding between a <see cref="DependencyObject"/> with its <see cref="DependencyProperty"/>
        /// or <see cref="PropertyInfo"/> and the <c>LocExtension</c>.
        /// </summary>
        /// <param name="targetObject">The target dependency object</param>
        /// <param name="targetProperty">The target property</param>
        /// <param name="targetPropertyIndex">The index of the target property. (only used for Lists)</param>
        /// <returns>
        /// TRUE if the binding was setup successfully, otherwise FALSE (Binding already exists).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the <paramref name="targetProperty"/> is
        /// not a <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>.
        /// </exception>
        public bool SetBinding(DependencyObject targetObject, object targetProperty, int targetPropertyIndex)
        {
            return SetBinding((object)targetObject, targetProperty, targetPropertyIndex);
        }

        /// <summary>
        /// Sets a binding between a <see cref="DependencyObject"/> with its <see cref="DependencyProperty"/>
        /// or <see cref="PropertyInfo"/> and the <c>LocExtension</c>.
        /// </summary>
        /// <param name="targetObject">The target object</param>
        /// <param name="targetProperty">The target property</param>
        /// <param name="targetPropertyIndex">The index of the target property. (only used for Lists)</param>
        /// <returns>
        /// TRUE if the binding was setup successfully, otherwise FALSE (Binding already exists).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the <paramref name="targetProperty"/> is
        /// not a <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>.
        /// </exception>
        public bool SetBinding(object targetObject, object targetProperty, int targetPropertyIndex)
        {
            var existingBinding = (from info in GetTargetPropertyPaths()
                                   where (info.EndPoint.TargetObject == targetObject) && (info.EndPoint.TargetProperty == targetProperty)
                                   select info).FirstOrDefault();

            // Return false, if the binding already exists
            if (existingBinding != null)
                return false;

            Type targetPropertyType = null;

            if (targetProperty is DependencyProperty)
                targetPropertyType = ((DependencyProperty)targetProperty).PropertyType;
            else if (targetProperty is PropertyInfo)
                targetPropertyType = ((PropertyInfo)targetProperty).PropertyType;

            var result = ProvideValue(new SimpleProvideValueServiceProvider(targetObject, targetProperty, targetPropertyType, targetPropertyIndex));

            SetPropertyValue(result, new TargetInfo(targetObject, targetProperty, targetPropertyType, targetPropertyIndex), false);

            return true;
        }
        #endregion

        #region ToString
        /// <summary>
        /// Overridden, to return the key of this instance.
        /// </summary>
        /// <returns>Loc: + key</returns>
        public override string ToString()
        {
            return "Loc:" + _key;
        }
        #endregion
    }
}

#region WPFLocalizeExtension

#region Copyright information
// <copyright file="BitmapSourceTypeConverter.cs">
//     Licensed under Microsoft Public License (Ms-PL)
//     https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension/blob/master/LICENSE
// </copyright>
// <author>Bernhard Millauer</author>
// <author>Uwe Mayer</author>
#endregion

namespace WPFLocalizeExtension
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using DeepWise.Localization;
    #endregion

    /// <summary>
    /// A localization extension based on <see cref="Binding"/>.
    /// </summary>
    public class BLoc : Binding, INotifyPropertyChanged, IDictionaryEventListener, IDisposable
    {
        #region PropertyChanged Logic
        /// <summary>
        /// Informiert über sich ändernde Eigenschaften.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify that a property has changed
        /// </summary>
        /// <param name="property">
        /// The property that changed
        /// </param>
        internal void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion

        #region Variables & Properties
        private static readonly object ResourceBufferLock = new object();
        private static Dictionary<string, object> _resourceBuffer = new Dictionary<string, object>();

        private object _value;
        /// <summary>
        /// The value, the internal binding is pointing at.
        /// </summary>
        public object Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    RaisePropertyChanged(nameof(Value));
                }
            }
        }

        private string _key;
        /// <summary>
        /// Gets or sets the Key to a .resx object
        /// </summary>
        public string Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    UpdateNewValue();
                    RaisePropertyChanged(nameof(Key));
                }
            }
        }

        /// <summary>
        /// Gets or sets the culture to force a fixed localized object
        /// </summary>
        public string ForceCulture { get; set; }
        #endregion

        #region Resource buffer handling.
        /// <summary>
        /// Clears the common resource buffer.
        /// </summary>
        public static void ClearResourceBuffer()
        {
            lock (ResourceBufferLock)
            {
                _resourceBuffer?.Clear();
                _resourceBuffer = null;
            }
        }

        /// <summary>
        /// Adds an item to the resource buffer (threadsafe).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        internal static void SafeAddItemToResourceBuffer(string key, object item)
        {
            lock (ResourceBufferLock)
            {
                if (!LocalizeDictionary.Instance.DisableCache && !_resourceBuffer.ContainsKey(key))
                    _resourceBuffer.Add(key, item);
            }
        }

        /// <summary>
        /// Removes an item from the resource buffer (threadsafe).
        /// </summary>
        /// <param name="key">The key.</param>
        internal static void SafeRemoveItemFromResourceBuffer(string key)
        {
            lock (ResourceBufferLock)
            {
                if (_resourceBuffer.ContainsKey(key))
                    _resourceBuffer.Remove(key);
            }
        }
        #endregion

        #region Block some Binding Parameters
        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new PropertyPath Path
        {
            get { return null; }
            set
            {
                throw new Exception(nameof(Path) + " not allowed for BLoc");
            }
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new object Source
        {
            get => null;
            set => throw new Exception(nameof(Source) + " not allowed for BLoc");
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new RelativeSource RelativeSource
        {
            get => null;
            set => throw new Exception(nameof(RelativeSource) + " not allowed for BLoc");
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string ElementName
        {
            get => null;
            set => throw new Exception(nameof(ElementName) + " not allowed for BLoc");
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new BindingMode Mode
        {
            get => BindingMode.Default;
            set => throw new Exception(nameof(Mode) + " not allowed for BLoc");
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string XPath
        {
            get => null;
            set => throw new Exception(nameof(XPath) + " not allowed for BLoc");
        }
        #endregion

        #region Constructors & Dispose
        /// <summary>
        /// Initializes a new instance of the <see cref="BLoc"/> class.
        /// </summary>
        public BLoc()
        {
            LocalizeDictionary.DictionaryEvent.AddListener(this);
            base.Path = new PropertyPath("Value");
            base.Source = this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BLoc"/> class.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        public BLoc(string key)
            : this()
        {
            Key = key;
        }

        /// <summary>
        /// Removes the listener from the dictionary.
        /// </summary>
        public void Dispose()
        {
            LocalizeDictionary.DictionaryEvent.RemoveListener(this);
        }

        /// <summary>
        /// The finalizer.
        /// </summary>
        ~BLoc()
        {
            Dispose();
        }
        #endregion

        #region Forced culture handling
        /// <summary>
        /// If Culture property defines a valid <see cref="CultureInfo"/>, a <see cref="CultureInfo"/> instance will get
        /// created and returned, otherwise <see cref="LocalizeDictionary"/>.Culture will get returned.
        /// </summary>
        /// <returns>The <see cref="CultureInfo"/></returns>
        /// <exception cref="System.ArgumentException">
        /// thrown if the parameter Culture don't defines a valid <see cref="CultureInfo"/>
        /// </exception>
        protected CultureInfo GetForcedCultureOrDefault()
        {
            // define a culture info
            CultureInfo cultureInfo;

            // check if the forced culture is not null or empty
            if (!string.IsNullOrEmpty(ForceCulture))
            {
                // try to create a valid cultureinfo, if defined
                try
                {
                    // try to create a specific culture from the forced one
                    // cultureInfo = CultureInfo.CreateSpecificCulture(this.ForceCulture);
                    cultureInfo = new CultureInfo(ForceCulture);
                }
                catch (ArgumentException ex)
                {
                    // on error, check if designmode is on
                    if (LocalizeDictionary.Instance.GetIsInDesignMode())
                    {
                        // cultureInfo will be set to the current specific culture
                        cultureInfo = LocalizeDictionary.Instance.SpecificCulture;
                    }
                    else
                    {
                        // tell the customer, that the forced culture cannot be converted propperly
                        throw new ArgumentException("Cannot create a CultureInfo with '" + ForceCulture + "'", ex);
                    }
                }
            }
            else
            {
                // take the current specific culture
                cultureInfo = LocalizeDictionary.Instance.SpecificCulture;
            }

            // return the evaluated culture info
            return cultureInfo;
        }
        #endregion

        /// <summary>
        /// This method is called when the resource somehow changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        public void ResourceChanged(DependencyObject sender, DictionaryEventArgs e)
        {
            UpdateNewValue();
        }

        private void UpdateNewValue()
        {
            Value = FormatOutput();
        }

        #region Future TargetMarkupExtension implementation
        /// <summary>
        /// This function returns the properly prepared output of the markup extension.
        /// </summary>
        public object FormatOutput()
        {
            object result = null;

            // Try to get the localized input from the resource.
            string resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(Key, null);

            var ci = GetForcedCultureOrDefault();

            var key = ci.Name + ":";

            // Check, if the key is already in our resource buffer.
            lock (ResourceBufferLock)
            {
                if (_resourceBuffer.ContainsKey(key + resourceKey))
                    result = _resourceBuffer[key + resourceKey];
            }

            if (result == null)
            {
                result = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, null, ci);

                if (result == null)
                {
                    var missingKeyEventResult = LocalizeDictionary.Instance.OnNewMissingKeyEvent(this, resourceKey);

                    if (missingKeyEventResult.Reload)
                        UpdateNewValue();

                    if (LocalizeDictionary.Instance.OutputMissingKeys
                        && !string.IsNullOrEmpty(_key))
                    {
                        if (missingKeyEventResult.MissingKeyResult != null)
                            result = missingKeyEventResult.MissingKeyResult;
                        else
                            result = "Key: " + _key;
                    }
                }
                else
                {
                    key += resourceKey;
                    SafeAddItemToResourceBuffer(key, result);
                }
            }

            return result;
        }
        #endregion
    }
}

namespace WPFLocalizeExtension
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup.Primitives;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using WPFLocalizeExtension;
    using WPFLocalizeExtension.TypeConverters;
    using XAMLMarkupExtensions.Base;
    using DeepWise.Localization;
    #endregion

    /// <summary>
    /// A localization utility based on <see cref="FrameworkElement"/>.
    /// </summary>
    public class FELoc : FrameworkElement, IDictionaryEventListener, INotifyPropertyChanged, IDisposable
    {
        #region PropertyChanged Logic
        /// <summary>
        /// Informiert über sich ändernde Eigenschaften.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify that a property has changed
        /// </summary>
        /// <param name="property">
        /// The property that changed
        /// </param>
        internal void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion

        #region Private variables
        private static readonly object ResourceBufferLock = new object();
        private static Dictionary<string, object> _resourceBuffer = new Dictionary<string, object>();

        private ParentChangedNotifier _parentChangedNotifier;
        private TargetInfo _targetInfo;
        #endregion

        #region Resource buffer handling.
        /// <summary>
        /// Clears the common resource buffer.
        /// </summary>
        public static void ClearResourceBuffer()
        {
            lock (ResourceBufferLock)
            {
                _resourceBuffer?.Clear();
                _resourceBuffer = null;
            }
        }

        /// <summary>
        /// Adds an item to the resource buffer (threadsafe).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        internal static void SafeAddItemToResourceBuffer(string key, object item)
        {
            lock (ResourceBufferLock)
            {
                if (!LocalizeDictionary.Instance.DisableCache && !_resourceBuffer.ContainsKey(key))
                    _resourceBuffer.Add(key, item);
            }
        }

        /// <summary>
        /// Removes an item from the resource buffer (threadsafe).
        /// </summary>
        /// <param name="key">The key.</param>
        internal static void SafeRemoveItemFromResourceBuffer(string key)
        {
            lock (ResourceBufferLock)
            {
                if (_resourceBuffer.ContainsKey(key))
                    _resourceBuffer.Remove(key);
            }
        }
        #endregion

        #region DependencyProperty: Key
        /// <summary>
        /// <see cref="DependencyProperty"/> Key to set the resource key.
        /// </summary>
        public static readonly DependencyProperty KeyProperty =
                DependencyProperty.Register(
                "Key",
                typeof(string),
                typeof(FELoc),
                new PropertyMetadata(null, DependencyPropertyChanged));

        /// <summary>
        /// The resource key.
        /// </summary>
        public string Key
        {
            get => this.GetValueSync<string>(KeyProperty);
            set => this.SetValueSync(KeyProperty, value);
        }
        #endregion

        #region DependencyProperty: Converter
        /// <summary>
        /// <see cref="DependencyProperty"/> Converter to set the <see cref="IValueConverter"/> used to adapt to the target.
        /// </summary>
        public static readonly DependencyProperty ConverterProperty =
                DependencyProperty.Register(
                "Converter",
                typeof(IValueConverter),
                typeof(FELoc),
                new PropertyMetadata(new DefaultConverter(), DependencyPropertyChanged));

        /// <summary>
        /// Gets or sets the custom value converter.
        /// </summary>
        public IValueConverter Converter
        {
            get => this.GetValueSync<IValueConverter>(ConverterProperty);
            set => this.SetValueSync(ConverterProperty, value);
        }
        #endregion

        #region DependencyProperty: ConverterParameter
        /// <summary>
        /// <see cref="DependencyProperty"/> ConverterParameter.
        /// </summary>
        public static readonly DependencyProperty ConverterParameterProperty =
                DependencyProperty.Register(
                "ConverterParameter",
                typeof(object),
                typeof(FELoc),
                new PropertyMetadata(null, DependencyPropertyChanged));

        /// <summary>
        /// Gets or sets the converter parameter.
        /// </summary>
        public object ConverterParameter
        {
            get => this.GetValueSync<object>(ConverterParameterProperty);
            set => this.SetValueSync(ConverterParameterProperty, value);
        }
        #endregion

        #region DependencyProperty: ForceCulture
        /// <summary>
        /// <see cref="DependencyProperty"/> ForceCulture.
        /// </summary>
        public static readonly DependencyProperty ForceCultureProperty =
                DependencyProperty.Register(
                "ForceCulture",
                typeof(string),
                typeof(FELoc),
                new PropertyMetadata(null, DependencyPropertyChanged));

        /// <summary>
        /// Gets or sets the forced culture.
        /// </summary>
        public string ForceCulture
        {
            get => this.GetValueSync<string>(ForceCultureProperty);
            set => this.SetValueSync(ForceCultureProperty, value);
        }
        #endregion

        #region DependencyProperty: Content - used for value transfer only!
        ///// <summary>
        ///// <see cref="DependencyProperty"/> ForceCulture.
        ///// </summary>
        //public static readonly DependencyProperty ContentProperty =
        //        DependencyProperty.Register(
        //        "Content",
        //        typeof(object),
        //        typeof(FELoc));

        ///// <summary>
        ///// Gets or sets the content.
        ///// </summary>
        //public object Content
        //{
        //    get { return GetValue(ContentProperty); }
        //    set { SetValue(ContentProperty, value); }
        //}

        private object _content;
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public object Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    RaisePropertyChanged(nameof(Content));
                }
            }
        }
        #endregion

        /// <summary>
        /// Indicates, that the key changed.
        /// </summary>
        /// <param name="obj">The FELoc object.</param>
        /// <param name="args">The event argument.</param>
        private static void DependencyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is FELoc loc)
                loc.UpdateNewValue();
        }

        #region Parent changed event
        /// <summary>
        /// Based on http://social.msdn.microsoft.com/Forums/en/wpf/thread/580234cb-e870-4af1-9a91-3e3ba118c89c
        /// </summary>
        /// <param name="element">The target object.</param>
        /// <returns>The list of DependencyProperties of the object.</returns>
        private IEnumerable<DependencyProperty> GetDependencyProperties(object element)
        {
            var properties = new List<DependencyProperty>();
            var markupObject = MarkupWriter.GetMarkupObjectFor(element);

            foreach (var mp in markupObject.Properties)
                if (mp.DependencyProperty != null)
                    properties.Add(mp.DependencyProperty);

            return properties;
        }

        private void RegisterParentNotifier()
        {
            _parentChangedNotifier = new ParentChangedNotifier(this, () =>
            {
                _parentChangedNotifier.Dispose();
                _parentChangedNotifier = null;
                var targetObject = Parent;
                if (targetObject != null)
                {
                    var properties = GetDependencyProperties(targetObject);
                    foreach (var p in properties)
                    {
                        if (targetObject.GetValue(p) == this)
                        {
                            _targetInfo = new TargetInfo(targetObject, p, p.PropertyType, -1);

                            var binding = new Binding("Content")
                            {
                                Source = this,
                                Converter = Converter,
                                ConverterParameter = ConverterParameter,
                                Mode = BindingMode.OneWay
                            };

                            BindingOperations.SetBinding(targetObject, p, binding);
                            UpdateNewValue();
                        }
                    }
                }
            });
        }
        #endregion

        #region Constructors & Dispose
        /// <summary>
        /// Initializes a new instance of the <see cref="BLoc"/> class.
        /// </summary>
        public FELoc()
        {
            LocalizeDictionary.DictionaryEvent.AddListener(this);
            RegisterParentNotifier();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BLoc"/> class.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        public FELoc(string key)
            : this()
        {
            Key = key;
        }

        /// <summary>
        /// Removes the listener from the dictionary.
        /// </summary>
        public void Dispose()
        {
            LocalizeDictionary.DictionaryEvent.RemoveListener(this);
        }

        /// <summary>
        /// The finalizer.
        /// </summary>
        ~FELoc()
        {
            Dispose();
        }
        #endregion

        #region Forced culture handling
        /// <summary>
        /// If Culture property defines a valid <see cref="CultureInfo"/>, a <see cref="CultureInfo"/> instance will get
        /// created and returned, otherwise <see cref="LocalizeDictionary"/>.Culture will get returned.
        /// </summary>
        /// <returns>The <see cref="CultureInfo"/></returns>
        /// <exception cref="System.ArgumentException">
        /// thrown if the parameter Culture don't defines a valid <see cref="CultureInfo"/>
        /// </exception>
        protected CultureInfo GetForcedCultureOrDefault()
        {
            // define a culture info
            CultureInfo cultureInfo;

            // check if the forced culture is not null or empty
            if (!string.IsNullOrEmpty(ForceCulture))
            {
                // try to create a valid cultureinfo, if defined
                try
                {
                    // try to create a specific culture from the forced one
                    // cultureInfo = CultureInfo.CreateSpecificCulture(this.ForceCulture);
                    cultureInfo = new CultureInfo(ForceCulture);
                }
                catch (ArgumentException ex)
                {
                    // on error, check if designmode is on
                    if (LocalizeDictionary.Instance.GetIsInDesignMode())
                    {
                        // cultureInfo will be set to the current specific culture
                        cultureInfo = LocalizeDictionary.Instance.SpecificCulture;
                    }
                    else
                    {
                        // tell the customer, that the forced culture cannot be converted propperly
                        throw new ArgumentException("Cannot create a CultureInfo with '" + ForceCulture + "'", ex);
                    }
                }
            }
            else
            {
                // take the current specific culture
                cultureInfo = LocalizeDictionary.Instance.SpecificCulture;
            }

            // return the evaluated culture info
            return cultureInfo;
        }
        #endregion

        /// <summary>
        /// This method is called when the resource somehow changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        public void ResourceChanged(DependencyObject sender, DictionaryEventArgs e)
        {
            UpdateNewValue();
        }

        private void UpdateNewValue()
        {
            Content = FormatOutput();
        }

        #region Resource loopkup
        /// <summary>
        /// This function returns the properly prepared output of the markup extension.
        /// </summary>
        public object FormatOutput()
        {
            object result = null;

            if (_targetInfo == null)
                return null;

            var targetObject = _targetInfo.TargetObject as DependencyObject;

            // Get target type. Change ImageSource to BitmapSource in order to use our own converter.
            var targetType = _targetInfo.TargetPropertyType;

            if (targetType == typeof(ImageSource))
                targetType = typeof(BitmapSource);

            // Try to get the localized input from the resource.
            string resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(Key, targetObject);

            var ci = GetForcedCultureOrDefault();

            // Extract the names of the endpoint object and property
            var epName = "";
            var epProp = "";

            if (targetObject is FrameworkElement element)
                epName = element.GetValueSync<string>(NameProperty);
            else if (targetObject is FrameworkContentElement)
                epName = ((FrameworkContentElement)targetObject).GetValueSync<string>(FrameworkContentElement.NameProperty);

            if (_targetInfo.TargetProperty is PropertyInfo info)
                epProp = info.Name;
            else if (_targetInfo.TargetProperty is DependencyProperty)
                epProp = ((DependencyProperty)_targetInfo.TargetProperty).Name;

            // What are these names during design time good for? Any suggestions?
            if (epProp.Contains("FrameworkElementWidth5"))
                epProp = "Height";
            else if (epProp.Contains("FrameworkElementWidth6"))
                epProp = "Width";
            else if (epProp.Contains("FrameworkElementMargin12"))
                epProp = "Margin";

            var resKeyBase = ci.Name + ":" + targetType.Name + ":";
            string resKeyNameProp = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(epName + LocalizeDictionary.GetSeparation(targetObject) + epProp, targetObject);
            string resKeyName = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(epName, targetObject);

            // Check, if the key is already in our resource buffer.
            object input = null;

            if (!string.IsNullOrEmpty(resourceKey))
            {
                // We've got a resource key. Try to look it up or get it from the dictionary.
                lock (ResourceBufferLock)
                {
                    if (_resourceBuffer.ContainsKey(resKeyBase + resourceKey))
                        result = _resourceBuffer[resKeyBase + resourceKey];
                    else
                    {
                        input = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, targetObject, ci);
                        resKeyBase += resourceKey;
                    }
                }
            }
            else
            {
                // Try the automatic lookup function.
                // First, look for a resource entry named: [FrameworkElement name][Separator][Property name]
                lock (ResourceBufferLock)
                {
                    if (_resourceBuffer.ContainsKey(resKeyBase + resKeyNameProp))
                        result = _resourceBuffer[resKeyBase + resKeyNameProp];
                    else
                    {
                        // It was not stored in the buffer - try to retrieve it from the dictionary.
                        input = LocalizeDictionary.Instance.GetLocalizedObject(resKeyNameProp, targetObject, ci);

                        if (input == null)
                        {
                            // Now, try to look for a resource entry named: [FrameworkElement name]
                            // Note - this has to be nested here, as it would take precedence over the first step in the buffer lookup step.
                            if (_resourceBuffer.ContainsKey(resKeyBase + resKeyName))
                                result = _resourceBuffer[resKeyBase + resKeyName];
                            else
                            {
                                input = LocalizeDictionary.Instance.GetLocalizedObject(resKeyName, targetObject, ci);
                                resKeyBase += resKeyName;
                            }
                        }
                        else
                            resKeyBase += resKeyNameProp;
                    }
                }
            }

            // If no result was found, convert the input and add it to the buffer.
            if (result == null && input != null)
            {
                result = Converter.Convert(input, targetType, ConverterParameter, ci);
                SafeAddItemToResourceBuffer(resKeyBase, result);
            }

            return result;
        }
        #endregion
    }
}

namespace WPFLocalizeExtension
{
    #region Usings
    using System.Linq;
    #endregion

    /// <summary>
    /// A class that bundles the key, assembly and dictionary information.
    /// </summary>
    public class FQAssemblyDictionaryKey : FullyQualifiedResourceKeyBase
    {
        private readonly string _key;
        /// <summary>
        /// The key.
        /// </summary>
        public string Key => _key;

        private readonly string _assembly;
        /// <summary>
        /// The assembly of the dictionary.
        /// </summary>
        public string Assembly => _assembly;

        private readonly string _dictionary;
        /// <summary>
        /// The resource dictionary.
        /// </summary>
        public string Dictionary => _dictionary;

        /// <summary>
        /// Creates a new instance of <see cref="FullyQualifiedResourceKeyBase"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="assembly">The assembly of the dictionary.</param>
        /// <param name="dictionary">The resource dictionary.</param>
        public FQAssemblyDictionaryKey(string key, string assembly = null, string dictionary = null)
        {
            _key = key;
            _assembly = assembly;
            _dictionary = dictionary;
        }

        /// <summary>
        /// Converts the object to a string.
        /// </summary>
        /// <returns>The joined version of the assembly, dictionary and key.</returns>
        public
            override string ToString()
        {
            return string.Join(":", new[] { Assembly, Dictionary, Key }.Where(x => !string.IsNullOrEmpty(x)).ToArray());
        }
    }
}

namespace WPFLocalizeExtension
{
    /// <summary>
    /// An abstract class for key identification.
    /// </summary>
    public abstract class FullyQualifiedResourceKeyBase
    {
        /// <summary>
        /// Implicit string operator.
        /// </summary>
        /// <param name="fullyQualifiedResourceKey">The object.</param>
        /// <returns>The joined version of the assembly, dictionary and key.</returns>
        public static implicit operator string(FullyQualifiedResourceKeyBase fullyQualifiedResourceKey)
        {
            return fullyQualifiedResourceKey?.ToString();
        }
    }
}

namespace WPFLocalizeExtension
{
    #region Usings
    using System;
    using System.Windows;
    #endregion

    /// <summary>
    /// Interface for listeners on dictionary events of the <see cref="LocalizeDictionary"/> class.
    /// </summary>
    public interface IDictionaryEventListener
    {
        /// <summary>
        /// This method is called when the resource somehow changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        void ResourceChanged(DependencyObject sender, DictionaryEventArgs e);
    }

    /// <summary>
    /// An enumeration of dictionary event types.
    /// </summary>
    public enum DictionaryEventType
    {
        /// <summary>
        /// The separation changed.
        /// </summary>
        SeparationChanged,
        /// <summary>
        /// The provider changed.
        /// </summary>
        ProviderChanged,
        /// <summary>
        /// A provider reports an update.
        /// </summary>
        ProviderUpdated,
        /// <summary>
        /// The culture changed.
        /// </summary>
        CultureChanged,
        /// <summary>
        /// A certain value changed.
        /// </summary>
        ValueChanged,
    }

    /// <summary>
    /// Event argument for dictionary events.
    /// </summary>
    public class DictionaryEventArgs : EventArgs
    {
        /// <summary>
        /// The type of the event.
        /// </summary>
        public DictionaryEventType Type { get; }

        /// <summary>
        /// A corresponding tag.
        /// </summary>
        public object Tag { get; }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="type">The type of the event.</param>
        /// <param name="tag">The corresponding tag.</param>
        public DictionaryEventArgs(DictionaryEventType type, object tag)
        {
            Type = type;
            Tag = tag;
        }

        /// <summary>
        /// Returns the type and tag as a string.
        /// </summary>
        /// <returns>The type and tag as a string.</returns>
        public override string ToString()
        {
            return Type + ": " + Tag;
        }
    }
}

namespace WPFLocalizeExtension
{
    #region Usings
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows;
    #endregion

    /// <summary>
    /// An interface describing classes that provide localized values based on a source/dictionary/key combination.
    /// and used for a localization provider that uses Inheriting Dependency Properties
    /// </summary>
    public interface IInheritingLocalizationProvider : ILocalizationProvider
    {

    }
}

namespace WPFLocalizeExtension
{
    #region Usings
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows;
    #endregion

    /// <summary>
    /// An interface describing classes that provide localized values based on a source/dictionary/key combination.
    /// </summary>
    public interface ILocalizationProvider
    {
        /// <summary>
        /// Uses the key and target to build a fully qualified resource key (Assembly, Dictionary, Key)
        /// </summary>
        /// <param name="key">Key used as a base to find the full key</param>
        /// <param name="target">Target used to help determine key information</param>
        /// <returns>Returns an object with all possible pieces of the given key (Assembly, Dictionary, Key)</returns>
        FullyQualifiedResourceKeyBase GetFullyQualifiedResourceKey(string key, DependencyObject target);

        /// <summary>
        /// Get the localized object.
        /// </summary>
        /// <param name="key">The key to the value.</param>
        /// <param name="target">The target <see cref="DependencyObject"/>.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The value corresponding to the source/dictionary/key path for the given culture (otherwise NULL).</returns>
        object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture);

        /// <summary>
        /// An observable list of available cultures.
        /// </summary>
        ObservableCollection<CultureInfo> AvailableCultures { get; }

        /// <summary>
        /// An event that is fired when the provider changed.
        /// </summary>
        event ProviderChangedEventHandler ProviderChanged;

        /// <summary>
        /// An event that is fired when an error occurred.
        /// </summary>
        event ProviderErrorEventHandler ProviderError;

        /// <summary>
        /// An event that is fired when a value changed.
        /// </summary>
        event ValueChangedEventHandler ValueChanged;
    }
}

namespace WPFLocalizeExtension
{
    #region Usings
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Resources;
    using System.Windows;
    using XAMLMarkupExtensions.Base;
    #endregion

    /// <summary>
    /// A singleton RESX provider that uses inheriting attached properties.
    /// </summary>
    public class InheritingResxLocalizationProvider : ResxLocalizationProviderBase, IInheritingLocalizationProvider
    {
        #region Dependency Properties
        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultDictionary to set the fallback resource dictionary.
        /// </summary>
        public static readonly DependencyProperty DefaultDictionaryProperty =
                DependencyProperty.RegisterAttached(
                "DefaultDictionary",
                typeof(string),
                typeof(InheritingResxLocalizationProvider),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, AttachedPropertyChanged));

        /// <summary>
        /// <see cref="DependencyProperty"/> DefaultAssembly to set the fallback assembly.
        /// </summary>
        public static readonly DependencyProperty DefaultAssemblyProperty =
            DependencyProperty.RegisterAttached(
                "DefaultAssembly",
                typeof(string),
                typeof(InheritingResxLocalizationProvider),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, AttachedPropertyChanged));
        #endregion

        #region Dependency Property Callback
        /// <summary>
        /// Indicates, that one of the attached properties changed.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="args">The event argument.</param>
        private static void AttachedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Instance.OnProviderChanged(obj);
        }
        #endregion

        #region Dependency Property Management
        #region Get
        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> default dictionary.
        /// </summary>
        /// <param name="obj">The dependency object to get the default dictionary from.</param>
        /// <returns>The default dictionary.</returns>
        public static string GetDefaultDictionary(DependencyObject obj)
        {
            return obj.GetValueSync<string>(DefaultDictionaryProperty);
        }

        /// <summary>
        /// Getter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to get the default assembly from.</param>
        /// <returns>The default assembly.</returns>
        public static string GetDefaultAssembly(DependencyObject obj)
        {
            return obj.GetValueSync<string>(DefaultAssemblyProperty);
        }
        #endregion

        #region Set
        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> default dictionary.
        /// </summary>
        /// <param name="obj">The dependency object to set the default dictionary to.</param>
        /// <param name="value">The dictionary.</param>
        public static void SetDefaultDictionary(DependencyObject obj, string value)
        {
            obj.SetValueSync(DefaultDictionaryProperty, value);
        }

        /// <summary>
        /// Setter of <see cref="DependencyProperty"/> default assembly.
        /// </summary>
        /// <param name="obj">The dependency object to set the default assembly to.</param>
        /// <param name="value">The assembly.</param>
        public static void SetDefaultAssembly(DependencyObject obj, string value)
        {
            obj.SetValueSync(DefaultAssemblyProperty, value);
        }
        #endregion
        #endregion

        #region Singleton Variables, Properties & Constructor
        /// <summary>
        /// The instance of the singleton.
        /// </summary>
        private static InheritingResxLocalizationProvider _instance;

        /// <summary>
        /// Lock object for the creation of the singleton instance.
        /// </summary>
        private static readonly object InstanceLock = new object();

        /// <summary>
        /// Gets the <see cref="ResxLocalizationProvider"/> singleton.
        /// </summary>
        public static InheritingResxLocalizationProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (_instance == null)
                            _instance = new InheritingResxLocalizationProvider();
                    }
                }

                // return the existing/new instance
                return _instance;
            }
        }

        /// <summary>
        /// The singleton constructor.
        /// </summary>
        private InheritingResxLocalizationProvider()
        {
            ResourceManagerList = new Dictionary<string, ResourceManager>();
            AvailableCultures = new ObservableCollection<CultureInfo> { CultureInfo.InvariantCulture };
        }
        #endregion

        #region Abstract assembly & dictionary lookup
        /// <inheritdoc/>
        protected override string GetAssembly(DependencyObject target)
        {
            return target?.GetValue(DefaultAssemblyProperty) as string;
        }

        /// <inheritdoc/>
        protected override string GetDictionary(DependencyObject target)
        {
            return target?.GetValue(DefaultDictionaryProperty) as string;
        }
        #endregion
    }
}

namespace WPFLocalizeExtension
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Linq;

    #endregion

    /// <summary>
    /// Represents a collection of listeners.
    /// </summary>
    internal class ListenersList
    {
        private readonly Dictionary<WeakReference, int> listeners;
        private readonly Dictionary<int, List<WeakReference>> listenersHashCodes;
        private readonly List<WeakReference> deadListeners;

        /// <summary>
        /// Create new empty <see cref="ListenersList" /> instance.
        /// </summary>
        public ListenersList()
        {
            listeners = new Dictionary<WeakReference, int>();
            listenersHashCodes = new Dictionary<int, List<WeakReference>>();
            deadListeners = new List<WeakReference>();
        }

        /// <summary>
        /// The count of listeners.
        /// </summary>
        public int Count => listeners.Count;

        /// <summary>
        /// Add new listener.
        /// </summary>
        public void AddListener(IDictionaryEventListener listener)
        {
            // Add listener if it not registered yet.
            var weakReference = new WeakReference(listener);
            var hashCode = listener.GetHashCode();
            if (!listenersHashCodes.TryGetValue(hashCode, out var sameHashCodeListeners))
            {
                listeners.Add(weakReference, hashCode);
                listenersHashCodes.Add(hashCode, new List<WeakReference> { weakReference });
            }
            else if (sameHashCodeListeners.All(wr => wr.Target != listener))
            {
                listeners.Add(weakReference, hashCode);
                sameHashCodeListeners.Add(weakReference);
            }
        }

        /// <summary>
        /// Get all alive listeners.
        /// </summary>
        public IEnumerable<IDictionaryEventListener> GetListeners()
        {
            try
            {
                foreach (var listener in listeners)
                {
                    var listenerReference = listener.Key.Target as IDictionaryEventListener;
                    if (listenerReference == null)
                    {
                        deadListeners.Add(listener.Key);
                        continue;
                    }

                    yield return listenerReference;
                }
            }
            finally
            {
                // Finally block is necessary because of `yield return`.
                // It guarantees this method will be called even if listeners won't enumerate till the end.
                ClearDeadReferences();
            }
        }

        /// <summary>
        /// Remove listener.
        /// </summary>
        public void RemoveListener(IDictionaryEventListener listener)
        {
            var hashCode = listener.GetHashCode();
            if (!listenersHashCodes.TryGetValue(hashCode, out var hashCodes))
                return;

            var wr = hashCodes.FirstOrDefault(l => l.Target == listener);
            if (wr == null)
                return;

            if (hashCodes.Count > 1)
                hashCodes.Remove(wr);
            else
                listenersHashCodes.Remove(hashCode);

            listeners.Remove(wr);
        }

        /// <summary>
        /// Clear internal list from all dead listeners.
        /// </summary>
        private void ClearDeadReferences()
        {
            if (deadListeners.Count == 0)
                return;

            foreach (var deadListener in deadListeners)
            {
                var hashCode = listeners[deadListener];
                listenersHashCodes[hashCode].Remove(deadListener);
                if (!listenersHashCodes[hashCode].Any())
                    listenersHashCodes.Remove(hashCode);

                listeners.Remove(deadListener);
            }

            deadListeners.Clear();
        }
    }
}

namespace WPFLocalizeExtension
{
    #region Usings
    using System;
    #endregion

    /// <summary>
    /// Event arguments for a missing key event.
    /// </summary>
    public class MissingKeyEventArgs : EventArgs
    {
        /// <summary>
        /// The key that is missing or has no data.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// A flag indicating that a reload should be performed.
        /// </summary>
        public bool Reload { get; set; }

        /// <summary>
        /// A custom returnmessage for the missing key
        /// </summary>
        public string MissingKeyResult { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="MissingKeyEventArgs"/>.
        /// </summary>
        /// <param name="key">The missing key.</param>
        public MissingKeyEventArgs(string key)
        {
            Key = key;
            Reload = false;
            MissingKeyResult = null;
        }
    }
}

namespace WPFLocalizeExtension
{
    #region Usings
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using WPFLocalizeExtension;
    using XAMLMarkupExtensions.Base;
    #endregion

    /// <summary>
    /// Extension methods for <see cref="DependencyObject"/> in conjunction with the <see cref="XAMLMarkupExtensions.Base.ParentChangedNotifier"/>.
    /// </summary>
    public static class ParentChangedNotifierHelper
    {
        /// <summary>
        /// Tries to get a value that is stored somewhere in the visual tree above this <see cref="DependencyObject"/>.
        /// <para>If this is not available, it will register a <see cref="XAMLMarkupExtensions.Base.ParentChangedNotifier"/> on the last element.</para>
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject"/>.</param>
        /// <param name="getFunction">The function that gets the value from a <see cref="DependencyObject"/>.</param>
        /// <param name="parentChangedAction">The notification action on the change event of the Parent property.</param>
        /// <param name="parentNotifiers">A dictionary of already registered notifiers.</param>
        /// <returns>The value, if possible.</returns>
        public static T GetValueOrRegisterParentNotifier<T>(
            this DependencyObject target,
            Func<DependencyObject, T> getFunction,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers)
        {
            var ret = default(T);

            if (target == null) return ret;

            var depObj = target;
            var weakTarget = new WeakReference(target);

            while (ret == null)
            {
                // Try to get the value using the provided GetFunction.
                ret = getFunction(depObj);

                if (ret != null)
                    parentNotifiers.Remove(target);

                // Try to get the parent using the visual tree helper. This may fail on some occations.
                if (depObj is System.Windows.Controls.ToolTip)
                    break;

                if (!(depObj is Visual) && !(depObj is Visual3D) && !(depObj is FrameworkContentElement))
                    break;

                if (depObj is Window)
                    break;

                DependencyObject depObjParent;

                if (depObj is FrameworkContentElement element)
                    depObjParent = element.Parent;
                else
                {
                    try
                    {
                        depObjParent = depObj.GetParent(false);
                    }
                    catch
                    {
                        depObjParent = null;
                    }
                }

                if (depObjParent == null)
                {
                    try
                    {
                        depObjParent = depObj.GetParent(true);
                    }
                    catch
                    {
                        break;
                    }
                }

                // If this failed, try again using the Parent property (sometimes this is not covered by the VisualTreeHelper class :-P.
                if (depObjParent == null && depObj is FrameworkElement)
                    depObjParent = ((FrameworkElement)depObj).Parent;

                if (ret == null && depObjParent == null)
                {
                    // Try to establish a notification on changes of the Parent property of dp.
                    if (depObj is FrameworkElement frameworkElement && !parentNotifiers.ContainsKey(target))
                    {
                        var pcn = new ParentChangedNotifier(frameworkElement, () =>
                        {
                            var localTarget = (DependencyObject)weakTarget.Target;
                            if (localTarget == null)
                                return;

                            // Call the action...
                            parentChangedAction(localTarget);
                            // ...and remove the notifier - it will probably not be used again.
                            parentNotifiers.Remove(localTarget);
                        });

                        parentNotifiers.Add(target, pcn);
                    }
                    break;
                }

                // Assign the parent to the current DependencyObject and start the next iteration.
                depObj = depObjParent;
            }

            return ret;
        }

        /// <summary>
        /// Tries to get a value that is stored somewhere in the visual tree above this <see cref="DependencyObject"/>.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject"/>.</param>
        /// <param name="getFunction">The function that gets the value from a <see cref="DependencyObject"/>.</param>
        /// <returns>The value, if possible.</returns>
        public static T GetValue<T>(this DependencyObject target, Func<DependencyObject, T> getFunction)
        {
            var ret = default(T);

            if (target != null)
            {
                var depObj = target;

                while (ret == null)
                {
                    // Try to get the value using the provided GetFunction.
                    ret = getFunction(depObj);

                    // Try to get the parent using the visual tree helper. This may fail on some occations.
                    if (!(depObj is Visual) && !(depObj is Visual3D) && !(depObj is FrameworkContentElement))
                        break;

                    DependencyObject depObjParent;

                    if (depObj is FrameworkContentElement element)
                        depObjParent = element.Parent;
                    else
                    {
                        try
                        {
                            depObjParent = depObj.GetParent(true);
                        }
                        catch
                        {
                            break;
                        }
                    }
                    // If this failed, try again using the Parent property (sometimes this is not covered by the VisualTreeHelper class :-P.
                    if (depObjParent == null && depObj is FrameworkElement)
                        depObjParent = ((FrameworkElement)depObj).Parent;

                    if (ret == null && depObjParent == null)
                        break;

                    // Assign the parent to the current DependencyObject and start the next iteration.
                    depObj = depObjParent;
                }
            }

            return ret;
        }

        /// <summary>
        /// Tries to get a value from a <see cref="DependencyProperty"/> that is stored somewhere in the visual tree above this <see cref="DependencyObject"/>.
        /// If this is not available, it will register a <see cref="XAMLMarkupExtensions.Base.ParentChangedNotifier"/> on the last element.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="target">The <see cref="DependencyObject"/>.</param>
        /// <param name="property">A <see cref="DependencyProperty"/> that will be read out.</param>
        /// <param name="parentChangedAction">The notification action on the change event of the Parent property.</param>
        /// <param name="parentNotifiers">A dictionary of already registered notifiers.</param>
        /// <returns>The value, if possible.</returns>
        public static T GetValueOrRegisterParentNotifier<T>(
            this DependencyObject target,
            DependencyProperty property,
            Action<DependencyObject> parentChangedAction,
            ParentNotifiers parentNotifiers)
        {
            return target.GetValueOrRegisterParentNotifier(depObj => depObj.GetValueSync<T>(property), parentChangedAction, parentNotifiers);
        }

        /// <summary>
        /// Gets the parent in the visual or logical tree.
        /// </summary>
        /// <param name="depObj">The dependency object.</param>
        /// <param name="isVisualTree">True for visual tree, false for logical tree.</param>
        /// <returns>The parent, if available.</returns>
        public static DependencyObject GetParent(this DependencyObject depObj, bool isVisualTree)
        {
            if (depObj.CheckAccess())
                return GetParentInternal(depObj, isVisualTree);

            return (DependencyObject)depObj.Dispatcher.Invoke(new Func<DependencyObject>(() => GetParentInternal(depObj, isVisualTree)));
        }

        private static DependencyObject GetParentInternal(DependencyObject depObj, bool isVisualTree)
        {
            if (isVisualTree)
                return VisualTreeHelper.GetParent(depObj);

            return LogicalTreeHelper.GetParent(depObj);
        }
    }
}

namespace WPFLocalizeExtension
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using XAMLMarkupExtensions.Base;
    #endregion

    /// <summary>
    /// A memory safe dictionary storage for <see cref="ParentChangedNotifier"/> instances.
    /// </summary>
    public class ParentNotifiers
    {
        readonly Dictionary<WeakReference<DependencyObject>, ParentChangedNotifier> _inner =
            new Dictionary<WeakReference<DependencyObject>, ParentChangedNotifier>();

        /// <summary>
        /// Check, if it contains the key.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>True, if the key exists.</returns>
        public bool ContainsKey(DependencyObject target)
        {
            return _inner.Keys.Any(x => x.TryGetTarget(out var item) && ReferenceEquals(item, target));
        }

        /// <summary>
        /// Removes the entry.
        /// </summary>
        /// <param name="target">The target object.</param>
        public void Remove(DependencyObject target)
        {
            if (_inner.Count == 0)
                return;

            var deadItems = new List<KeyValuePair<WeakReference<DependencyObject>, ParentChangedNotifier>>();

            foreach (var item in _inner)
            {
                // If we can't get target (== target is dead) or this is the item which we have to remove - add it to the collection for removing.
                if (!item.Key.TryGetTarget(out var itemTarget) || ReferenceEquals(itemTarget, target))
                {
                    deadItems.Add(item);
                }
            }

            foreach (var deadItem in deadItems)
            {
                deadItem.Value?.Dispose();
                _inner.Remove(deadItem.Key);
            }
        }

        /// <summary>
        /// Adds the key-value-pair.
        /// </summary>
        /// <param name="target">The target key object.</param>
        /// <param name="parentChangedNotifier">The notifier.</param>
        public void Add(DependencyObject target, ParentChangedNotifier parentChangedNotifier)
        {
            _inner.Add(new WeakReference<DependencyObject>(target), parentChangedNotifier);
        }
    }
}

namespace WPFLocalizeExtension
{
    #region Usings
    using System;
    using System.Windows;
    #endregion

    /// <summary>
    /// Events arguments for a ProviderChangedEventHandler.
    /// </summary>
    public class ProviderChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The target object.
        /// </summary>
        public DependencyObject Object { get; }

        /// <summary>
        /// Creates a new <see cref="ProviderChangedEventArgs"/> instance.
        /// </summary>
        /// <param name="obj">The target object.</param>
        public ProviderChangedEventArgs(DependencyObject obj)
        {
            Object = obj;
        }
    }

    /// <summary>
    /// An event handler for notification of provider changes.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The event arguments.</param>
    public delegate void ProviderChangedEventHandler(object sender, ProviderChangedEventArgs args);

    /// <summary>
    /// Events arguments for a ProviderErrorEventHandler.
    /// </summary>
    public class ProviderErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The target object.
        /// </summary>
        public DependencyObject Object { get; }

        /// <summary>
        /// The key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Creates a new <see cref="ProviderErrorEventArgs"/> instance.
        /// </summary>
        /// <param name="obj">The target object.</param>
        /// <param name="key">The key that caused the error.</param>
        /// <param name="message">The error message.</param>
        public ProviderErrorEventArgs(DependencyObject obj, string key, string message)
        {
            Object = obj;
            Key = key;
            Message = message;
        }
    }

    /// <summary>
    /// An event handler for notification of provider erorrs.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The event arguments.</param>
    public delegate void ProviderErrorEventHandler(object sender, ProviderErrorEventArgs args);

    /// <summary>
    /// Events arguments for a ValueChangedEventHandler.
    /// </summary>
    public class ValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// A custom tag.
        /// </summary>
        public object Tag { get; }

        /// <summary>
        /// The new value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// The key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Creates a new <see cref="ValueChangedEventArgs"/> instance.
        /// </summary>
        /// <param name="key">The key where the value was changed.</param>
        /// <param name="value">The new value.</param>
        /// <param name="tag">A custom tag.</param>
        public ValueChangedEventArgs(string key, object value, object tag)
        {
            Key = key;
            Value = value;
            Tag = tag;
        }
    }

    /// <summary>
    /// An event handler for notification of changes of localized values.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The event arguments.</param>
    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs args);
}

namespace WPFLocalizeExtension
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Reflection;
    using System.Resources;
    using System.Windows;
    #endregion

    /// <summary>
    /// The base for RESX file providers.
    /// </summary>
    public abstract class ResxLocalizationProviderBase : DependencyObject, ILocalizationProvider
    {
        #region Variables
        /// <summary>
        /// Holds the name of the Resource Manager.
        /// </summary>
        private const string ResourceManagerName = "ResourceManager";

        /// <summary>
        /// Holds the extension of the resource files.
        /// </summary>
        private const string ResourceFileExtension = ".resources";

        /// <summary>
        /// Holds the binding flags for the reflection to find the resource files.
        /// </summary>
        private const BindingFlags ResourceBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

        /// <summary>
        /// Gets the used ResourceManagers with their corresponding <c>namespaces</c>.
        /// </summary>
        protected Dictionary<string, ResourceManager> ResourceManagerList;

        /// <summary>
        /// Lock object for concurrent access to the resource manager list.
        /// </summary>
        protected object ResourceManagerListLock = new object();

        /// <summary>
        /// Lock object for concurrent access to the available culture list.
        /// </summary>
        protected object AvailableCultureListLock = new object();

        private bool _ignoreCase = true;
        /// <summary>
        /// Gets or sets the ignore case flag.
        /// </summary>
        public bool IgnoreCase
        {
            get => _ignoreCase;
            set => _ignoreCase = value;
        }

        private List<CultureInfo> searchCultures = null;
        /// <summary>
        /// Gets or sets the cultures there the RESX Provider search for.
        /// </summary>
        public List<CultureInfo> SearchCultures
        {
            get
            {
                if (searchCultures == null)
                    searchCultures = CultureInfo.GetCultures(CultureTypes.AllCultures).ToList();
                return searchCultures;
            }
            set
            {
                searchCultures = value;
            }
        }
        #endregion

        #region Helper functions
        /// <summary>
        /// Returns the <see cref="AssemblyName"/> of the passed assembly instance
        /// </summary>
        /// <param name="assembly">The Assembly where to get the name from</param>
        /// <returns>The Assembly name</returns>
        protected string GetAssemblyName(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (assembly.FullName == null)
            {
                throw new NullReferenceException("assembly.FullName is null");
            }

            return assembly.FullName.Split(',')[0];
        }

        /// <summary>
        /// Parses a key ([[Assembly:]Dict:]Key and return the parts of it.
        /// </summary>
        /// <param name="inKey">The key to parse.</param>
        /// <param name="outAssembly">The found or default assembly.</param>
        /// <param name="outDict">The found or default dictionary.</param>
        /// <param name="outKey">The found or default key.</param>
        public static void ParseKey(string inKey, out string outAssembly, out string outDict, out string outKey)
        {
            // Reset everything to null.
            outAssembly = null;
            outDict = null;
            outKey = null;

            if (!string.IsNullOrEmpty(inKey))
            {
                var split = inKey.Trim().Split(":".ToCharArray());

                // assembly:dict:key
                if (split.Length == 3)
                {
                    outAssembly = !string.IsNullOrEmpty(split[0]) ? split[0] : null;
                    outDict = !string.IsNullOrEmpty(split[1]) ? split[1] : null;
                    outKey = split[2];
                }

                // dict:key
                if (split.Length == 2)
                {
                    outDict = !string.IsNullOrEmpty(split[0]) ? split[0] : null;
                    outKey = split[1];
                }

                // key
                if (split.Length == 1)
                {
                    outKey = split[0];
                }
            }
        }
        #endregion

        #region Assembly & dictionary lookup
        /// <summary>
        /// Get the assembly from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The assembly name, if available.</returns>
        protected abstract string GetAssembly(DependencyObject target);

        /// <summary>
        /// Get the dictionary from the context, if possible.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <returns>The dictionary name, if available.</returns>
        protected abstract string GetDictionary(DependencyObject target);
        #endregion

        #region ResourceManager management
        /// <summary>
        /// Thread-safe access to the resource manager dictionary.
        /// </summary>
        /// <param name="thekey">Key.</param>
        /// <param name="result">Value.</param>
        /// <returns>Success of the operation.</returns>
        protected bool TryGetValue(string thekey, out ResourceManager result)
        {
            lock (ResourceManagerListLock) { return ResourceManagerList.TryGetValue(thekey, out result); }
        }

        /// <summary>
        /// Thread-safe access to the resource manager dictionary.
        /// </summary>
        /// <param name="thekey">Key.</param>
        /// <param name="value">Value.</param>
        protected void Add(string thekey, ResourceManager value)
        {
            lock (ResourceManagerListLock) { ResourceManagerList.Add(thekey, value); }
        }

        /// <summary>
        /// Tries to remove a key from the resource manager dictionary.
        /// </summary>
        /// <param name="thekey">Key.</param>
        protected void TryRemove(string thekey)
        {
            lock (ResourceManagerListLock) { if (ResourceManagerList.ContainsKey(thekey)) ResourceManagerList.Remove(thekey); }
        }

        /// <summary>
        /// Clears the whole list of cached resource managers.
        /// </summary>
        public void ClearResourceManagerList()
        {
            lock (ResourceManagerListLock) { ResourceManagerList.Clear(); }
        }

        /// <summary>
        /// Thread-safe access to the AvailableCultures list.
        /// </summary>
        /// <param name="c">The CultureInfo.</param>
        protected void AddCulture(CultureInfo c)
        {
            lock (AvailableCultureListLock)
            {
                if (!AvailableCultures.Contains(c))
                    AvailableCultures.Add(c);
            }
        }

        /// <summary>
        /// Updates the list of available cultures using the given resource location.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly.</param>
        /// <param name="resourceDictionary">The dictionary to look up.</param>
        /// <returns>True, if the update was successful.</returns>
        public bool UpdateCultureList(string resourceAssembly, string resourceDictionary)
        {
            return GetResourceManager(resourceAssembly, resourceDictionary) != null;
        }

        private static readonly Dictionary<int, string> ExecutablePaths = new Dictionary<int, string>();
        private DateTime _lastUpdateCheck = DateTime.MinValue;

        private static string _projectDirectory;
        private static string[] _projectFilesCache;

        /// <summary>
        /// Get the executable path for both x86 and x64 processes.
        /// </summary>
        /// <param name="processId">The process id.</param>
        /// <returns>The path if found; otherwise, null.</returns>
        private static string GetExecutablePath(int processId)
        {
            if (ExecutablePaths.ContainsKey(processId))
                return ExecutablePaths[processId];

            const string wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                var query = from p in Process.GetProcesses()
                            join mo in results.Cast<ManagementObject>()
                            on p.Id equals (int)(uint)mo["ProcessId"]
                            where p.Id == processId
                            select new
                            {
                                Process = p,
                                Path = (string)mo["ExecutablePath"],
                                CommandLine = (string)mo["CommandLine"],
                            };

                foreach (var item in query)
                {
                    ExecutablePaths.Add(processId, item.Path);
                    return item.Path;
                }
            }

            return null;
        }

        private static bool IsFileOfInterest(string f, string dir)
        {
            if (string.IsNullOrEmpty(f))
                return false;

            if (!(f.EndsWith(".resx", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".resources.dll", StringComparison.OrdinalIgnoreCase) ||
                  f.EndsWith(".resources", StringComparison.OrdinalIgnoreCase)) &&
                !dir.Equals(Path.GetDirectoryName(f), StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        /// <summary>
        /// Looks up in the cached <see cref="ResourceManager"/> list for the searched <see cref="ResourceManager"/>.
        /// </summary>
        /// <param name="resourceAssembly">The resource assembly.</param>
        /// <param name="resourceDictionary">The dictionary to look up.</param>
        /// <returns>
        /// The found <see cref="ResourceManager"/>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the ResourceManagers cannot be looked up
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the searched <see cref="ResourceManager"/> wasn't found
        /// </exception>
        protected ResourceManager GetResourceManager(string resourceAssembly, string resourceDictionary)
        {
            Assembly assembly = null;
            string foundResource = null;
            if (string.IsNullOrEmpty(resourceAssembly) && resourceDictionary.Contains('.')) resourceAssembly = resourceDictionary.Split('.')[0];
            var resManagerNameToSearch = resourceDictionary + ResourceFileExtension;
            //var resManagerNameToSearch = "." + resourceDictionary + ResourceFileExtension;

            var resManKey = resourceDictionary + ResourceFileExtension;
            //var resManKey = resourceAssembly + resManagerNameToSearch;

            // Here comes our great hack for full VS2012+ design time support with multiple languages.
            // We check only every second to reduce overhead in the designer.
            var now = DateTime.Now;

            if (AppDomain.CurrentDomain.FriendlyName.Contains("XDesProc") && ((now - _lastUpdateCheck).TotalSeconds >= 1.0))
            {
                // This block is only handled during design time.
                _lastUpdateCheck = now;

                // Get the directory of the executing assembly (some strange path in the middle of nowhere on the disk and attach "\tmp", e.g.:
                // %userprofile%\AppData\Local\Microsoft\VisualStudio\12.0\Designer\ShadowCache\erys4uqz.oq1\l24nfewi.r0y\tmp\
                var assemblyDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(), "tmp");

                // If not done yet, find the VS process that shows our design.
                if (string.IsNullOrEmpty(_projectDirectory))
                {
                    foreach (var process in Process.GetProcesses())
                    {
                        if (!process.ProcessName.Contains(".vshost"))
                            continue;

                        // Get the executable path (all paths are cached now in order to reduce WMI load.
                        _projectDirectory = Path.GetDirectoryName(GetExecutablePath(process.Id));

                        if (string.IsNullOrEmpty(_projectDirectory))
                            continue;

                        // Get all files.
                        var files = Directory.GetFiles(_projectDirectory, "*.*", SearchOption.AllDirectories);

                        if (files.Count(f => Path.GetFileName(f).StartsWith(resourceAssembly)) > 0)
                        {
                            // We got a hit. Filter the files that are of interest for this provider.
                            _projectFilesCache = files.Where(f => IsFileOfInterest(f, _projectDirectory)).ToArray();

                            // Must break here - otherwise, we might catch another instance of VS.
                            break;
                        }
                    }
                }
                else
                {
                    // Remove the resource manager from the dictionary.
                    TryRemove(resManKey);

                    // Copy all newer or missing files.
                    foreach (var f in _projectFilesCache)
                    {
                        try
                        {
                            var dst = Path.Combine(assemblyDir, f.Replace(_projectDirectory + "\\", ""));

                            if (!File.Exists(dst) || (Directory.GetLastWriteTime(dst) < Directory.GetLastWriteTime(f)))
                            {
                                var dstDir = Path.GetDirectoryName(dst);
                                if (string.IsNullOrEmpty(dstDir))
                                    continue;
                                if (!Directory.Exists(dstDir))
                                    Directory.CreateDirectory(dstDir);

                                File.Copy(f, dst, true);
                            }
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch
                        {
                        }
                    }

                    // Prepare and load (new) assembly.
                    var file = Path.Combine(assemblyDir, resourceAssembly + ".exe");
                    if (!File.Exists(file))
                        file = Path.Combine(assemblyDir, resourceAssembly + ".dll");

                    assembly = Assembly.LoadFrom(file);
                }
            }

            if (!TryGetValue(resManKey, out var resManager))
            {
                // If the assembly cannot be loaded, throw an exception
                if (assembly == null)
                {
                    try
                    {
                        // go through every assembly loaded in the app domain
                        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                        foreach (var assemblyInAppDomain in loadedAssemblies)
                        {
                            // get the assembly name object
                            var assemblyName = new AssemblyName(assemblyInAppDomain.FullName);

                            // check if the name of the assembly is the seached one
                            if (assemblyName.Name == resourceAssembly)
                            {
                                // assigne the assembly
                                assembly = assemblyInAppDomain;

                                // stop the search here
                                break;
                            }
                        }

                        // check if the assembly is still null
                        if (assembly == null)
                        {
                            // assign the loaded assembly
                            assembly = Assembly.Load(new AssemblyName(resourceAssembly));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"The Assembly '{resourceAssembly}' cannot be loaded.", ex);
                    }
                }

                // get all available resourcenames
                var availableResources = assembly.GetManifestResourceNames();

                // get all available types (and ignore unloadable types, e.g. because of unsatisfied dependencies)
                IEnumerable<Type> availableTypes;
                try
                {
                    availableTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    availableTypes = e.Types.Where(t => t != null);
                }

                // The proposed approach of Andras (http://wpflocalizeextension.codeplex.com/discussions/66098?ProjectName=wpflocalizeextension)
#pragma warning disable IDE0062
                string TryGetNamespace(Type type)
                {
                    // Ignore unloadable types
                    try
                    {
                        return type.Namespace;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
#pragma warning restore IDE0062

                var possiblePrefixes = availableTypes.Select(TryGetNamespace).Where(n => n != null).Distinct().ToList();

                foreach (var availableResource in availableResources)
                {
                    if (availableResource.EndsWith(resManagerNameToSearch) && possiblePrefixes.Any(p => availableResource.StartsWith(p + ".")))
                    {
                        // take the first occurrence and break
                        foundResource = availableResource;
                        break;
                    }
                }

                // NOTE: Inverted this IF (nesting is bad, I know) so we just create a new ResourceManager.  -gen3ric
                if (foundResource != null)
                {
                    // remove ".resources" from the end
                    foundResource = foundResource.Substring(0, foundResource.Length - ResourceFileExtension.Length);

                    // First try the simple retrieval
                    Type resourceManagerType;
                    try
                    {
                        resourceManagerType = assembly.GetType(foundResource);
                    }
                    catch (Exception)
                    {
                        resourceManagerType = null;
                    }

                    // If simple doesn't work, check all of the types without using dot notation
                    if (resourceManagerType == null)
                    {
                        var dictTypeName = resourceDictionary.Replace('.', '_');

                        bool MatchesDictTypeName(Type type)
                        {
                            // Ignore unloadable types
                            try
                            {
                                return type.Name == dictTypeName;
                            }
                            catch (Exception)
                            {
                                return false;
                            }
                        }

                        resourceManagerType = availableTypes.FirstOrDefault(MatchesDictTypeName);
                    }

                    resManager = GetResourceManagerFromType(resourceManagerType);
                }
                else
                {
                    //To be able to use Microsoft Resource like Key=PresentationCore:ExceptionStringTable:DeleteText. It is not detected at line 437
                    if (resManagerNameToSearch.StartsWith("."))
                    {
                        resManagerNameToSearch = resManagerNameToSearch.Remove(0, 1);
                        resManagerNameToSearch = resManagerNameToSearch.Replace(ResourceFileExtension, string.Empty);
                    }
                    resManager = new ResourceManager(resManagerNameToSearch, assembly);
                }

                // if no one was found, exception
                if (resManager == null)
                    throw new ArgumentException(string.Format("No resource manager for dictionary '{0}' in assembly '{1}' found! ({1}.{0})", resourceDictionary, resourceAssembly));

                // Add the ResourceManager to the cachelist
                Add(resManKey, resManager);

                try
                {
                    // Look in all cultures and check available ressources.
                    foreach (var c in SearchCultures)
                    {
                        var rs = resManager.GetResourceSet(c, true, false);
                        if (rs != null)
                            AddCulture(c);
                    }
                }
                catch
                {
                    // ignored
                }
            }

            // return the found ResourceManager
            return resManager;
        }

        private ResourceManager GetResourceManagerFromType(IReflect type)
        {
            if (type == null)
                return null;
            try
            {
                var propInfo = type.GetProperty(ResourceManagerName, ResourceBindingFlags);

                // get the GET-method from the methodinfo
                var methodInfo = propInfo.GetGetMethod(true);

                // cast it to a ResourceManager for better working with
                return (ResourceManager)methodInfo.Invoke(null, null);
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region ILocalizationProvider implementation
        /// <summary>
        /// Uses the key and target to build a fully qualified resource key (Assembly, Dictionary, Key)
        /// </summary>
        /// <param name="key">Key used as a base to find the full key</param>
        /// <param name="target">Target used to help determine key information</param>
        /// <returns>Returns an object with all possible pieces of the given key (Assembly, Dictionary, Key)</returns>
        public virtual FullyQualifiedResourceKeyBase GetFullyQualifiedResourceKey(string key, DependencyObject target)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            ParseKey(key, out var assembly, out var dictionary, out key);

            if (string.IsNullOrEmpty(assembly))
                assembly = GetAssembly(target);

            if (string.IsNullOrEmpty(dictionary))
                dictionary = GetDictionary(target);

            return new FQAssemblyDictionaryKey(key, assembly, dictionary);
        }

        /// <summary>
        /// Gets fired when the provider changed.
        /// </summary>
        public event ProviderChangedEventHandler ProviderChanged;

        /// <summary>
        /// An event that is fired when an error occurred.
        /// </summary>
        public event ProviderErrorEventHandler ProviderError;

        /// <summary>
        /// An event that is fired when a value changed.
        /// </summary>
        public event ValueChangedEventHandler ValueChanged;

        /// <summary>
        /// Calls the <see cref="ILocalizationProvider.ProviderChanged"/> event.
        /// </summary>
        /// <param name="target">The target object.</param>
        protected virtual void OnProviderChanged(DependencyObject target)
        {
            try
            {
                var assembly = GetAssembly(target);
                var dictionary = GetDictionary(target);

                if (!string.IsNullOrEmpty(assembly) && !string.IsNullOrEmpty(dictionary))
                    GetResourceManager(assembly, dictionary);
            }
            catch
            {
                // ignored
            }

            ProviderChanged?.Invoke(this, new ProviderChangedEventArgs(target));
        }

        /// <summary>
        /// Calls the <see cref="ILocalizationProvider.ProviderError"/> event.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="key">The key.</param>
        /// <param name="message">The error message.</param>
        protected virtual void OnProviderError(DependencyObject target, string key, string message)
        {
            ProviderError?.Invoke(this, new ProviderErrorEventArgs(target, key, message));
        }

        /// <summary>
        /// Calls the <see cref="ILocalizationProvider.ValueChanged"/> event.
        /// </summary>
        /// <param name="key">The key where the value was changed.</param>
        /// <param name="value">The new value.</param>
        /// <param name="tag">A custom tag.</param>
        protected virtual void OnValueChanged(string key, object value, object tag)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(key, value, tag));
        }

        /// <summary>
        /// Get the localized object.
        /// </summary>
        /// <param name="key">The key to the value.</param>
        /// <param name="target">The target object.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The value corresponding to the source/dictionary/key path for the given culture (otherwise NULL).</returns>
        public virtual object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture)
        {
            FQAssemblyDictionaryKey fqKey = (FQAssemblyDictionaryKey)GetFullyQualifiedResourceKey(key, target);

            // Final validation of the values.
            // Most important is key. fqKey may be null.
            if (string.IsNullOrEmpty(fqKey?.Key))
            {
                OnProviderError(target, key, "No key provided.");
                return null;
            }
            // fqKey cannot be null now
            if (string.IsNullOrEmpty(fqKey.Assembly))
            {
                OnProviderError(target, key, "No assembly provided.");
                return null;
            }
            if (string.IsNullOrEmpty(fqKey.Dictionary))
            {
                OnProviderError(target, key, "No dictionary provided.");
                return null;
            }

            // declaring local resource manager
            ResourceManager resManager;

            // try to get the resouce manager
            try
            {
                resManager = GetResourceManager(fqKey.Assembly, fqKey.Dictionary);
            }
            catch (Exception e)
            {
                OnProviderError(target, key, "Error retrieving the resource manager.\r\n" + e.Message);
                return null;
            }

            // finally, return the searched object as type of the generic type
            try
            {
                resManager.IgnoreCase = _ignoreCase;
                var result = resManager.GetObject(fqKey.Key, culture);

                if (result == null)
                    OnProviderError(target, key, "Missing key.");

                return result;
            }
            catch (Exception e)
            {
                OnProviderError(target, key, "Error retrieving the resource.\r\n" + e.Message);
                return null;
            }
        }

        /// <summary>
        /// An observable list of available cultures.
        /// </summary>
        public ObservableCollection<CultureInfo> AvailableCultures { get; protected set; }
        #endregion
    }
}

namespace WPFLocalizeExtension
{
    #region Usings
    using System;
    using XAMLMarkupExtensions.Base;
    #endregion

    /// <summary>
    /// An extension to the <see cref="T:XAMLMarkupExtensions.Base.TargetInfo" /> class with WeakReference instead of direct object linking.
    /// </summary>
    public class SafeTargetInfo : TargetInfo
    {
        /// <summary>
        /// Gets the target object reference.
        /// </summary>
        public WeakReference TargetObjectReference { get; }

        /// <summary>
        /// Creates a new TargetInfo instance.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="targetPropertyType">The target property type.</param>
        /// <param name="targetPropertyIndex">The target property index.</param>
        public SafeTargetInfo(object targetObject, object targetProperty, Type targetPropertyType, int targetPropertyIndex)
            : base(null, targetProperty, targetPropertyType, targetPropertyIndex)
        {
            TargetObjectReference = new WeakReference(targetObject);
        }

        /// <summary>
        /// Creates a new <see cref="SafeTargetInfo"/> based on a <see cref="XAMLMarkupExtensions.Base.TargetInfo"/> template.
        /// </summary>
        /// <param name="targetInfo">The target information.</param>
        /// <returns>A new instance with safe references.</returns>
        public static SafeTargetInfo FromTargetInfo(TargetInfo targetInfo)
        {
            return new SafeTargetInfo(targetInfo.TargetObject, targetInfo.TargetProperty, targetInfo.TargetPropertyType, targetInfo.TargetPropertyIndex);
        }
    }
}

namespace WPFLocalizeExtension.TypeConverters
{
    #region Usings
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Media.Imaging;
    #endregion

    /// <summary>
    /// A type converter class for Bitmap resources that are used in WPF.
    /// </summary>
    public class BitmapSourceTypeConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(Bitmap);
        }

        /// <inheritdoc/>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(Bitmap);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is Bitmap bitmap))
                return null;

            var bmpPt = bitmap.GetHbitmap();

            // create the bitmapSource
            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmpPt,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // freeze the bitmap to avoid hooking events to the bitmap
            bitmapSource.Freeze();

            // free memory
            DeleteObject(bmpPt);

            // return bitmapSource
            return bitmapSource;
        }

        /// <summary>
        /// Frees memory of a pointer.
        /// </summary>
        /// <param name="o">Object to remove from memory.</param>
        /// <returns>0 if the removing was success, otherwise another number.</returns>
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern int DeleteObject(IntPtr o);

        /// <inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var source = value as BitmapSource;

            if (value == null)
                return null;

            var bmp = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                PixelFormat.Format32bppPArgb);

            var data = bmp.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);

            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);

            bmp.UnlockBits(data);

            return bmp;
        }
    }
}

namespace WPFLocalizeExtension.TypeConverters
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    #endregion

    /// <summary>
    /// Implements a standard converter that calls itself all known type converters.
    /// </summary>
    public class DefaultConverter : IValueConverter
    {
        private static readonly Dictionary<Type, TypeConverter> TypeConverters = new Dictionary<Type, TypeConverter>();

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>The value to be passed to the target dependency property.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            object result;
            var resourceType = value.GetType();

            // Simplest cases: The target type is object or same as the input.
            if (targetType == typeof(object) || resourceType == targetType)
                return value;

            // Register missing type converters - this class will do this only once per appdomain.
            RegisterMissingTypeConverters.Register();

            // Is the type already known?
            if (!TypeConverters.ContainsKey(targetType))
            {
                var c = TypeDescriptor.GetConverter(targetType);

                if (targetType == typeof(Thickness))
                    c = new ThicknessConverter();

                // Get the type converter and store it in the dictionary (even if it is NULL).
                TypeConverters.Add(targetType, c);
            }

            // Get the converter.
            var conv = TypeConverters[targetType];

            // No converter or not convertable?
            if (conv == null || !conv.CanConvertFrom(resourceType))
                return null;

            // Finally, try to convert the value.
            try
            {
                result = conv.ConvertFrom(value);
            }
            catch
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>The value to be passed to the source object.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}

namespace WPFLocalizeExtension.TypeConverters
{
    #region Usings
    using System.ComponentModel;
    using System.Windows.Media.Imaging;
    #endregion

    /// <summary>
    /// Register missing type converters here.
    /// </summary>
    public static class RegisterMissingTypeConverters
    {
        /// <summary>
        /// A flag indication if the registration was successful.
        /// </summary>
        private static bool _registered;

        /// <summary>
        /// Registers the missing type converters.
        /// </summary>
        public static void Register()
        {
            if (_registered)
                return;

            TypeDescriptor.AddAttributes(typeof(BitmapSource), new TypeConverterAttribute(typeof(BitmapSourceTypeConverter)));

            _registered = true;
        }
    }
}
#endregion

#region XAMLMarkupExtensions_Base

namespace XAMLMarkupExtensions.Base
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    #endregion

    /// <summary>
    /// This class stores information about a markup extension target.
    /// </summary>
    public class TargetInfo
    {
        /// <summary>
        /// Gets the target object.
        /// </summary>
        public object TargetObject { get; private set; }

        /// <summary>
        /// Gets the target property.
        /// </summary>
        public object TargetProperty { get; private set; }

        /// <summary>
        /// Gets the target property type.
        /// </summary>
        public Type TargetPropertyType { get; private set; }

        /// <summary>
        /// Gets the target property index.
        /// </summary>
        public int TargetPropertyIndex { get; private set; }

        /// <summary>
        /// True, if the target object is a DependencyObject.
        /// </summary>
        public bool IsDependencyObject { get { return TargetObject is DependencyObject; } }

        /// <summary>
        /// True, if the target object is an endpoint (not another nested markup extension).
        /// </summary>
        public bool IsEndpoint { get { return !(TargetObject is INestedMarkupExtension); } }

        /// <summary>
        /// Determines, whether both objects are equal.
        /// </summary>
        /// <param name="obj">An object of type TargetInfo.</param>
        /// <returns>True, if both are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj is TargetInfo ti)
            {
                if (ti.TargetObject != this.TargetObject)
                    return false;
                if (ti.TargetProperty != this.TargetProperty)
                    return false;
                if (ti.TargetPropertyIndex != this.TargetPropertyIndex)
                    return false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function.
        /// </summary>
        /// <returns>The hash value.</returns>
        public override int GetHashCode()
        {
            // As this class is similar to a Tuple<T1, T2, T3> (the property type is redundant),
            // we take this as a template for the hash generation.
            return Tuple.Create<object, object, int>(this.TargetObject, this.TargetProperty, this.TargetPropertyIndex).GetHashCode();
        }

        /// <summary>
        /// Creates a new TargetInfo instance.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="targetPropertyType">The target property type.</param>
        /// <param name="targetPropertyIndex">The target property index.</param>
        public TargetInfo(object targetObject, object targetProperty, Type targetPropertyType, int targetPropertyIndex)
        {
            this.TargetObject = targetObject;
            this.TargetProperty = targetProperty;
            this.TargetPropertyType = targetPropertyType;
            this.TargetPropertyIndex = targetPropertyIndex;
        }
    }
}

namespace XAMLMarkupExtensions.Base
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Windows;
    #endregion

    /// <summary>
    /// Markup extensions that implement this interface shall be able to return their target objects.
    /// They should also implement a SetNewValue function that properly set the value to all their targets with their own modification of the value.
    /// </summary>
    public interface INestedMarkupExtension
    {
        /// <summary>
        /// Get the paths to all target properties through the nesting hierarchy.
        /// </summary>
        /// <returns>A list of combinations of property types and the corresponsing stacks that resemble the paths to the properties.</returns>
        List<TargetPath> GetTargetPropertyPaths();

        /// <summary>
        /// Trigger the update of the target(s).
        /// </summary>
        /// <param name="targetPath">A specific path to follow or null for all targets.</param>
        /// <returns>The output of the path at the endpoint.</returns>
        object UpdateNewValue(TargetPath targetPath);

        /// <summary>
        /// Format the output of the markup extension.
        /// </summary>
        /// <param name="endpoint">Information about the endpoint.</param>
        /// <param name="info">Information about the target.</param>
        /// <returns>The output of this extension for the given endpoint and target.</returns>
        object FormatOutput(TargetInfo endpoint, TargetInfo info);

        /// <summary>
        /// Check, if the given target is connected to this markup extension.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <returns>True, if a connection exits.</returns>
        bool IsConnected(TargetInfo info);
    }
}

namespace XAMLMarkupExtensions.Base
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    #endregion

    /// <summary>
    /// This class helps tracking the path to a specific endpoint.
    /// </summary>
    public class TargetPath
    {
        /// <summary>
        /// The path to the endpoint.
        /// </summary>
        private Stack<TargetInfo> Path { get; set; }
        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        public TargetInfo EndPoint { get; private set; }

        /// <summary>
        /// Add another step to the path.
        /// </summary>
        /// <param name="info">The TargetInfo object of the step.</param>
        public void AddStep(TargetInfo info) { Path.Push(info); }

        /// <summary>
        /// Get the next step and remove it from the path.
        /// </summary>
        /// <returns>The next steps TargetInfo.</returns>
        public TargetInfo GetNextStep() { return (Path.Count > 0) ? Path.Pop() : EndPoint; }

        /// <summary>
        /// Get the next step.
        /// </summary>
        /// <returns>The next steps TargetInfo.</returns>
        public TargetInfo ShowNextStep() { return (Path.Count > 0) ? Path.Peek() : EndPoint; }

        /// <summary>
        /// Creates a new TargetPath instance.
        /// </summary>
        /// <param name="endPoint">The endpoints TargetInfo of the path.</param>
        public TargetPath(TargetInfo endPoint)
        {
            if (!endPoint.IsEndpoint)
                throw new ArgumentException("A path endpoint cannot be another INestedMarkupExtension.");

            EndPoint = endPoint;
            Path = new Stack<TargetInfo>();
        }
    }
}

namespace XAMLMarkupExtensions.Base
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    #endregion

    /// <summary>
    /// A class that helps listening to changes on the Parent property of FrameworkElement objects.
    /// </summary>
    public class ParentChangedNotifier : DependencyObject, IDisposable
    {
        #region Parent property
        /// <summary>
        /// An attached property that will take over control of change notification.
        /// </summary>
        public static DependencyProperty ParentProperty = DependencyProperty.RegisterAttached("Parent", typeof(DependencyObject), typeof(ParentChangedNotifier), new PropertyMetadata(ParentChanged));

        /// <summary>
        /// Get method for the attached property.
        /// </summary>
        /// <param name="element">The target FrameworkElement object.</param>
        /// <returns>The target's parent FrameworkElement object.</returns>
        public static FrameworkElement GetParent(FrameworkElement element)
        {
            return element.GetValueSync<FrameworkElement>(ParentProperty);
        }

        /// <summary>
        /// Set method for the attached property.
        /// </summary>
        /// <param name="element">The target FrameworkElement object.</param>
        /// <param name="value">The target's parent FrameworkElement object.</param>
        public static void SetParent(FrameworkElement element, FrameworkElement value)
        {
            element.SetValueSync(ParentProperty, value);
        }
        #endregion

        #region ParentChanged callback
        /// <summary>
        /// The callback for changes of the attached Parent property.
        /// </summary>
        /// <param name="obj">The sender.</param>
        /// <param name="args">The argument.</param>
        private static void ParentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is FrameworkElement notifier)
            {
                var weakNotifier = OnParentChangedList.Keys.SingleOrDefault(x => x.IsAlive && ReferenceEquals(x.Target, notifier));

                if (weakNotifier != null)
                {
                    var list = new List<Action>(OnParentChangedList[weakNotifier]);
                    foreach (var OnParentChanged in list)
                        OnParentChanged();
                    list.Clear();
                }
            }
        }
        #endregion

        /// <summary>
        /// A static list of actions that should be performed on parent change events.
        /// <para>- Entries are added by each call of the constructor.</para>
        /// <para>- All elements are called by the parent changed callback with the particular sender as the key.</para>
        /// </summary>
        private static Dictionary<WeakReference, List<Action>> OnParentChangedList =
            new Dictionary<WeakReference, List<Action>>();

        /// <summary>
        /// The element this notifier is bound to. Needed to release the binding and Action entry.
        /// </summary>
        private WeakReference element = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="element">The element whose Parent property should be listened to.</param>
        /// <param name="onParentChanged">The action that will be performed upon change events.</param>
        public ParentChangedNotifier(FrameworkElement element, Action onParentChanged)
        {
            this.element = new WeakReference(element);

            if (onParentChanged != null)
            {
                if (!OnParentChangedList.ContainsKey(this.element))
                {
                    var foundOne = false;

                    foreach (var key in OnParentChangedList.Keys)
                    {
                        if (ReferenceEquals(key.Target, element))
                        {
                            this.element = key;
                            foundOne = true;
                            break;
                        }
                    }

                    if (!foundOne)
                        OnParentChangedList.Add(this.element, new List<Action>());
                }

                OnParentChangedList[this.element].Add(onParentChanged);
            }

            if (element.CheckAccess())
                SetBinding();
            else
                element.Dispatcher.Invoke(new Action(SetBinding));
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~ParentChangedNotifier()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes all used resources of the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose resources.
        /// </summary>
        /// <param name="isDisposing">
        /// <see langword="true" /> if calls from Dispose() method.
        /// <see langword="false" /> if calls from finalizer.
        /// </param>
        protected virtual void Dispose(bool isDisposing)
        {
            var weakElement = element;
            var weakElementReference = weakElement.Target;

            if (OnParentChangedList.ContainsKey(weakElement))
            {
                var list = OnParentChangedList[weakElement];
                list.Clear();
                OnParentChangedList.Remove(weakElement);
            }

            if (isDisposing)
            {
                if (weakElementReference == null || !weakElement.IsAlive)
                    return;

                try
                {
                    ((FrameworkElement)weakElementReference).ClearValue(ParentProperty);
                }
                finally
                {
                    element = null;
                }
            }
        }

        private void SetBinding()
        {
            var binding = new Binding("Parent")
            {
                RelativeSource = new RelativeSource()
                {
                    Mode = RelativeSourceMode.FindAncestor,
                    AncestorType = typeof(FrameworkElement)
                }
            };
            BindingOperations.SetBinding((FrameworkElement)element.Target, ParentProperty, binding);
        }
    }
}

namespace XAMLMarkupExtensions.Base
{
    #region Usings
    using System;
    using System.Windows;
    #endregion

    /// <summary>
    /// Extension methods for dependency objects.
    /// </summary>
    public static class DependencyObjectHelper
    {
        /// <summary>
        /// Gets the value thread-safe.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="property">The property.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The value.</returns>
        public static T GetValueSync<T>(this DependencyObject obj, DependencyProperty property)
        {
            if (obj.CheckAccess())
                return (T)obj.GetValue(property);
            else
                return (T)obj.Dispatcher.Invoke(new Func<object>(() => obj.GetValue(property)));
        }

        /// <summary>
        /// Sets the value thread-safe.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        public static void SetValueSync<T>(this DependencyObject obj, DependencyProperty property, T value)
        {
            if (obj.CheckAccess())
                obj.SetValue(property, value);
            else
                obj.Dispatcher.Invoke(new Action(() => obj.SetValue(property, value)));
        }
    }
}

namespace XAMLMarkupExtensions.Base
{
    #region Uses
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Controls;
    using System.Xaml;
    #endregion

    /// <summary>
    /// This class walks up the tree of markup extensions to support nesting.
    /// Based on <see href="https://github.com/SeriousM/WPFLocalizationExtension"/>
    /// </summary>
    [MarkupExtensionReturnType(typeof(object))]
    public abstract class NestedMarkupExtension : MarkupExtension, INestedMarkupExtension, IDisposable, IObjectDependency
    {
        /// <summary>
        /// Holds the collection of assigned dependency objects
        /// Instead of a single reference, a list is used, if this extension is applied to multiple instances.
        /// </summary>
        private readonly TargetObjectsList targetObjects = new TargetObjectsList();

        /// <summary>
        /// Holds the markup extensions root object hash code.
        /// </summary>
        private int rootObjectHashCode;

        /// <summary>
        /// Get the target objects and properties.
        /// </summary>
        /// <returns>A list of target objects.</returns>
        private List<TargetInfo> GetTargetObjectsAndProperties()
        {
            var result = targetObjects.GetTargetInfos().ToList();
            targetObjects.ClearDeadReferences();

            return result;
        }

        /// <summary>
        /// Get the paths to all target properties through the nesting hierarchy.
        /// </summary>
        /// <returns>A list of paths to the properties.</returns>
        public List<TargetPath> GetTargetPropertyPaths()
        {
            var list = new List<TargetPath>();
            var objList = GetTargetObjectsAndProperties();

            foreach (var info in objList)
            {
                if (info.IsEndpoint)
                {
                    TargetPath path = new TargetPath(info);
                    list.Add(path);
                }
                else
                {
                    foreach (var path in ((INestedMarkupExtension)info.TargetObject).GetTargetPropertyPaths())
                    {
                        // Push the ITargetMarkupExtension
                        path.AddStep(info);
                        // Add the tuple to the list
                        list.Add(path);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// An action that is called when the first target is bound.
        /// </summary>
        [Obsolete("Override 'OnFirstTargetAdded' method instead")]
        protected Action OnFirstTarget;

        /// <summary>
        /// An action that is called when the first target is bound.
        /// </summary>
        protected virtual void OnFirstTargetAdded()
        {
#pragma warning disable CS0618
            OnFirstTarget?.Invoke();
#pragma warning restore CS0618
        }

        /// <summary>
        /// An action that is called when the last target is unbound.
        /// </summary>
        /// <remarks>
        /// This method can be called without <see cref="OnFirstTargetAdded" /> before
        /// if extension disposed without adding targets.
        /// </remarks>
        protected virtual void OnLastTargetRemoved()
        {
            EndpointReachedEvent.RemoveListener(rootObjectHashCode, this);
        }

        /// <summary>
        /// This function must be implemented by all child classes.
        /// It shall return the properly prepared output of the markup extension.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <param name="endPoint">Information about the endpoint.</param>
        public abstract object FormatOutput(TargetInfo endPoint, TargetInfo info);

        /// <summary>
        /// Check, if the given target is connected to this markup extension.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <returns>True, if a connection exits.</returns>
        public bool IsConnected(TargetInfo info)
        {
            return targetObjects.IsConnected(info);
        }

        /// <summary>
        /// Override this function, if (and only if) additional information is needed from the <see cref="IServiceProvider"/> instance that is passed to <see cref="NestedMarkupExtension.ProvideValue"/>.
        /// </summary>
        /// <param name="serviceProvider">A service provider.</param>
        protected virtual void OnServiceProviderChanged(IServiceProvider serviceProvider)
        {
            // Do nothing in the base class.
        }

        /// <summary>
        /// The ProvideValue method of the <see cref="MarkupExtension"/> base class.
        /// </summary>
        /// <param name="serviceProvider">A service provider.</param>
        /// <returns>The value of the extension, or this if something gone wrong (needed for Templates).</returns>
        public sealed override object ProvideValue(IServiceProvider serviceProvider)
        {
            // If the service provider is null, return this
            if (serviceProvider == null)
                return this;

            OnServiceProviderChanged(serviceProvider);

            // Try to cast the passed serviceProvider to a IProvideValueTarget
            // If the cast fails, return this
            if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service))
                return this;

            // Try to cast the passed serviceProvider to a IRootObjectProvider and if the cast fails return null
            if (!(serviceProvider.GetService(typeof(IRootObjectProvider)) is IRootObjectProvider rootObject))
            {
                rootObjectHashCode = 0;
            }
            else
            {
                if (rootObject.RootObject == null)
                {
                    rootObjectHashCode = 0;
                }
                else
                {
                    rootObjectHashCode = rootObject.RootObject.GetHashCode();

                    // We only sign up once to the Window Closed event to clear the listeners list of root object.
                    if (!EndpointReachedEvent.ContainsRootObjectHash(rootObjectHashCode))
                    {
                        if (rootObject.RootObject is Window window)
                        {
                            window.Closed += delegate (object sender, EventArgs args) { EndpointReachedEvent.ClearListenersForRootObject(rootObjectHashCode); };
                        }
                        else if (rootObject.RootObject is FrameworkElement frameworkElement)
                        {
                            void frameworkElementUnloadedHandler(object sender, RoutedEventArgs args)
                            {
                                frameworkElement.Unloaded -= frameworkElementUnloadedHandler;
                                EndpointReachedEvent.ClearListenersForRootObject(rootObjectHashCode);
                            }

                            frameworkElement.Unloaded += frameworkElementUnloadedHandler;
                        }
                    }
                }
            }

            // Declare a target object and property
            TargetInfo endPoint = null;
            object targetObject = service.TargetObject;
            object targetProperty = service.TargetProperty;
            int targetPropertyIndex = -1;
            Type targetPropertyType = null;
            object overriddenResult = null;

            // If target object is a Binding and extension set at Value.
            // Return Binding which work with BindingValueProvider.
            if (targetObject is Setter setter && targetProperty is PropertyInfo spi && spi.Name == nameof(Setter.Value))
            {
                targetObject = new BindingValueProvider();
                targetProperty = BindingValueProvider.ValueProperty;
                // If Setter.TargetName is used then Setter.Property would be null.
                // We cannot define which type used in this case.
                targetPropertyType = setter.Property?.PropertyType ?? typeof(object);

                overriddenResult = new Binding(nameof(BindingValueProvider.Value))
                {
                    Source = targetObject,
                    Mode = BindingMode.TwoWay
                };
            }
            // If target object is a Binding and extension set at Source.
            // Reconfigure existing binding and return BindingValueProvider.
            else if (targetObject is Binding binding && targetProperty is PropertyInfo bpi && bpi.Name == nameof(Binding.Source))
            {
                binding.Path = new PropertyPath(nameof(BindingValueProvider.Value));
                binding.Mode = BindingMode.TwoWay;

                targetObject = new BindingValueProvider();
                targetProperty = BindingValueProvider.ValueProperty;
                overriddenResult = targetObject;
            }

            // First, check if the service provider is of type SimpleProvideValueServiceProvider
            //      -> If yes, get the target property type and index.
            // Check if the service.TargetProperty is a DependencyProperty or a PropertyInfo and set the type info
            if (serviceProvider is SimpleProvideValueServiceProvider spvServiceProvider)
            {
                targetPropertyType = spvServiceProvider.TargetPropertyType;
                targetPropertyIndex = spvServiceProvider.TargetPropertyIndex;
                endPoint = spvServiceProvider.EndPoint;
            }
            else if (targetPropertyType == null)
            {
                if (targetProperty is PropertyInfo pi)
                {
                    targetPropertyType = pi.PropertyType;

                    // Kick out indexers.
                    if (pi.GetIndexParameters().Any())
                        throw new InvalidOperationException("Indexers are not supported!");
                }
                else if (targetProperty is DependencyProperty dp)
                {
                    targetPropertyType = dp.PropertyType;
                }
                else
                    return this;
            }

            // If the service.TargetObject is System.Windows.SharedDp (= not a DependencyObject and not a PropertyInfo), we return "this".
            // The SharedDp will call this instance later again.
            if (!(targetObject is DependencyObject) && !(targetProperty is PropertyInfo))
                return this;

            // If the target object is a DictionaryEntry we presumably are facing a resource scenario.
            // We will be called again later with the proper target.
            if (targetObject is DictionaryEntry)
                return null;

            // Search for the target in the target object list
            WeakReference wr = targetObjects.TryFindKey(targetObject);
            if (wr == null)
            {
                // If it's the first object, call the appropriate action
                if (targetObjects.Count == 0)
                    OnFirstTargetAdded();

                // Add to the target object list
                wr = targetObjects.AddTargetObject(targetObject);

                // Add this extension to the ObjectDependencyManager to ensure the lifetime along with the target object
                ObjectDependencyManager.AddObjectDependency(wr, this);
            }

            // Finally, add the target prop and info to the list of this WeakReference
            targetObjects.AddTargetObjectProperty(wr, targetProperty, targetPropertyType, targetPropertyIndex);

            // Sign up to the EndpointReachedEvent only if the markup extension wants to do so.
            EndpointReachedEvent.AddListener(rootObjectHashCode, this);

            // Create the target info
            TargetInfo info = new TargetInfo(targetObject, targetProperty, targetPropertyType, targetPropertyIndex);

            // Return the result of FormatOutput
            object result = null;

            if (info.IsEndpoint)
            {
                var args = new EndpointReachedEventArgs(info);
                EndpointReachedEvent.Invoke(rootObjectHashCode, this, args);
                result = args.EndpointValue;
            }
            else
                result = FormatOutput(endPoint, info);

            if (overriddenResult != null)
                return overriddenResult;

            // Check type
            if (typeof(IList).IsAssignableFrom(targetPropertyType))
                return result;
            else if (result != null && targetPropertyType.IsInstanceOfType(result))
                return result;

            // Finally, if nothing was there, return null or default
            if (targetPropertyType.IsValueType)
                return Activator.CreateInstance(targetPropertyType);
            else
                return null;
        }

        /// <summary>
        /// Set the new value for all targets.
        /// </summary>
        protected void UpdateNewValue()
        {
            UpdateNewValue(null);
        }

        /// <summary>
        /// Trigger the update of the target(s).
        /// </summary>
        /// <param name="targetPath">A specific path to follow or null for all targets.</param>
        /// <returns>The output of the path at the endpoint.</returns>
        public object UpdateNewValue(TargetPath targetPath)
        {
            if (targetPath == null)
            {
                // No path supplied - send it to all targets.
                foreach (var path in GetTargetPropertyPaths())
                {
                    // Call yourself and supply the path to follow.
                    UpdateNewValue(path);
                }
            }
            else
            {
                // Get the info of the next step.
                TargetInfo info = targetPath.GetNextStep();

                // Get the own formatted output.
                object output = FormatOutput(targetPath.EndPoint, info);

                // Set the property of the target to the new value.
                SetPropertyValue(output, info, false);

                // Have we reached the endpoint?
                // If not, call the UpdateNewValue function of the next ITargetMarkupExtension
                if (info.IsEndpoint)
                    return output;
                else
                    return ((INestedMarkupExtension)info.TargetObject).UpdateNewValue(targetPath);
            }

            return null;
        }

        /// <summary>
        /// Sets the value of a property of type PropertyInfo or DependencyProperty.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="info">The target information.</param>
        /// <param name="forceNull">Determines, whether null values should be written.</param>
        public static void SetPropertyValue(object value, TargetInfo info, bool forceNull)
        {
            if ((value == null) && !forceNull)
                return;

            if (info.TargetObject is DependencyObject depObject)
            {
                if (depObject.IsSealed)
                    return;
            }

            // Anyway, a value type cannot receive null values...
            if (info.TargetPropertyType.IsValueType && (value == null))
                value = Activator.CreateInstance(info.TargetPropertyType);

            // Set the value.
            if (info.TargetProperty is DependencyProperty dp)
                ((DependencyObject)info.TargetObject).SetValueSync(dp, value);
            else
            {
                PropertyInfo pi = (PropertyInfo)info.TargetProperty;

                if (typeof(IList).IsAssignableFrom(info.TargetPropertyType) && (value != null) && !info.TargetPropertyType.IsAssignableFrom(value.GetType()))
                {
                    // A list, a list - get it and set the value directly via its index.
                    if (info.TargetPropertyIndex >= 0)
                    {
                        IList list = (IList)pi.GetValue(info.TargetObject, null);
                        if (list.Count > info.TargetPropertyIndex)
                            list[info.TargetPropertyIndex] = value;
                    }
                    return;
                }

                pi.SetValue(info.TargetObject, value, null);
            }
        }

        /// <summary>
        /// Gets the value of a property of type PropertyInfo or DependencyProperty.
        /// </summary>
        /// <param name="info">The target information.</param>
        /// <returns>The value.</returns>
        public static object GetPropertyValue(TargetInfo info)
        {
            if (info.TargetProperty is DependencyProperty tP)
            {
                if (info.TargetObject is DependencyObject tO)
                    return tO.GetValueSync<object>(tP);
                else
                    return null;
            }
            else if (info.TargetProperty is PropertyInfo pi)
            {
                if (info.TargetPropertyIndex >= 0)
                {
                    if (typeof(IList).IsAssignableFrom(info.TargetPropertyType))
                    {
                        IList list = (IList)pi.GetValue(info.TargetObject, null);
                        if (list.Count > info.TargetPropertyIndex)
                            return list[info.TargetPropertyIndex];
                    }
                }

                return pi.GetValue(info.TargetObject, null);
            }

            return null;
        }

        /// <summary>
        /// Safely get the value of a property that might be set by a further MarkupExtension.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="value">The value supplied by the set accessor of the property.</param>
        /// <param name="property">The property information.</param>
        /// <param name="index">The index of the indexed property, if applicable.</param>
        /// <param name="endPoint">An optional endpoint information.</param>
        /// <param name="service">An optional serviceProvider information.</param>
        /// <returns>The value or default.</returns>
        protected T GetValue<T>(object value, PropertyInfo property, int index, TargetInfo endPoint = null, IServiceProvider service = null)
        {
            // Simple case: value is of same type
            if (value is T t && !(value is MarkupExtension))
                return t;

            // No property supplied
            if (property == null)
                return default;

            // Is value of type MarkupExtension?
            if (value is MarkupExtension me)
            {
                object result = me.ProvideValue(new SimpleProvideValueServiceProvider(this, property, property.PropertyType, index, endPoint, service));
                if (result != null)
                    return (T)result;
            }

            // Default return path.
            return default;
        }

        /// <summary>
        /// This method must return true, if an update shall be executed when the given endpoint is reached.
        /// This method is called each time an endpoint is reached.
        /// </summary>
        /// <param name="endpoint">Information on the specific endpoint.</param>
        /// <returns>True, if an update of the path to this endpoint shall be performed.</returns>
        protected abstract bool UpdateOnEndpoint(TargetInfo endpoint);

        /// <summary>
        /// Get the path to a specific endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint info.</param>
        /// <returns>The path to the endpoint.</returns>
        protected TargetPath GetPathToEndpoint(TargetInfo endpoint)
        {
            // If endpoint is connected - return empty path.
            if (IsConnected(endpoint))
                return new TargetPath(endpoint);

            // Else try find endpoint in nested targets.
            foreach (var nestedTargetInfo in targetObjects.GetNestedTargetInfos())
            {
                // If nested target inherit NestedMarkupExtension - we can fast get path of endpoint using current method.
                // Otherwise use slow search by getting all paths.
                var interfaceInheritor = (INestedMarkupExtension)nestedTargetInfo.TargetObject;
                var path = nestedTargetInfo.TargetObject is NestedMarkupExtension classInheritor
                    ? classInheritor.GetPathToEndpoint(endpoint)
                    : interfaceInheritor.GetTargetPropertyPaths().FirstOrDefault(pp => pp.EndPoint.TargetObject == endpoint.TargetObject);
                if (path != null)
                {
                    targetObjects.ClearDeadReferences();

                    path.AddStep(nestedTargetInfo);
                    return path;
                }
            }

            targetObjects.ClearDeadReferences();
            return null;
        }

        /// <summary>
        /// Checks the existance of the given object in the target endpoint list.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>True, if the extension nesting tree reaches the given object.</returns>
        protected bool IsEndpointObject(object obj)
        {
            // Check if object contains in current targets.
            if (targetObjects.TryFindKey(obj) != null)
                return true;

            // Else try find object in nested targets.
            foreach (var nestedTargetInfo in targetObjects.GetNestedTargetInfos())
            {
                // If nested target inherit NestedMarkupExtension - we can fast get path of endpoint using current method.
                // Otherwise use slow search by getting all paths.
                var interfaceInheritor = (INestedMarkupExtension)nestedTargetInfo.TargetObject;
                var isEndpoint = nestedTargetInfo.TargetObject is NestedMarkupExtension classInheritor
                    ? classInheritor.IsEndpointObject(obj)
                    : interfaceInheritor.GetTargetPropertyPaths().Any(tpp => tpp.EndPoint.TargetObject == obj);
                if (isEndpoint)
                {
                    targetObjects.ClearDeadReferences();
                    return true;
                }
            }

            targetObjects.ClearDeadReferences();
            return false;
        }

        /// <summary>
        /// An event handler that is called from the static <see cref="EndpointReachedEvent"/> class.
        /// </summary>
        /// <param name="sender">The markup extension that reached an enpoint.</param>
        /// <param name="args">The event args containing the endpoint information.</param>
        private void OnEndpointReached(NestedMarkupExtension sender, EndpointReachedEventArgs args)
        {
            if (args.Handled)
                return;

            if ((this != sender) && !UpdateOnEndpoint(args.Endpoint))
                return;

            var path = GetPathToEndpoint(args.Endpoint);
            if (path == null)
                return;

            args.EndpointValue = UpdateNewValue(path);

            // Removed, because of no use:
            // args.Handled = true;
        }

        /// <summary>
        /// Implements the IDisposable.Dispose function.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose resources.
        /// </summary>
        /// <param name="isDisposing">
        /// <see langword="true" /> if calls from Dispose() method.
        /// <see langword="false" /> if calls from finalizer.
        /// </param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Remove strong reference from ObjectDependencyManager.
                ObjectDependencyManager.CleanUp(this);

                // Clean all targets.
                targetObjects.Clear();
                OnLastTargetRemoved();
            }
        }

        #region IObjectDependency

        /// <inheritdoc />
        void IObjectDependency.OnDependenciesRemoved(IEnumerable<WeakReference> deadDependencies)
        {
            targetObjects.ClearReferences(deadDependencies);

            if (targetObjects.Count == 0)
                OnLastTargetRemoved();
        }

        /// <inheritdoc />
        void IObjectDependency.OnAllDependenciesRemoved()
        {
            targetObjects.Clear();
            OnLastTargetRemoved();
        }

        #endregion

        #region EndpointReachedEvent
        /// <summary>
        /// A static proxy class that handles endpoint reached events for a list of weak references of <see cref="NestedMarkupExtension"/>.
        /// This circumvents the usage of a WeakEventManager while providing a static instance that is capable of firing the event.
        /// </summary>
        internal static class EndpointReachedEvent
        {
            /// <summary>
            /// A dicitonary which contains a list of listeners per unique rootObject hash.
            /// </summary>
            private static readonly Dictionary<int, List<WeakReference>> listeners;
            private static readonly object listenersLock;

            /// <summary>
            /// Fire the event.
            /// </summary>
            /// <param name="rootObjectHashCode"><paramref name="sender"/>s root object hash code.</param>
            /// <param name="sender">The markup extension that reached an end point.</param>
            /// <param name="args">The event args containing the endpoint information.</param>
            internal static void Invoke(int rootObjectHashCode, NestedMarkupExtension sender, EndpointReachedEventArgs args)
            {
                lock (listenersLock)
                {
                    // Do nothing if we don't have this root object hash.
                    if (!listeners.ContainsKey(rootObjectHashCode))
                        return;

                    foreach (var wr in listeners[rootObjectHashCode].ToList())
                    {
                        var targetReference = wr.Target;
                        if (targetReference is NestedMarkupExtension)
                            ((NestedMarkupExtension)targetReference).OnEndpointReached(sender, args);
                        else
                            listeners[rootObjectHashCode].Remove(wr);
                    }
                }
            }

            /// <summary>
            /// Adds a listener to the inner list of listeners.
            /// </summary>
            /// <param name="rootObjectHashCode"><paramref name="listener"/>s root object hash code.</param>
            /// <param name="listener">The listener to add.</param>
            internal static void AddListener(int rootObjectHashCode, NestedMarkupExtension listener)
            {
                if (listener == null)
                    return;

                lock (listenersLock)
                {
                    // Do we have a listeners list for this root object yet, if not add it.
                    if (!listeners.ContainsKey(rootObjectHashCode))
                    {
                        listeners[rootObjectHashCode] = new List<WeakReference>();
                    }

                    // Check, if this listener already was added.
                    foreach (var wr in listeners[rootObjectHashCode].ToList())
                    {
                        var targetReference = wr.Target;
                        if (targetReference == null)
                            listeners[rootObjectHashCode].Remove(wr);
                        else if (targetReference == listener)
                            return;
                        else
                        {
                            var existing = (NestedMarkupExtension)targetReference;
                            var targets = existing.GetTargetObjectsAndProperties();

                            foreach (var target in targets)
                            {
                                if (listener.IsConnected(target))
                                {
                                    listeners[rootObjectHashCode].Remove(wr);
                                    break;
                                }
                            }
                        }
                    }

                    // Add it now.
                    listeners[rootObjectHashCode].Add(new WeakReference(listener));
                }
            }

            /// <summary>
            /// Clears the listeners list for the given root object hash code <paramref name="rootObjectHashCode"/>.
            /// </summary>
            internal static void ClearListenersForRootObject(int rootObjectHashCode)
            {
                lock (listenersLock)
                {
                    if (!listeners.ContainsKey(rootObjectHashCode))
                        return;

                    listeners.Remove(rootObjectHashCode);
                }
            }

            /// <summary>
            /// Returns true if the given <paramref name="rootObjectHashCode"/> is already added, false otherwise.
            /// </summary>
            /// <param name="rootObjectHashCode">Root object hash code to check.</param>
            /// <returns>Returns true if the given <paramref name="rootObjectHashCode"/> is already added, false otherwise.</returns>
            internal static bool ContainsRootObjectHash(int rootObjectHashCode)
            {
                return listeners.ContainsKey(rootObjectHashCode);
            }

            /// <summary>
            /// Removes a listener from the inner list of listeners.
            /// </summary>
            /// <param name="rootObjectHashCode"><paramref name="listener"/>s root object hash code.</param>
            /// <param name="listener">The listener to remove.</param>
            internal static void RemoveListener(int rootObjectHashCode, NestedMarkupExtension listener)
            {
                if (listener == null)
                    return;

                lock (listenersLock)
                {
                    if (!listeners.ContainsKey(rootObjectHashCode))
                        return;

                    foreach (var wr in listeners[rootObjectHashCode].ToList())
                    {
                        var targetReference = wr.Target;
                        if (targetReference == null)
                            listeners[rootObjectHashCode].Remove(wr);
                        else if ((NestedMarkupExtension)targetReference == listener)
                            listeners[rootObjectHashCode].Remove(wr);
                    }

                    if (listeners[rootObjectHashCode].Count == 0)
                        listeners.Remove(rootObjectHashCode);
                }
            }

            /// <summary>
            /// An empty static constructor to prevent the class from being marked as beforefieldinit.
            /// </summary>
            static EndpointReachedEvent()
            {
                listeners = new Dictionary<int, List<WeakReference>>();
                listenersLock = new object();
            }
        }
        #endregion
    }
}

namespace XAMLMarkupExtensions.Base
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    #endregion

    /// <summary>
    /// The event arguments of the EndpointReached event.
    /// </summary>
    public class EndpointReachedEventArgs : EventArgs
    {
        /// <summary>
        /// The endpoint.
        /// </summary>
        public TargetInfo Endpoint { get; private set; }

        /// <summary>
        /// Get or set the value that will be stored to the endpoint.
        /// </summary>
        public object EndpointValue { get; set; }

        /// <summary>
        /// Get or set a flag indicating that the event was handled.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Creates a new <see cref="EndpointReachedEventArgs"/> object.
        /// </summary>
        /// <param name="endPoint">The endpoint.</param>
        public EndpointReachedEventArgs(TargetInfo endPoint)
        {
            this.Endpoint = endPoint;
            this.EndpointValue = null;
        }
    }
}

namespace XAMLMarkupExtensions.Base
{
    #region Uses
    using System;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// Interface for object which store at <see cref="ObjectDependencyManager" />.
    /// </summary>
    public interface IObjectDependency
    {
        /// <summary>
        /// Notify that some of dependencies are dead.
        /// </summary>
        void OnDependenciesRemoved(IEnumerable<WeakReference> deadDependencies);

        /// <summary>
        /// Notify that all dependencies are dead.
        /// </summary>
        void OnAllDependenciesRemoved();
    }
}

namespace XAMLMarkupExtensions.Base
{
    #region Usings
    using System;
    using System.Windows.Markup;
    #endregion

    /// <summary>
    /// This class implements the interfaces IServiceProvider and IProvideValueTarget for ProvideValue calls on markup extensions.
    /// </summary>
    public class SimpleProvideValueServiceProvider : IServiceProvider, IProvideValueTarget
    {
        #region IServiceProvider
        /// <summary>
        /// Return the requested service.
        /// </summary>
        /// <param name="service">The type of the service.</param>
        /// <returns>The service (this, if service ist IProvideValueTarget, otherwise null).</returns>
        public object GetService(Type service)
        {
            // This class only implements the IProvideValueTarget service.
            if (service == typeof(IProvideValueTarget))
                return this;

            return ServiceProvider?.GetService(service);
        }
        #endregion

        #region Properties

        #region IProvideValueTarget
        /// <summary>
        /// The target object.
        /// </summary>
        public object TargetObject { get; private set; }

        /// <summary>
        /// The target property.
        /// </summary>
        public object TargetProperty { get; private set; }
        #endregion

        /// <summary>
        /// The target property type.
        /// </summary>
        public Type TargetPropertyType { get; private set; }

        /// <summary>
        /// The target property index.
        /// </summary>
        public int TargetPropertyIndex { get; private set; }

        /// <summary>
        /// An optional endpoint information.
        /// </summary>
        public TargetInfo EndPoint { get; private set; }

        /// <summary>
        /// An optional IServiceProvider information.
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }
        #endregion

        #region Construtors
        /// <summary>
        /// Create a new instance of a SimpleProvideValueServiceProvider.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="targetPropertyType">The target property type.</param>
        /// <param name="targetPropertyIndex">The target property index.</param>
        /// <param name="endPoint">An optional endpoint information.</param>
        /// <param name="serviceProvider">An optional endpoint information.</param>
        public SimpleProvideValueServiceProvider(object targetObject, object targetProperty, Type targetPropertyType, int targetPropertyIndex, TargetInfo endPoint = null, IServiceProvider serviceProvider = null)
        {
            TargetObject = targetObject;
            TargetProperty = targetProperty;
            TargetPropertyType = targetPropertyType;
            TargetPropertyIndex = targetPropertyIndex;
            EndPoint = endPoint;
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Create a new instance of a SimpleProvideValueServiceProvider.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <param name="endPoint">An optional endpoint information.</param>
        /// <param name="serviceProvider">An optional endpoint information.</param>
        public SimpleProvideValueServiceProvider(TargetInfo info, TargetInfo endPoint = null, IServiceProvider serviceProvider = null) :
            this(info.TargetObject, info.TargetProperty, info.TargetPropertyType, info.TargetPropertyIndex, endPoint, serviceProvider)
        {
        }
        #endregion
    }
}

namespace XAMLMarkupExtensions.Base
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    #endregion

    /// <summary>
    /// Defines a collection of assigned dependency objects.
    /// Instead of a single reference, a list is used, if this extension is applied to multiple instances.
    /// </summary>
    internal class TargetObjectsList
    {
        /// <summary>
        /// Holds the collection of assigned dependency objects as WeakReferences and its property metadata.
        /// </summary>
        private readonly Dictionary<WeakReference, TargetObjectValue> targetObjects = new Dictionary<WeakReference, TargetObjectValue>();

        /// <summary>
        /// Holds hash codes of each target in <see cref="targetObjects" />.
        /// Allows fast find key in <see cref="targetObjects" /> by object.
        /// <see cref="List{WeakReference}" /> is necessary, because different objects can has same hashcode.
        /// </summary>
        private readonly Dictionary<int, List<WeakReference>> hashCodeTargetObjects = new Dictionary<int, List<WeakReference>>();

        /// <summary>
        /// Holds the collection of assigned dependency objects which implements <see cref="INestedMarkupExtension" />.
        /// This collection is subset of <see cref="targetObjects" />.
        /// </summary>
        private readonly HashSet<WeakReference> nestedTargetObjects = new HashSet<WeakReference>();

        /// <summary>
        /// Holds the collection of weak references, which target already collected by GC.
        /// This references should be removed from other collections.
        /// </summary>
        private readonly List<WeakReference> deadTargets = new List<WeakReference>();

        /// <summary>
        /// Gets the count of assigned dependency objects.
        /// </summary>
        public int Count => targetObjects.Count;

        /// <summary>
        /// Add new target to internal list.
        /// </summary>
        /// <param name="targetObject">the new target.</param>
        /// <returns>Weak reference of target object.</returns>
        public WeakReference AddTargetObject(object targetObject)
        {
            var wr = new WeakReference(targetObject);
            var targetObjectHashCode = targetObject.GetHashCode();
            targetObjects.Add(wr, new TargetObjectValue(targetObjectHashCode));

            if (!hashCodeTargetObjects.ContainsKey(targetObjectHashCode))
                hashCodeTargetObjects.Add(targetObjectHashCode, new List<WeakReference>());
            hashCodeTargetObjects[targetObjectHashCode].Add(wr);

            if (targetObject is INestedMarkupExtension)
                nestedTargetObjects.Add(wr);

            return wr;
        }

        /// <summary>
        /// Add property info of target object.
        /// </summary>
        /// <param name="targetObjectWeakReference">The weak reference of target object.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="targetPropertyType">The type of target property.</param>
        /// <param name="targetPropertyIndex">The index of target property.</param>
        public void AddTargetObjectProperty(WeakReference targetObjectWeakReference, object targetProperty, Type targetPropertyType, int targetPropertyIndex)
        {
            var targetPropertyInfo = new TargetPropertyInfo(targetProperty, targetPropertyType, targetPropertyIndex);
            var targetObjectProperties = targetObjects[targetObjectWeakReference].TargetProperties;

            if (!targetObjectProperties.Contains(targetPropertyInfo))
                targetObjectProperties.Add(targetPropertyInfo);
        }

        /// <summary>
        /// Check if target object and its property contains in list.
        /// </summary>
        /// <param name="info">Information about the target.</param>
        /// <returns><see langwrod="true" /> if it is contains.</returns>
        public bool IsConnected(TargetInfo info)
        {
            WeakReference wr = TryFindKey(info.TargetObject);
            if (wr == null)
                return false;

            var targetPropertyInfo = new TargetPropertyInfo(info.TargetProperty, info.TargetPropertyType, info.TargetPropertyIndex);
            return targetObjects[wr].TargetProperties.Contains(targetPropertyInfo);
        }

        /// <summary>
        /// Try find weak reference key of target object in list.
        /// </summary>
        /// <param name="targetObject">The searching target object.</param>
        /// <returns>
        /// <see cref="WeakReference" /> of target object if it contains in list.
        /// <see langword="null" /> otherwise.
        /// </returns>
        public WeakReference TryFindKey(object targetObject)
        {
            var hashCode = targetObject.GetHashCode();
            if (!hashCodeTargetObjects.TryGetValue(hashCode, out var weakReferences))
                return null;

            return weakReferences.FirstOrDefault(wr => wr.Target == targetObject);
        }

        /// <summary>
        /// Get information of all target objects.
        /// </summary>
        public IEnumerable<TargetInfo> GetTargetInfos()
        {
            foreach (var target in targetObjects)
            {
                var targetReference = target.Key.Target;
                if (targetReference == null)
                {
                    deadTargets.Add(target.Key);
                    continue;
                }

                foreach (var data in target.Value.TargetProperties)
                {
                    yield return new TargetInfo(targetReference, data.TargetProperty, data.TargetPropertyType, data.TargetPropertyIndex);
                }
            }
        }

        /// <summary>
        /// Get information of all targets objects which implements <see cref="INestedMarkupExtension" />.
        /// </summary>
        public IEnumerable<TargetInfo> GetNestedTargetInfos()
        {
            foreach (var nestedTargetObject in nestedTargetObjects)
            {
                var targetReference = nestedTargetObject.Target;
                if (targetReference == null)
                {
                    deadTargets.Add(nestedTargetObject);
                    continue;
                }

                foreach (var data in targetObjects[nestedTargetObject].TargetProperties)
                {
                    yield return new TargetInfo(targetReference, data.TargetProperty, data.TargetPropertyType, data.TargetPropertyIndex);
                }
            }
        }

        /// <summary>
        /// Clear all references to dead targets.
        /// </summary>
        public void ClearDeadReferences()
        {
            if (!deadTargets.Any())
                return;

            foreach (var deadWeakReference in deadTargets)
            {
                if (!targetObjects.TryGetValue(deadWeakReference, out var targetValue))
                    continue;

                hashCodeTargetObjects[targetValue.TargetObjectHashCode].Remove(deadWeakReference);
                if (!hashCodeTargetObjects[targetValue.TargetObjectHashCode].Any())
                    hashCodeTargetObjects.Remove(targetValue.TargetObjectHashCode);

                targetObjects.Remove(deadWeakReference);
                nestedTargetObjects.Remove(deadWeakReference);
            }

            deadTargets.Clear();
        }

        /// <summary>
        /// Clear specified references.
        /// </summary>
        public void ClearReferences(IEnumerable<WeakReference> references)
        {
            deadTargets.AddRange(references);
            ClearDeadReferences();
        }

        /// <summary>
        /// Clear all targets.
        /// </summary>
        public void Clear()
        {
            targetObjects.Clear();
            hashCodeTargetObjects.Clear();
            nestedTargetObjects.Clear();
            deadTargets.Clear();
        }

        /// <summary>
        /// Information about target object.
        /// </summary>
        private class TargetObjectValue
        {
            /// <summary>
            /// Create new <see cref="TargetObjectValue" /> instance.
            /// </summary>
            /// <param name="targetObjectHashCode">The hashcode of target object.</param>
            public TargetObjectValue(int targetObjectHashCode)
            {
                TargetObjectHashCode = targetObjectHashCode;
                TargetProperties = new HashSet<TargetPropertyInfo>();
            }

            /// <summary>
            /// Hashcode of target object.
            /// </summary>
            public int TargetObjectHashCode { get; }

            /// <summary>
            /// Information about properties of target object.
            /// </summary>
            public HashSet<TargetPropertyInfo> TargetProperties { get; }
        }
    }
}

namespace XAMLMarkupExtensions.Base
{
    using System.ComponentModel;
    using System.Reflection;
    /// <summary>
    /// Special class which use as value provider for bindings.
    /// Some properties are sealing (Setter.Value and Binding.Source) and cannot change, so
    /// if we use such provider, it stay same but it's value free to change.
    /// </summary>
    internal class BindingValueProvider : INotifyPropertyChanged
    {
        /// <summary>
        /// Property info of <see cref="Value" /> property.
        /// </summary>
        public static PropertyInfo ValueProperty = typeof(BindingValueProvider).GetProperty(nameof(Value));

        private object _value;

        /// <summary>
        /// Providing value.
        /// </summary>
        public object Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise <see cref="PropertyChanged" /> event.
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

namespace XAMLMarkupExtensions.Base
{
    #region Uses
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    #endregion

    /// <summary>
    /// This class ensures, that a specific object lives as long a associated object is alive.
    /// Based on: <see href="https://github.com/SeriousM/WPFLocalizationExtension"/>
    /// </summary>
    public static class ObjectDependencyManager
    {
        /// <summary>
        /// This member holds the list of all <see cref="WeakReference"/>s and their appropriate objects.
        /// </summary>
        private static readonly Dictionary<object, HashSet<WeakReference>> internalList;

        /// <summary>
        /// Initializes static members of the <see cref="ObjectDependencyManager"/> class.
        /// Static Constructor. Creates a new instance of
        /// Dictionary(object, <see cref="WeakReference"/>) and set it to the <see cref="internalList"/>.
        /// </summary>
        static ObjectDependencyManager()
        {
            internalList = new Dictionary<object, HashSet<WeakReference>>();
        }

        /// <summary>
        /// This method adds a new object dependency
        /// </summary>
        /// <param name="weakRefDp">The <see cref="WeakReference"/>, which ensures the live cycle of <paramref name="objToHold"/></param>
        /// <param name="objToHold">The object, which should stay alive as long <paramref name="weakRefDp"/> is alive</param>
        /// <returns>
        /// true, if the binding was successfully, otherwise false
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="objToHold"/> cannot be null
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="weakRefDp"/> cannot be null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="objToHold"/> cannot be type of <see cref="WeakReference"/>
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The <see cref="WeakReference"/>.Target cannot be the same as <paramref name="objToHold"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool AddObjectDependency(WeakReference weakRefDp, object objToHold)
        {
            // if the objToHold is null, we cannot handle this afterwards.
            if (objToHold == null)
            {
                throw new ArgumentNullException(nameof(objToHold), "The objToHold cannot be null");
            }

            // if the objToHold is a weakreference, we cannot handle this type afterwards.
            if (objToHold.GetType() == typeof(WeakReference))
            {
                throw new ArgumentException("objToHold cannot be type of WeakReference", nameof(objToHold));
            }

            // if the weakRefDp is null, we cannot handle this afterwards.
            if (weakRefDp == null)
            {
                throw new ArgumentNullException(nameof(weakRefDp), "The weakRefDp cannot be null");
            }

            // if the target of the weakreference is the objToHold, this would be a cycling play.
            if (weakRefDp.Target == objToHold)
            {
                throw new InvalidOperationException("The WeakReference.Target cannot be the same as objToHold");
            }

            // holds the status of registration of the object dependency
            bool itemRegistered = false;

            // check if the objToHold is contained in the internalList.
            if (!internalList.ContainsKey(objToHold))
            {
                // add the objToHold to the internal list.
                HashSet<WeakReference> lst = new HashSet<WeakReference> { weakRefDp };

                internalList.Add(objToHold, lst);

                itemRegistered = true;
            }
            else
            {
                // otherweise, check if the weakRefDp exists and add it if necessary
                HashSet<WeakReference> references = internalList[objToHold];
                if (!references.Contains(weakRefDp))
                {
                    references.Add(weakRefDp);

                    itemRegistered = true;
                }
            }

            // run the clean up to ensure that only objects are watched they are realy still alive
            CleanUp();

            // return the status of the registration
            return itemRegistered;
        }

        /// <summary>
        /// This method cleans up all independent (!<see cref="WeakReference"/>.IsAlive) objects.
        /// </summary>
        public static void CleanUp()
        {
            // call the overloaded method
            CleanUp(null);
        }

        /// <summary>
        /// This method cleans up all independent (!<see cref="WeakReference"/>.IsAlive) objects or a single object.
        /// </summary>
        /// <param name="objToRemove">
        /// If defined, the associated object dependency will be removed instead of a full CleanUp
        /// </param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void CleanUp(object objToRemove)
        {
            // if a particular object is passed, remove only it.
            if (objToRemove != null)
            {
                internalList.Remove(objToRemove);
                return;
            }

            // perform an full clean up

            // this list will hold all keys they has to be removed
            List<object> keysToRemove = new List<object>();

            // This list will hold all references which should be removed.
            // It's one for all keys for reduce memory allocations.
            List<WeakReference> deadReferences = new List<WeakReference>();

            // step through all object dependenies
            foreach (KeyValuePair<object, HashSet<WeakReference>> kvp in internalList)
            {
                var dependencies = kvp.Value;
                foreach (var target in dependencies)
                {
                    var targetReference = target.Target;
                    if (targetReference == null)
                        deadReferences.Add(target);
                }

                if (deadReferences.Count > 0)
                {
                    var objectDependency = kvp.Key as IObjectDependency;

                    // if the all of weak references is empty, remove the whole entry
                    if (deadReferences.Count == dependencies.Count)
                    {
                        // notify all references are dead.
                        objectDependency?.OnAllDependenciesRemoved();
                        keysToRemove.Add(kvp.Key);
                    }
                    else
                    {
                        // notify some references are dead.
                        objectDependency?.OnDependenciesRemoved(deadReferences);

                        foreach (var deadReference in deadReferences)
                        {
                            dependencies.Remove(deadReference);
                        }
                    }

                    deadReferences.Clear();
                }
            }

            // remove the key from the internalList
            foreach (var keyToRemove in keysToRemove)
            {
                internalList.Remove(keyToRemove);
            }
        }
    }
}

namespace XAMLMarkupExtensions.Base
{
    using System;
    /// <summary>
    /// Defines information about target object property.
    /// </summary>
    internal class TargetPropertyInfo
    {
        /// <summary>
        /// Gets the target property.
        /// </summary>
        public object TargetProperty { get; }

        /// <summary>
        /// Gets the target property type.
        /// </summary>
        public Type TargetPropertyType { get; }

        /// <summary>
        /// Gets the target property index.
        /// </summary>
        public int TargetPropertyIndex { get; }

        /// <summary>
        /// Create new <see cref="TargetPropertyInfo" /> instance.
        /// </summary>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="targetPropertyType">The type of property.</param>
        /// <param name="targetPropertyIndex">The target property index</param>
        public TargetPropertyInfo(object targetProperty, Type targetPropertyType, int targetPropertyIndex)
        {
            TargetProperty = targetProperty;
            TargetPropertyType = targetPropertyType;
            TargetPropertyIndex = targetPropertyIndex;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TargetPropertyInfo)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TargetProperty != null ? TargetProperty.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TargetPropertyIndex;
                return hashCode;
            }
        }

        /// <summary>
        /// Compare to another object.
        /// </summary>
        /// <param name="other">Other object for comparing.</param>
        /// <returns>
        /// <see langwrod="true" /> if objects are equal, <see langword="false" /> otherwise.
        /// </returns>
        protected bool Equals(TargetPropertyInfo other)
        {
            return Equals(TargetProperty, other.TargetProperty)
                   && TargetPropertyIndex == other.TargetPropertyIndex;
        }
    }
}
#endregion