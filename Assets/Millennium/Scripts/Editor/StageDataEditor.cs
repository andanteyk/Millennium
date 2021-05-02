using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Millennium.InGame.Stage;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Cysharp.Threading.Tasks;
using System.Linq;
using System;

namespace Millennium.Editor
{
    [CustomEditor(typeof(StageData))]
    public class StageDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Create from Spreadsheet"))
            {
                // このコンテキストで await すると怒られが発生するので…
                var values = LoadFromSpreadSheet(target.name);
                values.ContinueWith(result =>
                {
                    if (result != null)
                        Parse(target as StageData, result);
                });
            }

            base.OnInspectorGUI();
        }


        // note: DLL 参照失敗とかでうまく動かなかったら消すなりしてください?
        private static async UniTask<IList<IList<object>>> LoadFromSpreadSheet(string tabName)
        {
            // このディレクトリ下に以下ファイルを作成してください:
            // * client_secrets.json : 以下文献を参考に作成する
            //      - https://developers.google.com/workspace/guides/create-credentials
            //      - https://qiita.com/kik4/items/0723ce68d6bf994a5815 
            // * spreadsheet_id.txt : https://docs.google.com/spreadsheets/d/ の後に続く ID が書かれたテキストファイル
            string rootDirectory = "UserSettings/SpreadsheetCredentials/";

            UserCredential credential;

            using (var stream = new FileStream(rootDirectory + "client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(

                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { SheetsService.Scope.SpreadsheetsReadonly },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(rootDirectory + "token.json", true));
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Unity",
            });


            string spreadsheetId;
            using (var stream = new FileStream(rootDirectory + "spreadsheet_id.txt", FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                spreadsheetId = await reader.ReadLineAsync();
            }

            string range = tabName + "!A:D";

            var request = service.Spreadsheets.Values.Get(spreadsheetId, range);

            var response = await request.ExecuteAsync();
            var values = response.Values;
            if (!(values?.Count > 0))
            {
                Debug.LogError("No data found; something wrong?");
                return null;
            }

            return values;
        }


        private void Parse(StageData target, IList<IList<object>> values)
        {
            // シートの中身:
            // 単純に (spawnSeconds, Position.x, Position.y, PrefabPath) が書かれたシートです
            // 最初の行はヘッダなのでスキップします
            // タブ名 == ファイル(scriptable object)名です

            var list = new List<EnemyData>(values.Count);
            foreach (var row in values.Skip(1))
            {
                list.Add(new EnemyData
                {
                    SpawnSeconds = Convert.ToSingle(row[0]),
                    Position = new Vector2(Convert.ToSingle(row[1]), Convert.ToSingle(row[2])),
                    Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(row[3].ToString())
                });
            }

            target.Enemies = list.ToArray();
            EditorUtility.SetDirty(target);
        }
    }


}
