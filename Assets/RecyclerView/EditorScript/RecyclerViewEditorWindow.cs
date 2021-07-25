using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerView
{
#if (UNITY_EDITOR)

    public class Menu : EditorWindow
    {
        string objNames = "";

        void OnGUI()
        {
            EditorGUI.DropShadowLabel(new Rect(0, 0, position.width, 20),
                "Choose a name for the adapter:");

            objNames = EditorGUI.TextField(new Rect(10, 25, position.width - 20, 20),
                "Name:",
                objNames);

            if (GUI.Button(new Rect(0, 50, position.width, 30), "Create"))
            {
                Selection.activeTransform = Create(objNames);
                Close();
            }
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        [MenuItem("GameObject/UI/RecyclerView", false, 0)]
        static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            var window = ScriptableObject.CreateInstance<Menu>();
            window.Show();
        }

        private static Transform Create(string name)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject();
                canvasObj.name = "Canvas";
                canvasObj.AddComponent<RectTransform>();
                canvas = canvasObj.AddComponent<Canvas>();
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            GameObject script = new GameObject();
            script.name = name;
            script.AddComponent<RectTransform>();
            script.transform.SetParent(canvas.transform);
            CreateScript(script);
            return script.transform;
        }

        private static void CreateScript(GameObject obj)
        {
            Directory.CreateDirectory("Assets/RecyclerViewComponent");

            string name = obj.name;
            string copyPath;
            MonoScript script;
            int i = 1;
            copyPath = $"Assets/RecyclerViewComponent/{name}.cs";
            script = (MonoScript)AssetDatabase.LoadAssetAtPath(copyPath, typeof(MonoScript));
            if (script != null)
            {
                do
                {
                    name = $"{name}_{i}";
                    copyPath = $"Assets/RecyclerViewComponent/{name}.cs";
                    script = (MonoScript)AssetDatabase.LoadAssetAtPath(copyPath, typeof(MonoScript));
                    i++;
                } while (script != null);
            }

            if (File.Exists(copyPath) == false)
            { // do not overwrite
                using (StreamWriter outfile =
                    new StreamWriter(copyPath))
                {
                    string file = CreateFileData();

                    outfile.WriteLine(file.Replace("{{Name}}", name));
                }
            }
            AssetDatabase.Refresh();
            CompileScript compileScript = obj.AddComponent<CompileScript>();
            compileScript.ScriptName = name;
        }

        private static string CreateFileData()
        {
            return "using UnityEngine;\n" +
                "using System.Collections;\n" +
                "using RecyclerView;\n" +
                "\n" +
                "public class {{Name}} : Adapter<{{Name}}.Holder> {\n" +
                "\n" +
                "    public override int GetItemCount()\n" +
                "    {\n" +
                "        throw new System.NotImplementedException();\n" +
                "    }\n" +
                "\n" +
                "    public override void OnBindViewHolder(Holder holder, int i)\n" +
                "    {\n" +
                "        throw new System.NotImplementedException();\n" +
                "    }\n" +
                "\n" +
                "    public override GameObject OnCreateViewHolder()\n" +
                "    {\n" +
                "        throw new System.NotImplementedException();\n" +
                "    }\n" +
                "\n" +
                "    public class Holder : ViewHolder\n" +
                "    {\n" +
                "        public Holder(GameObject itemView) : base(itemView)\n" +
                "        {\n" +
                "        }\n" +
                "    }\n" +
                "}\n" +
                "\n";
        }
    }
    [ExecuteInEditMode]
    public class CompileScript : MonoBehaviour
    {
        public string ScriptName;

        void Update()
        {
            if (!EditorApplication.isCompiling)
            {
                gameObject.AddComponent(System.Type.GetType(ScriptName));
                DestroyImmediate(this);
            }

        }

    }

#endif
}