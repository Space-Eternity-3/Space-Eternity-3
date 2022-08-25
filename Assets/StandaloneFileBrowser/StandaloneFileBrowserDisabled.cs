#if UNITY_STANDALONE_OSX
#elif UNITY_STANDALONE_WIN
#elif UNITY_STANDALONE_LINUX
#elif UNITY_EDITOR
#else

using System;
using UnityEditor;

namespace SFB {
    public class StandaloneFileBrowserDisabled : IStandaloneFileBrowser  {
        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect) {
            return new string[0];
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb) {
            return;
        }

        public string[] OpenFolderPanel(string title, string directory, bool multiselect) {
            return new string[0];
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb) {
            return;
        }

        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions) {
            return "";
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb) {
            return;
        }

        // EditorUtility.OpenFilePanelWithFilters extension filter format
        private static string[] GetFilterFromFileExtensionList(ExtensionFilter[] extensions) {
            return new string[0];
        }
    }
}

#endif
