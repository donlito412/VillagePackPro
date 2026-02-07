using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Village Pack Pro - Procedural Village Generator
/// Version 1.0 | Create Medieval Villages Instantly
/// </summary>
public class VillagePackPro : EditorWindow
{
    public enum VillageStyle { Medieval, Fantasy, Tropical, Oriental, Viking }
    public enum VillageLayout { Circle, Grid, Organic, Linear }
    
    private VillageStyle style = VillageStyle.Medieval;
    private VillageLayout layout = VillageLayout.Circle;
    private int houseCount = 8;
    private float villageRadius = 30f;
    private bool addWell = true;
    private bool addFences = true;
    private bool addPaths = true;
    private bool addProps = true;
    private bool addLights = true;
    
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Village Pack Pro")]
    public static void ShowWindow()
    {
        VillagePackPro window = GetWindow<VillagePackPro>("Village Pack Pro");
        window.minSize = new Vector2(380, 520);
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Header
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 18;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🏘️ VILLAGE PACK PRO", headerStyle);
        GUILayout.Label("Procedural Village Generator", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.Space(10);
        
        // Style Section
        GUILayout.Label("🎨 Village Style", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        style = (VillageStyle)EditorGUILayout.EnumPopup("Style", style);
        DrawStylePreview();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Layout Settings
        GUILayout.Label("📐 Layout Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        layout = (VillageLayout)EditorGUILayout.EnumPopup("Layout", layout);
        houseCount = EditorGUILayout.IntSlider("House Count", houseCount, 4, 20);
        villageRadius = EditorGUILayout.Slider("Village Size", villageRadius, 15f, 60f);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Features
        GUILayout.Label("✨ Features", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        addWell = EditorGUILayout.Toggle("Add Central Well", addWell);
        addFences = EditorGUILayout.Toggle("Add Fences", addFences);
        addPaths = EditorGUILayout.Toggle("Add Paths", addPaths);
        addProps = EditorGUILayout.Toggle("Add Props (Crates, Barrels)", addProps);
        addLights = EditorGUILayout.Toggle("Add Lights", addLights);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        
        // Generate Button
        GUI.backgroundColor = new Color(0.4f, 0.7f, 0.3f);
        if (GUILayout.Button("🏘️ GENERATE VILLAGE", GUILayout.Height(45)))
        {
            GenerateVillage();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add House", GUILayout.Height(30)))
        {
            CreateHouse(Vector3.zero, null);
        }
        if (GUILayout.Button("Add Well", GUILayout.Height(30)))
        {
            CreateWell(Vector3.zero, null);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
        if (GUILayout.Button("🗑️ Clear Village", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear Village", "Delete the entire village?", "Yes", "Cancel"))
            {
                ClearVillage();
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(20);
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawStylePreview()
    {
        string desc = style switch
        {
            VillageStyle.Medieval => "🏰 European timber-frame houses",
            VillageStyle.Fantasy => "🧙 Whimsical colorful cottages",
            VillageStyle.Tropical => "🌴 Huts with thatched roofs",
            VillageStyle.Oriental => "🏯 East Asian architecture",
            VillageStyle.Viking => "⚔️ Nordic longhouses",
            _ => ""
        };
        EditorGUILayout.HelpBox(desc, MessageType.None);
    }
    
    private (Color wall, Color roof, Color trim, Color ground) GetStyleColors()
    {
        return style switch
        {
            VillageStyle.Medieval => (new Color(0.6f, 0.5f, 0.35f), new Color(0.4f, 0.2f, 0.1f), new Color(0.35f, 0.25f, 0.15f), new Color(0.4f, 0.35f, 0.25f)),
            VillageStyle.Fantasy => (new Color(0.9f, 0.85f, 0.7f), new Color(0.3f, 0.5f, 0.6f), new Color(0.6f, 0.4f, 0.2f), new Color(0.3f, 0.45f, 0.25f)),
            VillageStyle.Tropical => (new Color(0.7f, 0.6f, 0.4f), new Color(0.5f, 0.45f, 0.25f), new Color(0.5f, 0.4f, 0.3f), new Color(0.6f, 0.55f, 0.35f)),
            VillageStyle.Oriental => (new Color(0.85f, 0.8f, 0.7f), new Color(0.2f, 0.2f, 0.25f), new Color(0.6f, 0.15f, 0.1f), new Color(0.45f, 0.4f, 0.3f)),
            VillageStyle.Viking => (new Color(0.4f, 0.35f, 0.25f), new Color(0.35f, 0.3f, 0.2f), new Color(0.3f, 0.25f, 0.15f), new Color(0.35f, 0.4f, 0.3f)),
            _ => (Color.white, Color.gray, Color.black, Color.green)
        };
    }
    
    private void GenerateVillage()
    {
        EditorUtility.DisplayProgressBar("Village Pack Pro", "Creating village...", 0.1f);
        
        ClearVillage();
        
        GameObject parent = new GameObject("Village");
        parent.transform.position = Vector3.zero;
        
        var colors = GetStyleColors();
        
        // Create ground
        EditorUtility.DisplayProgressBar("Village Pack Pro", "Creating ground...", 0.2f);
        CreateGround(parent, colors.ground);
        
        // Calculate house positions
        List<Vector3> housePositions = GetHousePositions();
        
        // Create houses
        EditorUtility.DisplayProgressBar("Village Pack Pro", "Building houses...", 0.4f);
        foreach (Vector3 pos in housePositions)
        {
            CreateHouse(pos, parent);
        }
        
        // Add well
        if (addWell)
        {
            EditorUtility.DisplayProgressBar("Village Pack Pro", "Adding well...", 0.6f);
            CreateWell(Vector3.zero, parent);
        }
        
        // Add fences
        if (addFences)
        {
            EditorUtility.DisplayProgressBar("Village Pack Pro", "Building fences...", 0.7f);
            CreateFences(parent, housePositions);
        }
        
        // Add paths
        if (addPaths)
        {
            EditorUtility.DisplayProgressBar("Village Pack Pro", "Creating paths...", 0.75f);
            CreatePaths(parent, housePositions);
        }
        
        // Add props
        if (addProps)
        {
            EditorUtility.DisplayProgressBar("Village Pack Pro", "Scattering props...", 0.85f);
            CreateProps(parent, housePositions);
        }
        
        // Add lights
        if (addLights)
        {
            EditorUtility.DisplayProgressBar("Village Pack Pro", "Adding lights...", 0.9f);
            AddLights(parent, housePositions);
        }
        
        EditorUtility.ClearProgressBar();
        Selection.activeGameObject = parent;
        Debug.Log("✅ Village generated with " + houseCount + " houses!");
    }
    
    private List<Vector3> GetHousePositions()
    {
        List<Vector3> positions = new List<Vector3>();
        
        switch (layout)
        {
            case VillageLayout.Circle:
                for (int i = 0; i < houseCount; i++)
                {
                    float angle = (i / (float)houseCount) * Mathf.PI * 2;
                    positions.Add(new Vector3(Mathf.Cos(angle) * villageRadius, 0, Mathf.Sin(angle) * villageRadius));
                }
                break;
                
            case VillageLayout.Grid:
                int cols = Mathf.CeilToInt(Mathf.Sqrt(houseCount));
                float spacing = villageRadius * 2 / cols;
                for (int i = 0; i < houseCount; i++)
                {
                    int x = i % cols;
                    int z = i / cols;
                    positions.Add(new Vector3((x - cols/2f) * spacing, 0, (z - cols/2f) * spacing));
                }
                break;
                
            case VillageLayout.Organic:
                for (int i = 0; i < houseCount; i++)
                {
                    float r = Random.Range(villageRadius * 0.4f, villageRadius);
                    float angle = Random.Range(0f, Mathf.PI * 2);
                    positions.Add(new Vector3(Mathf.Cos(angle) * r, 0, Mathf.Sin(angle) * r));
                }
                break;
                
            case VillageLayout.Linear:
                float step = villageRadius * 2 / (houseCount - 1);
                for (int i = 0; i < houseCount; i++)
                {
                    float side = (i % 2 == 0) ? 5f : -5f;
                    positions.Add(new Vector3(side, 0, -villageRadius + i * step));
                }
                break;
        }
        
        return positions;
    }
    
    private void CreateGround(GameObject parent, Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ground.name = "Ground";
        ground.transform.parent = parent.transform;
        ground.transform.position = new Vector3(0, -0.5f, 0);
        ground.transform.localScale = new Vector3(villageRadius * 2.5f, 0.5f, villageRadius * 2.5f);
        ground.GetComponent<Renderer>().material = mat;
    }
    
    private void CreateHouse(Vector3 position, GameObject parent)
    {
        if (parent == null) parent = GameObject.Find("Village") ?? new GameObject("Village");
        
        var colors = GetStyleColors();
        
        Material wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        wallMat.color = colors.wall;
        Material roofMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        roofMat.color = colors.roof;
        Material trimMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        trimMat.color = colors.trim;
        
        float scale = Random.Range(0.8f, 1.2f);
        float width = Random.Range(4f, 6f) * scale;
        float depth = Random.Range(5f, 7f) * scale;
        float height = Random.Range(3f, 4f) * scale;
        
        GameObject house = new GameObject("House");
        house.transform.parent = parent.transform;
        house.transform.position = position;
        house.transform.LookAt(Vector3.zero);
        
        // Main body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.parent = house.transform;
        body.transform.localPosition = new Vector3(0, height/2, 0);
        body.transform.localScale = new Vector3(width, height, depth);
        body.GetComponent<Renderer>().material = wallMat;
        
        // Roof
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.parent = house.transform;
        roof.transform.localPosition = new Vector3(0, height + 0.8f * scale, 0);
        roof.transform.localScale = new Vector3(width + 0.5f, 1.5f * scale, depth + 0.5f);
        roof.transform.localRotation = Quaternion.Euler(0, 0, 5);
        roof.GetComponent<Renderer>().material = roofMat;
        
        // Door
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        door.transform.parent = house.transform;
        door.transform.localPosition = new Vector3(0, 0.9f, -depth/2 - 0.05f);
        door.transform.localScale = new Vector3(1f, 1.8f, 0.1f);
        door.GetComponent<Renderer>().material = trimMat;
        
        // Chimney (some houses)
        if (Random.value > 0.4f)
        {
            GameObject chimney = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chimney.name = "Chimney";
            chimney.transform.parent = house.transform;
            chimney.transform.localPosition = new Vector3(width/4, height + 2f, 0);
            chimney.transform.localScale = new Vector3(0.6f, 1.5f, 0.6f);
            chimney.GetComponent<Renderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.4f, 0.35f, 0.35f) };
        }
    }
    
    private void CreateWell(Vector3 position, GameObject parent)
    {
        if (parent == null) parent = GameObject.Find("Village") ?? new GameObject("Village");
        
        Material stoneMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        stoneMat.color = new Color(0.5f, 0.5f, 0.5f);
        Material woodMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        woodMat.color = new Color(0.4f, 0.3f, 0.2f);
        Material waterMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        waterMat.color = new Color(0.2f, 0.4f, 0.6f, 0.8f);
        
        GameObject well = new GameObject("Well");
        well.transform.parent = parent.transform;
        well.transform.position = position;
        
        // Base
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObj.transform.parent = well.transform;
        baseObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        baseObj.transform.localScale = new Vector3(2f, 1f, 2f);
        baseObj.GetComponent<Renderer>().material = stoneMat;
        
        // Water
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        water.transform.parent = well.transform;
        water.transform.localPosition = new Vector3(0, 0.3f, 0);
        water.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
        water.GetComponent<Renderer>().material = waterMat;
        DestroyImmediate(water.GetComponent<Collider>());
        
        // Roof supports
        for (int i = 0; i < 2; i++)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.transform.parent = well.transform;
            post.transform.localPosition = new Vector3(i == 0 ? -0.8f : 0.8f, 1.5f, 0);
            post.transform.localScale = new Vector3(0.15f, 2f, 0.15f);
            post.GetComponent<Renderer>().material = woodMat;
        }
        
        // Roof
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.transform.parent = well.transform;
        roof.transform.localPosition = new Vector3(0, 2.7f, 0);
        roof.transform.localScale = new Vector3(2.5f, 0.2f, 1.5f);
        roof.GetComponent<Renderer>().material = woodMat;
    }
    
    private void CreateFences(GameObject parent, List<Vector3> housePositions)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.4f, 0.3f, 0.2f);
        
        GameObject fences = new GameObject("Fences");
        fences.transform.parent = parent.transform;
        
        foreach (Vector3 housePos in housePositions)
        {
            if (Random.value > 0.6f) continue;
            
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad + Random.Range(-0.2f, 0.2f);
                Vector3 pos = housePos + new Vector3(Mathf.Cos(angle) * 4, 0, Mathf.Sin(angle) * 4);
                
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.transform.parent = fences.transform;
                post.transform.position = pos;
                post.transform.localScale = new Vector3(0.1f, 1f, 2f);
                post.transform.rotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);
                post.GetComponent<Renderer>().material = mat;
            }
        }
    }
    
    private void CreatePaths(GameObject parent, List<Vector3> housePositions)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.5f, 0.4f, 0.3f);
        
        GameObject paths = new GameObject("Paths");
        paths.transform.parent = parent.transform;
        
        foreach (Vector3 housePos in housePositions)
        {
            Vector3 dir = (Vector3.zero - housePos).normalized;
            float dist = Vector3.Distance(Vector3.zero, housePos);
            
            GameObject path = GameObject.CreatePrimitive(PrimitiveType.Cube);
            path.transform.parent = paths.transform;
            path.transform.position = housePos + dir * (dist / 2);
            path.transform.localScale = new Vector3(2f, 0.05f, dist);
            path.transform.LookAt(Vector3.zero);
            path.GetComponent<Renderer>().material = mat;
            DestroyImmediate(path.GetComponent<Collider>());
        }
    }
    
    private void CreateProps(GameObject parent, List<Vector3> housePositions)
    {
        Material crateMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        crateMat.color = new Color(0.5f, 0.4f, 0.25f);
        Material barrelMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        barrelMat.color = new Color(0.45f, 0.35f, 0.2f);
        
        GameObject props = new GameObject("Props");
        props.transform.parent = parent.transform;
        
        foreach (Vector3 housePos in housePositions)
        {
            if (Random.value > 0.5f) continue;
            
            Vector3 offset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            
            if (Random.value > 0.5f)
            {
                // Crate
                GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crate.name = "Crate";
                crate.transform.parent = props.transform;
                crate.transform.position = housePos + offset + Vector3.up * 0.3f;
                crate.transform.localScale = Vector3.one * 0.6f;
                crate.GetComponent<Renderer>().material = crateMat;
            }
            else
            {
                // Barrel
                GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                barrel.name = "Barrel";
                barrel.transform.parent = props.transform;
                barrel.transform.position = housePos + offset + Vector3.up * 0.4f;
                barrel.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                barrel.GetComponent<Renderer>().material = barrelMat;
            }
        }
    }
    
    private void AddLights(GameObject parent, List<Vector3> housePositions)
    {
        GameObject lights = new GameObject("Lights");
        lights.transform.parent = parent.transform;
        
        // Central light
        GameObject centerLight = new GameObject("CenterLight");
        centerLight.transform.parent = lights.transform;
        centerLight.transform.position = new Vector3(0, 5, 0);
        Light cl = centerLight.AddComponent<Light>();
        cl.type = LightType.Point;
        cl.color = new Color(1f, 0.9f, 0.7f);
        cl.intensity = 2f;
        cl.range = villageRadius;
        
        // House lights
        foreach (Vector3 pos in housePositions)
        {
            if (Random.value > 0.6f) continue;
            
            GameObject houseLight = new GameObject("HouseLight");
            houseLight.transform.parent = lights.transform;
            houseLight.transform.position = pos + Vector3.up * 2;
            Light hl = houseLight.AddComponent<Light>();
            hl.type = LightType.Point;
            hl.color = new Color(1f, 0.8f, 0.5f);
            hl.intensity = 1f;
            hl.range = 6f;
        }
    }
    
    private void ClearVillage()
    {
        GameObject village = GameObject.Find("Village");
        if (village != null) DestroyImmediate(village);
    }
}
