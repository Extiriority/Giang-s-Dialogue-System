using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class GDSEditorWindow : EditorWindow {
    private GDSGraphView graphView;

    private readonly string defaultFileName = "DialoguesFileName";

    private static TextField fileNameTextField;
    private Button saveButton;
    private Button miniMapButton;
    
    [MenuItem("Window/Giangs Dialogue System/Dialogue Graph")]
    public static void Open() {
        GetWindow<GDSEditorWindow>("Dialogue Graph");
    }
    
    private void OnEnable() {
        addGraphView();
        addStyles();
        
        addToolbar();
    }

    #region Elements addition

    private void addGraphView() {
        var graphView = new GDSGraphView(this);
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }
    
    private void addToolbar()
    {
        Toolbar toolbar = new Toolbar();

        fileNameTextField = GDSElementUtility.createTextField(defaultFileName, "File Name:", callback =>
        {
            fileNameTextField.value = callback.newValue;
        });

        saveButton = GDSElementUtility.createButton("Save");

        //Button loadButton = GDSElementUtility.createButton("Load", () => Load());
        //Button clearButton = GDSElementUtility.createButton("Clear", () => Clear());
        //Button resetButton = GDSElementUtility.createButton("Reset", () => ResetGraph());

        //miniMapButton = GDSElementUtility.createButton("Minimap", () => toggleMiniMap());

        toolbar.Add(fileNameTextField);
        toolbar.Add(saveButton);
        //toolbar.Add(loadButton);
        //toolbar.Add(clearButton);
        //toolbar.Add(resetButton);
        toolbar.Add(miniMapButton);

        toolbar.addStyleSheets("Assets/Giangs Dialogue System/EditorUSS/GDSToolbarStyles.uss");

        rootVisualElement.Add(toolbar);
    }

    private void addStyles() {
        rootVisualElement.addStyleSheets("Assets/Giangs Dialogue System/EditorUSS/GDSVariables.uss");
    }
    
    /*private void save()
    {
        if (string.IsNullOrEmpty(fileNameTextField.value))
        {
            EditorUtility.DisplayDialog("Invalid file name.", "Please ensure the file name you've typed in is valid.", "Roger!");

            return;
        }

        GDSIOUtility.Initialize(graphView, fileNameTextField.value);
        GDSIOUtility.Save();
    }*/
    
    public void enableSaving()
    {
        saveButton.SetEnabled(true);
    }

    public void disableSaving()
    {
        saveButton.SetEnabled(false);
    }

    #endregion
}
