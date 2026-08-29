using UnityEditor;
using Yamadev.YamaStream.Editor;

namespace Maksidze.YamaPlayer.GlobalVideoTexture.Editor
{
  [InitializeOnLoad]
  internal static class GlobalVideoTextureLocalizationBootstrap
  {
    static GlobalVideoTextureLocalizationBootstrap()
    {
      EditorApplication.delayCall += ReloadTranslations;
    }

    private static void ReloadTranslations()
    {
      if (EditorApplication.isCompiling || EditorApplication.isUpdating)
      {
        EditorApplication.delayCall += ReloadTranslations;
        return;
      }

      EditorLocalization.ReloadTranslations();
    }
  }
}
