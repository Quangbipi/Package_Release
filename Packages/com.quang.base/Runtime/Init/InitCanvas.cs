using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Quang.Base.Init
{
    using Quang.UI;
    using Quang.Base;
    using Quang.Data.Persistence;
    public class InitCanvas : UICanvas
    {
        public event Action<int, bool> _OnToggleValueChange;
        public event Action _OnStartGame;
        [SerializeField]
        UIToggle gridLogicDebugToggle;
        [SerializeField]
        UIToggle fpsDebugToggle;
        [SerializeField]
        UIToggle logToggle;
        [SerializeField]
        UIToggle showAdsToggle;
        [SerializeField]
        UIToggle iapToggle;
        [SerializeField]
        Button startButton;
        [SerializeField]
        Button clearDataButton;
        [SerializeField]
        TMP_InputField levelInputField;
        [SerializeField]
        public int StartLevel => int.Parse(levelInputField.text);
        void Start()
        {
            GameData.UserData data = Database.Load<GameData>(GameData.SaveKey).user;
            if (data != null)
                levelInputField.text = data.normalLevelIndex.ToString();
            gridLogicDebugToggle._OnValueChange = OnToggleValueChange;
            fpsDebugToggle._OnValueChange = OnToggleValueChange;
            logToggle._OnValueChange = OnToggleValueChange;
            showAdsToggle._OnValueChange += OnToggleValueChange;
            iapToggle._OnValueChange += OnToggleValueChange;

            startButton.onClick.AddListener(OnStartButtonClick);
            clearDataButton.onClick.AddListener(OnClearDataButtonClick);
        }

        public void SetData(bool value1, bool value2, bool value3, bool value4, bool value5)
        {
            gridLogicDebugToggle.SetValue(value1);
            fpsDebugToggle.SetValue(value2);
            logToggle.SetValue(value3);
            showAdsToggle.SetValue(value4);
            iapToggle.SetValue(value5);
        }

        private void OnToggleValueChange(int id, bool value)
        {
            _OnToggleValueChange?.Invoke(id, value);
        }

        private void OnStartButtonClick()
        {
            _OnStartGame?.Invoke();
        }

        private void OnDestroy()
        {
            startButton.onClick.RemoveListener(OnStartButtonClick);
            clearDataButton.onClick.RemoveListener(OnClearDataButtonClick);
        }
        void OnClearDataButtonClick()
        {
            // Canonical data is JSON files under <persistentDataPath>/ComQuang; PlayerPrefs is an
            // import-only legacy source. Wiping only the files lets the legacy import repopulate
            // the save on the next Load, so both media must go.
            string comQuangRoot = Path.Combine(Application.persistentDataPath, "ComQuang");
            foreach (string directory in OwnedRootLayout.DirectoryNames)
            {
                string path = Path.Combine(comQuangRoot, "Saves", directory);
                try
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);
                }
                catch (IOException e)
                {
                    Debug.LogError($"Clear data could not delete '{path}': {e.Message}", this);
                }
            }

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            // ponytail: disk only - deliberately does NOT touch live state. DataManager and
            // ItemManager both cache GameData in DontDestroyOnLoad singletons, and DataManager
            // re-seeds items only while its cache is null, so clearing mid-session would leave
            // an unseeded GameData alive and throw on the next GetItemData. Restart to reload.
            levelInputField.text = "0";
            Debug.Log("[Init] Save data cleared. Restart play mode for it to take effect.");
        }
    }
}
