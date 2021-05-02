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
                // ���̃R���e�L�X�g�� await ����Ɠ{��ꂪ��������̂Łc
                var values = LoadFromSpreadSheet(target.name);
                values.ContinueWith(result =>
                {
                    if (result != null)
                        Parse(target as StageData, result);
                });
            }

            base.OnInspectorGUI();
        }


        // note: DLL �Q�Ǝ��s�Ƃ��ł��܂������Ȃ�����������Ȃ肵�Ă�������?
        private static async UniTask<IList<IList<object>>> LoadFromSpreadSheet(string tabName)
        {
            // ���̃f�B���N�g�����Ɉȉ��t�@�C�����쐬���Ă�������:
            // * client_secrets.json : �ȉ��������Q�l�ɍ쐬����
            //      - https://developers.google.com/workspace/guides/create-credentials
            //      - https://qiita.com/kik4/items/0723ce68d6bf994a5815 
            // * spreadsheet_id.txt : https://docs.google.com/spreadsheets/d/ �̌�ɑ��� ID �������ꂽ�e�L�X�g�t�@�C��
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
            // �V�[�g�̒��g:
            // �P���� (spawnSeconds, Position.x, Position.y, PrefabPath) �������ꂽ�V�[�g�ł�
            // �ŏ��̍s�̓w�b�_�Ȃ̂ŃX�L�b�v���܂�
            // �^�u�� == �t�@�C��(scriptable object)���ł�

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
