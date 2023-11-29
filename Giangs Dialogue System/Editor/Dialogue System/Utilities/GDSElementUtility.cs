using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public static class GDSElementUtility 
{
   // two UI Utilities to stop repeating a lot of unnecessary code like using the Object Initializer
   // or calling in the EditorGUIUtility Load method

   public static Button createButton(string text, Action onClick = null) {
      var button = new Button(onClick) {
         text = text
      };
      
      return button;
   }
   public static Foldout createFoldout(string title, bool collapsed = false) {
      var foldout = new Foldout {
         text = title,
         value = !collapsed
      };

      return foldout;
   }

   public static Port createPort(this GDSNode node, string portName = "", Orientation orientation = Orientation.Horizontal,
      Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single) {
      
      var port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));
      port.portName = portName;

      return port;
   }
   
   public static TextField createTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null) {
      
      var textField = new TextField {
         value = value,
         label = label
      };

      if (onValueChanged != null) {
         textField.RegisterValueChangedCallback(onValueChanged);
      }

      return textField;
   }

   public static TextField createTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null) {
      
      var textArea = createTextField(value, label, onValueChanged);

      textArea.multiline = true;

      return textArea;
   }
}
