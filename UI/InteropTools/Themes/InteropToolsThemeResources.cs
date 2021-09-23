// InteropTools 3 Theme Resources

using InteropTools.Handlers; // //UI/IT/Handles
using System;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;

namespace InteropTools.Themes
{
    public class InteropToolsThemeResources : ResourceDictionary
    {
        public InteropToolsThemeResources()
        {
            // ?
            var settingshandler = new SettingsHandler();

            // choose: fluent or  mdl2
            if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush") && !settingshandler.useMDL2)
            {
                // fluent 

                MergedDictionaries.Add
                (
                    new ResourceDictionary { Source = new Uri("ms-appx:///Themes/fluent.xaml") }
                );
            }
            else
            {
                // mdl2

                MergedDictionaries.Add
                (
                    new ResourceDictionary { Source = new Uri("ms-appx:///Themes/mdl2.xaml") }
                );
            }
        }
    }
}
