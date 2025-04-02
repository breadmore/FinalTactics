using System.Collections;
using System.Text;
using GoogleSheetsToUnity;
using GoogleSheetsToUnity.ThirdPary;
using TinyJSON;
using UnityEngine;
using UnityEngine.Networking;

public delegate void OnSpreedSheetLoaded(GstuSpreadSheet sheet);

namespace GoogleSheetsToUnity
{
    public partial class SpreadsheetManager
    {
        static GoogleSheetsToUnityConfig _config;
        public static GoogleSheetsToUnityConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = (GoogleSheetsToUnityConfig)Resources.Load("GSTU_Config");
                }
                return _config;
            }
            set { _config = value; }
        }

        public static void ReadPublicSpreadsheet(GSTU_Search searchDetails, OnSpreedSheetLoaded callback)
        {
            if (string.IsNullOrEmpty(Config.API_Key))
            {
                Debug.Log("Missing API Key, please enter this in the config settings");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("https://sheets.googleapis.com/v4/spreadsheets");
            sb.Append("/" + searchDetails.sheetId);
            sb.Append("/values");
            sb.Append("/" + searchDetails.worksheetName + "!" + searchDetails.startCell + ":" + searchDetails.endCell);
            sb.Append("?key=" + Config.API_Key);

            if (Application.isPlaying)
            {
                new Task(Read(sb.ToString(), searchDetails.titleColumn, searchDetails.titleRow, callback));
            }
#if UNITY_EDITOR
            else
            {
                EditorCoroutineRunner.StartCoroutine(Read(sb.ToString(), searchDetails.titleColumn, searchDetails.titleRow, callback));
            }
#endif
        }

        static IEnumerator Read(string url, string titleColumn, int titleRow, OnSpreedSheetLoaded callback)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error fetching spreadsheet: " + request.error);
                    yield break;
                }

                ValueRange rawData = JSON.Load(request.downloadHandler.text).Make<ValueRange>();
                GSTU_SpreadsheetResponce response = new GSTU_SpreadsheetResponce(rawData);
                GstuSpreadSheet spreadSheet = new GstuSpreadSheet(response, titleColumn, titleRow);

                callback?.Invoke(spreadSheet);
            }
        }
    }
}
