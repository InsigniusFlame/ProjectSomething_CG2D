using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// Editor script to set up the Animal Demo scene with all necessary components.
/// Run via menu: AnimalDemo > Setup Demo Scene
/// </summary>
public class DemoSceneSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("AnimalDemo/Setup Demo Scene")]
    public static void SetupDemoScene()
    {
        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Setup ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(10, 1, 10); // 100x100 units
        
        // Create ground material
        Material groundMat = new Material(Shader.Find("Standard"));
        groundMat.color = new Color(0.3f, 0.5f, 0.2f); // Grass green
        ground.GetComponent<Renderer>().sharedMaterial = groundMat;
        
        // Note: NavMesh baking is now done via NavMeshSurface component in Unity 2022+
        // The user should add NavMeshSurface to the Ground and click Bake

        // Setup player
        GameObject player = CreatePlayer();
        player.transform.position = new Vector3(0, 1, 0);

        // Create passive animals (flee from player)
        Color[] passiveColors = {
            new Color(0.8f, 0.8f, 0.8f), // Light gray
            new Color(0.3f, 0.3f, 0.3f), // Dark gray
            new Color(0.9f, 0.9f, 0.6f)  // Cream
        };

        Vector3[] passivePositions = {
            new Vector3(-10, 0, 8),
            new Vector3(15, 0, -5),
            new Vector3(5, 0, 20)
        };

        for (int i = 0; i < passiveColors.Length; i++)
        {
            GameObject animal = CreateAnimal($"PassiveAnimal_{i + 1}", passiveColors[i], false);
            animal.transform.position = passivePositions[i];
        }

        // Create aggressive animals (attack player) - RED colored
        Color[] aggressiveColors = {
            new Color(0.8f, 0.2f, 0.2f), // Dark red
            new Color(1f, 0.3f, 0.3f)    // Bright red
        };

        Vector3[] aggressivePositions = {
            new Vector3(10, 0, 10),
            new Vector3(-8, 0, -12)
        };

        for (int i = 0; i < aggressiveColors.Length; i++)
        {
            GameObject animal = CreateAnimal($"AggressiveAnimal_{i + 1}", aggressiveColors[i], true);
            animal.transform.position = aggressivePositions[i];
        }

        // Setup lighting
        SetupLighting();

        // Create decorative elements
        CreateDecorations();

        // Save the scene
        string scenePath = "Assets/Scenes/AnimalDemo.unity";
        System.IO.Directory.CreateDirectory(Application.dataPath + "/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        
        Debug.Log("Demo scene created! Please bake the NavMesh:");
        Debug.Log("1. Select the Ground object");
        Debug.Log("2. Add Component > NavMesh Surface");
        Debug.Log("3. Click 'Bake' on the NavMesh Surface component");
        Debug.Log("4. Press Play to test!");
        Debug.Log("Note: RED animals are aggressive and will attack you!");

        // Open Navigation window
        EditorApplication.ExecuteMenuItem("Window/AI/Navigation");
    }

    private static GameObject CreatePlayer()
    {
        // Create player object
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Default");

        // Add capsule visual
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "PlayerVisual";
        visual.transform.SetParent(player.transform);
        visual.transform.localPosition = Vector3.zero;
        
        // Remove capsule collider (CharacterController handles collision)
        DestroyImmediate(visual.GetComponent<Collider>());
        
        // Player material
        Material playerMat = new Material(Shader.Find("Standard"));
        playerMat.color = new Color(0.2f, 0.4f, 0.8f); // Blue
        visual.GetComponent<Renderer>().sharedMaterial = playerMat;

        // Add CharacterController
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = new Vector3(0, 1f, 0);

        // Add PlayerController script
        player.AddComponent<PlayerController>();

        // Add DamageSystem for health and damage effects
        player.AddComponent<DamageSystem>();

        // Create camera
        GameObject cameraObj = new GameObject("PlayerCamera");
        cameraObj.transform.SetParent(player.transform);
        cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0);
        
        Camera cam = cameraObj.AddComponent<Camera>();
        cam.fieldOfView = 75;
        cam.nearClipPlane = 0.1f;
        
        // Remove default camera
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.gameObject != cameraObj)
        {
            DestroyImmediate(mainCam.gameObject);
        }
        cameraObj.tag = "MainCamera";

        return player;
    }

    private static GameObject CreateAnimal(string name, Color color, bool isAggressive)
    {
        // Create animal object
        GameObject animal = new GameObject(name);

        // Create body (capsule)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(animal.transform);
        body.transform.localPosition = new Vector3(0, 0.5f, 0);
        body.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
        body.transform.localRotation = Quaternion.Euler(90, 0, 0);
        
        // Remove collider (NavMeshAgent handles collision)
        DestroyImmediate(body.GetComponent<Collider>());

        // Create head (sphere)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(animal.transform);
        head.transform.localPosition = new Vector3(0, 0.6f, 0.4f);
        head.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        DestroyImmediate(head.GetComponent<Collider>());

        // Create material
        Material animalMat = new Material(Shader.Find("Standard"));
        animalMat.color = color;
        body.GetComponent<Renderer>().sharedMaterial = animalMat;
        head.GetComponent<Renderer>().sharedMaterial = animalMat;

        // Add NavMeshAgent
        NavMeshAgent agent = animal.AddComponent<NavMeshAgent>();
        agent.radius = 0.5f;
        agent.height = 1f;
        agent.speed = isAggressive ? 6f : 1.5f;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.5f;

        // Add appropriate AI script
        if (isAggressive)
        {
            animal.AddComponent<AggressiveAnimalAI>();
        }
        else
        {
            animal.AddComponent<AnimalAI>();
        }

        // Add collider for physics/damage detection
        CapsuleCollider col = animal.AddComponent<CapsuleCollider>();
        col.height = 1f;
        col.radius = 0.3f;
        col.center = new Vector3(0, 0.5f, 0);
        col.isTrigger = isAggressive; // Aggressive animals use trigger for damage

        return animal;
    }

    private static void SetupLighting()
    {
        // Find and configure directional light
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                light.transform.rotation = Quaternion.Euler(50, -30, 0);
                light.intensity = 1.2f;
                light.color = new Color(1f, 0.95f, 0.85f); // Warm sunlight
                light.shadows = LightShadows.Soft;
            }
        }

        // Set ambient color
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.4f, 0.45f, 0.5f);
        
        // Sky color
        Camera.main.backgroundColor = new Color(0.5f, 0.7f, 0.9f);
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
    }

    private static void CreateDecorations()
    {
        // Create a parent for decorations
        GameObject decorations = new GameObject("Decorations");

        // Create some trees (simple cylinders with sphere tops)
        Vector3[] treePositions = {
            new Vector3(25, 0, 25),
            new Vector3(-20, 0, 20),
            new Vector3(30, 0, -15),
            new Vector3(-25, 0, -25),
            new Vector3(15, 0, -30),
            new Vector3(-30, 0, 5),
            new Vector3(35, 0, 10),
            new Vector3(-15, 0, 35)
        };

        foreach (Vector3 pos in treePositions)
        {
            GameObject tree = CreateTree();
            tree.transform.position = pos;
            tree.transform.SetParent(decorations.transform);
            // Trees will be included in NavMesh bake automatically
        }

        // Create some rocks
        Vector3[] rockPositions = {
            new Vector3(8, 0, -8),
            new Vector3(-12, 0, 3),
            new Vector3(20, 0, 15),
            new Vector3(-5, 0, -20)
        };

        foreach (Vector3 pos in rockPositions)
        {
            GameObject rock = CreateRock();
            rock.transform.position = pos;
            rock.transform.SetParent(decorations.transform);
            // Rocks will be included in NavMesh bake automatically
        }
    }

    private static GameObject CreateTree()
    {
        GameObject tree = new GameObject("Tree");

        // Trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0, 2, 0);
        trunk.transform.localScale = new Vector3(0.8f, 2f, 0.8f);
        
        Material trunkMat = new Material(Shader.Find("Standard"));
        trunkMat.color = new Color(0.4f, 0.25f, 0.15f); // Brown
        trunk.GetComponent<Renderer>().sharedMaterial = trunkMat;
        
        // Trunk collider will be included in NavMesh bake

        // Foliage
        GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        foliage.name = "Foliage";
        foliage.transform.SetParent(tree.transform);
        foliage.transform.localPosition = new Vector3(0, 5, 0);
        foliage.transform.localScale = new Vector3(4, 4, 4);
        
        // Remove collider from foliage
        DestroyImmediate(foliage.GetComponent<Collider>());
        
        Material foliageMat = new Material(Shader.Find("Standard"));
        foliageMat.color = new Color(0.2f, 0.5f, 0.2f); // Dark green
        foliage.GetComponent<Renderer>().sharedMaterial = foliageMat;

        return tree;
    }

    private static GameObject CreateRock()
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "Rock";
        rock.transform.localScale = new Vector3(2f, 1.5f, 2f);
        rock.transform.localPosition = new Vector3(0, 0.5f, 0);
        
        Material rockMat = new Material(Shader.Find("Standard"));
        rockMat.color = new Color(0.5f, 0.5f, 0.5f); // Gray
        rock.GetComponent<Renderer>().sharedMaterial = rockMat;

        return rock;
    }
#endif
}
