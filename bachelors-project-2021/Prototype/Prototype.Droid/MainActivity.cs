using Android.App;
using Android.Content.PM;
using Android.OS;

namespace Prototype.Droid;

[Activity(Label = "Juttunurkka", Icon = "@drawable/emoji_main1", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
public class MainActivity : MauiAppCompatActivity
{
}
