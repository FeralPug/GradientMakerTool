using UnityEngine;
using UnityEditor;
using System.IO;

namespace FeralPugTools
{
    public class GradientMakerTool : EditorWindow
    {
        const int width = 10 * 4 + 126 * 3;
        bool wasDocked;

        //textures used, the default is just to draw something where the texture will be
        Texture2D gradientTexture;
        Texture2D defaultTexture;

        //resolution of the texture
        Vector2Int textureResolution;

        //draw mode
        DrawMode drawMode;

        //file type
        FileType fileType;

        //non radial draw mode settings
        float angle;
        float offset;
        float tile;

        //radial draw mode settings
        float radius;
        Vector2 centerOffset;
        float radialFalloff;

        //should it wrap repeat
        bool repeat;

        //the actual gradient to use
        Gradient gradient;

        Vector2 scrollPosition;

        enum FileType
        {
            png,
            jpg
        }

        enum DrawMode
        {
            Vertical,
            Horizontal,
            Radial
        }

        [MenuItem("Tools/Feral_Pug Tools/Gradient Maker")]
        public static void OpenWindow()
        {
            GradientMakerTool gradientMakerTool = GetWindow<GradientMakerTool>();

            gradientMakerTool.SetDefaultValues();
            gradientMakerTool.SetWindowSize();

            gradientMakerTool.Show();
        }

        void SetWindowSize()
        {
            if (!docked)
            {
                if (drawMode == DrawMode.Radial)
                {
                    Rect rect = position;
                    rect.width = width;
                    rect.height = 370;
                    position = rect;
                    minSize = new Vector2(width, 370);
                }
                else
                {
                    minSize = new Vector2(width, 350);
                    Rect rect = position;
                    rect.width = width;
                    rect.height = 345;
                    position = rect;
                }
            }
            else
            {
                scrollPosition = Vector2.zero;
            }

        }

        void SetDefaultValues()
        {
            DestroyImmediate(gradientTexture);

            textureResolution = new Vector2Int(256, 256);

            drawMode = DrawMode.Vertical;

            fileType = FileType.png;

            angle = 0f;
            offset = 0f;
            tile = 1f;

            radius = 1f;
            centerOffset = Vector2.zero;
            radialFalloff = 1f;

            repeat = true;

            gradient = new Gradient();
        }



        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Gradient Texture Maker Tool", MessageType.None);

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                DrawMode prevDrawMode = drawMode;
                Vector2 lastScrollPos = scrollPosition;

                if (docked)
                {
                    wasDocked = true;
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                }
                else
                {
                    if (wasDocked)
                    {
                        SetWindowSize();
                        wasDocked = false;
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    Rect control = EditorGUILayout.GetControlRect();
                    if (docked)
                    {
                        control.width = width;
                    }
                    Rect areaRect = EditorGUI.PrefixLabel(control, new GUIContent("TextureResolution"));
                    textureResolution = EditorGUI.Vector2IntField(areaRect, "", textureResolution);

                    textureResolution.x = Mathf.Max(1, textureResolution.x);
                    textureResolution.y = Mathf.Max(1, textureResolution.y);

                    textureResolution.x = Mathf.Min(8192, textureResolution.x);
                    textureResolution.y = Mathf.Min(8192, textureResolution.y);
                }

                using (new GUILayout.HorizontalScope())
                {
                    Rect control = EditorGUILayout.GetControlRect();
                    if (docked)
                    {
                        control.width = width;
                    }
                    Rect areaRect = EditorGUI.PrefixLabel(control, new GUIContent("Draw Mode"));
                    drawMode = (DrawMode)EditorGUI.EnumPopup(areaRect, drawMode);

                    if (drawMode != prevDrawMode)
                    {
                        SetWindowSize();
                    }
                }

                if (drawMode != DrawMode.Radial)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        Rect control = EditorGUILayout.GetControlRect();
                        if (docked)
                        {
                            control.width = width;
                        }
                        angle = EditorGUI.FloatField(control, "Gradient Angle", angle);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        Rect control = EditorGUILayout.GetControlRect();
                        if (docked)
                        {
                            control.width = width;
                            control.width = width / 2;
                        }

                        tile = EditorGUI.FloatField(control, "Tiling", tile);
                        if (docked)
                        {
                            control.x += 10 + control.width;
                            control.width -= 10;
                            offset = EditorGUI.FloatField(control, "Offset", offset);
                        }
                        else
                        {
                            offset = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), "Offset", offset);
                        }
                    }
                }
                else
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        Rect control = EditorGUILayout.GetControlRect();
                        if (docked)
                        {
                            control.width = width;
                        }
                        radius = EditorGUI.FloatField(control, "Gradient Radius", radius);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        Rect control = EditorGUILayout.GetControlRect();
                        if (docked)
                        {
                            control.width = width;
                        }
                        Rect areaRect = EditorGUI.PrefixLabel(control, new GUIContent("Center Offset"));
                        centerOffset = EditorGUI.Vector2Field(areaRect, "", centerOffset);
                    }

                    if (docked)
                    {
                        using (new GUILayout.HorizontalScope(GUILayout.Width(width)))
                        {
                            radialFalloff = EditorGUILayout.Slider("Radial Falloff", radialFalloff, 0f, 10f);
                        }
                    }
                    else
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            radialFalloff = EditorGUILayout.Slider("Radial Falloff", radialFalloff, 0f, 10f);
                        }
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    Rect control = EditorGUILayout.GetControlRect();
                    if (docked)
                    {
                        control.width = width;
                    }
                    repeat = EditorGUI.Toggle(control, "Repeat", repeat);
                }

                if (docked)
                {
                    using (new GUILayout.HorizontalScope(GUILayout.Width(width)))
                    {
                        gradient = EditorGUILayout.GradientField("Gradient", gradient);
                    }
                }
                else
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        gradient = EditorGUILayout.GradientField("Gradient", gradient);
                    }
                }

                if (change.changed)
                {
                    if ((prevDrawMode == drawMode || (prevDrawMode != drawMode && gradientTexture != null)) && scrollPosition == lastScrollPos)
                    {
                        if (drawMode == DrawMode.Horizontal)
                        {
                            CreateTextureHorizontal();
                        }
                        else if (drawMode == DrawMode.Vertical)
                        {
                            CreateTextureVertical();
                        }
                        else
                        {
                            CreateTextureRadial();
                        }
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    Rect rect = new Rect(10, EditorGUILayout.GetControlRect().y, 126, EditorGUILayout.GetControlRect().height);

                    EditorGUI.LabelField(rect, "Texture (RGBA)");
                    rect.x += 126 + 10;
                    EditorGUI.LabelField(rect, "Albedo");
                    rect.x += 126 + 10;
                    EditorGUI.LabelField(rect, "Alpha");
                }

                using (new GUILayout.VerticalScope(GUILayout.Height(126)))
                {
                    if (gradientTexture != null)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            Rect rect = EditorGUILayout.GetControlRect();
                            if (docked)
                            {
                                rect.width = width;
                            }
                            rect.width /= 3f;

                            EditorGUI.DrawTextureTransparent(new Rect(10, EditorGUILayout.GetControlRect().y, 126f, 126f), gradientTexture);
                            EditorGUI.DrawPreviewTexture(new Rect(10 + 126 + 10, EditorGUILayout.GetControlRect().y, 126f, 126f), gradientTexture);
                            EditorGUI.DrawTextureAlpha(new Rect(10 + 126 + 10 + 126 + 10, EditorGUILayout.GetControlRect().y, 126f, 126f), gradientTexture);
                        }
                    }
                    else
                    {
                        if (defaultTexture == null)
                        {
                            defaultTexture = new Texture2D(1, 1);
                            defaultTexture.SetPixel(0, 0, Color.clear);
                            defaultTexture.Apply();
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            Rect rect = EditorGUILayout.GetControlRect();
                            if (docked)
                            {
                                rect.width = width;
                            }
                            rect.width /= 3f;

                            EditorGUI.DrawTextureTransparent(new Rect(10, EditorGUILayout.GetControlRect().y, 126f, 126f), defaultTexture);
                            EditorGUI.DrawPreviewTexture(new Rect(10 + 126 + 10, EditorGUILayout.GetControlRect().y, 126f, 126f), defaultTexture);
                            EditorGUI.DrawTextureAlpha(new Rect(10 + 126 + 10 + 126 + 10, EditorGUILayout.GetControlRect().y, 126f, 126f), defaultTexture);
                        }
                    }
                }

                EditorGUILayout.Space();

                using (new GUILayout.HorizontalScope())
                {
                    Rect rect = EditorGUILayout.GetControlRect();
                    if (docked)
                    {
                        rect.width = width;
                    }
                    if (GUI.Button(rect, "Reset"))
                    {
                        SetDefaultValues();
                    }
                }

                EditorGUILayout.Space();

                if (docked)
                {
                    using (new GUILayout.HorizontalScope(GUILayout.Width(width)))
                    {
                        Rect rect = EditorGUILayout.GetControlRect();
                        Rect areaRect = EditorGUI.PrefixLabel(rect, new GUIContent("File Type"));
                        fileType = (FileType)EditorGUI.EnumPopup(areaRect, fileType);

                        if (gradientTexture == null)
                        {
                            GUI.enabled = false;
                        }
                        else
                        {
                            GUI.enabled = true;
                        }
                        rect = EditorGUILayout.GetControlRect();
                        if (GUI.Button(rect, "Save Texture"))
                        {
                            SaveTexture();
                        }
                        GUI.enabled = true;
                    }
                }
                else
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        Rect rect = EditorGUILayout.GetControlRect();
                        Rect areaRect = EditorGUI.PrefixLabel(rect, new GUIContent("File Type"));
                        fileType = (FileType)EditorGUI.EnumPopup(areaRect, fileType);

                        if (gradientTexture == null)
                        {
                            GUI.enabled = false;
                        }
                        else
                        {
                            GUI.enabled = true;
                        }
                        rect = EditorGUILayout.GetControlRect();
                        if (GUI.Button(rect, "Save Texture"))
                        {
                            SaveTexture();
                        }
                        GUI.enabled = true;
                    }
                }

                if (docked)
                {
                    EditorGUILayout.EndScrollView();
                }

            }
        }

        void SaveTexture()
        {
            string ext = fileType.ToString().ToLower();
            string dotExt = "." + ext;

            string absolutePath = EditorUtility.SaveFilePanel("Save Gradient", "Assets", "Gradient" + dotExt, ext);
            if (string.IsNullOrEmpty(absolutePath))
            {
                return;
            }

            byte[] bytes;

            if (fileType == FileType.png)
            {
                bytes = gradientTexture.EncodeToPNG();
            }
            else
            {
                bytes = gradientTexture.EncodeToJPG();
            }

            File.WriteAllBytes(absolutePath, bytes);

            AssetDatabase.Refresh();
        }

        void CreateTextureVertical()
        {
            gradientTexture = new Texture2D(textureResolution.x, textureResolution.y);

            //we always want the angle in the NE quadrant
            float yAngle = Mathf.Abs(angle);
            if (yAngle > 90f)
            {
                //this flips over the x axis if needed
                yAngle = 180 - yAngle;
            }

            //then calc distance using angle from y axis
            float yDist = Mathf.Abs(textureResolution.y / Mathf.Cos(yAngle * Mathf.Deg2Rad));

            //then do the same but from the x axis, 90 - angle gives us this
            float xDist = Mathf.Abs(textureResolution.x / Mathf.Cos((90 - yAngle) * Mathf.Deg2Rad));

            //then the distance we want is the smaller of the two.
            //this is because after 45 degrees the distance will be greater than the texture, one of these distances will be less than 45
            float dist = Mathf.Min(xDist, yDist);
            float texOffset = offset * dist;
            Vector2 dir = Quaternion.Euler(0f, 0f, -angle) * Vector2.up * Mathf.Sign(tile);

            //if our direction is negative in any direction we have to modifiy the pixel position in the loop
            //this gives us those modifications
            float xMod = dir.x < 0f ? gradientTexture.width : 0f;
            float yMod = dir.y < 0f ? gradientTexture.height : 0f;

            for (int x = 0; x < gradientTexture.width; x++)
            {
                for (int y = 0; y < gradientTexture.height; y++)
                {
                    float pixelDist = Vector2.Dot(dir, new Vector2(x - xMod, y - yMod));

                    //Color pixelColor = gradient.Evaluate(x / ((float)gradientTexture.width - 1));
                    float gradientPos = ((pixelDist + texOffset) / dist) * Mathf.Abs(tile);
                    float fracPos = gradientPos;
                    if (repeat)
                    {
                        fracPos = gradientPos % 1f + 1f;
                        fracPos = fracPos % 1f;
                    }

                    Color pixelColor = gradient.Evaluate(fracPos);

                    gradientTexture.SetPixel(x, y, pixelColor);
                }
            }

            gradientTexture.Apply();
        }

        void CreateTextureHorizontal()
        {
            gradientTexture = new Texture2D(textureResolution.x, textureResolution.y);

            //we always want the angle in the NE quadrant
            float xAngle = Mathf.Abs(angle);
            if (xAngle > 90f)
            {
                //this flips over the x axis if needed
                xAngle = 180 - xAngle;
            }

            //then calc distance using angle from y axis
            float xDist = Mathf.Abs(textureResolution.x / Mathf.Cos(xAngle * Mathf.Deg2Rad));

            //then do the same but from the x axis, 90 - angle gives us this
            float yDist = Mathf.Abs(textureResolution.y / Mathf.Cos((90 - xAngle) * Mathf.Deg2Rad));

            //then the distance we want is the smaller of the two.
            //this is because after 45 degrees the distance will be greater than the texture, one of these distances will be less than 45
            float dist = Mathf.Min(xDist, yDist);
            float texOffset = offset * dist;
            Vector2 dir = Quaternion.Euler(0f, 0f, -angle) * Vector2.right * Mathf.Sign(tile);

            //if our direction is negative in any direction we have to modifiy the pixel position in the loop
            //this gives us those modifications
            float xMod = dir.x < 0f ? gradientTexture.width : 0f;
            float yMod = dir.y < 0f ? gradientTexture.height : 0f;

            for (int x = 0; x < gradientTexture.width; x++)
            {
                for (int y = 0; y < gradientTexture.height; y++)
                {
                    float pixelDist = Vector2.Dot(dir, new Vector2(x - xMod, y - yMod));

                    //Color pixelColor = gradient.Evaluate(x / ((float)gradientTexture.width - 1));
                    float gradientPos = ((pixelDist + texOffset) / dist) * Mathf.Abs(tile);
                    float fracPos = gradientPos;
                    if (repeat)
                    {
                        fracPos = gradientPos % 1f + 1f;
                        fracPos = fracPos % 1f;
                    }

                    Color pixelColor = gradient.Evaluate(fracPos);

                    gradientTexture.SetPixel(x, y, pixelColor);
                }
            }

            gradientTexture.Apply();
        }

        void CreateTextureRadial()
        {
            gradientTexture = new Texture2D(textureResolution.x, textureResolution.y);

            Vector2 center = new Vector2((gradientTexture.width - 1) * .5f, (gradientTexture.height - 1) * .5f);
            center.x += centerOffset.x * ((gradientTexture.width - 1) * .5f);
            center.y += centerOffset.y * ((gradientTexture.height - 1) * .5f);

            float localRadius = ((gradientTexture.width - 1) * .5f) / radius;

            for (int x = 0; x < gradientTexture.width; x++)
            {
                for (int y = 0; y < gradientTexture.height; y++)
                {
                    float pixelDist = Vector2.Distance(center, new Vector2(x, y));

                    //+ 1 because even size texture will come just short of 1 at center edge
                    float gradientPos = (pixelDist + 1f) / localRadius;
                    float fracPos = gradientPos;
                    if (repeat)
                    {
                        fracPos = gradientPos % 1f + 1f;
                        fracPos = fracPos % 1f;
                    }

                    Color pixelColor = gradient.Evaluate(fracPos);
                    pixelColor.r = Mathf.Pow(pixelColor.r, radialFalloff);
                    pixelColor.g = Mathf.Pow(pixelColor.g, radialFalloff);
                    pixelColor.b = Mathf.Pow(pixelColor.b, radialFalloff);
                    pixelColor.a = Mathf.Pow(pixelColor.a, radialFalloff);

                    gradientTexture.SetPixel(x, y, pixelColor);
                }
            }

            gradientTexture.Apply();
        }
    }
}

