#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace FlexAnimation
{
    [InitializeOnLoad]
    public class FlexDefineManager
    {
        private const string SYMBOL_DOTWEEN = "DOTWEEN_ENABLED";

        static FlexDefineManager()
        {
            UpdateDefines();
        }

        public static void UpdateDefines()
        {
            bool hasDOTween = System.AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.GetName().Name.StartsWith("DOTween"));

            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);

            string definesString = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            List<string> allDefines = definesString.Split(';').ToList();

            if (hasDOTween)
            {
                if (!allDefines.Contains(SYMBOL_DOTWEEN))
                {
                    allDefines.Add(SYMBOL_DOTWEEN);
                    PlayerSettings.SetScriptingDefineSymbols(namedTarget, string.Join(";", allDefines.ToArray()));
                }
            }
            else
            {
                if (allDefines.Contains(SYMBOL_DOTWEEN))
                {
                    allDefines.Remove(SYMBOL_DOTWEEN);
                    PlayerSettings.SetScriptingDefineSymbols(namedTarget, string.Join(";", allDefines.ToArray()));
                }
            }
        }
    }
}
#endif