using UnityEngine;
using UnityEditor;
using Virtuery.PlayFab;

/// <summary>
/// Editor utility to create the PlayFabSetup prefab.
/// To use: In Unity, go to Tools > PlayFab > Create Setup Prefab
/// </summary>
public class PlayFabPrefabCreator
{
    [MenuItem("Tools/PlayFab/Create Setup Prefab", false, 1)]
    public static void CreatePrefab()
    {
        // Create the main GameObject
        GameObject prefabRoot = new GameObject("PlayFabSetup");
        
        // Add the PlayFabSetupPrefab component
        PlayFabSetupPrefab setupComponent = prefabRoot.AddComponent<PlayFabSetupPrefab>();
        
        // Create the child objects for organization
        GameObject managerObj = new GameObject("[PlayFabManager]");
        managerObj.transform.SetParent(prefabRoot.transform);
        managerObj.AddComponent<PlayFabManager>();
        
        GameObject authObj = new GameObject("[PlayFabAuthService]");
        authObj.transform.SetParent(prefabRoot.transform);
        authObj.AddComponent<PlayFabAuthService>();
        
        GameObject dataObj = new GameObject("[PlayFabPlayerData]");
        dataObj.transform.SetParent(prefabRoot.transform);
        dataObj.AddComponent<PlayFabPlayerData>();
        
        // Ensure the Prefabs directory exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // Save as prefab
        string prefabPath = "Assets/Prefabs/PlayFabSetup.prefab";
        
        // Check if prefab already exists
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            if (EditorUtility.DisplayDialog("Prefab Exists", 
                "PlayFabSetup.prefab already exists. Do you want to overwrite it?", 
                "Yes", "No"))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }
            else
            {
                Object.DestroyImmediate(prefabRoot);
                return;
            }
        }
        
        // Create the prefab
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        
        // Clean up the scene object
        Object.DestroyImmediate(prefabRoot);
        
        // Select the new prefab
        Selection.activeObject = savedPrefab;
        
        // Ping the prefab in the Project window
        EditorGUIUtility.PingObject(savedPrefab);
        
        Debug.Log($"[PlayFab] Created PlayFabSetup prefab at {prefabPath}");
        Debug.Log("[PlayFab] Drag this prefab into your scene and set your Title ID to enable PlayFab authentication!");
    }
    
    [MenuItem("Tools/PlayFab/Open Dashboard", false, 2)]
    public static void OpenPlayFabDashboard()
    {
        Application.OpenURL("https://developer.playfab.com/");
    }
    
    [MenuItem("Tools/PlayFab/Open Integration Guide", false, 3)]
    public static void OpenIntegrationGuide()
    {
        string guidePath = System.IO.Path.Combine(Application.dataPath, "Scripts/PlayFab/INTEGRATION_GUIDE.md");
        if (System.IO.File.Exists(guidePath))
        {
            EditorUtility.OpenWithDefaultApp(guidePath);
        }
        else
        {
            Debug.LogWarning($"Integration guide not found at: {guidePath}");
        }
    }
    
    [MenuItem("Tools/PlayFab/Select PlayFabSetup in Scene", false, 4)]
    public static void SelectPlayFabSetupInScene()
    {
        PlayFabSetupPrefab setup = Object.FindObjectOfType<PlayFabSetupPrefab>();
        if (setup != null)
        {
            Selection.activeGameObject = setup.gameObject;
            EditorGUIUtility.PingObject(setup.gameObject);
        }
        else
        {
            Debug.LogWarning("[PlayFab] No PlayFabSetup found in the current scene. Drag the PlayFabSetup prefab into your scene first.");
        }
    }
    
    [MenuItem("Tools/PlayFab/Validate Setup", false, 5)]
    public static void ValidateSetup()
    {
        bool allValid = true;
        
        // Check for PlayFabManager
        if (PlayFabManager.Instance == null)
        {
            Debug.LogWarning("[PlayFab] PlayFabManager not found in scene. Add the PlayFabSetup prefab.");
            allValid = false;
        }
        else
        {
            Debug.Log("[PlayFab] PlayFabManager: OK");
        }
        
        // Check for PlayFabAuthService
        if (PlayFabAuthService.Instance == null)
        {
            Debug.LogWarning("[PlayFab] PlayFabAuthService not found in scene.");
            allValid = false;
        }
        else
        {
            Debug.Log("[PlayFab] PlayFabAuthService: OK");
        }
        
        // Check for PlayFabPlayerData
        if (PlayFabPlayerData.Instance == null)
        {
            Debug.LogWarning("[PlayFab] PlayFabPlayerData not found in scene.");
            allValid = false;
        }
        else
        {
            Debug.Log("[PlayFab] PlayFabPlayerData: OK");
        }
        
        // Check Title ID
        if (PlayFabManager.Instance != null && string.IsNullOrEmpty(PlayFab.PlayFabSettings.TitleId))
        {
            Debug.LogWarning("[PlayFab] Title ID is not set. Set it in the PlayFabSetup component or PlayFabSettings.");
            allValid = false;
        }
        else if (!string.IsNullOrEmpty(PlayFab.PlayFabSettings.TitleId))
        {
            Debug.Log($"[PlayFab] Title ID: {PlayFab.PlayFabSettings.TitleId}");
        }
        
        if (allValid)
        {
            Debug.Log("[PlayFab] All components are properly configured!");
            EditorUtility.DisplayDialog("PlayFab Validation", "All PlayFab components are properly configured!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("PlayFab Validation", "Some issues were found. Check the Console for details.", "OK");
        }
    }
}