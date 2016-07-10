using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class PalettonMaterialGenerator : EditorWindow
{
    string shader = "Legacy Shaders/Diffuse";
    string paletteName = "New palette";
    string path = "Materials";
    string palettonText = "Paste paletton.com text eport here";

    char[] charsToTrim = { ' ', ':' };

    MenuCommand mc;

    [MenuItem("DonUrks/Paletton Material Generator")]
    static public void Factory()
    {
        PalettonMaterialGenerator window = (PalettonMaterialGenerator)EditorWindow.GetWindow(typeof(PalettonMaterialGenerator));
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Shader: " + this.shader);

        if (GUILayout.Button("Change shader"))
        {
            DisplayShaderContext(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup));
        }


        this.paletteName = EditorGUILayout.TextField("Palette name", this.paletteName);
        this.path = EditorGUILayout.TextField("Path", this.path);

        List<Color> colors = new List<Color>();

        if (GUILayout.Button("Generate"))
        {
            using (StringReader reader = new StringReader(this.palettonText))
            {
                string currentColorName = "";

                string line = string.Empty;
                do
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        line = line.Trim(this.charsToTrim);

                        if (line.StartsWith("*** "))
                        {
                            currentColorName = line.Substring(4, line.Length - 4);
                        }
                        if (line.StartsWith("shade "))
                        {
                            int shadeNameLength = line.IndexOf(" = ");
                            string currentShadeName = line.Substring(0, shadeNameLength);

                            int rgb0StartIndex = line.IndexOf(" = rgb0(") + 8;
                            string rgb0 = line.Substring(rgb0StartIndex).Trim(')');
                            string[] rgb0Elements = rgb0.Split(',');

                            Color newColor;
                            newColor.a = 1.0f;
                            newColor.r = float.Parse(rgb0Elements[0]);
                            newColor.g = float.Parse(rgb0Elements[1]);
                            newColor.b = float.Parse(rgb0Elements[2]);

                            colors.Add(newColor);

                            List<string> parts = new List<string>(this.path.Split('/'));
                            parts.Add(this.paletteName);

                            string guid = AssetDatabase.AssetPathToGUID("Assets");
                            foreach (string part in parts)
                            {
                                string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);
                                if (AssetDatabase.IsValidFolder(newFolderPath + "/" + part))
                                {
                                    guid = AssetDatabase.AssetPathToGUID(newFolderPath + "/" + part);
                                }
                                else
                                {
                                    guid = AssetDatabase.CreateFolder(newFolderPath, part);
                                }
                            }
                            this.SaveMaterial(currentColorName + " " + currentShadeName, AssetDatabase.GUIDToAssetPath(guid), newColor);
                        }
                    }

                } while (line != null);
            }

            AssetDatabase.SaveAssets();
        }

        this.palettonText = EditorGUILayout.TextArea(this.palettonText, GUILayout.MinHeight(80));
    }

    private void SaveMaterial(string materialName, string path, Color color)
    {
        string materialFilename = materialName + ".mat";
        Shader currentShader = Shader.Find(this.shader);

        Material m = (Material)AssetDatabase.LoadAssetAtPath(path + "/" + materialFilename, typeof(Material));
        if (m != null)
        {
            m.color = color;
            m.shader = currentShader;
        }
        else
        {
            Material material = new Material(currentShader);
            material.color = color;
            AssetDatabase.CreateAsset(material, path + "/" + materialFilename);
        }
    }

    /*
     * Shaderselector by Acegikmo @ http://answers.unity3d.com/answers/543407/view.html
     * @todo: Creating materials from shader source string will be removed in the future. Use Shader assets instead. 
     */
    private void DisplayShaderContext(Rect r)
    {
        if (this.mc == null)
        {
            this.mc = new MenuCommand(this, 0);
        }

        // Create dummy material to make it not highlight any shaders inside:
        string tmpStr = "Shader \"Hidden/tmp_shdr\"{SubShader{Pass{}}}";
        Material temp = new Material(tmpStr);

        // Rebuild shader menu:
        UnityEditorInternal.InternalEditorUtility.SetupShaderMenu(temp);

        // Destroy temporary shader and material:
        DestroyImmediate(temp.shader, true);
        DestroyImmediate(temp, true);

        // Display shader popup:
        EditorUtility.DisplayPopupMenu(r, "CONTEXT/ShaderPopup", this.mc);
    }

    private void OnSelectedShaderPopup(string command, Shader shader)
    {
        if (shader != null)
        {
            this.shader = shader.name;
        }
    }
}